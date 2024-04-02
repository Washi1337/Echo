using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using AsmResolver;
using AsmResolver.DotNet;
using AsmResolver.DotNet.Memory;
using AsmResolver.DotNet.Signatures;
using AsmResolver.DotNet.Signatures.Types;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;

namespace Echo.Platforms.AsmResolver.Emulation.Runtime;

/// <summary>
/// Represents an instantiation of a type in a .NET virtual machine for which the type layout is determined and
/// is accompanied with a concretized virtual function table. 
/// </summary>
public sealed class MethodTable
{
    private static readonly MethodInfo SetField;
    private static readonly MethodInfo SetSize;

    private readonly RuntimeTypeManager _manager;
    private readonly uint _alignment;
    private TypeMemoryLayout? _contentsLayout;
    private TypeMemoryLayout? _valueLayout;

    static MethodTable()
    {
        SetField = typeof(TypeMemoryLayout).GetMethod("set_Item", (BindingFlags) (-1))!;
        SetSize = typeof(TypeMemoryLayout).GetMethod("set_Size", (BindingFlags) (-1))!;
    }

    internal MethodTable(
        RuntimeTypeManager manager,
        MethodTable? baseMethodTable,
        ITypeDescriptor type,
        TypeDefinition? definition,
        uint alignment)
    {
        _manager = manager;
        _alignment = alignment;
        BaseMethodTable = baseMethodTable;
        Type = type;
        Definition = definition;
    }

    /// <summary>
    /// Gets the method table of the type's base type (if available).
    /// </summary>
    public MethodTable? BaseMethodTable { get; }

    /// <summary>
    /// Gets the type that this method table represents.
    /// </summary>
    public ITypeDescriptor Type { get; }

    /// <summary>
    /// Gets the type that this method table instantiated.
    /// </summary>
    public TypeDefinition? Definition { get; }

    /// <summary>
    /// Gets the virtual function table associated to the method table.
    /// </summary>
    public List<IMethodDescriptor> VTable { get; } = new();

    /// <summary>
    /// Gets a value indicating whether the class constructor of the type was called.
    /// </summary>
    public bool ClassConstructorCalled { get; set; }

    /// <summary>
    /// Gets the exception that was thrown during the initialization of the type, or <c>null</c> if no exception was
    /// thrown.
    /// </summary>
    public ObjectHandle LoaderException { get; set; }

    /// <summary>
    /// Gets the memory layout of the contents of the type, excluding the object header.
    /// </summary>
    public TypeMemoryLayout ContentsLayout
    {
        get
        {
            if (_contentsLayout is null)
                Interlocked.CompareExchange(ref _contentsLayout, DetermineContentsLayout(), null);
            return _contentsLayout;
        }
    }

    /// <summary>
    /// Gets the layout of a single value of the type.
    /// </summary>
    public TypeMemoryLayout ValueLayout
    {
        get
        {
            if (_valueLayout is null)
                Interlocked.CompareExchange(ref _valueLayout, DetermineValueLayout(), null);
            return _valueLayout;
        }
    }

    /// <summary>
    /// Gets the data size of the object's data. This includes both the object header and the contents of the object.
    /// </summary>
    public uint DataSize => _manager.ObjectHeaderSize + ContentsLayout.Size;
    
    private TypeMemoryLayout DetermineValueLayout()
    {
        var type = _manager.ContextModule.CorLibTypeFactory.FromType(Type) ?? Type;
        return type switch
        {
            ITypeDefOrRef typeDefOrRef => typeDefOrRef.GetImpliedMemoryLayout(_manager.Is32Bit),
            TypeSignature signature => signature.GetImpliedMemoryLayout(_manager.Is32Bit),
            _ => throw new ArgumentOutOfRangeException(nameof(type))
        };
    }

    private TypeMemoryLayout DetermineContentsLayout()
    {
        var type = _manager.ContextModule.CorLibTypeFactory.FromType(Type) ?? Type;
        return type switch
        {
            ITypeDefOrRef typeDefOrRef => GetTypeDefOrRefContentsLayout(typeDefOrRef, default, _alignment),
            TypeSignature signature => GetTypeSignatureContentsLayout(signature),
            _ => throw new ArgumentOutOfRangeException(nameof(type))
        };
    }

    private TypeMemoryLayout GetTypeDefOrRefContentsLayout(ITypeDefOrRef type, GenericContext context, uint alignment)
    {
        if (type is TypeSpecification { Signature: { } signature })
            return GetTypeSignatureContentsLayout(signature);

        if (type.Resolve() is not { IsValueType: false } definition)
            return type.GetImpliedMemoryLayout(_manager.Is32Bit);

        // Hack: AsmResolver currently does not support layout detection of reference types, mainly because it
        // is CLR implementation specific. Therefore, we "invent" a new layout ourselves instead for our minimal
        // "implementation" of the CLR. This does not have to be very accurate for most things to work, as programs
        // cannot really reliably assume the exact layout of these classes anyways. 

        // This does mean however that we need to manually construct a type layout through some reflection
        // since the setters in TypeMemoryLayout are marked internal. This probably is worthy of a change in 
        // AsmResolver's API in the future.

        var layout = new TypeMemoryLayout(definition, 0,
            _manager.Is32Bit ? MemoryLayoutAttributes.Is32Bit : MemoryLayoutAttributes.Is64Bit);

        uint currentOffset = 0;

        // Walk up the type hierarchy to root.
        var hierarchy = new Stack<TypeDefinition>();
        while (definition is not null)
        {
            hierarchy.Push(definition);
            definition = definition.BaseType?.Resolve();
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
                    .GetImpliedMemoryLayout(_manager.Is32Bit);

                var fieldLayout = new FieldMemoryLayout(field, currentOffset, contentsLayout);
                SetField.Invoke(layout, new object[] { field, fieldLayout });

                currentOffset = (currentOffset + contentsLayout.Size).Align(alignment);
            }
        }

        SetSize.Invoke(layout, new object[] { currentOffset });
        return layout;
    }

    private TypeMemoryLayout GetTypeSignatureContentsLayout(TypeSignature type)
    {
        switch (type.ElementType)
        {
            case ElementType.String:
                return GetTypeDefOrRefContentsLayout(type.GetUnderlyingTypeDefOrRef()!, GenericContext.FromType(type),
                    sizeof(uint));

            case ElementType.Class:
            case ElementType.GenericInst:
                return GetTypeDefOrRefContentsLayout(
                    type.GetUnderlyingTypeDefOrRef()!, 
                    GenericContext.FromType(type),
                    _manager.PointerSize
                );

            default:
                return type.GetImpliedMemoryLayout(_manager.Is32Bit);
        }
    }

    /// <inheritdoc />
    public override string ToString() => Type.FullName;
}