using System;
using System.Collections.Generic;
using System.Diagnostics;
using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.DotNet.Collections;

namespace Echo.Platforms.AsmResolver.Emulation.Stack;

[DebuggerTypeProxy(typeof(DebuggerProxy))]
public partial class CallFrame
{
    private sealed class DebuggerProxy
    {
        private readonly CallFrame _frame;

        public DebuggerProxy(CallFrame frame)
        {
            _frame = frame;
        }

        public IMethodDescriptor Method => _frame.Method;

        public int ProgramCounter => _frame.ProgramCounter;

        public EvaluationStack EvaluationStack => _frame.EvaluationStack;

        public ExceptionHandlerStack ExceptionHandlerStack => _frame.ExceptionHandlerStack;

        public KeyValuePair<Parameter, object>[] Arguments
        {
            get
            {
                // Verify we have access to everything.
                if (_frame is not {Body.Owner.Parameters: { } parameters, Method.Signature: { } signature})
                    return Array.Empty<KeyValuePair<Parameter, object>>();

                // Verify count.
                int count = signature.GetTotalParameterCount();
                if (count == 0)
                    return Array.Empty<KeyValuePair<Parameter, object>>();

                // Read all their values.
                var result = new KeyValuePair<Parameter, object>[count];
                for (int i = 0; i < result.Length; i++)
                {
                    result[i] = new KeyValuePair<Parameter, object>(
                        parameters.GetBySignatureIndex(i),
                        _frame.ReadArgument(i)
                    );
                }

                return result;
            }
        }

        public KeyValuePair<CilLocalVariable, object>[] Locals
        {
            get
            {
                // Verify locals exist.
                if (_frame is not {Body.LocalVariables: {} locals} || locals.Count == 0)
                    return Array.Empty<KeyValuePair<CilLocalVariable, object>>();
                
                // Read all their values.
                var result = new KeyValuePair<CilLocalVariable, object>[locals.Count];
                for (int i = 0; i < result.Length; i++)
                    result[i] = new KeyValuePair<CilLocalVariable, object>(locals[i], _frame.ReadLocal(i));
                
                return result;
            }
        }
    }
}