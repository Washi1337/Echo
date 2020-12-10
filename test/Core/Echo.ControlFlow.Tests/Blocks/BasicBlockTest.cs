using Echo.ControlFlow.Blocks;
using Xunit;

namespace Echo.ControlFlow.Tests.Blocks
{
    public class BasicBlockTest
    {
        [Fact]
        public void FirstBlockShouldBeItself()
        {
            var block = new BasicBlock<int>();
            Assert.Same(block, block.GetFirstBlock());
        }
        
        [Fact]
        public void LastBlockShouldBeItself()
        {
            var block = new BasicBlock<int>();
            Assert.Same(block, block.GetLastBlock());
        }
        
    }
}