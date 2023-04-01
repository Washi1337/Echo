using System;
using System.Collections.Generic;
using Echo.Graphing;
using Echo.Graphing.Serialization.Dot;

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
        /// Gets or sets a value indicating whether edges representing stack dependencies should be annotated
        /// with the stack slot index. 
        /// </summary>
        public bool IncludeStackEdgeLabels
        {
            get;
            set;
        } = true;
        
        /// <summary>
        /// Gets or sets the edge style to use for edges representing variable dependencies.
        /// </summary>
        public DotEntityStyle VariableDependencyStyle
        {
            get;
            set;
        } = new DotEntityStyle("gray", "dashed");

        /// <summary>
        /// Gets or sets a value indicating whether edges representing variable dependencies should be annotated
        /// with the variable that was referenced. 
        /// </summary>
        public bool IncludeVariableEdgeLabels
        {
            get;
            set;
        } = true;
        
        /// <inheritdoc />
        public IDictionary<string, string> GetEdgeAttributes(IEdge edge, long sourceId, long targetId)
        {
            if (edge is DataFlowEdge<TContents> e)
            {
                var result = new Dictionary<string, string>();
                
                (var style, string label) = e.DataSource switch
                {
                    StackDataSource<TContents> source => (StackDependencyStyle, source.SlotIndex.ToString()),
                    VariableDataSource<TContents> source => (VariableDependencyStyle, source.Variable.Name),
                    _ => default
                };
                
                if (!string.IsNullOrEmpty(style.Color))
                    result["color"] = style.Color;
                if (!string.IsNullOrEmpty(style.Style))
                    result["style"] = style.Style;
                if (!string.IsNullOrEmpty(label))
                    result["label"] = label;

                return result;
            }
            
            return null;
        }
    }
}