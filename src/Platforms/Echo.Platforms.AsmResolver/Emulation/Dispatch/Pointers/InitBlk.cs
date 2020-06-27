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
    /// Provides a handler for instructions with a variation of the <see cref="CilOpCodes.Initblk"/> operation code.
    /// </summary>
    public class InitBlk : FallThroughOpCodeHandler
    {
        /// <inheritdoc />
        public override IReadOnlyCollection<CilCode> SupportedOpCodes => new[]
        {
            CilCode.Initblk
        };

        /// <inheritdoc />
        public override DispatchResult Execute(ExecutionContext context, CilInstruction instruction)
        {
            var stack = context.ProgramState.Stack;

            // Pop arguments.
            var address = (ICliValue) stack.Pop();
            var initValue = (ICliValue) stack.Pop();
            var lengthValue = (ICliValue) stack.Pop();

            // Interpret arguments.
            if (!(address is IPointerValue pointerValue))
                return DispatchResult.InvalidProgram();

            if (!lengthValue.IsKnown)
                throw new DispatchException("Number of bytes to initialize is unknown.");
            int length = lengthValue.InterpretAsI4().I32;
            
            var byteInitValue = initValue.InterpretAsU1();
            
            // Prepare initialization data.
            Span<byte> data = stackalloc byte[length];
            data.Fill((byte) (byteInitValue.U32 & 0xFF));
            
            Span<byte> bitmask = stackalloc byte[length];
            data.Fill((byte) (byteInitValue.Mask & 0xFF));

            // Write data.
            pointerValue.WriteBytes(0, data, bitmask);
            
            return base.Execute(context, instruction);
        }
    }
}