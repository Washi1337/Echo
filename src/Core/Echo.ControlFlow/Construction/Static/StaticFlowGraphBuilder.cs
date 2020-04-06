using System;
using System.Collections.Generic;
using System.Linq;
using Echo.Core.Code;

namespace Echo.ControlFlow.Construction.Static
{
    /// <summary>
    /// Provides an implementation of a control flow graph builder that traverses the instructions in a recursive manner,
    /// and determines for each instruction the successors by looking at the general flow control of each instruction.
    /// </summary>
    /// <typeparam name="TInstruction">The type of instructions to store in the control flow graph.</typeparam>
    /// <remarks>
    /// This flow graph builder does <strong>not</strong> do any emulation or data flow analysis. Therefore, this flow
    /// graph builder can only be reliably used when the instructions to graph do not contain any indirect branching
    /// instructions. For example, if we target x86, the instruction <c>jmp 12345678h</c> is possible to process using
    /// this graph builder, but <c>jmp eax</c> is not.
    /// </remarks>
    public class StaticFlowGraphBuilder<TInstruction> : FlowGraphBuilderBase<TInstruction>
    {
        /// <summary>
        /// Creates a new static graph builder using the provided instruction successor resolver.
        /// </summary>
        /// <param name="architecture">The architecture of the instructions to graph.</param>
        /// <param name="successorResolver">The object used to determine the successors of a single instruction.</param>
        /// <exception cref="ArgumentNullException">Occurs when any of the arguments is <c>null</c>.</exception>
        public StaticFlowGraphBuilder(
            IInstructionSetArchitecture<TInstruction> architecture,
            IStaticSuccessorResolver<TInstruction> successorResolver)
            : base(architecture)
        {
            SuccessorResolver = successorResolver ?? throw new ArgumentNullException(nameof(successorResolver));
        }

        /// <summary>
        /// Gets the object used to determine the successors of a single instruction.
        /// </summary>
        public IStaticSuccessorResolver<TInstruction> SuccessorResolver
        {
            get;
        }

        /// <inheritdoc />
        protected override IInstructionTraversalResult<TInstruction> CollectInstructions(
            IInstructionProvider<TInstruction> instructions, long entrypoint, IEnumerable<long> knownBlockHeaders)
        {
            var visited = new HashSet<long>();
            
            var result = new InstructionTraversalResult<TInstruction>();
            result.BlockHeaders.Add(entrypoint);
            result.BlockHeaders.UnionWith(knownBlockHeaders);

            // Start at the entrypoint and block headers.
            var agenda = new Stack<long>();
            foreach (var header in result.BlockHeaders)
                agenda.Push(header);

            while (agenda.Count > 0)
            {
                // Get the current offset to process.
                long currentOffset = agenda.Pop();

                if (visited.Add(currentOffset))
                {
                    // Get the instruction at the provided offset and figure out successors.
                    var instruction = instructions.GetInstructionAtOffset(currentOffset);
                    var currentSuccessors = SuccessorResolver.GetSuccessors(instruction);
                    
                    // Store collected data.
                    result.Instructions.Add(currentOffset, new InstructionInfo<TInstruction>(instruction, currentSuccessors));
                    
                    // Figure out next offsets to process.
                    bool nextInstructionIsSuccessor = false;
                    foreach (long destinationAddress in currentSuccessors
                        .Select(s => s.DestinationAddress)
                        .Distinct())
                    {
                        if (destinationAddress == currentOffset + Architecture.GetSize(instruction))
                        {
                            // Successor is just the next instruction.
                            nextInstructionIsSuccessor = true;
                        }
                        else
                        {
                            // Successor is a jump to another address. This is a new basic block header! 
                            result.BlockHeaders.Add(destinationAddress);
                        }

                        agenda.Push(destinationAddress);
                    }

                    // If we have multiple successors (e.g. as with an if-else construct), or the next instruction is
                    // not a successor (e.g. with a return address), the next instruction is another block header. 
                    if (!nextInstructionIsSuccessor || currentSuccessors.Count > 1)
                        result.BlockHeaders.Add(currentOffset + Architecture.GetSize(instruction));
                    
                }
            }

            return result;
        }
    }
    
}