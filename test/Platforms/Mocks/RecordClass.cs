namespace Mocks
{
    public class RecordClass
    {
        private readonly int _x;
        private readonly int _y;

        public RecordClass(int x, int y)
        {
            _x = x;
            _y = y;
        }

        public int InstanceMethod(int z) => _x + _y + z;

        public static int StaticMethod(int x, int y) => x + y;
    }
}