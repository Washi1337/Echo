using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Echo.Ast.Patterns
{
    /// <summary>
    /// Describes an expression pattern that matches on an instance of a <see cref="InstructionExpression{TInstruction}"/>. 
    /// </summary>
    /// <typeparam name="TInstruction">The type of instruction that is stored in the expression.</typeparam>
    public class InstructionExpressionPattern<TInstruction> : ExpressionPattern<TInstruction>
        where TInstruction : notnull
    {
        /// <summary>
        /// Creates a new instruction expression pattern describing an instruction expression with zero parameters.
        /// </summary>
        /// <param name="instruction">The pattern describing the instruction that is stored in the expression.</param>
        public InstructionExpressionPattern(Pattern<TInstruction> instruction)
        {
            Instruction = instruction;
            AnyArguments = false;
        }

        /// <summary>
        /// Creates a new instruction expression pattern.
        /// </summary>
        /// <param name="instruction">The pattern describing the instruction that is stored in the expression.</param>
        /// <param name="anyArguments"><c>true</c> if any arguments should be accepted, <c>false</c> to indicate the
        /// expression should have zero parameters.</param>
        public InstructionExpressionPattern(Pattern<TInstruction> instruction, bool anyArguments)
        {
            Instruction = instruction;
            AnyArguments = anyArguments;
        }

        /// <summary>
        /// Creates a new instruction expression pattern.
        /// </summary>
        /// <param name="instruction">The pattern describing the instruction that is stored in the expression.</param>
        /// <param name="arguments">The list of patterns that describe the arguments of the input expression should match with.</param>
        public InstructionExpressionPattern(Pattern<TInstruction> instruction, params Pattern<Expression<TInstruction>>[] arguments)
        {
            Instruction = instruction;
            AnyArguments = false;
            foreach (var argument in arguments)
                Arguments.Add(argument);
        }

        /// <summary>
        /// Creates a new instruction expression pattern.
        /// </summary>
        /// <param name="instruction">The pattern describing the instruction that is stored in the expression.</param>
        /// <param name="arguments">The list of patterns that describe the arguments of the input expression should match with.</param>
        public InstructionExpressionPattern(Pattern<TInstruction> instruction, IEnumerable<Pattern<Expression<TInstruction>>> arguments)
        {
            Instruction = instruction;
            AnyArguments = false;
            foreach (var argument in arguments)
                Arguments.Add(argument);
        }

        /// <summary>
        /// Gets or sets a pattern describing the instruction that is stored in the expression. 
        /// </summary>
        public Pattern<TInstruction> Instruction
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the pattern should match on any number of arguments in the
        /// input expression. 
        /// </summary>
        public bool AnyArguments
        {
            get;
            set;
        }

        /// <summary>
        /// Gets an ordered list of patterns that describe the arguments of the input expression should match with.
        /// </summary>
        /// <remarks>
        /// When <see cref="AnyArguments"/> is set to <c>true</c>, this property is ignored.
        /// </remarks>
        public IList<Pattern<Expression<TInstruction>>> Arguments
        {
            get;
        } = new List<Pattern<Expression<TInstruction>>>();
        
        /// <inheritdoc />
        protected override void MatchChildren(Expression<TInstruction> input, MatchResult result)
        {
            // Test whether the expression is an instruction expression.
            if (!(input is InstructionExpression<TInstruction> expression))
            {
                result.IsSuccess = false;
                return;
            }

            // Match contents.
            Instruction.Match(expression.Instruction, result);
            if (!result.IsSuccess)
                return;

            // Match arguments.
            if (!AnyArguments)
            {
                if (expression.Arguments.Count != Arguments.Count)
                {
                    result.IsSuccess = false;
                    return;
                }

                // TODO: remove ToArray() and use indexing in the Parameters property directly.       
                var arguments = expression.Arguments.ToArray();
                for (int i = 0; i < Arguments.Count && result.IsSuccess; i++)
                    Arguments[i].Match(arguments[i], result);
            }
        }

        /// <summary>
        /// Indicates the pattern should match on instances with the provided number of arguments.
        /// </summary>
        /// <param name="argumentCount">The number of arguments the expression should have.</param>
        /// <returns>The current pattern.</returns>
        public InstructionExpressionPattern<TInstruction> WithArguments(int argumentCount)
        {
            Arguments.Clear();

            for (int i = 0; i < argumentCount; i++)
                Arguments.Add(ExpressionPattern.Any<TInstruction>());
            
            return this;
        }

        /// <summary>
        /// Sets the argument patterns to the provided expression patterns.
        /// </summary>
        /// <param name="arguments">The patterns that describe the arguments of the expression.</param>
        /// <returns>The pattern.</returns>
        public InstructionExpressionPattern<TInstruction> WithArguments(
            params Pattern<Expression<TInstruction>>[] arguments)
        {
            Arguments.Clear();
            foreach (var argument in arguments)
                Arguments.Add(argument);
            return this;
        }

        /// <summary>
        /// Sets the argument patterns to the provided expression patterns.
        /// </summary>
        /// <param name="arguments">The patterns that describe the arguments of the expression.</param>
        /// <returns>The pattern.</returns>
        public InstructionExpressionPattern<TInstruction> WithArguments(
            IEnumerable<Pattern<Expression<TInstruction>>> arguments)
        {
            Arguments.Clear();
            foreach (var argument in arguments)
                Arguments.Add(argument);
            return this;
        }

        /// <summary>
        /// Indicate any number of arguments is allowed. 
        /// </summary>
        /// <returns>The pattern.</returns>
        public InstructionExpressionPattern<TInstruction> WithAnyArguments()
        {
            Arguments.Clear();
            AnyArguments = true;
            return this;
        }

        /// <summary>
        /// Indicates the instruction should be captured in a certain group.
        /// </summary>
        /// <param name="captureGroup">The group.</param>
        /// <returns>The current pattern.</returns>
        public InstructionExpressionPattern<TInstruction> CaptureInstruction(CaptureGroup<TInstruction> captureGroup)
        {
            Instruction.CaptureAs(captureGroup);
            return this;
        }

        /// <summary>
        /// Indicates all arguments should be captured in a certain group.
        /// </summary>
        /// <param name="captureGroup">The group.</param>
        /// <returns>The current pattern.</returns>
        public InstructionExpressionPattern<TInstruction> CaptureArguments(CaptureGroup<Expression<TInstruction>> captureGroup)
        {
            foreach (var argument in Arguments)
                argument.CaptureAs(captureGroup);
            
            return this;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            var builder = new StringBuilder();
            
            builder.Append(Instruction);
            
            builder.Append('(');
            if (AnyArguments)
            {
                builder.Append('*');
            }
            else
            {
                for (int i = 0; i < Arguments.Count; i++)
                {
                    builder.Append(Arguments[i]);
                    if (i < Arguments.Count - 1)
                        builder.Append(", ");
                }
            }

            builder.Append(')');
            
            return builder.ToString();
        }
        
    }
}