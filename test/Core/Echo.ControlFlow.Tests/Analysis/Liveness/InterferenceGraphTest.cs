using System;
using System.Linq;
using Echo.ControlFlow.Analysis.Liveness;
using Echo.ControlFlow.Blocks;
using Echo.Platforms.DummyPlatform.Code;
using Xunit;

namespace Echo.ControlFlow.Tests.Analysis.Liveness;

public class InterferenceGraphTest
{
    [Fact]
    public void AddSameVariable()
    {
        var v1 = new DummyVariable("v1");
        
        var graph = new InterferenceGraph();
        var node = graph.Nodes.GetOrAdd(v1);
        Assert.Same(node, graph.Nodes.GetOrAdd(v1));
        Assert.Single(graph.Nodes);
    }
    
    [Fact]
    public void AddNodeSameVariable()
    {
        var v1 = new DummyVariable("v1");
        
        var graph = new InterferenceGraph();
        var node = graph.Nodes.GetOrAdd(v1);
        Assert.Throws<ArgumentException>(() => graph.Nodes.Add(new InterferenceNode(v1)));
    }
    
    [Fact]
    public void AddDifferentVariable()
    {
        var v1 = new DummyVariable("v1");
        var v2 = new DummyVariable("v2");
        
        var graph = new InterferenceGraph();
        var n1 = graph.Nodes.GetOrAdd(v1);
        var n2 = graph.Nodes.GetOrAdd(v2);
        Assert.NotSame(n1, n2);
        Assert.Equal(2, graph.Nodes.Count);
    }

    [Fact]
    public void AddNodeThatIsAlreadyAddedToAnotherGraphShouldThrow()
    {
        var v1 = new DummyVariable("v1");
        var g1 = new InterferenceGraph();
        var g2 = new InterferenceGraph();

        var n1 = g1.Nodes.GetOrAdd(v1);
        Assert.Throws<ArgumentException>(() => g2.Nodes.Add(n1));
    }

    [Fact]
    public void AddInterferenceToNodeWithNoParentGraphShouldThrow()
    {
        var n1 = new InterferenceNode(new DummyVariable("v1"));
        var n2 = new InterferenceNode(new DummyVariable("v2"));

        Assert.Throws<InvalidOperationException>(() => n1.Interference.Add(n2));
    }

    [Fact]
    public void AddInterferenceShouldAddToBothEnds()
    {
        var v1 = new DummyVariable("v1");
        var v2 = new DummyVariable("v2");
        
        var graph = new InterferenceGraph();

        var n1 = graph.Nodes.GetOrAdd(v1);
        var n2 = graph.Nodes.GetOrAdd(v2);

        n1.Interference.Add(n2);
        Assert.Same(n2, Assert.Single(n1.Interference));
        Assert.Same(n1, Assert.Single(n2.Interference));
    }

    [Fact]
    public void RemoveShouldRemoveFromNeighbors()
    {
        var graph = new InterferenceGraph();

        var n1 = graph.Nodes.GetOrAdd(new DummyVariable("v1"));
        var n2 = graph.Nodes.GetOrAdd(new DummyVariable("v2"));
        var n3 = graph.Nodes.GetOrAdd(new DummyVariable("v3"));
        n3.Interference.Add(n1);
        n3.Interference.Add(n2);

        Assert.Equal([n1, n2], n3.Interference.ToHashSet());
        
        graph.Nodes.Remove(n1);
        
        Assert.Same(n2, Assert.Single(n3.Interference));
    }

    [Fact]
    public void ConflictingVariablesShouldHaveInterference()
    {
        var v1 = new DummyVariable("v1");
        var v2 = new DummyVariable("v2");
        var v3 = new DummyVariable("v3");
        
        var cfg = new ControlFlowGraph<DummyInstruction>(DummyArchitecture.Instance);
        var n1 = cfg.EntryPoint = cfg.Nodes.Add(new BasicBlock<DummyInstruction>([
            DummyInstruction.Push(0, 1),
            DummyInstruction.Set(1, v1),
            DummyInstruction.Push(2, 1),
            DummyInstruction.Set(3, v2),
            DummyInstruction.Get(4, v1),
            DummyInstruction.Get(5, v2),
            DummyInstruction.Push(6, 1),
            DummyInstruction.Set(7, v3),
            DummyInstruction.Get(8, v3),
            DummyInstruction.Ret(9),
        ]));

        var graph = InterferenceGraph.FromFlowGraph(cfg);
        Assert.Equal(v2, Assert.Single(graph.Nodes[v1].Interference).Variable);
        Assert.Equal(v1, Assert.Single(graph.Nodes[v2].Interference).Variable);
        Assert.Empty(graph.Nodes[v3].Interference);
    }
}