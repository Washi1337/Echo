using System;
using System.Collections.Generic;
using System.Reflection;
using AsmResolver.PE.DotNet.Cil;
using Echo.Concrete.Emulation;
using Echo.Concrete.Emulation.Dispatch;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch
{
    /// <summary>
    /// Provides a default implementation for a CIL operation code handler dispatcher.
    /// </summary>
    public class DefaultCilDispatcher : IVirtualMachineDispatcher<CilInstruction>
    {
        private static readonly IDictionary<Module, ICollection<ICilOpCodeHandler>> HandlerInstances
            = new Dictionary<Module, ICollection<ICilOpCodeHandler>>();

        private static IEnumerable<ICilOpCodeHandler> GetOrCreateHandlersInModule(Module module)
        {
            lock (HandlerInstances)
            {
                if (!HandlerInstances.TryGetValue(module, out var handlers))
                {
                    handlers = new List<ICilOpCodeHandler>();

                    foreach (var type in module.GetTypes())
                    {
                        if (!type.IsAbstract && typeof(ICilOpCodeHandler).IsAssignableFrom(type))
                            handlers.Add((ICilOpCodeHandler) Activator.CreateInstance(type));
                    }

                    HandlerInstances.Add(module, handlers);
                }

                return handlers;
            }
        }
        
        /// <inheritdoc />
        public event EventHandler<BeforeInstructionDispatchEventArgs<CilInstruction>> BeforeInstructionDispatch;
        
        /// <inheritdoc />
        public event EventHandler<InstructionDispatchEventArgs<CilInstruction>> AfterInstructionDispatch;

        /// <summary>
        /// Creates a new CIL dispatcher using the handlers defined in the current module. 
        /// </summary>
        public DefaultCilDispatcher()
            : this(typeof(DefaultCilDispatcher).Module)
        {
            
        }

        /// <summary>
        /// Creates a new CIL dispatcher using the handlers defined in the provided module. 
        /// </summary>
        public DefaultCilDispatcher(Module handlerModule)
        {
            var table = new Dictionary<CilCode, ICilOpCodeHandler>();
            foreach (var handler in GetOrCreateHandlersInModule(handlerModule))
            {
                foreach (var code in handler.SupportedOpCodes)
                    table.Add(code, handler);
            }

            DispatcherTable = table;
        }

        /// <summary>
        /// Gets the used dispatcher table.
        /// </summary>
        public IDictionary<CilCode, ICilOpCodeHandler> DispatcherTable
        {
            get;
        }
        
        /// <inheritdoc />
        public DispatchResult Execute(ExecutionContext context, CilInstruction instruction)
        {
            var eventArgs = new BeforeInstructionDispatchEventArgs<CilInstruction>(context, instruction);
            OnBeforeInstructionDispatch(eventArgs);

            DispatchResult result;
            if (eventArgs.Handled)
            {
                result = eventArgs.ResultOverride;
            }
            else
            {
                var handler = GetOpCodeHandler(instruction);
                result = handler.Execute(context, instruction);
            }

            OnAfterInstructionDispatch(new AfterInstructionDispatchEventArgs<CilInstruction>(context, instruction, result));
            return result;
        }

        /// <summary>
        /// Obtains the operation code handler for the provided instruction. 
        /// </summary>
        /// <param name="instruction">The instruction to get the handler for.</param>
        /// <returns>The operation code handler.</returns>
        /// <exception cref="UndefinedInstructionException">Occurs when the instruction is invalid or unsupported.</exception>
        protected virtual ICilOpCodeHandler GetOpCodeHandler(CilInstruction instruction)
        {
            if (!DispatcherTable.TryGetValue(instruction.OpCode.Code, out var handler))
                throw new UndefinedInstructionException(instruction.Offset);
            
            return handler;
        } 

        /// <summary>
        /// Invoked when an instruction is about to be dispatched. 
        /// </summary>
        /// <param name="e">The arguments describing the event.</param>
        protected virtual void OnBeforeInstructionDispatch(BeforeInstructionDispatchEventArgs<CilInstruction> e)
        {
            BeforeInstructionDispatch?.Invoke(this, e);
        }

        /// <summary>
        /// Invoked when an instruction is about to be dispatched. 
        /// </summary>
        /// <param name="e">The arguments describing the event.</param>
        protected virtual void OnAfterInstructionDispatch(InstructionDispatchEventArgs<CilInstruction> e)
        {
            AfterInstructionDispatch?.Invoke(this, e);
        }
    }
}