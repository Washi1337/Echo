using System.Collections.Generic;
using AsmResolver.DotNet.Signatures.Types;
using AsmResolver.PE.DotNet.Cil;
using Echo.Concrete.Emulation;
using Echo.Concrete.Emulation.Dispatch;
using Echo.Platforms.AsmResolver.Emulation.Values.Cli;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch.Pointers
{
    /// <summary>
    /// Provides a handler for instructions with the <see cref="CilOpCodes.Localloc"/> operation code.
    /// </summary>
    public class Localloc : FallThroughOpCodeHandler
    {
        /// <inheritdoc />
        public override IReadOnlyCollection<CilCode> SupportedOpCodes => new[]
        {
            CilCode.Localloc
        };

        /// <inheritdoc />
        public override DispatchResult Execute(ExecutionContext context, CilInstruction instruction)
        {
            var environment = context.GetService<ICilRuntimeEnvironment>();
            var stack = context.ProgramState.Stack;

            var sizeValue = (ICliValue) stack.Pop();
            if (!sizeValue.IsKnown)
                return DispatchResult.InvalidProgram();

            int size = sizeValue.InterpretAsI4().I32;
            var memoryValue = environment.ValueFactory.AllocateMemory(size, true);
            stack.Push(environment.CliMarshaller.ToCliValue(memoryValue,
                new PointerTypeSignature(environment.Module.CorLibTypeFactory.Byte)));
            
            return base.Execute(context, instruction);
        }
        
    }
}