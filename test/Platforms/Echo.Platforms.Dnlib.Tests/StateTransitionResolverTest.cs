using System.Collections.Generic;
using System.IO;
using System.Linq;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using Echo.ControlFlow.Serialization.Dot;
using Echo.Core.Graphing.Serialization.Dot;
using Echo.DataFlow.Serialization.Dot;
using Echo.Platforms.Dnlib.Tests.Mock;
using Xunit;

namespace Echo.Platforms.Dnlib.Tests
{
    public class StateTransitionResolverTest : IClassFixture<CurrentModuleFixture>
    {
        private readonly CurrentModuleFixture _moduleFixture;

        public StateTransitionResolverTest(CurrentModuleFixture moduleFixture)
        {
            _moduleFixture = moduleFixture;
        }
        
        [Fact]
        public void SingleBlock()
        {
            var type = (TypeDef) _moduleFixture.Module.ResolveToken(typeof(SimpleClass).MetadataToken);
            var method = type.Methods.First(m => m.Name == nameof(SimpleClass.HelloWorld));
            var cfg = method.ConstructSymbolicFlowGraph(out _);
            
            Assert.Single(cfg.Nodes);
            Assert.Equal(method.Body.Instructions, cfg.Nodes[0].Contents.Instructions);
        }
        
        [Fact]
        public void If()
        {
            var type = (TypeDef) _moduleFixture.Module.ResolveToken(typeof(SimpleClass).MetadataToken);
            var method = type.Methods.First(m => m.Name == nameof(SimpleClass.If));
            var cfg = method.ConstructSymbolicFlowGraph(out var dfg);
            
            Assert.Single(cfg.Entrypoint.ConditionalEdges);
            
            var ldstrAdult = FindLdstr("Adult");
            var ldstrChild = FindLdstr("Child");
            var ret = method.Body.Instructions[^1];
            Assert.True(ValueReachesOffset(ldstrAdult.Offset, ret.Offset));
            Assert.True(ValueReachesOffset(ldstrChild.Offset, ret.Offset));

            Instruction FindLdstr(string value)
            {
                return method.Body.Instructions
                    .First(i => i.OpCode.Code == Code.Ldstr && (string) i.Operand == value);
            }

            bool ValueReachesOffset(uint sourceOffset, uint targetOffset)
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
            var type = (TypeDef) _moduleFixture.Module.ResolveToken(typeof(SimpleClass).MetadataToken);
            var method = type.Methods.First(m => m.Name == nameof(SimpleClass.Switch));
            var cfg = method.ConstructSymbolicFlowGraph(out _);
            
            Assert.Equal(3, cfg.Entrypoint.ConditionalEdges.Count);
        }
    }
}