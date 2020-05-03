using System.Collections.Generic;
using AsmResolver.PE.DotNet.Cil;
using Echo.Concrete.Emulation;
using Echo.Concrete.Values;
using Echo.Platforms.AsmResolver.Emulation.Values;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch.Arrays
{
    /// <summary>
    /// Provides a handler for instructions that obtain an integral value from an array.
    /// </summary>
    public class LdElemMacro : LdElemBase
    {
        /// <inheritdoc />
        public override IReadOnlyCollection<CilCode> SupportedOpCodes => new[]
        {
            CilCode.Ldelem_I,
            CilCode.Ldelem_I1,
            CilCode.Ldelem_I2,
            CilCode.Ldelem_I4,
            CilCode.Ldelem_I8,
            CilCode.Ldelem_U1,
            CilCode.Ldelem_U2,
            CilCode.Ldelem_U4,
            CilCode.Ldelem_R4,
            CilCode.Ldelem_R8,
            CilCode.Ldelem_Ref
        };

        /// <inheritdoc />
        protected override IConcreteValue GetValue(ExecutionContext context, CilInstruction instruction, IDotNetArrayValue array,
            int index)
        {
            var marshaller = context.GetService<ICliMarshaller>();

            return instruction.OpCode.Code switch
            {
                CilCode.Ldelem_I => array.LoadElementI(index, marshaller),
                CilCode.Ldelem_I1 => array.LoadElementI1(index, marshaller),
                CilCode.Ldelem_I2 => array.LoadElementI2(index, marshaller),
                CilCode.Ldelem_I4 => array.LoadElementI4(index, marshaller),
                CilCode.Ldelem_I8 => array.LoadElementI8(index, marshaller),
                CilCode.Ldelem_U1 => array.LoadElementU1(index, marshaller),
                CilCode.Ldelem_U2 => array.LoadElementU2(index, marshaller),
                CilCode.Ldelem_U4 => array.LoadElementU4(index, marshaller),
                CilCode.Ldelem_R4 => array.LoadElementR4(index, marshaller),
                CilCode.Ldelem_R8 => array.LoadElementR8(index, marshaller),
                CilCode.Ldelem_Ref => array.LoadElementRef(index, marshaller),
            };
        }
        
    }
}