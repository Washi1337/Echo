using AsmResolver.DotNet.Code.Cil;
using AsmResolver.DotNet.Signatures;
using AsmResolver.PE.DotNet.Cil;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch.Variables
{
    /// <summary>
    /// Implements a CIL instruction handler for <c>stloc</c> operations and its derivatives.
    /// </summary>
    [DispatcherTableEntry(
        CilCode.Stloc, CilCode.Stloc_0, CilCode.Stloc_1, 
        CilCode.Stloc_2, CilCode.Stloc_3, CilCode.Stloc_S)]
    public class StLocHandler : FallThroughOpCodeHandler
    {
        /// <inheritdoc />
        protected override CilDispatchResult DispatchInternal(CilExecutionContext context, CilInstruction instruction)
        {
            var frame = context.CurrentFrame;
            var factory = context.Machine.ValueFactory;
            var genericContext = GenericContext.FromMethod(context.CurrentFrame.Method);

            // Extract local variable in opcode or operand.
            var local = instruction.GetLocalVariable(frame.Body!.LocalVariables);

            var localType = local.VariableType.InstantiateGenericTypes(genericContext);

            // Pop top of stack into the variable.
            var value = frame.EvaluationStack.Pop(localType);
            frame.WriteLocal(local.Index, value.AsSpan());
            
            // Return rented bit vectors.
            factory.BitVectorPool.Return(value);
            
            return CilDispatchResult.Success();
        }
    }
}