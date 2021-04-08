using System;
using System.Collections.Generic;
using AsmResolver.PE.DotNet.Cil;
using Echo.Concrete.Emulation;
using Echo.Platforms.AsmResolver.Emulation.Values.Cli;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch.Operators
{
    /// <summary>
    /// Provides a handler for instructions with any variant of the Conv operation codes.
    /// </summary>
    public class Conv : FallThroughOpCodeHandler
    {
        /// <inheritdoc />
        public override IReadOnlyCollection<CilCode> SupportedOpCodes => new[]
        {
            CilCode.Conv_I1,
            CilCode.Conv_Ovf_I1,
            CilCode.Conv_Ovf_I1_Un,
            CilCode.Conv_U1,
            CilCode.Conv_Ovf_U1,
            CilCode.Conv_Ovf_U1_Un,
            CilCode.Conv_I2,
            CilCode.Conv_Ovf_I2,
            CilCode.Conv_Ovf_I2_Un,
            CilCode.Conv_U2,
            CilCode.Conv_Ovf_U2,
            CilCode.Conv_Ovf_U2_Un,
            CilCode.Conv_I4,
            CilCode.Conv_Ovf_I4,
            CilCode.Conv_Ovf_I4_Un,
            CilCode.Conv_U4,
            CilCode.Conv_Ovf_U4,
            CilCode.Conv_Ovf_U4_Un,
            CilCode.Conv_I8,
            CilCode.Conv_Ovf_I8,
            CilCode.Conv_Ovf_I8_Un,
            CilCode.Conv_U8,
            CilCode.Conv_Ovf_U8,
            CilCode.Conv_Ovf_U8_Un,
            CilCode.Conv_I,
            CilCode.Conv_Ovf_I,
            CilCode.Conv_Ovf_I_Un,
            CilCode.Conv_U,
            CilCode.Conv_Ovf_U,
            CilCode.Conv_Ovf_U_Un,
            CilCode.Conv_R4,
            CilCode.Conv_R8,
            CilCode.Conv_R_Un,
        };

        /// <inheritdoc />
        public override DispatchResult Execute(CilExecutionContext context, CilInstruction instruction)
        {
            var environment = context.GetService<ICilRuntimeEnvironment>();
            var stack = context.ProgramState.Stack;

            var value = (ICliValue) stack.Pop();

            bool overflowed = false;
            ICliValue newValue = instruction.OpCode.Code switch
            {
                CilCode.Conv_I1 => value.ConvertToI1(false, out _),
                CilCode.Conv_Ovf_I1 => value.ConvertToI1(false, out overflowed),
                CilCode.Conv_Ovf_I1_Un => value.ConvertToI1(true, out overflowed),
                CilCode.Conv_U1 => value.ConvertToU1(false, out _),
                CilCode.Conv_Ovf_U1 => value.ConvertToU1(false, out overflowed),
                CilCode.Conv_Ovf_U1_Un => value.ConvertToU1(true, out overflowed),
                CilCode.Conv_I2 => value.ConvertToI2(false, out _),
                CilCode.Conv_Ovf_I2 => value.ConvertToI2(false, out overflowed),
                CilCode.Conv_Ovf_I2_Un => value.ConvertToI2(true, out overflowed),
                CilCode.Conv_U2 => value.ConvertToU2(false, out _),
                CilCode.Conv_Ovf_U2 => value.ConvertToU2(false, out overflowed),
                CilCode.Conv_Ovf_U2_Un => value.ConvertToU2(true, out overflowed),
                CilCode.Conv_I4 => value.ConvertToI4(false, out _),
                CilCode.Conv_Ovf_I4 => value.ConvertToI4(false, out overflowed),
                CilCode.Conv_Ovf_I4_Un => value.ConvertToI4(true, out overflowed),
                CilCode.Conv_U4 => value.ConvertToU4(false, out _),
                CilCode.Conv_Ovf_U4 => value.ConvertToU4(false, out overflowed),
                CilCode.Conv_Ovf_U4_Un => value.ConvertToU4(true, out overflowed),
                CilCode.Conv_I8 => value.ConvertToI8(false, out _),
                CilCode.Conv_Ovf_I8 => value.ConvertToI8(false, out overflowed),
                CilCode.Conv_Ovf_I8_Un => value.ConvertToI8(true, out overflowed),
                CilCode.Conv_U8 => value.ConvertToU8(false, out _),
                CilCode.Conv_Ovf_U8 => value.ConvertToU8(false, out overflowed),
                CilCode.Conv_Ovf_U8_Un => value.ConvertToU8(true, out overflowed),
                CilCode.Conv_I => value.ConvertToI(environment.Is32Bit, false, out _),
                CilCode.Conv_Ovf_I => value.ConvertToI(environment.Is32Bit, false, out overflowed),
                CilCode.Conv_Ovf_I_Un => value.ConvertToI(environment.Is32Bit, true, out overflowed),
                CilCode.Conv_U => value.ConvertToU(environment.Is32Bit, false, out _),
                CilCode.Conv_Ovf_U => value.ConvertToU(environment.Is32Bit, false, out overflowed),
                CilCode.Conv_Ovf_U_Un => value.ConvertToU(environment.Is32Bit, true, out overflowed),
                CilCode.Conv_R4 => value.ConvertToR4(),
                CilCode.Conv_R8 => value.ConvertToR8(),
                CilCode.Conv_R_Un => value.ConvertToR(),
                _ => throw new ArgumentOutOfRangeException()
            };

            if (overflowed)
                return new DispatchResult(new OverflowException());

            context.ProgramState.Stack.Push(newValue);
            return base.Execute(context, instruction);
        }
    }
}