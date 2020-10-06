using System.Linq;
using dnlib.DotNet;
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
            var method = type.Methods.First(m => m.Name == nameof(SimpleClass.Switch));
            var cfg = method.ConstructStaticFlowGraph();
            
            Assert.Equal(3, cfg.Entrypoint.ConditionalEdges.Count);
        }
    }
}