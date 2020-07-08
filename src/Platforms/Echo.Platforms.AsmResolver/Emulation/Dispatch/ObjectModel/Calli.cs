using System.Collections.Generic;
using AsmResolver.DotNet;
using AsmResolver.DotNet.Signatures;
using AsmResolver.PE.DotNet.Cil;
using Echo.Platforms.AsmResolver.Emulation.Values.Cli;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch.ObjectModel
{
    /// <summary>
    /// Provides a handler for instructions with the <see cref="CilOpCodes.Calli"/> operation code.
    /// </summary>
    public class Calli : CallBase
    {
        /// <inheritdoc />
        public override IReadOnlyCollection<CilCode> SupportedOpCodes => new[]
        {
            CilCode.Calli
        };

        /// <inheritdoc />
        protected override MethodDevirtualizationResult DevirtualizeMethod(CilInstruction instruction,
            IList<ICliValue> arguments)
        {
            var sig = (StandAloneSignature) instruction.Operand;
            return new MethodDevirtualizationResult(sig.Signature as MethodSignature);
        }
        
    }
}