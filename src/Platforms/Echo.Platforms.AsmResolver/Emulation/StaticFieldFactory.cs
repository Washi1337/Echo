using System;
using System.Collections.Concurrent;
using AsmResolver.DotNet;
using Echo.Platforms.AsmResolver.Emulation.Values;
using Echo.Platforms.AsmResolver.Emulation.Values.Cli;

namespace Echo.Platforms.AsmResolver.Emulation
{
    /// <summary>
    /// Provides a mechanism for creating and storing static fields within an instance of a .NET virtual machine.
    /// </summary>
    public class StaticFieldFactory
    {
        private readonly ConcurrentDictionary<IFieldDescriptor, StaticField> _cache =
            new ConcurrentDictionary<IFieldDescriptor, StaticField>();
        
        /// <summary>
        /// Gets or creates an instance of a static field. 
        /// </summary>
        /// <param name="field">The field to encapsulate.</param>
        /// <returns>The static field.</returns>
        public StaticField Get(IFieldDescriptor field)
        {
            if (field is null)
                throw new ArgumentNullException(nameof(field));
            if (field.Signature.HasThis)
                throw new ArgumentException("Field has the HasThis flag set in the field signature.");
                
            StaticField staticField;
            while (!_cache.TryGetValue(field, out staticField))
                _cache.TryAdd(field, new StaticField(field));
            return staticField;
        }

    }
}