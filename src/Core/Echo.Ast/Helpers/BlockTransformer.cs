using System;
using System.Collections.Generic;
using System.Linq;
using Echo.ControlFlow;
using Echo.ControlFlow.Blocks;
using Echo.ControlFlow.Regions;
using Echo.Core.Code;
using Echo.DataFlow;

namespace Echo.Ast.Helpers
{
    internal sealed class BlockTransformer<TInstruction> : IBlockListener<TInstruction>
    {
        private readonly CompilationUnit<TInstruction> _compilationUnit;
        private readonly ControlFlowGraph<TInstruction> _controlFlowGraph;
        private readonly DataFlowGraph<TInstruction> _dataFlowGraph;
        private readonly Dictionary<TInstruction, AstVariable[]> _stackSlots = new Dictionary<TInstruction, AstVariable[]>();
        private readonly Stack<IControlFlowRegion<AstStatementBase<TInstruction>>>
            _regions = new Stack<IControlFlowRegion<AstStatementBase<TInstruction>>>();
        private readonly Dictionary<ControlFlowNode<TInstruction>, ControlFlowNode<AstStatementBase<TInstruction>>>
            _mapping = new Dictionary<ControlFlowNode<TInstruction>, ControlFlowNode<AstStatementBase<TInstruction>>>();
        private readonly Dictionary<IControlFlowRegion<TInstruction>, IControlFlowRegion<AstStatementBase<TInstruction>>>
            _regionsMapping = new Dictionary<IControlFlowRegion<TInstruction>, IControlFlowRegion<AstStatementBase<TInstruction>>>();

        private long _id = -1;
        private long _varCount;
        private long _phiVarCount;

        internal BlockTransformer(
            CompilationUnit<TInstruction> compilationUnit,
            ControlFlowGraph<TInstruction> controlFlowGraph,
            DataFlowGraph<TInstruction> dataFlowGraph)
        {
            _compilationUnit = compilationUnit;
            _controlFlowGraph = controlFlowGraph;
            _dataFlowGraph = dataFlowGraph;
            _regionsMapping[_controlFlowGraph] = _compilationUnit;
        }

        private IInstructionSetArchitecture<TInstruction> Architecture => _controlFlowGraph.Architecture;

        public void VisitBasicBlock(BasicBlock<TInstruction> block)
        {
            static IVariable[] CreateVariablesBuffer(int count) =>
                count == 0 ? Array.Empty<IVariable>() : new IVariable[count];
            
            int phiCount = 0;
            var mock = new BasicBlock<AstStatementBase<TInstruction>>(block.Offset);

            foreach (var instruction in block.Instructions)
            {
                long offset = Architecture.GetOffset(instruction);
                var dataFlowNode = _dataFlowGraph.Nodes[offset];
                var stackDependencies = dataFlowNode.StackDependencies;
                var variableDependencies = dataFlowNode.VariableDependencies;
                var targetVariables = CreateVariablesBuffer(
                    stackDependencies.Count + variableDependencies.Count);

                for (int i = 0; i < stackDependencies.Count; i++)
                {
                    var sources = stackDependencies[i];
                    if (sources.Count == 1)
                    {
                        var source = sources.First();
                        var slot = _stackSlots[source.Node.Contents][source.SlotIndex];
                        targetVariables[i] = slot;
                    }
                    else
                    {
                        var phiVar = new AstVariable($"phi_{_phiVarCount++}");
                        
                        var slots = sources
                            .Select(s => _stackSlots[s.Node.Contents][s.SlotIndex]);
                        var variables = slots
                            .Select(s => new AstVariableExpression<TInstruction>(_id--, s));
                        
                        var phiStatement = new AstPhiStatement<TInstruction>(
                            _id--, variables.ToArray(), phiVar);
                        
                        mock.Instructions.Insert(phiCount++, phiStatement);
                        targetVariables[i] = phiVar;
                    }
                }

                int index = stackDependencies.Count;
                foreach (var variable in variableDependencies)
                    targetVariables[index++] = variable.Key;
                
                var instructionExpression = new AstInstructionExpression<TInstruction>(
                    offset,
                    instruction,
                    targetVariables
                        .Select(t => new AstVariableExpression<TInstruction>(_id--, t))
                        .ToArray()
                );

                var writtenVariables = CreateVariablesBuffer(
                    Architecture.GetWrittenVariablesCount(instruction));
                if (writtenVariables.Length > 0)
                    Architecture.GetWrittenVariables(instruction, writtenVariables.AsSpan());

                if (!dataFlowNode.GetDependants().Any() && writtenVariables.Length == 0)
                {
                    mock.Instructions.Add(new AstExpressionStatement<TInstruction>(_id--, instructionExpression));
                }
                else
                {
                    int stackPushCount = Architecture.GetStackPushCount(instruction);
                    var slots = stackPushCount == 0
                        ? Array.Empty<AstVariable>()
                        : CreateVariablesBuffer(stackPushCount)
                            .Select(v => new AstVariable($"stack_slot{_varCount++}"))
                            .ToArray();

                    var combined = CreateVariablesBuffer(writtenVariables.Length + slots.Length);
                    slots.CopyTo(combined, 0);
                    writtenVariables.CopyTo(combined, Math.Max(0, slots.Length - 1));

                    _stackSlots[instruction] = slots;
                    mock.Instructions.Add(
                        new AstAssignmentStatement<TInstruction>(_id--, instructionExpression, combined));
                }
            }

            var original = _controlFlowGraph.Nodes[block.Offset];
            var node = new ControlFlowNode<AstStatementBase<TInstruction>>(mock.Offset, mock);
            _mapping[original] = node;
        }

        public void EnterScopeBlock(ScopeBlock<TInstruction> block)
        {
            long offset = block.GetAllBlocks().First().Offset;
            var node = _controlFlowGraph.Nodes[offset];
            var region = node.ParentRegion;
            IControlFlowRegion<AstStatementBase<TInstruction>> transformed = region switch
            {
                ControlFlowGraph<TInstruction> _ => _compilationUnit,
                BasicControlFlowRegion<TInstruction> _ => new BasicControlFlowRegion<AstStatementBase<TInstruction>>(),
                _ => throw new NotSupportedException()
            };
            
            _regionsMapping[region] = transformed;
            _regions.Push(transformed);
        }

        public void ExitScopeBlock(ScopeBlock<TInstruction> block)
        {
            _regions.Pop();
            if (_regions.Count != 0)
                return;
            
            foreach (var edge in _controlFlowGraph.GetEdges())
                _mapping[edge.Origin].ConnectWith(_mapping[edge.Target], edge.Type);
            
            CloneRegions(_controlFlowGraph, _compilationUnit);
        }

        public void EnterExceptionHandlerBlock(ExceptionHandlerBlock<TInstruction> block) { }

        public void ExitExceptionHandlerBlock(ExceptionHandlerBlock<TInstruction> block) { }

        public void EnterProtectedBlock(ExceptionHandlerBlock<TInstruction> block) { }

        public void ExitProtectedBlock(ExceptionHandlerBlock<TInstruction> block) { }

        public void EnterHandlerBlock(ExceptionHandlerBlock<TInstruction> block, int handlerIndex) { }

        public void ExitHandlerBlock(ExceptionHandlerBlock<TInstruction> block, int handlerIndex) { }

        private void CloneRegions(
            IControlFlowRegion<TInstruction> old, IControlFlowRegion<AstStatementBase<TInstruction>> @new)
        {
            IControlFlowRegion<AstStatementBase<TInstruction>> GetClone(IControlFlowRegion<TInstruction> oldReg)
            {
                if (_regionsMapping.TryGetValue(oldReg, out var value))
                    return value;
                
                var newRegion = new ExceptionHandlerRegion<AstStatementBase<TInstruction>>();
                _regionsMapping[oldReg] = newRegion;

                return newRegion;
            }
            
            if (old is ControlFlowGraph<TInstruction> cfg)
            {
                foreach (var region in cfg.Regions)
                    CloneRegions(region, GetClone(region));
            }
            else if (old is BasicControlFlowRegion<TInstruction> basic)
            {
                foreach (var node in basic.Nodes)
                    ((BasicControlFlowRegion<AstStatementBase<TInstruction>>) @new).Nodes.Add(_mapping[node]);
            }
            else
            {
                var eh = (ExceptionHandlerRegion<TInstruction>) old;
                CloneRegions(eh.ProtectedRegion, GetClone(eh.ProtectedRegion));
                foreach (var handler in eh.HandlerRegions)
                    CloneRegions(handler, GetClone(handler));
            }
        }
    }
}