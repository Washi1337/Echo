using System;
using System.Collections.Generic;
using AsmResolver.DotNet;
using AsmResolver.PE.DotNet.Cil;
using Echo.Concrete.Emulation;
using Echo.Concrete.Values;
using Echo.Core;
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
        public override DispatchResult Execute(CilExecutionContext context, CilInstruction instruction)
        {
            var environment = context.GetService<ICilRuntimeEnvironment>();
            var field = ((IFieldDescriptor) instruction.Operand).Resolve();
            var stack = context.ProgramState.Stack;

            var objectValue = (ICliValue) stack.Pop();

            IConcreteValue fieldValue;
            if (field.IsStatic)
            {
                // Undocumented: The runtime does allow access of static fields through the ldfld opcode.
                // In this case, the object instance that is pushed is ignored, allowing constructs like:
                //
                //    ldnull
                //    ldfld <some_static_field>
                //
                // without the runtime throwing a NullReferenceException.
                
                var staticField = environment.StaticFieldFactory.Get(field);
                fieldValue = environment.CliMarshaller.ToCliValue(staticField.Value, field.Signature.FieldType);
            }
            else
            {
                // Attempt to dereference the object instance.
                
                switch (objectValue)
                {
                    case { IsKnown: false }:
                        fieldValue = environment.ValueFactory.CreateValue(field.Signature.FieldType, false);
                        break;

                    case OValue { IsZero: { Value: TrileanValue.True } }:
                        return new DispatchResult(new NullReferenceException());

                    case OValue { ReferencedObject: IDotNetStructValue compoundObject }:
                        fieldValue = compoundObject.GetFieldValue(field);
                        break;

                    default:
                        return DispatchResult.InvalidProgram();
                }
            }

            stack.Push(environment.CliMarshaller.ToCliValue(fieldValue, field.Signature.FieldType));
            return base.Execute(context, instruction);
        }
    }
}