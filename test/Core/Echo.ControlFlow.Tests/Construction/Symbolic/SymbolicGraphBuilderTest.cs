using System.Collections.Generic;
using System.Linq;
using Echo.ControlFlow.Construction.Symbolic;
using Echo.Core.Emulation;
using Echo.DataFlow;
using Echo.Platforms.DummyPlatform.Code;
using Echo.Platforms.DummyPlatform.ControlFlow;
using Xunit;

namespace Echo.ControlFlow.Tests.Construction.Symbolic
{
    public class SymbolicGraphBuilderTest
    {
        private readonly SymbolicFlowGraphBuilder<DummyInstruction> _cfgBuilder;
        private readonly DummyTransitionResolver _dfgBuilder;

        public SymbolicGraphBuilderTest()
        {
            _dfgBuilder = new DummyTransitionResolver();
            _cfgBuilder = new SymbolicFlowGraphBuilder<DummyInstruction>(DummyArchitecture.Instance, _dfgBuilder);
        }

        [Fact]
        public void NonPoppingInstructionShouldHaveNoStackDependencies()
        {
            var instructions = new[]
            {
                DummyInstruction.Ret(0)
            };

            var cfg = _cfgBuilder.ConstructFlowGraph(instructions, 0);
            var dfg = _dfgBuilder.DataFlowGraph;
            
            Assert.Empty(dfg.Nodes[0].StackDependencies);
        }
        
        [Fact]
        public void SinglePopShouldHaveSingleStackDependency()
        {
            var instructions = new[]
            {
                DummyInstruction.Push(0, 1),
                DummyInstruction.Pop(1, 1),
                DummyInstruction.Ret(2)
            };

            var cfg = _cfgBuilder.ConstructFlowGraph(instructions, 0);
            var dfg = _dfgBuilder.DataFlowGraph;
            
            Assert.Single(dfg.Nodes[1].StackDependencies);
            Assert.Equal(dfg.Nodes[0], dfg.Nodes[1].StackDependencies[0].DataSources.First());
            Assert.Equal(new[]{dfg.Nodes[1]}, dfg.Nodes[0].GetDependants());
        }
        
        [Fact]
        public void MultiplePopsShouldHaveMultipleStackDependencies()
        {
            var instructions = new[]
            {
                DummyInstruction.Push(0, 1),
                DummyInstruction.Push(1, 1),
                DummyInstruction.Push(2, 1),
                DummyInstruction.Pop(3, 3),
                DummyInstruction.Ret(4)
            };

            var cfg = _cfgBuilder.ConstructFlowGraph(instructions, 0);
            var dfg = _dfgBuilder.DataFlowGraph;
            
            Assert.Equal(3, dfg.Nodes[3].StackDependencies.Count);
            Assert.Equal(new[]
            {
                dfg.Nodes[0], dfg.Nodes[1], dfg.Nodes[2],
            }, dfg.Nodes[3].StackDependencies.Select(dep => dep.DataSources.First()));
        }
        
        [Fact]
        public void BranchingPathsShouldCopyDependency()
        {
            var instructions = new[]
            {
                DummyInstruction.Push(0, 1),
                
                DummyInstruction.Push(1, 1),
                DummyInstruction.JmpCond(2, 5),
                
                DummyInstruction.Pop(3, 1),
                DummyInstruction.Jmp(4, 6),
                
                DummyInstruction.Pop(5, 1),
                
                DummyInstruction.Ret(6)
            };

            var cfg = _cfgBuilder.ConstructFlowGraph(instructions, 0);
            var dfg = _dfgBuilder.DataFlowGraph;
            
            Assert.Single(dfg.Nodes[3].StackDependencies);
            Assert.Equal(dfg.Nodes[0], dfg.Nodes[3].StackDependencies[0].DataSources.First());
            Assert.Single(dfg.Nodes[5].StackDependencies);
            Assert.Equal(dfg.Nodes[0], dfg.Nodes[5].StackDependencies[0].DataSources.First());
        }

        [Fact]
        public void JoiningPathsShouldMergeDependencies()
        {
            var instructions = new[]
            {
                DummyInstruction.Push(0, 1),
                DummyInstruction.JmpCond(1, 4),
            
                DummyInstruction.Push(2, 1),
                DummyInstruction.Jmp(3, 5),
            
                DummyInstruction.Push(4, 1),
            
                DummyInstruction.Pop(5, 1),
                DummyInstruction.Ret(6)
            };

            var cfg = _cfgBuilder.ConstructFlowGraph(instructions, 0);
            var dfg = _dfgBuilder.DataFlowGraph;
        
            Assert.Single(dfg.Nodes[5].StackDependencies);
            Assert.Equal(
                new HashSet<IDataFlowNode> { dfg.Nodes[2], dfg.Nodes[4] },
                new HashSet<IDataFlowNode>(dfg.Nodes[5].StackDependencies[0].DataSources));
        }

        [Fact]
        public void JoiningPathsWithDifferentStackHeightsShouldThrow()
        {
            var instructions = new[]
            {
                DummyInstruction.Push(0, 1),
                DummyInstruction.JmpCond(1, 4),
            
                DummyInstruction.Push(2, 2),
                DummyInstruction.Jmp(3, 5),
            
                DummyInstruction.Push(4, 1),
            
                DummyInstruction.Pop(5, 1),
                DummyInstruction.Ret(6)
            };

            Assert.Throws<StackImbalanceException>(() => _cfgBuilder.ConstructFlowGraph(instructions, 0));
        }
    }
}