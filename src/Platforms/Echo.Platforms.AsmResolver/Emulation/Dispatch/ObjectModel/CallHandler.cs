using System.Collections.Generic;
using AsmResolver.DotNet;
using AsmResolver.PE.DotNet.Cil;
using Echo.Memory;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch.ObjectModel
{
    /// <summary>
    /// Implements a CIL instruction handler for <c>call</c> operations.
    /// </summary>
    [DispatcherTableEntry(CilCode.Call)]
    public class CallHandler : CallHandlerBase
    {
        /// <inheritdoc />
        protected override MethodDevirtualizationResult DevirtualizeMethodInternal(
            CilExecutionContext context,
            IMethodDescriptor method,
            IList<BitVector> arguments)
        {
            // If we are constraining on a specific declaring type, we need to simulate a "virtual dispatch" on that type.
            if (context.CurrentFrame.ConstrainedType?.TryResolve(context.RuntimeContext, out var constrainedType) is true)
            {
                if (method.TryResolve(context.RuntimeContext, out var resolvedBaseMethod) &&  resolvedBaseMethod.IsVirtual)
                {
                    var implementationMethod = FindMethodImplementationInType(context.RuntimeContext, constrainedType, resolvedBaseMethod);
                    if (implementationMethod is not null)
                        return MethodDevirtualizationResult.Success(implementationMethod);
                }
            }

            return MethodDevirtualizationResult.Success(method);
        }
    }
}