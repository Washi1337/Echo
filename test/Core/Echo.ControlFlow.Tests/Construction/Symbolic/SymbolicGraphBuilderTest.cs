using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Echo.ControlFlow.Construction;
using Echo.ControlFlow.Construction.Symbolic;
using Echo.Core.Emulation;
using Echo.DataFlow;
using Echo.DataFlow.Emulation;
using Echo.DataFlow.Values;
using Echo.Platforms.DummyPlatform.Code;
using Echo.Platforms.DummyPlatform.ControlFlow;
using Xunit;

namespace Echo.ControlFlow.Tests.Construction.Symbolic
{
    public class SymbolicGraphBuilderTest
    {
        private static (ControlFlowGraph<DummyInstruction> cfg, DataFlowGraph<DummyInstruction> dfg) BuildFlowGraphs(
            DummyInstruction[] instructions,
            long entrypoint = 0,
            IEnumerable<long> knownBlockHeaders = null)
        {
            var dfgBuilder = new DummyTransitionResolver();
            var cfgBuilder = new SymbolicFlowGraphBuilder<DummyInstruction>(
                DummyArchitecture.Instance,
                instructions,
                dfgBuilder);

            var cfg = cfgBuilder.ConstructFlowGraph(entrypoint, knownBlockHeaders ?? ImmutableArray<long>.Empty);
            return (cfg, dfgBuilder.DataFlowGraph);
        }

        [Fact]
        public void NonPoppingInstructionShouldHaveNoStackDependencies()
        {
            var instructions = new[]
            {
                DummyInstruction.Ret(0)
            };

            var (cfg, dfg) = BuildFlowGraphs(instructions);
            
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

            var (cfg, dfg) = BuildFlowGraphs(instructions);
            
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

            var (cfg, dfg) = BuildFlowGraphs(instructions);
            
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

            var (cfg, dfg) = BuildFlowGraphs(instructions);
            
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

            var (cfg, dfg) = BuildFlowGraphs(instructions);
        
            Assert.Single(dfg.Nodes[5].StackDependencies);
            Assert.Equal(
                new HashSet<IDataFlowNode> { dfg.Nodes[2], dfg.Nodes[4] },
                new HashSet<IDataFlowNode>(dfg.Nodes[5].StackDependencies[0].DataSources));
        }

        [Fact]
        public void JoiningPathsShouldMergeDependenciesAndPropagate()
        {
            var instructions = new[]
            {
                DummyInstruction.Push(0, 1),
                DummyInstruction.JmpCond(1, 4),
            
                DummyInstruction.Push(2, 1),
                DummyInstruction.Jmp(3, 5),
            
                DummyInstruction.Push(4, 1),
            
                DummyInstruction.Push(5, 1),
                DummyInstruction.Pop(6, 2),
                DummyInstruction.Ret(7)
            };

            var (cfg, dfg) = BuildFlowGraphs(instructions);
        
            Assert.Equal(
                new HashSet<IDataFlowNode> { dfg.Nodes[2], dfg.Nodes[4] },
                new HashSet<IDataFlowNode>(dfg.Nodes[6].StackDependencies[0].DataSources));
            Assert.Equal(new[]
            {
                dfg.Nodes[6]
            }, dfg.Nodes[2].GetDependants());
            Assert.Equal(new[]
            {
                dfg.Nodes[6]
            }, dfg.Nodes[4].GetDependants());
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

            Assert.Throws<StackImbalanceException>(() => BuildFlowGraphs(instructions));
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
            
            var (cfg, dfg) = BuildFlowGraphs(instructions);
            
            Assert.Equal(new[]
            {
                0L, 100L
            }, cfg.Nodes.Select(n => n.Offset));
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
            
            var (cfg, dfg) = BuildFlowGraphs(instructions, 100);
            
            Assert.Equal(new[]
            {
                0L, 100L
            }, cfg.Nodes.Select(n => n.Offset));
        }

        [Fact]
        public void EntryPointPopWithInitialStateEmptyShouldThrow()
        {
            var instructions = new[]
            {
                DummyInstruction.Pop(0, 1),
                DummyInstruction.Ret(1),
            };
            
            var dfgBuilder = new DummyTransitionResolver
            {
                InitialState = new SymbolicProgramState<DummyInstruction>()
            };

            var cfgBuilder = new SymbolicFlowGraphBuilder<DummyInstruction>(
                DummyArchitecture.Instance,
                instructions,
                dfgBuilder);

            Assert.Throws<StackImbalanceException>(() => cfgBuilder.ConstructFlowGraph(0));
        }

        [Fact]
        public void EntryPointPopWithSingleItemOnStackShouldAddDependencyToExternalSource()
        {
            var instructions = new[]
            {
                DummyInstruction.Pop(0, 1),
                DummyInstruction.Ret(1),
            };

            var dfgBuilder = new DummyTransitionResolver();
            var argument = new ExternalDataSource<DummyInstruction>(-1, "Argument 1");
            dfgBuilder.DataFlowGraph.Nodes.Add(argument);
            dfgBuilder.InitialState = new SymbolicProgramState<DummyInstruction>();
            dfgBuilder.InitialState.Stack.Push(new SymbolicValue<DummyInstruction>(argument));

            var cfgBuilder = new SymbolicFlowGraphBuilder<DummyInstruction>(
                DummyArchitecture.Instance,
                instructions,
                dfgBuilder);
            cfgBuilder.ConstructFlowGraph(0);
            var dfg = dfgBuilder.DataFlowGraph;

            Assert.Equal(new[] {argument}, dfg.Nodes[0].StackDependencies[0].DataSources);
        }

    }
}