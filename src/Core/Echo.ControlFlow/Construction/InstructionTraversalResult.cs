using System;
using System.Collections.Generic;
using System.Linq;
using Echo.Core.Code;

namespace Echo.ControlFlow.Construction
{
    /// <summary>
    /// Provides a default implementation of the <see cref="IInstructionTraversalResult{TInstruction}"/> interface,
    /// using a dictionary and a set to store the instructions and block header offsets. 
    /// </summary>
    /// <typeparam name="TInstruction">The type of instructions that were traversed.</typeparam>
    public class InstructionTraversalResult<TInstruction> : IInstructionTraversalResult<TInstruction>
    {
        private readonly IDictionary<long, TInstruction> _instructions = new Dictionary<long, TInstruction>();
        private readonly ISet<long> _fallThroughOffsets = new HashSet<long>();
        private readonly IDictionary<long, IList<SuccessorInfo>> _nonTrivialEdges = new Dictionary<long, IList<SuccessorInfo>>();
        
        /// <summary>
        /// Creates a new instance of the <see cref="InstructionTraversalResult{TInstruction}"/> class.
        /// </summary>
        /// <param name="architecture">The architecture.</param>
        public InstructionTraversalResult(IInstructionSetArchitecture<TInstruction> architecture)
        {
            Architecture = architecture ?? throw new ArgumentNullException(nameof(architecture));
        }

        /// <inheritdoc />
        public IInstructionSetArchitecture<TInstruction> Architecture
        {
            get;
        }

        /// <summary>
        /// Gets a collection of recorded block headers.
        /// </summary>
        public ISet<long> BlockHeaders
        {
            get;
        } = new HashSet<long>();
        
        /// <inheritdoc />
        public TInstruction GetInstructionAtOffset(long offset) => _instructions[offset];

        /// <inheritdoc />
        public bool IsBlockHeader(long offset) => BlockHeaders.Contains(offset);

        /// <inheritdoc />
        public bool ContainsInstruction(long offset) => _instructions.ContainsKey(offset);

        /// <inheritdoc />
        public IEnumerable<TInstruction> GetAllInstructions()
        {
            return _instructions.Values.OrderBy(i => Architecture.GetOffset(i));
        }

        /// <summary>
        /// Adds a single instruction to the traversal result.
        /// </summary>
        /// <param name="instruction">The instruction to add.</param>
        public void AddInstruction(in TInstruction instruction)
        {
            _instructions.Add(Architecture.GetOffset(instruction), instruction);
        }
        
        /// <inheritdoc />
        public int GetSuccessorCount(long offset)
        {
            int count = 0;
            
            if (_fallThroughOffsets.Contains(offset))
                count++;

            if (_nonTrivialEdges.TryGetValue(offset, out var successors))
                count += successors.Count; 
            
            return count;
        }

        /// <inheritdoc />
        public int GetSuccessors(long offset, Span<SuccessorInfo> successorsBuffer)
        {
            int count = 0;
            
            if (_fallThroughOffsets.Contains(offset))
            {
                var instruction = GetInstructionAtOffset(offset);
                long destinationAddress = Architecture.GetOffset(instruction) + Architecture.GetSize(instruction);
                successorsBuffer[count] = new SuccessorInfo(destinationAddress, ControlFlowEdgeType.FallThrough);
                count++;
            }

            if (_nonTrivialEdges.TryGetValue(offset, out var successors))
            {
                for (int i = 0; i < successors.Count; i++, count++)
                    successorsBuffer[count] = successors[i];
            } 

            return count;
        }

        /// <summary>
        /// Clears all registered successors for the provided instruction.
        /// </summary>
        /// <param name="instruction">The instruction.</param>
        public void ClearSuccessors(in TInstruction instruction)
        {
            long offset = Architecture.GetOffset(instruction);
            
            _fallThroughOffsets.Remove(offset);
            if (_nonTrivialEdges.TryGetValue(offset, out var successors))
                successors.Clear();
        }
        
        /// <summary>
        /// Registers a successor for the provided instruction.
        /// </summary>
        /// <param name="instruction">The instruction.</param>
        /// <param name="successorInfo">The successor information.</param>
        public void RegisterSuccessor(in TInstruction instruction, SuccessorInfo successorInfo)
        {
            long offset = Architecture.GetOffset(instruction);
            long size = Architecture.GetSize(instruction);
            
            if (successorInfo.EdgeType == ControlFlowEdgeType.FallThrough
                && offset + size == successorInfo.DestinationAddress)
            {
                // Register the fallthrough successor info.
                _fallThroughOffsets.Add(offset);
            }
            else
            {
                // Check if the successor list already exists (=> this is a revisit of the instruction). 
                if (!_nonTrivialEdges.TryGetValue(offset, out var successors))
                {
                    successors = new List<SuccessorInfo>();
                    _nonTrivialEdges[offset] = successors;
                }
        
                // Add the non-trivial successor info.
                successors.Add(successorInfo);
            }
        }
        
    }
}