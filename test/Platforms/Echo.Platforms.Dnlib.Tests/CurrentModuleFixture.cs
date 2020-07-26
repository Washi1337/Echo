using dnlib.DotNet;

namespace Echo.Platforms.Dnlib.Tests
{
    public class CurrentModuleFixture
    {
        public ModuleDef Module
        {
            get;
        } = ModuleDefMD.Load(typeof(CurrentModuleFixture).Assembly.Location);
    }
}