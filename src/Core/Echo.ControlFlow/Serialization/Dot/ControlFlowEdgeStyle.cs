namespace Echo.ControlFlow.Serialization.Dot
{
    /// <summary>
    /// Defines a tuple of style properties for an edge in a control flow graph. 
    /// </summary>
    public readonly struct ControlFlowEdgeStyle
    {
        /// <summary>
        /// Creates a new style for an edge.
        /// </summary>
        /// <param name="color">The color of the edge.</param>
        /// <param name="style">The line drawing style of the edge.</param>
        public ControlFlowEdgeStyle(string color, string style)
        {
            Color = color;
            Style = style;
        }
        
        /// <summary>
        /// Gets the color of the edge.
        /// </summary>
        public string Color
        {
            get;
        }

        /// <summary>
        /// Gets the line drawing style of the edge.
        /// </summary>
        public string Style
        {
            get;
        }
    }
}