using System.Collections.Generic;
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
            var body = method.CilMethodBody!;
            var cfg = body.ConstructSymbolicFlowGraph(out _);
            
            Assert.Single(cfg.Nodes);
            Assert.Equal(body.Instructions, cfg.Nodes.GetByOffset(0)!.Contents.Instructions);
        }
        
        [Fact]
        public void If()
        {
            var type = (TypeDefinition) _moduleFixture.MockModule.LookupMember(typeof(SimpleClass).MetadataToken);
            var method = type.Methods.First(m => m.Name == nameof(SimpleClass.If));
            var body = method.CilMethodBody!;
            var cfg = body.ConstructSymbolicFlowGraph(out var dfg);
            var offsetMap = dfg.Nodes.CreateOffsetMap();
            
            Assert.Single(cfg.EntryPoint!.ConditionalEdges);
            
            var ldstrAdult = FindLdstr("Adult");
            var ldstrChild = FindLdstr("Child");
            var ret = body.Instructions[^1];
            Assert.True(ValueReachesOffset(ldstrAdult.Offset, ret.Offset));
            Assert.True(ValueReachesOffset(ldstrChild.Offset, ret.Offset));

            CilInstruction FindLdstr(string value)
            {
                return body.Instructions
                    .First(i => i.OpCode.Code == CilCode.Ldstr && i.Operand?.ToString() == value);
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
                    
                    var current = offsetMap[currentOffset];
                    foreach (var dependant in current.GetDependants())
                        agenda.Push(dependant.Offset);
                }

                return false;
            }
        }

        [Fact]
        public void JmpShouldTerminate()
        {
            var method = new MethodDefinition("Dummy", MethodAttributes.Static,
                MethodSignature.CreateStatic(_moduleFixture.MockModule.CorLibTypeFactory.Void));

            method.CilMethodBody = new CilMethodBody(method)
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

            var cfg = method.CilMethodBody.ConstructSymbolicFlowGraph(out _);

            var node = Assert.Single(cfg.Nodes);
            Assert.Equal(CilOpCodes.Jmp, node.Contents.Footer!.OpCode);
        }
    }
}