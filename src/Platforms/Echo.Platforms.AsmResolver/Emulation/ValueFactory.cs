using System;
using System.Collections.Generic;
using AsmResolver.DotNet;
using AsmResolver.DotNet.Memory;
using AsmResolver.DotNet.Signatures.Types;

namespace Echo.Platforms.AsmResolver.Emulation
{
    /// <summary>
    /// Provides a service for querying information about- and constructing new values.
    /// </summary>
    public class ValueFactory
    {
        private readonly Dictionary<ITypeDescriptor, TypeMemoryLayout> _memoryLayouts = new();

        /// <summary>
        /// Creates a new value factory.
        /// </summary>
        /// <param name="contextModule">The manifest module to use for context.</param>
        /// <param name="is32Bit">A value indicating whether the environment is a 32-bit or 64-bit system.</param>
        public ValueFactory(ModuleDefinition contextModule, bool is32Bit)
        {
            ContextModule = contextModule;
            Is32Bit = is32Bit;
        }

        /// <summary>
        /// Gets the manifest module to use for context.
        /// </summary>
        public ModuleDefinition ContextModule
        {
            get;
        }
        
        /// <summary>
        /// Gets a value indicating whether the environment is a 32-bit or 64-bit system.
        /// </summary>
        public bool Is32Bit
        {
            get;
        }
        
        /// <summary>
        /// Obtains the memory layout of a type in the current environment.
        /// </summary>
        /// <param name="type">The type to measure.</param>
        /// <returns>The measured layout.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Occurs when the type could not be measured.</exception>
        public TypeMemoryLayout GetTypeMemoryLayout(ITypeDescriptor type)
        {
            type = ContextModule.CorLibTypeFactory.FromType(type) ?? type;
            if (!_memoryLayouts.TryGetValue(type, out var memoryLayout))
            {
                memoryLayout = type switch
                {
                    ITypeDefOrRef typeDefOrRef => typeDefOrRef.GetImpliedMemoryLayout(Is32Bit),
                    TypeSignature signature => signature.GetImpliedMemoryLayout(Is32Bit),
                    _ => throw new ArgumentOutOfRangeException(nameof(type))
                };

                _memoryLayouts[type] = memoryLayout;
            }

            return memoryLayout;
        }
    }
}