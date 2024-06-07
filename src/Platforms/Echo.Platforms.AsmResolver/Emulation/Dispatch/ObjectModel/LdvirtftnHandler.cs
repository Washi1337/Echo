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

        var virtualFunction = (IMethodDescriptor)instruction.Operand!;

        IMethodDescriptor? implementation = CallHandlerBase.FindMethodImplementationInType(thisObjectType, virtualFunction.Resolve());

        if (implementation == null)
        {
            return CilDispatchResult.Exception(
                context.Machine.Heap.AllocateObject(
                        context.Machine.ValueFactory.MissingMethodExceptionType,
                        true)
                    .AsObjectHandle(context.Machine)
            );
        }

        // Instantiate any generics.
        var genericContext = GenericContext.FromMethod(virtualFunction);
        if (!genericContext.IsEmpty)
        {
            var instantiated = thisObjectType
            .CreateMemberReference(implementation.Name!, implementation.Signature!);

            implementation = genericContext.Method != null
                ? instantiated.MakeGenericInstanceMethod(genericContext.Method.TypeArguments.ToArray())
                : instantiated;
        }

        var functionPointer = methods.GetAddress(implementation);
        stack.Push(factory.CreateNativeInteger(functionPointer), type);
        return CilDispatchResult.Success();
    }
}
