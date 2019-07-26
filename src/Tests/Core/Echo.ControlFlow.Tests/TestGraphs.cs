using Echo.ControlFlow.Specialized;
using Echo.Core.Emulation;
using Echo.Platforms.DummyPlatform.Code;

namespace Echo.ControlFlow.Tests
{
    public static class TestGraphs
    {
        public static ControlFlowGraph<DummyInstruction> CreateSingularGraph()
        {
            var graph = new ControlFlowGraph<DummyInstruction>();

            var n1 = new BasicBlockNode<DummyInstruction>(0, new[]
            {
                DummyInstruction.Ret(0)
            });

            graph.Nodes.Add(n1);
            graph.Entrypoint = n1;

            return graph;
        }

        public static ControlFlowGraph<DummyInstruction> CreatePath()
        {
            var graph = new ControlFlowGraph<DummyInstruction>();

            var n1 = new BasicBlockNode<DummyInstruction>(0, new[]
            {
                DummyInstruction.Op(0, 0, 0)
            });

            var n2 = new BasicBlockNode<DummyInstruction>(1, new[]
            {
                DummyInstruction.Op(1, 0, 0)
            });

            var n3 = new BasicBlockNode<DummyInstruction>(2, new[]
            {
                DummyInstruction.Op(2, 0, 0)
            });

            var n4 = new BasicBlockNode<DummyInstruction>(3, new[]
            {
                DummyInstruction.Ret(3)
            });

            graph.Nodes.AddRange(new[] {n1, n2, n3, n4});
            graph.Entrypoint = n1;

            n1.ConnectWith(n2);
            n2.ConnectWith(n3);
            n3.ConnectWith(n4);

            return graph;
        }

        public static ControlFlowGraph<DummyInstruction> CreateIf()
        {
            var graph = new ControlFlowGraph<DummyInstruction>();

            var n1 = new BasicBlockNode<DummyInstruction>(0, new[]
            {
                DummyInstruction.Op(0, 0, 1),
                DummyInstruction.JmpCond(1, 3),
            });

            var n2 = new BasicBlockNode<DummyInstruction>(2, new[]
            {
                DummyInstruction.Op(2, 0, 0)
            });

            var n3 = new BasicBlockNode<DummyInstruction>(3, new[]
            {
                DummyInstruction.Ret(3)
            });

            graph.Nodes.AddRange(new[] {n1, n2, n3});
            graph.Entrypoint = n1;

            n1.ConnectWith(n2);
            n1.ConnectWith(n3, EdgeType.Conditional);
            n2.ConnectWith(n3);

            return graph;
        }
        
        public static ControlFlowGraph<DummyInstruction> CreateIfElse()
        {
            var graph = new ControlFlowGraph<DummyInstruction>();

            var n1 = new BasicBlockNode<DummyInstruction>(0, new[]
            {
                DummyInstruction.Op(0, 0, 1),
                DummyInstruction.JmpCond(1, 3),
            });

            var n2 = new BasicBlockNode<DummyInstruction>(2, new[]
            {
                DummyInstruction.Jmp(2, 4)
            });

            var n3 = new BasicBlockNode<DummyInstruction>(3, new[]
            {
                DummyInstruction.Op(3, 0, 0)
            });

            var n4 = new BasicBlockNode<DummyInstruction>(4, new[]
            {
                DummyInstruction.Ret(4)
            });

            graph.Nodes.AddRange(new[] {n1, n2, n3, n4});
            graph.Entrypoint = n1;

            n1.ConnectWith(n2);
            n1.ConnectWith(n3, EdgeType.Conditional);
            n2.ConnectWith(n4);
            n3.ConnectWith(n4);

            return graph;
        }
        
        public static ControlFlowGraph<DummyInstruction> CreateIfElseNested()
        {
            var graph = new ControlFlowGraph<DummyInstruction>();

            var n1 = new BasicBlockNode<DummyInstruction>(0, new[]
            {
                DummyInstruction.Op(0, 0, 1),
                DummyInstruction.JmpCond(1, 7),
            });

            var n2 = new BasicBlockNode<DummyInstruction>(2, new[]
            {
                DummyInstruction.Op(2, 0, 1),
                DummyInstruction.JmpCond(3, 4)
            });

            var n3 = new BasicBlockNode<DummyInstruction>(4, new[]
            {
                DummyInstruction.Jmp(4, 6)
            });
            
            var n4 = new BasicBlockNode<DummyInstruction>(5, new[]
            {
                DummyInstruction.Jmp(5, 6)
            });
            
            var n5 = new BasicBlockNode<DummyInstruction>(6, new[]
            {
                DummyInstruction.Jmp(6, 7)
            });

            var n6 = new BasicBlockNode<DummyInstruction>(7, new[]
            {
                DummyInstruction.Ret(7)
            });

            graph.Nodes.AddRange(new[] {n1, n2, n3, n4, n5, n6});
            graph.Entrypoint = n1;

            n1.ConnectWith(n2);
            n1.ConnectWith(n6, EdgeType.Conditional);
            n2.ConnectWith(n3);
            n2.ConnectWith(n4, EdgeType.Conditional);
            n3.ConnectWith(n5);
            n4.ConnectWith(n5);
            n5.ConnectWith(n6);

            return graph;
        }

        public static ControlFlowGraph<DummyInstruction> CreateLoop()
        {
            var graph = new ControlFlowGraph<DummyInstruction>();

            var n1 = new BasicBlockNode<DummyInstruction>(0, new[]
            {
                DummyInstruction.Jmp(0, 2),
            });

            var n2 = new BasicBlockNode<DummyInstruction>(1, new[]
            {
                DummyInstruction.Jmp(1, 4)
            });

            var n3 = new BasicBlockNode<DummyInstruction>(2, new[]
            {
                DummyInstruction.Op(2, 0, 1),
                DummyInstruction.JmpCond(3, 1)
            });

            var n4 = new BasicBlockNode<DummyInstruction>(4, new[]
            {
                DummyInstruction.Ret(4)
            });

            graph.Nodes.AddRange(new[] {n1, n2, n3, n4});
            graph.Entrypoint = n1;

            n1.ConnectWith(n3);
            n3.ConnectWith(n2, EdgeType.Conditional);
            n3.ConnectWith(n4);
            n2.ConnectWith(n3);

            return graph;
        }

        public static ControlFlowGraph<DummyInstruction> CreateSwitch()
        {
            var graph = new ControlFlowGraph<DummyInstruction>();

            var n1 = new BasicBlockNode<DummyInstruction>(0, new[]
            {
                DummyInstruction.Switch(0, 2, 3, 4, 5),
            });
            var n2 = new BasicBlockNode<DummyInstruction>(1, new[]
            {
                DummyInstruction.Jmp(1, 5),
            });
            var n3 = new BasicBlockNode<DummyInstruction>(2, new[]
            {
                DummyInstruction.Jmp(2, 5),
            });
            var n4 = new BasicBlockNode<DummyInstruction>(3, new[]
            {
                DummyInstruction.Jmp(3, 5),
            });
            var n5 = new BasicBlockNode<DummyInstruction>(4, new[]
            {
                DummyInstruction.Jmp(4, 5),
            });
            var n6 = new BasicBlockNode<DummyInstruction>(5, new[]
            {
                DummyInstruction.Ret(5), 
            });
            
            graph.Nodes.AddRange(new[] {n1, n2, n3, n4, n5, n6});
            graph.Entrypoint = n1;

            n1.ConnectWith(n2);
            n1.ConnectWith(n3,EdgeType.Conditional);
            n1.ConnectWith(n4,EdgeType.Conditional);
            n1.ConnectWith(n5,EdgeType.Conditional);
            n2.ConnectWith(n6);
            n3.ConnectWith(n6);
            n4.ConnectWith(n6);
            n5.ConnectWith(n6);
            
            return graph;
        }
    }
}