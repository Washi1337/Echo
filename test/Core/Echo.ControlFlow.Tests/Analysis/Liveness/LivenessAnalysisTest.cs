using Echo.ControlFlow.Analysis.Liveness;
using Echo.ControlFlow.Blocks;
using Echo.Platforms.DummyPlatform.Code;
using Xunit;

namespace Echo.ControlFlow.Tests.Analysis.Liveness;

public class LivenessAnalysisTest
{
    [Fact]
    public void VariableSetShouldIntroduceLiveVariable()
    {
        var v1 = new DummyVariable("v1");
        var cfg = new ControlFlowGraph<DummyInstruction>(DummyArchitecture.Instance);
        var n1 = cfg.EntryPoint = cfg.Nodes.Add(new BasicBlock<DummyInstruction>([
            DummyInstruction.Push(0, 1),
            DummyInstruction.Set(1, v1),
            DummyInstruction.Op(2, 0, 0),
            DummyInstruction.Get(3, v1),
        ]));

        var analysis = LivenessAnalysis.FromFlowGraph(cfg);
        Assert.Equal(new LivenessData([], []), analysis[n1.Contents.Instructions[0]]);
        Assert.Equal(new LivenessData([], [v1]), analysis[n1.Contents.Instructions[1]]);
        Assert.Equal(new LivenessData([v1], [v1]), analysis[n1.Contents.Instructions[2]]);
        Assert.Equal(new LivenessData([v1], []), analysis[n1.Contents.Instructions[3]]);
    }

    [Fact]
    public void TwoVariableSetShouldIntroduceLiveVariables()
    {
        var v1 = new DummyVariable("v1");
        var v2 = new DummyVariable("v2");
        var cfg = new ControlFlowGraph<DummyInstruction>(DummyArchitecture.Instance);
        var n1 = cfg.EntryPoint = cfg.Nodes.Add(new BasicBlock<DummyInstruction>([
            DummyInstruction.Push(0, 1),
            DummyInstruction.Set(1, v1),
            DummyInstruction.Push(2, 1),
            DummyInstruction.Set(3, v2),
            DummyInstruction.Get(4, v1),
            DummyInstruction.Get(5, v2),
            DummyInstruction.Op(6, 2, 0),
        ]));

        var analysis = LivenessAnalysis.FromFlowGraph(cfg);
        Assert.Equal(new LivenessData([], []), analysis[n1.Contents.Instructions[0]]);
        Assert.Equal(new LivenessData([], [v1]), analysis[n1.Contents.Instructions[1]]);
        Assert.Equal(new LivenessData([v1], [v1]), analysis[n1.Contents.Instructions[2]]);
        Assert.Equal(new LivenessData([v1], [v1, v2]), analysis[n1.Contents.Instructions[3]]);
        Assert.Equal(new LivenessData([v1, v2], [v2]), analysis[n1.Contents.Instructions[4]]);
        Assert.Equal(new LivenessData([v2], []), analysis[n1.Contents.Instructions[5]]);
        Assert.Equal(new LivenessData([], []), analysis[n1.Contents.Instructions[6]]);
    }

    [Fact]
    public void KeepAliveResultVariables()
    {
        var v1 = new DummyVariable("v1");
        var v2 = new DummyVariable("v2");
        var cfg = new ControlFlowGraph<DummyInstruction>(DummyArchitecture.Instance);
        var n1 = cfg.EntryPoint = cfg.Nodes.Add(new BasicBlock<DummyInstruction>([
            DummyInstruction.Push(0, 1),
            DummyInstruction.Set(1, v1),
            DummyInstruction.Push(2, 1),
            DummyInstruction.Set(3, v2),
            DummyInstruction.Get(4, v1),
            DummyInstruction.Op(5, 1, 0),
            DummyInstruction.Ret(6), 
        ]));

        var analysis = LivenessAnalysis.FromFlowGraph(cfg, [v2]);
        Assert.Equal(new LivenessData([], []), analysis[n1.Contents.Instructions[0]]);
        Assert.Equal(new LivenessData([], [v1]), analysis[n1.Contents.Instructions[1]]);
        Assert.Equal(new LivenessData([v1], [v1]), analysis[n1.Contents.Instructions[2]]);
        Assert.Equal(new LivenessData([v1], [v1, v2]), analysis[n1.Contents.Instructions[3]]);
        Assert.Equal(new LivenessData([v1, v2], [v2]), analysis[n1.Contents.Instructions[4]]);
        Assert.Equal(new LivenessData([v2], [v2]), analysis[n1.Contents.Instructions[5]]);
        Assert.Equal(new LivenessData([v2], [v2]), analysis[n1.Contents.Instructions[6]]);
    }

    [Fact]
    public void DeadVariableSetShouldNotIntroduceLiveVariable()
    {
        var v1 = new DummyVariable("v1");
        var cfg = new ControlFlowGraph<DummyInstruction>(DummyArchitecture.Instance);
        var n1 = cfg.EntryPoint = cfg.Nodes.Add(new BasicBlock<DummyInstruction>([
            DummyInstruction.Push(0, 1),
            DummyInstruction.Set(1, v1),
            DummyInstruction.Op(2, 0, 0),
        ]));

        var analysis = LivenessAnalysis.FromFlowGraph(cfg);
        Assert.Equal(new LivenessData([], []), analysis[n1.Contents.Instructions[0]]);
        Assert.Equal(new LivenessData([], []), analysis[n1.Contents.Instructions[1]]);
        Assert.Equal(new LivenessData([], []), analysis[n1.Contents.Instructions[2]]);
    }

    [Fact]
    public void DeadVariableSetBeforeSecondSetShouldNotIntroduceLiveVariable()
    {
        var v1 = new DummyVariable("v1");
        var cfg = new ControlFlowGraph<DummyInstruction>(DummyArchitecture.Instance);
        var n1 = cfg.EntryPoint = cfg.Nodes.Add(new BasicBlock<DummyInstruction>([
            DummyInstruction.Push(0, 1),
            DummyInstruction.Set(1, v1),
            DummyInstruction.Push(1, 1),
            DummyInstruction.Set(2, v1),
            DummyInstruction.Op(3, 0, 0),
            DummyInstruction.Get(4, v1),
        ]));

        var analysis = LivenessAnalysis.FromFlowGraph(cfg);
        Assert.Equal(new LivenessData([], []), analysis[n1.Contents.Instructions[0]]);
        Assert.Equal(new LivenessData([], []), analysis[n1.Contents.Instructions[1]]);
        Assert.Equal(new LivenessData([], []), analysis[n1.Contents.Instructions[2]]);
        Assert.Equal(new LivenessData([], [v1]), analysis[n1.Contents.Instructions[3]]);
        Assert.Equal(new LivenessData([v1], [v1]), analysis[n1.Contents.Instructions[4]]);
        Assert.Equal(new LivenessData([v1], []), analysis[n1.Contents.Instructions[5]]);
    }

    [Fact]
    public void VariableSetTwoBlocks()
    {
        var v1 = new DummyVariable("v1");
        var cfg = new ControlFlowGraph<DummyInstruction>(DummyArchitecture.Instance);
        var n1 = cfg.EntryPoint = cfg.Nodes.Add(new BasicBlock<DummyInstruction>([
            DummyInstruction.Push(0, 1),
            DummyInstruction.Set(1, v1),
            DummyInstruction.Op(2, 0, 0),
        ]));
        var n2 = cfg.Nodes.Add(new BasicBlock<DummyInstruction>([
            DummyInstruction.Get(100, v1),
            DummyInstruction.Op(101, 1, 0),
        ]));
        n1.ConnectWith(n2);

        var analysis = LivenessAnalysis.FromFlowGraph(cfg);
        Assert.Equal(new LivenessData([], []), analysis[n1.Contents.Instructions[0]]);
        Assert.Equal(new LivenessData([], [v1]), analysis[n1.Contents.Instructions[1]]);
        Assert.Equal(new LivenessData([v1], [v1]), analysis[n1.Contents.Instructions[2]]);
        Assert.Equal(new LivenessData([v1], []), analysis[n2.Contents.Instructions[0]]);
        Assert.Equal(new LivenessData([], []), analysis[n2.Contents.Instructions[1]]);
    }

    [Fact]
    public void VariableSetBeforeConditional()
    {
        var v1 = new DummyVariable("v1");
        var cfg = new ControlFlowGraph<DummyInstruction>(DummyArchitecture.Instance);
        var n1 = cfg.EntryPoint = cfg.Nodes.Add(new BasicBlock<DummyInstruction>([
            DummyInstruction.Push(0, 1),
            DummyInstruction.Set(1, v1),
            DummyInstruction.Op(2, 0, 0),
        ]));
        var n2 = cfg.Nodes.Add(new BasicBlock<DummyInstruction>([
            DummyInstruction.Op(100, 1, 0),
        ]));
        var n3 = cfg.Nodes.Add(new BasicBlock<DummyInstruction>([
            DummyInstruction.Op(200, 1, 0),
        ]));
        var n4 = cfg.Nodes.Add(new BasicBlock<DummyInstruction>([
            DummyInstruction.Get(300, v1),
            DummyInstruction.Op(301, 1, 0),
        ]));
        n1.ConnectWith(n2, ControlFlowEdgeType.Conditional);
        n1.ConnectWith(n3, ControlFlowEdgeType.FallThrough);
        n2.ConnectWith(n4, ControlFlowEdgeType.Unconditional);
        n3.ConnectWith(n4, ControlFlowEdgeType.Unconditional);

        var analysis = LivenessAnalysis.FromFlowGraph(cfg);
        Assert.Equal(new LivenessData([], []), analysis[n1.Contents.Instructions[0]]);
        Assert.Equal(new LivenessData([], [v1]), analysis[n1.Contents.Instructions[1]]);
        Assert.Equal(new LivenessData([v1], [v1]), analysis[n1.Contents.Instructions[2]]);
        Assert.Equal(new LivenessData([v1], [v1]), analysis[n2.Contents.Instructions[0]]);
        Assert.Equal(new LivenessData([v1], [v1]), analysis[n3.Contents.Instructions[0]]);
        Assert.Equal(new LivenessData([v1], []), analysis[n4.Contents.Instructions[0]]);
        Assert.Equal(new LivenessData([], []), analysis[n4.Contents.Instructions[1]]);
    }

    [Fact]
    public void ConditionalSetSameVariable()
    {
        var v1 = new DummyVariable("v1");

        var cfg = new ControlFlowGraph<DummyInstruction>(DummyArchitecture.Instance);
        var n1 = cfg.EntryPoint = cfg.Nodes.Add(new BasicBlock<DummyInstruction>([
            DummyInstruction.Op(2, 0, 0),
        ]));
        var n2 = cfg.Nodes.Add(new BasicBlock<DummyInstruction>([
            DummyInstruction.Push(100, 1),
            DummyInstruction.Set(101, v1),
            DummyInstruction.Op(102, 1, 0),
        ]));
        var n3 = cfg.Nodes.Add(new BasicBlock<DummyInstruction>([
            DummyInstruction.Push(200, 1),
            DummyInstruction.Set(201, v1),
            DummyInstruction.Op(202, 1, 0),
        ]));
        var n4 = cfg.Nodes.Add(new BasicBlock<DummyInstruction>([
            DummyInstruction.Get(300, v1),
            DummyInstruction.Op(301, 1, 0),
        ]));
        n1.ConnectWith(n2, ControlFlowEdgeType.Conditional);
        n1.ConnectWith(n3, ControlFlowEdgeType.FallThrough);
        n2.ConnectWith(n4, ControlFlowEdgeType.Unconditional);
        n3.ConnectWith(n4, ControlFlowEdgeType.Unconditional);

        var analysis = LivenessAnalysis.FromFlowGraph(cfg);
        Assert.Equal(new LivenessData([], []), analysis[n1.Contents.Instructions[0]]);
        Assert.Equal(new LivenessData([], []), analysis[n2.Contents.Instructions[0]]);
        Assert.Equal(new LivenessData([], [v1]), analysis[n2.Contents.Instructions[1]]);
        Assert.Equal(new LivenessData([v1], [v1]), analysis[n2.Contents.Instructions[2]]);
        Assert.Equal(new LivenessData([], []), analysis[n3.Contents.Instructions[0]]);
        Assert.Equal(new LivenessData([], [v1]), analysis[n3.Contents.Instructions[1]]);
        Assert.Equal(new LivenessData([v1], [v1]), analysis[n3.Contents.Instructions[2]]);
        Assert.Equal(new LivenessData([v1], []), analysis[n4.Contents.Instructions[0]]);
        Assert.Equal(new LivenessData([], []), analysis[n4.Contents.Instructions[1]]);
    }

    [Fact]
    public void ConditionalSetDifferentVariableShouldMakeBothVariablesLiveOnBothBranches()
    {
        var v1 = new DummyVariable("v1");
        var v2 = new DummyVariable("v2");

        var cfg = new ControlFlowGraph<DummyInstruction>(DummyArchitecture.Instance);
        var n1 = cfg.EntryPoint = cfg.Nodes.Add(new BasicBlock<DummyInstruction>([
            DummyInstruction.Op(2, 0, 0),
        ]));
        var n2 = cfg.Nodes.Add(new BasicBlock<DummyInstruction>([
            DummyInstruction.Push(100, 1),
            DummyInstruction.Set(101, v1),
            DummyInstruction.Op(102, 1, 0),
        ]));
        var n3 = cfg.Nodes.Add(new BasicBlock<DummyInstruction>([
            DummyInstruction.Push(200, 1),
            DummyInstruction.Set(201, v2),
            DummyInstruction.Op(202, 1, 0),
        ]));
        var n4 = cfg.Nodes.Add(new BasicBlock<DummyInstruction>([
            DummyInstruction.Get(300, v1),
            DummyInstruction.Get(301, v2),
            DummyInstruction.Op(302, 2, 0),
        ]));
        n1.ConnectWith(n2, ControlFlowEdgeType.Conditional);
        n1.ConnectWith(n3, ControlFlowEdgeType.FallThrough);
        n2.ConnectWith(n4, ControlFlowEdgeType.Unconditional);
        n3.ConnectWith(n4, ControlFlowEdgeType.Unconditional);

        var analysis = LivenessAnalysis.FromFlowGraph(cfg);
        Assert.Equal(new LivenessData([v1, v2], [v1, v2]), analysis[n1.Contents.Instructions[0]]);
        Assert.Equal(new LivenessData([v2], [v2]), analysis[n2.Contents.Instructions[0]]);
        Assert.Equal(new LivenessData([v2], [v1, v2]), analysis[n2.Contents.Instructions[1]]);
        Assert.Equal(new LivenessData([v1, v2], [v1, v2]), analysis[n2.Contents.Instructions[2]]);
        Assert.Equal(new LivenessData([v1], [v1]), analysis[n3.Contents.Instructions[0]]);
        Assert.Equal(new LivenessData([v1], [v1, v2]), analysis[n3.Contents.Instructions[1]]);
        Assert.Equal(new LivenessData([v1, v2], [v1, v2]), analysis[n3.Contents.Instructions[2]]);
        Assert.Equal(new LivenessData([v1, v2], [v2]), analysis[n4.Contents.Instructions[0]]);
        Assert.Equal(new LivenessData([v2], []), analysis[n4.Contents.Instructions[1]]);
        Assert.Equal(new LivenessData([], []), analysis[n4.Contents.Instructions[2]]);
    }
}