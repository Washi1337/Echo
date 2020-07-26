using System;
using AsmResolver.PE.DotNet.Cil;
using Echo.Concrete.Emulation;
using Echo.Concrete.Emulation.Dispatch;
using Echo.Core;
using Echo.Platforms.AsmResolver.Emulation.Values;
using Echo.Platforms.AsmResolver.Emulation.Values.Cli;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch.Arrays
{
    /// <summary>
    /// Provides a base handler for instructions with any variant of the <see cref="CilOpCodes.Stelem"/> operation codes.
    /// </summary>
    public abstract class StElemBase : FallThroughOpCodeHandler
    {
        /// <inheritdoc />
        public override DispatchResult Execute(ExecutionContext context, CilInstruction instruction)
        {
            var stack = context.ProgramState.Stack;
            
            // Pop arguments.
            var valueValue = (ICliValue) stack.Pop();
            var indexValue = (ICliValue) stack.Pop();
            var arrayValue = (ICliValue) stack.Pop();

            // Check if both array and index are known.
            if (!arrayValue.IsKnown)
                throw new DispatchException($"Destination array value for instruction at offset IL_{instruction.Offset:X4} is unknown.");
            if (!indexValue.IsKnown)
                throw new DispatchException($"Index for instruction at offset IL_{instruction.Offset:X4} is unknown.");

            // Expect an int32 or a native int for index, and extract its value.
            if (indexValue.CliValueType != CliValueType.Int32 && indexValue.CliValueType != CliValueType.NativeInt)
                return DispatchResult.InvalidProgram();
            
            int index = indexValue.InterpretAsI4().I32;

            // Expect an O value with a .NET array in it.
            switch (arrayValue)
            {
                case OValue { IsZero: { Value: TrileanValue.True } }:
                    // Pushed array object is null.
                    return new DispatchResult(new NullReferenceException());
                
                case OValue { ReferencedObject: IDotNetArrayValue dotNetArray }:
                    // Check if in bounds.
                    if (index < 0 || index >= dotNetArray.Length)
                        return new DispatchResult(new IndexOutOfRangeException());
            
                    // Push value stored in array.
                    StoreElement(context, instruction, dotNetArray, index, valueValue);
                    return base.Execute(context, instruction);
                
                default:
                    return DispatchResult.InvalidProgram();

            }
        }

        /// <summary>
        /// Stores an element in the provided array using the instruction's operation code.
        /// </summary>
        /// <param name="context">The execution context the instruction is being executed in.</param>
        /// <param name="instruction">The instruction that is being executed.</param>
        /// <param name="dotNetArray">The array to store the value in.</param>
        /// <param name="index">The index to store the element at.</param>
        /// <param name="value">The value of the element.</param>
        protected abstract void StoreElement(
            ExecutionContext context,
            CilInstruction instruction,
            IDotNetArrayValue dotNetArray,
            int index,
            ICliValue value);
    }
}