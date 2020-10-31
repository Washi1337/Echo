using System;
using System.Collections.Concurrent;
using System.Text;
using AsmResolver.DotNet;
using AsmResolver.DotNet.Signatures.Types;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;
using Echo.Concrete.Values;
using Echo.Concrete.Values.ReferenceType;
using Echo.Concrete.Values.ValueType;
using Echo.Platforms.AsmResolver.Emulation.Values;
using Echo.Platforms.AsmResolver.Emulation.Values.Cli;

namespace Echo.Platforms.AsmResolver.Emulation
{
    /// <summary>
    /// Provides a mechanism for creating and storing static fields within an instance of a .NET virtual machine.
    /// </summary>
    public class StaticFieldFactory
    {
        private readonly IUnknownValueFactory _unknownValueFactory;
        private readonly IMemoryAllocator _memoryAllocator;

        private readonly ConcurrentDictionary<IFieldDescriptor, StaticField> _cache =
            new ConcurrentDictionary<IFieldDescriptor, StaticField>();

        /// <summary>
        /// Creates a new instance of the <see cref="StaticFieldFactory"/> class.
        /// </summary>
        /// <param name="unknownValueFactory">The factory responsible for creating unknown values.</param>
        /// <param name="memoryAllocator">The object responsible for allocating memory for default values of fields.</param>
        public StaticFieldFactory(IUnknownValueFactory unknownValueFactory, IMemoryAllocator memoryAllocator)
        {
            _unknownValueFactory = unknownValueFactory ?? throw new ArgumentNullException(nameof(unknownValueFactory));
            _memoryAllocator = memoryAllocator ?? throw new ArgumentNullException(nameof(memoryAllocator));
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
                staticField = new StaticField(field);

                var definition = field.Resolve();
                
                // Check if the field has a constant.
                var constant = definition?.Constant;
                if (constant?.Value != null)
                    staticField.Value = ObjectToCtsValue(constant.Value.Data, constant.Type);

                staticField.Value ??= _unknownValueFactory.CreateUnknown(field.Signature.FieldType);
                _cache.TryAdd(field, staticField);
            }

            return staticField;
        }

        private IConcreteValue ObjectToCtsValue(byte[] rawData, ElementType type)
        {
            switch (type)
            {
                case ElementType.Boolean:
                case ElementType.I1:
                case ElementType.U1:
                    return new Integer8Value(rawData[0]);

                case ElementType.Char:
                case ElementType.I2:
                case ElementType.U2:
                    return new Integer16Value(BitConverter.ToUInt16(rawData, 0));

                case ElementType.I4:
                case ElementType.U4:
                    return new Integer32Value(BitConverter.ToUInt32(rawData, 0));

                case ElementType.I8:
                case ElementType.U8:
                    return new Integer64Value(BitConverter.ToUInt64(rawData, 0));

                case ElementType.R4:
                    return new Float32Value(BitConverter.ToSingle(rawData, 0));

                case ElementType.R8:
                    return new Float64Value(BitConverter.ToDouble(rawData, 0));

                case ElementType.String:
                    return new ObjectReference(
                        _memoryAllocator.GetStringValue(Encoding.Unicode.GetString(rawData)),
                        _memoryAllocator.Is32Bit);

                default:
                    return null;
            }
        }
        
    }
}