namespace Echo.Platforms.AsmResolver.Emulation.Invocation;

/// <summary>
/// Provides members describing the different types of allocation results that can be produced during an object
/// allocation in a CIL virtual machine.
/// </summary>
public enum AllocationResultType
{
    /// <summary>
    /// Indicates the allocation was not handled yet.
    /// </summary>
    Inconclusive,
    
    /// <summary>
    /// Indicates the object was allocated but not initialized yet and a constructor should be called.
    /// </summary>
    Allocated,
    
    /// <summary>
    /// Indicates the object was allocated and also initialized.
    /// </summary>
    FullyConstructed,
    
    /// <summary>
    /// Indicates the allocation failed with an exception.
    /// </summary>
    Exception
}