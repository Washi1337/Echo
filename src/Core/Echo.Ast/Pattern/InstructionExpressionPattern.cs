using System.Collections.Generic;
using System.Linq;

namespace Echo.Ast.Pattern
{
    /// <summary>
    /// Describes an expression pattern that matches on an instance of a <see cref="InstructionExpressionPattern{TInstruction}"/>. 
    /// </summary>
    /// <typeparam name="TInstruction">The type of instruction that is stored in the expression.</typeparam>
    public class InstructionExpressionPattern<TInstruction> : ExpressionPattern<TInstruction>
    {
        /// <summary>
        /// Creates a new instruction expression pattern describing an instruction expression with zero parameters.
        /// </summary>
        /// <param name="content">The pattern describing the instruction that is stored in the expression.</param>
        public InstructionExpressionPattern(Pattern<TInstruction> content)
        {
            Content = content;
            AnyArguments = false;
        }

        /// <summary>
        /// Creates a new instruction expression pattern.
        /// </summary>
        /// <param name="content">The pattern describing the instruction that is stored in the expression.</param>
        /// <param name="anyArguments"><c>true</c> if any arguments should be accepted, <c>false</c> to indicate the
        /// expression should have zero parameters.</param>
        public InstructionExpressionPattern(Pattern<TInstruction> content, bool anyArguments)
        {
            Content = content;
            AnyArguments = anyArguments;
        }

        /// <summary>
        /// Creates a new instruction expression pattern.
        /// </summary>
        /// <param name="content">The pattern describing the instruction that is stored in the expression.</param>
        /// <param name="arguments">The list of patterns that describe the arguments of the input expression should match with.</param>
        public InstructionExpressionPattern(Pattern<TInstruction> content, params Pattern<AstExpressionBase<TInstruction>>[] arguments)
        {
            Content = content;
            AnyArguments = false;
            foreach (var argument in arguments)
                Arguments.Add(argument);
        }

        /// <summary>
        /// Creates a new instruction expression pattern.
        /// </summary>
        /// <param name="content">The pattern describing the instruction that is stored in the expression.</param>
        /// <param name="arguments">The list of patterns that describe the arguments of the input expression should match with.</param>
        public InstructionExpressionPattern(Pattern<TInstruction> content, IEnumerable<Pattern<AstExpressionBase<TInstruction>>> arguments)
        {
            Content = content;
            AnyArguments = false;
            foreach (var argument in arguments)
                Arguments.Add(argument);
        }

        /// <summary>
        /// Gets or sets a pattern describing the instruction that is stored in the expression. 
        /// </summary>
        public Pattern<TInstruction> Content
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
        public IList<Pattern<AstExpressionBase<TInstruction>>> Arguments
        {
            get;
        } = new List<Pattern<AstExpressionBase<TInstruction>>>();
        
        /// <inheritdoc />
        protected override void MatchChildren(AstExpressionBase<TInstruction> input, MatchResult result)
        {
            // Test whether the expression is an instruction expression.
            if (!(input is AstInstructionExpression<TInstruction> expression))
            {
                result.IsSuccess = false;
                return;
            }

            // Match contents.
            Content.Match(expression.Content, result);
            if (!result.IsSuccess)
                return;

            // Match arguments.
            if (!AnyArguments)
            {
                if (expression.Parameters.Count != Arguments.Count)
                {
                    result.IsSuccess = false;
                    return;
                }

                // TODO: remove ToArray() and use indexing in the Parameters property directly.       
                var arguments = expression.Parameters.ToArray();
                for (int i = 0; i < Arguments.Count && result.IsSuccess; i++)
                    Arguments[i].Match(arguments[i], result);
            }
        }

        /// <summary>
        /// Sets the argument patterns to the provided expression patterns.
        /// </summary>
        /// <param name="arguments">The patterns that describe the arguments of the expression.</param>
        /// <returns>The pattern.</returns>
        public InstructionExpressionPattern<TInstruction> WithArguments(
            params Pattern<AstExpressionBase<TInstruction>>[] arguments)
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
            IEnumerable<Pattern<AstExpressionBase<TInstruction>>> arguments)
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

    }
}