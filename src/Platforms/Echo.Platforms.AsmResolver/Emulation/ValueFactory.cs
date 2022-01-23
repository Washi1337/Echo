using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AsmResolver;
using AsmResolver.DotNet;
using AsmResolver.DotNet.Memory;
using AsmResolver.DotNet.Signatures;
using AsmResolver.DotNet.Signatures.Types;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;
using Echo.Concrete;
using Echo.Platforms.AsmResolver.Emulation.Runtime;

namespace Echo.Platforms.AsmResolver.Emulation
{
    /// <summary>
    /// Provides a service for querying information about- and constructing new values.
    /// </summary>
    public class ValueFactory
    {
        private static readonly MethodInfo SetField;
        private static readonly MethodInfo SetSize;

        private readonly Dictionary<ITypeDescriptor, TypeMemoryLayout> _contentLayouts = new(new SignatureComparer());
        private readonly Dictionary<ITypeDescriptor, TypeMemoryLayout> _valueLayouts = new(new SignatureComparer());

        static ValueFactory()
        {
            SetField = typeof(TypeMemoryLayout).GetMethod("set_Item", (BindingFlags) (-1))!;
            SetSize = typeof(TypeMemoryLayout).GetMethod("set_Size", (BindingFlags) (-1))!;
        }
        
        /// <summary>
        /// Creates a new value factory.
        /// </summary>
        /// <param name="contextModule">The manifest module to use for context.</param>
        /// <param name="is32Bit">A value indicating whether the environment is a 32-bit or 64-bit system.</param>
        public ValueFactory(ModuleDefinition contextModule, bool is32Bit)
        {
            ContextModule = contextModule;
            Is32Bit = is32Bit;
            PointerSize = is32Bit ? 4u : 8u;
            ClrMockMemory = new ClrMockMemory();
            BitVectorPool = new BitVectorPool();

            InvalidProgramExceptionType = new TypeReference(
                    contextModule, 
                    contextModule.CorLibTypeFactory.CorLibScope, 
                    "System",
                    "InvalidProgramException").Resolve()!;
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
        /// Gets the size of a pointer in the current environment.
        /// </summary>
        public uint PointerSize
        {
            get;
        }

        /// <summary>
        /// Gets the CLR mock memory used for managing method tables (types).
        /// </summary>
        public ClrMockMemory ClrMockMemory
        {
            get;
        }

        /// <summary>
        /// Gets a reference to the <see cref="InvalidProgramException"/> type. 
        /// </summary>
        public ITypeDescriptor InvalidProgramExceptionType
        {
            get;
        }

        /// <summary>
        /// Gets the bitvector pool that this factory uses for creating and reusing bit vectors. 
        /// </summary>
        public BitVectorPool BitVectorPool
        {
            get;
        }

        /// <summary>
        /// Gets the size of an object header.
        /// </summary>
        public uint ObjectHeaderSize => PointerSize;

        /// <summary>
        /// Gets the number of bytes prepended to the data of the array.
        /// </summary>
        public uint ArrayHeaderSize => ObjectHeaderSize * 2;

        /// <summary>
        /// Gets the offset within an array object that indexes the start of the Length field.
        /// </summary>
        public uint ArrayLengthOffset => ObjectHeaderSize;

        /// <summary>
        /// Gets the number of bytes prepended to the characters of the string.
        /// </summary>
        public uint StringHeaderSize => ObjectHeaderSize + sizeof(uint);

        /// <summary>
        /// Gets the offset within an string object that indexes the start of the Length field.
        /// </summary>
        public uint StringLengthOffset => ObjectHeaderSize;

        /// <summary>
        /// Computes the total size of an array object.
        /// </summary>
        /// <param name="elementType">The type of elements the array stores.</param>
        /// <param name="elementCount">The number of elements.</param>
        /// <returns>The total size in bytes.</returns>
        public uint GetArrayObjectSize(ITypeDescriptor elementType, int elementCount)
        {
            uint elementSize = elementType.IsValueType
                ? GetTypeValueMemoryLayout(elementType).Size
                : PointerSize;

            return ArrayHeaderSize + elementSize * (uint) elementCount;
        }

        /// <summary>
        /// Computes the total size of a string object.
        /// </summary>
        /// <param name="length">The number of characters in the string.</param>
        /// <returns>The total size in bytes.</returns>
        public uint GetStringObjectSize(int length) => StringHeaderSize + sizeof(char) * (uint) length;

        /// <summary>
        /// Computes the total size of an object.
        /// </summary>
        /// <param name="type">The type of object to measure.</param>
        /// <returns>The total size in bytes.</returns>
        public uint GetObjectSize(ITypeDescriptor type) => ObjectHeaderSize + GetTypeContentsMemoryLayout(type).Size;
        
        /// <summary>
        /// Obtains the memory layout of a type in the current environment. If the provided type is a reference type,
        /// then it will measure the object reference itself, and not the contents behind the reference.
        /// </summary>
        /// <param name="type">The type to measure.</param>
        /// <returns>The measured layout.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Occurs when the type could not be measured.</exception>
        public TypeMemoryLayout GetTypeValueMemoryLayout(ITypeDescriptor type)
        {
            type = ContextModule.CorLibTypeFactory.FromType(type) ?? type;
            if (!_valueLayouts.TryGetValue(type, out var memoryLayout))
            {
                memoryLayout = type switch
                {
                    ITypeDefOrRef typeDefOrRef => typeDefOrRef.GetImpliedMemoryLayout(Is32Bit),
                    TypeSignature signature => signature.GetImpliedMemoryLayout(Is32Bit),
                    _ => throw new ArgumentOutOfRangeException(nameof(type))
                };

                _valueLayouts[type] = memoryLayout;
            }

            return memoryLayout;
        }
        
        /// <summary>
        /// Obtains the memory layout of a type in the current environment. If the provided type is a reference type,
        /// then it will measure the size of the contents behind the object reference. 
        /// </summary>
        /// <param name="type">The type to measure.</param>
        /// <returns>The measured layout.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Occurs when the type could not be measured.</exception>
        public TypeMemoryLayout GetTypeContentsMemoryLayout(ITypeDescriptor type)
        {
            type = ContextModule.CorLibTypeFactory.FromType(type) ?? type;
            if (!_contentLayouts.TryGetValue(type, out var memoryLayout))
            {
                memoryLayout = type switch
                {
                    ITypeDefOrRef typeDefOrRef => GetTypeDefOrRefContentsLayout(typeDefOrRef, default),
                    TypeSignature signature => signature.GetImpliedMemoryLayout(Is32Bit),
                    _ => throw new ArgumentOutOfRangeException(nameof(type))
                };

                _contentLayouts[type] = memoryLayout;
            }

            return memoryLayout;
        }
        
        private TypeMemoryLayout GetTypeDefOrRefContentsLayout(ITypeDefOrRef type, GenericContext context)
        {
            if (type is TypeSpecification {Signature: { } signature})
                return GetTypeSignatureContentsLayout(signature);

            if (type.Resolve() is not {IsValueType: false} definition)
                return type.GetImpliedMemoryLayout(Is32Bit);
            
            // Hack: AsmResolver currently does not support layout detection of reference types, mainly because it
            // is CLR implementation specific. Therefore, we "invent" a new layout ourselves instead for our minimal
            // "implementation" of the CLR. This does not have to be very accurate for most things to work, as programs
            // cannot really reliably assume the exact layout of these classes anyways. 
                
            // This does mean however that we need to manually construct a type layout through some reflection
            // since the setters in TypeMemoryLayout are marked internal. This probably is worthy of a change in 
            // AsmResolver's API in the future.
            
            var layout = new TypeMemoryLayout(definition, 0,
                Is32Bit ? MemoryLayoutAttributes.Is32Bit : MemoryLayoutAttributes.Is64Bit);
            
            uint currentOffset = 0;

            while (definition is not null)
            {
                foreach (var field in definition.Fields)
                {
                    if (field.IsStatic || field.Signature is null)
                        continue;

                    var contentsLayout = field.Signature.FieldType
                        .InstantiateGenericTypes(context)
                        .GetImpliedMemoryLayout(Is32Bit);

                    var fieldLayout = new FieldMemoryLayout(field, currentOffset, contentsLayout);

                    SetField.Invoke(layout, new object[] {field, fieldLayout});
                    currentOffset = (currentOffset + contentsLayout.Size).Align(PointerSize);
                }

                definition = definition.BaseType?.Resolve();
            }

            SetSize.Invoke(layout, new object[] {currentOffset});
            return layout;
        }

        private TypeMemoryLayout GetTypeSignatureContentsLayout(TypeSignature type)
        {
            switch (type.ElementType)
            {
                case ElementType.Class:
                case ElementType.GenericInst:
                    return GetTypeDefOrRefContentsLayout(type.GetUnderlyingTypeDefOrRef()!, GenericContext.FromType(type));

                default:
                    return type.GetImpliedMemoryLayout(Is32Bit);
            }
        }
    }
}