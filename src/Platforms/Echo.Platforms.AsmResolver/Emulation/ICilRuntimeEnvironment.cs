using AsmResolver.PE.DotNet.Cil;
using Echo.Core.Code;

namespace Echo.Platforms.AsmResolver.Emulation
{
    /// <summary>
    /// Provides members for describing an environment that a .NET virtual machine runs in.
    /// </summary>
    public interface ICilRuntimeEnvironment 
    {
        /// <summary>
        /// Gets the architecture description of the instructions to execute. 
        /// </summary>
        IInstructionSetArchitecture<CilInstruction> Architecture
        {
            get;
        }
        
        /// <summary>
        /// Gets a value indicating whether the virtual machine runs in 32-bit mode or in 64-bit mode.
        /// </summary>
        bool Is32Bit
        {
            get;
        }
    }
}