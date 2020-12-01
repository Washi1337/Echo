using System;
using System.Collections.Generic;
using Echo.ControlFlow.Regions;
using Echo.Core.Graphing;
using Echo.Core.Graphing.Serialization.Dot;

namespace Echo.ControlFlow.Serialization.Dot
{
    /// <summary>
    /// Represents an adorner that adds styles to regions in control flow graphs. 
    /// </summary>
    /// <typeparam name="TInstruction">The type of instructions the nodes contain.</typeparam>
    public class ExceptionHandlerAdorner<TInstruction> : IDotSubGraphAdorner
    {
        /// <summary>
        /// Gets or sets the style of an exception handler region.
        /// </summary>
        public DotEntityStyle ExceptionHandlerStyle
        {
            get;
            set;
        } = new DotEntityStyle("red", "solid");

        /// <summary>
        /// Gets or sets the style of the protected region in an exception handler region.
        /// </summary>
        public DotEntityStyle ProtectedRegionColor
        {
            get;
            set;
        } = new DotEntityStyle("green", "dashed");

        /// <summary>
        /// Gets or sets the style of a prologue region in an exception handler region.
        /// </summary>
        public DotEntityStyle PrologueRegionStyle
        {
            get;
            set;
        } = new DotEntityStyle("purple", "dashed");

        /// <summary>
        /// Gets or sets the style of a handler region in an exception handler region.
        /// </summary>
        public DotEntityStyle HandlerRegionStyle
        {
            get;
            set;
        } = new DotEntityStyle("blue", "dashed");

        /// <summary>
        /// Gets or sets the style of an epilogue region in an exception handler region.
        /// </summary>
        public DotEntityStyle EpilogueRegionStyle
        {
            get;
            set;
        } = new DotEntityStyle("orange", "dashed");

        /// <summary>
        /// Gets or sets the default style of a control flow region.
        /// </summary>
        public DotEntityStyle DefaultRegionStyle
        {
            get;
            set;
        } = new DotEntityStyle("gray", "dashed");
        
        /// <inheritdoc />
        public string GetSubGraphName(ISubGraph subGraph)
        {
            if (!(subGraph is IControlFlowRegion<TInstruction> region))
                return null;
            
            string prefix = DetermineRegionPrefix(region);

            long min = long.MaxValue;
            long max = long.MinValue;
            foreach (var node in region.GetNodes())
            {
                min = Math.Min(min, node.Offset);
                max = Math.Max(max, node.Offset);
            }

            return $"{prefix}_{min:X}_{max:X}";
        }

        private static string DetermineRegionPrefix(IControlFlowRegion<TInstruction> region)
        {
            switch (region)
            {
                case ScopeRegion<TInstruction> basicRegion:
                    switch (basicRegion.ParentRegion)
                    {
                        case ExceptionHandlerRegion<TInstruction> parentEh:
                        {
                            if (parentEh.ProtectedRegion == basicRegion)
                                return "cluster_protected";
                            break;
                        }

                        case HandlerRegion<TInstruction> parentHandler:
                        {
                            if (parentHandler.Prologue == basicRegion)
                                return "cluster_handler_prologue";
                            if (parentHandler.Contents == basicRegion)
                                return  "cluster_handler";
                            if (parentHandler.Epilogue == basicRegion)
                                return "cluster_handler_epilogue";
                            break;
                        }
                    }

                    return "cluster_block";

                case ExceptionHandlerRegion<TInstruction> _:
                    return "cluster_eh";

                case HandlerRegion<TInstruction> _:
                    return "cluster_handler";
                
                default:
                    return "cluster_region";
            }
        }

        /// <inheritdoc />
        public IDictionary<string, string> GetSubGraphAttributes(ISubGraph subGraph)
        {
            if (!(subGraph is IControlFlowRegion<TInstruction> region))
                return null;
            
            var regionStyle = DefaultRegionStyle;
            switch (region)
            {
                case ScopeRegion<TInstruction> basicRegion:
                    switch (basicRegion.ParentRegion)
                    {
                        case ExceptionHandlerRegion<TInstruction> parentEh:
                        {
                            if (parentEh.ProtectedRegion == basicRegion)
                                regionStyle = ProtectedRegionColor;
                            break;
                        }

                        case HandlerRegion<TInstruction> parentHandler:
                        {
                            if (parentHandler.Prologue == basicRegion)
                                regionStyle = PrologueRegionStyle;
                            else if (parentHandler.Contents == basicRegion)
                                regionStyle = DefaultRegionStyle;
                            else if (parentHandler.Epilogue == basicRegion)
                                regionStyle = EpilogueRegionStyle;
                            break;
                        }
                    }
                    break;

                case ExceptionHandlerRegion<TInstruction> _:
                    regionStyle = ExceptionHandlerStyle;
                    break;

                case HandlerRegion<TInstruction> _:
                    regionStyle = HandlerRegionStyle;
                    break;
            }
            
            return new Dictionary<string, string>
            {
                ["color"] = regionStyle.Color,
                ["style"] = regionStyle.Style
            };
        }

    }
}