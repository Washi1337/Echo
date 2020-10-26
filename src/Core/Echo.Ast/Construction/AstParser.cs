using System;
using System.Collections.Generic;
using Echo.ControlFlow;
using Echo.ControlFlow.Regions;
using Echo.ControlFlow.Serialization.Blocks;
using Echo.DataFlow;

namespace Echo.Ast.Construction
{
    /// <summary>
    /// Transforms a <see cref="ControlFlowGraph{TInstruction}"/> and a <see cref="DataFlowGraph{TContents}"/> into an Ast
    /// </summary>
    public sealed class AstParser<TInstruction>
    {
        private readonly ControlFlowGraph<TInstruction> _controlFlowGraph;
        private readonly AstArchitecture<TInstruction> _architecture;
        private readonly BlockTransformer<TInstruction> _transformer;
        private readonly Dictionary<BasicControlFlowRegion<TInstruction>, BasicControlFlowRegion<Statement<TInstruction>>> _regionsMapping =
            new Dictionary<BasicControlFlowRegion<TInstruction>, BasicControlFlowRegion<Statement<TInstruction>>>();
        
        /// <summary>
        /// Creates a new Ast parser with the given <see cref="ControlFlowGraph{TInstruction}"/>
        /// </summary>
        /// <param name="controlFlowGraph">The <see cref="ControlFlowGraph{TInstruction}"/> to parse</param>
        /// <param name="dataFlowGraph">The <see cref="DataFlowGraph{TContents}"/> to parse</param>
        public AstParser(ControlFlowGraph<TInstruction> controlFlowGraph, DataFlowGraph<TInstruction> dataFlowGraph)
        {
            _controlFlowGraph = controlFlowGraph;
            _architecture = new AstArchitecture<TInstruction>(controlFlowGraph.Architecture);

            var context = new AstParserContext<TInstruction>(_controlFlowGraph, dataFlowGraph);
            _transformer = new BlockTransformer<TInstruction>(context);
        }
        
        /// <summary>
        /// Parses the given <see cref="ControlFlowGraph{TInstruction}"/>
        /// </summary>
        /// <returns>A <see cref="ControlFlowGraph{TInstruction}"/> representing the Ast</returns>
        public ControlFlowGraph<Statement<TInstruction>> Parse()
        {
            var newGraph = new ControlFlowGraph<Statement<TInstruction>>(_architecture);
            var blockBuilder = new BlockBuilder<TInstruction>();
            var rootScope = blockBuilder.ConstructBlocks(_controlFlowGraph);

            // Transform and add regions.
            foreach (var originalRegion in _controlFlowGraph.Regions)
            {
                var newRegion = TransformRegion(originalRegion);
                newGraph.Regions.Add(newRegion);
            }

            // Transform and add nodes.
            foreach (var originalBlock in rootScope.GetAllBlocks())
            {
                var originalNode = _controlFlowGraph.Nodes[originalBlock.Offset];
                var transformedBlock = _transformer.Transform(originalBlock);
                var newNode = new ControlFlowNode<Statement<TInstruction>>(originalBlock.Offset, transformedBlock);
                newGraph.Nodes.Add(newNode);
                
                // Move node to newly created region.
                if (originalNode.ParentRegion is BasicControlFlowRegion<TInstruction> basicRegion)
                    newNode.MoveToRegion(_regionsMapping[basicRegion]);
            }

            // Clone edges.
            foreach (var originalEdge in _controlFlowGraph.GetEdges())
            {
                var newOrigin = newGraph.Nodes[originalEdge.Origin.Offset];
                var newTarget = newGraph.Nodes[originalEdge.Target.Offset];
                newOrigin.ConnectWith(newTarget, originalEdge.Type);
            }
            
            // Fix entry point.
            newGraph.Entrypoint = newGraph.Nodes[_controlFlowGraph.Entrypoint.Offset];

            return newGraph;
        }

        private ControlFlowRegion<Statement<TInstruction>> TransformRegion(IControlFlowRegion<TInstruction> region)
        {
            switch (region)
            {
                case BasicControlFlowRegion<TInstruction> basicRegion:
                    // Create new basic region.
                    var newBasicRegion = new BasicControlFlowRegion<Statement<TInstruction>>();
                    TransformSubRegions(basicRegion, newBasicRegion);

                    // Register basic region pair.
                    _regionsMapping[basicRegion] = newBasicRegion;

                    return newBasicRegion;

                case ExceptionHandlerRegion<TInstruction> ehRegion:
                    var newEhRegion = new ExceptionHandlerRegion<Statement<TInstruction>>();

                    // ProtectedRegion is read-only, so instead we just transform all sub regions and add it to the
                    // existing protected region.
                    TransformSubRegions(ehRegion.ProtectedRegion, newEhRegion.ProtectedRegion);
                    _regionsMapping[ehRegion.ProtectedRegion] = newEhRegion.ProtectedRegion;
                    
                    // Add handler regions.
                    foreach (var subRegion in ehRegion.HandlerRegions)
                        newEhRegion.HandlerRegions.Add(TransformRegion(subRegion));

                    return newEhRegion;

                default:
                    throw new ArgumentOutOfRangeException(nameof(region));
            }

            void TransformSubRegions(
                BasicControlFlowRegion<TInstruction> originalRegion, 
                BasicControlFlowRegion<Statement<TInstruction>> newRegion)
            {
                foreach (var subRegion in originalRegion.Regions)
                    newRegion.Regions.Add(TransformRegion(subRegion));
            }
        }
    }
}