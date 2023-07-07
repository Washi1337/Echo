using System.Linq;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using Mocks;
using Xunit;

namespace Echo.Platforms.Dnlib.Tests
{
    public class StaticSuccessorResolverTest : IClassFixture<CurrentModuleFixture>
    {
        private readonly CurrentModuleFixture _moduleFixture;

        public StaticSuccessorResolverTest(CurrentModuleFixture moduleFixture)
        {
            _moduleFixture = moduleFixture;
        }
        
        [Fact]
        public void SingleBlock()
        {
            var type = (TypeDef) _moduleFixture.MockModule.ResolveToken(typeof(SimpleClass).MetadataToken);
            var method = type.Methods.First(m => m.Name == nameof(SimpleClass.HelloWorld));
            var cfg = method.ConstructStaticFlowGraph();
            
            Assert.Single(cfg.Nodes);
            Assert.Equal(method.Body.Instructions, cfg.Nodes[0].Contents.Instructions);
        }
        
        [Fact]
        public void If()
        {
            var type = (TypeDef) _moduleFixture.MockModule.ResolveToken(typeof(SimpleClass).MetadataToken);
            var method = type.Methods.First(m => m.Name == nameof(SimpleClass.If));
            var cfg = method.ConstructStaticFlowGraph();
            
            Assert.Single(cfg.Entrypoint.ConditionalEdges);
        }

        [Fact]
        public void Switch()
        {
            var type = (TypeDef) _moduleFixture.MockModule.ResolveToken(typeof(SimpleClass).MetadataToken);
            var method = type.Methods.First(m => m.Name == nameof(SimpleClass.SwitchColor));
            var cfg = method.ConstructStaticFlowGraph();
            
            Assert.Equal(3, cfg.Entrypoint.ConditionalEdges.Count);
        }
        
        [Fact]
        public void JmpShouldTerminate()
        {
            var method = new MethodDefUser("Dummy", MethodSig.CreateStatic(_moduleFixture.MockModule.CorLibTypes.Void), MethodAttributes.Static);

            method.Body = new CilBody
            {
                Instructions =
                {
                    Instruction.Create(OpCodes.Ldc_I4_1),
                    Instruction.Create(OpCodes.Pop),
                    Instruction.Create(OpCodes.Jmp, method),
                    Instruction.Create(OpCodes.Ldc_I4_2)
                }
            };
            method.Body.UpdateInstructionOffsets();

            var cfg = method.ConstructStaticFlowGraph();

            var node = Assert.Single(cfg.Nodes);
            Assert.Equal(OpCodes.Jmp, node.Contents.Footer.OpCode);
        }
    }
}