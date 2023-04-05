using AsmResolver.DotNet.Code.Cil;
using AsmResolver.DotNet.Signatures.Types;
using AsmResolver.DotNet.Signatures;
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
            var genericContext = GenericContext.FromMethod(context.CurrentFrame.Method);

            // Extract parameter in opcode or operand.
            var parameter = instruction.GetParameter(frame.Body!.Owner.Parameters);

            var parameterType = parameter.ParameterType.InstantiateGenericTypes(genericContext);

            // Pop top of stack into the parameter.
            var value = frame.EvaluationStack.Pop(parameterType);
            frame.WriteArgument(parameter.Index, value.AsSpan());
            
            // Return rented bit vectors.
            factory.BitVectorPool.Return(value);
            
            return CilDispatchResult.Success();
        }
    }
}