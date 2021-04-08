using System;
using System.Collections.Generic;
using AsmResolver.PE.DotNet.Cil;
using Echo.Concrete.Emulation;
using Echo.Concrete.Emulation.Dispatch;
using Echo.Concrete.Values.ReferenceType;
using Echo.Platforms.AsmResolver.Emulation.Values.Cli;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch.Pointers
{
    /// <summary>
    /// Provides a handler for instructions with a variation of the <see cref="CilOpCodes.Cpblk"/> operation code.
    /// </summary>
    public class CpBlk : FallThroughOpCodeHandler
    {
        /// <inheritdoc />
        public override IReadOnlyCollection<CilCode> SupportedOpCodes => new[]
        {
            CilCode.Cpblk
        };

        /// <inheritdoc />
        public override DispatchResult Execute(CilExecutionContext context, CilInstruction instruction)
        {
            var stack = context.ProgramState.Stack;

            // Pop arguments.
            var lengthValue = (ICliValue) stack.Pop();
            var sourceAddress = (ICliValue) stack.Pop();
            var destinationAddress = (ICliValue) stack.Pop();

            // Interpret arguments.
            if (!(destinationAddress is IPointerValue destinationPointer) || !(sourceAddress is IPointerValue sourcePointer))
                return DispatchResult.InvalidProgram();

            if (!lengthValue.IsKnown)
                throw new DispatchException("Number of bytes to copy is unknown.");
            int length = lengthValue.InterpretAsI4().I32;
            
            // Copy data.
            Span<byte> data = stackalloc byte[length];
            Span<byte> bitmask = stackalloc byte[length];
            sourcePointer.ReadBytes(0, data, bitmask);
            destinationPointer.WriteBytes(0, data, bitmask);
            
            return base.Execute(context, instruction);
        }
    }
}