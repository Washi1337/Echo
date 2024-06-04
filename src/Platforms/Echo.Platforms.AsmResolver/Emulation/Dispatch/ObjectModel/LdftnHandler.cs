using System;
using AsmResolver.DotNet;
using AsmResolver.PE.DotNet.Cil;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch.ObjectModel;

/// <summary>
/// Implements a CIL instruction handler for <c>ldftn</c> instruction.
/// </summary>
[DispatcherTableEntry(CilCode.Ldftn)]
public class LdftnHandler : FallThroughOpCodeHandler
{
    /// <inheritdoc/>
    protected override CilDispatchResult DispatchInternal(CilExecutionContext context, CilInstruction instruction)
    {
        var stack = context.CurrentFrame.EvaluationStack;
        var factory = context.Machine.ValueFactory;
        var methods = context.Machine.ValueFactory.ClrMockMemory.MethodEntryPoints;
        var type = context.Machine.ContextModule.CorLibTypeFactory.IntPtr;

        var functionPointer = methods.GetAddress((IMethodDescriptor)instruction.Operand!);
        stack.Push(factory.CreateNativeInteger(functionPointer), type);

        return CilDispatchResult.Success();
    }
}
