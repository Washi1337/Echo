using System;
using System.Buffers;
using System.Collections.Generic;
using Echo.Code;

namespace Echo.ControlFlow.Construction
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
    public class StaticFlowGraphBuilder<TInstruction> : FlowGraphBuilder<TInstruction>
        where TInstruction : notnull
    {
        /// <summary>
        /// Creates a new static graph builder using the provided instruction successor resolver.
        /// </summary>
        /// <param name="architecture">The architecture of the instructions.</param>
        /// <param name="instructions">The instructions to traverse.</param>
        /// <param name="successorResolver">The object used to determine the successors of a single instruction.</param>
        /// <exception cref="ArgumentNullException">Occurs when any of the arguments is <c>null</c>.</exception>
        public StaticFlowGraphBuilder(
            IArchitecture<TInstruction> architecture,
            IEnumerable<TInstruction> instructions,
            IStaticSuccessorResolver<TInstruction> successorResolver)
        {
            Instructions = new ListInstructionProvider<TInstruction>(architecture, instructions);
            SuccessorResolver = successorResolver ?? throw new ArgumentNullException(nameof(successorResolver));
        }

        /// <summary>
        /// Creates a new static graph builder using the provided instruction successor resolver.
        /// </summary>
        /// <param name="instructions">The instructions to traverse.</param>
        /// <param name="successorResolver">The object used to determine the successors of a single instruction.</param>
        /// <exception cref="ArgumentNullException">Occurs when any of the arguments is <c>null</c>.</exception>
        public StaticFlowGraphBuilder(
            IStaticInstructionProvider<TInstruction> instructions,
            IStaticSuccessorResolver<TInstruction> successorResolver)
        {
            Instructions = instructions ?? throw new ArgumentNullException(nameof(instructions));
            SuccessorResolver = successorResolver ?? throw new ArgumentNullException(nameof(successorResolver));
        }

        /// <summary>
        /// Gets the instructions to traverse.
        /// </summary>
        public IStaticInstructionProvider<TInstruction> Instructions
        {
            get;
        }

        /// <inheritdoc />
        public override IArchitecture<TInstruction> Architecture => Instructions.Architecture;

        /// <summary>
        /// Gets the object used to determine the successors of a single instruction.
        /// </summary>
        public IStaticSuccessorResolver<TInstruction> SuccessorResolver
        {
            get;
        }

        /// <inheritdoc />
        protected override IInstructionTraversalResult<TInstruction> CollectInstructions(long entrypoint, IEnumerable<long> knownBlockHeaders)
        {
            var result = new InstructionTraversalResult<TInstruction>(Architecture);
            result.BlockHeaders.Add(entrypoint);
            result.BlockHeaders.UnionWith(knownBlockHeaders);
            
            // Most instructions will have <= 2 successors.
            // - Immediate fallthrough successor or unconditional branch target.
            // - A single conditional branch target.
            
            // The only exception will be switch-like instructions.
            // Therefore we start off by renting a buffer of at least two elements.
            var successorsBuffer = new List<SuccessorInfo>(2);

            var visited = new HashSet<long>();

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
                    // Get the instruction at the provided offset.
                    var instruction = Instructions.GetInstructionAtOffset(currentOffset);
                    
                    // Determine successors.
                    successorsBuffer.Clear();
                    SuccessorResolver.GetSuccessors(instruction, successorsBuffer);

                    // Store collected data.
                    result.AddInstruction(instruction);

                    // Figure out next offsets to process.
                    bool nextInstructionIsSuccessor = false;
                    int realSuccessorCount = 0;
                    for (int i = 0; i < successorsBuffer.Count; i++)
                    {
                        var successor = successorsBuffer[i];
                        long destinationAddress = successor.DestinationAddress;

                        if (!successor.IsRealEdge)
                        {
                            // Successor is implied by the instruction but does not necessarily
                            // transfer control to it directly. Only register the block header. 
                            result.BlockHeaders.Add(destinationAddress);
                            agenda.Push(destinationAddress);
                            continue;
                        }
                        
                        realSuccessorCount++;
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
                        
                        result.RegisterSuccessor(instruction, successor);
                        agenda.Push(destinationAddress);
                    }

                    // If we have multiple successors (e.g. as with an if-else construct), or the next instruction is
                    // not a successor (e.g. with a return address), the next instruction is another block header. 
                    if (!nextInstructionIsSuccessor
                        || realSuccessorCount > 1
                        || (Architecture.GetFlowControl(instruction) & InstructionFlowControl.CanBranch) != 0)
                    {
                        result.BlockHeaders.Add(currentOffset + Architecture.GetSize(instruction));
                    }

                }
            }

            return result;
        }
    }
}