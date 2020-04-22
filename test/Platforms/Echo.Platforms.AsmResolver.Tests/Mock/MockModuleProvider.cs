using System.Collections.Concurrent;
using AsmResolver.DotNet;

namespace Echo.Platforms.AsmResolver.Tests.Mock
{
    public class MockModuleProvider
    {
        private readonly ConcurrentDictionary<AssemblyReference, ModuleDefinition> _modules =
            new ConcurrentDictionary<AssemblyReference, ModuleDefinition>();
            
        public ModuleDefinition GetModule() => 
            GetModule(KnownCorLibs.NetStandard_v2_0_0_0);

        public ModuleDefinition GetModule(AssemblyReference corlibScope)
        {
            ModuleDefinition module = null;

            while (module is null)
            {
                if (!_modules.TryGetValue(corlibScope, out module))
                {
                    var newModule = new ModuleDefinition(
                        $"MockModule_{corlibScope.Name}_{corlibScope.Version}.dll",
                        corlibScope);
                    
                    if (_modules.TryAdd(corlibScope, newModule))
                        module = newModule;
                }
            }

            return module;
        }
    }
}