using AsmResolver.DotNet;
using AsmResolver.DotNet.Signatures;
using AsmResolver.PE.DotNet.Cil;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch.ObjectModel;

/// <summary>
/// Implements a CIL instruction handler for <c>constrained</c> operations.
/// </summary>
[DispatcherTableEntry(CilCode.Constrained)]
public class ConstrainedHandler : FallThroughOpCodeHandler
{
    /// <inheritdoc />
    protected override CilDispatchResult DispatchInternal(CilExecutionContext context, CilInstruction instruction)
    {
        var genericContext = GenericContext.FromMethod(context.CurrentFrame.Method);
        context.CurrentFrame.ConstrainedType = ((ITypeDescriptor)instruction.Operand!)
            .ToTypeSignature()
            .InstantiateGenericTypes(genericContext);

        return CilDispatchResult.Success();
    }
}