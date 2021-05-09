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
        private readonly Dictionary<IVariable, IConcreteValue> _variables = new();
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
                if (!_variables.TryGetValue(variable, out var value))
                {
                    value = variable switch
                    {
                        CilVariable cilVariable => _valueFactory.CreateValue(cilVariable.Variable.VariableType, false),
                        CilParameter cilParameter => _valueFactory.CreateValue(cilParameter.Parameter.ParameterType,
                            false),
                        _ => throw new NotSupportedException($"IVariable implementation {variable.GetType()} is not supported.")
                    };
                    _variables[variable] = value;
                }

                return value;
            }
            set
            {
                if (!(variable is CilParameter) && !(variable is CilVariable))
                    throw new NotSupportedException($"IVariable implementation {variable.GetType()} is not supported.");

                _variables[variable] = value;
            }
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

        /// <inheritdoc />
        public bool Remove(IVariable variable)
        {
            return _variables.Remove(variable);
        }

        /// <inheritdoc />
        public void Clear()
        {
            _variables.Clear();
        }
    }
}