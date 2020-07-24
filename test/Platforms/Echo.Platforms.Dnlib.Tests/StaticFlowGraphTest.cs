using dnlib.DotNet.Emit;
using Echo.ControlFlow.Construction;
using Echo.ControlFlow.Construction.Static;
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
            Assert.Equal(0, graph.Entrypoint.OutDegree);
        }

        [Fact]
        public void ConditionalBranchShouldHaveTwoOutgoingEdges()
        {
            var method = Helpers.GetTestMethod(typeof(TestClass), nameof(TestClass.GetIsEvenString));

            var arch = new CilArchitecture(method);
            var resolver = new CilStaticSuccessorResolver();
            var graphBuilder = new StaticFlowGraphBuilder<Instruction>(arch, arch.Method.Body.Instructions, resolver);

            var graph = graphBuilder.ConstructFlowGraph(0);
            Assert.Equal(2, graph.Entrypoint.OutDegree);
        }
    }
}