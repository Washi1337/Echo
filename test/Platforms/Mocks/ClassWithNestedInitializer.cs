namespace Mocks;

    public class ClassWithNestedInitializer
    {
        public class Class1
        {
            public static int Field = Class2.Field;

            public static int Method() => Field;
        }

        public class Class2
        {
            public static int Field = 1337;
        }
    }