using dnlib.DotNet.Emit;
using Echo.ControlFlow.Construction;
using Echo.DataFlow.Construction;
using Mocks;
using Xunit;

namespace Echo.Platforms.Dnlib.Tests
{
    public class SymbolicFlowGraphTest
    {
        [Fact]
        public void BranchlessMethodShouldHaveSingleBlock()
        {
            var method = Helpers.GetTestMethod(typeof(TestClass), nameof(TestClass.GetConstantString));

            var arch = new CilArchitecture(method);
            var resolver = new CilStateTransitioner(arch);
            var graphBuilder = new SymbolicFlowGraphBuilder<Instruction>(arch, arch.Method.Body.Instructions, resolver);

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
            var resolver = new CilStateTransitioner(arch);
            var graphBuilder = new SymbolicFlowGraphBuilder<Instruction>(arch, arch.Method.Body.Instructions, resolver);

            var graph = graphBuilder.ConstructFlowGraph(0);
            Assert.Equal(2, graph.EntryPoint.OutDegree);
        }

        [Fact]
        public void DataFlowGraphShouldBeCorrectOnConditional()
        {
            var method = Helpers.GetTestMethod(typeof(TestClass), nameof(TestClass.GetIsEvenString));

            var arch = new CilArchitecture(method);
            var resolver = new CilStateTransitioner(arch);
            var graphBuilder = new SymbolicFlowGraphBuilder<Instruction>(arch, arch.Method.Body.Instructions, resolver);
            _ = graphBuilder.ConstructFlowGraph(0);

            var graph = resolver.DataFlowGraph;

            // check that `arg0 % 2` is correctly turned into dfg
            var remNode = Assert.Single(graph.Nodes, n => n.StackDependencies.Count == 2);
            Assert.Single(remNode!.StackDependencies, n => Assert.Single(n)!.Node.Instruction.IsLdarg());
            Assert.Single(remNode!.StackDependencies, n => Assert.Single(n)!.Node.Instruction.IsLdcI4());
            Assert.Single(remNode.GetDependants());
        }
    }
}