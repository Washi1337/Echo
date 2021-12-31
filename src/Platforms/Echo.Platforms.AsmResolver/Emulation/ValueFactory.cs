using System;
using System.Collections.Generic;
using AsmResolver.DotNet;
using AsmResolver.DotNet.Memory;
using AsmResolver.DotNet.Signatures.Types;

namespace Echo.Platforms.AsmResolver.Emulation
{
    public class ValueFactory
    {
        private readonly Dictionary<ITypeDescriptor, TypeMemoryLayout> _memoryLayouts = new();

        public ValueFactory(ModuleDefinition contextModule, bool is32Bit)
        {
            ContextModule = contextModule;
            Is32Bit = is32Bit;
        }

        public ModuleDefinition ContextModule
        {
            get;
        }
        
        public bool Is32Bit
        {
            get;
        }
        
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