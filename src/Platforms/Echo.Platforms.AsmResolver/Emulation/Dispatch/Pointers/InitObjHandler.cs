using AsmResolver.DotNet;
using AsmResolver.PE.DotNet.Cil;
using Echo.Core;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch.Pointers
{
    /// <summary>
    /// Implements a CIL instruction handler for <c>initobj</c> operations and its derivatives.
    /// </summary>
    [DispatcherTableEntry(CilCode.Initobj)]
    public class InitObjHandler : FallThroughOpCodeHandler
    {
        /// <inheritdoc />
        protected override CilDispatchResult DispatchInternal(CilExecutionContext context, CilInstruction instruction)
        {
            var factory = context.Machine.ValueFactory;
            
            var type = (ITypeDefOrRef) instruction.Operand!;
            
            var addressSlot = context.CurrentFrame.EvaluationStack.Pop().Contents;
            var initializedObject = factory.RentValue(type.ToTypeSignature(), true);

            try
            {
                var addressSpan = addressSlot.AsSpan();
                switch (addressSpan)
                {
                    case { IsFullyKnown: false }:
                        // TODO: Make configurable.
                        throw new CilEmulatorException("Attempted to initialize an object at an unknown pointer.");
                    
                    case { IsZero.Value: TrileanValue.True }:
                        return CilDispatchResult.NullReference(context);
                    
                    default:
                        long address = addressSpan.ReadNativeInteger(context.Machine.Is32Bit);
                        context.Machine.Memory.Write(address, initializedObject);
                        break;
                }
            }
            finally
            {
                factory.BitVectorPool.Return(addressSlot);
                factory.BitVectorPool.Return(initializedObject);
            }
            
            return CilDispatchResult.Success();
        }
    }
}