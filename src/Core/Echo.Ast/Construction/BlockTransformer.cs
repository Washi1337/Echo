using System;
using System.Collections.Generic;
using System.Linq;
using Echo.ControlFlow.Blocks;
using Echo.Core.Code;
using Echo.DataFlow;

namespace Echo.Ast.Construction
{
    internal sealed class BlockTransformer<TInstruction>
    {
        private readonly AstParserContext<TInstruction> _context;

        internal BlockTransformer(AstParserContext<TInstruction> context)
        {
            _context = context;
        }

        internal BasicBlock<Statement<TInstruction>> Transform(BasicBlock<TInstruction> originalBlock)
        {
            int phiStatementCount = 0;
            var result = new BasicBlock<Statement<TInstruction>>();

            foreach (var instruction in originalBlock.Instructions)
            {
                long offset = _context.Architecture.GetOffset(instruction);
                var dataFlowNode = _context.DataFlowGraph.Nodes[offset];
                var stackDependencies = dataFlowNode.StackDependencies;
                var variableDependencies = dataFlowNode.VariableDependencies;
                var buffer = CreateVariableBuffer(stackDependencies.Count + variableDependencies.Count);

                // First step is to collect all the stack dependencies.
                CollectStackDependencies(result, dataFlowNode, buffer, ref phiStatementCount);
                
                // Second step is to collect all the variable dependencies.
                var slice = buffer.AsSpan().Slice(stackDependencies.Count);
                CollectVariableDependencies(result, dataFlowNode, slice, ref phiStatementCount);
            }

            return result;
        }

        private void CollectStackDependencies(
            BasicBlock<Statement<TInstruction>> block,
            DataFlowNode<TInstruction> dataFlowNode,
            Span<IVariable> buffer,
            ref int phiStatementCount)
        {
            var stackDependencies = dataFlowNode.StackDependencies;
            
            for (int i = 0; i < stackDependencies.Count; i++)
            {
                var sources = stackDependencies[i];
                if (sources.Count == 1)
                {
                    // If the dependency only has 1 possible data source, we can simply
                    // create a stack slot and an assignment to that slot.
                    buffer[i] = GetOrCreateStackSlot(sources.First());
                }
                else
                {
                    // If we have more than 1 possible data source, we
                    // will add a "phi" node. This basically means
                    // that the result of the "phi function" will return
                    // the value based on prior control flow.
                    var phi = CreatePhiSlot();
                    var slots = sources
                        .Select(source => new VariableExpression<TInstruction>(GetOrCreateStackSlot(source)))
                        .ToArray();

                    block.Instructions.Insert(phiStatementCount++, new PhiStatement<TInstruction>(phi, slots));
                    buffer[i] = phi;
                }
            }
        }

        private void CollectVariableDependencies(
            BasicBlock<Statement<TInstruction>> block,
            DataFlowNode<TInstruction> dataFlowNode,
            Span<IVariable> buffer,
            ref int phiStatementCount)
        {
            int index = 0;
            var variableDependencies = dataFlowNode.VariableDependencies;
            
            foreach (var pair in variableDependencies)
            {
                var variable = pair.Key;
                var dependency = pair.Value;
                
                if (dependency.Count == 1)
                {
                    // If the dependency has only 1 possible source we just simply
                    // get the variable. But since the AST utilizes SSA, all of the
                    // variables are versioned. This is good because everything is
                    // "immutable". One "real" variable will have a new versioned
                    // variable created every time it is assigned to.
                    int version = _context.GetVariableVersion(variable);
                    var snapshot = new VariableSnapshot(variable, version);

                    buffer[index++] = _context.GetVersionedVariable(snapshot);
                    continue;
                }

                // Otherwise (>0), we will get the list of versioned(!) variables
                // that could reach the instruction and create a "phi" statement
                // like in the stack dependencies.
                var sources = CollectVariables();

                if (_context.VariableSourcesToPhiVariable.TryGetValue(sources, out var phi))
                {
                    // If a phi slot already exists for the list of variables,
                    // reuse the same phi slot.
                    buffer[index++] = phi;
                }
                else
                {
                    // Otherwise, create a new phi slot for the list of variables
                    // and save it if we encounter the same variables again.
                    phi = CreatePhiSlot();
                    var slots = sources
                        .Select(source => new VariableExpression<TInstruction>(source))
                        .ToArray();
                        
                    _context.VariableSourcesToPhiVariable.Add(sources, phi);
                    block.Instructions.Insert(phiStatementCount++, new PhiStatement<TInstruction>(phi, slots));
                    buffer[index++] = phi;
                }

                List<AstVariable> CollectVariables()
                {
                    var result = new List<AstVariable>();
                    
                    foreach (var instruction in dependency.Select(dep => dep.Node.Contents))
                    {
                        if (_context.VariableStates.TryGetValue(instruction, out var versions))
                        {
                            // If we already have a dictionary for the instruction, we will
                            // just get the versioned variable from the existing dictionary.
                            var snapshot = new VariableSnapshot(variable, versions[variable]);
                            result.Add(_context.VersionedVariables[snapshot]);
                        }
                        else
                        {
                            // Otherwise, we will create a new dictionary for the instruction.
                            var snapshot = new VariableSnapshot(variable, 0);
                            var slot = _context.GetVersionedVariable(snapshot);
                            
                            _context.VersionedVariables.Add(snapshot, slot);
                            _context.VariableStates.Add(instruction, new Dictionary<IVariable, int>
                            {
                                [variable] = 0
                            });
                            
                            result.Add(slot);
                        }
                    }

                    return result;
                }
            }
        }
        
        private IVariable GetOrCreateStackSlot(DataSource<TInstruction> dataSource)
        {
            var node = dataSource.Node;
            if (node is ExternalDataSourceNode<TInstruction> external)
                return _context.GetExternalVariable(external);

            return _context.StackSlots[node.Id][dataSource.SlotIndex];
        }

        private AstVariable CreatePhiSlot() => new AstVariable($"phi_{_context.PhiCount++}");

        private static IVariable[] CreateVariableBuffer(int length) =>
            length == 0 ? Array.Empty<IVariable>() : new IVariable[length];
    }
}