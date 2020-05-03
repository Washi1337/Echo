using System;
using AsmResolver.PE.DotNet.Cil;
using Echo.Concrete.Emulation;
using Echo.Concrete.Emulation.Dispatch;
using Echo.Platforms.AsmResolver.Emulation.Values;
using Echo.Platforms.AsmResolver.Emulation.Values.Cli;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch.Arrays
{
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
            if (!(arrayValue is OValue { ObjectValue: IDotNetArrayValue dotNetArray }))
                return DispatchResult.InvalidProgram();

            // Check if in bounds.
            if (index < 0 || index >= dotNetArray.Length)
                return new DispatchResult(new IndexOutOfRangeException());
            
            // Push value stored in array.
            StoreElement(context, instruction, dotNetArray, index, valueValue);
            
            return base.Execute(context, instruction);
        }

        protected abstract void StoreElement(
            ExecutionContext context,
            CilInstruction instruction,
            IDotNetArrayValue dotNetArray,
            int index,
            ICliValue valueValue);
    }
}