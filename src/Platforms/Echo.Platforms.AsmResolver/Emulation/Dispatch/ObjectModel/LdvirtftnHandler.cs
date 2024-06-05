using System.Linq;
using AsmResolver.DotNet;
using AsmResolver.DotNet.Signatures;
using AsmResolver.PE.DotNet.Cil;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch.ObjectModel;

/// <summary>
/// Implements a CIL instruction handler for <c>ldvirtftn</c> instruction.
/// </summary>
[DispatcherTableEntry(CilCode.Ldvirtftn)]
public class LdvirtftnHandler : FallThroughOpCodeHandler
{
    /// <inheritdoc/>
    protected override CilDispatchResult DispatchInternal(CilExecutionContext context, CilInstruction instruction)
    {
        var stack = context.CurrentFrame.EvaluationStack;
        var factory = context.Machine.ValueFactory;
        var methods = context.Machine.ValueFactory.ClrMockMemory.MethodEntryPoints;
        var type = context.Machine.ContextModule.CorLibTypeFactory.IntPtr;

        var thisObject = stack.Pop().Contents;
        if (!thisObject.IsFullyKnown)
            throw new CilEmulatorException("Unable to resolve an unknown object.");

        var thisObjectType = thisObject.AsObjectHandle(context.Machine).GetObjectType().Resolve();
        if (thisObjectType == null)
            throw new CilEmulatorException("Unable to resolve the type of object");

        var virtualFunction = ((IMethodDescriptor)instruction.Operand!);
        var virtualFunctionName = virtualFunction.Name;
        var virtualFunctionSignature = virtualFunction.Signature;

        do
        {   
            // try resolve function
            var resolvedVirtualFunction = thisObjectType.Methods
                .FirstOrDefault(method => method.Name == virtualFunctionName 
                && SignatureComparer.Default.Equals(method.Signature, virtualFunctionSignature));

            // if resolved then push function pointer
            if (resolvedVirtualFunction != null)
            {
                var functionPointer = methods.GetAddress(resolvedVirtualFunction);
                stack.Push(factory.CreateNativeInteger(functionPointer), type);
                return CilDispatchResult.Success();
            }

            // else switch to BaseType and try resolve again
            thisObjectType = thisObjectType.BaseType?.Resolve();
        }   // or exit and throw CilEmulationException
        while (thisObjectType != null);

        throw new CilEmulatorException($"Unable to resolve a virtual function for type {thisObjectType!.FullName}");
    }
}
