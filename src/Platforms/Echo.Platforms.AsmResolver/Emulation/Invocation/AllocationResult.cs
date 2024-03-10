using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Echo.Memory;

namespace Echo.Platforms.AsmResolver.Emulation.Invocation;

/// <summary>
/// Describes the result of an allocation of an object. 
/// </summary>
[DebuggerDisplay("{Tag,nq}({DebuggerDisplay})")]
public readonly struct AllocationResult
{
    private AllocationResult(AllocationResultType resultType, BitVector? address, ObjectHandle exceptionObject)
    {
        ResultType = resultType;
        Address = address;
        ExceptionObject = exceptionObject;
    }

    /// <summary>
    /// Gets the type of result this object contains. 
    /// </summary>
    public AllocationResultType ResultType { get; }

    /// <summary>
    /// Gets a value indicating whether the invocation was inconclusive and not handled yet.
    /// </summary>
    public bool IsInconclusive => ResultType is AllocationResultType.Inconclusive;

    /// <summary>
    /// Determines whether the invocation was successful.
    /// </summary>
    [MemberNotNullWhen(true, nameof(Address))]
    public bool IsSuccess => ResultType is AllocationResultType.Allocated or AllocationResultType.FullyConstructed;

    /// <summary>
    /// When <see cref="ResultType"/> is <see cref="InvocationResultType.StepOver"/>, gets the address of the object
    /// that was allocated or constructed.
    /// </summary>
    public BitVector? Address { get; }

    /// <summary>
    /// When <see cref="ResultType"/> is <see cref="InvocationResultType.Exception"/>, gets the handle to the
    /// exception object that was thrown.
    /// </summary>
    public ObjectHandle ExceptionObject { get; }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    internal string Tag => ResultType.ToString();

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    internal object? DebuggerDisplay => IsSuccess ? Address : ExceptionObject;

    /// <summary>
    /// Constructs a new inconclusive allocation result, where the allocation was not handled yet.
    /// </summary>
    public static AllocationResult Inconclusive() => new(AllocationResultType.Inconclusive, null, default);

    /// <summary>
    /// Constructs a new conclusive allocation result, where the object was successfully allocated but not yet
    /// initialized yet.
    /// </summary>
    /// <param name="address">The address of the allocated object.</param>
    public static AllocationResult Allocated(BitVector address) => new(AllocationResultType.Allocated, address, default);
    
    /// <summary>
    /// Constructs a new conclusive allocation result, where the object was successfully allocated and is also fully
    /// initialized by a constructor.
    /// </summary>
    /// <param name="address">The address of the allocated object.</param>
    public static AllocationResult FullyConstructed(BitVector address) => new(AllocationResultType.FullyConstructed, address, default);

    /// <summary>
    /// Constructs a new failed allocation result with the provided pointer to an exception object describing the
    /// error that occurred.
    /// </summary>
    /// <param name="exceptionObject">The handle to the exception object that was thrown.</param>
    public static AllocationResult Exception(ObjectHandle exceptionObject) =>
        new(AllocationResultType.Exception, null, exceptionObject);
}