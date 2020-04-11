using AsmResolver.PE.DotNet.Cil;
using Echo.Concrete.Emulation;
using Echo.Concrete.Emulation.Dispatch;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch
{
    /// <summary>
    /// Represents an operation code handler executing an instruction in a virtual machine.
    /// </summary>
    public interface ICilOpCodeHandler
    {
        /// <summary>
        /// Executes an instruction in the provided context.
        /// </summary>
        /// <param name="context">The context to execute the instruction in.</param>
        /// <param name="instruction">The instruction to execute.</param>
        /// <returns>A result.</returns>
        DispatchResult Execute(ExecutionContext context, CilInstruction instruction);
    }
}