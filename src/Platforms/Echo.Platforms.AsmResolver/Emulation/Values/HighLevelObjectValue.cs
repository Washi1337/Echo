using System;
using System.Collections.Generic;
using System.Text;
using AsmResolver.DotNet;
using AsmResolver.DotNet.Signatures.Types;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;
using Echo.Concrete.Values;
using Echo.Core.Values;

namespace Echo.Platforms.AsmResolver.Emulation.Values
{
    /// <summary>
    /// Provides a high level implementation of an object that consists of a collection of fields.
    /// </summary>
    /// <remarks>
    /// Instances of the <see cref="HighLevelObjectValue"/> class are passed on by-value. They are used for representing
    /// instances of value types, or the object referenced in an object reference.
    /// </remarks>
    public class HighLevelObjectValue : IDotNetObjectValue
    {
        private readonly IDictionary<IFieldDescriptor, IConcreteValue> _fieldValues =
            new Dictionary<IFieldDescriptor, IConcreteValue>();
        private readonly bool _is32Bit;

        private HighLevelObjectValue(TypeSignature objectType, IDictionary<IFieldDescriptor, IConcreteValue> values,
            bool is32Bit)
        {
            _is32Bit = is32Bit;
            Type = objectType ?? throw new ArgumentNullException(nameof(objectType));
            foreach (var entry in values)
                _fieldValues[entry.Key] = (IConcreteValue) entry.Value.Copy();
        }

        /// <summary>
        /// Creates a new instance of a compound object.
        /// </summary>
        /// <param name="objectType">The type of the object.</param>
        /// <param name="is32Bit">Indicates any pointer that is defined in this object is 32 or 64 bits wide.</param>
        /// <exception cref="NotSupportedException"></exception>
        public HighLevelObjectValue(TypeSignature objectType, bool is32Bit)
        {
            _is32Bit = is32Bit;
            Type = objectType ?? throw new ArgumentNullException(nameof(objectType));

            switch (objectType.ElementType)
            {
                case ElementType.Class:
                case ElementType.ValueType:
                case ElementType.GenericInst:
                    break;
                
                default:
                    throw new NotSupportedException("Unsupported object type.");
            }
            
            InitializeFields();
        }

        /// <inheritdoc />
        public TypeSignature Type
        {
            get;
        }
        
        /// <inheritdoc />
        public bool IsKnown => true;

        /// <inheritdoc />
        public int Size => Type.GetSize(_is32Bit);

        /// <inheritdoc />
        public bool IsValueType => true;

        /// <inheritdoc />
        public bool? IsZero => false;

        /// <inheritdoc />
        public bool? IsNonZero => true;

        /// <inheritdoc />
        public bool? IsPositive => true;

        /// <inheritdoc />
        public bool? IsNegative => false;

        /// <inheritdoc />
        public IValue Copy() => new HighLevelObjectValue(Type, _fieldValues, _is32Bit);

        private void InitializeFields()
        {
            var type = Type.GetUnderlyingTypeDefOrRef().Resolve();
            while (type is {})
            {
                foreach (var field in type.Fields)
                {
                    if (!field.IsStatic)
                        _fieldValues[field] = new UnknownValue();
                }

                type = type.BaseType?.Resolve();
            }
        }

        /// <inheritdoc />
        public IConcreteValue GetFieldValue(IFieldDescriptor field)
        {
            if (!_fieldValues.TryGetValue(field, out var value))
                throw new ArgumentException($"Field {field} is not defined in this object.");
            return value;
        }

        /// <inheritdoc />
        public void SetFieldValue(IFieldDescriptor field, IConcreteValue value)
        {
            if (!_fieldValues.ContainsKey(field))
                throw new ArgumentException($"Field {field} is not defined in this object.");
            _fieldValues[field] = value;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.Append(Type.Name);
            builder.Append(" {");

            int count = 0;
            foreach (var entry in _fieldValues)
            {
                builder.Append($"{entry.Key.Name}: {entry.Value}");
                count++;
                if (count < _fieldValues.Count)
                    builder.Append(", ");
            }

            builder.Append("}");

            return builder.ToString();
        }
    }
}