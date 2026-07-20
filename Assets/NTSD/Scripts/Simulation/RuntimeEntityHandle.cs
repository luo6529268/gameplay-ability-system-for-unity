using System;

namespace NTSD.Simulation
{
    public readonly struct RuntimeEntityHandle : IEquatable<RuntimeEntityHandle>
    {
        public static readonly RuntimeEntityHandle Invalid = new RuntimeEntityHandle(-1, 0);

        public RuntimeEntityHandle(int slot, uint generation)
        {
            Slot = slot;
            Generation = generation;
        }

        public int Slot { get; }
        public uint Generation { get; }
        public bool IsValid => Slot >= 0 && Generation != 0;

        public bool Equals(RuntimeEntityHandle other)
        {
            return Slot == other.Slot && Generation == other.Generation;
        }

        public override bool Equals(object obj)
        {
            return obj is RuntimeEntityHandle other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Slot * 397) ^ (int)Generation;
            }
        }

        public override string ToString()
        {
            return IsValid ? $"{Slot}:{Generation}" : "Invalid";
        }

        public static bool operator ==(RuntimeEntityHandle left, RuntimeEntityHandle right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(RuntimeEntityHandle left, RuntimeEntityHandle right)
        {
            return !left.Equals(right);
        }
    }
}
