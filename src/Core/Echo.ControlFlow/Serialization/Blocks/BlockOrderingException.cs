using System;

namespace Echo.ControlFlow.Serialization.Blocks
{
    [Serializable]
    public class BlockOrderingException : Exception
    {
        public BlockOrderingException()
        {
        }

        public BlockOrderingException(string message)
            : base(message)
        {
        }

        public BlockOrderingException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}