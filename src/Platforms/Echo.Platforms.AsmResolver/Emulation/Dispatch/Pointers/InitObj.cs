using System;
using System.Collections.Generic;
using AsmResolver.DotNet;
using AsmResolver.PE.DotNet.Cil;
using Echo.Concrete.Emulation;
using Echo.Concrete.Emulation.Dispatch;
using Echo.Platforms.AsmResolver.Emulation.Values.Cli;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch.Pointers
{
    /// <summary>
    /// Provides a handler for instructions with the <see cref="CilOpCodes.Initobj"/> operation code.
    /// </summary>
    public class InitObj : FallThroughOpCodeHandler
    {
        /// <inheritdoc />
        public override IReadOnlyCollection<CilCode> SupportedOpCodes => new[]
        {
            CilCode.Initobj
        };

        /// <inheritdoc />
        public override DispatchResult Execute(CilExecutionContext context, CilInstruction instruction)
        {
            var type = (ITypeDefOrRef) instruction.Operand;
            var memoryLayout = context.GetService<ICilRuntimeEnvironment>().ValueFactory.GetTypeMemoryLayout(type);
            Span<byte> zeroes = stackalloc byte[(int) memoryLayout.Size];
            zeroes.Fill(0);

            var address = context.ProgramState.Stack.Pop();

            switch (address)
            {
                case { IsKnown: false }:
                    throw new DispatchException("Cannot initialize a memory block pointed by an unknown memory address.");

                case PointerValue pointerValue:
                    pointerValue.WriteBytes(0, zeroes);
                    break;

                default:
                    return DispatchResult.InvalidProgram();
            }

            return base.Execute(context, instruction);
        }
        
    }
}