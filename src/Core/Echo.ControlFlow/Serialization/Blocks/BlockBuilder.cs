using System;
using System.Collections.Generic;
using System.Linq;
using Echo.ControlFlow.Blocks;
using Echo.ControlFlow.Regions;

namespace Echo.ControlFlow.Serialization.Blocks
{
    /// <summary>
    /// Provides a mechanism for transforming a control flow graph into a tree of scopes and basic blocks.
    /// </summary>
    /// <typeparam name="TInstruction">The type of instructions.</typeparam>
    public class BlockBuilder<TInstruction>
    {
        /// <summary>
        /// Constructs the tree of scopes and basic blocks based on the provided control flow graph. 
        /// </summary>
        /// <param name="cfg">The control flow graph.</param>
        /// <returns>The root scope.</returns>
        public ScopeBlock<TInstruction> ConstructBlocks(ControlFlowGraph<TInstruction> cfg)
        {
            var sorter = new BlockSorter<TInstruction>(cfg);
            var sorting = sorter.GetSorting();
            return BuildBlocksFromSortedNodes(cfg, sorting);
        }


        private static ScopeBlock<TInstruction> BuildBlocksFromSortedNodes(
            ControlFlowGraph<TInstruction> cfg, 
            IEnumerable<ControlFlowNode<TInstruction>> sorting)
        {
            // We maintain a stack of scope information. Every time we enter a new region, we enter a new scope,
            // and similarly, we leave a scope when we leave a region.

            var rootScope = new ScopeBlock<TInstruction>();
            var scopeStack = new IndexableStack<ScopeInfo>();
            scopeStack.Push(new ScopeInfo(cfg, rootScope));

            // Add the nodes in the order of the sorting.
            foreach (var node in sorting)
            {
                var currentScope = scopeStack.Peek();
                if (node.ParentRegion != currentScope.Region)
                {
                    UpdateScopeStack(node, scopeStack);
                    currentScope = scopeStack.Peek();
                }

                currentScope.AddBlock(node.Contents);
            }

            return rootScope;
        }

        private static void UpdateScopeStack(ControlFlowNode<TInstruction> node, IndexableStack<ScopeInfo> scopeStack)
        {
            var activeRegions = node.GetSituatedRegions()
                .Reverse()
                .ToArray();
            
            int largestPossibleCommonDepth = Math.Min(scopeStack.Count, activeRegions.Length);

            // Figure out common region depth.
            int commonDepth = 1;
            while (commonDepth < largestPossibleCommonDepth)
            {
                if (scopeStack[commonDepth].Region != activeRegions[commonDepth])
                    break;
                commonDepth++;
            }

            // Leave for every left region a scope block.   
            while (scopeStack.Count > commonDepth)
                scopeStack.Pop();

            // Enter for every entered region a new block.
            while (scopeStack.Count < activeRegions.Length)
            {
                // Create new scope block.
                var enteredRegion = activeRegions[scopeStack.Count];

                // Add new cope block to the current scope.
                var currentScope = scopeStack.Peek();

                if (enteredRegion is ExceptionHandlerRegion<TInstruction> ehRegion)
                {
                    // We entered an exception handler region.
                    var ehBlock = new ExceptionHandlerBlock<TInstruction>();
                    currentScope.AddBlock(ehBlock);
                    scopeStack.Push(new ScopeInfo(ehRegion, ehBlock));
                }
                else if (enteredRegion.ParentRegion is ExceptionHandlerRegion<TInstruction> parentEhRegion)
                {
                    // We entered one of the exception handler sub regions. Figure out which one it is.
                    var enteredBlock = default(ScopeBlock<TInstruction>);

                    if (!(currentScope.Block is ExceptionHandlerBlock<TInstruction> ehBlock))
                        throw new InvalidOperationException("The parent scope is not an exception handler scope.");

                    if (parentEhRegion.ProtectedRegion == enteredRegion)
                    {
                        // We entered the protected region.
                        enteredBlock = ehBlock.ProtectedBlock;
                    }
                    else
                    {
                        // We entered a handler region.
                        enteredBlock = new ScopeBlock<TInstruction>();
                        ehBlock.HandlerBlocks.Add(enteredBlock);
                    }

                    // Push the entered scope.
                    scopeStack.Push(new ScopeInfo(parentEhRegion.ProtectedRegion, enteredBlock));
                }
                else
                {
                    // Fall back method: just enter a new scope block.
                    var scopeBlock = new ScopeBlock<TInstruction>();
                    currentScope.AddBlock(scopeBlock);
                    scopeStack.Push(new ScopeInfo(enteredRegion, scopeBlock));
                }
            }
        }

        private readonly struct ScopeInfo
        {
            public ScopeInfo(IControlFlowRegion<TInstruction> region, IBlock<TInstruction> block)
            {
                Region = region;
                Block = block;
            }
            
            public IControlFlowRegion<TInstruction> Region
            {
                get;
            }

            public IBlock<TInstruction> Block
            {
                get;
            }

            public void AddBlock(IBlock<TInstruction> basicBlock)
            {
                if (Block is ScopeBlock<TInstruction> scopeBlock)
                    scopeBlock.Blocks.Add(basicBlock);
                else
                    throw new InvalidOperationException("Current scope is not a container of basic blocks.");
            }

            public override string ToString()
            {
                return $"{Region.GetType().Name}, Offset: {Region.GetEntrypoint().Offset:X8}";
            }
        }
    }

}