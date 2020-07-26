using System.Collections.Generic;
using AsmResolver.PE.DotNet.Cil;
using Echo.Concrete.Emulation;
using Echo.Concrete.Emulation.Dispatch;
using Echo.Concrete.Values.ReferenceType;
using Echo.Concrete.Values.ValueType;
using Echo.Platforms.AsmResolver.Emulation.Values.Cli;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch.Pointers
{
    /// <summary>
    /// Provides a handler for instructions with one of the STIND operation codes.
    /// </summary>
    public class StInd : FallThroughOpCodeHandler
    {
        /// <inheritdoc />
        public override IReadOnlyCollection<CilCode> SupportedOpCodes => new[]
        {
            CilCode.Stind_I,
            CilCode.Stind_I1,
            CilCode.Stind_I2,
            CilCode.Stind_I4,
            CilCode.Stind_I8,
            CilCode.Stind_R4,
            CilCode.Stind_R8,
        };

        /// <inheritdoc />
        public override DispatchResult Execute(ExecutionContext context, CilInstruction instruction)
        {
            var environment = context.GetService<ICilRuntimeEnvironment>();
            var factory = environment.Module.CorLibTypeFactory;

            var stack = context.ProgramState.Stack;
            var valueValue = (ICliValue) stack.Pop();
            var pointerValue = (IPointerValue) stack.Pop();

            var marshaller = environment.CliMarshaller;
            
            switch (instruction.OpCode.Code)
            {
               case CilCode.Stind_I:
                   if (pointerValue.Is32Bit)
                       pointerValue.WriteInteger32(0, valueValue.InterpretAsI4());
                   else
                       pointerValue.WriteInteger64(0, valueValue.InterpretAsI8());
                   break;
               
               case CilCode.Stind_I1:
                   pointerValue.WriteInteger8(0,
                       (Integer8Value) marshaller.ToCtsValue(valueValue.InterpretAsI1(), factory.SByte));
                   break;
               
               case CilCode.Stind_I2:
                   pointerValue.WriteInteger16(0,
                       (Integer16Value) marshaller.ToCtsValue(valueValue.InterpretAsI2(), factory.Int16));
                   break;
               
               case  CilCode.Stind_I4:
                   pointerValue.WriteInteger32(0,
                       (Integer32Value) marshaller.ToCtsValue(valueValue.InterpretAsI4(), factory.Int32));
                   break;
               
               case CilCode.Stind_I8:
                   pointerValue.WriteInteger64(0,
                       (Integer64Value) marshaller.ToCtsValue(valueValue.InterpretAsI8(), factory.Int64));
                   break;
               
               case CilCode.Stind_R4:
                   pointerValue.WriteFloat32(0,
                       (Float32Value) marshaller.ToCtsValue(valueValue.InterpretAsR4(), factory.Single));
                   break;
               
               case CilCode.Stind_R8:
                   pointerValue.WriteFloat64(0,
                       (Float64Value) marshaller.ToCtsValue(valueValue.InterpretAsR8(), factory.Double));
                   break;
               
               default:
                   return DispatchResult.InvalidProgram();
            }
            
            return base.Execute(context, instruction);
        }
    }
}