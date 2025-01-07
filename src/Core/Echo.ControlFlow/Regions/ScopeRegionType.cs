namespace Echo.ControlFlow.Regions;

/// <summary>
/// Provides members describing all possible types a scope region can be.
/// </summary>
public enum ScopeRegionType
{
    /// <summary>
    /// Indicates no special semantics.
    /// </summary>
    None,

    /// <summary>
    /// Indicates the scope was introduced as a result of a conditional branch (e.g., if-else or switch).
    /// The entry point node of the scope implements the condition and the true/false branches.
    /// </summary>
    Conditional,

    /// <summary>
    /// Indicates the scope was introduced as a result of one or more back-edges and forms a loop.
    /// The entry point node of the scope is the loop header, and all incoming edges originating from within the scope
    /// define all back-edges.
    /// </summary>
    Loop,
}