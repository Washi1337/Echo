using System;
using AsmResolver.DotNet;
using AsmResolver.PE.DotNet.Cil;
using Echo.Core.Code;
using Echo.Platforms.AsmResolver.Emulation.Invocation;
using Echo.Platforms.AsmResolver.Emulation.Values;

namespace Echo.Platforms.AsmResolver.Emulation
{
    /// <summary>
    /// Provides members for describing an environment that a .NET virtual machine runs in.
    /// </summary>
    public interface ICilRuntimeEnvironment : IDisposable
    {
        /// <summary>
        /// Gets the architecture description of the instructions to execute. 
        /// </summary>
        IInstructionSetArchitecture<CilInstruction> Architecture
        {
            get;
        }
        
        /// <summary>
        /// Gets a value indicating whether the virtual machine runs in 32-bit mode or in 64-bit mode.
        /// </summary>
        bool Is32Bit
        {
            get;
        }

        /// <summary>
        /// Gets the module that this execution context is part of. 
        /// </summary>
        ModuleDefinition Module
        {
            get;
        }

        /// <summary>
        /// Gets the object responsible for marshalling concrete values to values that can be put
        /// on the evaluation stack and back. 
        /// </summary>
        ICliMarshaller CliMarshaller
        {
            get;
        }

        /// <summary>
        /// Gets the object responsible for allocating memory on the virtual heap of the virtual machine.
        /// </summary>
        IMemoryAllocator MemoryAllocator
        {
            get;
        }

        /// <summary>
        /// Gets the object responsible for making calls to procedures outside of the method body.
        /// </summary>
        IMethodInvoker MethodInvoker
        {
            get;
        }

        /// <summary>
        /// Gets the object responsible for maintaining static fields within the virtual machine.
        /// </summary>
        StaticFieldFactory StaticFieldFactory
        {
            get;
        }

        /// <summary>
        /// Gets the object responsible for constructing new unknown values.
        /// </summary>
        IUnknownValueFactory UnknownValueFactory
        {
            get;
        }
    }
}