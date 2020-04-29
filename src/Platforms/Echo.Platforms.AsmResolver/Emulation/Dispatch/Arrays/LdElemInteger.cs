using System;
using System.Collections.Generic;
using AsmResolver.PE.DotNet.Cil;
using Echo.Concrete.Emulation;
using Echo.Concrete.Values;
using Echo.Concrete.Values.ReferenceType;
using Echo.Concrete.Values.ValueType;
using Echo.Platforms.AsmResolver.Emulation.Values.Cli;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch.Arrays
{
    /// <summary>
    /// Provides a handler for instructions that obtain an integral value from an array.
    /// </summary>
    public class LdElemInteger : LdElemBase
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
            CilCode.Ldelem_U4
        };

        /// <inheritdoc />
        protected override IConcreteValue GetValue(ExecutionContext context, CilInstruction instruction, ArrayValue array,
            int index)
        {
            bool is32Bit = context.GetService<ICilRuntimeEnvironment>().Is32Bit;
            var code = instruction.OpCode.Code;
            var integerValue = array[index] as IntegerValue;

            if (integerValue is null)
                return GetUnknownInteger(code, is32Bit);

            var result = GetKnownInteger(code, integerValue, is32Bit);
            if (code == CilCode.Ldelem_I)
                result = new NativeIntegerValue(result, is32Bit);
            
            return result;
        }

        private static IntegerValue GetUnknownInteger(CilCode code, bool is32Bit)
        {
            switch (code)
            {
                case CilCode.Ldelem_I:
                    IntegerValue result = new NativeIntegerValue(0, is32Bit);
                    result.MarkFullyUnknown();
                    return result;
                
                case CilCode.Ldelem_U1:
                    return new Integer32Value(0, 0xFFFFFF00U);
                
                case CilCode.Ldelem_U2:
                    return new Integer32Value(0, 0xFFFF0000U);

                case CilCode.Ldelem_I1:
                case CilCode.Ldelem_I2:
                case CilCode.Ldelem_I4:
                case CilCode.Ldelem_U4:
                    return new Integer32Value(0, 0);

                case CilCode.Ldelem_I8:
                    return new Integer64Value(0, 0);

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static IntegerValue GetKnownInteger(CilCode code, IntegerValue integerValue, bool is32Bit)
        {
            (int realSize, int stackSize, bool signed) = code switch
            {
                CilCode.Ldelem_I => (is32Bit ? 4 : 8, is32Bit ? 4 : 8, true),
                CilCode.Ldelem_I1 => (1, 4, true),
                CilCode.Ldelem_I2 => (2, 4, true),
                CilCode.Ldelem_I4 => (4, 4, true),
                CilCode.Ldelem_I8 => (8, 8, true),
                CilCode.Ldelem_U1 => (1, 4, false),
                CilCode.Ldelem_U2 => (2, 4, false),
                CilCode.Ldelem_U4 => (4, 4, false),
                _ => throw new ArgumentOutOfRangeException()
            };

            if (integerValue.Size < realSize)
                return integerValue.Extend(stackSize * 8, false);

            if (integerValue.Size > realSize)
            {
                var result = (IntegerValue) integerValue.Copy();
                result = result.Truncate(realSize * 8);
                return result.Extend(stackSize * 8, signed);
            }

            return (IntegerValue) integerValue.Copy();
        }
    }
}