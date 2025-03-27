using System.Collections.Generic;
using System.Collections.Immutable;
using Echo.Code;

namespace Echo.ControlFlow.Analysis.Liveness;

/// <summary>
/// Provides information about live variables at a specific execution point.
/// </summary>
/// <param name="In">The incoming live variables.</param>
/// <param name="Out">The outgoing live variables.</param>
public record struct LivenessData(ImmutableHashSet<IVariable> In, ImmutableHashSet<IVariable> Out)
{
    /// <summary>
    /// The empty liveness data with no incoming and outgoing live variables.
    /// </summary>
    public static LivenessData Empty { get; } = new(
        ImmutableHashSet<IVariable>.Empty,
        ImmutableHashSet<IVariable>.Empty
    );

    /// <summary>
    /// Constructs new liveness data that unions the provided variables with the current live input variables.
    /// </summary>
    /// <param name="variables">The newly introduced input live variables.</param>
    /// <returns>The new liveness data.</returns>
    public LivenessData UnionIn(IEnumerable<IVariable> variables) => this with {In = In.Union(variables)};

    /// <summary>
    /// Constructs new liveness data that unions the provided variables with the current live output variables.
    /// </summary>
    /// <param name="variables">The newly introduced output live variables.</param>
    /// <returns>The new liveness data.</returns>
    public LivenessData UnionOut(IEnumerable<IVariable> variables) => this with {Out = Out.Union(variables)};

    /// <inheritdoc />
    public override string ToString()
    {
        string liveIn = string.Join(", ", In);
        string liveOut = string.Join(", ", Out);

        return $"In: [{liveIn}], Out: [{liveOut}]";
    }

    /// <inheritdoc />
    public bool Equals(LivenessData other) => In.SetEquals(other.In) && Out.SetEquals(other.Out);

    /// <inheritdoc />
    public override int GetHashCode() => In.GetHashCode() ^ Out.GetHashCode();
}