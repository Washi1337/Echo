using System;
using System.Collections.Generic;
using System.Text;
using AsmResolver.DotNet;
using AsmResolver.DotNet.Memory;
using AsmResolver.DotNet.Signatures.Types;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;
using Echo.Concrete.Values;
using Echo.Concrete.Values.ReferenceType;
using Echo.Concrete.Values.ValueType;
using Echo.Platforms.AsmResolver.Emulation.Values.Cli;

namespace Echo.Platforms.AsmResolver.Emulation.Values
{
    /// <summary>
    /// Provides a default implementation of the <see cref="IValueFactory"/> interface.  
    /// </summary>
    public class DefaultValueFactory : IValueFactory
    {
        private readonly Dictionary<string, StringValue> _cachedStrings = new();
        private readonly Dictionary<ITypeDescriptor, TypeMemoryLayout> _memoryLayouts = new();
        private readonly ModuleDefinition _contextModule;

        /// <summary>
        /// Creates a new instance of the <see cref="DefaultValueFactory"/> class.
        /// </summary>
        public DefaultValueFactory(ModuleDefinition contextModule, bool is32Bit)
        {
            _contextModule = contextModule ?? throw new ArgumentNullException(nameof(contextModule));
            Is32Bit = is32Bit;
        }

        /// <inheritdoc />
        public bool Is32Bit
        {
            get;
        }

        /// <inheritdoc />
        public IConcreteValue CreateValue(TypeSignature type, bool initialize) => initialize
            ? CreateDefault(type)
            : CreateUnknown(type);

        /// <inheritdoc />
        public ObjectReference CreateObject(TypeSignature type, bool initialize) => 
            new ObjectReference(AllocateStruct(type, initialize), Is32Bit);

        private IConcreteValue CreateDefault(TypeSignature type)
        {
            while (true)
            {
                switch (type.ElementType)
                {
                    case ElementType.Boolean:
                    case ElementType.I1:
                    case ElementType.U1:
                        return new Integer8Value(0);

                    case ElementType.I2:
                    case ElementType.U2:
                    case ElementType.Char:
                        return new Integer16Value(0);

                    case ElementType.I4:
                    case ElementType.U4:
                        return new Integer32Value(0);

                    case ElementType.I8:
                    case ElementType.U8:
                        return new Integer64Value(0);

                    case ElementType.R4:
                        return new Float32Value(0);

                    case ElementType.R8:
                        return new Float64Value(0);

                    case ElementType.FnPtr:
                    case ElementType.Ptr:
                    // TODO: this could be improved when CLI pointers are supported.

                    case ElementType.I:
                    case ElementType.U:
                        return new NativeIntegerValue(0, Is32Bit);

                    case ElementType.MVar:
                    case ElementType.Var:
                        // TODO: resolve type argument (maybe add a generic context parameter to this factory method?)
                        return ObjectReference.Null(Is32Bit);

                    case ElementType.ByRef:
                    case ElementType.ValueType:
                    case ElementType.TypedByRef:
                        return AllocateStruct(type, true);

                    case ElementType.CModReqD:
                    case ElementType.CModOpt:
                    case ElementType.Pinned:
                        // Annotated types don't affect the actual type of value. Move to the inner type signature.
                        type = ((CustomModifierTypeSignature) type).BaseType;
                        continue;

                    default:
                        return ObjectReference.Null(Is32Bit);
                }
            }
        }

        private IConcreteValue CreateUnknown(TypeSignature type)
        {
            while (true)
            {
                switch (type.ElementType)
                {
                    case ElementType.Boolean:
                        // For booleans, only the least significant bit is unknown (it's either zero or one).
                        return new Integer8Value(0, 0xFE);

                    case ElementType.I1:
                    case ElementType.U1:
                        return new Integer8Value(0, 0);

                    case ElementType.I2:
                    case ElementType.U2:
                    case ElementType.Char:
                        return new Integer16Value(0, 0);

                    case ElementType.I4:
                    case ElementType.U4:
                        return new Integer32Value(0, 0);

                    case ElementType.I8:
                    case ElementType.U8:
                        return new Integer64Value(0, 0);

                    case ElementType.R4:
                        return new Float32Value(0); // TODO: use unknown floats.

                    case ElementType.R8:
                        return new Float64Value(0); // TODO: use unknown floats.

                    case ElementType.FnPtr:
                    case ElementType.Ptr:
                        // TODO: this could be improved when CLI pointers are supported.
                    
                    case ElementType.I:
                    case ElementType.U:
                        return new NativeIntegerValue(0, 0, Is32Bit);

                    case ElementType.MVar:
                    case ElementType.Var:
                        // TODO: resolve type argument (maybe add a generic context parameter to this factory method?)
                        return new ObjectReference(null, false, Is32Bit);

                    case ElementType.ByRef:
                    case ElementType.ValueType:
                    case ElementType.TypedByRef:
                        return AllocateStruct(type, false);

                    case ElementType.CModReqD:
                    case ElementType.CModOpt:
                    case ElementType.Pinned:
                        // Annotated types don't affect the actual type of value. Move to the inner type signature.

                        type = ((CustomModifierTypeSignature) type).BaseType;
                        continue;

                    default:
                        // At this point we know it is at least an object reference, as value types have been captured by
                        // other cases. Return an unknown object reference.

                        return new ObjectReference(null, false, Is32Bit);
                }
            }
        }

        /// <inheritdoc />
        public IMemoryAccessValue AllocateMemory(int size, bool initialize) => 
            new MemoryBlockValue(size, initialize);

        /// <inheritdoc />
        public IDotNetArrayValue AllocateArray(TypeSignature elementType, int length)
        {
            if (elementType.IsValueType)
            {
                int size = length * (int) GetTypeMemoryLayout(elementType).Size;
                var memory = AllocateMemory(size, true);
                return new LleStructValue(this, new SzArrayTypeSignature(elementType), memory);
            }

            return new HleArrayValue(this, elementType, length);
        }

        /// <inheritdoc />
        public IDotNetStructValue AllocateStruct(TypeSignature type, bool initialize)
        {
            IDotNetStructValue result;
            
            if (type.IsValueType)
            {
                var memoryLayout = GetTypeMemoryLayout(type);
                var contents = AllocateMemory((int) memoryLayout.Size, initialize);
                result = new LleStructValue(this, type, contents);
            }
            else
            {
                result = new HleStructValue(this, type, Is32Bit);
            }

            return result;
        }

        /// <inheritdoc />
        public StringValue GetStringValue(string value)
        {
            if (!_cachedStrings.TryGetValue(value, out var stringValue))
            {
                var rawMemory = AllocateMemory(value.Length * 2, false);
                var span = new ReadOnlySpan<byte>(Encoding.Unicode.GetBytes(value));
                rawMemory.WriteBytes(0, span);
                stringValue = new StringValue(_contextModule.CorLibTypeFactory.String, rawMemory);
                _cachedStrings.Add(value, stringValue);
            }

            return stringValue;
        }

        /// <inheritdoc />
        public TypeMemoryLayout GetTypeMemoryLayout(ITypeDescriptor type)
        {
            type = _contextModule.CorLibTypeFactory.FromType(type) ?? type;
            if (!_memoryLayouts.TryGetValue(type, out var memoryLayout))
            {
                memoryLayout = type switch
                {
                    ITypeDefOrRef typeDefOrRef => typeDefOrRef.GetImpliedMemoryLayout(Is32Bit),
                    TypeSignature signature => signature.GetImpliedMemoryLayout(Is32Bit),
                    _ => throw new ArgumentOutOfRangeException(nameof(type))
                };

                _memoryLayouts[type] = memoryLayout;
            }

            return memoryLayout;
        }

        /// <inheritdoc />
        public void Dispose()
        {
        }
    }
}