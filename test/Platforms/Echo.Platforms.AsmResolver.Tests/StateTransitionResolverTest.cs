using System.Collections.Generic;
using System.Linq;
using AsmResolver.DotNet;
using AsmResolver.PE.DotNet.Cil;
using Echo.Platforms.AsmResolver.Tests.Mock;
using Mocks;
using Xunit;

namespace Echo.Platforms.AsmResolver.Tests
{
    public class StateTransitionResolverTest : IClassFixture<MockModuleFixture>
    {
        private readonly MockModuleFixture _moduleFixture;

        public StateTransitionResolverTest(MockModuleFixture moduleFixture)
        {
            _moduleFixture = moduleFixture;
        }
        
        [Fact]
        public void SingleBlock()
        {
            var type = (TypeDefinition) _moduleFixture.MockModule.LookupMember(typeof(SimpleClass).MetadataToken);
            var method = type.Methods.First(m => m.Name == nameof(SimpleClass.HelloWorld));
            var body = method.CilMethodBody;
            var cfg = body.ConstructSymbolicFlowGraph(out _);
            
            Assert.Single(cfg.Nodes);
            Assert.Equal(body.Instructions, cfg.Nodes[0].Contents.Instructions);
        }
        
        [Fact]
        public void If()
        {
            var type = (TypeDefinition) _moduleFixture.MockModule.LookupMember(typeof(SimpleClass).MetadataToken);
            var method = type.Methods.First(m => m.Name == nameof(SimpleClass.If));
            var body = method.CilMethodBody;
            var cfg = body.ConstructSymbolicFlowGraph(out var dfg);
            
            Assert.Single(cfg.Entrypoint.ConditionalEdges);
            
            var ldstrAdult = FindLdstr("Adult");
            var ldstrChild = FindLdstr("Child");
            var ret = body.Instructions[^1];
            Assert.True(ValueReachesOffset(ldstrAdult.Offset, ret.Offset));
            Assert.True(ValueReachesOffset(ldstrChild.Offset, ret.Offset));

            CilInstruction FindLdstr(string value)
            {
                return body.Instructions
                    .First(i => i.OpCode.Code == CilCode.Ldstr && (string) i.Operand == value);
            }

            bool ValueReachesOffset(int sourceOffset, int targetOffset)
            {
                var visited = new HashSet<long>();
                var agenda = new Stack<long>();
                agenda.Push(sourceOffset);

                while (agenda.Count > 0)
                {
                    long currentOffset = agenda.Pop();
                    
                    if (currentOffset == targetOffset)
                        return true;
                    if (!visited.Add(currentOffset))
                        continue;
                    
                    var current = dfg.Nodes[currentOffset];
                    foreach (var dependant in current.GetDependants())
                        agenda.Push(dependant.Id);
                }

                return false;
            }
        }

        [Fact]
        public void Switch()
        {
            var type = (TypeDefinition) _moduleFixture.MockModule.LookupMember(typeof(SimpleClass).MetadataToken);
            var method = type.Methods.First(m => m.Name == nameof(SimpleClass.Switch));
            var body = method.CilMethodBody;
            var cfg = body.ConstructSymbolicFlowGraph(out _);
            
            Assert.Equal(3, cfg.Entrypoint.ConditionalEdges.Count);
        }
    }
}