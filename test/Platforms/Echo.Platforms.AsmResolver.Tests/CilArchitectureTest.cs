using System;
using System.Linq;
using AsmResolver.DotNet;
using AsmResolver.PE.DotNet.Cil;
using Xunit;

namespace Echo.Platforms.AsmResolver.Tests
{
    public class CilArchitectureTest
    {
        [Fact]
        public void GetReadParametersInStaticContextShouldStartAtZeroIndex()
        {
            var module = ModuleDefinition.FromFile(typeof(TestClass).Assembly.Location);
            var type = (TypeDefinition) module.LookupMember(typeof(TestClass).MetadataToken);
            var method = type.Methods
                .First(m => m.Name == nameof(TestClass.StaticMethod));
            
            var architecture = new CilArchitecture(method.CilMethodBody);

            Assert.Equal(new[] { method.Parameters[0] }, architecture
                .GetReadVariables(new CilInstruction(CilOpCodes.Ldarg_0))
                .Cast<CilParameter>()
                .Select(p => p.Parameter));
        }
        
        [Fact]
        public void GetReadParametersInInstanceContextShouldStartAtThisParameter()
        {
            var module = ModuleDefinition.FromFile(typeof(TestClass).Assembly.Location);
            var type = (TypeDefinition) module.LookupMember(typeof(TestClass).MetadataToken);
            var method = type.Methods
                .First(m => m.Name == nameof(TestClass.InstanceMethod));
            
            var architecture = new CilArchitecture(method.CilMethodBody);

            Assert.Equal(new[] { method.Parameters.ThisParameter }, architecture
                .GetReadVariables(new CilInstruction(CilOpCodes.Ldarg_0))
                .Cast<CilParameter>()
                .Select(p => p.Parameter));

            Assert.Equal(new[] { method.Parameters[0] }, architecture
                .GetReadVariables(new CilInstruction(CilOpCodes.Ldarg_1))
                .Cast<CilParameter>()
                .Select(p => p.Parameter));
        }
        
        private sealed class TestClass
        {
            public static void StaticMethod(int a1)
            {
            }
            
            public void InstanceMethod(int a1)
            {
            }
        }
    }
}