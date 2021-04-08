using System;
using System.Collections.Generic;
using AsmResolver.DotNet;
using AsmResolver.PE.DotNet.Cil;
using Echo.Concrete.Emulation;
using Echo.Core;
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
        public override DispatchResult Execute(CilExecutionContext context, CilInstruction instruction)
        {          
            var environment = context.GetService<ICilRuntimeEnvironment>();
            var field = ((IFieldDescriptor) instruction.Operand).Resolve();
            var stack = context.ProgramState.Stack;

            var fieldValue = (ICliValue) stack.Pop();
            var objectValue = (ICliValue) stack.Pop();

            if (field.IsStatic)
            {
                // Undocumented: The runtime does allow access of static fields through the stlfd opcode.
                // In this case, the object instance that is pushed is ignored, allowing constructs like:
                //
                //    ldnull
                //    ldc.i4 1234
                //    ldfld <some_static_field>
                //
                // without the runtime throwing a NullReferenceException.
                
                var staticField = environment.StaticFieldFactory.Get(field);
                staticField.Value = environment.CliMarshaller.ToCtsValue(fieldValue, field.Signature.FieldType);
            }
            else
            {
                // Attempt to dereference the object instance.
                
                switch (objectValue)
                {
                    case { IsKnown: false }:
                        throw new DispatchException("Could not infer object instance of field to set the value for.");

                    case OValue { IsZero: { Value: TrileanValue.True } }:
                        return new DispatchResult(new NullReferenceException());

                    case OValue { ReferencedObject: HleStructValue compoundObject }:
                        compoundObject.SetFieldValue(field,
                            environment.CliMarshaller.ToCtsValue(fieldValue, field.Signature.FieldType));
                        break;

                    default:
                        return DispatchResult.InvalidProgram();
                }
            }

            return base.Execute(context, instruction);
        }
        
    }
}