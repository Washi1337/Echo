using System;

namespace Echo.Core.Emulation
{
    public class StackImbalanceException : Exception
    {
        public StackImbalanceException()
            : this("Stack imbalance was detected.")
        {
        }
        
        public StackImbalanceException(long offset)
            : this($"Stack imbalance was detected at offset {offset:X8}.")
        {
        }
        
        public StackImbalanceException(string message) 
            : base(message)
        {
        }

        public StackImbalanceException(string message, Exception innerException) 
            : base(message, innerException)
        {
        }

        public long Offset
        {
            get;
        }
    }
}