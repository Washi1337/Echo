using Echo.Platforms.DummyPlatform.Code;

namespace Echo.ControlFlow.Tests
{
    public static class TestGraphs
    {
        public static ControlFlowGraph<DummyInstruction> CreateSingularGraph()
        {
            var graph = new ControlFlowGraph<DummyInstruction>(DummyArchitecture.Instance);

            var n1 = new ControlFlowNode<DummyInstruction>(DummyInstruction.Ret(0));

            graph.Nodes.Add(n1);
            graph.EntryPoint = n1;

            return graph;
        }

        public static ControlFlowGraph<DummyInstruction> CreatePath()
        {
            var graph = new ControlFlowGraph<DummyInstruction>(DummyArchitecture.Instance);

            var n1 = new ControlFlowNode<DummyInstruction>( 
                DummyInstruction.Op(0, 0, 0)
            );

            var n2 = new ControlFlowNode<DummyInstruction>(
                DummyInstruction.Op(1, 0, 0)
            );

            var n3 = new ControlFlowNode<DummyInstruction>(
                DummyInstruction.Op(2, 0, 0)
            );

            var n4 = new ControlFlowNode<DummyInstruction>(
                DummyInstruction.Ret(3)
            );

            graph.Nodes.AddRange([n1, n2, n3, n4]);
            graph.Nodes.UpdateOffsets();
            graph.EntryPoint = n1;

            n1.ConnectWith(n2);
            n2.ConnectWith(n3);
            n3.ConnectWith(n4);

            return graph;
        }

        public static ControlFlowGraph<DummyInstruction> CreateIf()
        {
            var graph = new ControlFlowGraph<DummyInstruction>(DummyArchitecture.Instance);

            var n1 = new ControlFlowNode<DummyInstruction>(
                DummyInstruction.Op(0, 0, 1),
                DummyInstruction.JmpCond(1, 3)
            );

            var n2 = new ControlFlowNode<DummyInstruction>(DummyInstruction.Op(2, 0, 0));

            var n3 = new ControlFlowNode<DummyInstruction>(DummyInstruction.Ret(3));

            graph.Nodes.AddRange([n1, n2, n3]);
            graph.Nodes.UpdateOffsets();
            graph.EntryPoint = n1;

            n1.ConnectWith(n2);
            n1.ConnectWith(n3, ControlFlowEdgeType.Conditional);
            n2.ConnectWith(n3);

            return graph;
        }
        
        public static ControlFlowGraph<DummyInstruction> CreateIfElse()
        {
            var graph = new ControlFlowGraph<DummyInstruction>(DummyArchitecture.Instance);

            var n1 = new ControlFlowNode<DummyInstruction>(
                DummyInstruction.Op(0, 0, 1), 
                DummyInstruction.JmpCond(1, 3)
            );

            var n2 = new ControlFlowNode<DummyInstruction>(DummyInstruction.Jmp(2, 4));

            var n3 = new ControlFlowNode<DummyInstruction>(DummyInstruction.Op(3, 0, 0));

            var n4 = new ControlFlowNode<DummyInstruction>(DummyInstruction.Ret(4));

            graph.Nodes.AddRange([n1, n2, n3, n4]);
            graph.Nodes.UpdateOffsets();
            graph.EntryPoint = n1;

            n1.ConnectWith(n2);
            n1.ConnectWith(n3, ControlFlowEdgeType.Conditional);
            n2.ConnectWith(n4);
            n3.ConnectWith(n4);

            return graph;
        }
        
        public static ControlFlowGraph<DummyInstruction> CreateIfElseNested()
        {
            var graph = new ControlFlowGraph<DummyInstruction>(DummyArchitecture.Instance);

            var n1 = new ControlFlowNode<DummyInstruction>(
                DummyInstruction.Op(0, 0, 1), 
                DummyInstruction.JmpCond(1, 7)
            );

            var n2 = new ControlFlowNode<DummyInstruction>( 
                DummyInstruction.Op(2, 0, 1), 
                DummyInstruction.JmpCond(3, 4)
            );

            var n3 = new ControlFlowNode<DummyInstruction>(DummyInstruction.Jmp(4, 6));
            
            var n4 = new ControlFlowNode<DummyInstruction>(DummyInstruction.Jmp(5, 6));
            
            var n5 = new ControlFlowNode<DummyInstruction>(DummyInstruction.Jmp(6, 7));

            var n6 = new ControlFlowNode<DummyInstruction>(DummyInstruction.Ret(7));

            graph.Nodes.AddRange([n1, n2, n3, n4, n5, n6]);
            graph.Nodes.UpdateOffsets();
            graph.EntryPoint = n1;

            n1.ConnectWith(n2);
            n1.ConnectWith(n6, ControlFlowEdgeType.Conditional);
            n2.ConnectWith(n3);
            n2.ConnectWith(n4, ControlFlowEdgeType.Conditional);
            n3.ConnectWith(n5);
            n4.ConnectWith(n5);
            n5.ConnectWith(n6);

            return graph;
        }

        public static ControlFlowGraph<DummyInstruction> CreateLoop()
        {
            var graph = new ControlFlowGraph<DummyInstruction>(DummyArchitecture.Instance);

            var n1 = new ControlFlowNode<DummyInstruction>(DummyInstruction.Jmp(0, 2));

            var n2 = new ControlFlowNode<DummyInstruction>(DummyInstruction.Jmp(1, 4));

            var n3 = new ControlFlowNode<DummyInstruction>(
                DummyInstruction.Op(2, 0, 1), 
                DummyInstruction.JmpCond(3, 1)
            );

            var n4 = new ControlFlowNode<DummyInstruction>(DummyInstruction.Ret(4));

            graph.Nodes.AddRange([n1, n2, n3, n4]);
            graph.Nodes.UpdateOffsets();
            graph.EntryPoint = n1;

            n1.ConnectWith(n3);
            n3.ConnectWith(n2, ControlFlowEdgeType.Conditional);
            n3.ConnectWith(n4);
            n2.ConnectWith(n3);

            return graph;
        }

        public static ControlFlowGraph<DummyInstruction> CreateSwitch()
        {
            var graph = new ControlFlowGraph<DummyInstruction>(DummyArchitecture.Instance);

            var n1 = new ControlFlowNode<DummyInstruction>(DummyInstruction.Switch(0, 2, 3, 4, 5));
            var n2 = new ControlFlowNode<DummyInstruction>(DummyInstruction.Jmp(1, 5));
            var n3 = new ControlFlowNode<DummyInstruction>(DummyInstruction.Jmp(2, 5));
            var n4 = new ControlFlowNode<DummyInstruction>(DummyInstruction.Jmp(3, 5));
            var n5 = new ControlFlowNode<DummyInstruction>(DummyInstruction.Jmp(4, 5));
            var n6 = new ControlFlowNode<DummyInstruction>(                DummyInstruction.Ret(5));
            
            graph.Nodes.AddRange([n1, n2, n3, n4, n5, n6]);
            graph.Nodes.UpdateOffsets();
            graph.EntryPoint = n1;

            n1.ConnectWith(n2);
            n1.ConnectWith(n3,ControlFlowEdgeType.Conditional);
            n1.ConnectWith(n4,ControlFlowEdgeType.Conditional);
            n1.ConnectWith(n5,ControlFlowEdgeType.Conditional);
            n2.ConnectWith(n6);
            n3.ConnectWith(n6);
            n4.ConnectWith(n6);
            n5.ConnectWith(n6);
            
            return graph;
        }
    }
}