using System;
using System.Collections.Generic;
using AsmResolver.DotNet;
using AsmResolver.DotNet.Signatures;
using AsmResolver.PE.DotNet.Cil;
using Echo.Platforms.AsmResolver.Emulation.Values;
using Echo.Platforms.AsmResolver.Emulation.Values.Cli;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch.ObjectModel
{
    /// <summary>
    /// Provides a handler for instructions with the <see cref="CilOpCodes.Newobj"/> operation code.
    /// </summary>
    public class NewObj : CallBase
    {
        /// <inheritdoc />
        public override IReadOnlyCollection<CilCode> SupportedOpCodes => new[]
        {
            CilCode.Newobj
        };

        /// <inheritdoc />
        protected override MethodDevirtualizationResult DevirtualizeMethod(CilInstruction instruction, IList<ICliValue> arguments)
        {
            return new MethodDevirtualizationResult((IMethodDescriptor) instruction.Operand);
        }
    }
}