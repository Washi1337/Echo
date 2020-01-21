using System.Collections.Generic;
using System.Linq;
using Echo.ControlFlow.Serialization.StructuredFlow;
using Echo.ControlFlow.Specialized;
using Echo.ControlFlow.Specialized.Blocks;
using Echo.ControlFlow.Tests.Analysis;
using Echo.Platforms.DummyPlatform.Code;
using Xunit;

namespace Echo.ControlFlow.Tests.Serialization.StructuredFlow
{
    public class StructuredFlowGeneratorTest
    {
        private static void AssertOrderIsOneOf(ControlFlowGraph<DummyInstruction> cfg, params IEnumerable<Node<BasicBlock<DummyInstruction>>>[] expectedOrders)
        {
            var generator = new StructuredFlowGenerator<DummyInstruction>();
            var block = generator.Generate(cfg);

            var generatedEntries = block.GetAllBlocks()
                .Select(x => x.Instructions[0])
                .ToArray();

            var expectedEntries = expectedOrders
                .Select(order => order.Select(n => n.Contents.Instructions[0]))
                .ToArray();

            Assert.Contains(generatedEntries, expectedEntries);
        }
        
        [Fact]
        public void SingleNode()
        {
            var cfg = new ControlFlowGraph<DummyInstruction>();
            
            var block1 = new[]
            {
                DummyInstruction.Ret(0),
            };
            var node = new BasicBlockNode<DummyInstruction>(block1);
            cfg.Nodes.Add(node);
            cfg.Entrypoint = node;
            
            var generator = new StructuredFlowGenerator<DummyInstruction>();
            var block = generator.Generate(cfg);

            var basicBlocks = block.GetAllBlocks().ToArray();
            Assert.Single(basicBlocks);
            Assert.Equal(block1, basicBlocks[0].Instructions);
        }

        [Fact]
        public void If()
        {
            var cfg = TestGraphs.CreateIf();
            var expectedOrder = cfg.Nodes
                .OrderBy(n => n.Contents.Instructions[0].Offset);
            
            AssertOrderIsOneOf(cfg, expectedOrder);
        }

        [Fact]
        public void IfElse()
        {
            var cfg = TestGraphs.CreateIfElse();
            var expectedOrder = cfg.Nodes
                .OrderBy(n => n.Contents.Instructions[0].Offset);

            AssertOrderIsOneOf(cfg, expectedOrder);
        }

        [Fact]
        public void IfElseNested()
        {
            var cfg = TestGraphs.CreateIfElseNested();
            var expectedOrder = cfg.Nodes
                .OrderBy(n => n.Contents.Instructions[0].Offset);

            AssertOrderIsOneOf(cfg, expectedOrder);
        }

        [Fact]
        public void Loop()
        {
            var cfg = TestGraphs.CreateLoop();
            var orderdNodes = cfg.Nodes
                .OrderBy(n => n.Contents.Instructions[0].Offset)
                .ToArray();
            
            AssertOrderIsOneOf(cfg, 
                new[]
                {
                    orderdNodes[0],
                    orderdNodes[1],
                    orderdNodes[2],
                    orderdNodes[3],
                }, 
                new[]
                {
                    orderdNodes[0],
                    orderdNodes[2],
                    orderdNodes[1],
                    orderdNodes[3],
                });
        }

        [Fact]
        public void Switch()
        {
            var cfg = TestGraphs.CreateSwitch();
            var orderedEntries = cfg.Nodes
                .Select(n => n.Contents.Instructions[0])
                .OrderBy(i => i.Offset)
                .ToArray();
            
            var generator = new StructuredFlowGenerator<DummyInstruction>();
            var block = generator.Generate(cfg);
            
            var generatedEntries = block.GetAllBlocks()
                .Select(x => x.Instructions[0])
                .ToList();

            Assert.All(generatedEntries, i => Assert.True(
                generatedEntries.IndexOf(i) <= generatedEntries.IndexOf(orderedEntries[5])));
            
            Assert.True(generatedEntries.IndexOf(orderedEntries[1]) < generatedEntries.IndexOf(orderedEntries[2]));
            Assert.True(generatedEntries.IndexOf(orderedEntries[1]) < generatedEntries.IndexOf(orderedEntries[3]));
            Assert.True(generatedEntries.IndexOf(orderedEntries[1]) < generatedEntries.IndexOf(orderedEntries[4]));
            
        }
        
    }
}