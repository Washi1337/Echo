using System;
using System.Collections.Generic;
using Echo.Concrete.Values;
using Echo.Core.Code;
using Echo.Core.Emulation;
using Echo.Platforms.AsmResolver.Emulation.Values;

namespace Echo.Platforms.AsmResolver.Emulation
{
    /// <summary>
    /// Provides an implementation of a variable state in a CIL environment, that initially assigns for every
    /// variable a default value using an instance of the <see cref="IValueFactory"/> interface.
    /// </summary>
    public class CilVariableState : IVariableState<IConcreteValue>
    {
        private readonly IDictionary<CilVariable, IConcreteValue> _variables = new Dictionary<CilVariable, IConcreteValue>();
        private readonly IValueFactory _valueFactory;

        /// <summary>
        /// Creates a new variable state snapshot, using the provided default value.
        /// </summary>
        /// <param name="valueFactory">The factory responsible for creating the default value for all variables.</param>
        public CilVariableState(IValueFactory valueFactory)
        {
            _valueFactory = valueFactory ?? throw new ArgumentNullException(nameof(valueFactory));
        }
        
        /// <inheritdoc />
        public IConcreteValue this[IVariable variable]
        {
            get
            {
                var cilVariable = (CilVariable) variable;
                if (!_variables.TryGetValue(cilVariable, out var value))
                {
                    value = _valueFactory.CreateValue(cilVariable.Variable.VariableType, false);
                    _variables[cilVariable] = value;
                }

                return value;
            }
            set => _variables[(CilVariable) variable] = value;
        }

        /// <inheritdoc />
        public IEnumerable<IVariable> GetAllRecordedVariables() => _variables.Keys;

        /// <inheritdoc />
        public IVariableState<IConcreteValue> Copy()
        {
            var result = new CilVariableState(_valueFactory);

            foreach (var entry in _variables)
                result._variables.Add(entry.Key, (IConcreteValue) entry.Value.Copy());
            
            return result;
        }
    }
}