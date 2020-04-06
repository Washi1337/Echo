using System.Linq;
using Echo.ControlFlow.Construction;
using Echo.ControlFlow.Construction.Static;
using Echo.Platforms.DummyPlatform.Code;
using Xunit;

namespace Echo.ControlFlow.Tests.Construction.Static
{
    public class StaticGraphBuilderTest
    {
        private readonly StaticFlowGraphBuilder<DummyInstruction> _builder;

        public StaticGraphBuilderTest()
        {
            _builder = new StaticFlowGraphBuilder<DummyInstruction>(
                DummyArchitecture.Instance,
                DummyArchitecture.Instance.SuccessorResolver);
        }
        
        [Fact]
        public void SingleBlock()
        {
            var instructions = new[]
            {
                DummyInstruction.Push(0, 1),
                DummyInstruction.Push(1, 1),
                DummyInstruction.Op(2, 2, 1),
                DummyInstruction.Push(3, 1),
                DummyInstruction.Push(4, 1),
                DummyInstruction.Ret(5)
            };
            
            var graph = _builder.ConstructFlowGraph(instructions, 0);
            
            Assert.Single(graph.Nodes);
            Assert.Equal(instructions, graph.Nodes.First().Contents.Instructions);
            Assert.Equal(graph.Nodes.First(), graph.Entrypoint);
        }

        [Fact]
        public void JumpForwardToFarBlock()
        {
            var instructions = new[]
            {
                DummyInstruction.Push(0, 1),
                DummyInstruction.Jmp(1, 100),
                
                DummyInstruction.Pop(100, 1),
                DummyInstruction.Ret(101),
            };
            
            var graph = _builder.ConstructFlowGraph(instructions, 0);
            Assert.Equal(new[]
            {
                0L, 100L
            }, graph.Nodes.Select(n => n.Offset));
        }

        [Fact]
        public void JumpBackToFarBlock()
        {
            var instructions = new[]
            {
                DummyInstruction.Pop(0, 1),
                DummyInstruction.Ret(1),
                
                DummyInstruction.Push(100, 1),
                DummyInstruction.Jmp(101, 0),
            };
            
            var graph = _builder.ConstructFlowGraph(instructions, 100);
            Assert.Equal(new[]
            {
                0L, 100L
            }, graph.Nodes.Select(n => n.Offset));
        }

        [Fact]
        public void If()
        {
            var instructions = new[]
            {
                // Entrypoint
                DummyInstruction.Push(0, 1),
                DummyInstruction.JmpCond(1, 5),
                
                // True
                DummyInstruction.Op(2, 0, 0),
                DummyInstruction.Op(3, 0, 0),
                DummyInstruction.Jmp(4, 7),
                
                // False
                DummyInstruction.Op(5, 0, 0),
                DummyInstruction.Op(6, 0, 0),
                
                // Endif
                DummyInstruction.Ret(7)
            };
            
            var graph = _builder.ConstructFlowGraph(instructions, 0);

            Assert.Equal(4, graph.Nodes.Count);
            Assert.Single(graph.Entrypoint.ConditionalEdges);
            Assert.NotNull(graph.Entrypoint.FallThroughEdge);
            Assert.Equal(
                graph.Entrypoint.FallThroughNeighbour.FallThroughNeighbour, 
                graph.Entrypoint.ConditionalEdges.First().Target.FallThroughNeighbour);
        }

        [Fact]
        public void Loop()
        {
            var instructions = new[]
            {
                // Entrypoint
                DummyInstruction.Push(0, 1),
                DummyInstruction.Pop(1, 1),
                DummyInstruction.Jmp(2, 7),
                
                // Loop body
                DummyInstruction.Push(3, 1),
                DummyInstruction.Push(4, 1),
                DummyInstruction.Op(5, 2, 1),
                DummyInstruction.Pop(6, 1),
                
                // Loop header
                DummyInstruction.Push(7, 1),
                DummyInstruction.Push(8, 1),
                DummyInstruction.Op(9, 2, 1),
                DummyInstruction.JmpCond(10, 3),
                
                // Exit
                DummyInstruction.Op(11, 0, 0),
                DummyInstruction.Ret(12)
            };
            
            var graph = _builder.ConstructFlowGraph(instructions, 0);
            
            Assert.Equal(4, graph.Nodes.Count);
            
            // Entrypoint.
            Assert.NotNull(graph.Entrypoint.FallThroughNeighbour);
            Assert.Empty(graph.Entrypoint.ConditionalEdges);
            
            // Loop header
            var loopHeader = graph.Entrypoint.FallThroughNeighbour;
            Assert.NotNull(loopHeader.FallThroughEdge);
            Assert.Single(loopHeader.ConditionalEdges);
            
            // Loop body
            var loopBody = loopHeader.ConditionalEdges.First().Target;
            Assert.Equal(loopHeader, loopBody.FallThroughNeighbour);
            Assert.Empty(loopBody.ConditionalEdges);
            
            // Exit
            var exit = loopHeader.FallThroughNeighbour;
            Assert.Empty(exit.GetOutgoingEdges());
        }

        [Fact]
        public void UnreachableNodesShouldNotBeIncluded()
        {
            var instructions = new[]
            {
                DummyInstruction.Ret(0),
                
                DummyInstruction.Ret(10),
            };
            
            var graph = _builder.ConstructFlowGraph(instructions, 0);
            Assert.Contains(graph.Nodes, n => n.Offset == 0);
            Assert.DoesNotContain(graph.Nodes, n => n.Offset == 10);
        }

        [Fact]
        public void UnreachableNodesShouldBeIncludedIfKnownBlockHeader()
        {
            var instructions = new[]
            {
                DummyInstruction.Ret(0),
                
                DummyInstruction.Ret(10),
            };
            
            var graph = _builder.ConstructFlowGraph(instructions, 0, new long[] {10});
            Assert.Contains(graph.Nodes, n => n.Offset == 0);
            Assert.Contains(graph.Nodes, n => n.Offset == 10);
        }

        [Fact]
        public void ExplicitBlockHeaderShouldAlwaysBeAdded()
        {
            var instructions = new[]
            {
                DummyInstruction.Op(0, 0 ,0),
                DummyInstruction.Op(1, 0 ,0),
                DummyInstruction.Ret(2),
            };
            
            var graph = _builder.ConstructFlowGraph(instructions, 0, new long[] {1});
            Assert.Contains(graph.Nodes, n => n.Offset == 0);
            Assert.Contains(graph.Nodes, n => n.Offset == 1);
        }
    }
}