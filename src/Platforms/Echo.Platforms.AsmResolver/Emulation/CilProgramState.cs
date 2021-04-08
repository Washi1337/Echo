using System;
using Echo.Concrete.Values;
using Echo.Core.Emulation;
using Echo.Platforms.AsmResolver.Emulation.Values;
using Echo.Platforms.AsmResolver.Emulation.Values.Cli;

namespace Echo.Platforms.AsmResolver.Emulation
{
    /// <summary>
    /// Represents a snapshot of the state of the program in a particular point of execution of a CIL method body.
    /// </summary>
    public class CilProgramState
    {
        /// <summary>
        /// Creates a new empty instance of the <see cref="CilProgramState"/> class.
        /// </summary>
        public CilProgramState(IValueFactory valueFactory)
        {
            Stack = new StackState<ICliValue>();
            Variables = new CilVariableState(valueFactory);
        }

        public long ProgramCounter
        {
            get;
            set;
        }

        public IStackState<ICliValue> Stack
        {
            get;
        }

        public IVariableState<IConcreteValue> Variables
        {
            get;
        }

    }
}