namespace Echo.Concrete.Emulation
{
    /// <summary>
    /// Provides members describing all possible states a virtual machine can be in.
    /// </summary>
    public enum VirtualMachineStatus
    {
        /// <summary>
        /// Indicates the virtual machine is idle and is not executing any instructions.
        /// </summary>
        Idle,
        
        /// <summary>
        /// Indicates the virtual machine is running.
        /// </summary>
        Running,
    }
}