using System;
using System.Collections.Generic;
using Echo.Core.Graphing;
using Echo.Core.Graphing.Serialization.Dot;

namespace Echo.ControlFlow.Serialization.Dot
{
    /// <summary>
    /// Represents an adorner that styles edges in a control flow graph.
    /// </summary>
    /// <typeparam name="TInstruction">The type of instructions the nodes contain.</typeparam>
    public class ControlFlowEdgeAdorner<TInstruction> : IDotEdgeAdorner
    {
        /// <summary>
        /// Gets or sets the edge style to use for normal fallthrough edges.
        /// </summary>
        public DotEntityStyle FallthroughStyle
        {
            get;
            set;
        } = new DotEntityStyle("gray", null);
        
        /// <summary>
        /// Gets or sets the edge style to use for unconditional branch edges.
        /// </summary>
        public DotEntityStyle UnconditionalStyle
        {
            get;
            set;
        } = new DotEntityStyle(null, null);

        /// <summary>
        /// Gets or sets the edge style to use for any fallthrough edge originating from a branching node.
        /// </summary>
        public DotEntityStyle FalseStyle
        {
            get;
            set;
        } = new DotEntityStyle("red", null);

        /// <summary>
        /// Gets or sets the edge style to use for any conditional edge.
        /// </summary>
        public DotEntityStyle TrueStyle
        {
            get;
            set;
        } = new DotEntityStyle("green", null);

        /// <summary>
        /// Gets or sets the edge style to use for any abnormal edge.
        /// </summary>
        public DotEntityStyle AbnormalStyle
        {
            get;
            set;
        } = new DotEntityStyle("gray", "dashed");

        /// <inheritdoc />
        public IDictionary<string, string> GetEdgeAttributes(IEdge edge, long sourceId, long targetId)
        {
            if (edge is ControlFlowEdge<TInstruction> cfgEdge)
            {
                var result = new Dictionary<string, string>();

                var style = cfgEdge.Type switch
                {
                    ControlFlowEdgeType.FallThrough => cfgEdge.Origin.ConditionalEdges.Count > 0
                        ? FalseStyle
                        : FallthroughStyle,
                    ControlFlowEdgeType.Unconditional => UnconditionalStyle, 
                    ControlFlowEdgeType.Conditional => TrueStyle,
                    ControlFlowEdgeType.Abnormal => AbnormalStyle,
                    _ => throw new ArgumentOutOfRangeException()
                };

                if (!string.IsNullOrEmpty(style.Color))
                    result["color"] = style.Color;
                if (!string.IsNullOrEmpty(style.Style))
                    result["style"] = style.Style;
                
                return result;
            }

            return null;
        }
    }
}