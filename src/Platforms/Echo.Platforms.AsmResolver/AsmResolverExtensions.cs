using System.Collections.Generic;
using System.Linq;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.PE.DotNet.Cil;
using Echo.ControlFlow;
using Echo.ControlFlow.Construction;
using Echo.ControlFlow.Regions.Detection;
using Echo.DataFlow;
using Echo.DataFlow.Construction;

namespace Echo.Platforms.AsmResolver
{
    /// <summary>
    /// Provides extension methods to AsmResolver models.
    /// </summary>
    public static class AsmResolverExtensions
    {
        /// <summary>
        /// Converts an instance of <see cref="CilExceptionHandler"/> to an <see cref="ExceptionHandlerRange"/>. 
        /// </summary>
        /// <param name="handler">The handler to convert.</param>
        /// <returns>The converted handler.</returns>
        public static ExceptionHandlerRange ToEchoRange(this CilExceptionHandler handler)
        {
            var tryRange = new AddressRange(handler.TryStart!.Offset, handler.TryEnd!.Offset);
            var handlerRange = new AddressRange(handler.HandlerStart!.Offset, handler.HandlerEnd!.Offset);
            
            if (handler.HandlerType == CilExceptionHandlerType.Filter)
            {
                var filterRange = new AddressRange(handler.FilterStart!.Offset, handler.HandlerStart.Offset);
                return new ExceptionHandlerRange(tryRange, filterRange, handlerRange, handler);
            }

            return new ExceptionHandlerRange(tryRange, handlerRange, handler);
        }

        /// <summary>
        /// Converts a collection of <see cref="CilExceptionHandler"/> instances to a collection of
        /// <see cref="ExceptionHandlerRange"/> instances. 
        /// </summary>
        /// <param name="handlers">The handlers to convert.</param>
        /// <returns>The converted handlers.</returns>
        public static IEnumerable<ExceptionHandlerRange> ToEchoRanges(this IEnumerable<CilExceptionHandler> handlers)
        {
            return handlers.Select(ToEchoRange);
        }

        /// <summary>
        /// Constructs a control flow graph from a CIL method body.
        /// </summary>
        /// <param name="self">The method body.</param>
        /// <returns>The control flow graph.</returns>
        public static ControlFlowGraph<CilInstruction> ConstructStaticFlowGraph(this CilMethodBody self)
        {
            var architecture = new CilArchitecture(self);
            var cfgBuilder = new StaticFlowGraphBuilder<CilInstruction>(
                architecture,
                self.Instructions,
                architecture.SuccessorResolver);

            var ehRanges = self.ExceptionHandlers
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
        public static ControlFlowGraph<CilInstruction> ConstructSymbolicFlowGraph(
            this CilMethodBody self, 
            out DataFlowGraph<CilInstruction> dataFlowGraph)
        {
            var architecture = new CilArchitecture(self);
            var dfgBuilder = new CilStateTransitioner(architecture);
            var cfgBuilder = new SymbolicFlowGraphBuilder<CilInstruction>(
                architecture,
                self.Instructions,
                dfgBuilder
            );

            var ehRanges = self.ExceptionHandlers
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