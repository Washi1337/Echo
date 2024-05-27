using System;
using System.Collections.Generic;
using System.Linq;
using AsmResolver.DotNet;

namespace Echo.Platforms.AsmResolver.Emulation;

/// <inheritdoc/>
public class FunctionMarshaller : IFunctionMarshaller
{
    private readonly Dictionary<nuint, IMethodDescriptor> mappedPointers = new();
    private nuint pointer = 13;

    /// <summary>
    /// Creates a new marshaller for the provided virtual machine.
    /// </summary>
    /// <param name="machine"></param>
    public FunctionMarshaller(CilVirtualMachine machine)
    {
        Machine = machine;
    }

    /// <inheritdoc/>
    public CilVirtualMachine Machine
    {
        get;
    }

    /// <inheritdoc/>
    public nuint GetFunctionPointer(IMethodDescriptor method)
    {
        foreach (var pair in mappedPointers)
        {
            if (pair.Value == method)
                return pair.Key;
        }
        mappedPointers.Add(pointer, method);
        return pointer++;
    }

    /// <inheritdoc/>
    public IMethodDescriptor? ResolveMethodPointer(nuint pointer)
    {
        if (mappedPointers.TryGetValue(pointer, out var result))
            return result;
        return null;
    }
}
