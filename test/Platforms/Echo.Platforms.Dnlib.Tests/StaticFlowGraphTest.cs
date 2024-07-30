using dnlib.DotNet;
using dnlib.DotNet.Emit;
using Echo.ControlFlow.Construction;
using Echo.ControlFlow.Regions;
using Mocks;
using Xunit;

namespace Echo.Platforms.Dnlib.Tests
{
    public class StaticFlowGraphTest
    {
        [Fact]
        public void BranchlessMethodShouldHaveSingleBlock()
        {
            var method = Helpers.GetTestMethod(typeof(TestClass), nameof(TestClass.GetConstantString));

            var arch = new CilArchitecture(method);
            var resolver = new CilStaticSuccessorResolver();
            var graphBuilder = new StaticFlowGraphBuilder<Instruction>(arch, arch.Method.Body.Instructions, resolver);

            var graph = graphBuilder.ConstructFlowGraph(0);
            Assert.Single(graph.Nodes);
            Assert.Empty(graph.GetEdges());
            Assert.Equal(0, graph.EntryPoint.OutDegree);
        }

        [Fact]
        public void ConditionalBranchShouldHaveTwoOutgoingEdges()
        {
            var method = Helpers.GetTestMethod(typeof(TestClass), nameof(TestClass.GetIsEvenString));

            var arch = new CilArchitecture(method);
            var resolver = new CilStaticSuccessorResolver();
            var graphBuilder = new StaticFlowGraphBuilder<Instruction>(arch, arch.Method.Body.Instructions, resolver);

            var graph = graphBuilder.ConstructFlowGraph(0);
            Assert.Equal(2, graph.EntryPoint.OutDegree);
        }

        [Fact]
        public void ExceptionHandlerWithHandlerEndNull()
        {
            // https://github.com/Washi1337/Echo/issues/101

            // Set up test case.
            var module = new ModuleDefUser("Module");
            var method = new MethodDefUser("MyMethod", MethodSig.CreateStatic(module.CorLibTypes.Void), MethodAttributes.Static);
            var body = method.Body = new CilBody();

            var start = Instruction.Create(OpCodes.Nop);
            var tryStart = Instruction.Create(OpCodes.Nop);
            var handlerStart = Instruction.Create(OpCodes.Pop);
            body.Instructions.Add(start);
            body.Instructions.Add(tryStart);
            body.Instructions.Add(Instruction.Create(OpCodes.Leave_S, start));
            body.Instructions.Add(handlerStart);
            body.Instructions.Add(Instruction.Create(OpCodes.Leave_S, start));

            body.ExceptionHandlers.Add(new ExceptionHandler(ExceptionHandlerType.Catch)
            {
                TryStart = tryStart,
                TryEnd = handlerStart,
                HandlerStart = handlerStart,
                HandlerEnd = null, // End of method.
                CatchType = module.CorLibTypes.Object.ToTypeDefOrRef()
            });

            body.UpdateInstructionOffsets();

            // Verify.
            var cfg = method.ConstructStaticFlowGraph();
            Assert.NotNull(cfg);
            Assert.Equal(3, cfg.Nodes.Count);
            var region = Assert.Single(cfg.Regions);
            var eh = Assert.IsAssignableFrom<ExceptionHandlerRegion<Instruction>>(region);
            Assert.Single(eh.Handlers);
        }
    }
}