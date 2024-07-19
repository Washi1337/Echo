using System;
using System.Collections.Generic;
using Echo.ControlFlow.Regions;
using Echo.Graphing;
using Echo.Graphing.Serialization.Dot;

namespace Echo.ControlFlow.Serialization.Dot
{
    /// <summary>
    /// Represents an adorner that adds styles to regions in control flow graphs. 
    /// </summary>
    /// <typeparam name="TInstruction">The type of instructions the nodes contain.</typeparam>
    public class ExceptionHandlerAdorner<TInstruction> : IDotSubGraphAdorner
        where TInstruction : notnull
    {
        /// <summary>
        /// Gets or sets the style of an enclosing exception handler region.
        /// </summary>
        public DotEntityStyle ExceptionHandlerStyle
        {
            get;
            set;
        } = new DotEntityStyle("black", "dashed");

        /// <summary>
        /// Gets or sets the label of an enclosing exception handler region.
        /// </summary>
        public string ExceptionHandlerLabel
        {
            get;
            set;
        } = "Exception Handler Region";

        /// <summary>
        /// Gets or sets the style of the protected region in an exception handler region.
        /// </summary>
        public DotEntityStyle ProtectedStyle
        {
            get;
            set;
        } = new DotEntityStyle("green", "solid");

        /// <summary>
        /// Gets or sets the label of the protected region in an exception handler region.
        /// </summary>
        public string ProtectedLabel
        {
            get;
            set;
        } = "Protected";

        /// <summary>
        /// Gets or sets the style of a handler region in an exception handler region.
        /// </summary>
        public DotEntityStyle HandlerStyle
        {
            get;
            set;
        } = new DotEntityStyle("red", "dashed");

        /// <summary>
        /// Gets or sets the label of a handler region in an exception handler region.
        /// </summary>
        public string HandlerLabel
        {
            get;
            set;
        } = "Handler Region";

        /// <summary>
        /// Gets or sets the style of a prologue region in an exception handler region.
        /// </summary>
        public DotEntityStyle PrologueStyle
        {
            get;
            set;
        } = new DotEntityStyle("royalblue", "solid");

        /// <summary>
        /// Gets or sets the label of the prologue region in an exception handler region.
        /// </summary>
        public string PrologueLabel
        {
            get;
            set;
        } = "Prologue";

        /// <summary>
        /// Gets or sets the default style of a control flow region.
        /// </summary>
        public DotEntityStyle HandlerContentsStyle
        {
            get;
            set;
        } = new DotEntityStyle("red", "solid");
        
        /// <summary>
        /// Gets or sets the label of a contents region in a handler of an exception handler region.
        /// </summary>
        public string HandlerContentsLabel
        {
            get;
            set;
        } = "Handler";

        /// <summary>
        /// Gets or sets the style of an epilogue region in an exception handler region.
        /// </summary>
        public DotEntityStyle EpilogueStyle
        {
            get;
            set;
        } = new DotEntityStyle("orange", "solid");

        /// <summary>
        /// Gets or sets the label of an epilogue region in an exception handler region.
        /// </summary>
        public string EpilogueLabel
        {
            get;
            set;
        } = "Epilogue";

        /// <summary>
        /// Gets or sets the default style of a control flow region.
        /// </summary>
        public DotEntityStyle DefaultStyle
        {
            get;
            set;
        } = new DotEntityStyle("gray", "solid");
        
        /// <inheritdoc />
        public string GetSubGraphName(ISubGraph subGraph)
        {
            if (!(subGraph is IControlFlowRegion<TInstruction> region))
                return string.Empty;
            
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
                case ScopeRegion<TInstruction> scopeRegion:
                    return GetScopeRegionPrefix(scopeRegion);
                
                case ExceptionHandlerRegion<TInstruction> _:
                    return "cluster_eh";

                case HandlerRegion<TInstruction> _:
                    return "cluster_handler";
                
                default:
                    return "cluster_region";
            }
        }

        private static string GetScopeRegionPrefix(ScopeRegion<TInstruction> basicRegion)
        {
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
                        return "cluster_handler";
                    if (parentHandler.Epilogue == basicRegion)
                        return "cluster_handler_epilogue";
                    break;
                }
            }

            return "cluster_block";
        }

        /// <inheritdoc />
        public IDictionary<string, string>? GetSubGraphAttributes(ISubGraph subGraph)
        {
            if (!(subGraph is IControlFlowRegion<TInstruction> region))
                return null;
            
            (var style, string label) = GetSubGraphStyle(region);

            var result = new Dictionary<string, string>();
            if (!string.IsNullOrEmpty(style.Color))
                result["color"] = style.Color!;
            if (!string.IsNullOrEmpty(style.Style))
                result["style"] = style.Style!;
            result["label"] = label;
            
            return result;
        }

        private (DotEntityStyle Style, string Label) GetSubGraphStyle(IControlFlowRegion<TInstruction> region)
        {
            switch (region)
            {
                case ScopeRegion<TInstruction> basicRegion:
                    return GetScopeStyle(basicRegion);

                case ExceptionHandlerRegion<TInstruction> _:
                    return (ExceptionHandlerStyle, ExceptionHandlerLabel);

                case HandlerRegion<TInstruction> _:
                    return (HandlerStyle, HandlerLabel);
            }

            return (DefaultStyle, string.Empty);
        }

        private (DotEntityStyle Style, string Label) GetScopeStyle(ScopeRegion<TInstruction> basicRegion)
        {
            switch (basicRegion.ParentRegion)
            {
                case ExceptionHandlerRegion<TInstruction> parentEh:
                {
                    if (parentEh.ProtectedRegion == basicRegion)
                        return (ProtectedStyle, ProtectedLabel);
                    break;
                }

                case HandlerRegion<TInstruction> parentHandler:
                {
                    if (parentHandler.Prologue == basicRegion)
                        return (PrologueStyle, PrologueLabel);
                    if (parentHandler.Contents == basicRegion)
                        return (HandlerContentsStyle, HandlerContentsLabel);
                    if (parentHandler.Epilogue == basicRegion)
                        return (EpilogueStyle, EpilogueLabel);
                    break;
                }
            }

            return (DefaultStyle, "Scope");
        }
        
    }
}