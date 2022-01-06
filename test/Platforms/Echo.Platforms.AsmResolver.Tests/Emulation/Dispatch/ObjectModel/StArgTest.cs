using System.Linq;
using System.Threading;
using AsmResolver.DotNet;
using AsmResolver.PE.DotNet.Cil;
using Echo.Concrete.Values.ValueType;
using Echo.Platforms.AsmResolver.Emulation;
using Echo.Platforms.AsmResolver.Tests.Mock;
using Xunit;

namespace Echo.Platforms.AsmResolver.Tests.Emulation.Dispatch.ObjectModel
{
    public class StArgTest : DispatcherTestBase
    {
        private readonly TypeDefinition _type;

        public StArgTest(MockModuleFixture moduleFixture)
            : base(moduleFixture)
        {
            var module = ModuleDefinition.FromFile(typeof(StArgTest).Assembly.Location);
            _type = (TypeDefinition) module.LookupMember(typeof(StArgTest).MetadataToken);
        }

        [Fact]
        public void WriteStaticArgs()
        {
            var method = _type.Methods.Single(m => m.Name == nameof(Sum));

            // check that the method contains a StArg instruction, to ensure roslyn didn't optimize it out
            Assert.Contains(method.CilMethodBody.Instructions,
                i => i.OpCode == CilOpCodes.Starg || i.OpCode == CilOpCodes.Starg_S);

            var vm = new CilVirtualMachine(method.CilMethodBody, true);
            var variables = vm.CurrentState.Variables;
            variables[vm.Architecture.GetParameter(method.Parameters[0])] = new Integer32Value(3);
            variables[vm.Architecture.GetParameter(method.Parameters[1])] = new Integer32Value(5);

            var res = vm.Execute(CancellationToken.None);

            Assert.True(res.IsSuccess);
            Assert.Equal(new Integer32Value(Sum(3, 5)), res.ReturnValue);
        }

        [Fact]
        public void WriteInstanceArgs()
        {
            var nestedType = _type.NestedTypes.Single(t => t.Name == nameof(MyClass));
            var method = nestedType.Methods.Single(m => m.Name == nameof(MyClass.Product));

            // check that the method contains a StArg instruction, to ensure roslyn didn't optimize it out
            Assert.Contains(method.CilMethodBody.Instructions,
                i => i.OpCode == CilOpCodes.Starg || i.OpCode == CilOpCodes.Starg_S);

            var vm = new CilVirtualMachine(method.CilMethodBody, true);
            var variables = vm.CurrentState.Variables;
            variables[vm.Architecture.GetParameter(method.Parameters[0])] = new Integer32Value(3);
            variables[vm.Architecture.GetParameter(method.Parameters[1])] = new Integer32Value(5);

            var res = vm.Execute(CancellationToken.None);

            Assert.True(res.IsSuccess);
            Assert.Equal(new Integer32Value(new MyClass().Product(3, 5)), res.ReturnValue);
        }

        // TODO: WriteThisArg test
        // TODO: tests with ref/in/out types

        public static int Sum(int x, int y)
        {
            x += y;
            return x;
        }

        public class MyClass
        {
            public int Product(int x, int y)
            {
                x *= y;
                return x;
            }
        }
    }
}