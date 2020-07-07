using System;
using System.Collections.Generic;
using AsmResolver.DotNet;
using AsmResolver.PE.DotNet.Cil;
using Echo.Concrete.Emulation;
using Echo.Concrete.Emulation.Dispatch;
using Echo.Platforms.AsmResolver.Emulation.Values;
using Echo.Platforms.AsmResolver.Emulation.Values.Cli;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch.ObjectModel
{
    /// <summary>
    /// Provides a handler for instructions with the <see cref="CilOpCodes.Stfld"/> operation code.
    /// </summary>
    public class StFld : FallThroughOpCodeHandler
    {
        /// <inheritdoc />
        public override IReadOnlyCollection<CilCode> SupportedOpCodes => new[]
        {
            CilCode.Stfld
        };

        /// <inheritdoc />
        public override DispatchResult Execute(ExecutionContext context, CilInstruction instruction)
        {          
            var environment = context.GetService<ICilRuntimeEnvironment>();
            var field = ((IFieldDescriptor) instruction.Operand).Resolve();
            var stack = context.ProgramState.Stack;

            var fieldValue = (ICliValue) stack.Pop();
            var objectValue = (ICliValue) stack.Pop();

            switch (objectValue)
            {
                case { IsKnown: false }:
                    throw new DispatchException("Could not infer object instance of field to set the value for.");
                
                case OValue { IsZero: true }:
                    return new DispatchResult(new NullReferenceException());
                
                case OValue { ReferencedObject: HleObjectValue compoundObject }:
                    compoundObject.SetFieldValue(field,
                        environment.CliMarshaller.ToCtsValue(fieldValue, field.Signature.FieldType));
                    break;
                
                default:
                    return DispatchResult.InvalidProgram();
            }

            return base.Execute(context, instruction);
        }
        
    }
}