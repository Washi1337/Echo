using System;
using System.Collections.Generic;
using AsmResolver.PE.DotNet.Cil;
using Echo.Concrete.Emulation;
using Echo.Concrete.Values;
using Echo.Concrete.Values.ReferenceType;
using Echo.Concrete.Values.ValueType;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch.Arrays
{
    /// <summary>
    /// Provides a handler for instructions that obtain a floating point value from an array.
    /// </summary>
    public class LdElemFloat : LdElemBase
    {
        /// <inheritdoc />
        public override IReadOnlyCollection<CilCode> SupportedOpCodes => new[]
        {
            CilCode.Ldelem_R4, CilCode.Ldelem_R8
        };

        /// <inheritdoc />
        protected override IConcreteValue GetValue(ExecutionContext context, CilInstruction instruction, ArrayValue array,
            int index)
        {
            switch (array[index])
            {
                case Float32Value float32Value:
                    return new Float64Value(float32Value.F32);
                
                case Float64Value float64Value:
                    return (Float64Value) float64Value.Copy();
                
                default:
                    // TODO: Apply undocumented behaviour when if integer was stored in the array, these bits should be
                    // reinterpreted bits as a float.
                    
                    return new UnknownValue();
            }
        }
    }
}