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
            set;
        } = true;

        /// <summary>
        /// Provides a collection of objects extracted from the input object.
        /// </summary>
        public IDictionary<CaptureGroup, IList<object>> Captures
        {
            get;
        } = new Dictionary<CaptureGroup, IList<object>>();

        /// <summary>
        /// Adds an extracted object to a capture group.
        /// </summary>
        /// <param name="captureGroup">The capture group to add it to.</param>
        /// <param name="value">The extracted object to add.</param>
        public void AddCapturedObject(CaptureGroup captureGroup, object value)
        {
            if (!Captures.TryGetValue(captureGroup, out var objects))
            {
                objects = new List<object>();
                Captures.Add(captureGroup, objects);
            }

            objects.Add(value);
        }
    }
}