using AsmResolver.PE.DotNet.Cil;
using Echo.Concrete.Emulation;
using Echo.Concrete.Values.ValueType;
using Echo.Core;
using Echo.Platforms.AsmResolver.Emulation.Values.Cli;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch.ControlFlow
{
    /// <summary>
    /// Provides a base for all branching operation codes that pop two arguments from the stack.
    /// </summary>
    public abstract class BinaryBranchHandler : BranchHandler
    {
        /// <inheritdoc />
        protected override int ArgumentCount => 2;

        /// <inheritdoc />
        public override Trilean VerifyCondition(CilExecutionContext context, CilInstruction instruction)
        {
            var (left, right) = BinaryOperationHelper.PeekBinaryOperationArguments(context);

            return (left, right) switch
            {
                (IntegerValue a, IntegerValue b) => VerifyCondition(context, instruction, a, b),
                (FValue a, FValue b) => VerifyCondition(context, instruction, a, b),
                (OValue a, OValue b) => VerifyCondition(context, instruction, a, b),
                _ => Trilean.Unknown,
            };
        }

        /// <summary>
        /// Determines whether the branch condition has been met, based on two integer values.
        /// </summary>
        /// <param name="context">The context in which the instruction is being executed in.</param>
        /// <param name="instruction">The instruction that is being executed.</param>
        /// <param name="left">The left operand of the comparison.</param>
        /// <param name="right">The right operand of the comparison.</param>
        /// <returns><c>true</c> if the branch should be taken, <c>false</c> if not, and <see cref="Trilean.Unknown"/>
        /// if the conclusion is unknown.</returns>
        protected abstract Trilean VerifyCondition(CilExecutionContext context, CilInstruction instruction,
            IntegerValue left, IntegerValue right);
        
        /// <summary>
        /// Determines whether the branch condition has been met, based on two floating point values.
        /// </summary>
        /// <param name="context">The context in which the instruction is being executed in.</param>
        /// <param name="instruction">The instruction that is being executed.</param>
        /// <param name="left">The left operand of the comparison.</param>
        /// <param name="right">The right operand of the comparison.</param>
        /// <returns><c>true</c> if the branch should be taken, <c>false</c> if not, and <see cref="Trilean.Unknown"/>
        /// if the conclusion is unknown.</returns>
        protected abstract Trilean VerifyCondition(CilExecutionContext context, CilInstruction instruction,
            FValue left, FValue right);
        
        /// <summary>
        /// Determines whether the branch condition has been met, based on two object references.
        /// </summary>
        /// <param name="context">The context in which the instruction is being executed in.</param>
        /// <param name="instruction">The instruction that is being executed.</param>
        /// <param name="left">The left operand of the comparison.</param>
        /// <param name="right">The right operand of the comparison.</param>
        /// <returns><c>true</c> if the branch should be taken, <c>false</c> if not, and <see cref="Trilean.Unknown"/>
        /// if the conclusion is unknown.</returns>
        protected abstract Trilean VerifyCondition(CilExecutionContext context, CilInstruction instruction, 
            OValue left, OValue right);
    }
}