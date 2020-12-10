using Echo.ControlFlow.Blocks;
using Xunit;

namespace Echo.ControlFlow.Tests.Blocks
{
    public class HandlerBlockTest
    {
        [Fact]
        public void FirstBlockWithNullPrologueShouldResultInFirstBlockOfHandler()
        {
            var handler = new HandlerBlock<int>();
            handler.Contents.Blocks.Add(new BasicBlock<int>());
            
            Assert.Same(handler.Contents.Blocks[0], handler.GetFirstBlock());
        }
        
        [Fact]
        public void FirstBlockWithEmptyPrologueShouldResultInFirstBlockOfHandler()
        {
            var handler = new HandlerBlock<int>();
            handler.Prologue = new ScopeBlock<int>();
            handler.Contents.Blocks.Add(new BasicBlock<int>());
            
            Assert.Same(handler.Contents.Blocks[0], handler.GetFirstBlock());
        }
        
        [Fact]
        public void FirstBlockWithNonEmptyPrologueShouldResultInFirstBlockOfPrologue()
        {
            var handler = new HandlerBlock<int>();
            handler.Prologue = new ScopeBlock<int>();
            handler.Prologue.Blocks.Add(new BasicBlock<int>());
            handler.Contents.Blocks.Add(new BasicBlock<int>());
            
            Assert.Same(handler.Prologue.Blocks[0], handler.GetFirstBlock());
        }
        
        [Fact]
        public void FirstBlockWithEmptyContentAndNonEmptyEpilogueShouldResultInFirstBlockOfEpilogue()
        {
            var handler = new HandlerBlock<int>();
            handler.Epilogue = new ScopeBlock<int>();
            handler.Epilogue.Blocks.Add(new BasicBlock<int>());
            
            Assert.Same(handler.Epilogue.Blocks[0], handler.GetFirstBlock());
        }
        
        
        [Fact]
        public void LastBlockWithNullEpilogueShouldResultInLastBlockOfHandler()
        {
            var handler = new HandlerBlock<int>();
            handler.Contents.Blocks.Add(new BasicBlock<int>());
            handler.Contents.Blocks.Add(new BasicBlock<int>());
            handler.Contents.Blocks.Add(new BasicBlock<int>());
            
            Assert.Same(handler.Contents.Blocks[^1], handler.GetLastBlock());
        }
        
        [Fact]
        public void LastFirstBlockWithEmptyEpilogueShouldResultInLastBlockOfHandler()
        {
            var handler = new HandlerBlock<int>();
            handler.Contents.Blocks.Add(new BasicBlock<int>());
            handler.Contents.Blocks.Add(new BasicBlock<int>());
            handler.Contents.Blocks.Add(new BasicBlock<int>());
            handler.Epilogue = new ScopeBlock<int>();

            Assert.Same(handler.Contents.Blocks[^1], handler.GetLastBlock());
        }
        
        [Fact]
        public void LastBlockWithNonEmptyEpilogueShouldResultInLastBlockOfEpilogue()
        {
            var handler = new HandlerBlock<int>();
            handler.Contents.Blocks.Add(new BasicBlock<int>());
            handler.Contents.Blocks.Add(new BasicBlock<int>());
            handler.Epilogue = new ScopeBlock<int>();
            handler.Epilogue.Blocks.Add(new BasicBlock<int>());
            handler.Epilogue.Blocks.Add(new BasicBlock<int>());
            handler.Epilogue.Blocks.Add(new BasicBlock<int>());
            
            Assert.Same(handler.Epilogue.Blocks[^1], handler.GetLastBlock());
        }
        
        [Fact]
        public void LastBlockWithEmptyContentAndNonEmptyPrologueShouldResultInLastBlockOfPrologue()
        {
            var handler = new HandlerBlock<int>();
            handler.Prologue = new ScopeBlock<int>();
            handler.Prologue.Blocks.Add(new BasicBlock<int>());
            handler.Prologue.Blocks.Add(new BasicBlock<int>());
            handler.Prologue.Blocks.Add(new BasicBlock<int>());
            
            Assert.Same(handler.Prologue.Blocks[^1], handler.GetLastBlock());
        }
    }
}