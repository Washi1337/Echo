using System.Collections.Generic;
using System.Linq;
using AsmResolver.DotNet;
using AsmResolver.DotNet.Signatures;
using AsmResolver.PE.DotNet.Cil;
using Echo.Memory;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch.ObjectModel
{
    /// <summary>
    /// Implements a CIL instruction handler for <c>callvirt</c> operations.
    /// </summary>
    [DispatcherTableEntry(CilCode.Callvirt)]
    public class CallVirtHandler : CallHandlerBase
    {
        /// <inheritdoc />
        protected override MethodDevirtualizationResult DevirtualizeMethodInternal(
            CilExecutionContext context,
            IMethodDescriptor method,
            IList<BitVector> arguments)
        {
            switch (arguments[0].AsSpan())
            {
                case { IsFullyKnown: false }:
                    // We cannot dereference unknown pointers.
                    return MethodDevirtualizationResult.Unknown(); 
                
                case { IsZero.Value: TrileanValue.True }:
                    // We are trying to dereference a null pointer.
                    return MethodDevirtualizationResult.Exception(
                        context.Machine.Heap.AllocateObject(
                                context.Machine.ValueFactory.NullReferenceExceptionType,
                                true)
                            .AsObjectHandle(context.Machine)
                    );
                
                case var objectPointer:
                    // Determine the type to apply virtual dispatch on.
                    var objectType = context.CurrentFrame.ConstrainedType
                        ?? objectPointer.AsObjectHandle(context.Machine).GetObjectType();
                    
                    // Find the implementation.
                    var implementation = FindMethodImplementationInType(objectType.Resolve(), method.Resolve());
                    if (implementation is null)
                    {
                        // There is no implementation for the method.
                        return MethodDevirtualizationResult.Exception(
                            context.Machine.Heap.AllocateObject(
                                    context.Machine.ValueFactory.MissingMethodExceptionType,
                                    true)
                                .AsObjectHandle(context.Machine)
                        );
                    }
                    
                    // Instantiate any generics.
                    var genericContext = GenericContext.FromMethod(method);
                    if (genericContext.IsEmpty)
                        return MethodDevirtualizationResult.Success(implementation);

                    var instantiated = objectType
                        .ToTypeDefOrRef()
                        .CreateMemberReference(implementation.Name!, implementation.Signature!);

                    return MethodDevirtualizationResult.Success(genericContext.Method != null
                        ? instantiated.MakeGenericInstanceMethod(genericContext.Method.TypeArguments.ToArray())
                        : instantiated
                    );
            }
        }

      
    }
}