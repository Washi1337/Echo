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

        [Fact]
        public void RecursiveTraversalOfConditionalEdgeShouldTraverseBothPaths()
        {
            // https://github.com/Washi1337/Echo/issues/38
            
            var cfg = ConstructStaticFlowGraph(new byte[]
            {
                /* 0x0 */     0x89, 0xE0,                           // mov eax,esp
                /* 0x2 */     0x90,                                 // nop
                /* 0x3 */     0x53,                                 // push ebx
                /* 0x4 */     0x57,                                 // push edi
                /* 0x5 */     0x56,                                 // push esi
                /* 0x6 */     0x29, 0xE0,                           // sub eax,esp
                /* 0x8 */     0x83, 0xF8, 0x18,                     // cmp eax,18h
                /* 0xB */     0x74, 0x07,                           // je short 00000014h
                /* 0xD */     0x8B, 0x44, 0x24, 0x10,               // mov eax,[esp+10h]
                /* 0x11 */    0x50,                                 // push eax
                /* 0x12 */    0xEB, 0x01,                           // jmp short 00000015h
                /* 0x14 */    0x51,                                 // push ecx
                /* 0x15 */    0x58,                                 // pop eax
                /* 0x16 */    0xF7, 0xD8,                           // neg eax
                /* 0x18 */    0xB9, 0x59, 0x42, 0x9B, 0x72,         // mov ecx,729B4259h
                /* 0x1D */    0x81, 0xC1, 0x43, 0x47, 0xD9, 0x8E,   // add ecx,8ED94743h
                /* 0x23 */    0x01, 0xC8,                           // add eax,ecx
                /* 0x25 */    0x81, 0xC0, 0x72, 0x5A, 0xE2, 0x10,   // add eax,10E25A72h
                /* 0x2B */    0x5E,                                 // pop esi
                /* 0x2C */    0x5F,                                 // pop edi
                /* 0x2D */    0x5B,                                 // pop ebx
                /* 0x2E */    0xC3,                                 // ret
            }, 0);
            
            Assert.Equal(new long[]
            {
                0x0, 0xD, 0x14, 0x15,
            }.ToHashSet(), cfg.Nodes.Select(n => n.Offset).ToHashSet());

            Assert.Equal(cfg.Nodes[0xD], cfg.Nodes[0].FallThroughNeighbour);
            Assert.Equal(cfg.Nodes[0x15], cfg.Nodes[0xD].FallThroughNeighbour);
            Assert.Equal(cfg.Nodes[0x15], cfg.Nodes[0x14].FallThroughNeighbour);
            Assert.Contains(
                cfg.Nodes[0x0].ConditionalEdges
                    .Select(e => e.Target),
                node => node.Offset == 0x14);
        }
    }
}