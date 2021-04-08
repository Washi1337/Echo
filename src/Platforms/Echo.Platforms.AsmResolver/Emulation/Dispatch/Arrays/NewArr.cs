using System.Collections.Generic;
using AsmResolver.DotNet;
using AsmResolver.DotNet.Signatures.Types;
using AsmResolver.PE.DotNet.Cil;
using Echo.Concrete.Emulation;
using Echo.Platforms.AsmResolver.Emulation.Values.Cli;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch.Arrays
{
    /// <summary>
    /// Provides a base handler for instructions with the <see cref="CilOpCodes.Newarr"/> operation code.
    /// </summary>
    public class NewArr : FallThroughOpCodeHandler
    {
        /// <inheritdoc />
        public override IReadOnlyCollection<CilCode> SupportedOpCodes => new[]
        {
            CilCode.Newarr
        };

        /// <inheritdoc />
        public override DispatchResult Execute(CilExecutionContext context, CilInstruction instruction)
        {
            var environment = context.GetService<ICilRuntimeEnvironment>();
            var stack = context.ProgramState.Stack;
            
            var sizeValue = (ICliValue) stack.Pop();
            if (!sizeValue.IsKnown)
                return DispatchResult.InvalidProgram();
            
            var elementType = ((ITypeDefOrRef) instruction.Operand).ToTypeSignature();
            int size = sizeValue.InterpretAsI4().I32;
            var array = environment.ValueFactory.AllocateArray(elementType, size);
            
            stack.Push(environment.CliMarshaller.ToCliValue(array, new SzArrayTypeSignature(elementType)));
            
            return base.Execute(context, instruction);
        }
    }
}