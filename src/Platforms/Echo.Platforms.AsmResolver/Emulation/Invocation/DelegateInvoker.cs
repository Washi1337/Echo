using System;
using System.Collections.Generic;
using AsmResolver.DotNet;
using Echo.Memory;
using Echo.Platforms.AsmResolver.Emulation.Dispatch;

namespace Echo.Platforms.AsmResolver.Emulation.Invocation;

/// <summary>
/// Implements a method invoker that handles <see cref="System.Delegate"/> and its derivatives.
/// </summary>
public class DelegateInvoker : IMethodInvoker
{
    /// <summary>
    /// Gets the singleton instance of the <see cref="DelegateInvoker"/> class.
    /// </summary>
    public static DelegateInvoker Instance { get; } = new();

    /// <inheritdoc />
    public InvocationResult Invoke(CilExecutionContext context, IMethodDescriptor method, IList<BitVector> arguments)
    {
        if (method is not { Name.Value: { } name, DeclaringType: { } declaringType, Signature: not null })
            return InvocationResult.Inconclusive();
        
        if (declaringType.Resolve() is { IsDelegate: false })
            return InvocationResult.Inconclusive();

        return name switch
        {
            ".ctor" => ConstructDelegate(context, arguments),
            "Invoke" => InvokeDelegate(context, method, arguments),
            _ => InvocationResult.Inconclusive()
        };
    }
    
    private static InvocationResult ConstructDelegate(CilExecutionContext context, IList<BitVector> arguments)
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
    
    private static InvocationResult InvokeDelegate(CilExecutionContext context, IMethodDescriptor invokeMethod, IList<BitVector> arguments)
    {
        var vm = context.Machine;
        var valueFactory = vm.ValueFactory;

        var self = arguments[0].AsObjectHandle(vm);

        // Try to resolve the managed method behind the delegate.
        IMethodDescriptor? method = null;

        var methodPtrFieldValue = self.ReadField(valueFactory.DelegateMethodPtrField).AsSpan();
        if (methodPtrFieldValue.IsFullyKnown)
        {
            long methodPtr = methodPtrFieldValue.ReadNativeInteger(vm.Is32Bit);
            valueFactory.ClrMockMemory.MethodEntryPoints.TryGetObject(methodPtr, out method);
        }

        method ??= vm.UnknownResolver.ResolveDelegateTarget(context, self, arguments);
        if (method is null)
            return InvocationResult.Inconclusive();

        // Prepare new arguments.
        var newArguments = new BitVector[method.Signature!.GetTotalParameterCount()];

        int argumentIndex = 0;

        // Read and push this for HasThis methods
        if (method.Signature.HasThis)
            newArguments[argumentIndex++] = self.ReadField(valueFactory.DelegateTargetField);

        // Skip 1 for delegate "this"
        for (int i = 1; i < arguments.Count; i++)
            newArguments[argumentIndex++] = arguments[i];

        // Invoke the backing method.
        var result = context.Machine.Invoker.Invoke(context, method, newArguments);

        switch (result.ResultType)
        {
            case InvocationResultType.Exception:
            case InvocationResultType.Inconclusive:
            case InvocationResultType.StepOver:
            case InvocationResultType.FullyHandled:
                return result;

            case InvocationResultType.StepIn:
                // Create and push the trampoline frame
                var trampolineFrame = context.Thread.CallStack.Push(invokeMethod);
                trampolineFrame.IsTrampoline = true;

                // Create and push the method for which the delegate was created
                var frame = context.Thread.CallStack.Push(method);
                for (int i = 0; i < newArguments.Length; i++)
                    frame.WriteArgument(i, newArguments[i]);

                return InvocationResult.FullyHandled();

            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}