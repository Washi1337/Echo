using AsmResolver.DotNet.Code.Cil;
using AsmResolver.PE.DotNet.Cil;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch.Variables
{
    [DispatcherTableEntry(CilCode.Starg, CilCode.Starg_S)]
    public class StArgHandler : ICilOpCodeHandler
    {
        /// <inheritdoc />
        public CilDispatchResult Dispatch(CilExecutionContext context, CilInstruction instruction)
        {
            var frame = context.CurrentFrame;
            var factory = context.Machine.ValueFactory;
            
            // Extract parameter in opcode or operand.
            var parameter = instruction.GetParameter(frame.Body!.Owner.Parameters);

            var value = frame.EvaluationStack.Pop();

            var marshalled = factory.Marshaller.FromCliValue(value, parameter.ParameterType);
            frame.WriteArgument(parameter.Index, marshalled.AsSpan());
            
            factory.BitVectorPool.Return(marshalled);
            factory.BitVectorPool.Return(value.Contents);
            
            return CilDispatchResult.Success();
        }
    }
}