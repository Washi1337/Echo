using System;
using Echo.Concrete.Values;
using Echo.Core.Emulation;
using Echo.Platforms.AsmResolver.Emulation.Values;

namespace Echo.Platforms.AsmResolver.Emulation
{
    /// <summary>
    /// Represents a snapshot of the state of the program in a particular point of execution of a CIL method body.
    /// </summary>
    public class CilProgramState : IProgramState<IConcreteValue>
    {
        private readonly IValueFactory _valueFactory;

        /// <summary>
        /// Creates a new empty instance of the <see cref="CilProgramState"/> class.
        /// </summary>
        public CilProgramState(IValueFactory valueFactory)
        {
            _valueFactory = valueFactory ?? throw new ArgumentNullException(nameof(valueFactory));
            Stack = new StackState<IConcreteValue>();
            Variables = new CilVariableState(valueFactory);
        }

        /// <inheritdoc />
        public long ProgramCounter
        {
            get;
            set;
        }

        /// <inheritdoc />
        public IStackState<IConcreteValue> Stack
        {
            get;
            private set;
        }

        /// <inheritdoc />
        public IVariableState<IConcreteValue> Variables
        {
            get;
            private set;
        }

        /// <inheritdoc />
        public IProgramState<IConcreteValue> Copy()
        {
            return new CilProgramState(_valueFactory)
            {
                Stack = Stack.Copy(),
                Variables = Variables.Copy()
            };
        }
    }
}