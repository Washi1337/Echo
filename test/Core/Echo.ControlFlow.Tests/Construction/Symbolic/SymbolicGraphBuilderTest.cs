using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using Echo.ControlFlow.Construction;
using Echo.ControlFlow.Construction.Symbolic;
using Echo.ControlFlow.Serialization.Dot;
using Echo.Core.Emulation;
using Echo.Core.Graphing.Serialization.Dot;
using Echo.DataFlow;
using Echo.DataFlow.Emulation;
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

            var cfg = cfgBuilder.ConstructFlowGraph(entrypoint, knownBlockHeaders ?? Enumerable.Empty<long>());
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
            Assert.Equal(dfg.Nodes[0], dfg.Nodes[1].StackDependencies[0].First().Node);
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
            }, dfg.Nodes[3].StackDependencies.Select(dep => dep.First().Node));
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
            Assert.Equal(dfg.Nodes[0], dfg.Nodes[3].StackDependencies[0].First().Node);
            Assert.Single(dfg.Nodes[5].StackDependencies);
            Assert.Equal(dfg.Nodes[0], dfg.Nodes[5].StackDependencies[0].First().Node);
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
                new HashSet<DataFlowNode<DummyInstruction>> {dfg.Nodes[2], dfg.Nodes[4]},
                new HashSet<DataFlowNode<DummyInstruction>>(dfg.Nodes[5].StackDependencies[0].Select(s => s.Node)));
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
                new HashSet<DataFlowNode<DummyInstruction>> {dfg.Nodes[2], dfg.Nodes[4]},
                new HashSet<DataFlowNode<DummyInstruction>>(dfg.Nodes[6].StackDependencies[0].Select(s => s.Node)));
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
                InitialState = SymbolicProgramState<DummyInstruction>.Empty
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
            var argument = new ExternalDataSourceNode<DummyInstruction>(-1, "Argument 1");
            dfgBuilder.DataFlowGraph.Nodes.Add(argument);
            dfgBuilder.InitialState = new SymbolicProgramState<DummyInstruction>(0,
                ImmutableStack.Create(new SymbolicValue<DummyInstruction>(argument)));

            var cfgBuilder = new SymbolicFlowGraphBuilder<DummyInstruction>(
                DummyArchitecture.Instance,
                instructions,
                dfgBuilder);
            cfgBuilder.ConstructFlowGraph(0);
            var dfg = dfgBuilder.DataFlowGraph;

            Assert.Equal(new[] {argument}, dfg.Nodes[0].StackDependencies[0].GetNodes());
        }

        [Fact]
        public void PushingMultipleStackSlots()
        {
            var instructions = new[]
            {
                DummyInstruction.Push(0, 2),
                DummyInstruction.Pop(1, 1),
                DummyInstruction.Pop(2, 1),
                DummyInstruction.Ret(3),
            };
            
            var (cfg, dfg) = BuildFlowGraphs(instructions);
            
            Assert.Equal(1, dfg.Nodes[1].StackDependencies[0].First().SlotIndex);
            Assert.Equal(0, dfg.Nodes[2].StackDependencies[0].First().SlotIndex);
        }

        [Fact]
        public void BlockHeadersImpliedByInstructionsShouldAlwaysBeAdded()
        {
            var instructions = new[]
            {
                DummyInstruction.PushOffset(0, 10),
                DummyInstruction.Ret(1),
                
                DummyInstruction.Op(10, 0 ,0),
                DummyInstruction.Ret(11),
            };
            
            var (cfg, _) = BuildFlowGraphs(instructions);
            Assert.Contains(cfg.Nodes, n => n.Offset == 0);
            Assert.Contains(cfg.Nodes, n => n.Offset == 10);
            Assert.DoesNotContain(cfg.Nodes, n => n.Offset == 1);
            Assert.Empty(cfg.Nodes[0].GetOutgoingEdges());
            Assert.Empty(cfg.Nodes[10].GetIncomingEdges());
        }
    }
}