using System.Diagnostics.CodeAnalysis;
using System.Threading;
using AsmResolver.DotNet;
using Echo.Concrete.Values.ValueType;
using Echo.Platforms.AsmResolver.Emulation;
using Xunit;

namespace Echo.Platforms.AsmResolver.Tests.Emulation
{
    public class CilVirtualMachineTest 
    {
        public static int TestMethod()
        {
            return 3 + 4;
        }

        public static int TestLoop()
        {
            int result = 0;
            for (int i = 0; i < 10; i++)
                result += i;
            return result;
        }
        
        [Theory]
        [InlineData(nameof(TestMethod))]
        [InlineData(nameof(TestLoop))]
        [SuppressMessage("ReSharper", "PossibleNullReferenceException")]
        public void SimpleMethodBody(string methodName)
        {
            // Look up method.
            var module = ModuleDefinition.FromFile(typeof(CilVirtualMachineTest).Assembly.Location);
            var reflectionMethod = typeof(CilVirtualMachineTest).GetMethod(methodName);
            var asmResMethod = (MethodDefinition) module.LookupMember(reflectionMethod.MetadataToken);
            
            // Create new virtual machine.
            var vm = new CilVirtualMachine(asmResMethod.CilMethodBody, true);
            
            // Execute.
            var result = vm.Execute(CancellationToken.None);
            
            // Inspect return value.
            int expectedResult = (int) reflectionMethod.Invoke(null, null);
            Assert.Equal(new Integer32Value(expectedResult), result.ReturnValue);
        }
        
    }
}