using System;
using System.Collections.Generic;
using System.Text;
using AsmResolver.DotNet;
using AsmResolver.DotNet.Signatures.Types;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;
using Echo.Concrete.Values;
using Echo.Core;
using Echo.Core.Values;

namespace Echo.Platforms.AsmResolver.Emulation.Values
{
    /// <summary>
    /// Provides a high level implementation of a structure that consists of a collection of fields.
    /// </summary>
    /// <remarks>
    /// This class is <strong>not</strong> meant to be used as an object reference. Instances of the
    /// <see cref="HleStructValue"/> class are passed on by-value. They are used for representing instances of value
    /// types, or the object referenced in an object reference, not the object reference itself. 
    /// </remarks>
    public class HleStructValue : IDotNetStructValue
    {
        private readonly Dictionary<IFieldDescriptor, IConcreteValue> _fieldValues =
            new Dictionary<IFieldDescriptor, IConcreteValue>();
        private readonly bool _is32Bit;

        private HleStructValue(TypeSignature objectType, IDictionary<IFieldDescriptor, IConcreteValue> values,
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
        /// <param name="valueFactory">The object responsible for creating instances of values in a field.</param>
        /// <param name="objectType">The type of the object.</param>
        /// <param name="initialize">Indicates whether the object should be initialized with zeroes.</param>
        /// <exception cref="NotSupportedException"></exception>
        public HleStructValue(IValueFactory valueFactory, TypeSignature objectType, bool initialize)
        {
            if (valueFactory == null)
                throw new ArgumentNullException(nameof(valueFactory));
            
            _is32Bit = valueFactory.Is32Bit;
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
            
            InitializeFields(valueFactory, initialize);
        }

        /// <inheritdoc />
        public TypeSignature Type
        {
            get;
        }
        
        /// <inheritdoc />
        public bool IsKnown
        {
            get
            { 
                foreach (var value in _fieldValues.Values)
                {
                    if (!value.IsKnown)
                        return false;
                }
                
                return true;
            }
        }

        /// <inheritdoc />
        public int Size => _is32Bit ? 4 : 8;

        /// <inheritdoc />
        public bool IsValueType => true;

        /// <inheritdoc />
        public Trilean IsZero => false;

        /// <inheritdoc />
        public Trilean IsNonZero => true;

        /// <inheritdoc />
        public Trilean IsPositive => true;

        /// <inheritdoc />
        public Trilean IsNegative => false;

        /// <inheritdoc />
        public IValue Copy() => new HleStructValue(Type, _fieldValues, _is32Bit);

        private void InitializeFields(IValueFactory valueFactory, bool initialize)
        {
            var type = Type.GetUnderlyingTypeDefOrRef().Resolve();
            while (type is {})
            {
                foreach (var field in type.Fields)
                {
                    if (!field.IsStatic)
                    {
                        var value = valueFactory.CreateValue(field.Signature.FieldType, initialize);
                        _fieldValues[field] = value;
                    }
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