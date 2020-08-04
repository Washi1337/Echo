using System;
using System.Collections.Generic;
using System.Linq;
using Echo.ControlFlow;
using Echo.ControlFlow.Blocks;
using Echo.ControlFlow.Regions;
using Echo.ControlFlow.Serialization.Blocks;
using Echo.Core.Code;
using Echo.DataFlow;

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
            var transformer = new BlockTransformer(root, _controlFlowGraph, _dataFlowGraph);
            var walker = new BlockWalker<TInstruction>(transformer);

            rootScope.AcceptVisitor(walker);
            
            return root;
        }

        private sealed class BlockTransformer : IBlockListener<TInstruction>
        {
            private readonly CompilationUnit<TInstruction> _compilationUnit;
            private readonly ControlFlowGraph<TInstruction> _controlFlowGraph;
            private readonly DataFlowGraph<TInstruction> _dataFlowGraph;
            private readonly Dictionary<TInstruction, AstVariable[]> _stackSlots;
            private readonly Dictionary<ControlFlowNode<TInstruction>, ControlFlowNode<AstStatementBase<TInstruction>>> _mapping;
            private readonly Dictionary<ControlFlowRegion<TInstruction>, ControlFlowRegion<AstStatementBase<TInstruction>>> _oldToNew;
            private readonly Dictionary<ControlFlowRegion<AstStatementBase<TInstruction>>, ControlFlowRegion<TInstruction>> _newToOld;
            private readonly Stack<IControlFlowRegion<AstStatementBase<TInstruction>>> _regions;
            private long _id = -1;
            private long _variableCount;
            private long _phiSlotCount;
            
            internal BlockTransformer(
                CompilationUnit<TInstruction> compilationUnit,
                ControlFlowGraph<TInstruction> controlFlowGraph,
                DataFlowGraph<TInstruction> dataFlowGraph)
            {
                _compilationUnit = compilationUnit;
                _controlFlowGraph = controlFlowGraph;
                _dataFlowGraph = dataFlowGraph;
                _stackSlots = new Dictionary<TInstruction, AstVariable[]>();
                _mapping = new Dictionary<ControlFlowNode<TInstruction>, ControlFlowNode<AstStatementBase<TInstruction>>>();
                _oldToNew = new Dictionary<ControlFlowRegion<TInstruction>, ControlFlowRegion<AstStatementBase<TInstruction>>>();
                _newToOld = new Dictionary<ControlFlowRegion<AstStatementBase<TInstruction>>, ControlFlowRegion<TInstruction>>();
                _regions = new Stack<IControlFlowRegion<AstStatementBase<TInstruction>>>();
            }

            private IInstructionSetArchitecture<TInstruction> Architecture => _controlFlowGraph.Architecture;

            public void VisitBasicBlock(BasicBlock<TInstruction> block)
            {
                int phiCount = 0;
                var astBlock = new BasicBlock<AstStatementBase<TInstruction>>();
                
                foreach (var instruction in block.Instructions)
                {
                    long offset = Architecture.GetOffset(instruction);
                    var dataFlowNode = _dataFlowGraph.Nodes[offset];
                    var stackDependencies = dataFlowNode.StackDependencies;
                    var variableDependencies = dataFlowNode.VariableDependencies;
                    var targetVariables = new IVariable[stackDependencies.Count + variableDependencies.Count];

                    for (int i = 0; i < stackDependencies.Count; i++)
                    {
                        var sources = stackDependencies[i];
                        if (sources.Count == 1)
                        {
                            var dep = sources.First(); 
                            var variable = _stackSlots[dep.Node.Contents];
                            targetVariables[i] = variable[dep.SlotIndex];
                        }
                        else
                        {
                            var phiSlot = new AstVariable($"phi_{_phiSlotCount++}");
                            var phi = new AstPhiStatement<TInstruction>(
                                _id--,
                                sources.Select(s =>
                                    new AstVariableExpression<TInstruction>(_id--,
                                        _stackSlots[s.Node.Contents][s.SlotIndex])
                                ).ToArray(), phiSlot);

                            astBlock.Instructions.Insert(phiCount++, phi);
                            targetVariables[i] = phiSlot;
                        }
                    }

                    int index = stackDependencies.Count;
                    foreach (var dep in variableDependencies)
                    {
                        targetVariables[index] = dep.Key;
                        index++;
                    }

                    var instructionExpression = new AstInstructionExpression<TInstruction>(offset, instruction,
                        targetVariables.Select(t => new AstVariableExpression<TInstruction>(_id--, t)).ToArray());

                    var writtenVariables = new IVariable[Architecture.GetWrittenVariablesCount(instruction)];
                    Architecture.GetWrittenVariables(instruction, writtenVariables.AsSpan());
                    
                    if (!dataFlowNode.GetDependants().Any() && writtenVariables.Length == 0)
                        astBlock.Instructions.Add(new AstExpressionStatement<TInstruction>(offset, instructionExpression));
                    else
                    {
                        int stackPushCount = Architecture.GetStackPushCount(instruction);
                        var slots = stackPushCount == 0
                            ? Array.Empty<AstVariable>()
                            : Enumerable.Range(0, stackPushCount)
                                .Select(_ => new AstVariable($"stack_slot_{_variableCount++}"))
                                .ToArray();

                        var combined = new IVariable[slots.Length + writtenVariables.Length];
                        slots.CopyTo(combined, 0);
                        writtenVariables.CopyTo(combined, Math.Max(0, slots.Length - 1));
                        _stackSlots[instruction] = slots;
                        astBlock.Instructions.Add(
                            new AstAssignmentStatement<TInstruction>(offset, instructionExpression, combined));
                    }
                }

                long firstOffset = Architecture.GetOffset(block.Instructions[0]);
                var newNode = new ControlFlowNode<AstStatementBase<TInstruction>>(firstOffset, astBlock);
                _mapping[_controlFlowGraph.Nodes[firstOffset]] = newNode;
                _compilationUnit.Nodes.Add(newNode);
            }

            public void EnterScopeBlock(ScopeBlock<TInstruction> block)
            {
                long first = block.Blocks[0].GetAllBlocks().First().Offset;
                var region = _controlFlowGraph.Nodes[first].ParentRegion;
                _regions.Push(TransformRegion(region));
            }

            public void ExitScopeBlock(ScopeBlock<TInstruction> block)
            {
                var region = _regions.Pop();
                if (region is CompilationUnit<TInstruction>)
                {
                    foreach (var edge in _controlFlowGraph.GetEdges())
                        _mapping[edge.Origin].ConnectWith(_mapping[edge.Target], edge.Type);

                    foreach (var subregion in _compilationUnit.Regions)
                        CopyRegion(_newToOld[subregion], subregion);
                    return;
                }

                void CopyRegion(ControlFlowRegion<TInstruction> old,
                    ControlFlowRegion<AstStatementBase<TInstruction>> @new)
                {
                    if (old is ExceptionHandlerRegion<TInstruction> oldExceptionHandlerRegion)
                    {
                        foreach (var protectedNode in oldExceptionHandlerRegion.ProtectedRegion.Nodes)
                            ((ExceptionHandlerRegion<AstStatementBase<TInstruction>>) @new).ProtectedRegion.Nodes.Add(_mapping[protectedNode]);
                        foreach (var handlerRegion in oldExceptionHandlerRegion.HandlerRegions)
                            CopyRegion(handlerRegion, _oldToNew[handlerRegion]);
                    }

                    if (old is BasicControlFlowRegion<TInstruction> basicControlFlowRegion)
                    {
                        foreach (var node in basicControlFlowRegion.Nodes)
                            ((BasicControlFlowRegion<AstStatementBase<TInstruction>>) @new).Nodes.Add(_mapping[node]);
                        foreach (var subregion in basicControlFlowRegion.Regions)
                            CopyRegion(subregion, _oldToNew[subregion]);
                    }
                }

                _compilationUnit.Regions.Add((ControlFlowRegion<AstStatementBase<TInstruction>>) region);
            }

            private IControlFlowRegion<AstStatementBase<TInstruction>> TransformRegion(
                IControlFlowRegion<TInstruction> region)
            {
                IControlFlowRegion<AstStatementBase<TInstruction>> AddToMapping(ControlFlowRegion<TInstruction> old)
                {
                    ControlFlowRegion<AstStatementBase<TInstruction>> @new = old switch
                    {
                        BasicControlFlowRegion<TInstruction> _ => new BasicControlFlowRegion<AstStatementBase<TInstruction>>(),
                        ExceptionHandlerRegion<TInstruction> _ => new ExceptionHandlerRegion<AstStatementBase<TInstruction>>(),
                        _ => throw new NotSupportedException()
                    };

                    _oldToNew[old] = @new;
                    _newToOld[@new] = old;
                    return @new;
                }
                
                return region switch
                {
                    ControlFlowGraph<TInstruction> _ => _compilationUnit,
                    BasicControlFlowRegion<TInstruction> basicControlFlowRegion => AddToMapping(basicControlFlowRegion),
                    ExceptionHandlerRegion<TInstruction> exceptionHandlerRegion => AddToMapping(exceptionHandlerRegion),
                    _ => throw new NotSupportedException()
                };
            }
        }
    }
}