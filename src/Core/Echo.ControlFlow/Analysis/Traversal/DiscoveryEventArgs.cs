using System;

namespace Echo.ControlFlow.Analysis.Traversal
{
    public abstract class DiscoveryEventArgs : EventArgs
    {
        public bool ContinueExploring
        {
            get;
            set;
        } = true;

        public bool Abort
        {
            get;
            set;
        } = false;
    }
}