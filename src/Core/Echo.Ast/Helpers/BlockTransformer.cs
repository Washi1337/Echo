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
        private readonly Dictionary<ControlFlowRegion<TInstruction>, ControlFlowRegion<AstStatementBase<TInstruction>>>
            _regionMapping = new Dictionary<ControlFlowRegion<TInstruction>, ControlFlowRegion<AstStatementBase<TInstruction>>>();

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
        }

        public void VisitBasicBlock(BasicBlock<TInstruction> block)
        {
            static IVariable[] CreateVariablesBuffer(int count) =>
                count == 0 ? Array.Empty<IVariable>() : new IVariable[count];
            
            int phiCount = 0;
            var mock = new BasicBlock<AstStatementBase<TInstruction>>(block.Offset);

            foreach (var instruction in block.Instructions)
            {
                var architecture = _controlFlowGraph.Architecture;
                long offset = architecture.GetOffset(instruction);
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
                    architecture.GetWrittenVariablesCount(instruction));
                if (writtenVariables.Length > 0)
                    architecture.GetWrittenVariables(instruction, writtenVariables.AsSpan());

                if (!dataFlowNode.GetDependants().Any() && writtenVariables.Length == 0)
                {
                    mock.Instructions.Add(new AstExpressionStatement<TInstruction>(_id--, instructionExpression));
                }
                else
                {
                    int stackPushCount = architecture.GetStackPushCount(instruction);
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

            _compilationUnit.Nodes.Add(new ControlFlowNode<AstStatementBase<TInstruction>>(mock.Offset, mock));
        }

        public void EnterScopeBlock(ScopeBlock<TInstruction> block)
        {
            throw new NotImplementedException();
        }

        public void ExitScopeBlock(ScopeBlock<TInstruction> block)
        {
            throw new NotImplementedException();
        }

        public void EnterExceptionHandlerBlock(ExceptionHandlerBlock<TInstruction> block)
        {
            throw new NotImplementedException();
        }

        public void ExitExceptionHandlerBlock(ExceptionHandlerBlock<TInstruction> block)
        {
            throw new NotImplementedException();
        }

        public void EnterProtectedBlock(ExceptionHandlerBlock<TInstruction> block)
        {
            throw new NotImplementedException();
        }

        public void ExitProtectedBlock(ExceptionHandlerBlock<TInstruction> block)
        {
            throw new NotImplementedException();
        }

        public void EnterHandlerBlock(ExceptionHandlerBlock<TInstruction> block, int handlerIndex)
        {
            throw new NotImplementedException();
        }

        public void ExitHandlerBlock(ExceptionHandlerBlock<TInstruction> block, int handlerIndex)
        {
            throw new NotImplementedException();
        }
    }
}