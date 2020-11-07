using AsmResolver.DotNet;
using Echo.Concrete.Values;

namespace Echo.Platforms.AsmResolver.Emulation.Values
{
    public partial class LleStructValue : IDotNetStructValue
    {
        // -------------------------
        // Implementation rationale
        // -------------------------
        //
        // Objects in .NET are implemented as a single memory region containing all fields and are referenced by
        // a single pointer to the first byte of the object. Getting and setting field values of an object is done
        // by reading from this base pointer + the field offset. In C this would be something like the following: 
        //
        //    *(MyFieldType*) (ptr_to_TypeA_object + offset_MyField);
        //
        // or using pointer notation:
        //
        //    ptr_to_TypeA_object->MyField
        //
        // Although C# represents itself as a type-safe language, CIL is not. As a result, it is possible for a
        // ldfld instruction to perform type confusion, and read "non-existing" fields from a type, by pushing
        // an object reference of type A, and referencing a field from another type B in the  operand of the
        // ldfld instruction. For example, the following CIL snippet runs fine:
        //
        //     newobj     void TypeA::.ctor()
        //     ldfld      MyFieldType TypeB::MyField
        //
        // The runtime (tested on Mono and .NET Core) will reinterpret the pushed object as TypeB, even though it is
        // a pointer to TypeA, and use the field offset as if it was actually an instance of TypeB:
        //
        //    ((TypeB*) ptr_to_TypeA_object)->MyField
        // 
        // which is equivalent to:
        //
        //    *(MyFieldType*) (ptr_to_TypeA_object + offset_MyField))
        //
        // For this reason, we do not store the memory type layout of the current object within this class, but rather
        // query the memory allocator that was used to create this LLE object for the memory layout of the field's
        // declaring type.

        /// <inheritdoc />
        public IConcreteValue GetFieldValue(IFieldDescriptor field)
        {
            var typeMemoryLayout = _valueFactory.GetTypeMemoryLayout(field.DeclaringType);
            var fieldMemoryLayout = typeMemoryLayout[field.Resolve()];
            return this.ReadStruct((int) fieldMemoryLayout.Offset, _valueFactory, fieldMemoryLayout.ContentsLayout);
        }

        /// <inheritdoc />
        public void SetFieldValue(IFieldDescriptor field, IConcreteValue value)
        {
            var typeMemoryLayout = _valueFactory.GetTypeMemoryLayout(field.DeclaringType);
            var fieldMemoryLayout = typeMemoryLayout[field.Resolve()];
            this.WriteStruct((int) fieldMemoryLayout.Offset, _valueFactory, fieldMemoryLayout.ContentsLayout, value);
        }

    }
}