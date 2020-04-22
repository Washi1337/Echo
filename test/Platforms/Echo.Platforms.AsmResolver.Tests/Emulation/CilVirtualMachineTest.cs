using System.Linq;
using System.Threading;
using AsmResolver.DotNet;
using AsmResolver.DotNet.Signatures;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;
using Echo.Concrete.Values.ValueType;
using Echo.Platforms.AsmResolver.Emulation;
using Echo.Platforms.AsmResolver.Tests.Mock;
using Xunit;

namespace Echo.Platforms.AsmResolver.Tests.Emulation
{
    public class CilVirtualMachineTest 
    {
        private static int TestMethod()
        {
            return 3 + 4;
        }
        
        [Fact]
        public void SimpleMethodBody()
        {
            // Look up method.
            var module = ModuleDefinition.FromFile(typeof(CilVirtualMachineTest).Assembly.Location);
            var method = module
                .TopLevelTypes.First(t => t.Name == nameof(CilVirtualMachineTest))
                .Methods.First(m => m.Name == nameof(TestMethod));
            
            // Create new virtual machine.
            var vm = new CilVirtualMachine(method.CilMethodBody, true);
            
            // Execute.
            var result = vm.Execute(CancellationToken.None);
            
            // Inspect return value.
            Assert.Equal(new Integer32Value(7), result.ReturnValue);
        }
    }
}