using System;

namespace Echo.Platforms.AsmResolver.Emulation;

/// <summary>
/// Defines flags that control or override the behavior of a virtual machine.
/// </summary>
[Flags]
public enum CilEmulationFlags
{
    /// <summary>
    /// Indicates no special flags.
    /// </summary>
    None = 0,

    /// <summary>
    /// Indicates type initializers (.cctor) will not be called when accessing a type or a member of a type for the
    /// first time.
    /// </summary>
    SkipTypeInitializations = 1 << 0,
}