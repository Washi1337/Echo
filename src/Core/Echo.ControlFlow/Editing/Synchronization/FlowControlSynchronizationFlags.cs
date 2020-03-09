using System;

namespace Echo.ControlFlow.Editing.Synchronization
{
    /// <summary>
    /// Provides flags that dictate the strategy used for pulling updates of a basic block into a control flow graph.
    /// </summary>
    [Flags]
    public enum FlowControlSynchronizationFlags
    {
        /// <summary>
        /// Indicates the synchronizer should only look at changes in the footer of a node in a control flow graph.
        /// </summary>
        TraverseFootersOnly = 0,
        
        /// <summary>
        /// Indicates the synchronizer should traverse the entire basic block of a node in a control flow graph.
        /// </summary>
        TraverseEntireBasicBlock = 1
    }
}