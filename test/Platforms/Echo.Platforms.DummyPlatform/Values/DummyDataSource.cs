using System;
using Echo.Symbolic.Values;

namespace Echo.Platforms.DummyPlatform.Values
{
    public class DummyDataSource : IDataSource
    {
        private static readonly Random Random = new Random();
        
        public DummyDataSource()
            : this(Random.Next())
        {
        }
        
        public DummyDataSource(int identifier)
        {
            Identifier = identifier;
        }
        
        public int Identifier
        {
            get;
        }

        public override string ToString()
        {
            return "source_" + Identifier;
        }
        
    }
}