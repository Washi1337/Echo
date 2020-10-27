using dnlib.DotNet.Emit;
using Mocks;
using Xunit;
using IVariable = Echo.Core.Code.IVariable;

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

            var readVariables = new IVariable[1];
            arch.GetReadVariables(testInstruction, readVariables);

            Assert.Equal(parameterName, readVariables[0].Name);
        }
    }
}