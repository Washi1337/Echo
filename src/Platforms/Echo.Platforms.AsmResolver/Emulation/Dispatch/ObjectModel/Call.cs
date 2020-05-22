using System.Collections.Generic;
using AsmResolver.DotNet;
using AsmResolver.PE.DotNet.Cil;
using Echo.Platforms.AsmResolver.Emulation.Values.Cli;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch.ObjectModel
{
    /// <summary>
    /// Provides a handler for instructions with the <see cref="CilOpCodes.Call"/> operation code.
    /// </summary>
    public class Call : CallBase
    {
        /// <inheritdoc />
        public override IReadOnlyCollection<CilCode> SupportedOpCodes => new[]
        {
            CilCode.Call
        };

        /// <inheritdoc />
        protected override MethodDevirtualizationResult DevirtualizeMethod(CilInstruction instruction,
            IList<ICliValue> arguments)
        {
            // Call opcodes have no special devirtualization.
            return new MethodDevirtualizationResult((IMethodDescriptor) instruction.Operand);
        }
        
    }
}