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
        
        if (declaringType.Resolve()?.IsDelegate == false)
            return InvocationResult.Inconclusive();
        
        if (method.Name == ".ctor")
        {
            return ConstructDelegate(context, arguments);
        }
        
        if (method.Name == "Invoke")
        {
            return InvokeDelegate(context, arguments);
        }

        return InvocationResult.Inconclusive();
    }
    
    private InvocationResult ConstructDelegate(CilExecutionContext context, IList<BitVector> arguments)
    {
        var vm = context.Machine;
        var valueFactory = vm.ValueFactory;

        var self = arguments[0].AsObjectHandle(vm);
        var obj = arguments[1];
        var methodPtr = arguments[2];

        var _target = valueFactory.DelegateTargetField;
        var _methodPtr = valueFactory.DelegateMethodPtrField;

        self.WriteField(_target, obj);
        self.WriteField(_methodPtr, methodPtr);

        return InvocationResult.StepOver(null);
    }
    
    private InvocationResult InvokeDelegate(CilExecutionContext context, IList<BitVector> arguments)
    {
        var vm = context.Machine;
        var valueFactory = vm.ValueFactory;
        var stack = context.CurrentFrame.EvaluationStack;

        var self = arguments[0].AsObjectHandle(vm);

        var _target = valueFactory.DelegateTargetField;
        var _methodPtr = valueFactory.DelegateMethodPtrField;

        var methodPtr = self.ReadField(_methodPtr).AsSpan().ReadNativeInteger(vm.Is32Bit);

        if (!valueFactory.ClrMockMemory.MethodEntryPoints.TryGetObject(methodPtr, out var method))
            throw new CilEmulatorException($"Cant resolve method from {self.GetObjectType().FullName}::_methodPtr. Possible causes: IMethodDescriptor was not mapped by the emulator, or memory was corrupted.");

        // read and push this for HasThis methods
        if (method!.Signature!.HasThis)
            stack.Push(self.ReadField(_target).AsObjectHandle(vm));

        // skip 1 for delegate "this"
        for (var i = 1; i < arguments.Count; i++)
            stack.Push(arguments[i], method!.Signature!.ParameterTypes[i]);

        var result = vm.Dispatcher.Dispatch(context, new(CilOpCodes.Call, method));

        if (!result.IsSuccess)
            return InvocationResult.Exception(result.ExceptionObject);

        return InvocationResult.StepOver(null);
    }
}
