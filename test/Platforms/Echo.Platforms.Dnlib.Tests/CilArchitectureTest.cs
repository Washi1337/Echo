using System;
using System.Linq;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using Xunit;

namespace Echo.Platforms.Dnlib.Tests
{
    public class CilArchitectureTest
    {
        [Theory]
        [InlineData("StaticMethod", "i", 0)]
        [InlineData("InstanceMethod", "", 0)] // "this" param has empty name
        [InlineData("InstanceMethod", "i", 1)]
        public void LdArgInstructionShouldMatchReadParameter(string name, string parameterName, int index)
        {
            var assembly = AssemblyDef.Load(typeof(TestClass).Assembly.Location);
            var module = assembly.ManifestModule;
            var type = module.GetTypes().Single(t =>
                t.Name == nameof(TestClass) && t.DeclaringType.Name == nameof(CilArchitectureTest));

            var method = type.FindMethod(name);
            var arch = new CilArchitecture(method);

            
            var expectedParam = method.Parameters[index];
            var testInstruction = Instruction.Create(OpCodes.Ldarg, expectedParam);

            var param = arch.GetReadVariables(testInstruction).Single();
            Assert.Equal(param.Name, parameterName);
        }

        // ReSharper disable once MemberCanBePrivate.Global
        public class TestClass
        {
            public static void StaticMethod(int i) => Console.WriteLine(i);
            public void InstanceMethod(int i) => Console.WriteLine(i);
        }
    }
}