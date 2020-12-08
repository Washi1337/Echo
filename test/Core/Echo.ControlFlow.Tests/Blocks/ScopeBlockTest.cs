using Echo.ControlFlow.Blocks;
using Xunit;

namespace Echo.ControlFlow.Tests.Blocks
{
    public class ScopeBlockTest
    {
        [Fact]
        public void FirstBlockOnEmptyScopeShouldReturnNull()
        {
            var scope = new ScopeBlock<int>();
            Assert.Null(scope.GetFirstBlock());
        }
        
        [Fact]
        public void LastBlockOnEmptyScopeShouldReturnNull()
        {
            var scope = new ScopeBlock<int>();
            Assert.Null(scope.GetLastBlock());
        }
        
        [Fact]
        public void FirstBlockOnNonEmptyFlatScopeShouldReturnFirstBasicBlockInList()
        {
            var scope = new ScopeBlock<int>();
            scope.Blocks.Add(new BasicBlock<int>());
            Assert.Same(scope.Blocks[0], scope.GetFirstBlock());
        }
        
        [Fact]
        public void LastBlockOnFlatScopeWithSingleItemShouldReturnFirstBasicBlockInList()
        {
            var scope = new ScopeBlock<int>();
            scope.Blocks.Add(new BasicBlock<int>());
            Assert.Same(scope.Blocks[0], scope.GetLastBlock());
        }
        
        [Fact]
        public void LastBlockOnNonEmptyFlatScopeShouldReturnLastBasicBlockInList()
        {
            var scope = new ScopeBlock<int>();
            scope.Blocks.Add(new BasicBlock<int>());
            scope.Blocks.Add(new BasicBlock<int>());
            scope.Blocks.Add(new BasicBlock<int>());
            Assert.Same(scope.Blocks[^1], scope.GetLastBlock());
        }

        [Fact]
        public void FirstBlockOnNonFlatScopeShouldReturnFirstNestedBasicBlock()
        {
            var scope1 = new ScopeBlock<int>();
            var scope2 = new ScopeBlock<int>();
            
            var scope3 = new ScopeBlock<int>();
            scope3.Blocks.Add(new BasicBlock<int>());

            scope2.Blocks.Add(scope3);
            scope2.Blocks.Add(new BasicBlock<int>());

            scope1.Blocks.Add(scope2);
            scope1.Blocks.Add(new BasicBlock<int>());
            
            Assert.Same(scope3.Blocks[0], scope1.GetFirstBlock());
        }
    }
}