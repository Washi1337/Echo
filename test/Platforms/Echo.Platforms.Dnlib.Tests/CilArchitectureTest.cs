using System;
using System.Linq;
using dnlib.DotNet.Emit;
using Xunit;

namespace Echo.Platforms.Dnlib.Tests
{
    public class CilArchitectureTest
    {
        [Theory]
        [InlineData(nameof(TestClass.StaticMethod), "i", 0)]
        [InlineData(nameof(TestClass.InstanceMethod), "", 0)] // "this" param has empty name
        [InlineData(nameof(TestClass.InstanceMethod), "i", 1)]
        public void LdArgInstructionShouldMatchReadParameter(string name, string parameterName, int index)
        {
            var method = Helpers.GetTestMethod(typeof(TestClass), name);
            var arch = new CilArchitecture(method);

            var param = method.Parameters[index];
            var testInstruction = Instruction.Create(OpCodes.Ldarg, param);
            var actualParam = arch.GetReadVariables(testInstruction).Single();

            Assert.Equal(parameterName, actualParam.Name);
        }
    }
}