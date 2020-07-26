using AsmResolver.DotNet;

namespace Echo.Platforms.AsmResolver.Tests
{
    public class CurrentModuleFixture
    {
        public ModuleDefinition Module
        {
            get;
        } = ModuleDefinition.FromFile(typeof(CurrentModuleFixture).Assembly.Location);
    }
}