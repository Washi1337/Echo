using Echo.ControlFlow.Blocks;
using Xunit;

namespace Echo.ControlFlow.Tests.Blocks
{
    public class ExceptionHandlerBlockTest
    {
        [Fact]
        public void FirstBlockOnNonEmptyProtectedBlock()
        {
            var eh = new ExceptionHandlerBlock<int>();
            eh.ProtectedBlock.Blocks.Add(new BasicBlock<int>());
            Assert.Same(eh.ProtectedBlock.Blocks[0], eh.GetFirstBlock());
        }
        
        [Fact]
        public void LastBlockOnEmptyHandlerListShouldResultInLastBlockOfProtectedBlock()
        {
            var eh = new ExceptionHandlerBlock<int>();
            eh.ProtectedBlock.Blocks.Add(new BasicBlock<int>());
            eh.ProtectedBlock.Blocks.Add(new BasicBlock<int>());
            eh.ProtectedBlock.Blocks.Add(new BasicBlock<int>());
            Assert.Same(eh.ProtectedBlock.Blocks[^1], eh.GetLastBlock());
        }
        
        [Fact]
        public void FirstBlockOnEmptyProtectedBlockShouldResultInFirstBlocKOfFirstHandler()
        {
            var eh = new ExceptionHandlerBlock<int>();
            var handler = new HandlerBlock<int>();
            handler.Contents.Blocks.Add(new BasicBlock<int>());
            eh.Handlers.Add(handler);
            Assert.Same(handler.Contents.Blocks[0], eh.GetFirstBlock());
        }
        
        [Fact]
        public void FirstBlockOnEmptyProtectedBlockAndMultipleEmptyHandlerBlocksShouldResultInFirstBlocKOfFirstNonEmptyHandlerBlock()
        {
            var eh = new ExceptionHandlerBlock<int>();
            eh.Handlers.Add(new HandlerBlock<int>());
            eh.Handlers.Add(new HandlerBlock<int>());
            eh.Handlers.Add(new HandlerBlock<int>());
            var handler = new HandlerBlock<int>();
            handler.Contents.Blocks.Add(new BasicBlock<int>());
            eh.Handlers.Add(handler);
            Assert.Same(handler.Contents.Blocks[0], eh.GetFirstBlock());
        }
    }
}