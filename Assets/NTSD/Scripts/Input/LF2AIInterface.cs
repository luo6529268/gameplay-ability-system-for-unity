using NTSD.Animation;
using NTSD.Animation.LF2Objects;

namespace NTSD.Input
{
    /// <summary>
    /// AI 查询接口，对应 FLF AI.js AIin。
    ///
    /// 职责：
    /// - 向 AI 脚本暴露角色的只读状态查询方法
    /// - 屏蔽内部实现细节，提供语义清晰的 AI 友好 API
    ///
    /// 对应 FLF AI.js: AIin.facing / type / weapon_type / blink / shake /
    ///                  ctimer / seqcheck / frame / rand 等
    /// </summary>
    public class LF2AIInterface
    {
        private readonly LF2LivingObject _self;

        public LF2AIInterface(LF2LivingObject self)
        {
            _self = self;
        }

        // ==================== 状态查询 ====================

        /// <summary>朝向左时返回 true（对应 FLF AIin.facing）</summary>
        public bool Facing() => _self.PS?.dir == "left";

        /// <summary>对象类型数字（对应 FLF AIin.type）</summary>
        public int Type()
        {
            switch (_self.Type)
            {
                case LF2ObjectType.Character:     return 0;
                case LF2ObjectType.LightWeapon:   return 1;
                case LF2ObjectType.HeavyWeapon:   return 2;
                case LF2ObjectType.SpecialAttack: return 3;
                default:                          return 0;
            }
        }

        /// <summary>
        /// 持有武器类型（对应 FLF AIin.weapon_type）
        /// 0=无 1=轻武器 2=重武器 101=可投掷轻武器
        /// </summary>
        public int WeaponType()
        {
            var character = _self as LF2Character;
            var held = character?.GetHeldWeapon() as LF2LivingObject;
            if (held == null) return 0;

            switch (held.Type)
            {
                case LF2ObjectType.LightWeapon: return 1;
                case LF2ObjectType.HeavyWeapon: return 2;
                default:                        return 0;
            }
        }

        /// <summary>
        /// 当前帧的闪烁计时（对应 FLF AIin.blink）
        /// blink 激活时返回 timeout/2，否则 0
        /// </summary>
        public int Blink()
        {
            if (_self.Effect != null && _self.Effect.Blink)
                return System.Math.Max(0, _self.Effect.TimeOut / 2);
            return 0;
        }

        /// <summary>
        /// 抓取计时（对应 FLF AIin.ctimer）
        /// state==9(catching) 时 Catching 非空，返回 statemem.counter*6 近似值
        /// 注意：FLF 的 statemem.counter 在 C# 中通过 Frame.N 间接体现，此处简化为帧号
        /// </summary>
        public int Ctimer()
        {
            if (_self.GetState() != LF2States.Catching) return 0;
            if (_self.Catching == null) return 0;
            // 简化：对应 FLF statemem.counter * 6，用当前帧号近似
            return _self.Frame.N * 6;
        }

        /// <summary>帧数据查询（对应 FLF AIin.frame(N)）</summary>
        public AiFrameInfo Frame(int frameId)
        {
            var frameData = _self.GetFrameDataById(frameId);
            if (frameData == null) return new AiFrameInfo();

            return new AiFrameInfo
            {
                bdy_count = frameData.bodies?.Count ?? 0,
                itr_count = frameData.itrs?.Count ?? 0,
                state     = frameData.state,
                wait      = frameData.wait,
                next      = frameData.next,
            };
        }

        /// <summary>
        /// 当前角色状态（对应 FLF $.state()）
        /// </summary>
        public int State() => _self.GetState();

        /// <summary>
        /// 当前帧编号（对应 FLF $.frame.N）
        /// </summary>
        public int FrameN() => _self.Frame.N;

        /// <summary>
        /// 随机数（对应 FLF AIin.rand(i) = floor(random() * i)）
        /// </summary>
        public int Rand(int i)
        {
            if (i <= 0) return 0;
            return UnityEngine.Random.Range(0, i);
        }

        // ==================== 辅助结构 ====================

        /// <summary>AI 帧数据查询结果（对应 FLF AIin.frame() 返回的对象）</summary>
        public struct AiFrameInfo
        {
            public int bdy_count;
            public int itr_count;
            public int state;
            public int wait;
            public int next;
        }
    }
}
