using System.IO;
using System.Linq;
using Echo.ControlFlow.Serialization.Dot;
using Echo.Core.Code;
using Echo.Platforms.DummyPlatform.Code;
using Echo.Platforms.DummyPlatform.ControlFlow;
using Xunit;

namespace Echo.ControlFlow.Tests.Construction
{
    public class StaticGraphBuilderTest
    {
        [Fact]
        public void SingleBlock()
        {
            var instructions = new[]
            {
                DummyInstruction.Op(0, 0, 1),
                DummyInstruction.Op(1, 0, 1),
                DummyInstruction.Op(2, 2, 1),
                DummyInstruction.Op(3, 0, 1),
                DummyInstruction.Op(4, 0, 1),
                DummyInstruction.Ret(5)
            };
            
            var list = new InstructionList<DummyInstruction>(instructions);
            var builder = new DummyStaticGraphBuilder(list);
            var graph = builder.ConstructFlowGraph(0);
            
            Assert.Single(graph.Nodes);
            Assert.Equal(instructions, graph.Nodes.First().Instructions);
            Assert.Equal(graph.Nodes.First(), graph.Entrypoint);
        }
        
        [Fact]
        public void If()
        {
            var instructions = new[]
            {
                DummyInstruction.Op(0, 0, 1),
                DummyInstruction.JmpCond(1, 5),
                
                DummyInstruction.Op(2, 0, 0),
                DummyInstruction.Op(3, 0, 0),
                DummyInstruction.Jmp(4, 7),
                
                DummyInstruction.Op(5, 0, 0),
                DummyInstruction.Op(6, 0, 0),
                
                DummyInstruction.Ret(7)
            };
            
            var list = new InstructionList<DummyInstruction>(instructions);
            var builder = new DummyStaticGraphBuilder(list);
            var graph = builder.ConstructFlowGraph(0);


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
                DummyInstruction.Op(0, 0, 1),
                DummyInstruction.Op(1, 1, 0),
                DummyInstruction.Jmp(2, 7),
                
                // Loop body
                DummyInstruction.Op(3, 0, 1),
                DummyInstruction.Op(4, 0, 1),
                DummyInstruction.Op(5, 2, 1),
                DummyInstruction.Op(6, 1, 0),
                
                // Loop header
                DummyInstruction.Op(7, 0, 1),
                DummyInstruction.Op(8, 0, 1),
                DummyInstruction.Op(9, 2, 1),
                DummyInstruction.JmpCond(10, 3),
                
                // Exit
                DummyInstruction.Op(11, 0, 0),
                DummyInstruction.Ret(12)
            };
            
            var list = new InstructionList<DummyInstruction>(instructions);
            var builder = new DummyStaticGraphBuilder(list);
            var graph = builder.ConstructFlowGraph(0);
            
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
    }
}