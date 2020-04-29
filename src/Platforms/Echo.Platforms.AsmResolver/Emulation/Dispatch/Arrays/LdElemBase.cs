using System;
using AsmResolver.PE.DotNet.Cil;
using Echo.Concrete.Emulation;
using Echo.Concrete.Emulation.Dispatch;
using Echo.Concrete.Values;
using Echo.Concrete.Values.ReferenceType;
using Echo.Concrete.Values.ValueType;
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
            var indexValue = stack.Pop();
            var arrayValue = stack.Pop() as ArrayValue;

            // Extract actual integer index.
            int? index = indexValue switch
            {
                Integer32Value i32 => i32.I32,
                NativeIntegerValue nativeInt => (int) nativeInt.ToInt64().I64,
                _ => null
            };

            // Check for invalid CIL.
            if (arrayValue is null || index is null)
                return new DispatchResult(new InvalidProgramException());

            // Check if index is actually known.
            if (!indexValue.IsKnown)
            {
                // TODO: dispatch event, allowing the user to handle unknown array indices.
                throw new DispatchException("Could not obtain an element from an array.",
                    new NotSupportedException("Obtaining values from indices with unknown bits is not supported."));
            }

            // Check if in bounds.
            if (index < 0 || index >= arrayValue.Length)
                return new DispatchResult(new IndexOutOfRangeException());
            
            // Push value stored in array.
            var value = GetValue(context, instruction, arrayValue, index.Value);

            stack.Push(value);
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
        protected abstract IConcreteValue GetValue(
            ExecutionContext context, 
            CilInstruction instruction,
            ArrayValue array,
            int index);
    }
}