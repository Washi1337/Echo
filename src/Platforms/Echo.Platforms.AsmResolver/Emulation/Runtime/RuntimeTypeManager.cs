using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using AsmResolver.DotNet;
using AsmResolver.DotNet.Memory;
using AsmResolver.DotNet.Signatures;
using AsmResolver.DotNet.Signatures.Types;

namespace Echo.Platforms.AsmResolver.Emulation.Runtime;

/// <summary>
/// Provides a mechanism for initialization and management of types residing in a .NET virtual machine.
/// </summary>
public sealed class RuntimeTypeManager
{
    private readonly ConcurrentDictionary<IMethodDescriptor, int> _vtableIndices = new(EqualityComparer<IMethodDescriptor>.Default);
    private readonly ConcurrentDictionary<ITypeDescriptor, MethodTable> _methodTables = new(SignatureComparer.Default);

    /// <summary>
    /// Creates a new type manager for a .NET process that has the provided main module.
    /// </summary>
    /// <param name="contextModule">The main module </param>
    /// <param name="is32Bit"><c>true</c> if the type manager is 32-bits, <c>false</c> if 64-bits.</param>
    public RuntimeTypeManager(ModuleDefinition contextModule, bool is32Bit)
    {
        ContextModule = contextModule;
        Is32Bit = is32Bit;
        PointerSize = is32Bit ? 4u : 8u;

        // Force 4-byte alignment for System.String.
        var stringType = contextModule.CorLibTypeFactory.String.Type;
        _methodTables[stringType] = CreateMethodTable(stringType, sizeof(uint));
    }

    /// <summary>
    /// Gets the manifest module to use for context.
    /// </summary>
    public ModuleDefinition ContextModule { get; }

    /// <summary>
    /// Gets a value indicating whether the environment is a 32-bit or 64-bit system.
    /// </summary>
    public bool Is32Bit { get; }

    /// <summary>
    /// Gets the size in bytes of a pointer in the current environment.
    /// </summary>
    public uint PointerSize { get; }
    
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
    /// Obtains the method table of the provided type.
    /// </summary>
    /// <param name="type">The type.</param>
    /// <returns>The associated method table.</returns>
    public MethodTable GetMethodTable(ITypeDescriptor type)
    {
        if (_methodTables.TryGetValue(type, out var methodTable))
            return methodTable;

        var newMethodTable = CreateMethodTable(type, PointerSize);
        while (!_methodTables.TryGetValue(type, out methodTable))
        {
            if (_methodTables.TryAdd(type, newMethodTable))
            {
                methodTable = newMethodTable;
                break;
            }
        }

        return methodTable;
    }

    private MethodTable CreateMethodTable(ITypeDescriptor type, uint alignment)
    {
        var definition = type.Resolve();

        // If we cannot resolve, assume it is a standalone type with no base-type.
        if (definition is null)
            return new MethodTable(this, null, type, null, alignment);

        var context = GenericContext.FromType(type);

        // Determine base method table.
        var baseMethodTable = definition.BaseType is { } baseType
            ? GetMethodTable(baseType.ToTypeSignature().InstantiateGenericTypes(context))
            : null;

        var result = new MethodTable(this, baseMethodTable, type, definition, alignment);

        // Copy VTable of base type into our own type. This is safe even for generic types, since the base method table
        // was constructed with the instantiated type signature.
        if (baseMethodTable is not null)
            result.VTable.AddRange(baseMethodTable.VTable);

        OverrideVTableSlots(result, FindOverridingMethods(result, definition));

        return result;
    }

    private IList<MethodDefinition> FindOverridingMethods(MethodTable result, TypeDefinition definition)
    {
        var overridingMethods = new List<MethodDefinition>();

        for (int i = 0; i < definition.Methods.Count; i++)
        {
            var method = definition.Methods[i];
            if (!method.IsVirtual)
                continue;

            if (method.IsNewSlot)
            {
                // This is a new VTable slot.
                _vtableIndices[method] = result.VTable.Count;
                result.VTable.Add(method);
            }
            else if (method.IsReuseSlot)
            {
                // Method is reusing an already existing VTable slot.
                overridingMethods.Add(method);
            }
        }

        return overridingMethods;
    }

    private void OverrideVTableSlots(MethodTable result, IList<MethodDefinition> pendingOverridingMethods)
    {
        var currentTable = result.BaseMethodTable;
        while (pendingOverridingMethods.Count > 0)
        {
            // Have we reached the root of the type hierarchy?
            if (currentTable is null)
                break;

            // Did we successfully resolve the current type?
            var definition = currentTable.Definition;
            if (definition is null)
                break;

            // Determine the generic context of the type.
            var genericContext = GenericContext.FromType(currentTable.Type);
            
            // Go over all virtual methods in the method table and see if they are overridden by the new type.
            foreach (var candidateMethod in currentTable.VTable)
            {
                if (candidateMethod.Signature is null)
                    continue;

                // Determine the signature that we would need to match.
                var expectedSignature = candidateMethod.Signature.InstantiateGenericTypes(genericContext);

                for (int i = 0; i < pendingOverridingMethods.Count; i++)
                {
                    var overridingMethod = pendingOverridingMethods[i];

                    // Names must match.
                    if (overridingMethod.Name != candidateMethod.Name)
                        continue;

                    // Signature must match the expected signature.
                    if (!SignatureComparer.Default.Equals(expectedSignature, overridingMethod.Signature))
                        continue;

                    // Copy VTable index of base method.
                    int vtableIndex = _vtableIndices[candidateMethod];
                    _vtableIndices[overridingMethod] = vtableIndex;
                    result.VTable[vtableIndex] = overridingMethod;
                    pendingOverridingMethods.RemoveAt(i);

                    break;
                }
            }

            // Go up the type hierarchy.
            currentTable = currentTable.BaseMethodTable;
        }
    }
    
    /// <summary>
    /// Computes the total size of an array object.
    /// </summary>
    /// <param name="elementType">The type of elements the array stores.</param>
    /// <param name="elementCount">The number of elements.</param>
    /// <returns>The total size in bytes.</returns>
    public uint GetArrayObjectSize(ITypeDescriptor elementType, int elementCount)
    {
        uint elementSize = elementType.IsValueType
            ? GetMethodTable(elementType).ValueLayout.Size
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
    /// Calculates the offset to the element within an array. 
    /// </summary>
    /// <param name="elementType">The element type.</param>
    /// <param name="index">The element's index.</param>
    /// <returns>The offset, relative to the start of an array object.</returns>
    public long GetArrayElementOffset(TypeSignature elementType, long index)
    {
        return ArrayHeaderSize + index * GetMethodTable(elementType).ValueLayout.Size;
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

        var methodTable = GetMethodTable(field.DeclaringType);

        if (field.Resolve() is not { } resolvedField)
            throw new ArgumentException($"Could not resolve field '{field}'");
            
        return methodTable.ContentsLayout[resolvedField];
    }
    
    /// <summary>
    /// Registers the event that a type has failed to initialize. 
    /// </summary>
    /// <param name="machine">The machine the exception is instantiated in.</param>
    /// <param name="type">The type that failed to initialize.</param>
    /// <param name="innerException">The exception object that describes the failure.</param>
    /// <returns>The resulting TypeInitializationException instance.</returns>
    public ObjectHandle RegisterInitializationException(
        CilVirtualMachine machine, 
        ITypeDescriptor type, 
        ObjectHandle innerException)
    {
        var methodTable = GetMethodTable(type);
        if (!methodTable.LoaderException.IsNull)
            return methodTable.LoaderException;

        lock (methodTable)
        {
            if (methodTable.LoaderException.IsNull)
            {
                methodTable.LoaderException = machine.Heap
                    .AllocateObject(machine.ValueFactory.TypeInitializationExceptionType, true)
                    .AsObjectHandle(machine);
            }
            
            // TODO: incorporate `innerException`.
        }

        return methodTable.LoaderException;
    }

    /// <summary>
    /// Handles the type initialization on the provided thread.
    /// </summary>
    /// <param name="thread">The thread the initialization is to be called on.</param>
    /// <param name="type">The type to initialize.</param>
    /// <returns>The initialization result.</returns>
    public TypeInitializerResult HandleInitialization(CilThread thread, ITypeDescriptor type)
    {
        var methodTable = GetMethodTable(type);

        // If we already have an exception cached as a result of a previous type-load failure, rethrow it.
        if (!methodTable.LoaderException.IsNull)
            return TypeInitializerResult.Exception(methodTable.LoaderException);
        
        // We only need to call the constructor once.
        if (methodTable.ClassConstructorCalled)
            return TypeInitializerResult.NoAction();
        
        lock (methodTable)
        {
            // Try check if any thread beat us in the initialization handling.
            if (!methodTable.LoaderException.IsNull)
                return TypeInitializerResult.Exception(methodTable.LoaderException);

            if (methodTable.ClassConstructorCalled)
                return TypeInitializerResult.NoAction();
            
            // Try resolve the type that is being initialized.
            var definition = type.Resolve();
            if (definition is null)
            {
                methodTable.LoaderException = thread.Machine.Heap
                    .AllocateObject(thread.Machine.ValueFactory.TypeInitializationExceptionType, true)
                    .AsObjectHandle(thread.Machine);
                    
                return TypeInitializerResult.Exception(methodTable.LoaderException);
            }
            
            // "Call" the constructor.
            methodTable.ClassConstructorCalled = true;

            // Actually find the constructor and call it if it is there.
            var cctor = definition.GetStaticConstructor();
            if (cctor is not null)
            {
                thread.CallStack.Push(cctor);
                return TypeInitializerResult.Redirected();
            }

            return TypeInitializerResult.NoAction();
        }
    }
}