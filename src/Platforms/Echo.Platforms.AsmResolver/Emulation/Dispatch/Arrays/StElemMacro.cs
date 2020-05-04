using System;
using System.Collections.Generic;
using AsmResolver.PE.DotNet.Cil;
using Echo.Concrete.Emulation;
using Echo.Platforms.AsmResolver.Emulation.Values;
using Echo.Platforms.AsmResolver.Emulation.Values.Cli;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch.Arrays
{
    /// <summary>
    /// Provides a base handler for instructions with one of the <see cref="CilOpCodes.Stelem"/> macro operation codes.
    /// </summary>
    public class StElemMacro : StElemBase
    {
        /// <inheritdoc />
        public override IReadOnlyCollection<CilCode> SupportedOpCodes => new[]
        {
            CilCode.Stelem_I,
            CilCode.Stelem_I1,
            CilCode.Stelem_I2,
            CilCode.Stelem_I4,
            CilCode.Stelem_I8,
            CilCode.Stelem_R4,
            CilCode.Stelem_R8,
            CilCode.Stelem_Ref
        };

        /// <inheritdoc />
        protected override void StoreElement(ExecutionContext context, CilInstruction instruction,
            IDotNetArrayValue dotNetArray, int index, ICliValue value)
        {
            var marshaller = context.GetService<ICilRuntimeEnvironment>().CliMarshaller;

            switch (instruction.OpCode.Code)
            {
                case CilCode.Stelem_I:
                    dotNetArray.StoreElementI(index, value.InterpretAsI(marshaller.Is32Bit), marshaller);
                    break;
                
                case CilCode.Stelem_I1:
                    dotNetArray.StoreElementI1(index, value.InterpretAsI1(), marshaller);
                    break;
                
                case CilCode.Stelem_I2:
                    dotNetArray.StoreElementI2(index, value.InterpretAsI2(), marshaller);
                    break;
                
                case CilCode.Stelem_I4:
                    dotNetArray.StoreElementI4(index, value.InterpretAsI4(), marshaller);
                    break;
                
                case CilCode.Stelem_I8:
                    dotNetArray.StoreElementI8(index, value.InterpretAsI8(), marshaller);
                    break;
                
                case CilCode.Stelem_R4:
                    dotNetArray.StoreElementR4(index, value.InterpretAsR4(), marshaller);
                    break;
                
                case CilCode.Stelem_R8:
                    dotNetArray.StoreElementR8(index, value.InterpretAsR8(), marshaller);
                    break;
                
                case CilCode.Stelem_Ref:
                    dotNetArray.StoreElementRef(index, value.InterpretAsRef(marshaller.Is32Bit), marshaller);
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
        }
    }
}