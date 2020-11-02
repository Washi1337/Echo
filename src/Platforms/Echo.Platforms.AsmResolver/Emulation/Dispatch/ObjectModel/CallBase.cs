using System.Collections.Generic;
using System.Linq;
using AsmResolver.DotNet;
using AsmResolver.DotNet.Signatures;
using AsmResolver.PE.DotNet.Cil;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;
using Echo.Concrete.Emulation;
using Echo.Concrete.Emulation.Dispatch;
using Echo.Concrete.Values;
using Echo.Platforms.AsmResolver.Emulation.Values.Cli;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch.ObjectModel
{
    /// <summary>
    /// Provides a base for operation code handlers that call procedures outside of the method body. 
    /// </summary>
    public abstract class CallBase : FallThroughOpCodeHandler
    {
        /// <inheritdoc />
        public override DispatchResult Execute(ExecutionContext context, CilInstruction instruction)
        {
            var environment = context.GetService<ICilRuntimeEnvironment>();

            // Pop arguments.
            int argumentCount = environment.Architecture.GetStackPopCount(instruction);
            var arguments = context.ProgramState.Stack
                .Pop(argumentCount, true)
                .Cast<ICliValue>()
                .ToList();

            // Dispatch
            var methodDispatch = DevirtualizeMethod(instruction, arguments);
            if (methodDispatch.Exception != null)
                return new DispatchResult(methodDispatch.Exception);

            // Invoke.
            var result = InvokeAndGetResult(environment, instruction, methodDispatch, arguments);

            // Push result if necessary.
            UpdateStack(context, result, instruction);

            return base.Execute(context, instruction);
        }

        private static IConcreteValue InvokeAndGetResult(ICilRuntimeEnvironment environment, CilInstruction instruction,
            in MethodDevirtualizationResult methodDispatch, IReadOnlyList<ICliValue> arguments)
        {
            // If method dispatch's result was unknown, assume a non-null object instance and return an unknown value. 
            if (methodDispatch.IsUnknown)
                return CreateUnknownResult(environment, instruction);

            // Marshal stack values to normal values.
            var marshalledArguments = MarshalMethodArguments(environment, arguments, methodDispatch.GetMethodSignature());

            // Invoke.
            if (methodDispatch.ResultingMethod != null)
                return environment.MethodInvoker.Invoke(methodDispatch.ResultingMethod, marshalledArguments);

            if (methodDispatch.ResultingMethodSignature != null)
            {
                var address = marshalledArguments.Last();
                marshalledArguments.Remove(address);
                return environment.MethodInvoker.InvokeIndirect(address, methodDispatch.ResultingMethodSignature,
                    marshalledArguments);
            }

            return null;
        }

        internal static List<IConcreteValue> MarshalMethodArguments(
            ICilRuntimeEnvironment environment,
            IReadOnlyList<ICliValue> arguments,
            MethodSignature signature)
        {
            var marshaller = environment.CliMarshaller;
            var marshalledArguments = new List<IConcreteValue>(arguments.Count);

            // Include instance object when necessary. 
            int index = 0;
            if (signature.HasThis || signature.ExplicitThis)
            {
                // Instance method calls always are object references. This is why we can always just marshal
                // to a normal object reference.
                var objectInstance = marshaller.ToCtsValue(arguments[0], environment.Module.CorLibTypeFactory.Object);
                marshalledArguments.Add(objectInstance);
            }

            // Marshal remaining arguments.
            for (int i = 0; i < signature.ParameterTypes.Count; i++)
            {
                var marshalledArgument = marshaller.ToCtsValue(arguments[index++], signature.ParameterTypes[i]);
                marshalledArguments.Add(marshalledArgument);
            }

            // Add any remaining (excess) arguments. 
            // This is e.g. used by calli opcodes that also push the address of the function to be called.
            for (int i = marshalledArguments.Count; i < arguments.Count; i++)
                marshalledArguments.Add(arguments[i]);
            
            return marshalledArguments;
        }

        private static IConcreteValue CreateUnknownResult(ICilRuntimeEnvironment environment, CilInstruction instruction)
        {
            var signature = instruction.Operand switch
            {
                IMethodDescriptor method => method.Signature,
                StandAloneSignature standAlone => (MethodSignature) standAlone.Signature,
                _ => throw new DispatchException("Operand of call instruction is not a method or a signature.")
            };

            return environment.CliMarshaller.ToCliValue(
                environment.UnknownValueFactory.CreateUnknown(signature.ReturnType),
                signature.ReturnType);
        }

        private static void UpdateStack(ExecutionContext context, IConcreteValue result, CilInstruction instruction)
        {
            var environment = context.GetService<ICilRuntimeEnvironment>();

            var returnType = instruction.Operand switch
            {
                IMethodDescriptor method => method.Signature.ReturnType,
                StandAloneSignature signature => ((MethodSignatureBase) signature.Signature).ReturnType,
                _ => throw new DispatchException("Operand of call instruction is not a method or a signature.")
            };
            
            bool pushesValue = returnType.ElementType != ElementType.Void;
            
            if (result is null)
            {
                if (pushesValue)
                {
                    throw new DispatchException(
                        "Method was expected to return a value, but the method invoker returned a null value.");
                }
            }
            else
            {
                if (!pushesValue)
                {
                    throw new DispatchException(
                        "Method was not expected to return a value, but the method invoker returned a non-null value.");
                }

                var marshalledReturnValue = environment.CliMarshaller.ToCliValue(result, returnType);
                context.ProgramState.Stack.Push(marshalledReturnValue);
            }
        }

        /// <summary>
        /// Devirtualizes the method referenced by the provided instruction, and infers the actual method
        /// implementing the referenced method that was called.
        /// </summary>
        /// <param name="instruction">The call instruction.</param>
        /// <param name="arguments">The arguments of the method call.</param>
        /// <returns>The result of the devirtualization process.</returns>
        protected abstract MethodDevirtualizationResult DevirtualizeMethod(
            CilInstruction instruction,
            IList<ICliValue> arguments);
    }
}