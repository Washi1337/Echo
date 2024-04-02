using AsmResolver.DotNet;
using AsmResolver.PE.DotNet.Cil;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch.ObjectModel;

/// <summary>
/// Represents a handler that handles opcodes related to field access.
/// </summary>
public abstract class FieldOpCodeHandler : ICilOpCodeHandler
{
    /// <inheritdoc />
    public CilDispatchResult Dispatch(CilExecutionContext context, CilInstruction instruction)
    {
        var field = (IFieldDescriptor) instruction.Operand!;
        
        // Ensure the enclosing type is initialized in the runtime.
        if (field.DeclaringType is { } declaringType)
        {
            var initResult = context.Machine.ValueFactory.TypeManager.HandleInitialization(context.Thread, declaringType);
            if (!initResult.IsNoAction)
                return initResult.ToDispatchResult();
        }

        // Handle the actual field operation.
        var dispatchResult = DispatchInternal(context, instruction, field);
        
        // We are not inheriting from FallThroughOpCodeHandler because of the type initialization.
        // This means we need to manually increase the PC on success.
        if (dispatchResult.IsSuccess)
            context.CurrentFrame.ProgramCounter += instruction.Size;

        return dispatchResult;
    }

    /// <summary>
    /// Handles the actual operation on the field.
    /// </summary>
    /// <param name="context">The context to evaluate the instruction in.</param>
    /// <param name="instruction">The instruction to dispatch and evaluate.</param>
    /// <param name="field">The field to perform the operation on.</param>
    /// <returns>The dispatching result.</returns>
    protected abstract CilDispatchResult DispatchInternal(
        CilExecutionContext context,
        CilInstruction instruction,
        IFieldDescriptor field
    );
}