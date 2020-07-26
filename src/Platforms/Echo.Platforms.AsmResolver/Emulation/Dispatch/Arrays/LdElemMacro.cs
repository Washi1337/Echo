using System;
using System.Collections.Generic;
using AsmResolver.PE.DotNet.Cil;
using Echo.Concrete.Emulation;
using Echo.Platforms.AsmResolver.Emulation.Values;
using Echo.Platforms.AsmResolver.Emulation.Values.Cli;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch.Arrays
{
    /// <summary>
    /// Provides a handler for instructions with the <see cref="CilOpCodes.Ldelem"/> operation code.
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
        protected override ICliValue GetElementValue(ExecutionContext context, CilInstruction instruction, IDotNetArrayValue array,
            int index)
        {
            var marshaller = context.GetService<ICilRuntimeEnvironment>().CliMarshaller;

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
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        /// <inheritdoc />
        protected override ICliValue GetUnknownElementValue(ExecutionContext context, CilInstruction instruction)
        {        
            var environment = context.GetService<ICilRuntimeEnvironment>();

            return instruction.OpCode.Code switch
            {
                CilCode.Ldelem_I => new I4Value(0,0),
                CilCode.Ldelem_I1 => new I4Value(0,0),
                CilCode.Ldelem_I2 => new I4Value(0,0),
                CilCode.Ldelem_I4 => new I4Value(0,0),
                CilCode.Ldelem_I8 => new I8Value(0,0),
                CilCode.Ldelem_U1 => new I4Value(0,0),
                CilCode.Ldelem_U2 => new I4Value(0,0),
                CilCode.Ldelem_U4 => new I4Value(0,0),
                CilCode.Ldelem_R4 => new FValue(0),
                CilCode.Ldelem_R8 => new FValue(0),
                CilCode.Ldelem_Ref => new OValue(null, false, environment.Is32Bit),
                _ => throw new ArgumentOutOfRangeException()
            };
        }
        
    }
}