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
    public static class BlockBuilder
    {
        /// <summary>
        /// Constructs the tree of scopes and basic blocks based on the provided control flow graph. 
        /// </summary>
        /// <param name="cfg">The control flow graph .</param>
        /// <typeparam name="TInstruction">The type of instructions stored in the graph.</typeparam>
        /// <returns>The root scope.</returns>
        public static ScopeBlock<TInstruction> ConstructBlocks<TInstruction>(this ControlFlowGraph<TInstruction> cfg)
        {
            return BuildBlocksFromSortedNodes(cfg, cfg.SortNodes());
        }

        private static ScopeBlock<TInstruction> BuildBlocksFromSortedNodes<TInstruction>(
            ControlFlowGraph<TInstruction> cfg, 
            IEnumerable<ControlFlowNode<TInstruction>> sorting)
        {
            // We maintain a stack of scope information. Every time we enter a new region, we enter a new scope,
            // and similarly, we leave a scope when we leave a region.

            var rootScope = new ScopeBlock<TInstruction>();
            var scopeStack = new IndexableStack<ScopeInfo<TInstruction>>();
            scopeStack.Push(new ScopeInfo<TInstruction>(cfg, rootScope));

            // Add the nodes in the order of the sorting.
            foreach (var node in sorting)
            {
                var currentScope = scopeStack.Peek();
                if (node.ParentRegion != currentScope.Region)
                {
                    UpdateScopeStack(scopeStack, node);
                    currentScope = scopeStack.Peek();
                }

                currentScope.AddBlock(node.Contents);
            }

            return rootScope;
        }

        private static void UpdateScopeStack<TInstruction>(
            IndexableStack<ScopeInfo<TInstruction>> scopeStack, ControlFlowNode<TInstruction> node)
        {
            // Figure out regions the node is in.
            var activeRegions = node.GetSituatedRegions()
                .Reverse()
                .ToArray();

            // Leave for every left region a scope block.   
            int commonDepth = GetCommonRegionDepth(scopeStack, activeRegions);
            while (scopeStack.Count > commonDepth)
                scopeStack.Pop();

            // Enter for every entered region a new block.
            while (scopeStack.Count < activeRegions.Length)
                EnterNextRegion(scopeStack, activeRegions);
        }

        private static int GetCommonRegionDepth<TInstruction>(
            IndexableStack<ScopeInfo<TInstruction>> scopeStack, 
            IControlFlowRegion<TInstruction>[] activeRegions)
        {
            int largestPossibleCommonDepth = Math.Min(scopeStack.Count, activeRegions.Length);

            int commonDepth = 1;
            while (commonDepth < largestPossibleCommonDepth)
            {
                if (scopeStack[commonDepth].Region != activeRegions[commonDepth])
                    break;
                commonDepth++;
            }

            return commonDepth;
        }

        private static void EnterNextRegion<TInstruction>(
            IndexableStack<ScopeInfo<TInstruction>> scopeStack, 
            IControlFlowRegion<TInstruction>[] activeRegions)
        {
            var enteredRegion = activeRegions[scopeStack.Count];
            
            if (enteredRegion is ExceptionHandlerRegion<TInstruction> ehRegion)
            {
                // We entered an exception handler region.
                EnterExceptionHandlerRegion(scopeStack, ehRegion);
            }
            else switch (enteredRegion.ParentRegion)
            {
                case ExceptionHandlerRegion<TInstruction> parentEhRegion:
                    // We entered one of the exception handler sub regions.
                    EnterExceptionHandlerSubRegion(scopeStack, parentEhRegion, enteredRegion);
                    break;

                case HandlerRegion<TInstruction> parentHandlerRegion:
                    // We entered one of the handler sub regions.
                    EnterHandlerSubRegion(scopeStack, parentHandlerRegion, enteredRegion);
                    break;

                default:
                    // Fall back method: just enter a new scope block.
                    EnterGenericRegion(scopeStack, enteredRegion);
                    break;
            }
        }

        private static void EnterExceptionHandlerRegion<TInstruction>(
            IndexableStack<ScopeInfo<TInstruction>> scopeStack, 
            ExceptionHandlerRegion<TInstruction> ehRegion)
        {
            var ehBlock = new ExceptionHandlerBlock<TInstruction>
            {
                Tag = ehRegion.Tag
            };
            
            scopeStack.Peek().AddBlock(ehBlock);
            scopeStack.Push(new ScopeInfo<TInstruction>(ehRegion, ehBlock));
        }

        private static void EnterExceptionHandlerSubRegion<TInstruction>(
            IndexableStack<ScopeInfo<TInstruction>> scopeStack, 
            ExceptionHandlerRegion<TInstruction> parentRegion, 
            IControlFlowRegion<TInstruction> enteredRegion)
        {
            IBlock<TInstruction> enteredBlock;
            IControlFlowRegion<TInstruction> enteredSubRegion;

            if (!(scopeStack.Peek().Block is ExceptionHandlerBlock<TInstruction> ehBlock))
                throw new InvalidOperationException("The parent scope is not an exception handler scope.");

            // Did we enter the protected region, or one of the handler regions?
            if (parentRegion.ProtectedRegion == enteredRegion)
            {
                // We entered the protected region.
                enteredBlock = ehBlock.ProtectedBlock;
                enteredSubRegion = parentRegion.ProtectedRegion;
            }
            else
            {
                // We entered a handler region.
                var handlerRegion = parentRegion.Handlers.First(r => r == enteredRegion);
                var handlerBlock = new HandlerBlock<TInstruction>
                {
                    Tag = handlerRegion.Tag
                };

                enteredBlock = handlerBlock;
                enteredSubRegion = handlerRegion;
                ehBlock.Handlers.Add(handlerBlock);
            }

            // Sanity check: If the entered subregion's parent is the exception handler region but the region
            // isn't a protected, prologue, epilogue nor one of the handler regions, that would mean that
            // something went *seriously* wrong.
            if (enteredSubRegion is null)
            {
                throw new InvalidOperationException(
                    "Entered subregion of exception handler doesn't belong to any of its regions!?");
            }

            // Push the entered scope.
            scopeStack.Push(new ScopeInfo<TInstruction>(enteredSubRegion, enteredBlock));
        }

        private static void EnterHandlerSubRegion<TInstruction>(
            IndexableStack<ScopeInfo<TInstruction>> scopeStack,
            HandlerRegion<TInstruction> parentRegion, 
            IControlFlowRegion<TInstruction> enteredRegion)
        {
            IBlock<TInstruction> enteredBlock;
            IControlFlowRegion<TInstruction> enteredSubRegion;

            if (!(scopeStack.Peek().Block is HandlerBlock<TInstruction> handlerBlock))
                throw new InvalidOperationException("The parent scope is not a handler scope.");

            if (parentRegion.Prologue == enteredRegion)
            {
                // We entered the prologue.
                handlerBlock.Prologue = new ScopeBlock<TInstruction>();
                enteredBlock = handlerBlock.Prologue;
                enteredSubRegion = parentRegion.Prologue;
            }
            else if (parentRegion.Contents == enteredRegion)
            {
                // We entered the contents.
                enteredBlock = handlerBlock.Contents;
                enteredSubRegion = parentRegion.Contents;
            }
            else if (parentRegion.Epilogue == enteredRegion)
            {
                // We entered the epilogue.
                handlerBlock.Epilogue = new ScopeBlock<TInstruction>();
                enteredBlock = handlerBlock.Epilogue;
                enteredSubRegion = parentRegion.Epilogue;
            }
            else
            {
                // Exhaustive search, this should never happen.
                throw new InvalidOperationException(
                    "Entered subregion of handler doesn't belong to any of its regions.");
            }

            // Push the entered scope.
            scopeStack.Push(new ScopeInfo<TInstruction>(enteredSubRegion, enteredBlock));
        }

        private static void EnterGenericRegion<TInstruction>(IndexableStack<ScopeInfo<TInstruction>> scopeStack,
            IControlFlowRegion<TInstruction> enteredRegion)
        {
            var scopeBlock = new ScopeBlock<TInstruction>();
            scopeStack.Peek().AddBlock(scopeBlock);
            scopeStack.Push(new ScopeInfo<TInstruction>(enteredRegion, scopeBlock));
        }

        private readonly struct ScopeInfo<TInstruction>
        {
            public ScopeInfo(IControlFlowRegion<TInstruction> region, IBlock<TInstruction> block)
            {
                Region = region ?? throw new ArgumentNullException(nameof(region));
                Block = block ?? throw new ArgumentNullException(nameof(block));
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