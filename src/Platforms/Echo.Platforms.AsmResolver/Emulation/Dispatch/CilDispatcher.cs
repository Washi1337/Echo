using System;
using System.Collections.Generic;
using System.Reflection;
using AsmResolver.PE.DotNet.Cil;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch
{
    /// <summary>
    /// Provides a mechanism for dispatching instructions to their respective handlers.
    /// </summary>
    public class CilDispatcher
    {
        private static readonly Dictionary<CilCode, ICilOpCodeHandler> DefaultDispatcherTable = new();

        /// <summary>
        /// Fires before an instruction gets dispatched.
        /// </summary>
        public event EventHandler<CilDispatchEventArgs>? BeforeInstructionDispatch;
        
        /// <summary>
        /// Fires after an instruction gets dispatched.
        /// </summary> 
        public event EventHandler<CilDispatchEventArgs>? AfterInstructionDispatch;

        private readonly CilDispatchEventArgs _dispatchEventArgs = new();

        static CilDispatcher() => InitializeDefaultDispatcherTable();

        /// <summary>
        /// Gets the table that is used for dispatching instructions by their mnemonic. 
        /// </summary>
        public Dictionary<CilCode, ICilOpCodeHandler> DispatcherTable
        {
            get;
        } = new(DefaultDispatcherTable);

        private static void InitializeDefaultDispatcherTable()
        {
            foreach (var type in typeof(ICilOpCodeHandler).Module.GetTypes())
            {
                if (!type.IsAbstract && typeof(ICilOpCodeHandler).IsAssignableFrom(type))
                {
                    var attribute = type.GetCustomAttribute<DispatcherTableEntryAttribute>();

                    var instance = (ICilOpCodeHandler) Activator.CreateInstance(type);
                    foreach (var opCode in attribute.OpCodes)
                        DefaultDispatcherTable[opCode] = instance;
                }
            }
        } 
        
        /// <summary>
        /// Dispatches and evaluates a single instruction.
        /// </summary>
        /// <param name="context">The context to evaluate the instruction in.</param>
        /// <param name="instruction">The instruction to dispatch and evaluate.</param>
        /// <returns>A value indicating whether the dispatch was successful or caused an error.</returns>
        /// <exception cref="NotSupportedException">Occurs when an operation was not supported by the dispatcher.</exception>
        public CilDispatchResult Dispatch(CilExecutionContext context, CilInstruction instruction)
        {
            _dispatchEventArgs.Context = context;
            _dispatchEventArgs.Instruction = instruction;
            _dispatchEventArgs.IsHandled = false;
            _dispatchEventArgs.Result = default;

            OnBeforeInstructionDispatch(_dispatchEventArgs);

            if (!_dispatchEventArgs.IsHandled)
            {
                if (!DispatcherTable.TryGetValue(instruction.OpCode.Code, out var handler))
                    throw new NotSupportedException($"OpCode {instruction.OpCode.Mnemonic} is not supported.");
                _dispatchEventArgs.Result = handler.Dispatch(context, instruction);
            }

            _dispatchEventArgs.IsHandled = true;
            OnAfterInstructionDispatch(_dispatchEventArgs);

            return _dispatchEventArgs.Result;
        }

        /// <summary>
        /// Fires the <see cref="BeforeInstructionDispatch" /> event.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        protected virtual void OnBeforeInstructionDispatch(CilDispatchEventArgs e)
        {
            BeforeInstructionDispatch?.Invoke(this, e);
        }

        /// <summary>
        /// Fires the <see cref="AfterInstructionDispatch" /> event.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        protected virtual void OnAfterInstructionDispatch(CilDispatchEventArgs e)
        {
            AfterInstructionDispatch?.Invoke(this, e);
        }
    }
}