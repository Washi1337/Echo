using System;
using System.Collections.Generic;
using AsmResolver.PE.DotNet.Cil;
using Echo.Concrete.Emulation;
using Echo.Concrete.Emulation.Dispatch;
using Echo.Concrete.Values.ValueType;
using Echo.Platforms.AsmResolver.Emulation.Values.Cli;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch.Miscellaneous
{
    /// <summary>
    /// Provides a handler for instructions with the CKFINITE operation code.
    /// </summary>
    public class CkFinite : FallThroughOpCodeHandler
    {
        /// <inheritdoc />
        public override IReadOnlyCollection<CilCode> SupportedOpCodes => new[]
        {
            CilCode.Ckfinite
        };

        /// <inheritdoc />
        public override DispatchResult Execute(ExecutionContext context, CilInstruction instruction)
        {
            var val = context.ProgramState.Stack.Pop();
            if (val is FValue value)
            {
                if (double.IsNaN(value.F64) || double.IsInfinity(value.F64))
                    return new DispatchResult(new ArithmeticException());
            }

            return base.Execute(context, instruction);
        }
    }
}