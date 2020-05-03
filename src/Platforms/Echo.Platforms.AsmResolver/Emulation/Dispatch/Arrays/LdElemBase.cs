using System;
using AsmResolver.PE.DotNet.Cil;
using Echo.Concrete.Emulation;
using Echo.Concrete.Emulation.Dispatch;
using Echo.Concrete.Values;
using Echo.Platforms.AsmResolver.Emulation.Values;
using Echo.Platforms.AsmResolver.Emulation.Values.Cli;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch.Arrays
{
    /// <summary>
    /// Provides a base handler for instructions with any variant of the <see cref="CilOpCodes.Ldelem"/> operation codes.
    /// </summary>
    public abstract class LdElemBase : FallThroughOpCodeHandler
    {
        /// <inheritdoc />
        public override DispatchResult Execute(ExecutionContext context, CilInstruction instruction)
        {
            var stack = context.ProgramState.Stack;
            
            // Pop arguments.
            var indexValue = (ICliValue) stack.Pop();
            var arrayValue = (ICliValue) stack.Pop();

            // Check if both array and index are known.
            if (!arrayValue.IsKnown || !indexValue.IsKnown)
            {
                stack.Push(GetUnknownElementValue(context, instruction));
                return base.Execute(context, instruction);
            }

            // Expect an int32 or a native int for index, and extract its value.
            if (indexValue.CliValueType != CliValueType.Int32 && indexValue.CliValueType != CliValueType.NativeInt)
                return DispatchResult.InvalidProgram();
            
            int index = indexValue.InterpretAsI4().I32;

            // Expect an O value with a .NET array in it.
            if (!(arrayValue is OValue { ObjectValue: IDotNetArrayValue dotNetArray }))
                return DispatchResult.InvalidProgram();

            // Check if in bounds.
            if (index < 0 || index >= dotNetArray.Length)
                return new DispatchResult(new IndexOutOfRangeException());
            
            // Push value stored in array.
            stack.Push(GetElementValue(context, instruction, dotNetArray, index));
            
            return base.Execute(context, instruction);
        }

        /// <summary>
        /// Obtains the value in the array using the provided operation code.
        /// </summary>
        /// <param name="context">The context in which the instruction is being executed in.</param>
        /// <param name="instruction">The instruction that is being executed.</param>
        /// <param name="array">The array to get the element from.</param>
        /// <param name="index">The index of the element to get.</param>
        /// <returns>The value.</returns>
        protected abstract IConcreteValue GetElementValue(
            ExecutionContext context,
            CilInstruction instruction,
            IDotNetArrayValue array,
            int index);

        /// <summary>
        /// Creates a fully unknown value that was read from the array. 
        /// </summary>
        /// <param name="context">The context in which the instruction is being executed in.</param>
        /// <param name="instruction">The instruction that is being executed.</param>
        /// <returns>The value.</returns>
        /// <remarks>
        /// This method is called when either the array or the index of the requested element is not fully known.
        /// </remarks>
        protected abstract IConcreteValue GetUnknownElementValue(ExecutionContext context, CilInstruction instruction);
    }
}