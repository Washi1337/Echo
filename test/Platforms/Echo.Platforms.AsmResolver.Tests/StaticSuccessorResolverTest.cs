using System.Linq;
using AsmResolver.DotNet;
using Echo.Platforms.AsmResolver.Tests.Emulation.Dispatch;
using Echo.Platforms.AsmResolver.Tests.Mock;
using Mocks;
using Xunit;

namespace Echo.Platforms.AsmResolver.Tests
{
    public class StaticSuccessorResolverTest : DispatcherTestBase
    {
        private readonly MockModuleFixture _moduleFixture;

        public StaticSuccessorResolverTest(MockModuleFixture moduleFixture)
            : base(moduleFixture)
        {
            _moduleFixture = moduleFixture;
        }
        
        [Fact]
        public void SingleBlock()
        {
            var type = (TypeDefinition) _moduleFixture.MockModule.LookupMember(typeof(SimpleClass).MetadataToken);
            var method = type.Methods.First(m => m.Name == nameof(SimpleClass.HelloWorld));
            var body = method.CilMethodBody;
            var cfg = body.ConstructStaticFlowGraph();
            
            Assert.Single(cfg.Nodes);
            Assert.Equal(body.Instructions, cfg.Nodes[0].Contents.Instructions);
        }
        
        [Fact]
        public void If()
        {
            var type = (TypeDefinition) _moduleFixture.MockModule.LookupMember(typeof(SimpleClass).MetadataToken);
            var method = type.Methods.First(m => m.Name == nameof(SimpleClass.If));
            var body = method.CilMethodBody;
            var cfg = body.ConstructStaticFlowGraph();
            
            Assert.Single(cfg.Entrypoint.ConditionalEdges);
        }

        [Fact]
        public void Switch()
        {
            var type = (TypeDefinition) _moduleFixture.MockModule.LookupMember(typeof(SimpleClass).MetadataToken);
            var method = type.Methods.First(m => m.Name == nameof(SimpleClass.Switch));
            var body = method.CilMethodBody;
            var cfg = body.ConstructStaticFlowGraph();
            
            Assert.Equal(3, cfg.Entrypoint.ConditionalEdges.Count);
        }
    }
}