using AsmResolver.DotNet.Signatures.Types;
using AsmResolver.DotNet.Signatures;
using AsmResolver.PE.DotNet.Cil;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch.ControlFlow
{
    /// <summary>
    /// Implements a CIL instruction handler for <c>ret</c> operations.
    /// </summary>
    [DispatcherTableEntry(CilCode.Ret)]
    public class RetHandler : ICilOpCodeHandler
    {
        /// <inheritdoc />
        public CilDispatchResult Dispatch(CilExecutionContext context, CilInstruction instruction)
        {
            var frame = context.Machine.CallStack.Pop();

            if (frame.Method.Signature!.ReturnsValue)
            {
                var returnType = frame.Method.Signature.ReturnType;

                if (returnType is GenericParameterSignature parameterSignature) {
                    var genericContext = GenericContext.FromMethod(frame.Method);
                    returnType = genericContext.Method!.TypeArguments[parameterSignature.Index];
                }

                var value = frame.EvaluationStack.Pop(returnType);
                context.CurrentFrame.EvaluationStack.Push(value, returnType, true);
            }

            return CilDispatchResult.Success();
        }
    }
}