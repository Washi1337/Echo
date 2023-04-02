namespace Echo.Tests.Graphing
{
    public static class TestGraphs
    {
          public static IntGraph CreateSingularGraph()
        {
            var graph = new IntGraph();
            graph.AddNode(1);
            return graph;
        }

        public static IntGraph CreatePath()
        {
            var graph = new IntGraph();

            var n1 = graph.AddNode(1);
            var n2 = graph.AddNode(2);
            var n3 = graph.AddNode(3);
            var n4 = graph.AddNode(4);
            
            n1.ConnectWith(n2);
            n2.ConnectWith(n3);
            n3.ConnectWith(n4);

            return graph;
        }

        public static IntGraph CreateIf()
        {
            var graph = new IntGraph();
            var n1 = graph.AddNode(1);
            var n2 = graph.AddNode(2);
            var n3 = graph.AddNode(3);

            n1.ConnectWith(n2);
            n1.ConnectWith(n3);
            n2.ConnectWith(n3);

            return graph;
        }
        
        public static IntGraph CreateIfElse()
        {
            var graph = new IntGraph();
            var n1 = graph.AddNode(1);
            var n2 = graph.AddNode(2);
            var n3 = graph.AddNode(3);
            var n4 = graph.AddNode(4);

            n1.ConnectWith(n2);
            n1.ConnectWith(n3);
            n2.ConnectWith(n4);
            n3.ConnectWith(n4);

            return graph;
        }
        
        public static IntGraph CreateIfElseNested()
        {
            var graph = new IntGraph();

            var n1 = graph.AddNode(1);
            var n2 = graph.AddNode(2);
            var n3 = graph.AddNode(3);
            var n4 = graph.AddNode(4);
            var n5 = graph.AddNode(5);
            var n6 = graph.AddNode(6);

            n1.ConnectWith(n2);
            n1.ConnectWith(n6);
            n2.ConnectWith(n3);
            n2.ConnectWith(n4);
            n3.ConnectWith(n5);
            n4.ConnectWith(n5);
            n5.ConnectWith(n6);

            return graph;
        }

        public static IntGraph CreateLoop()
        {
            var graph = new IntGraph();
            
            var n1 = graph.AddNode(1);
            var n2 = graph.AddNode(2);
            var n3 = graph.AddNode(3);
            var n4 = graph.AddNode(4);

            n1.ConnectWith(n3);
            n3.ConnectWith(n2);
            n3.ConnectWith(n4);
            n2.ConnectWith(n3);

            return graph;
        }

        public static IntGraph CreateSwitch()
        {
            var graph = new IntGraph();

            var n1 = graph.AddNode(1);
            var n2 = graph.AddNode(2);
            var n3 = graph.AddNode(3);
            var n4 = graph.AddNode(4);
            var n5 = graph.AddNode(5);
            var n6 = graph.AddNode(6);

            n1.ConnectWith(n2);
            n1.ConnectWith(n3);
            n1.ConnectWith(n4);
            n1.ConnectWith(n5);
            n2.ConnectWith(n6);
            n3.ConnectWith(n6);
            n4.ConnectWith(n6);
            n5.ConnectWith(n6);
            
            return graph;
        }
    }
}