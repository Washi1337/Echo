using System;
using System.Collections.Generic;
using AsmResolver.DotNet;
using AsmResolver.DotNet.Signatures;
using AsmResolver.PE.DotNet.Cil;
using Echo.Memory;
using Echo.Platforms.AsmResolver.Emulation.Dispatch;

namespace Echo.Platforms.AsmResolver.Emulation.Invocation;
/// <summary>
/// Wrapper for Delegates
/// </summary>
public class DelegateInvoker : IMethodInvoker
{
    /// <summary>
    /// Instance
    /// </summary>
    public static DelegateInvoker Instance { get; } = new();

    /// <inheritdoc />
    public InvocationResult Invoke(CilExecutionContext context, IMethodDescriptor method, IList<BitVector> arguments)
    {
        if (method is not { Name: { } name, DeclaringType: { } declaringType, Signature: { } signature })
            return InvocationResult.Inconclusive();
        
        if (declaringType.Resolve() is { IsDelegate: false })
            return InvocationResult.Inconclusive();
        
        if (method.Name == ".ctor")
        {
            return ConstructDelegate(context, arguments);
        }
        else if (method.Name == "Invoke")
        {
            return InvokeDelegate(context, method, arguments);
        }
        else
        {
            return InvocationResult.Inconclusive();
        }
    }
    
    private InvocationResult ConstructDelegate(CilExecutionContext context, IList<BitVector> arguments)
    {
        var vm = context.Machine;
        var valueFactory = vm.ValueFactory;

        var self = arguments[0].AsObjectHandle(vm);
        var obj = arguments[1];
        var methodPtr = arguments[2];

        self.WriteField(valueFactory.DelegateTargetField, obj);
        self.WriteField(valueFactory.DelegateMethodPtrField, methodPtr);

        return InvocationResult.StepOver(null);
    }
    
    private InvocationResult InvokeDelegate(CilExecutionContext context, IMethodDescriptor invokeMethod, IList<BitVector> arguments)
    {
        var vm = context.Machine;
        var valueFactory = vm.ValueFactory;

        var self = arguments[0].AsObjectHandle(vm);

        var methodPtrFieldValue = self.ReadField(valueFactory.DelegateMethodPtrField);

        if (!methodPtrFieldValue.IsFullyKnown)
            throw new CilEmulatorException($"Field {self.GetObjectType().FullName}::_methodPtr contains an unknown value. Perhaps delegate was initialised with an error, or memory was corrupted.");

        var methodPtr = methodPtrFieldValue.AsSpan().ReadNativeInteger(vm.Is32Bit);

        if (!valueFactory.ClrMockMemory.MethodEntryPoints.TryGetObject(methodPtr, out var method))
            throw new CilEmulatorException($"Cant resolve method from {self.GetObjectType().FullName}::_methodPtr. Possible causes: IMethodDescriptor was not mapped by the emulator, or memory was corrupted.");

        var newArguments = new BitVector[method!.Signature!.GetTotalParameterCount()];

        int argumentIndex = 0;

        // read and push this for HasThis methods
        if (method!.Signature!.HasThis)
            newArguments[argumentIndex++] = self.ReadField(valueFactory.DelegateTargetField);

        // skip 1 for delegate "this"
        for (var i = 1; i < arguments.Count; i++)
            newArguments[argumentIndex++] = arguments[i];

        var result = context.Machine.Invoker.Invoke(context, method, newArguments);

        if (!result.IsSuccess)
        {
            // return error
            return result;
        }
        else if (result.ResultType == InvocationResultType.StepOver)
        {
            // return value
            return result;
        }
        else if (result.ResultType == InvocationResultType.StepIn)
        {
            // create and push the trampoline frame
            var trampolineFrame = context.Thread.CallStack.Push(invokeMethod);
            trampolineFrame.IsTrampoline = true;

            // create and push the method for which the delegate was created
            var frame = context.Thread.CallStack.Push(method);
            for (int i = 0; i < newArguments.Length; i++)
                frame.WriteArgument(i, newArguments[i]);
        }
        
        return InvocationResult.FullyHandled();
    }
}
