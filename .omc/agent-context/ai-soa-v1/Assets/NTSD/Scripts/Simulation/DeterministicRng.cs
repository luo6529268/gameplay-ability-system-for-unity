namespace NTSD.Simulation
{
    /// <summary>
    /// 对齐 C# 权威工程的确定性伪随机数生成器。
    /// 公式：seed = seed * 0x343FD + 0x269EC3，
    /// 返回值：(seed >> 16) & 0x7FFF。
    /// </summary>
    public sealed class DeterministicRng
    {
        private uint _seed;
        private ulong _callCount;

        public uint State => _seed;
        public ulong CallCount => _callCount;

        public DeterministicRng()
        {
            _seed = 0;
            _callCount = 0;
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
            _callCount = 0;
        }

        public void Seed(uint seed)
        {
            _seed = seed;
            _callCount = 0;
        }

        public int NextRaw()
        {
            unchecked
            {
                _seed = _seed * 0x343FDu + 0x269EC3u;
                _callCount++;
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

        internal void RestoreState(uint state, ulong callCount)
        {
            _seed = state;
            _callCount = callCount;
        }
    }
}
