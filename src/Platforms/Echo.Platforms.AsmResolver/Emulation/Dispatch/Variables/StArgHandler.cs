using AsmResolver.DotNet.Code.Cil;
using AsmResolver.PE.DotNet.Cil;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch.Variables
{
    /// <summary>
    /// Implements a CIL instruction handler for <c>starg</c> operations and its derivatives.
    /// </summary>
    [DispatcherTableEntry(CilCode.Starg, CilCode.Starg_S)]
    public class StArgHandler : FallThroughOpCodeHandler
    {
        /// <inheritdoc />
        protected override CilDispatchResult DispatchInternal(CilExecutionContext context, CilInstruction instruction)
        {
            var frame = context.CurrentFrame;
            var factory = context.Machine.ValueFactory;
            
            // Extract parameter in opcode or operand.
            var parameter = instruction.GetParameter(frame.Body!.Owner.Parameters);

            // Pop top of stack into the parameter.
            var value = frame.EvaluationStack.Pop();
            var marshalled = factory.Marshaller.FromCliValue(value, parameter.ParameterType);
            frame.WriteArgument(parameter.Index, marshalled.AsSpan());
            
            // Return rented bit vectors.
            factory.BitVectorPool.Return(marshalled);
            factory.BitVectorPool.Return(value.Contents);
            
            return CilDispatchResult.Success();
        }
    }
}