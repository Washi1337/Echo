using System;
using System.Buffers.Binary;
using System.Collections.Concurrent;
using System.Text;
using AsmResolver;
using AsmResolver.DotNet;
using AsmResolver.DotNet.Signatures.Types;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;
using Echo.Concrete.Values;
using Echo.Concrete.Values.ReferenceType;
using Echo.Concrete.Values.ValueType;
using Echo.Platforms.AsmResolver.Emulation.Values;

namespace Echo.Platforms.AsmResolver.Emulation
{
    /// <summary>
    /// Provides a mechanism for creating and storing static fields within an instance of a .NET virtual machine.
    /// </summary>
    public class StaticFieldFactory
    {
        private readonly IValueFactory _valueFactory;
        private readonly ConcurrentDictionary<IFieldDescriptor, StaticField> _cache = new();

        /// <summary>
        /// Creates a new instance of the <see cref="StaticFieldFactory"/> class.
        /// </summary>
        /// <param name="valueFactory">The factory responsible for creating unknown values.</param>
        public StaticFieldFactory(IValueFactory valueFactory)
        {
            _valueFactory = valueFactory ?? throw new ArgumentNullException(nameof(valueFactory));
        }
        
        /// <summary>
        /// Gets or creates an instance of a static field. 
        /// </summary>
        /// <param name="field">The field to encapsulate.</param>
        /// <returns>The static field.</returns>
        public StaticField Get(IFieldDescriptor field)
        {
            if (field is null)
                throw new ArgumentNullException(nameof(field));
            if (field.Signature.HasThis)
                throw new ArgumentException("Field has the HasThis flag set in the field signature.");

            StaticField staticField;
            while (!_cache.TryGetValue(field, out staticField))
            {
                staticField = Create(field);
                _cache.TryAdd(field, staticField);
            }

            return staticField;
        }

        private StaticField Create(IFieldDescriptor field) => new StaticField(field)
        {
            Value = GetInitialFieldValue(field)
        };

        private IConcreteValue GetInitialFieldValue(IFieldDescriptor field)
        {
            var definition = field.Resolve();
            
            if (definition != null)
            {
                // Check if the field has an initial value through a Constant row.
                var constant = definition.Constant;
                if (constant?.Value is not null)
                {
                    return ObjectToCtsValue(
                        constant.Value.Data,
                        definition.Module.CorLibTypeFactory.FromElementType(constant.Type));
                }

                // Check if the field has an initial value through a field RVA row.
                if (definition.HasFieldRva && definition.FieldRva is IReadableSegment readableSegment)
                {
                    return ObjectToCtsValue(
                        readableSegment.ToArray(),
                        field.Signature.FieldType);
                }
            }

            return _valueFactory.CreateValue(field.Signature.FieldType, false);
        }

        private IConcreteValue ObjectToCtsValue(byte[] rawData, TypeSignature type)
        {
            switch (type.ElementType)
            {
                case ElementType.Boolean:
                case ElementType.I1:
                case ElementType.U1:
                    return new Integer8Value(rawData[0]);

                case ElementType.Char:
                case ElementType.I2:
                case ElementType.U2:
                    return new Integer16Value(BinaryPrimitives.ReadUInt16LittleEndian(rawData));

                case ElementType.I4:
                case ElementType.U4:
                    return new Integer32Value(BinaryPrimitives.ReadUInt32LittleEndian(rawData));

                case ElementType.I8:
                case ElementType.U8:
                    return new Integer64Value(BinaryPrimitives.ReadUInt64LittleEndian(rawData));

                case ElementType.R4:
                    return new Float32Value(BitConverter.ToSingle(rawData, 0));

                case ElementType.R8:
                    return new Float64Value(BitConverter.ToDouble(rawData, 0));

                case ElementType.String:
                    return new ObjectReference(
                        _valueFactory.GetStringValue(Encoding.Unicode.GetString(rawData)),
                        _valueFactory.Is32Bit);

                case ElementType.ValueType:
                    var memory = _valueFactory.AllocateMemory(rawData.Length, false);
                    memory.WriteBytes(0, rawData);
                    return new LleStructValue(_valueFactory, type, memory);
                
                default:
                    return _valueFactory.CreateValue(type, false);
            }
        }
        
    }
}