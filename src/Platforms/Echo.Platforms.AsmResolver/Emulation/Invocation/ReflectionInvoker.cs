using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AsmResolver.DotNet;
using Echo.Memory;
using Echo.Platforms.AsmResolver.Emulation.Dispatch;

namespace Echo.Platforms.AsmResolver.Emulation.Invocation
{
    /// <summary>
    /// Provides an implementation of a method invoker that steps over any method by invoking it via System.Reflection. 
    /// </summary>
    public class ReflectionInvoker : IMethodInvoker
    {
        /// <summary>
        /// Gets the default instance of the <see cref="ReflectionInvoker"/> class.
        /// </summary>
        public static ReflectionInvoker Instance
        {
            get;
        } = new();
        
        /// <inheritdoc />
        public InvocationResult Invoke(CilExecutionContext context, IMethodDescriptor method, IList<BitVector> arguments)
        {
            var reflectionMethod = FindReflectionMethod(method);
            if (reflectionMethod is null)
                throw new CilEmulatorException($"Could not resolve {method} to a MethodBase.");

            var marshaller = context.Machine.ObjectMarshaller;
            (object? marshalledInstance, object?[] marshalledArguments) = MarshalArguments(
                reflectionMethod,
                arguments, 
                marshaller); 

            try
            {
                // TODO: replace with dynamic method to force call specific method.
                object? result = reflectionMethod.Invoke(marshalledInstance, marshalledArguments);
                return InvocationResult.StepOver(method.Signature!.ReturnsValue 
                    ? marshaller.ToBitVector(result)
                    : null);
            }
            catch (Exception ex)
            {
                return InvocationResult.Exception(marshaller.ToObjectHandle(ex));
            }
        }

        private static MethodBase? FindReflectionMethod(IMethodDescriptor method)
        {
            if (method.Name is null || method.DeclaringType is null)
                return null;
            
            // Find declaring type.
            var reflectionType = FindReflectionType(method.DeclaringType);
            if (reflectionType is null)
                return null;

            // Select appropriate method collection to search for candidates.
            var collection = method.Name.Value is ".ctor" or ".cctor"
                ? reflectionType
                    .GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                    .Cast<MethodBase>()
                : reflectionType.GetRuntimeMethods();

            var returnType = FindReflectionType(method.Signature!.ReturnType);
            
            // Search through candidates.
            foreach (var candidate in collection)
            {
                // Short circuit on name.
                if (candidate.Name != method.Name)
                    continue;

                // Short circuit on return type.
                if (candidate is MethodInfo methodInfo && methodInfo.ReturnType != returnType)
                    continue;

                // Check all parameters.
                var parameters = candidate.GetParameters();
                if (parameters.Length != method.Signature.ParameterTypes.Count)
                    continue;
                
                bool fullMatch = true;
                for (int i = 0; fullMatch && i < parameters.Length; i++)
                {
                    if (parameters[i].ParameterType != FindReflectionType(method.Signature!.ParameterTypes[i]))
                        fullMatch = false;
                }

                if (fullMatch)
                    return candidate;
            }

            return null;
        }

        private static Type? FindReflectionType(ITypeDescriptor type)
        {
            var descriptor = type.Scope?.GetAssembly();
            if (descriptor is null)
                return null;
            
            var assembly = Assembly.Load(descriptor.FullName);
            return assembly.ManifestModule.GetType(type.FullName);
        }

        private static (object? Instance, object?[] Arguments) MarshalArguments(
            MethodBase reflectionMethod,
            IList<BitVector> arguments, 
            IObjectMarshaller marshaller)
        {
            object? marshalledInstance = null;
            int offset = 0;

            if (!reflectionMethod.IsStatic)
            {
                marshalledInstance = marshaller.ToObject(arguments[0], reflectionMethod.ReflectedType!);
                offset = 1;
            }

            var reflectionParameters = reflectionMethod.GetParameters();
            object?[] marshalledArguments = new object?[reflectionParameters.Length];
            for (int i = 0; i < reflectionParameters.Length; i++)
            {
                marshalledArguments[i] = marshaller.ToObject(
                    arguments[i + offset], 
                    reflectionParameters[i].ParameterType);
            }

            return (marshalledInstance, marshalledArguments);
        }
    }
}