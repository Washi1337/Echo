using System.Linq;
using Echo.ControlFlow;
using Echo.ControlFlow.Construction;
using Echo.ControlFlow.Construction.Symbolic;
using Echo.DataFlow;
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
            var instructionProvider = new X86DecoderInstructionProvider(_architecture, rawCode, 32);
            var dfgBuilder = new X86StateTransitioner(_architecture);
            
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
            Assert.True(dfg.Nodes[0x5].VariableDependencies.ContainsVariable(eax));
            Assert.Contains(dfg.Nodes[0], dfg.Nodes[0x5].VariableDependencies[eax].GetNodes());
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
            Assert.True(dfg.Nodes[0x3].VariableDependencies.ContainsVariable(of));
            Assert.Contains(dfg.Nodes[0], dfg.Nodes[0x3].VariableDependencies[of].GetNodes());
        }

        [Fact]
        public void PushEspShouldOnlyReturnEspOnceForReadAndWrittenVariables()
        {
            var (cfg, dfg) = ConstructSymbolicFlowGraph(new byte[]
            {
                /* 0: */ 0x54, // push esp
                /* 1: */ 0x5C, // pop esp
                /* 2: */ 0xC3  // ret
            }, 0);

            var dependency = dfg.Nodes[1].VariableDependencies
                .FirstOrDefault(dependency => dependency.Variable.Name == "ESP");
            Assert.NotNull(dependency);
            Assert.Contains(dfg.Nodes[0], dependency.GetNodes());
        }
    }
}