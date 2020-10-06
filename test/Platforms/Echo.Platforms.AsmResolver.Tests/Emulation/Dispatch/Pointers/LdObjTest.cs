using System.Linq;
using AsmResolver.DotNet;
using AsmResolver.PE.DotNet.Cil;
using Echo.Concrete.Values.ReferenceType;
using Echo.Concrete.Values.ValueType;
using Echo.Platforms.AsmResolver.Emulation;
using Echo.Platforms.AsmResolver.Emulation.Values.Cli;
using Echo.Platforms.AsmResolver.Tests.Mock;
using Mocks;
using Xunit;

namespace Echo.Platforms.AsmResolver.Tests.Emulation.Dispatch.Pointers
{
    public class LdObjTest : DispatcherTestBase
    {
        private readonly TypeDefinition _structType;
        private readonly FieldDefinition _x;
        private readonly FieldDefinition _y;

        public LdObjTest(MockModuleFixture moduleFixture)
            : base(moduleFixture)
        {
            var module = moduleFixture.Module;
            
            _structType = (TypeDefinition) module.LookupMember(typeof(SimpleStruct).MetadataToken);
            _x = _structType.Fields.First(f => f.Name == nameof(SimpleStruct.X));
            _y = _structType.Fields.First(f => f.Name == nameof(SimpleStruct.Y));
        }

        [Fact]
        public void LoadObjectFromUnknownPointerShouldResultInUnknownObjectContents()
        {
            var stack = ExecutionContext.ProgramState.Stack;

            // Push unknown pointer.
            stack.Push(new PointerValue(false));

            var result = Dispatcher.Execute(ExecutionContext, new CilInstruction(CilOpCodes.Ldobj, _structType));
            
            // Check if top of stack is a structure.
            Assert.True(result.IsSuccess);
            Assert.IsAssignableFrom<StructValue>(stack.Top);
            Assert.True(stack.Top.IsValueType);
            
            // Check structure contents.
            var instance = (StructValue) stack.Top;
            Assert.False(instance.GetFieldValue(_x).IsKnown);
            Assert.False(instance.GetFieldValue(_y).IsKnown);
        }
        
        [Fact]
        public void LoadObjectFromKnownPointerShouldResultInSameContents()
        {
            var environment = ExecutionContext.GetService<ICilRuntimeEnvironment>();
            var originalInstance = environment.MemoryAllocator.AllocateObject(_structType.ToTypeSignature());
            originalInstance.SetFieldValue(_x, new Integer32Value(0x12345678));
            originalInstance.SetFieldValue(_y, new Integer32Value(0x12345678, 0xFF00FF00));
            
            var stack = ExecutionContext.ProgramState.Stack;

            stack.Push(new PointerValue((IPointerValue) originalInstance));

            var result = Dispatcher.Execute(ExecutionContext, new CilInstruction(CilOpCodes.Ldobj, _structType));
            
            // Push unknown pointer.
            Assert.True(result.IsSuccess);
            Assert.IsAssignableFrom<StructValue>(stack.Top);
            Assert.True(stack.Top.IsValueType);
            
            // Check structure contents.
            var instance = (StructValue) stack.Top;
            Assert.NotSame(originalInstance, instance);
            Assert.Equal(originalInstance.GetFieldValue(_x), instance.GetFieldValue(_x));
            Assert.Equal(originalInstance.GetFieldValue(_y), instance.GetFieldValue(_y));
        }
        
    }
}