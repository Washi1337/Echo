using AsmResolver.DotNet.Code.Cil;
using AsmResolver.PE.DotNet.Cil;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch.Variables
{
    [DispatcherTableEntry(
        CilCode.Ldloc, CilCode.Ldloc_0, CilCode.Ldloc_1, 
        CilCode.Ldloc_2, CilCode.Ldloc_3, CilCode.Ldloc_S)]
    public class LdLocHandler : ICilOpCodeHandler
    {
        /// <inheritdoc />
        public CilDispatchResult Dispatch(CilExecutionContext context, CilInstruction instruction)
        {
            var frame = context.CurrentFrame;
            var factory = context.Machine.ValueFactory;
         
            // Extract local variable in opcode or operand.
            var local = instruction.GetLocalVariable(frame.Body!.LocalVariables);

            // Read local from stack frame.
            var layout = factory.GetTypeValueMemoryLayout(local.VariableType);
            var result = factory.BitVectorPool.Rent((int) (layout.Size * 8), false);
            frame.ReadLocal(local.Index, result.AsSpan());
             
            // Marshal and push.
            context.CurrentFrame.EvaluationStack.Push(factory.Marshaller.ToCliValue(result, local.VariableType));
            
            return CilDispatchResult.Success();
        }
    }
}