using dnlib.DotNet;
using Mocks;

namespace Echo.Platforms.Dnlib.Tests
{
    public class CurrentModuleFixture
    {
        public ModuleDef MockModule
        {
            get;
        } = ModuleDefMD.Load(typeof(SimpleClass).Assembly.Location);
    }
}