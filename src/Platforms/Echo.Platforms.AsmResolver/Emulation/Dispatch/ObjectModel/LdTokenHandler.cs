using System;
using System.Collections.Generic;
using AsmResolver.DotNet;
using AsmResolver.DotNet.Signatures;
using AsmResolver.PE.DotNet.Cil;
using AsmResolver.PE.DotNet.Metadata.Tables;
using Echo.Platforms.AsmResolver.Emulation.Stack;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch.ObjectModel
{
    /// <summary>
    /// Implements a CIL instruction handler for <c>ldtoken</c> operations.
    /// </summary>
    [DispatcherTableEntry(CilCode.Ldtoken)]
    public class LdTokenHandler : FallThroughOpCodeHandler
    {
        /// <inheritdoc />
        protected override CilDispatchResult DispatchInternal(CilExecutionContext context, CilInstruction instruction)
        {
            var clrMemory = context.Machine.ValueFactory.ClrMockMemory;
            var member = (IMetadataMember) instruction.Operand!;

            long address;
            switch (member.MetadataToken.Table)
            {
                case TableIndex.TypeDef:
                case TableIndex.TypeRef:
                case TableIndex.TypeSpec:
                    address = clrMemory.MethodTables.GetAddress((ITypeDescriptor) member);
                    break;
                
                case TableIndex.Method:
                case TableIndex.MethodSpec:
                    address = clrMemory.Methods.GetAddress((IMethodDescriptor) member);
                    break;
                case TableIndex.Field:
                    address = clrMemory.Fields.GetAddress((IFieldDescriptor) member);
                    break;
                
                case TableIndex.MemberRef:
                    address = ((MemberReference) member).Signature!.IsField
                        ? clrMemory.Fields.GetAddress((IFieldDescriptor) member)
                        : clrMemory.Methods.GetAddress((IMethodDescriptor) member);
                    break;    
                
                default:
                    throw new ArgumentOutOfRangeException();
            }

            var vector = context.Machine.ValueFactory.RentNativeInteger(address);
            context.CurrentFrame.EvaluationStack.Push(new StackSlot(vector, StackSlotTypeHint.Integer));

            return CilDispatchResult.Success();
        }
    }
}