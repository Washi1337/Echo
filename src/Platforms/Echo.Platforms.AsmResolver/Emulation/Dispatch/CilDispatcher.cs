using System;
using System.Collections.Generic;
using System.Reflection;
using AsmResolver.PE.DotNet.Cil;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch
{
    public class CilDispatcher
    {
        private static readonly Dictionary<CilCode, ICilOpCodeHandler> DefaultDispatcherTable = new();

        public event EventHandler<CilDispatchEventArgs>? BeforeInstructionDispatch; 
        public event EventHandler<CilDispatchEventArgs>? AfterInstructionDispatch;
        private readonly CilDispatchEventArgs _dispatchEventArgs = new();

        static CilDispatcher() => InitializeDefaultDispatcherTable();

        public CilDispatcher()
        {
            DispatcherTable = new Dictionary<CilCode, ICilOpCodeHandler>(DefaultDispatcherTable);
        }

        public Dictionary<CilCode, ICilOpCodeHandler> DispatcherTable
        {
            get;
        }

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

        protected virtual void OnBeforeInstructionDispatch(CilDispatchEventArgs e)
        {
            BeforeInstructionDispatch?.Invoke(this, e);
        }

        protected virtual void OnAfterInstructionDispatch(CilDispatchEventArgs e)
        {
            AfterInstructionDispatch?.Invoke(this, e);
        }
    }
}