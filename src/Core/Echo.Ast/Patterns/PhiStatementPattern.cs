using System.Collections.Generic;
using System.Linq;
using Echo.Core.Code;

namespace Echo.Ast.Patterns
{
    /// <summary>
    /// Describes a pattern that matches on instances of <see cref="PhiStatement{TInstruction}"/>.
    /// </summary>
    /// <typeparam name="TInstruction">The type of instructions stored in the abstract syntax tree.</typeparam>
    public class PhiStatementPattern<TInstruction> : StatementPattern<TInstruction>
    {
        /// <summary>
        /// Creates a new phi statement that matches on any target and source variables.
        /// </summary>
        public PhiStatementPattern()
        {
            Target = Pattern.Any<IVariable>();
            AnySources = true;
        }
        
        /// <summary>
        /// Creates a new phi statement pattern that matches on any source variables.
        /// </summary>
        /// <param name="target">The pattern describing the target variable.</param>
        public PhiStatementPattern(Pattern<IVariable> target)
        {
            Target = target;
            AnySources = true;
        }
        
        /// <summary>
        /// Gets or sets the pattern describing the target of the phi statement.
        /// </summary>
        public Pattern<IVariable> Target
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating any number of sources is accepted or not.
        /// </summary>
        public bool AnySources
        {
            get;
            set;
        } = true;

        /// <summary>
        /// Gets a collection of patterns that describe the sources in the phi statement.
        /// </summary>
        /// <remarks>
        /// If <see cref="AnySources"/> is set to <c>true</c>, this property is ignored.
        /// </remarks>
        public IList<Pattern<VariableExpression<TInstruction>>> Sources
        {
            get;
        } = new List<Pattern<VariableExpression<TInstruction>>>();

        /// <inheritdoc />
        protected override void MatchChildren(StatementBase<TInstruction> input, MatchResult result)
        {
            // Test whether the expression is an instruction expression.
            if (!(input is PhiStatement<TInstruction> statement))
            {
                result.IsSuccess = false;
                return;
            }

            // Match contents.
            Target.Match(statement.Target, result);
            if (!result.IsSuccess)
                return;

            // Match sources.
            if (!AnySources)
            {
                if (statement.Sources.Count != Sources.Count)
                {
                    result.IsSuccess = false;
                    return;
                }

                // TODO: remove ToArray() and use indexing in the Sources property directly.
                var sources = statement.Sources.ToArray();
                for (int i = 0; i < Sources.Count && result.IsSuccess; i++)
                    Sources[i].Match(sources[i], result);
            }
        }

        /// <summary>
        /// Sets the target variable patterns to the provided expression patterns.
        /// </summary>
        /// <param name="target">The pattern describing the target of the phi node.</param>
        /// <returns>The current pattern.</returns>
        public PhiStatementPattern<TInstruction> WithTarget(Pattern<IVariable> target)
        {
            Target = target;
            return this;
        }

        /// <summary>
        /// Indicate any number of sources is allowed. 
        /// </summary>
        /// <returns>The current pattern.</returns>
        public PhiStatementPattern<TInstruction> WithAnySources()
        {
            Sources.Clear();
            AnySources = true;
            return this;
        }

        /// <summary>
        /// Sets the source patterns to the provided expression patterns.
        /// </summary>
        /// <param name="sources">The patterns that describe the sources of the phi node.</param>
        /// <returns>The current pattern.</returns>
        public PhiStatementPattern<TInstruction> WithSources(params Pattern<VariableExpression<TInstruction>>[] sources)
        {
            return WithSources(sources.ToArray());
        }

        /// <summary>
        /// Sets the source patterns to the provided expression patterns.
        /// </summary>
        /// <param name="sources">The patterns that describe the sources of the phi node.</param>
        /// <returns>The current pattern.</returns>
        public PhiStatementPattern<TInstruction> WithSources(IEnumerable<Pattern<VariableExpression<TInstruction>>> sources)
        {
            AnySources = false;

            Sources.Clear();
            foreach (var source in sources)
                Sources.Add(source);

            return this;
        }
    }
}