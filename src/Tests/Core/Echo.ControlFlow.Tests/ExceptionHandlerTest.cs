//using Echo.ControlFlow.Specialized;
//using Echo.Platforms.DummyPlatform.Code;
//using Xunit;
//
//namespace Echo.ControlFlow.Tests
//{
//    public class ExceptionHandlerTest
//    {
//        [Fact]
//        public void Simple()
//        {
//            var cfg = new Graph<DummyInstruction>();
//
//            var nodes = new Node<DummyInstruction>[4];
//            for (int i = 0; i < nodes.Length; i++)
//            {
//                nodes[i] = new Node<DummyInstruction>();
//                cfg.Nodes.Add(nodes[i]);
//            }
//
//            nodes[0].ConnectWith(nodes[1]);
//            nodes[1].ConnectWith(nodes[2], EdgeType.Abnormal);
//            nodes[1].ConnectWith(nodes[3]);
//            nodes[2].ConnectWith(nodes[3]);
//            
//            var eh = new ExceptionHandler<DummyInstruction>();
//            cfg.ExceptionHandlers.Add(eh);
//            
//            eh.Try.Nodes.Add(nodes[1]);
//            eh.Handler.Nodes.Add(nodes[2]);
//
//            Assert.Empty(nodes[0].GetExceptionHandlers());
//            Assert.Equal(new[] {eh}, nodes[1].GetExceptionHandlers());
//            Assert.Equal(new[] {eh}, nodes[2].GetExceptionHandlers());
//            Assert.Empty(nodes[3].GetExceptionHandlers());
//        }
//
//        [Fact]
//        public void Nested()
//        {
//            var cfg = new Graph<DummyInstruction>();
//
//            var nodes = new Node<DummyInstruction>[8];
//            for (int i = 0; i < nodes.Length; i++)
//            {
//                nodes[i] = new Node<DummyInstruction>();
//                cfg.Nodes.Add(nodes[i]);
//            }
//
//            nodes[0].ConnectWith(nodes[1]);
//            nodes[1].ConnectWith(nodes[2]);
//            nodes[2].ConnectWith(nodes[3]);
//            nodes[3].ConnectWith(nodes[5]);
//            nodes[5].ConnectWith(nodes[7]);
//            
//            nodes[1].ConnectWith(nodes[6], EdgeType.Abnormal);
//            nodes[2].ConnectWith(nodes[6], EdgeType.Abnormal);
//            nodes[3].ConnectWith(nodes[6], EdgeType.Abnormal);
//            nodes[4].ConnectWith(nodes[6], EdgeType.Abnormal);
//            nodes[5].ConnectWith(nodes[6], EdgeType.Abnormal);
//            
//            nodes[2].ConnectWith(nodes[4], EdgeType.Abnormal);
//            nodes[3].ConnectWith(nodes[4], EdgeType.Abnormal);
//            
//            var eh1 = new ExceptionHandler<DummyInstruction>();
//            cfg.ExceptionHandlers.Add(eh1);
//            eh1.Try.Nodes.UnionWith(new[] {nodes[1], nodes[2], nodes[3], nodes[4], nodes[5]});
//            eh1.Handler.Nodes.Add(nodes[6]);
//            
//            var eh2 = new ExceptionHandler<DummyInstruction>();
//            cfg.ExceptionHandlers.Add(eh2);
//            eh2.Try.Nodes.UnionWith(new[] {nodes[2], nodes[3]});
//            eh2.Handler.Nodes.Add(nodes[4]);
//
//            Assert.Empty(nodes[0].GetExceptionHandlers());
//            Assert.Equal(new[] {eh1}, nodes[1].GetExceptionHandlers());
//            Assert.Equal(new[] {eh1, eh2}, nodes[2].GetExceptionHandlers());
//            Assert.Equal(new[] {eh1, eh2}, nodes[3].GetExceptionHandlers());
//            Assert.Equal(new[] {eh1, eh2}, nodes[4].GetExceptionHandlers());
//            Assert.Equal(new[] {eh1}, nodes[5].GetExceptionHandlers());
//            Assert.Equal(new[] {eh1}, nodes[6].GetExceptionHandlers());
//            Assert.Empty(nodes[7].GetExceptionHandlers());   
//        }
//    }
//}