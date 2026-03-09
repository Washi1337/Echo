using System;
using System.Collections.Generic;
using System.Reflection;
using AsmResolver;
using AsmResolver.DotNet;
using AsmResolver.DotNet.Memory;
using AsmResolver.DotNet.Signatures;
using AsmResolver.PE.DotNet.Metadata.Tables;
using Echo.Memory;
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

        private readonly Dictionary<ITypeDescriptor, TypeMemoryLayout> _contentLayouts = new(SignatureComparer.Default);
        private readonly Dictionary<ITypeDescriptor, TypeMemoryLayout> _valueLayouts = new(SignatureComparer.Default);

        static ValueFactory()
        {
            SetField = typeof(TypeMemoryLayout).GetMethod("set_Item", (BindingFlags) (-1))!;
            SetSize = typeof(TypeMemoryLayout).GetMethod("set_Size", (BindingFlags) (-1))!;
        }
        
        /// <summary>
        /// Creates a new value factory.
        /// </summary>
        /// <param name="runtimeContext">The runtime context to assume when creating values and managing types.</param>
        /// <param name="is32Bit">A value indicating whether the environment is a 32-bit or 64-bit system.</param>
        public ValueFactory(RuntimeContext runtimeContext, bool is32Bit)
        {
            RuntimeContext = runtimeContext;
            Is32Bit = is32Bit;
            PointerSize = is32Bit ? 4u : 8u;
            ClrMockMemory = new ClrMockMemory();
            BitVectorPool = new BitVectorPool();
            Marshaller = new CliMarshaller(this);
            var corLibScope = RuntimeContext.RuntimeCorLib?.ToAssemblyReference()
                ?? throw new ArgumentException("The provided runtime context does not have an implementation.");
            
            // TODO: we still rely on some old behavior where we need ContextModule to be set in some fringe situations. Remove this.
            var dummyModule = new ModuleDefinition("Root", corLibScope);
            Importer = dummyModule.DefaultImporter;
            CorLibTypeFactory = dummyModule.CorLibTypeFactory;
            // CorLibTypeFactory = new CorLibTypeFactory(corLibScope);

            // Force System.String to be aligned at 4 bytes (required for low level string APIs).
            GetTypeDefOrRefContentsLayout(CorLibTypeFactory.String.Type, default, 4);

            DelegateType = corLibScope
                .CreateTypeReference(nameof(System), nameof(Delegate))
                .Resolve(runtimeContext);

            DelegateTargetField = DelegateType.ToTypeDefOrRef()
                .CreateMemberReference("_target", new FieldSignature(CorLibTypeFactory.Object));

            DelegateMethodPtrField = DelegateType.ToTypeDefOrRef()
                .CreateMemberReference("_methodPtr", new FieldSignature(CorLibTypeFactory.IntPtr));

            DecimalType = corLibScope
                .CreateTypeReference(nameof(System), nameof(Decimal))
                .Resolve(runtimeContext);
            
            InvalidProgramExceptionType = corLibScope
                .CreateTypeReference(nameof(System), nameof(InvalidProgramException))
                .Resolve(runtimeContext);
            
            TypeInitializationExceptionType = corLibScope
                .CreateTypeReference(nameof(System), nameof(TypeInitializationException))
                .Resolve(runtimeContext);
            
            NullReferenceExceptionType = corLibScope
                .CreateTypeReference(nameof(System), nameof(NullReferenceException))
                .Resolve(runtimeContext);
            
            InvalidProgramExceptionType = corLibScope
                .CreateTypeReference(nameof(System), nameof(InvalidProgramException))
                .Resolve(runtimeContext);
            
            IndexOutOfRangeExceptionType = corLibScope
                .CreateTypeReference(nameof(System), nameof(IndexOutOfRangeException))
                .Resolve(runtimeContext);
            
            StackOverflowExceptionType = corLibScope
                .CreateTypeReference(nameof(System), nameof(StackOverflowException))
                .Resolve(runtimeContext);
            
            MissingMethodExceptionType = corLibScope
                .CreateTypeReference(nameof(System), nameof(MissingMethodException))
                .Resolve(runtimeContext);
            
            InvalidCastExceptionType = corLibScope
                .CreateTypeReference(nameof(System), nameof(InvalidCastException))
                .Resolve(runtimeContext);

            OverflowExceptionType = corLibScope
                .CreateTypeReference(nameof(System), nameof(OverflowException))
                .Resolve(runtimeContext);
        }

        /// <summary>
        /// Gets the runtime context the value factory assumes when managing types.
        /// </summary>
        public RuntimeContext RuntimeContext
        {
            get;
        }

        /// <summary>
        /// Gets the importer that is associated to the <see cref="ValueFactory"/>.
        /// </summary>
        public ReferenceImporter Importer
        {
            get;
        }

        /// <summary>
        /// Gets the corlib type factory that is associated to the <see cref="ValueFactory"/>.
        /// </summary>
        public CorLibTypeFactory CorLibTypeFactory
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
        /// Gets the size in bytes of a pointer in the current environment.
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
        /// Gets a reference to the <see cref="decimal"/> type. 
        /// </summary>
        public ITypeDefOrRef DecimalType
        {
            get;
        }

        /// <summary>
        /// Gets a reference to the <see cref="Delegate"/> type. 
        /// </summary>
        public ITypeDefOrRef DelegateType
        {
            get;
        }

        /// <summary>
        /// Get a reference to the <see cref="Delegate"/> _target field.
        /// </summary>
        public IFieldDescriptor DelegateTargetField
        {
            get;
        }

        /// <summary>
        /// Get a reference to the <see cref="Delegate"/> _methodPtr field.
        /// </summary>
        public IFieldDescriptor DelegateMethodPtrField
        {
            get;
        }

        /// <summary>
        /// Gets a reference to the <see cref="InvalidProgramException"/> type. 
        /// </summary>
        public ITypeDefOrRef InvalidProgramExceptionType
        {
            get;
        }

        /// <summary>
        /// Gets a reference to the <see cref="TypeInitializationException"/> type. 
        /// </summary>
        public TypeDefinition TypeInitializationExceptionType
        {
            get;
        }

        /// <summary>
        /// Gets a reference to the <see cref="NullReferenceException"/> type. 
        /// </summary>
        public ITypeDefOrRef NullReferenceExceptionType
        {
            get;
        }

        /// <summary>
        /// Gets a reference to the <see cref="IndexOutOfRangeException"/> type. 
        /// </summary>
        public ITypeDefOrRef IndexOutOfRangeExceptionType
        {
            get;
        }

        /// <summary>
        /// Gets a reference to the <see cref="StackOverflowException"/> type. 
        /// </summary>
        public ITypeDefOrRef StackOverflowExceptionType
        {
            get;
        }

        /// <summary>
        /// Gets a reference to the <see cref="MissingMethodException"/> type. 
        /// </summary>
        public ITypeDefOrRef MissingMethodExceptionType
        {
            get;
        }

        /// <summary>
        /// Gets a reference to the <see cref="InvalidCastException"/> type. 
        /// </summary>
        public ITypeDefOrRef InvalidCastExceptionType
        {
            get;
        }

        /// <summary>
        /// Gets a reference to the <see cref="OverflowException"/> type. 
        /// </summary>
        public ITypeDefOrRef OverflowExceptionType
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
        /// Gets the service responsible for marshalling values into stack slots and back. 
        /// </summary>
        public CliMarshaller Marshaller
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
        public uint ArrayHeaderSize => ObjectHeaderSize + PointerSize;

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
            uint elementSize = elementType.GetIsValueType(RuntimeContext)
                ? GetTypeValueMemoryLayout(elementType).Size
                : PointerSize;

            return ArrayHeaderSize + elementSize * (uint) elementCount;
        }

        /// <summary>
        /// Computes the total size of a string object.
        /// </summary>
        /// <param name="length">The number of characters in the string.</param>
        /// <returns>The total size in bytes.</returns>
        public uint GetStringObjectSize(int length) => StringHeaderSize + sizeof(char) * (uint)(length + 1);

        /// <summary>
        /// Computes the total size of an object.
        /// </summary>
        /// <param name="type">The type of object to measure.</param>
        /// <returns>The total size in bytes.</returns>
        public uint GetObjectSize(ITypeDescriptor type) => ObjectHeaderSize + GetTypeContentsMemoryLayout(type).Size;

        /// <summary>
        /// Creates a new native integer bit vector containing the null reference.
        /// </summary>
        /// <returns>The constructed bit vector.</returns>
        public BitVector CreateNull() => CreateNativeInteger(true);

        /// <summary>
        /// Creates a new native integer bit vector.
        /// </summary>
        /// <param name="initialize">
        /// <c>true</c> if the value should be set to 0, <c>false</c> if the integer should remain unknown.
        /// </param>
        /// <returns>The constructed bit vector.</returns>
        public BitVector CreateNativeInteger(bool initialize) => new((int) PointerSize * 8, initialize);
        
        /// <summary>
        /// Creates a new native integer bit vector.
        /// </summary>
        /// <param name="value">The value to initialize the integer with.</param>
        /// <returns>The constructed bit vector.</returns>
        public BitVector CreateNativeInteger(long value)
        {
            var vector = new BitVector((int) (PointerSize * 8), false);
            vector.AsSpan().WriteNativeInteger(value, Is32Bit);
            return vector;
        }

        /// <summary>
        /// Creates a new 32-bit vector from the bit vector pool containing the boolean value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The vector.</returns>
        public BitVector CreateBoolean(Trilean value)
        {
            var vector = new BitVector(32, true);
            var span = vector.AsSpan();
            span[0] = value;
            return vector;
        }

        /// <summary>
        /// Rents a new native integer bit vector containing the null reference.
        /// </summary>
        /// <returns>The constructed bit vector.</returns>
        public BitVector RentNull() => RentNativeInteger(true);

        /// <summary>
        /// Rents a native integer bit vector from the bit vector pool.
        /// </summary>
        /// <param name="initialize">
        /// <c>true</c> if the value should be set to 0, <c>false</c> if the integer should remain unknown.
        /// </param>
        /// <returns>The rented bit vector.</returns>
        public BitVector RentNativeInteger(bool initialize) => BitVectorPool.RentNativeInteger(Is32Bit, initialize);

        /// <summary>
        /// Rents a native integer bit vector from the bit vector pool.
        /// </summary>
        /// <param name="value">The value to initialize the integer with.</param>
        /// <returns>The rented bit vector.</returns>
        public BitVector RentNativeInteger(long value)
        {
            var vector = BitVectorPool.RentNativeInteger(Is32Bit, false);
            vector.AsSpan().WriteNativeInteger(value, Is32Bit);
            return vector;
        }

        /// <summary>
        /// Rents a 32-bit vector from the bit vector pool containing the boolean value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The vector</returns>
        public BitVector RentBoolean(Trilean value)
        {
            var vector = BitVectorPool.Rent(32, true);
            var span = vector.AsSpan();
            span[0] = value;
            return vector;
        }
        
        /// <summary>
        /// Creates a new bit vector that can be used to represent an instance of the provided type.
        /// </summary>
        /// <param name="type">The type to represent.</param>
        /// <param name="initialize">
        /// <c>true</c> if the value should be set to 0, <c>false</c> if the value should remain unknown.
        /// </param>
        /// <returns>The constructed bit vector.</returns>
        public BitVector CreateValue(TypeSignature type, bool initialize)
        {
            type = type.StripModifiers();
            uint size = GetTypeValueMemoryLayout(type).Size;
            if (type.ElementType != ElementType.Boolean)
                return new BitVector((int) size * 8, initialize);
            
            // For booleans, we only set the LSB to unknown if necessary.
            var result = new BitVector((int) size * 8, true);
            
            if (!initialize)
            {
                var span = result.AsSpan();
                span[0] = Trilean.Unknown;
            }

            return result;
        }
        
        /// <summary>
        /// Rents a bit vector from the pool that can be used to represent an instance of the provided type.
        /// </summary>
        /// <param name="type">The type to represent.</param>
        /// <param name="initialize">
        /// <c>true</c> if the value should be set to 0, <c>false</c> if the value should remain unknown.
        /// </param>
        /// <returns>The rented bit vector.</returns>
        public BitVector RentValue(TypeSignature type, bool initialize)
        {
            type = type.StripModifiers();
            uint size = GetTypeValueMemoryLayout(type).Size;
            if (type.ElementType != ElementType.Boolean)
                return BitVectorPool.Rent((int) size * 8, initialize);

            // For booleans, we only set the LSB to unknown if necessary.
            var result = BitVectorPool.Rent((int) size * 8, true);
            
            if (!initialize)
            {
                var span = result.AsSpan();
                span[0] = Trilean.Unknown;
            }

            return result;
        }
        
        /// <summary>
        /// Obtains the memory layout of a type in the current environment. If the provided type is a reference type,
        /// then it will measure the object reference itself, and not the contents behind the reference.
        /// </summary>
        /// <param name="type">The type to measure.</param>
        /// <returns>The measured layout.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Occurs when the type could not be measured.</exception>
        public TypeMemoryLayout GetTypeValueMemoryLayout(ITypeDescriptor type)
        {
            type = CorLibTypeFactory.FromType(type) ?? type;
            if (!_valueLayouts.TryGetValue(type, out var memoryLayout))
            {
                memoryLayout = type switch
                {
                    ITypeDefOrRef typeDefOrRef => typeDefOrRef.GetImpliedMemoryLayout(RuntimeContext, Is32Bit),
                    TypeSignature signature => signature.GetImpliedMemoryLayout(RuntimeContext, Is32Bit),
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
            type = CorLibTypeFactory.FromType(type) ?? type;
            if (!_contentLayouts.TryGetValue(type, out var memoryLayout))
            {
                memoryLayout = type switch
                {
                    ITypeDefOrRef typeDefOrRef => GetTypeDefOrRefContentsLayout(typeDefOrRef, default, PointerSize),
                    TypeSignature signature => GetTypeSignatureContentsLayout(signature),
                    _ => throw new ArgumentOutOfRangeException(nameof(type))
                };

                _contentLayouts[type] = memoryLayout;
            }

            return memoryLayout;
        }

        /// <summary>
        /// Obtains memory layout information of a field.
        /// </summary>
        /// <param name="field">The field.</param>
        /// <returns>The memory layout information.</returns>
        /// <exception cref="ArgumentException">Occurs when the field could not be resolved.</exception>
        public FieldMemoryLayout GetFieldMemoryLayout(IFieldDescriptor field)
        {
            if (field.DeclaringType is null)
                throw new ArgumentException("Field declaring type is unknown.");

            var layout = GetTypeContentsMemoryLayout(field.DeclaringType);
            var definition = field.Resolve(RuntimeContext);

            return layout[definition];
        }
        
        /// <summary>
        /// Calculates the offset to the element within an array. 
        /// </summary>
        /// <param name="elementType">The element type.</param>
        /// <param name="index">The element's index.</param>
        /// <returns>The offset, relative to the start of an array object.</returns>
        public long GetArrayElementOffset(TypeSignature elementType, long index)
        {
            return ArrayHeaderSize + index * GetTypeValueMemoryLayout(elementType).Size;
        }
        
        private TypeMemoryLayout GetTypeDefOrRefContentsLayout(ITypeDefOrRef type, GenericContext context, uint alignment)
        {
            if (type is TypeSpecification {Signature: { } signature})
                return GetTypeSignatureContentsLayout(signature);

            var definition = type.Resolve(RuntimeContext);
            if (definition.IsValueType)
                return type.GetImpliedMemoryLayout(RuntimeContext, Is32Bit);
            
            // Hack: AsmResolver currently does not support layout detection of reference types, mainly because it
            // is CLR implementation specific. Therefore, we "invent" a new layout ourselves instead for our minimal
            // "implementation" of the CLR. This does not have to be very accurate for most things to work, as programs
            // cannot really reliably assume the exact layout of these classes anyways. 
                
            // This does mean however that we need to manually construct a type layout through some reflection
            // since the setters in TypeMemoryLayout are marked internal. This probably is worthy of a change in 
            // AsmResolver's API in the future.

            var finalType = context.Type != null
                ? definition.MakeGenericInstanceType(RuntimeContext, context.Type.TypeArguments).ToTypeDefOrRef()
                : definition;

            var layout = new TypeMemoryLayout(
                finalType,
                size: 0,
                Is32Bit ? MemoryLayoutAttributes.Is32Bit : MemoryLayoutAttributes.Is64Bit
            );
            
            uint currentOffset = 0;

            // Walk up the type hierarchy to root.
            var hierarchy = new Stack<TypeDefinition>();
            while (definition is not null)
            {
                hierarchy.Push(definition);
                definition = definition.BaseType?.Resolve(RuntimeContext);
            } 

            // Add all fields top-to-bottom to the layout.
            while (hierarchy.Count > 0)
            {
                var currentType = hierarchy.Pop();

                foreach (var field in currentType.Fields)
                {
                    if (field.IsStatic || field.Signature is null)
                        continue;

                    // Infer layout for this field.
                    var contentsLayout = field.Signature.FieldType
                        .InstantiateGenericTypes(context)
                        .GetImpliedMemoryLayout(RuntimeContext, Is32Bit);
                    
                    var fieldLayout = new FieldMemoryLayout(field, currentOffset, contentsLayout);
                    SetField.Invoke(layout, [field, fieldLayout]);
                    
                    currentOffset = (currentOffset + contentsLayout.Size).Align(alignment);
                }
            }

            SetSize.Invoke(layout, new object[] {currentOffset});
            return layout;
        }

        private TypeMemoryLayout GetTypeSignatureContentsLayout(TypeSignature type)
        {
            switch (type.ElementType)
            {
                case ElementType.String:
                    return GetTypeDefOrRefContentsLayout(type.GetUnderlyingTypeDefOrRef()!, GenericContext.FromType(type), sizeof(uint));
                
                case ElementType.Class:
                case ElementType.GenericInst:
                    return GetTypeDefOrRefContentsLayout(type.GetUnderlyingTypeDefOrRef()!, GenericContext.FromType(type), PointerSize);

                default:
                    return type.GetImpliedMemoryLayout(RuntimeContext, Is32Bit);
            }
        }
    }
}