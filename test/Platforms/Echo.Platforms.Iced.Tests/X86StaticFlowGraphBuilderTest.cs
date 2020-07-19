using System.Linq;
using Echo.ControlFlow;
using Echo.ControlFlow.Construction;
using Echo.ControlFlow.Construction.Static;
using Iced.Intel;
using Xunit;

namespace Echo.Platforms.Iced.Tests
{
    public class X86StaticFlowGraphBuilderTest
    {
        private static ControlFlowGraph<Instruction> ConstructStaticFlowGraph(byte[] rawCode, long entrypoint)
        {
            var architecture = new X86Architecture();
            var instructionProvider = new X86DecoderInstructionProvider(architecture, rawCode, 32);

            var cfgBuilder = new StaticFlowGraphBuilder<Instruction>(
                instructionProvider,
                new X86StaticSuccessorResolver());

            return cfgBuilder.ConstructFlowGraph(entrypoint);
        }

        [Fact]
        public void NoBranchesShouldResultInSingleBlock()
        {
            var cfg = ConstructStaticFlowGraph(new byte[]
            {
                0x90, // nop
                0xc3 // ret
            }, 0);

            Assert.Single(cfg.Nodes);
        }

        [Fact]
        public void ConditionalBranchesShouldResultInBlocksWithConditionalEdges()
        {
            var cfg = ConstructStaticFlowGraph(new byte[]
            {
                0x55,                 // push ebp
                0x89, 0xe5,           // mov ebp, esp
                0x31, 0xc0,           // xor eax, eax
                0x31, 0xc9,           // xor ecx, ecx
                
                // loop:
                0x01, 0xc8,           // add eax, ecx
                0x83, 0xf8, 0x64,     // cmp eax, 0x64
                0x7c, 0x03,           // jl end_if
                
                0x83, 0xe8, 0x64,     // sub eax, 0x64
                
                // end_if:
                0x41,                 // inc ecx
                0x83, 0xf9, 0x0a,     // cmp ecx, 0xa
                0x7c, 0xf0,           // jl loop
                
                0x5d,                 // pop ebp
                0xc3                  // ret
            }, 0);
            
            Assert.Equal(new long[]
            {
                0x0, 0x7, 0xe, 0x11, 0x17,
            }.ToHashSet(), cfg.Nodes.Select(n => n.Offset).ToHashSet());
            Assert.Equal(cfg.Nodes[0x7], cfg.Nodes[0x0].FallThroughNeighbour);
            Assert.Equal(cfg.Nodes[0xE], cfg.Nodes[0x7].FallThroughNeighbour);
            Assert.Contains(cfg.Nodes[0x11], cfg.Nodes[0x7].ConditionalEdges.Select(e=>e.Target));
            Assert.Equal(cfg.Nodes[0x11], cfg.Nodes[0xE].FallThroughNeighbour);
            Assert.Contains(cfg.Nodes[0x7], cfg.Nodes[0x11].ConditionalEdges.Select(e=>e.Target));
        }

        [Fact]
        public void DisconnectedBlocksShouldSkipOverNotExecutableData()
        {
            var cfg = ConstructStaticFlowGraph(new byte[]
            {
                /* 0: */ 0xEB, 0x01,  // jmp    0x3
                /* 2: */ 0xFF,        // db     0xFF
                /* 3: */ 0xC3         // ret
            }, 0);
            
            Assert.Contains(cfg.Nodes, node => node.Offset == 0);
            Assert.DoesNotContain(cfg.Nodes, node => node.Offset == 2);
            Assert.Contains(cfg.Nodes, node => node.Offset == 3);
            
            Assert.Single(cfg.Nodes[0].Contents.Instructions);
            Assert.Single(cfg.Nodes[3].Contents.Instructions);
        }
    }
}