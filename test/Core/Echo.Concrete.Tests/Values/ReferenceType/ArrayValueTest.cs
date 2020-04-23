using System.Collections.Generic;
using Echo.Concrete.Values;
using Echo.Concrete.Values.ReferenceType;
using Echo.Concrete.Values.ValueType;
using Xunit;

namespace Echo.Concrete.Tests.Values.ReferenceType
{
    public class ArrayValueTest
    {
        [Fact]
        public void CreateEmptyArray()
        {
            var array = new ArrayValue();
            Assert.Empty(array);
            Assert.Equal(0, array.Length);
        }

        [Fact]
        public void CreateNewArrayWithDefaultValueShouldResultInShallowCopiesOfDefaultValue()
        {
            var array = new ArrayValue(2, new Integer32Value(0));
            
            var element1 = (Integer32Value) array[0];
            var element2 = (Integer32Value) array[1];
            
            Assert.NotSame(element1, element2);
            
            element1.Add(new Integer32Value(1234));
            Assert.NotEqual(element1.U32, element2.U32);
        }

        [Fact]
        public void CreateNewArrayFromExistingArray()
        {
            var elements = new IConcreteValue[]
            {
                new Integer32Value(0), new Integer32Value(1), new Integer32Value(2),
            };
            
            var array = new ArrayValue(elements);
            
            Assert.Equal(elements, array);
        }

        [Fact]
        public void ShallowCopyShouldReferenceSameArray()
        {
            var array = new ArrayValue(new IConcreteValue[]
            {
                new Integer32Value(0), new Integer32Value(1), new Integer32Value(2),
            });

            var copy = (ArrayValue) array.Copy();

            Assert.NotSame(array, copy);
            Assert.Equal((IEnumerable<IConcreteValue>) array, copy);
            
            copy[0] = new Integer32Value(1234);
            Assert.Equal(new Integer32Value(1234), array[0]);
        }
    }
}