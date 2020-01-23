using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Echo.ControlFlow.Specialized;
using Echo.ControlFlow.Specialized.Blocks;
using Echo.Core.Code;

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
    public class StaticFlowGraphBuilder<TInstruction> : IFlowGraphBuilder<TInstruction>
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
        {
            Architecture = architecture;
            SuccessorResolver = successorResolver ?? throw new ArgumentNullException(nameof(successorResolver));
        }

        /// <summary>
        /// Gets the architecture of the instructions to graph.
        /// </summary>
        public IInstructionSetArchitecture<TInstruction> Architecture
        {
            get;
        }

        /// <summary>
        /// Gets the object used to determine the successors of a single instruction.
        /// </summary>
        public IStaticSuccessorResolver<TInstruction> SuccessorResolver
        {
            get;
        }

        /// <summary>
        /// Constructs a control flow graph in a static analysis manner, starting at the provided entrypoint address.
        /// </summary>
        /// <param name="instructions">The instructions</param>
        /// <param name="entrypoint">The address of the first instruction to traverse.</param>
        /// <returns>
        /// The constructed control flow graph, with the entrypoint set to the node containing the entrypoint address
        /// provided in <paramref name="entrypoint"/>.
        /// </returns>
        public ControlFlowGraph<TInstruction> ConstructFlowGraph(IEnumerable<TInstruction> instructions, long entrypoint)
        {
            return ConstructFlowGraph(new ListInstructionProvider<TInstruction>(Architecture, instructions.ToList()), entrypoint);
        }
        
        /// <summary>
        /// Constructs a control flow graph in a static analysis manner, starting at the provided entrypoint address.
        /// </summary>
        /// <param name="instructions">The instructions</param>
        /// <param name="entrypoint">The address of the first instruction to traverse.</param>
        /// <returns>
        /// The constructed control flow graph, with the entrypoint set to the node containing the entrypoint address
        /// provided in <paramref name="entrypoint"/>.
        /// </returns>
        public ControlFlowGraph<TInstruction> ConstructFlowGraph(IInstructionProvider<TInstruction> instructions, long entrypoint)
        {
            CollectInstructions(instructions, entrypoint, out var instructionInfos, out var blockHeaders);

            var graph = new ControlFlowGraph<TInstruction>();
            CreateNodes(graph, instructionInfos, blockHeaders);
            ConnectNodes(graph, instructionInfos);
            graph.Entrypoint = graph.GetNodeByOffset(entrypoint);
            return graph;
        }

        private void CollectInstructions(IInstructionProvider<TInstruction> instructions, long entrypoint, 
            out IDictionary<long, InstructionInfo<TInstruction>> instructionInfos, 
            out HashSet<long> blockHeaders)
        {
            var visited = new HashSet<long>();
            instructionInfos = new Dictionary<long, InstructionInfo<TInstruction>>();
            blockHeaders = new HashSet<long> {entrypoint};

            // Start at the entrypoint.
            var agenda = new Stack<long>();
            agenda.Push(entrypoint);

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
                    instructionInfos.Add(currentOffset, new InstructionInfo<TInstruction>(instruction, currentSuccessors));
                    
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
                            blockHeaders.Add(destinationAddress);
                        }

                        agenda.Push(destinationAddress);
                    }

                    // If we have multiple successors (e.g. as with an if-else construct), or the next instruction is
                    // not a successor (e.g. with a return address), the next instruction is another block header. 
                    if (!nextInstructionIsSuccessor || currentSuccessors.Count > 1)
                        blockHeaders.Add(currentOffset + Architecture.GetSize(instruction));
                    
                }
            }
        }

        private void CreateNodes(ControlFlowGraph<TInstruction> graph, IDictionary<long, InstructionInfo<TInstruction>> instructions, 
            ICollection<long> blockHeaders)
        {
            BasicBlockNode<TInstruction> currentNode = null;
            foreach (var info in instructions.Values.OrderBy(t => Architecture.GetOffset(t.Instruction)))
            {
                // Check if we reached a new block header.
                long offset = Architecture.GetOffset(info.Instruction);
                if (currentNode == null || blockHeaders.Contains(offset))
                {
                    // We arrived at a new basic block header. Create a new node for it. 
                    currentNode = new BasicBlockNode<TInstruction>(new BasicBlock<TInstruction>(offset));
                    graph.Nodes.Add(currentNode);
                }

                // Add the current instruction to the current block that we're populating.
                currentNode.Contents.Instructions.Add(info.Instruction);

                // Edge case: Blocks might not come straight after each other. If we see that the next instruction
                // does not exist, or the previous algorithm has decided the next instruction is a block header, 
                // we can create a new block in the next iteration.
                long nextOffset = offset + Architecture.GetSize(info.Instruction);
                if (!instructions.ContainsKey(nextOffset) || blockHeaders.Contains(nextOffset))
                    currentNode = null;
            }
        }

        private void ConnectNodes(ControlFlowGraph<TInstruction> graph, IDictionary<long, InstructionInfo<TInstruction>> instructions)
        {
            foreach (var node in graph.Nodes)
            {
                // Get the successors of the last instruction in the current block.
                var block = node.Contents;
                long footerOffset = Architecture.GetOffset(block.Instructions[block.Instructions.Count - 1]);
                var successors = instructions[footerOffset].Successors;

                // Add edges accordingly.
                foreach (var successor in successors)
                {
                    var successorNode = graph.GetNodeByOffset(successor.DestinationAddress);
                    if (successorNode == null)
                    {
                        throw new ArgumentException(
                            $"Instruction at address {footerOffset:X8} refers to a non-existing node.");
                    }

                    node.ConnectWith(successorNode, successor.EdgeType);
                }
            }
        }
    }
    
}