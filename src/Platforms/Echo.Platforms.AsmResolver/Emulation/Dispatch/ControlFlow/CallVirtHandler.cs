using System.Collections.Generic;
using AsmResolver.DotNet;
using AsmResolver.DotNet.Signatures;
using AsmResolver.PE.DotNet.Cil;
using Echo.Concrete;
using Echo.Core;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch.ControlFlow
{
    [DispatcherTableEntry(CilCode.Callvirt)]
    public class CallVirtHandler : CallHandlerBase
    {
        private static readonly SignatureComparer Comparer = new();
        
        protected override MethodDevirtualizationResult DevirtualizeMethod(CilExecutionContext context, CilInstruction instruction, IList<BitVector> arguments)
        {
            var method = ((IMethodDescriptor) instruction.Operand!).Resolve();

            switch (arguments[0].AsSpan())
            {
                case { IsFullyKnown: false }:
                    return new MethodDevirtualizationResult(); 
                
                case { IsZero: { Value: TrileanValue.True } }:
                    return new MethodDevirtualizationResult(context.Machine.Heap.AllocateObject(context.Machine.ValueFactory.NullReferenceExceptionType, true));
                
                case var objectPointer:
                    var type = objectPointer.GetObjectPointerType(context.Machine);
                    
                    var implementation = FindMethodImplementationInType(type.Resolve(), method);
                    return implementation is null
                        ? new MethodDevirtualizationResult(context.Machine.Heap
                            .AllocateObject(context.Machine.ValueFactory.MissingMethodExceptionType, true))
                        : new MethodDevirtualizationResult(implementation);
            }
        }

        private static MethodDefinition? FindMethodImplementationInType(TypeDefinition? type, MethodDefinition baseMethod)
        {
            if (!baseMethod.IsVirtual)
                return baseMethod;

            var implementation = default(MethodDefinition);
            var declaringType = baseMethod.DeclaringType!;
            while (type is not null && !Comparer.Equals(type, declaringType))
            {
                // Prioritize explicit interface implementations.
                if (declaringType.IsInterface)
                    implementation = TryFindExplicitInterfaceImplementationInType(type, baseMethod);

                // Try find any implicit implementations.
                implementation ??= TryFindImplicitImplementationInType(type, baseMethod);
                
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
                if (method.IsVirtual
                    && method.IsReuseSlot
                    && method.Name == baseMethod.Name
                    && Comparer.Equals(method.Signature, baseMethod.Signature))
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
                if (Comparer.Equals(baseMethod, impl.Declaration))
                    return impl.Body?.Resolve();
            }

            return null;
        }
    }
}