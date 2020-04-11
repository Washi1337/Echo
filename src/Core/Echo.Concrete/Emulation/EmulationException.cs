using System;
using System.Runtime.Serialization;

namespace Echo.Concrete.Emulation
{
    [Serializable]
    public class EmulationException : Exception
    {
        public EmulationException()
        {
        }

        public EmulationException(string message)
            : base(message)
        {
        }

        public EmulationException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected EmulationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}