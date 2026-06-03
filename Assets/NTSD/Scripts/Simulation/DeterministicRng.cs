namespace NTSD.Simulation
{
    /// <summary>
    /// Release-aligned pseudo-random number generator.
    /// C++ release uses ntsd_rand(): seed = seed * 0x343FD + 0x269EC3,
    /// result = (seed >> 16) & 0x7FFF.
    /// </summary>
    public sealed class DeterministicRng
    {
        private uint _seed;

        public DeterministicRng()
        {
            _seed = 0;
        }

        public DeterministicRng(int seed)
        {
            Seed(seed);
        }

        public DeterministicRng(uint seed)
        {
            Seed(seed);
        }

        public void Seed(int seed)
        {
            _seed = unchecked((uint)seed);
        }

        public void Seed(uint seed)
        {
            _seed = seed;
        }

        public int NextRaw()
        {
            unchecked
            {
                _seed = _seed * 0x343FDu + 0x269EC3u;
            }

            return (int)((_seed >> 16) & 0x7FFFu);
        }

        public float Next()
        {
            return NextRaw() / 32768f;
        }

        public int NextInt(int a, int b)
        {
            if (b <= a) return a;
            return a + (NextRaw() % (b - a));
        }
    }
}
