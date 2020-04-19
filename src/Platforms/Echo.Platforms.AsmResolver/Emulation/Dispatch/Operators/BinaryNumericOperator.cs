using System;
using System.Collections.Generic;
using AsmResolver.PE.DotNet.Cil;
using Echo.Concrete.Emulation;
using Echo.Concrete.Emulation.Dispatch;
using Echo.Concrete.Values;
using Echo.Concrete.Values.ValueType;
using Echo.Platforms.AsmResolver.Emulation.Values;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch.Operators
{
    public abstract class BinaryNumericOperator : ICilOpCodeHandler
    {
        /// <inheritdoc />
        public abstract IReadOnlyCollection<CilCode> SupportedOpCodes
        {
            get;
        }

        /// <inheritdoc />
        public DispatchResult Execute(ExecutionContext context, CilInstruction instruction)
        {
            var value2 = context.ProgramState.Stack.Pop();
            var value1 = context.ProgramState.Stack.Pop();

            var (left, right) = PrepareIntegers(value1, value2);
            
            var result = new DispatchResult();
            if (left is null || right is null)
            {
                result.Exception = new InvalidProgramException();
            }
            else
            {
                result = Execute(context, left, right);

                if (result.IsSuccess)
                    context.ProgramState.ProgramCounter += instruction.Size;
            }

            return result;
        }

        /// <summary>
        /// Performs the operation on the two pushed integers.
        /// </summary>
        /// <param name="context">The context to execute the instruction in.</param>
        /// <param name="left">The left side of the operation.</param>
        /// <param name="right">The right side of the operation.</param>
        /// <returns>The result of the operation.</returns>
        protected abstract DispatchResult Execute(ExecutionContext context, IntegerValue left, IntegerValue right);

        private (IntegerValue, IntegerValue) PrepareIntegers(IConcreteValue value1, IConcreteValue value2)
        {
            return (value1, value2) switch
            {
                (Integer32Value a, Integer32Value b) => (a, b),
                (Integer64Value a, Integer64Value b) => (a, b),
                (NativeIntegerValue a, NativeIntegerValue b) => (a, b),
                
                (Integer32Value a, NativeIntegerValue b) => (new NativeIntegerValue(a, b.Size==4), b),
                (NativeIntegerValue a, Integer32Value b) => (a, new NativeIntegerValue(b, a.Size==4)),
                
                _ => (null, null),
            };
        }
    }
}