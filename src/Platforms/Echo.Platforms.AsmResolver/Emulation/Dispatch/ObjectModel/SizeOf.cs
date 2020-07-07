using System.Collections.Generic;
using AsmResolver.DotNet;
using AsmResolver.PE.DotNet.Cil;
using Echo.Concrete.Emulation.Dispatch;
using Echo.Platforms.AsmResolver.Emulation.Values.Cli;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch.ObjectModel
{
    /// <summary>
    /// //
    /// </summary>
    public sealed class SizeOf : FallThroughOpCodeHandler
    {
        /// <inheritdoc />
        public override IReadOnlyCollection<CilCode> SupportedOpCodes => new []
        {
            CilCode.Sizeof
        };

        /// <inheritdoc />
        public override DispatchResult Execute(Concrete.Emulation.ExecutionContext context, CilInstruction instruction)
        {
            var type = instruction.Operand as ITypeDefOrRef;
            var environment = context.GetService<ICilRuntimeEnvironment>();
            var allocator = environment.MemoryAllocator;
            var layout = allocator.GetTypeMemoryLayout(type);
            context.ProgramState.Stack.Push(new I4Value((int) layout.Size));

            return DispatchResult.Success();
        }
    }
}
