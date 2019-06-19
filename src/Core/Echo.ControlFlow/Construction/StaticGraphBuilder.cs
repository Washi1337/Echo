using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
    public class StaticGraphBuilder<TInstruction> : IGraphBuilder<TInstruction>
        where TInstruction : IInstruction
    {
        /// <summary>
        /// Creates a new static graph builder using the provided instructions and successor resolver.
        /// </summary>
        /// <param name="instructions">The object containing the instructions to graph.</param>
        /// <param name="successorResolver">The object used to determine the successors of a single instruction.</param>
        /// <exception cref="ArgumentNullException">Occurs when any of the arguments is <c>null</c>.</exception>
        public StaticGraphBuilder(IEnumerable<TInstruction> instructions,
            IStaticSuccessorResolver<TInstruction> successorResolver)
            : this(new ListInstructionProvider<TInstruction>(instructions), successorResolver)
        {
        }

        /// <summary>
        /// Creates a new static graph builder using the provided instructions and successor resolver.
        /// </summary>
        /// <param name="provider">The object containing the instructions to graph.</param>
        /// <param name="successorResolver">The object used to determine the successors of a single instruction.</param>
        /// <exception cref="ArgumentNullException">Occurs when any of the arguments is <c>null</c>.</exception>
        public StaticGraphBuilder(IInstructionProvider<TInstruction> provider,
            IStaticSuccessorResolver<TInstruction> successorResolver)
        {
            InstructionProvider = provider ?? throw new ArgumentNullException(nameof(provider));
            SuccessorResolver = successorResolver ?? throw new ArgumentNullException(nameof(successorResolver));
        }

        /// <summary>
        /// Gets the object used to obtain the instructions to put in the control flow graph. 
        /// </summary>
        public IInstructionProvider<TInstruction> InstructionProvider
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

        /// <inheritdoc />
        public Graph<TInstruction> ConstructFlowGraph(long entrypoint)
        {
            CollectInstructions(entrypoint, out var instructions, out var blockHeaders);

            var graph = new Graph<TInstruction>();
            CreateNodes(graph, instructions, blockHeaders);
            ConnectNodes(graph, instructions);
            graph.Entrypoint = graph.GetNodeByOffset(entrypoint);
            return graph;
        }

        private void CollectInstructions(long entrypoint, out IDictionary<long, InstructionInfo<TInstruction>> instructions, 
            out HashSet<long> blockHeaders)
        {
            var visited = new HashSet<long>();
            instructions = new Dictionary<long, InstructionInfo<TInstruction>>();
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
                    var instruction = InstructionProvider.GetInstructionAtOffset(currentOffset);
                    var currentSuccessors = SuccessorResolver.GetSuccessors(instruction);
                    
                    // Store collected data.
                    instructions.Add(currentOffset, new InstructionInfo<TInstruction>(instruction, currentSuccessors));
                    
                    // Figure out next offsets to process.
                    bool nextInstructionIsSuccessor = false;
                    foreach (var destinationAddress in currentSuccessors
                        .Select(s => s.DestinationAddress)
                        .Distinct())
                    {
                        if (destinationAddress == currentOffset + instruction.Size)
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
                        blockHeaders.Add(currentOffset + instruction.Size);
                    
                }
            }
        }

        private static void CreateNodes(Graph<TInstruction> graph, IDictionary<long, InstructionInfo<TInstruction>> instructions, 
            ICollection<long> blockHeaders)
        {
            Node<TInstruction> currentNode = null;
            foreach (var info in instructions.Values.OrderBy(t => t.Instruction.Offset))
            {
                // Check if we reached a new block header.
                if (currentNode == null || blockHeaders.Contains(info.Instruction.Offset))
                {
                    // We arrived at a new basic block header. Create a new node for it. 
                    currentNode = new Node<TInstruction>();
                    graph.Nodes.Add(currentNode);
                }

                // Add the current instruction to the current block that we're populating.
                currentNode.Instructions.Add(info.Instruction);

                // Edge case: Blocks might not come straight after each other. If we see that the next instruction
                // does not exist, or the previous algorithm has decided the next instruction is a block header, 
                // we can create a new block in the next iteration.
                long nextOffset = info.Instruction.Offset + info.Instruction.Size;
                if (!instructions.ContainsKey(nextOffset) || blockHeaders.Contains(nextOffset))
                    currentNode = null;
            }
        }

        private static void ConnectNodes(Graph<TInstruction> graph, IDictionary<long, InstructionInfo<TInstruction>> instructions)
        {
            foreach (var node in graph.Nodes)
            {
                // Get the successors of the last instruction in the current block.
                var last = node.Instructions[node.Instructions.Count - 1];
                var successors = instructions[last.Offset].Successors;

                // Add edges accordingly.
                foreach (var successor in successors)
                {
                    var successorNode = graph.GetNodeByOffset(successor.DestinationAddress);
                    if (successorNode == null)
                    {
                        throw new ArgumentException(
                            $"Instruction at address {last.Offset:X8} refers to a non-existing node.");
                    }

                    node.ConnectWith(successorNode, successor.EdgeType);
                }
            }
        }
    }
    
}