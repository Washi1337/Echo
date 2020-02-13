using System;
using System.Collections.Generic;
using Echo.Core.Graphing;
using Echo.Core.Graphing.Serialization.Dot;

namespace Echo.DataFlow.Serialization.Dot
{
    /// <summary>
    /// Represents an adorner that styles edges in a data flow graph.
    /// </summary>
    /// <typeparam name="TContents">The type of contents the nodes contain.</typeparam>
    public class DataFlowEdgeAdorner<TContents> : IDotEdgeAdorner
    {
        /// <summary>
        /// Gets or sets the edge style to use for edges representing stack dependencies.
        /// </summary>
        public DotEntityStyle StackDependencyStyle
        {
            get;
            set;
        } = new DotEntityStyle("black", "solid");
        
        /// <summary>
        /// Gets or sets the edge style to use for edges representing variable dependencies.
        /// </summary>
        public DotEntityStyle VariableDependencyStyle
        {
            get;
            set;
        } = new DotEntityStyle("gray", "dashed");
        
        /// <inheritdoc />
        public IDictionary<string, string> GetEdgeAttributes(IEdge edge)
        {
            if (edge is DataFlowEdge<TContents> e)
            {
                var result = new Dictionary<string, string>();
                
                var style = e.Type switch
                {
                    DataDependencyType.Stack => StackDependencyStyle,
                    DataDependencyType.Variable => VariableDependencyStyle,
                    _ => default
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