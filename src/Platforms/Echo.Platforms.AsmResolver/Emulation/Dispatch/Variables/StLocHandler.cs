using AsmResolver.DotNet.Code.Cil;
using AsmResolver.PE.DotNet.Cil;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch.Variables
{
    [DispatcherTableEntry(
        CilCode.Stloc, CilCode.Stloc_0, CilCode.Stloc_1, 
        CilCode.Stloc_2, CilCode.Stloc_3, CilCode.Stloc_S)]
    public class StLocHandler : ICilOpCodeHandler
    {
        /// <inheritdoc />
        public CilDispatchResult Dispatch(CilExecutionContext context, CilInstruction instruction)
        {
            var frame = context.CurrentFrame;
            var factory = context.Machine.ValueFactory;
            
            // Extract local variable in opcode or operand.
            var local = instruction.GetLocalVariable(frame.Body!.LocalVariables);

            var value = frame.EvaluationStack.Pop();

            var marshalled = factory.Marshaller.FromCliValue(value, local.VariableType);
            frame.WriteLocal(local.Index, marshalled.AsSpan());
            
            factory.BitVectorPool.Return(marshalled);
            factory.BitVectorPool.Return(value.Contents);
            
            return CilDispatchResult.Success();
        }
    }
}