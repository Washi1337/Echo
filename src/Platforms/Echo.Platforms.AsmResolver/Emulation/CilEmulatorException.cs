using System;

namespace Echo.Platforms.AsmResolver.Emulation
{
    [Serializable]
    public class CilEmulatorException : Exception
    {
        public CilEmulatorException()
        {
        }

        public CilEmulatorException(string message)
            : base(message)
        {
        }

        public CilEmulatorException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}