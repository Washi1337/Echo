using Echo.Core.Code;

namespace Echo.Core.Tests.Code
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