using System.IO;
using Echo.ControlFlow;
using Echo.ControlFlow.Construction;
using Echo.ControlFlow.Construction.Symbolic;
using Echo.Core.Graphing.Serialization.Dot;
using Echo.DataFlow;
using Echo.DataFlow.Serialization.Dot;
using Iced.Intel;
using Xunit;

namespace Echo.Platforms.Iced.Tests
{
    public class X86StateTransitionResolverTest
    {
        private readonly X86Architecture _architecture;

        public X86StateTransitionResolverTest()
        {
            _architecture = new X86Architecture();
        }

        private (ControlFlowGraph<Instruction> Cfg, DataFlowGraph<Instruction> Dfg) ConstructSymbolicFlowGraph(
            byte[] rawCode, long entrypoint)
        {
            var decoder = Decoder.Create(32, new ByteArrayCodeReader(rawCode));
            
            var instructionProvider = new X86DecoderInstructionProvider(_architecture, decoder);
            var dfgBuilder = new X86StateTransitionResolver(_architecture);
            
            var cfgBuilder = new SymbolicFlowGraphBuilder<Instruction>(
                instructionProvider,
                dfgBuilder);

            return (cfgBuilder.ConstructFlowGraph(entrypoint), dfgBuilder.DataFlowGraph);
        }

        [Fact]
        public void TestGeneralPurposeRegisterDependency()
        {
            var (cfg, dfg) = ConstructSymbolicFlowGraph(new byte[]
            {
                0xB8, 0x01, 0x00, 0x00, 0x00,    // mov eax, 1
                0x83, 0xC0, 0x02,                // add eax, 2
                0xC3                             // ret
            }, 0);

            var eax = _architecture.GetRegister(Register.EAX);
            Assert.True(dfg.Nodes[0x5].VariableDependencies.ContainsKey(eax));
            Assert.Contains(dfg.Nodes[0], dfg.Nodes[0x5].VariableDependencies[eax].DataSources);
        }

        [Fact]
        public void TestFlagRegisterDependency()
        {
            var (cfg, dfg) = ConstructSymbolicFlowGraph(new byte[]
            { 
                0x83, 0xF8, 0x64,        // cmp eax, 100
                0x7C, 0x00,              // jl +0
                0xC3                     // ret
            }, 0);

            var of = _architecture.GetFlag(RflagsBits.OF);
            Assert.True(dfg.Nodes[0x3].VariableDependencies.ContainsKey(of));
            Assert.Contains(dfg.Nodes[0], dfg.Nodes[0x3].VariableDependencies[of].DataSources);
        }

    }
}