using System;
using System.Collections.Generic;
using System.Diagnostics;
using AsmResolver.DotNet.Signatures.Types;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;

namespace Echo.Platforms.AsmResolver.Emulation;

[DebuggerTypeProxy(typeof(DebuggerProxy))]
public readonly partial struct ObjectHandle
{
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private object? Tag
    {
        get
        {
            if (IsNull)
                return null;

            try
            {
                return GetMethodTable().Type.FullName;
            }
            catch
            {
                return "<invalid>";
            }
        }
    }

    private sealed class DebuggerProxy
    {
        private readonly ObjectHandle _handle;

        public DebuggerProxy(ObjectHandle handle)
        {
            _handle = handle;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public KeyValuePair<object, object>[] Items
        {
            get
            {
                if (_handle.IsNull)
                    return Array.Empty<KeyValuePair<object, object>>();

                var type = _handle.GetMethodTable().Type.ToTypeSignature();

                return type switch
                {
                    CorLibTypeSignature {ElementType: ElementType.String} => GetStringValue(),
                    SzArrayTypeSignature arrayType => GetArrayValues(arrayType),
                    _ => GetObjectFieldValues(type)
                };
            }
        }

        private KeyValuePair<object, object>[] GetStringValue()
        {
            // We cannot infer the string data if we don't know its length.
            if (!_handle.ReadStringLength().IsFullyKnown)
                return Array.Empty<KeyValuePair<object, object>>();

            // Read and stringify the data.
            var data = _handle.ReadStringData();
            char[] result = new char[data.ByteCount / 2];
            for (int i = 0; i < result.Length; i++)
            {
                var c = data.AsSpan(i * 8 * sizeof(char), 8 * sizeof(char));
                result[i] = c.IsFullyKnown
                    ? (char) c.U16
                    : '?';
            }
            
            return new[] {new KeyValuePair<object, object>(0, new string(result))};
        }

        private KeyValuePair<object, object>[] GetArrayValues(SzArrayTypeSignature type)
        {
            // We cannot infer the array data if we don't know its length.
            var length = _handle.ReadArrayLength();
            if (!length.IsFullyKnown)
                return Array.Empty<KeyValuePair<object, object>>();

            var elementType = type.BaseType;

            // Collect all elements.
            var result = new KeyValuePair<object, object>[length.AsSpan().I32];
            for (int i = 0; i < result.Length; i++)
            {
                var element = _handle.ReadArrayElement(elementType, i);
                result[i] = new KeyValuePair<object, object>(i, element);
            }

            return result;
        }

        private KeyValuePair<object, object>[] GetObjectFieldValues(TypeSignature type)
        {
            // We need to resolve the type to know which fields to include.
            var definition = type.Resolve();
            if (definition is null)
                return Array.Empty<KeyValuePair<object, object>>();

            // Collect all fields and their values.
            var result = new List<KeyValuePair<object, object>>();
            for (int i = 0; i < definition.Fields.Count; i++)
            {
                var field = definition.Fields[i];
                if (field.IsStatic)
                    continue;

                result.Add(new KeyValuePair<object, object>(field, _handle.ReadField(field)));
            }

            return result.ToArray();
        }
    }
}