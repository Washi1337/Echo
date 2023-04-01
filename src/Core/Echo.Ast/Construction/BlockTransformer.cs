using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Echo.ControlFlow.Blocks;
using Echo.Code;
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
            result.Offset = originalBlock.Offset;

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
                
                // Third step is to increment the "version" of the written variables.
                var writtenVariables = VersionWrittenVariables(instruction);

                // Create the instruction expression.
                var variableExpressions = buffer.Select(v => new VariableExpression<TInstruction>(v));
                var expression = new InstructionExpression<TInstruction>(instruction, variableExpressions);

                // Fourth step is to create an assignment- or an expression statement depending
                // on whether the data flow node has any dependants and how many variables
                // the instruction writes to.
                if (dataFlowNode.InDegree == 0 && writtenVariables.Length == 0)
                {
                    // The data flow node has no dependants and doesn't write to any variables.
                    // So we can create a simple expression statement.
                    result.Instructions.Add(new ExpressionStatement<TInstruction>(expression));
                }
                else
                {
                    // Otherwise an assignment statement will be added.
                    result.Instructions.Add(CreateAssignment(expression, instruction, writtenVariables));
                }
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
                    buffer[i] = GetStackSlot(sources.First());
                }
                else
                {
                    // If we have more than 1 possible data source, we
                    // will add a "phi" node. This basically means
                    // that the result of the "phi function" will return
                    // the value based on prior control flow.
                    var phi = CreatePhiSlot();
                    var slots = sources
                        .Select(source => new VariableExpression<TInstruction>(GetStackSlot(source)))
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
            
            foreach (var dependency in variableDependencies)
            {
                var variable = dependency.Variable;
                
                if (dependency.Count <= 1)
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

        private IVariable[] VersionWrittenVariables(TInstruction instruction)
        {
            var buffer = GetWrittenVariables(instruction);

            foreach (var variable in buffer)
            {
                if (_context.VariableStates.TryGetValue(instruction, out var states))
                {
                    // If we already have a dictionary that contains versions for this instruction,
                    // we can reuse the same instance.
                    
                    // If the instruction hasn't "seen" the variable before, we add a new entry.
                    if (!states.ContainsKey(variable))
                        states.Add(variable, _context.VariableVersions[variable]++);

                    // Create a new snapshot of the variable and store the versioned variable
                    // created from the snapshot.
                    var snapshot = new VariableSnapshot(variable, _context.VariableVersions[variable]);
                    var versionedVariable = _context.GetVersionedVariable(snapshot);
                    _context.VersionedVariables[snapshot] = versionedVariable;
                }
                else
                {
                    // Otherwise we will create a new dictionary and store it so it can be
                    // reused later.
                    
                    // Increment the version of the variable, create a snapshot and
                    // add it to the newly created dictionary.
                    int version = _context.IncrementVariableVersion(variable);
                    var snapshot = new VariableSnapshot(variable, version);
                    states = new Dictionary<IVariable, int>
                    {
                        [variable] = version
                    };

                    // Save the dictionary for later reuse and get a variable for the snapshot.
                    _context.VariableStates[instruction] = states;
                    _context.VersionedVariables[snapshot] = _context.GetVersionedVariable(snapshot);
                }
            }

            return buffer;
        }

        private IVariable[] GetWrittenVariables(TInstruction instruction)
        {
            int count = _context.Architecture.GetWrittenVariablesCount(instruction);
            if (count == 0)
                return Array.Empty<IVariable>();

            var buffer = new IVariable[count];
            _context.Architecture.GetWrittenVariables(instruction, buffer);

            return buffer;
        }

        [SuppressMessage("ReSharper", "SuggestBaseTypeForParameter")]
        private AssignmentStatement<TInstruction> CreateAssignment(
            Expression<TInstruction> expression,
            TInstruction instruction,
            IVariable[] writtenVariables)
        {
            // We will create stack slots for every push and "simulate" an assignment
            // to them. This way the resulting AST won't have the "concept" of a stack.
            int pushes = _context.Architecture.GetStackPushCount(instruction);
            var slots = GetStackSlots(pushes);
            var buffer = new AstVariable[slots.Length + writtenVariables.Length];
            slots.CopyTo(buffer.AsSpan());

            // Also assign to written variables.
            for (int i = 0; i < writtenVariables.Length; i++)
            {
                // Get the correct version of the variable to assign to.
                var variable = writtenVariables[i];
                int version = _context.GetVariableVersion(variable);
                var snapshot = new VariableSnapshot(variable, version);
                var versionedVariable = _context.GetVersionedVariable(snapshot);

                buffer[slots.Length + i] = versionedVariable;
            }

            // Assign stack slots.
            _context.StackSlots[instruction] = slots;
            return new AssignmentStatement<TInstruction>(buffer, expression);
        }
        
        private IVariable GetStackSlot(StackDataSource<TInstruction> dataSource)
        {
            var node = dataSource.Node;
            if (node is ExternalDataSourceNode<TInstruction> external)
                return _context.GetExternalVariable(external);

            return _context.StackSlots[node.Contents][dataSource.SlotIndex];
        }

        private AstVariable[] GetStackSlots(int pushes)
        {
            if (pushes == 0)
                return Array.Empty<AstVariable>();

            var buffer = new AstVariable[pushes];
            for (int i = 0; i < buffer.Length; i++)
                buffer[i] = _context.CreateStackSlot();

            return buffer;
        }

        private AstVariable CreatePhiSlot() => new AstVariable($"phi_{_context.PhiCount++}");

        private static IVariable[] CreateVariableBuffer(int length) =>
            length == 0 ? Array.Empty<IVariable>() : new IVariable[length];
    }
}