namespace Echo.ControlFlow.Construction
{
    /// <summary>
    /// Represents a reference to an instruction that is the successor of another instruction. 
    /// </summary>
    public readonly struct SuccessorInfo
    {
        /// <summary>
        /// Creates a new successor reference.
        /// </summary>
        /// <param name="destinationAddress">The address of the successor instruction.</param>
        /// <param name="edgeType">The type of control flow transfer that has to be made to go to this successor.</param>
        public SuccessorInfo(long destinationAddress, ControlFlowEdgeType edgeType)
        {
            DestinationAddress = destinationAddress;
            EdgeType = edgeType;
        }

        /// <summary>
        /// Gets the address of the successor instruction.
        /// </summary>
        public long DestinationAddress
        {
            get;
        }
        
        /// <summary>
        /// Gets the type of edge that would be introduced if this control flow transfer was included in a
        /// control flow graph. 
        /// </summary>
        public ControlFlowEdgeType EdgeType
        {
            get;
        }

        /// <summary>
        /// Gets whether the edge is a real edge (not <see cref="ControlFlowEdgeType.None"/>).
        /// </summary>
        public bool IsRealEdge => EdgeType != ControlFlowEdgeType.None;

        /// <inheritdoc />
        public override string ToString() => 
            $"{DestinationAddress:X8} ({EdgeType})";
    }
}