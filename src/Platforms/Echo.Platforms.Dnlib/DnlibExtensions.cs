using System.Collections.Generic;
using System.Linq;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using Echo.ControlFlow;
using Echo.ControlFlow.Construction;
using Echo.ControlFlow.Construction.Static;
using Echo.ControlFlow.Construction.Symbolic;
using Echo.ControlFlow.Regions.Detection;
using Echo.Code;
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
        /// <param name="lastOffset">The IL offset at the end of the method body.</param>
        /// <returns>The converted handler.</returns>
        private static ExceptionHandlerRange ToEchoRange(ExceptionHandler handler, uint lastOffset)
        {
            var tryRange = new AddressRange(handler.TryStart.Offset, handler.TryEnd?.Offset ?? lastOffset);
            var handlerRange = new AddressRange(handler.HandlerStart.Offset, handler.HandlerEnd?.Offset ?? lastOffset);

            if (handler.HandlerType == ExceptionHandlerType.Filter)
            {
                var filterRange = new AddressRange(handler.FilterStart.Offset, handler.HandlerStart.Offset);
                return new ExceptionHandlerRange(tryRange, filterRange, handlerRange, handler);
            }
            
            return new ExceptionHandlerRange(tryRange, handlerRange, handler);
        }

        /// <summary>
        /// Converts a collection of <see cref="ExceptionHandler"/> instances in a method body to a collection of
        /// <see cref="ExceptionHandlerRange"/> instances. 
        /// </summary>
        /// <param name="body">The body containing the exception handlers to convert.</param>
        /// <returns>The converted handlers.</returns>
        public static IEnumerable<ExceptionHandlerRange> GetExceptionHandlerRanges(this CilBody body)
        {
            var instruction = body.Instructions[body.Instructions.Count - 1];
            uint lastOffset = (uint) (instruction.Offset + instruction.GetSize());
            for (int i = 0; i < body.ExceptionHandlers.Count; i++)
                yield return ToEchoRange(body.ExceptionHandlers[i], lastOffset);
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

            var ehRanges = body
                .GetExceptionHandlerRanges()
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

            var ehRanges = body
                .GetExceptionHandlerRanges()
                .ToArray();

            var cfg = cfgBuilder.ConstructFlowGraph(0, ehRanges);
            if (ehRanges.Length > 0)
                cfg.DetectExceptionHandlerRegions(ehRanges);

            dataFlowGraph = dfgBuilder.DataFlowGraph;
            return cfg;
        }

    }
}