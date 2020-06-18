using System;
using AsmResolver.DotNet;
using Echo.Core.Code;
using Echo.Platforms.AsmResolver.Emulation.Values;

namespace Echo.Platforms.AsmResolver.Emulation
{
    /// <summary>
    /// Provides an adapter for static fields represented by a <see cref="IFieldDescriptor"/> to an instance
    /// of the <see cref="IVariable"/> interface. 
    /// </summary>
    public class StaticField : IVariable
    {
        /// <summary>
        /// Creates a new instance of the <see cref="StaticField"/> class.
        /// </summary>
        /// <param name="field">The field to encapsulate </param>
        public StaticField(IFieldDescriptor field)
        {
            if (field == null)
                throw new ArgumentNullException(nameof(field));
            if (field.Signature.HasThis)
                throw new ArgumentException("Field must be static.");
            Field = field;
        }

        /// <inheritdoc />
        public string Name => Field.Name;

        /// <summary>
        /// Gets the static field that was encapsulated.
        /// </summary>
        public IFieldDescriptor Field
        {
            get;
        }

        /// <summary>
        /// Gets or sets the value assigned to the static field.
        /// </summary>
        public IDotNetValue Value
        {
            get;
            set;
        }

        /// <inheritdoc />
        public override string ToString() => Field.FullName;
    }
}