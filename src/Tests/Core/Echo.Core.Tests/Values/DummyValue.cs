using System;
using Echo.Core.Values;

namespace Echo.Core.Tests.Values
{
    public class DummyValue : IValue
    {
        private static readonly Random Random = new Random();
        
        public DummyValue()
            : this(Random.Next(), true, 1)
        {
        }

        public DummyValue(int identifier)
            : this(identifier, true, 1)
        {
        }

        public DummyValue(int identifier, bool isKnown, int size)
        {
            Identifier = identifier;
            IsKnown = isKnown;
            Size = size;
        }

        public int Identifier
        {
            get;
            set;
        }

        public bool IsKnown
        {
            get;
        }

        public int Size
        {
            get;
        }

        public override string ToString()
        {
            return $"value_{Identifier} (Known: {IsKnown}, Size: {Size})";
        }
    }
}