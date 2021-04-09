using System;
using Echo.Core.Emulation;

namespace Echo.Platforms.DummyPlatform.Values
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
        }

        public bool IsKnown
        {
            get;
        }

        public int Size
        {
            get;
        }

        public IValue Copy()
        {
            return new DummyValue(Identifier, IsKnown, Size);
        }

        public override string ToString()
        {
            return $"value_{Identifier} (Known: {IsKnown}, Size: {Size})";
        }

        protected bool Equals(DummyValue other)
        {
            return Identifier == other.Identifier
                   && IsKnown == other.IsKnown 
                   && Size == other.Size;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            if (obj.GetType() != this.GetType())
                return false;
            return Equals((DummyValue) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = Identifier;
                hashCode = (hashCode * 397) ^ IsKnown.GetHashCode();
                hashCode = (hashCode * 397) ^ Size;
                return hashCode;
            }
        }
    }
}