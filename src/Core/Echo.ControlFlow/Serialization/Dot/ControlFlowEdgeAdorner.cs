using System;
using System.Collections.Generic;
using Echo.Core.Graphing;
using Echo.Core.Graphing.Serialization.Dot;

namespace Echo.ControlFlow.Serialization.Dot
{
    /// <summary>
    /// Represents 
    /// </summary>
    /// <typeparam name="TInstruction"></typeparam>
    public class ControlFlowEdgeAdorner<TInstruction> : IDotEdgeAdorner
    {
        /// <summary>
        /// Gets or sets the edge style to use for normal fallthrough edges.
        /// </summary>
        public ControlFlowEdgeStyle FallthroughStyle
        {
            get;
            set;
        } = new ControlFlowEdgeStyle(null, null);

        /// <summary>
        /// Gets or sets the edge style to use for any fallthrough edge originating from a branching node.
        /// </summary>
        public ControlFlowEdgeStyle FalseStyle
        {
            get;
            set;
        } = new ControlFlowEdgeStyle("red", null);

        /// <summary>
        /// Gets or sets the edge style to use for any conditional edge.
        /// </summary>
        public ControlFlowEdgeStyle TrueStyle
        {
            get;
            set;
        } = new ControlFlowEdgeStyle("green", null);

        /// <summary>
        /// Gets or sets the edge style to use for any abnormal edge.
        /// </summary>
        public ControlFlowEdgeStyle AbnormalStyle
        {
            get;
            set;
        } = new ControlFlowEdgeStyle("gray", "dashed");

        /// <inheritdoc />
        public IDictionary<string, string> GetEdgeAttributes(IEdge edge)
        {
            if (edge is ControlFlowEdge<TInstruction> cfgEdge)
            {
                var result = new Dictionary<string, string>();

                var style = cfgEdge.Type switch
                {
                    ControlFlowEdgeType.FallThrough => (cfgEdge.Origin.ConditionalEdges.Count > 0
                        ? FalseStyle
                        : FallthroughStyle),
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