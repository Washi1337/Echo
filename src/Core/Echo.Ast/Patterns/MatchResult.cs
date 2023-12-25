using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Echo.Ast.Patterns
{
    /// <summary>
    /// Describes an object pattern matching result.
    /// </summary>
    public class MatchResult
    {
        private readonly Dictionary<CaptureGroup, IList> _captureGroups = new();
        
        /// <summary>
        /// Gets a value indicating whether the matching was successful.
        /// </summary>
        public bool IsSuccess
        {
            get;
            set;
        } = true;

        /// <summary>
        /// Gets all the groups that were captured during the matching process. 
        /// </summary>
        /// <returns>The groups.</returns>
        public IEnumerable<CaptureGroup> GetCaptureGroups() => _captureGroups.Keys;

        /// <summary>
        /// Gets all captured objects belonging to the provided capture group.
        /// </summary>
        /// <param name="captureGroup">The group to get the captured objects for.</param>
        /// <typeparam name="T">The type of objects that were captured.</typeparam>
        /// <returns>The captured groups</returns>
        public IList<T> GetCaptures<T>(CaptureGroup<T> captureGroup)
        {
            if (_captureGroups.TryGetValue(captureGroup, out var list))
                return (IList<T>) list;
            
            return ImmutableList<T>.Empty;
        }

        internal void MergeWith(MatchResult other)
        {
            foreach (var entry in other._captureGroups)
            {
                if (!_captureGroups.TryGetValue(entry.Key, out var list))
                {
                    _captureGroups.Add(entry.Key, entry.Value);   
                }
                else
                {
                    foreach (object capture in entry.Value)
                        list.Add(capture);
                }
            }
        }

        internal void Clear()
        {
            IsSuccess = true;
            _captureGroups.Clear();
        }

        /// <summary>
        /// Adds an extracted object to a capture group.
        /// </summary>
        /// <param name="captureGroup">The capture group to add it to.</param>
        /// <param name="value">The extracted object to add.</param>
        public void AddCapturedObject<T>(CaptureGroup<T> captureGroup, T value)
        {
            if (!_captureGroups.TryGetValue(captureGroup, out var objects))
            {
                objects = new List<T>();
                _captureGroups.Add(captureGroup, objects);
            }

            objects.Add(value);
        }
    }
}