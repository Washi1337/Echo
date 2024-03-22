using Echo.Platforms.AsmResolver.Emulation.Dispatch;

namespace Echo.Platforms.AsmResolver.Emulation.Runtime;

/// <summary>
/// Describes a result of a type initialization.
/// </summary>
public readonly struct TypeInitializerResult
{
    private TypeInitializerResult(bool isNoAction, ObjectHandle exceptionObject)
    {
        IsNoAction = isNoAction;
        ExceptionObject = exceptionObject;
    }

    /// <summary>
    /// Gets a value indicating whether the type initialization does not require any further action.
    /// </summary>
    public bool IsNoAction { get; }

    /// <summary>
    /// Gets a value indicating whether control was redirected to the class constructor of a type.
    /// </summary>
    public bool IsRedirectedToConstructor => !IsNoAction;
    
    /// <summary>
    /// Gets the exception that was thrown when initialization the type, if any.
    /// </summary>
    public ObjectHandle ExceptionObject { get; }

    /// <summary>
    /// Creates a result that indicates no further action was taken.
    /// </summary>
    /// <returns>The result.</returns>
    public static TypeInitializerResult NoAction() => new(true, default);

    /// <summary>
    /// Creates a result that indicates control was redirected to a class constructor.
    /// </summary>
    /// <returns>The result.</returns>
    public static TypeInitializerResult Redirected() => new(false, default);

    /// <summary>
    /// Creates a result that throws a type initialization exception. 
    /// </summary>
    /// <param name="exception">The exception that was thrown.</param>
    /// <returns>The result.</returns>
    public static TypeInitializerResult Exception(ObjectHandle exception) => new(false, exception);

    /// <summary>
    /// Transforms the type initialization result into a <see cref="CilDispatchResult"/>.
    /// </summary>
    /// <returns>The new dispatcher result.</returns>
    public CilDispatchResult ToDispatchResult()
    {
        if (!ExceptionObject.IsNull)
            return CilDispatchResult.Exception(ExceptionObject);
        return CilDispatchResult.Success();
    }
}