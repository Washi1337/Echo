using System;
using System.Collections.Generic;
using AsmResolver.PE.DotNet.Cil;
using Echo.Concrete.Emulation;
using Echo.Concrete.Emulation.Dispatch;
using Echo.Core;
using Echo.Platforms.AsmResolver.Emulation.Values;
using Echo.Platforms.AsmResolver.Emulation.Values.Cli;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch.Arrays
{
    /// <summary>
    /// Provides a handler for instructions with the <see cref="CilOpCodes.Ldlen"/> operation code.
    /// </summary>
    public class LdLen : FallThroughOpCodeHandler
    {
        /// <inheritdoc />
        public override IReadOnlyCollection<CilCode> SupportedOpCodes => new[]
        {
            CilCode.Ldlen
        };

        /// <inheritdoc />
        public override DispatchResult Execute(CilExecutionContext context, CilInstruction instruction)
        {
            var environment = context.GetService<ICilRuntimeEnvironment>();
            var stack = context.ProgramState.Stack;
            
            NativeIntegerValue lengthValue;
            
            var argument = stack.Pop();
            switch (argument)
            {
                case OValue { IsZero: { Value: TrileanValue.True } }:
                    // Pushed object is null.
                    return new DispatchResult(new NullReferenceException());
                
                case OValue { ReferencedObject: IDotNetArrayValue arrayValue }:
                    // Get length of the array and wrap in native int.
                    lengthValue = new NativeIntegerValue(arrayValue.Length, environment.Is32Bit);
                    break;
                
                case OValue _:
                    // Undefined behaviour when this operation is applied on any other kind of object.
                    lengthValue = new NativeIntegerValue(0, 0, environment.Is32Bit);
                    break;

                default:
                    return DispatchResult.InvalidProgram();
            }
            
            stack.Push(lengthValue);
            return base.Execute(context, instruction);
        }
        
    }
}