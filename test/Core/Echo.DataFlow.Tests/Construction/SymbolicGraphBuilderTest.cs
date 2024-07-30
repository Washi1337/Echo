using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Echo.ControlFlow;
using Echo.ControlFlow.Construction;
using Echo.DataFlow.Construction;
using Echo.DataFlow.Emulation;
using Echo.Platforms.DummyPlatform.Code;
using Echo.Platforms.DummyPlatform.ControlFlow;
using Xunit;

namespace Echo.DataFlow.Tests.Construction
{
    public class SymbolicGraphBuilderTest
    {
        private static (ControlFlowGraph<DummyInstruction> cfg, DataFlowGraph<DummyInstruction> dfg) BuildFlowGraphs(
            DummyInstruction[] instructions,
            long entrypoint = 0,
            IEnumerable<long> knownBlockHeaders = null)
        {
            var dfgBuilder = new DummyTransitioner();
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
            var offsetMap = dfg.Nodes.CreateOffsetMap();
            
            Assert.Empty(offsetMap[0].StackDependencies);
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
            var offsetMap = dfg.Nodes.CreateOffsetMap();
            
            Assert.Single(offsetMap[1].StackDependencies);
            Assert.Equal(offsetMap[0], offsetMap[1].StackDependencies[0].First().Node);
            Assert.Equal(new[]{offsetMap[1]}, offsetMap[0].GetDependants());
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
            var offsetMap = dfg.Nodes.CreateOffsetMap();
            
            Assert.Equal(3, offsetMap[3].StackDependencies.Count);
            Assert.Equal(new[]
            {
                offsetMap[0], offsetMap[1], offsetMap[2],
            }, offsetMap[3].StackDependencies.Select(dep => dep.First().Node));
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
            var offsetMap = dfg.Nodes.CreateOffsetMap();
            
            Assert.Single(offsetMap[3].StackDependencies);
            Assert.Equal(offsetMap[0], offsetMap[3].StackDependencies[0].First().Node);
            Assert.Single(offsetMap[5].StackDependencies);
            Assert.Equal(offsetMap[0], offsetMap[5].StackDependencies[0].First().Node);
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
            var offsetMap = dfg.Nodes.CreateOffsetMap();
        
            Assert.Single(offsetMap[5].StackDependencies);
            Assert.Equal(
                new HashSet<DataFlowNode<DummyInstruction>> {offsetMap[2], offsetMap[4]},
                new HashSet<DataFlowNode<DummyInstruction>>(offsetMap[5].StackDependencies[0].Select(s => s.Node)));
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
            var offsetMap = dfg.Nodes.CreateOffsetMap();

            Assert.Equal(
                new HashSet<DataFlowNode<DummyInstruction>> {offsetMap[2], offsetMap[4]},
                new HashSet<DataFlowNode<DummyInstruction>>(offsetMap[6].StackDependencies[0].Select(s => s.Node)));
            Assert.Equal(new[]
            {
                offsetMap[6]
            }, offsetMap[2].GetDependants());
            Assert.Equal(new[]
            {
                offsetMap[6]
            }, offsetMap[4].GetDependants());
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
            
            var dfgBuilder = new DummyTransitioner
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

            var dfgBuilder = new DummyTransitioner();
            var argument = new ExternalDataSourceNode<DummyInstruction>("Argument 1");
            dfgBuilder.DataFlowGraph.Nodes.Add(argument);
            dfgBuilder.InitialState = new SymbolicProgramState<DummyInstruction>(0,
                ImmutableStack.Create(SymbolicValue<DummyInstruction>.CreateStackValue(argument)));

            var cfgBuilder = new SymbolicFlowGraphBuilder<DummyInstruction>(
                DummyArchitecture.Instance,
                instructions,
                dfgBuilder);
            cfgBuilder.ConstructFlowGraph(0);
            
            var dfg = dfgBuilder.DataFlowGraph;
            var offsetMap = dfg.Nodes.CreateOffsetMap();

            Assert.Equal(new[] {argument}, offsetMap[0].StackDependencies[0].GetNodes());
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
            var offsetMap = dfg.Nodes.CreateOffsetMap();
            
            Assert.Equal(1, offsetMap[1].StackDependencies[0].First().SlotIndex);
            Assert.Equal(0, offsetMap[2].StackDependencies[0].First().SlotIndex);
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
            Assert.Empty(cfg.Nodes.GetByOffset(0)!.GetOutgoingEdges());
            Assert.Empty(cfg.Nodes.GetByOffset(10)!.GetIncomingEdges());
        }
    }
}