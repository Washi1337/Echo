using System.Collections.Generic;
using AsmResolver.DotNet;
using AsmResolver.PE.DotNet.Cil;
using Echo.Concrete.Emulation;
using Echo.Platforms.AsmResolver.Emulation.Values;
using Echo.Platforms.AsmResolver.Emulation.Values.Cli;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch.Arrays
{
    /// <summary>
    /// Provides a handler for instructions with the <see cref="CilOpCodes.Stelem"/> operation code.
    /// </summary>
    public class StElem : StElemBase
    {
        /// <inheritdoc />
        public override IReadOnlyCollection<CilCode> SupportedOpCodes => new[]
        {
            CilCode.Stelem
        };

        /// <inheritdoc />
        protected override void StoreElement(
            CilExecutionContext context,
            CilInstruction instruction, 
            IDotNetArrayValue array,
            int index,
            ICliValue value)
        {
            var environment = context.GetService<ICilRuntimeEnvironment>();

            var type = (ITypeDescriptor) instruction.Operand;
            var typeLayout = environment.ValueFactory.GetTypeMemoryLayout(type);

            array.StoreElement(index, typeLayout, value, environment.CliMarshaller);
        }
    }
}