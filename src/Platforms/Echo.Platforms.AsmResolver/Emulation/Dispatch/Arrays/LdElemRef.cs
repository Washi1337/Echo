using System.Collections.Generic;
using AsmResolver.PE.DotNet.Cil;
using Echo.Concrete.Emulation;
using Echo.Concrete.Values;
using Echo.Concrete.Values.ReferenceType;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch.Arrays
{
    /// <summary>
    /// Provides a base handler for instructions with the <see cref="CilOpCodes.Ldelem_Ref"/> operation code.
    /// </summary>
    public class LdElemRef : LdElemBase
    {
        /// <inheritdoc />
        public override IReadOnlyCollection<CilCode> SupportedOpCodes => new[]
        {
            CilCode.Ldelem_Ref
        };

        /// <inheritdoc />
        protected override IConcreteValue GetValue(ExecutionContext context, CilInstruction instruction, ArrayValue array,
            int index)
        {
            var value = array[index];
            
            if (value.IsValueType)
                return new UnknownValue();
            
            return (IConcreteValue) value.Copy();
        }
    }
}