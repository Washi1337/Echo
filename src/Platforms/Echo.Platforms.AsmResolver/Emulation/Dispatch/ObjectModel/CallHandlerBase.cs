using System;
using System.Collections.Generic;
using AsmResolver.DotNet;
using AsmResolver.DotNet.Signatures;
using AsmResolver.DotNet.Signatures.Types;
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
            var method = InstantiateDeclaringType(context.CurrentFrame.Method, (IMethodDescriptor)instruction.Operand!);
            var arguments = GetArguments(context, method);

            try
            {
                return HandleCall(context, instruction, method, arguments);
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
        /// <param name="method">The method that is called.</param>
        /// <param name="arguments">The arguments to call the method with.</param>
        /// <returns>The dispatching result.</returns>
        protected CilDispatchResult HandleCall(
            CilExecutionContext context, 
            CilInstruction instruction, 
            IMethodDescriptor method,
            IList<BitVector> arguments)
        {
            var callerFrame = context.CurrentFrame;

            // Devirtualize the method in the operand.
            var devirtualization = DevirtualizeMethod(context, instruction, method, arguments);
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
            IMethodDescriptor method,
            IList<BitVector> arguments)
        {
            var result = DevirtualizeMethodInternal(context, method, arguments);
            if (!result.IsUnknown)
                return result;
            
            var resolved = context.Machine.UnknownResolver.ResolveMethod(context, instruction, arguments)
                         ?? instruction.Operand as IMethodDescriptor;

            if (resolved is not null)
                result = MethodDevirtualizationResult.Success(resolved);

            return result;
        }

        /// <summary>
        /// Devirtualizes and resolves the method that is referenced by the provided instruction.
        /// </summary>
        /// <param name="context">The execution context the instruction is evaluated in.</param>
        /// <param name="method">The method that is being devirtualized.</param>
        /// <param name="arguments">The arguments pushed onto the stack.</param>
        /// <returns>The result of the devirtualization.</returns>
        protected abstract MethodDevirtualizationResult DevirtualizeMethodInternal(
            CilExecutionContext context,
            IMethodDescriptor method,
            IList<BitVector> arguments
        );

        private static IMethodDescriptor InstantiateDeclaringType(IMethodDescriptor caller, IMethodDescriptor callee)
        {
            // The caller may pass along generic type arguments to the callee.
            var context = GenericContext.FromMethod(caller); 
            if (callee.DeclaringType?.ToTypeDefOrRef() is TypeSpecification { Signature: {} typeSignature })
            {
                var newType = typeSignature.InstantiateGenericTypes(context);
                if (newType != typeSignature)
                    return newType.ToTypeDefOrRef().CreateMemberReference(callee.Name!, callee.Signature!);
            }

            return callee;
        }
        
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
        
        protected static MethodDefinition? FindMethodImplementationInType(TypeDefinition? type, MethodDefinition? baseMethod)
        {
            if (type is null || baseMethod is null || !baseMethod.IsVirtual)
                return baseMethod;

            var implementation = default(MethodDefinition);
            var declaringType = baseMethod.DeclaringType!;

            // If this is a static method, it means we must be implementing a 'static abstract' interface method. 
            if (baseMethod.IsStatic && !declaringType.IsInterface)
                return baseMethod;
            
            while (type is not null && !SignatureComparer.Default.Equals(type, declaringType))
            {
                if (baseMethod.IsStatic)
                {
                    // Static base methods can only be implemented through explicit interface implementation.
                    implementation = TryFindExplicitInterfaceImplementationInType(type, baseMethod);
                }
                else
                {
                    // Prioritize interface implementations.
                    if (declaringType.IsInterface)
                    {
                        implementation = TryFindExplicitInterfaceImplementationInType(type, baseMethod)
                            ?? TryFindImplicitInterfaceImplementationInType(type, baseMethod);
                    }

                    // Try to find other implicit implementations.
                    implementation ??= TryFindImplicitImplementationInType(type, baseMethod);
                }

                if (implementation is not null)
                    break;
                
                // Move up type hierarchy tree.
                type = type.BaseType?.Resolve();
            }

            // If there's no override, just use the base implementation (if available).
            if (implementation is null && !baseMethod.IsAbstract)
                implementation = baseMethod;
            
            return implementation;
        }

        private static MethodDefinition? TryFindImplicitImplementationInType(TypeDefinition type, MethodDefinition baseMethod)
        {
            foreach (var method in type.Methods)
            {
                if (method is { IsVirtual: true, IsReuseSlot: true }
                    && method.Name == baseMethod.Name
                    && SignatureComparer.Default.Equals(method.Signature, baseMethod.Signature))
                {
                    return method;
                }
            }

            return null;
        }

        private static MethodDefinition? TryFindImplicitInterfaceImplementationInType(TypeDefinition type, MethodDefinition baseMethod)
        {
            // Find the correct interface implementation and instantiate any generics.
            MethodSignature? baseMethodSig = null;
            foreach (var interfaceImpl in type.Interfaces)
            {
                if (SignatureComparer.Default.Equals(interfaceImpl.Interface?.ToTypeSignature().GetUnderlyingTypeDefOrRef(), baseMethod.DeclaringType))
                {
                    baseMethodSig = baseMethod.Signature?.InstantiateGenericTypes(GenericContext.FromType(interfaceImpl.Interface!));
                    break;
                }
            }
            if (baseMethodSig is null)
                return null;

            // Find implemented method in type.
            for (int i = 0; i < type.Methods.Count; i++)
            {
                var method = type.Methods[i];
                // Only public virtual instance methods can implicity implement interface methods. (ECMA-335, 6th edition, II.12.2)
                if (method is { IsPublic: true, IsVirtual: true, IsStatic: false }
                    && method.Name == baseMethod.Name
                    && SignatureComparer.Default.Equals(method.Signature, baseMethodSig))
                {
                    return method;
                }
            }

            return null;
        }

        private static MethodDefinition? TryFindExplicitInterfaceImplementationInType(TypeDefinition type, MethodDefinition baseMethod)
        {
            for (int i = 0; i < type.MethodImplementations.Count; i++)
            {
                var impl = type.MethodImplementations[i];
                if (impl.Declaration is null || impl.Declaration.Name != baseMethod.Name)
                    continue;

                // Compare underlying TypeDefOrRef and instantiate any generics to ensure correct comparison.
                var declaringType = impl.Declaration?.DeclaringType?.ToTypeSignature().GetUnderlyingTypeDefOrRef();
                if (!SignatureComparer.Default.Equals(declaringType, baseMethod.DeclaringType))
                    continue;

                var context = GenericContext.FromMethod(impl.Declaration!);
                var implMethodSig = impl.Declaration!.Signature?.InstantiateGenericTypes(context); 
                var baseMethodSig = baseMethod.Signature?.InstantiateGenericTypes(context);
                if (SignatureComparer.Default.Equals(baseMethodSig, implMethodSig))
                    return impl.Body?.Resolve();
            }

            return null;
        }
    }
}