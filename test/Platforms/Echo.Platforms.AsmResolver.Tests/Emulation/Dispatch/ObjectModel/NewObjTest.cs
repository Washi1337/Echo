using System.Linq;
using AsmResolver.DotNet;
using AsmResolver.PE.DotNet.Cil;
using Echo.Platforms.AsmResolver.Emulation.Values.Cli;
using Echo.Platforms.AsmResolver.Tests.Mock;
using Xunit;

namespace Echo.Platforms.AsmResolver.Tests.Emulation.Dispatch.ObjectModel
{
    public class NewObjTest : DispatcherTestBase
    {
        public NewObjTest(MockModuleFixture moduleFixture)
            : base(moduleFixture)
        {
        }
        
        [Fact]
        public void NewObjParameterLess()
        {
            var module = ModuleDefinition.FromFile(typeof(NewObjTest).Assembly.Location);
            var type = (TypeDefinition)module.LookupMember(typeof(Constr).MetadataToken);
            var method = type.Methods.First(m => m.IsConstructor);

            var res = Dispatcher.Execute(ExecutionContext, new CilInstruction(CilOpCodes.Newobj,method));
            Assert.True(res.IsSuccess);
            Assert.Equal(1, ExecutionContext.ProgramState.Stack.Size);
            Assert.IsAssignableFrom<OValue>(ExecutionContext.ProgramState.Stack.Top);
        }
        
        [Fact]
        public void NewObjParameter()
        {
            var module = ModuleDefinition.FromFile(typeof(NewObjTest).Assembly.Location);
            var type = (TypeDefinition)module.LookupMember(typeof(Constr).MetadataToken);

            ExecutionContext.ProgramState.Stack.Push(new I4Value(1));
            var method = type.Methods.First(q => q.IsConstructor && q.Parameters.Count == 1);
            var res = Dispatcher.Execute(ExecutionContext, new CilInstruction(CilOpCodes.Newobj,method));
            
            Assert.True(res.IsSuccess);
            Assert.Equal(1, ExecutionContext.ProgramState.Stack.Size);
            Assert.IsAssignableFrom<OValue>(ExecutionContext.ProgramState.Stack.Top);
        }

        internal class Constr
        {
            public Constr()
            {
            }

            public Constr(int param)
            {
                
            }
        }
    }
}