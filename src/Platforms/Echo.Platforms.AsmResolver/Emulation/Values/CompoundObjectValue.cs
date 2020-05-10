using System;
using System.Collections.Generic;
using System.Text;
using AsmResolver.DotNet;
using AsmResolver.DotNet.Signatures;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;
using Echo.Concrete.Values;
using Echo.Core.Values;

namespace Echo.Platforms.AsmResolver.Emulation.Values
{
    /// <summary>
    /// Represents an object that consists of a collection of fields.
    /// </summary>
    /// <remarks>
    /// Instances of the <see cref="CompoundObjectValue"/> class are passed on by-value. They are used for representing
    /// instances of value types, or the object referenced in an object reference.
    /// </remarks>
    public class CompoundObjectValue : IDotNetValue
    {
        private readonly IDictionary<FieldDefinition, IConcreteValue> _fieldValues =
            new Dictionary<FieldDefinition, IConcreteValue>();
        private readonly bool _is32Bit;

        private CompoundObjectValue(TypeSignature objectType, IDictionary<FieldDefinition, IConcreteValue> values,
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
        public CompoundObjectValue(TypeSignature objectType, bool is32Bit)
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

        /// <summary>
        /// Gets or sets the value of a field stored in the object.
        /// </summary>
        /// <param name="field">The field.</param>
        /// <exception cref="ArgumentException">Thrown when the field is not defined in this object.</exception>
        public IConcreteValue this[FieldDefinition field]
        {
            get
            {
                if (!_fieldValues.TryGetValue(field, out var value))
                    throw new ArgumentException($"Field {field} is not defined in this object.");
                return value;
            }
            set
            {
                if (!_fieldValues.ContainsKey(field))
                    throw new ArgumentException($"Field {field} is not defined in this object.");
                _fieldValues[field] = value;
            }
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
        public IValue Copy() => new CompoundObjectValue(Type, _fieldValues, _is32Bit);

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