using NTSD.Simulation;
using UnityEngine;

namespace NTSD.Animation
{
    /// <summary>
    /// 角色受击相关计数器。
    /// fall、bdefend、AttackExempt、HitStateCount 都绑定到正式版实体 Runtime 字段。
    /// </summary>
    public sealed class LF2HitCountersModule
    {
        private NTSDEntityRuntime _runtime;
        private int _fall;
        private int _bdefend;
        private int _attackExempt;
        private int _hitStateCount;

        // fall/bdefend 的自然恢复是小数步进，累积到整数阈值后再写回正式字段。
        private float _fallAccum;
        private float _bdefendAccum;

        public int Fall => _runtime?.Fall ?? _fall;
        public int Bdefend => _runtime?.Bdefend ?? _bdefend;

        /// <summary>
        /// 攻击方碰撞豁免计数。命中后通常设为 6，每帧递减，用于跳过重复碰撞。
        /// </summary>
        public int AttackExempt => _runtime?.AttackExempt ?? _attackExempt;

        /// <summary>
        /// 受击状态计数。命中时通常设为 45，每帧递减，用于重击飞等分支判断。
        /// </summary>
        public int HitStateCount => _runtime?.HitStateCount ?? _hitStateCount;

        /// <summary>
        /// 绑定正式版实体运行时字段。绑定后计数器读写都落到 Runtime。
        /// </summary>
        public void BindRuntime(NTSDEntityRuntime runtime)
        {
            if (runtime == null) return;

            runtime.Fall = Fall;
            runtime.Bdefend = Bdefend;
            runtime.AttackExempt = AttackExempt;
            runtime.HitStateCount = HitStateCount;
            _runtime = runtime;
        }

        public void SetAttackExempt(int value)
        {
            if (_runtime != null) _runtime.AttackExempt = value;
            else _attackExempt = value;
        }

        public void SetHitStateCount(int value)
        {
            if (_runtime != null) _runtime.HitStateCount = value;
            else _hitStateCount = value;
        }

        public void AddHitStateCount(int amount)
        {
            SetHitStateCount(HitStateCount + amount);
        }

        public void Reset()
        {
            SetFall(0);
            SetBdefend(0);
            SetAttackExempt(0);
            SetHitStateCount(0);
            _fallAccum = 0f;
            _bdefendAccum = 0f;
        }

        public void AddFall(int amount)
        {
            SetFall(Mathf.Max(0, Fall + amount));
        }

        /// <summary>直接设置 fall 值，用于受击档位钳制。</summary>
        public void SetFall(int value)
        {
            if (_runtime != null) _runtime.Fall = value;
            else _fall = value;
        }

        public void ResetFall()
        {
            // 正式版击飞时写入 80，不是清零。
            SetFall(80);
            _fallAccum = 0f;
        }

        public void AddBdefend(int amount)
        {
            SetBdefend(Mathf.Max(0, Bdefend + amount));
        }

        public void ResetBdefend()
        {
            SetBdefend(0);
            _bdefendAccum = 0f;
        }

        /// <summary>直接设置 bdefend 值。</summary>
        public void SetBdefend(int value)
        {
            if (_runtime != null) _runtime.Bdefend = value;
            else _bdefend = value;
        }

        /// <summary>
        /// fall 自然恢复。amount 通常为负数，累积到 -1 后扣减整数值。
        /// </summary>
        public void RecoverFall(float amount)
        {
            if (Fall <= 0) return;

            _fallAccum += amount;
            if (_fallAccum <= -1f)
            {
                int decrement = Mathf.FloorToInt(-_fallAccum);
                SetFall(Mathf.Max(0, Fall - decrement));
                _fallAccum += decrement;
            }
        }

        /// <summary>
        /// bdefend 自然恢复。amount 通常为负数，累积到 -1 后扣减整数值。
        /// </summary>
        public void RecoverBdefend(float amount)
        {
            if (Bdefend <= 0) return;

            _bdefendAccum += amount;
            if (_bdefendAccum <= -1f)
            {
                int decrement = Mathf.FloorToInt(-_bdefendAccum);
                SetBdefend(Mathf.Max(0, Bdefend - decrement));
                _bdefendAccum += decrement;
            }
        }
    }
}
