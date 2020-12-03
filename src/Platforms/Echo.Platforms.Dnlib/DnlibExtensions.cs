using System.Collections.Generic;
using System.Linq;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using Echo.ControlFlow;
using Echo.ControlFlow.Construction;
using Echo.ControlFlow.Construction.Static;
using Echo.ControlFlow.Construction.Symbolic;
using Echo.ControlFlow.Regions.Detection;
using Echo.Core.Code;
using Echo.DataFlow;

namespace Echo.Platforms.Dnlib
{
    /// <summary>
    /// Provides extension methods to dnlib models.
    /// </summary>
    public static class DnlibExtensions
    {
        /// <summary>
        /// Converts an instance of <see cref="ExceptionHandler"/> to an <see cref="ExceptionHandlerRange"/>. 
        /// </summary>
        /// <param name="handler">The handler to convert.</param>
        /// <returns>The converted handler.</returns>
        public static ExceptionHandlerRange ToEchoRange(this ExceptionHandler handler)
        {
            var tryRange = new AddressRange(handler.TryStart.Offset, handler.TryEnd.Offset);
            var handlerRange = new AddressRange(handler.HandlerStart.Offset, handler.HandlerEnd.Offset);

            if (handler.HandlerType == ExceptionHandlerType.Filter)
            {
                var filterRange = new AddressRange(handler.FilterStart.Offset, handler.HandlerStart.Offset);
                return new ExceptionHandlerRange(tryRange, filterRange, handlerRange, handler);
            }
            
            return new ExceptionHandlerRange(tryRange, handlerRange, handler);
        }

        /// <summary>
        /// Converts a collection of <see cref="ExceptionHandler"/> instances to a collection of
        /// <see cref="ExceptionHandlerRange"/> instances. 
        /// </summary>
        /// <param name="handlers">The handlers to convert.</param>
        /// <returns>The converted handlers.</returns>
        public static IEnumerable<ExceptionHandlerRange> ToEchoRanges(this IEnumerable<ExceptionHandler> handlers)
        {
            return handlers.Select(ToEchoRange);
        }

        /// <summary>
        /// Constructs a control flow graph from a CIL method body.
        /// </summary>
        /// <param name="self">The method body.</param>
        /// <returns>The control flow graph.</returns>
        public static ControlFlowGraph<Instruction> ConstructStaticFlowGraph(this MethodDef self)
        {
            var body = self.Body;
            
            var architecture = new CilArchitecture(self);
            var cfgBuilder = new StaticFlowGraphBuilder<Instruction>(
                architecture,
                body.Instructions,
                architecture.SuccessorResolver);

            var ehRanges = body.ExceptionHandlers
                .ToEchoRanges()
                .ToArray();
            
            var cfg = cfgBuilder.ConstructFlowGraph(0, ehRanges);
            if (ehRanges.Length > 0)
                cfg.DetectExceptionHandlerRegions(ehRanges);
            return cfg;
        }

        /// <summary>
        /// Constructs a control flow graph and a data flow graph from a CIL method body.
        /// </summary>
        /// <param name="self">The method body.</param>
        /// <param name="dataFlowGraph">The constructed data flow graph.</param>
        /// <returns>The control flow graph.</returns>
        public static ControlFlowGraph<Instruction> ConstructSymbolicFlowGraph(
            this MethodDef self, 
            out DataFlowGraph<Instruction> dataFlowGraph)
        {
            var body = self.Body;
            
            var architecture = new CilArchitecture(self);
            var dfgBuilder = new CilStateTransitionResolver(architecture);
            var cfgBuilder = new SymbolicFlowGraphBuilder<Instruction>(
                architecture,
                body.Instructions,
                dfgBuilder);

            var ehRanges = body.ExceptionHandlers
                .ToEchoRanges()
                .ToArray();
            
            var cfg = cfgBuilder.ConstructFlowGraph(0, ehRanges);
            if (ehRanges.Length > 0)
                cfg.DetectExceptionHandlerRegions(ehRanges);

            dataFlowGraph = dfgBuilder.DataFlowGraph;
            return cfg;
        }
        
        
    }
}