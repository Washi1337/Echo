using System;
using System.Collections.Generic;
using AsmResolver.DotNet;
using AsmResolver.DotNet.Signatures;
using AsmResolver.PE.DotNet.Cil;
using Echo.Memory;
using Echo.Platforms.AsmResolver.Emulation.Invocation;
using Echo.Platforms.AsmResolver.Emulation.Stack;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch.ObjectModel
{
    /// <summary>
    /// Provides a base implementation for operation codes that call functions or methods.
    /// </summary>
    public abstract class CallHandlerBase : ICilOpCodeHandler
    {
        /// <inheritdoc />
        public virtual CilDispatchResult Dispatch(CilExecutionContext context, CilInstruction instruction)
        {
            var method = (IMethodDescriptor) instruction.Operand!;
            var arguments = GetArguments(context, method);

            try
            {
                return HandleCall(context, instruction, arguments);
            }
            finally
            {
                for (int i = 0; i < arguments.Count; i++)
                    context.Machine.ValueFactory.BitVectorPool.Return(arguments[i]);
            }
        }

        /// <summary>
        /// Determines whether the instance object should be popped from the stack for the provided method.
        /// </summary>
        /// <param name="method">The method to test.</param>
        /// <returns><c>true</c> if the instance object should be popped, <c>false</c> otherwise.</returns>
        protected virtual bool ShouldPopInstanceObject(IMethodDescriptor method)
        {
            return method.Signature!.HasThis && !method.Signature.ExplicitThis;
        }

        /// <summary>
        /// Handles the actual calling mechanism of the instruction.
        /// </summary>
        /// <param name="context">The context to evaluate the instruction in.</param>
        /// <param name="instruction">The instruction to dispatch and evaluate.</param>
        /// <param name="arguments">The arguments to call the method with.</param>
        /// <returns>The dispatching result.</returns>
        protected CilDispatchResult HandleCall(
            CilExecutionContext context, 
            CilInstruction instruction, 
            IList<BitVector> arguments)
        {
            var callerFrame = context.CurrentFrame;

            // Devirtualize the method in the operand.
            var devirtualization = DevirtualizeMethod(context, instruction, arguments);
            if (devirtualization.IsUnknown)
                throw new CilEmulatorException($"Devirtualization of method call {instruction} was inconclusive.");

            // If that resulted in any error, throw it.
            if (!devirtualization.IsSuccess)
                return CilDispatchResult.Exception(devirtualization.ExceptionObject);

            // Invoke it otherwise.
            var result = Invoke(context, devirtualization.ResultingMethod, arguments);

            // Move to the next instruction if call succeeded.
            if (result.IsSuccess)
                callerFrame.ProgramCounter += instruction.Size;

            return result;
        }

        /// <summary>
        /// Pops the required arguments for a method call from the stack.
        /// </summary>
        /// <param name="context">The context to evaluate the method call in.</param>
        /// <param name="method">The method to pop arguments for.</param>
        /// <returns>The list of marshalled arguments.</returns>
        protected IList<BitVector> GetArguments(CilExecutionContext context, IMethodDescriptor method)
        {
            var stack = context.CurrentFrame.EvaluationStack;
            var result = new List<BitVector>(method.Signature!.GetTotalParameterCount());
            var genericContext = GenericContext.FromMethod(method);
            
            // Pop sentinel arguments.
            for (int i = method.Signature.SentinelParameterTypes.Count - 1; i >= 0; i--)
                result.Add(stack.Pop(method.Signature.SentinelParameterTypes[i].InstantiateGenericTypes(genericContext)));

            // Pop normal arguments.
            for (int i = method.Signature.ParameterTypes.Count - 1; i >= 0; i--)
                result.Add(stack.Pop(method.Signature.ParameterTypes[i].InstantiateGenericTypes(genericContext)));

            // Pop instance object.
            if (ShouldPopInstanceObject(method))
                result.Add(GetInstancePointer(context, method));

            // Correct for stack order.
            result.Reverse();

            return result;
        }

        private static BitVector GetInstancePointer(CilExecutionContext context, IMethodDescriptor method)
        {
            var factory = context.Machine.ValueFactory;
            var stack = context.CurrentFrame.EvaluationStack;
            
            var declaringType = method.DeclaringType?.ToTypeSignature() ?? factory.ContextModule.CorLibTypeFactory.Object;
            return stack.Pop(declaringType);
        }

        private MethodDevirtualizationResult DevirtualizeMethod(
            CilExecutionContext context,
            CilInstruction instruction, 
            IList<BitVector> arguments)
        {
            var result = DevirtualizeMethodInternal(context, instruction, arguments);
            if (!result.IsUnknown)
                return result;
            
            var method = context.Machine.UnknownResolver.ResolveMethod(context, instruction, arguments)
                         ?? instruction.Operand as IMethodDescriptor;

            if (method is not null)
                result = MethodDevirtualizationResult.Success(method);

            return result;
        }

        /// <summary>
        /// Devirtualizes and resolves the method that is referenced by the provided instruction.
        /// </summary>
        /// <param name="context">The execution context the instruction is evaluated in.</param>
        /// <param name="instruction">The instruction that is being evaluated.</param>
        /// <param name="arguments">The arguments pushed onto the stack.</param>
        /// <returns>The result of the devirtualization.</returns>
        protected abstract MethodDevirtualizationResult DevirtualizeMethodInternal(
            CilExecutionContext context,
            CilInstruction instruction,
            IList<BitVector> arguments);

        private static CilDispatchResult Invoke(CilExecutionContext context, IMethodDescriptor method, IList<BitVector> arguments)
        {
            var result = context.Machine.Invoker.Invoke(context, method, arguments);

            switch (result.ResultType)
            {
                case InvocationResultType.StepIn:
                    // We are stepping into this method, push new call frame with the arguments that were pushed onto the stack.
                    var frame = new CallFrame(method, context.Machine.ValueFactory);
                    for (int i = 0; i < arguments.Count; i++)
                        frame.WriteArgument(i, arguments[i]);

                    context.Thread.CallStack.Push(frame);

                    // Ensure type initializer is called for declaring type when necessary.
                    // TODO: Handle `beforefieldinit` flag.
                    if (method.DeclaringType is { } declaringType)
                    {
                        return context.Machine.TypeManager
                            .HandleInitialization(context.Thread, declaringType)
                            .ToDispatchResult();
                    }
                    
                    return CilDispatchResult.Success();

                case InvocationResultType.StepOver:
                    // Method was fully handled by the invoker, push result if it produced any.
                    if (result.Value is not null)
                    {
                        var genericContext = GenericContext.FromMethod(method);
                        var returnType = method.Signature!.ReturnType.InstantiateGenericTypes(genericContext);
                        context.CurrentFrame.EvaluationStack.Push(result.Value, returnType, true);
                    }

                    return CilDispatchResult.Success();

                case InvocationResultType.Exception:
                    // There was an exception during the invocation. Throw it.
                    return CilDispatchResult.Exception(result.ExceptionObject);

                case InvocationResultType.Inconclusive:
                    // Method invoker was not able to handle the method call.
                    throw new CilEmulatorException($"Invocation of method {method} was inconclusive.");

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}