using System;
using System.Collections.Generic;
using AsmResolver.PE.DotNet.Cil;
using Echo.Concrete.Emulation;
using Echo.Platforms.AsmResolver.Emulation.Values;
using Echo.Platforms.AsmResolver.Emulation.Values.Cli;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch.Arrays
{
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
            IDotNetArrayValue dotNetArray, int index, ICliValue valueValue)
        {
            var marshaller = context.GetService<ICilRuntimeEnvironment>().CliMarshaller;

            switch (instruction.OpCode.Code)
            {
                case CilCode.Stelem_I:
                    dotNetArray.StoreElementI(index, valueValue.InterpretAsI(marshaller.Is32Bit), marshaller);
                    break;
                
                case CilCode.Stelem_I1:
                    dotNetArray.StoreElementI1(index, valueValue.InterpretAsI1(), marshaller);
                    break;
                
                case CilCode.Stelem_I2:
                    dotNetArray.StoreElementI2(index, valueValue.InterpretAsI2(), marshaller);
                    break;
                
                case CilCode.Stelem_I4:
                    dotNetArray.StoreElementI4(index, valueValue.InterpretAsI4(), marshaller);
                    break;
                
                case CilCode.Stelem_I8:
                    dotNetArray.StoreElementI8(index, valueValue.InterpretAsI8(), marshaller);
                    break;
                
                case CilCode.Stelem_R4:
                    dotNetArray.StoreElementR4(index, valueValue.InterpretAsR4(), marshaller);
                    break;
                
                case CilCode.Stelem_R8:
                    dotNetArray.StoreElementR8(index, valueValue.InterpretAsR8(), marshaller);
                    break;
                
                case CilCode.Stelem_Ref:
                    dotNetArray.StoreElementRef(index, valueValue.InterpretAsRef(marshaller.Is32Bit), marshaller);
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
        }
    }
}