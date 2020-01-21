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
        public SuccessorInfo(long destinationAddress, EdgeType edgeType)
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
        public EdgeType EdgeType
        {
            get;
        }
    }
}