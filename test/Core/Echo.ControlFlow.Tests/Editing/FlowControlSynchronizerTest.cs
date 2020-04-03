using System;
using System.Linq;
using Echo.ControlFlow.Construction;
using Echo.ControlFlow.Construction.Static;
using Echo.ControlFlow.Editing.Synchronization;
using Echo.Platforms.DummyPlatform.Code;
using Xunit;

namespace Echo.ControlFlow.Tests.Editing
{
    public class FlowControlSynchronizerTest
    {
        private readonly StaticFlowGraphBuilder<DummyInstruction> _builder;

        public FlowControlSynchronizerTest()
        {
            _builder = new StaticFlowGraphBuilder<DummyInstruction>(DummyArchitecture.Instance, DummyArchitecture.Instance.SuccessorResolver);
        }
        
        [Fact]
        public void NoChangeShouldResultInNoChangeInTheGraph()
        {
            var cfg = _builder.ConstructFlowGraph(new[]
            { 
                DummyInstruction.Ret(0)
            },0);
            
            Assert.False(cfg.UpdateFlowControl(_builder.SuccessorResolver));
        }

        [Fact]
        public void BranchTargetChangeToAnotherNodeHeaderShouldUpdateFallThroughEdge()
        {
            var cfg = _builder.ConstructFlowGraph(new[]
            { 
                DummyInstruction.Op(0,0, 0),
                DummyInstruction.Jmp(1, 10),
                
                DummyInstruction.Jmp(10, 20),
                
                DummyInstruction.Ret(20)
            },0);
            
            // Change branch target of the first jmp to the ret at offset 20.
            cfg.Nodes[0].Contents.Footer.Operands[0] = 20L;
            
            Assert.True(cfg.UpdateFlowControl(_builder.SuccessorResolver));
            Assert.Same(cfg.Nodes[20], cfg.Nodes[0].FallThroughNeighbour);
        }

        [Fact]
        public void ConditionalBranchTargetChangeToAnotherNodeHeaderShouldUpdateConditionalEdge()
        {
            var cfg = _builder.ConstructFlowGraph(new[]
            { 
                DummyInstruction.Push(0,1),
                DummyInstruction.JmpCond(1, 20),
                
                DummyInstruction.Jmp(2, 20),
                
                DummyInstruction.Ret(20)
            },0);
            
            // Add a new node to use as a branch target.
            var newTarget = new ControlFlowNode<DummyInstruction>(100, DummyInstruction.Jmp(100, 20));
            cfg.Nodes.Add(newTarget);
            newTarget.ConnectWith(cfg.Nodes[20]);
            
            // Update branch target.
            cfg.Nodes[0].Contents.Footer.Operands[0] = 100L;
            
            Assert.True(cfg.UpdateFlowControl(_builder.SuccessorResolver));
            Assert.Single(cfg.Nodes[0].ConditionalEdges);
            Assert.True(cfg.Nodes[0].ConditionalEdges.Contains(cfg.Nodes[100]));
        }

        [Fact]
        public void SwapUnconditionalWithConditionalBranchShouldUpdateFallThroughAndConditionalEdge()
        {
            var cfg = _builder.ConstructFlowGraph(new[]
            { 
                DummyInstruction.Push(0,1),
                DummyInstruction.Jmp(1, 10),
                
                DummyInstruction.Jmp(2, 20),
                
                DummyInstruction.Jmp(10, 2),
                
                DummyInstruction.Ret(20)
            },0);

            // Update unconditional jmp to a conditional one.
            var blockInstructions = cfg.Nodes[0].Contents.Instructions;
            blockInstructions[blockInstructions.Count - 1] = DummyInstruction.JmpCond(1, 20);

            Assert.True(cfg.UpdateFlowControl(_builder.SuccessorResolver));
            Assert.Same(cfg.Nodes[2], cfg.Nodes[0].FallThroughNeighbour);
            Assert.Single(cfg.Nodes[0].ConditionalEdges);
            Assert.True(cfg.Nodes[0].ConditionalEdges.Contains(cfg.Nodes[20]));
        }

        [Fact]
        public void ChangeBranchTargetToMiddleOfNodeShouldSplitNode()
        {
            var instructions = new[]
            { 
                DummyInstruction.Push(0,1),
                DummyInstruction.Jmp(1, 10),
                
                DummyInstruction.Op(10, 0,0),
                DummyInstruction.Op(11, 0,0),
                DummyInstruction.Op(12, 0,0),
                DummyInstruction.Op(13, 0,0),
                DummyInstruction.Op(14, 0,0),
                DummyInstruction.JmpCond(15, 10),
                
                DummyInstruction.Ret(16)
            };
            var cfg = _builder.ConstructFlowGraph(instructions,0);

            // Change jmp target to an instruction in the middle of node[10].
            cfg.Nodes[0].Contents.Footer.Operands[0] = 13L;
            
            Assert.True(cfg.UpdateFlowControl(_builder.SuccessorResolver));
            
            Assert.True(cfg.Nodes.Contains(10), "Original target does not exist anymore.");
            Assert.True(cfg.Nodes.Contains(13), "Original target was not split up correctly.");
            
            Assert.Same(cfg.Nodes[13], cfg.Nodes[10].FallThroughNeighbour);
            Assert.Same(cfg.Nodes[13], cfg.Nodes[0].FallThroughNeighbour);
        }

        [Fact]
        public void UpdateToInvalidBranchTargetShouldThrowAndDiscard()
        {
            var cfg = _builder.ConstructFlowGraph(new[]
            {
                DummyInstruction.Jmp(0, 10),
                DummyInstruction.Ret(10), 
            },0);

            cfg.Nodes[0].Contents.Header.Operands[0] = 100L;
            
            Assert.Throws<ArgumentException>(() => cfg.UpdateFlowControl(_builder.SuccessorResolver));
            
            Assert.Same(cfg.Nodes[10], cfg.Nodes[0].FallThroughNeighbour);
        }
        
        [Fact]
        public void SplittedNodeShouldBeRemergedAfterDetectingInvalidFallThroughNeighbour()
        {
            var instructions = new[]
            {
                DummyInstruction.Op(0, 0, 0), 
                DummyInstruction.Jmp(1, 10),
                DummyInstruction.Op(10, 0, 0),
                DummyInstruction.Op(11, 0, 0),
                DummyInstruction.Op(12, 0, 0), 
                DummyInstruction.Jmp(13, 20),
                DummyInstruction.Ret(20), 
            };
            var cfg = _builder.ConstructFlowGraph(instructions,0);

            instructions[1].Operands[0] = 12L;
            instructions[5].Operands[0] = 100L;
            
            Assert.Throws<ArgumentException>(() => cfg.UpdateFlowControl(_builder.SuccessorResolver));
            
            Assert.Same(cfg.Nodes[10], cfg.Nodes[0].FallThroughNeighbour);
            Assert.False(cfg.Nodes.Contains(12));
        }
        
        [Fact]
        public void SplittedNodeShouldBeRemergedAfterDetectingInvalidConditionalNeighbour()
        {
            var instructions = new[]
            {
                DummyInstruction.Op(0, 0, 0),
                DummyInstruction.Switch(1, 2, 3, 4, 6),
                
                DummyInstruction.Jmp(2, 20),
                
                DummyInstruction.Jmp(3, 20),
                
                DummyInstruction.Op(4, 0, 0),
                DummyInstruction.Jmp(5, 20),
                
                DummyInstruction.Jmp(6, 20),
                
                DummyInstruction.Ret(20),
            };
            var cfg = _builder.ConstructFlowGraph(instructions,0);

            var targets = (long[]) instructions[1].Operands[0];
            targets[2] = 5L;
            targets[3] = 100L;
            
            Assert.Throws<ArgumentException>(() => cfg.UpdateFlowControl(_builder.SuccessorResolver));
            
            Assert.False(cfg.Nodes.Contains(5));
            Assert.Equal(new[]
            {
                instructions[4],
                instructions[5]
            }, cfg.Nodes[4].Contents.Instructions);
        }

        [Fact]
        public void AddBranchInMiddleOfBlockShouldSplit()
        {
            var instructions = new[]
            {
                DummyInstruction.Op(0, 0, 0),
                DummyInstruction.Op(1, 0, 0),
                DummyInstruction.Op(2, 0, 0),
                DummyInstruction.Op(3, 0, 0),
                DummyInstruction.Ret(4),
            };
            var cfg = _builder.ConstructFlowGraph(instructions, 0);

            cfg.Nodes[0].Contents.Instructions[1] = DummyInstruction.Jmp(1, 4);
            cfg.UpdateFlowControl(_builder.SuccessorResolver);

            Assert.True(cfg.Nodes.Contains(0));
            Assert.True(cfg.Nodes.Contains(4));
            Assert.Same(cfg.Nodes[4], cfg.Nodes[0].FallThroughNeighbour);
        }

        [Fact]
        public void AddBranchInMiddleOfBlockShouldSplit2()
        {
            var instructions = new[]
            {
                DummyInstruction.Push(0, 1),
                DummyInstruction.Pop(1, 1),
                DummyInstruction.Push(2, 1),
                DummyInstruction.JmpCond(3, 5),
                
                DummyInstruction.Ret(4),
                
                DummyInstruction.Op(5, 0, 0),
                DummyInstruction.Ret(6),
            };
            var cfg = _builder.ConstructFlowGraph(instructions, 0);

            cfg.Nodes[0].Contents.Instructions[1] = DummyInstruction.JmpCond(1, 5);
            cfg.Nodes[0].Contents.Instructions[3] = DummyInstruction.Pop(3, 1);
            cfg.UpdateFlowControl(_builder.SuccessorResolver);

            Assert.True(cfg.Nodes.Contains(2));
            Assert.Same(cfg.Nodes[2], cfg.Nodes[0].FallThroughNeighbour);
            Assert.Same(cfg.Nodes[5], cfg.Nodes[0].ConditionalEdges.First().Target);
            Assert.Same(cfg.Nodes[4], cfg.Nodes[2].FallThroughNeighbour);
            Assert.Empty(cfg.Nodes[2].ConditionalEdges);
        }
    }
}