using System;
using System.Collections.Generic;
using AsmResolver.DotNet;
using AsmResolver.PE.DotNet.Cil;
using Echo.Concrete.Emulation;
using Echo.Concrete.Emulation.Dispatch;
using Echo.Concrete.Values;
using Echo.Platforms.AsmResolver.Emulation.Values;
using Echo.Platforms.AsmResolver.Emulation.Values.Cli;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch.ObjectModel
{
    /// <summary>
    /// Provides a handler for instructions with the <see cref="CilOpCodes.Ldfld"/> operation code.
    /// </summary>
    public class LdFld : FallThroughOpCodeHandler
    {
        /// <inheritdoc />
        public override IReadOnlyCollection<CilCode> SupportedOpCodes => new[]
        {
            CilCode.Ldfld
        };

        /// <inheritdoc />
        public override DispatchResult Execute(ExecutionContext context, CilInstruction instruction)
        {
            var environment = context.GetService<ICilRuntimeEnvironment>();
            var field = ((IFieldDescriptor) instruction.Operand).Resolve();
            var stack = context.ProgramState.Stack;

            var objectValue = (ICliValue) stack.Pop();

            IConcreteValue fieldValue;
            switch (objectValue)
            {
                case { IsKnown: false }:
                    fieldValue = new UnknownValue();
                    break;
                
                case OValue { IsZero: true }:
                    return new DispatchResult(new NullReferenceException());
                
                case OValue { ReferencedObject: HleObjectValue compoundObject }:
                    fieldValue = compoundObject.GetFieldValue(field);
                    break;
                
                default:
                    return DispatchResult.InvalidProgram();
            }

            stack.Push(environment.CliMarshaller.ToCliValue(fieldValue, field.Signature.FieldType));
            return base.Execute(context, instruction);
        }
    }
}