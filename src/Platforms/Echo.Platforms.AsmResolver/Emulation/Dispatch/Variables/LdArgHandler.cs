using AsmResolver.DotNet.Code.Cil;
using AsmResolver.PE.DotNet.Cil;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch.Variables
{
    /// <summary>
    /// Implements a CIL instruction handler for <c>ldarg</c> operations and its derivatives.
    /// </summary>
    [DispatcherTableEntry(
        CilCode.Ldarg, CilCode.Ldarg_0, CilCode.Ldarg_1, 
        CilCode.Ldarg_2, CilCode.Ldarg_3, CilCode.Ldarg_S)]
    public class LdArgHandler : FallThroughOpCodeHandler
    {
        /// <inheritdoc />
        protected override CilDispatchResult DispatchInternal(CilExecutionContext context, CilInstruction instruction)
        {
            var frame = context.CurrentFrame;
            var factory = context.Machine.ValueFactory;

            // Extract parameter in opcode or operand.
            var parameter = instruction.GetParameter(frame.Body!.Owner.Parameters);
            
            var result = factory.RentValue(parameter.ParameterType, false);
            try
            {
                // Marshal and push.
                frame.ReadArgument(parameter.Index, result.AsSpan());
                context.CurrentFrame.EvaluationStack.Push(result, parameter.ParameterType);
            }
            finally
            {
                factory.BitVectorPool.Return(result);
            }

            return CilDispatchResult.Success();
        }
    }
}