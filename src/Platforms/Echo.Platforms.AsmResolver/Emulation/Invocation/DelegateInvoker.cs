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
        
        if (declaringType.Resolve()?.BaseType?.IsTypeOf("System", "MulticastDelegate") == false)
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
        var corlib = vm.ContextModule.CorLibTypeFactory;

        var self = arguments[0].AsObjectHandle(vm);
        var obj = arguments[1];
        var methodPtr = arguments[2];

        var _delegate = self.GetObjectType().Resolve()!.BaseType!.Resolve()!.BaseType;
        IFieldDescriptor _target = new MemberReference(_delegate, "_target", new FieldSignature(corlib.Object));
        IFieldDescriptor _methodPtr = new MemberReference(_delegate, "_methodPtr", new FieldSignature(corlib.IntPtr));

        if (obj.IsFullyKnown && obj.AsObjectHandle(vm).IsNull)
            self.WriteField(_target, obj);

        self.WriteField(_methodPtr, methodPtr);

        //var methodBase = vm.FunctionMarshaller.ResolveMethodPointer((nuint)methodPtr.AsSpan().ReadNativeInteger(vm.Is32Bit));
        //if (methodBase != null)
        //    self.WriteField(_methodBase, marshall IMethodDescriptor to RuntimeMethodInfo);

        return InvocationResult.StepOver(null);
    }
    
    private InvocationResult InvokeDelegate(CilExecutionContext context, IList<BitVector> arguments)
    {
        var vm = context.Machine;
        var stack = context.CurrentFrame.EvaluationStack;
        var corlib = vm.ContextModule.CorLibTypeFactory;

        var self = arguments[0].AsObjectHandle(vm);

        var _delegate = self.GetObjectType().Resolve()!.BaseType!.Resolve()!.BaseType;
        IFieldDescriptor _target = new MemberReference(_delegate, "_target", new FieldSignature(corlib.Object));
        IFieldDescriptor _methodPtr = new MemberReference(_delegate, "_methodPtr", new FieldSignature(corlib.IntPtr));

        var methodPtr = self.ReadField(_methodPtr).AsSpan().ReadNativeInteger(vm.Is32Bit);
        var method = vm.FunctionMarshaller.ResolveMethodPointer((nuint)methodPtr);

        if (method == null)
            throw new Exception("Cant resolve method");

        var arg1 = self.ReadField(_target);
        if (arg1.AsSpan().ReadNativeInteger(vm.Is32Bit) != 0)
            stack.Push(arg1.AsObjectHandle(vm));

        // skip 1 for delegate "this"
        for (var i = 1; i < arguments.Count; i++)
            stack.Push(arguments[i], method.Signature!.ParameterTypes[i]);

        var result = vm.Dispatcher.Dispatch(context, new(CilOpCodes.Call, method));

        if (!result.IsSuccess)
            return InvocationResult.Exception(result.ExceptionObject);

        return InvocationResult.StepOver(null);
    }
}
