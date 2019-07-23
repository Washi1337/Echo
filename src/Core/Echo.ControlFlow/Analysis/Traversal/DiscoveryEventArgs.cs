using System;

namespace Echo.ControlFlow.Analysis.Traversal
{
    /// <summary>
    /// Provides a base for a discovery event that occurs while traversing a graph.
    /// </summary>
    public abstract class DiscoveryEventArgs : EventArgs
    {
        /// <summary>
        /// Gets or sets a value indicating whether the traversal should continue exploring the current path.
        /// </summary>
        public bool ContinueExploring
        {
            get;
            set;
        } = true;

        /// <summary>
        /// Gets or sets a value indicating whether the traversal should be aborted or not.
        /// </summary>
        public bool Abort
        {
            get;
            set;
        } = false;
    }
}