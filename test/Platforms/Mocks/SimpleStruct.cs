// ReSharper disable UnassignedField.Global

using System;
using System.Runtime.InteropServices;

namespace Mocks
{
    [StructLayout(LayoutKind.Sequential)]
    public struct SimpleStruct
    {
        public int X;
        public int Y;
        public int Z;
        
        public SimpleStruct(int x, int y, int z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public bool Equals(SimpleStruct other)
        {
            return X == other.X && Y == other.Y && Z == other.Z;
        }

        public override bool Equals(object obj)
        {
            return obj is SimpleStruct other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y, Z);
        }

        public override string ToString()
        {
            return $"{nameof(X)}: {X}, {nameof(Y)}: {Y}, {nameof(Z)}: {Z}";
        }
    }
}