namespace NTSD.Simulation
{
    /// <summary>
    /// 确定性伪随机数生成器 — 对齐 FLF third_party/random.js (George Marsaglia MWC)
    /// 
    /// FLF 调用链：
    ///   manager.js:229  randomseed = new Random(); randomseed.seed(824163532)
    ///   match.js:787    rand.seed(this.manager.random())  // 每局用全局随机数做种子
    ///   match.js:794    match.prototype.random = function() { return this.randomseed.next() }
    ///   character.js    $.match.random() < 0.5
    ///
    /// FLF random.js 原始实现：
    ///   this.seed = function(x) { this.x = x * 3253; this.y = this.nextX() }
    ///   this.nextX = function() { return 36969 * (this.x & 0xFFFF) + (this.x >> 16) }
    ///   this.nextY = function() { return 18273 * (this.y & 0xFFFF) + (this.y >> 16) }
    ///   this.next = function() {
    ///     if (this.x == 0) this.x = -1;
    ///     if (this.y == 0) this.y = -1;
    ///     this.x = this.nextX();
    ///     this.y = this.nextY();
    ///     return ((this.x << 16) + (this.y & 0xFFFF)) / 0xFFFFFFFF + 0.5;
    ///   }
    /// </summary>
    public sealed class DeterministicRng
    {
        private int _x;
        private int _y;

        public DeterministicRng()
        {
            _x = -1;
            _y = -1;
        }

        public DeterministicRng(int seed)
        {
            Seed(seed);
        }

        /// <summary>对齐 FLF: this.seed = function(x) { this.x = x*3253; this.y = this.nextX() }</summary>
        public void Seed(int x)
        {
            _x = x * 3253;
            _y = NextX();
        }

        /// <summary>对齐 FLF: this.seed2d</summary>
        public void Seed2D(int x, int y)
        {
            _x = x * 2549 + y * 3571;
            _y = y * 2549 + x * 3571;
        }

        private int NextX()
        {
            return 36969 * (_x & 0xFFFF) + (_x >> 16);
        }

        private int NextY()
        {
            return 18273 * (_y & 0xFFFF) + (_y >> 16);
        }

        /// <summary>
        /// 返回 [0, 1) 的确定性浮点数
        /// 对齐 FLF: ((this.x << 16) + (this.y & 0xFFFF)) / 0xFFFFFFFF + 0.5
        /// </summary>
        public float Next()
        {
            if (_x == 0) _x = -1;
            if (_y == 0) _y = -1;

            _x = NextX();
            _y = NextY();

            // JS 中 0xFFFFFFFF 是 4294967295.0（无符号），C# 中需要用 uint 转 double 保持一致
            uint combined = (uint)((_x << 16) + (_y & 0xFFFF));
            return (float)(combined / 4294967295.0);
        }

        /// <summary>返回 [a, b) 的确定性整数</summary>
        public int NextInt(int a, int b)
        {
            return (int)(Next() * (b - a)) + a;
        }
    }
}
