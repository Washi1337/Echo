using AsmResolver.DotNet;

namespace Echo.Platforms.AsmResolver.Emulation;
/// <summary>
/// Provides methods for marshalling functions into pointers
/// </summary>
public interface IFunctionMarshaller
{
    /// <summary>
    /// Gets the machine the marshaller is targeting.
    /// </summary>
    CilVirtualMachine Machine
    {
        get;
    }

    /// <summary>
    /// Returns pointer for method descriptor
    /// </summary>
    nuint GetFunctionPointer(IMethodDescriptor method);

    /// <summary>
    /// Returns method descriptor for pointer
    /// </summary>
    IMethodDescriptor? ResolveMethodPointer(nuint pointer);
}
