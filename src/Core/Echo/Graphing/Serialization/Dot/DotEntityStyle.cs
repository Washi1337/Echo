namespace Echo.Graphing.Serialization.Dot
{
    /// <summary>
    /// Defines a tuple of style properties for an entity in a control flow graph. 
    /// </summary>
    public readonly struct DotEntityStyle
    {
        /// <summary>
        /// Creates a new style for an entity.
        /// </summary>
        /// <param name="color">The color of the entity.</param>
        /// <param name="style">The line drawing style of the entity.</param>
        public DotEntityStyle(string color, string style)
        {
            Color = color;
            Style = style;
        }
        
        /// <summary>
        /// Gets the color of the entity.
        /// </summary>
        public string Color
        {
            get;
        }

        /// <summary>
        /// Gets the line drawing style of the entity.
        /// </summary>
        public string Style
        {
            get;
        }
    }
}