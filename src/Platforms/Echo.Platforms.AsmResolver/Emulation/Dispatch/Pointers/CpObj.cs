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
    /// Provides a handler for instructions with the <see cref="CilOpCodes.Cpobj"/> operation code.
    /// </summary>
    public class CpObj : FallThroughOpCodeHandler
    {
        /// <inheritdoc />
        public override IReadOnlyCollection<CilCode> SupportedOpCodes => new[]
        {
            CilCode.Cpobj
        };

        /// <inheritdoc />
        public override DispatchResult Execute(CilExecutionContext context, CilInstruction instruction)
        {
            var type = (ITypeDefOrRef) instruction.Operand;
            var memoryLayout = context.GetService<ICilRuntimeEnvironment>().ValueFactory.GetTypeMemoryLayout(type);

            var sourceAddress = context.ProgramState.Stack.Pop();
            var destinationAddress = context.ProgramState.Stack.Pop();

            switch (destinationAddress)
            {
                case { IsKnown: false }:
                    throw new DispatchException("Cannot write to a memory block pointed by an unknown memory address.");

                case PointerValue destinationPointer:
                    Span<byte> memory = stackalloc byte[(int) memoryLayout.Size];
                    Span<byte> knownBitmask = stackalloc byte[(int) memoryLayout.Size];

                    // Read data from source pointer.
                    if (sourceAddress is PointerValue {IsKnown: true} sourcePointer)
                        sourcePointer.ReadBytes(0, memory, knownBitmask);
                    
                    // Write data from destination pointer.
                    destinationPointer.WriteBytes(0, memory, knownBitmask);
                    break;

                default:
                    return DispatchResult.InvalidProgram();
            }

            return base.Execute(context, instruction);
        }
        
    }
}