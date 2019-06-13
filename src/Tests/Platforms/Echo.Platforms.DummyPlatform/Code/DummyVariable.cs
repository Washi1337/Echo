using Echo.Core.Code;

namespace Echo.Platforms.DummyPlatform.Code
{
    public class DummyVariable : IVariable
    {
        public DummyVariable(string name)
        {
            Name = name;
        }
        
        public string Name
        {
            get;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}