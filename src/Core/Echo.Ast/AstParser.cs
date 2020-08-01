using System;
using System.Collections.Generic;
using System.Linq;
using Echo.ControlFlow;
using Echo.ControlFlow.Blocks;
using Echo.ControlFlow.Regions;
using Echo.ControlFlow.Serialization.Blocks;
using Echo.Core.Code;
using Echo.DataFlow;
using Echo.DataFlow.Analysis;

namespace Echo.Ast
{
    /// <summary>
    /// Transforms a <see cref="ControlFlowGraph{TInstruction}"/> into an Ast
    /// </summary>
    public sealed class AstParser<TInstruction>
    {
        private readonly ControlFlowGraph<TInstruction> _controlFlowGraph;
        private readonly DataFlowGraph<TInstruction> _dataFlowGraph;
        private readonly IInstructionSetArchitecture<AstStatementBase<TInstruction>> _architecture;

        /// <summary>
        /// Creates a new Ast parser with the given <see cref="ControlFlowGraph{TInstruction}"/>
        /// </summary>
        /// <param name="controlFlowGraph">The <see cref="ControlFlowGraph{TInstruction}"/> to parse</param>
        /// <param name="dataFlowGraph">The <see cref="DataFlowGraph{TContents}"/> to parse</param>
        public AstParser(ControlFlowGraph<TInstruction> controlFlowGraph, DataFlowGraph<TInstruction> dataFlowGraph)
        {
            _controlFlowGraph = controlFlowGraph;
            _dataFlowGraph = dataFlowGraph;
            _architecture = new AstInstructionSetArchitectureDecorator<TInstruction>(_controlFlowGraph.Architecture);
        }

        /// <summary>
        /// Parses the given <see cref="ControlFlowGraph{TInstruction}"/>
        /// </summary>
        /// <returns>A <see cref="CompilationUnit{TInstruction}"/> representing the Ast</returns>
        public CompilationUnit<TInstruction> Parse()
        {
            var root = new CompilationUnit<TInstruction>(_architecture);
            var blockBuilder = new BlockBuilder<TInstruction>();
            var rootScope = blockBuilder.ConstructBlocks(_controlFlowGraph);
            var walker = new BlockWalker<TInstruction>(new BlockTransformer(root, _controlFlowGraph, _dataFlowGraph));

            rootScope.AcceptVisitor(walker);
            
            return root;
        }

        private sealed class BlockTransformer : IBlockListener<TInstruction>
        {
            private readonly CompilationUnit<TInstruction> _compilationUnit;
            private readonly ControlFlowGraph<TInstruction> _controlFlowGraph;
            private readonly DataFlowGraph<TInstruction> _dataFlowGraph;
            private readonly Dictionary<TInstruction, AstVariable> _stackSlots;
            private readonly Stack<IControlFlowRegion<AstNodeBase<TInstruction>>> _regions;
            
            internal BlockTransformer(
                CompilationUnit<TInstruction> compilationUnit,
                ControlFlowGraph<TInstruction> controlFlowGraph,
                DataFlowGraph<TInstruction> dataFlowGraph)
            {
                _compilationUnit = compilationUnit;
                _controlFlowGraph = controlFlowGraph;
                _dataFlowGraph = dataFlowGraph;
                _stackSlots = new Dictionary<TInstruction, AstVariable>();
                _regions = new Stack<IControlFlowRegion<AstNodeBase<TInstruction>>>();
            }

            private IInstructionSetArchitecture<TInstruction> Architecture => _controlFlowGraph.Architecture;

            public void VisitBasicBlock(BasicBlock<TInstruction> block)
            {
                // TODO: Phi statements should always be at the top of a block
                foreach (var instruction in block.Instructions)
                {
                    long offset = Architecture.GetOffset(instruction);
                    var dataFlowNode = _dataFlowGraph.Nodes[offset];
                    
                    // Nothing depends on this instruction, so we can just make a statement and add it to the block
                    if (!dataFlowNode.GetDependants().Any())
                    {
                        //
                    }
                    
                    var stackDependencies = dataFlowNode.StackDependencies;
                    var instructionExpression = new AstInstructionExpression<TInstruction>(offset, instruction, null);
                }
            }

            public void EnterScopeBlock(ScopeBlock<TInstruction> block)
            {
                _regions.Push(TransformRegion(_controlFlowGraph.Nodes[block.GetAllBlocks().First().Offset].ParentRegion));
            }

            public void ExitScopeBlock(ScopeBlock<TInstruction> block)
            {
                _regions.Pop();
            }

            private static IControlFlowRegion<AstNodeBase<TInstruction>> TransformRegion(
                IControlFlowRegion<TInstruction> region)
            {
                return region switch
                {
                    BasicControlFlowRegion<TInstruction> _ => new BasicControlFlowRegion<AstNodeBase<TInstruction>>(),
                    ExceptionHandlerRegion<TInstruction> _ => new ExceptionHandlerRegion<AstNodeBase<TInstruction>>(),
                    _ => throw new NotSupportedException()
                };
            }
        }
    }
}