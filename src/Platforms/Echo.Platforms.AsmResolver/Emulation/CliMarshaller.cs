using System.Collections.Generic;
using AsmResolver.DotNet;
using AsmResolver.DotNet.Signatures.Types;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;
using Echo.Concrete;
using Echo.Platforms.AsmResolver.Emulation.Stack;

namespace Echo.Platforms.AsmResolver.Emulation
{
    public class CliMarshaller
    {
        private readonly ValueFactory _valueFactory;
        private readonly Dictionary<TypeSignature, TypeDefinition> _resolvedTypes = new();

        public CliMarshaller(ValueFactory valueFactory)
        {
            _valueFactory = valueFactory;
        }

        private TypeSignature GetElementType(TypeSignature type)
        {
            switch (type.ElementType)
            {
                case ElementType.ValueType:
                case ElementType.Enum:
                    if (!_resolvedTypes.TryGetValue(type, out var definition))
                    {
                        definition = type.Resolve();
                        if (definition is not null)
                            _resolvedTypes.Add(type, definition);
                    }

                    if (definition?.GetEnumUnderlyingType() is not { } enumUnderlyingType)
                        throw new CilEmulatorException($"Could not resolve enum type {type.FullName}.");
                    
                    return GetElementType(enumUnderlyingType);
                
                default:
                    return type;
            }
        }

        private bool IsFloatType(TypeSignature type) => type.ElementType is ElementType.R4 or ElementType.R8;

        private bool IsSignedIntegerType(TypeSignature type)
        {
            switch (type.ElementType)
            {
                case ElementType.I1:
                case ElementType.I2:
                case ElementType.I4:
                case ElementType.I8:
                case ElementType.I:
                    return true;

                default:
                    return false;
            }
        }

        private bool IsUnsignedIntegerType(TypeSignature type)
        {
            switch (type.ElementType)
            {
                case ElementType.U1:
                case ElementType.U2:
                case ElementType.U4:
                case ElementType.U8:
                case ElementType.U:
                    return true;

                default:
                    return false;
            }
        }

        public StackSlot ToCliValue(BitVector value, TypeSignature originalType)
        {
            // Resolve the "underlying" element type, if there is any.
            originalType = GetElementType(originalType);

            // Measure the new size on the stack.
            uint size = _valueFactory.GetTypeValueMemoryLayout(originalType).Size;
            if (size <= 4)
                size = 4;
            else if (size <= 8)
                size = 8;

            // Create the new bit vector.
            bool signExtend = IsSignedIntegerType(originalType);
            var result = value.Resize((int) (size * 8), signExtend, _valueFactory.BitVectorPool);

            // Determine the type of value on the stack. 
            StackSlotTypeHint typeHint;
            if (signExtend || IsUnsignedIntegerType(originalType))
                typeHint = StackSlotTypeHint.Integer;
            else if (IsFloatType(originalType))
                typeHint = StackSlotTypeHint.Float;
            else if (originalType.IsValueType)
                typeHint = StackSlotTypeHint.Structure;
            else 
                typeHint = StackSlotTypeHint.Integer; // We might need to classify objects. 
            
            return new StackSlot(result, typeHint);
        }

        public BitVector FromCliValue(StackSlot value, TypeSignature targetType)
        {
            // Resolve the "underlying" element type, if there is any.
            targetType = GetElementType(targetType);
            
            uint size = _valueFactory.GetTypeValueMemoryLayout(targetType).Size;
            
            bool signExtend = IsSignedIntegerType(targetType);
            return value.Contents.Resize((int) (size * 8), signExtend, _valueFactory.BitVectorPool);
        }

    }
}