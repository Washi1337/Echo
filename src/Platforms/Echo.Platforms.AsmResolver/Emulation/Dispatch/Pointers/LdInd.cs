using System.Collections.Generic;
using AsmResolver.PE.DotNet.Cil;
using Echo.Concrete.Emulation;
using Echo.Concrete.Emulation.Dispatch;
using Echo.Concrete.Values;
using Echo.Concrete.Values.ReferenceType;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch.Pointers
{
    /// <summary>
    /// Provides a handler for instructions with one of the LDIND operation codes.
    /// </summary>
    public class LdInd : FallThroughOpCodeHandler
    {
        /// <inheritdoc />
        public override IReadOnlyCollection<CilCode> SupportedOpCodes => new[]
        {
            CilCode.Ldind_I,
            CilCode.Ldind_I1,
            CilCode.Ldind_I2,
            CilCode.Ldind_I4,
            CilCode.Ldind_I8,
            CilCode.Ldind_U1,
            CilCode.Ldind_U2,
            CilCode.Ldind_U4,
            CilCode.Ldind_R4,
            CilCode.Ldind_R8,
        };

        /// <inheritdoc />
        public override DispatchResult Execute(ExecutionContext context, CilInstruction instruction)
        {
            var environment = context.GetService<ICilRuntimeEnvironment>();
            var corLibTypeFactory = environment.Module.CorLibTypeFactory;

            var stack = context.ProgramState.Stack;
            var pointerValue = (IPointerValue) stack.Pop();

            var (value, type) = instruction.OpCode.Code switch
            {
                CilCode.Ldind_I => (pointerValue.Is32Bit
                    ? (IConcreteValue) pointerValue.ReadInteger32(0)
                    : pointerValue.ReadInteger64(0), corLibTypeFactory.IntPtr),
                CilCode.Ldind_I1 => (pointerValue.ReadInteger8(0), corLibTypeFactory.SByte),
                CilCode.Ldind_I2 => (pointerValue.ReadInteger16(0), corLibTypeFactory.Int16),
                CilCode.Ldind_I4 => (pointerValue.ReadInteger32(0), corLibTypeFactory.Int32),
                CilCode.Ldind_I8 => (pointerValue.ReadInteger64(0), corLibTypeFactory.Int64),
                CilCode.Ldind_U1 => (pointerValue.ReadInteger8(0), corLibTypeFactory.Byte),
                CilCode.Ldind_U2 => (pointerValue.ReadInteger16(0), corLibTypeFactory.UInt16),
                CilCode.Ldind_U4 => (pointerValue.ReadInteger32(0), corLibTypeFactory.UInt32),
                CilCode.Ldind_R4 => (pointerValue.ReadFloat32(0), corLibTypeFactory.Single),
                CilCode.Ldind_R8 => (pointerValue.ReadFloat64(0), corLibTypeFactory.Double),
                _ => (null, null)
            };

            if (value is null)
                return DispatchResult.InvalidProgram();
            
            var cliValue = environment.CliMarshaller.ToCliValue(value, type);
            stack.Push(cliValue);
            
            return base.Execute(context, instruction);
        }
        
    }
}