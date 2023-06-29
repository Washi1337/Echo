using System.Collections.Generic;
using System.Linq;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using Mocks;
using Xunit;
using DnlibCode = dnlib.DotNet.Emit.Code;

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
            var type = (TypeDef) _moduleFixture.MockModule.ResolveToken(typeof(SimpleClass).MetadataToken);
            var method = type.Methods.First(m => m.Name == nameof(SimpleClass.HelloWorld));
            var cfg = method.ConstructSymbolicFlowGraph(out _);
            
            Assert.Single(cfg.Nodes);
            Assert.Equal(method.Body.Instructions, cfg.Nodes[0].Contents.Instructions);
        }
        
        [Fact]
        public void If()
        {
            var type = (TypeDef) _moduleFixture.MockModule.ResolveToken(typeof(SimpleClass).MetadataToken);
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
                    .First(i => i.OpCode.Code == DnlibCode.Ldstr && (string) i.Operand == value);
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
            var type = (TypeDef) _moduleFixture.MockModule.ResolveToken(typeof(SimpleClass).MetadataToken);
            var method = type.Methods.First(m => m.Name == nameof(SimpleClass.SwitchColor));
            var cfg = method.ConstructSymbolicFlowGraph(out _);
            
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

            var cfg = method.ConstructSymbolicFlowGraph(out _);

            var node = Assert.Single(cfg.Nodes);
            Assert.Equal(OpCodes.Jmp, node.Contents.Footer.OpCode);
        }
    }
}