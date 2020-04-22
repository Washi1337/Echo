using AsmResolver.PE.DotNet.Cil;
using Echo.Core.Code;
using Echo.Platforms.AsmResolver.Emulation;

namespace Echo.Platforms.AsmResolver.Tests.Mock
{
    public sealed class MockCilRuntimeEnvironment : ICilRuntimeEnvironment
    {
        public MockCilRuntimeEnvironment()
        {
        }

        public MockCilRuntimeEnvironment(CilArchitecture architecture, bool is32Bit)
        {
            Architecture = architecture;
            Is32Bit = is32Bit;
        }
            
        public IInstructionSetArchitecture<CilInstruction> Architecture
        {
            get;
            set;
        }

        public bool Is32Bit
        {
            get;
            set;
        }
    }
}