using System.Collections.Generic;

namespace Echo.Ast.Pattern
{
    /// <summary>
    /// Describes an object pattern matching result.
    /// </summary>
    public class MatchResult
    {
        /// <summary>
        /// Gets a value indicating whether the matching was successful.
        /// </summary>
        public bool IsSuccess
        {
            get;
        }

        /// <summary>
        /// Provides a collection of objects extracted from the input object.
        /// </summary>
        public IDictionary<CaptureGroup, IList<object>> Captures
        {
            get;
        } = new Dictionary<CaptureGroup, IList<object>>();
    }
}