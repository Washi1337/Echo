using System.Linq;
using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.DotNet.Signatures;
using AsmResolver.PE.DotNet.Cil;
using AsmResolver.PE.DotNet.Metadata.Tables;
using Echo.Platforms.AsmResolver.Tests.Mock;
using Mocks;
using Xunit;

namespace Echo.Platforms.AsmResolver.Tests
{
    public class StaticSuccessorResolverTest : IClassFixture<MockModuleFixture>
    {
        private readonly MockModuleFixture _moduleFixture;

        public StaticSuccessorResolverTest(MockModuleFixture moduleFixture)
        {
            _moduleFixture = moduleFixture;
        }
        
        [Fact]
        public void SingleBlock()
        {
            var type = (TypeDefinition) _moduleFixture.MockModule.LookupMember(typeof(SimpleClass).MetadataToken);
            var method = type.Methods.First(m => m.Name == nameof(SimpleClass.HelloWorld));
            var body = method.CilMethodBody!;
            var cfg = body.ConstructStaticFlowGraph();
            
            Assert.Single(cfg.Nodes);
            Assert.Equal(body.Instructions, cfg.Nodes.GetByOffset(0)!.Contents.Instructions);
        }
        
        [Fact]
        public void If()
        {
            var type = (TypeDefinition) _moduleFixture.MockModule.LookupMember(typeof(SimpleClass).MetadataToken);
            var method = type.Methods.First(m => m.Name == nameof(SimpleClass.If));
            var body = method.CilMethodBody!;
            var cfg = body.ConstructStaticFlowGraph();
            
            Assert.Single(cfg.EntryPoint!.ConditionalEdges);
        }

        [Fact]
        public void Switch()
        {
            var type = (TypeDefinition) _moduleFixture.MockModule.LookupMember(typeof(SimpleClass).MetadataToken);
            var method = type.Methods.First(m => m.Name == nameof(SimpleClass.SwitchColor));
            var body = method.CilMethodBody!;
            var cfg = body.ConstructStaticFlowGraph();
            
            Assert.Equal(3, cfg.EntryPoint!.ConditionalEdges.Count);
        }

        [Fact]
        public void JmpShouldTerminate()
        {
            var method = new MethodDefinition("Dummy", MethodAttributes.Static,
                MethodSignature.CreateStatic(_moduleFixture.MockModule.CorLibTypeFactory.Void));

            method.CilMethodBody = new CilMethodBody
            {
                Instructions =
                {
                    CilOpCodes.Ldc_I4_1,
                    CilOpCodes.Pop,
                    {CilOpCodes.Jmp, method},
                    CilOpCodes.Ldc_I4_2
                }
            };
            method.CilMethodBody.Instructions.CalculateOffsets();

            var cfg = method.CilMethodBody.ConstructStaticFlowGraph();

            var node = Assert.Single(cfg.Nodes);
            Assert.Equal(CilOpCodes.Jmp, node.Contents.Footer!.OpCode);
        }
    }
}