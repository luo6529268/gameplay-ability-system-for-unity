using System.Collections.Generic;
using NTSD.Simulation;
using UnityEngine;

namespace NTSD.Animation
{
    /// <summary>
    /// 最小 FLF 风格碰撞系统（Phase 0）
    /// - 仅做 ITR(攻击类 kind) vs BDY 的 overlap 检测
    /// - 体积格式严格对齐 FLF scene.js：{x,y,z,vx,vy,w,h,zwidth}
    /// - 默认每个 sim tick 只执行一次（防止被每个角色重复调用）
    ///
    /// 参考 FLF：
    /// - character.js: pre_interaction/post_interaction（vol.zwidth=0，query tag:'body'）
    /// - scene.js: intersect(rect_flat + zwidth 区间)
    /// </summary>
    public static class LF2CollisionSystem
    {
        private static int _lastPostProcessedTick = int.MinValue;
        private static int _lastPreProcessedTick = int.MinValue;

        public static bool DebugLog { get; set; } = false;

        // FLF default: GC.default.itr.hit_stop = 3
        // Source: I:\C++Test\NTSD\F.LF-master\LF\global.js:131
        private const int FLF_DEFAULT_ITR_HIT_STOP = 3;

        public struct HitEvent
        {
            public int tickIndex;
            public LF2CharacterAnimator attacker;
            public LF2CharacterAnimator target;
            public InteractionArea itr;
        }

        public static System.Action<HitEvent> OnHit;

        public struct PreInteractionEvent
        {
            public int tickIndex;
            public LF2CharacterAnimator actor;
            public LF2CharacterAnimator target;
            public InteractionArea itr;
        }

        public static System.Action<PreInteractionEvent> OnPreInteraction;

        public static void ProcessPreInteractionTick()
        {
            int tickIndex = GetCurrentTickIndexFallback();
            if (_lastPreProcessedTick == tickIndex) return;
            _lastPreProcessedTick = tickIndex;

            LF2CharacterAnimator[] animators = Object.FindObjectsByType<LF2CharacterAnimator>(FindObjectsSortMode.None);
            System.Array.Sort(animators, (a, b) =>
            {
                if (a == null && b == null) return 0;
                if (a == null) return 1;
                if (b == null) return -1;
                return a.StableId.CompareTo(b.StableId);
            });

            for (int a = 0; a < animators.Length; a++)
            {
                LF2CharacterAnimator actor = animators[a];
                if (actor == null || actor.ps == null) continue;
                if (actor.CurrentFrame == null) continue;
                if (!actor.ItrArestTest()) continue;

                bool triggered = false;

                int nextFrameId = actor.trans.NextFrameResolved();
                LF2FrameData nextFrame = actor.GetFrameDataById(nextFrameId);
                if (nextFrame == null || nextFrame.itrs == null || nextFrame.itrs.Count == 0) continue;

                float actorSpriteWidthPx = actor.GetSpriteWidthPxForCollision();
                if (actorSpriteWidthPx <= 0f) continue;

                List<PhysicsState.FlfVolume> actorItrVolumes = actor.ps.GetItrVolumes(
                    nextFrame.itrs,
                    nextFrame.centerx,
                    nextFrame.centery,
                    actorSpriteWidthPx,
                    itrZWidthPx: 0f
                );

                for (int t = 0; t < animators.Length; t++)
                {
                    if (t == a) continue;
                    LF2CharacterAnimator target = animators[t];
                    if (target == null || target.ps == null) continue;
                    if (target.CurrentFrame == null) continue;

                    // pre_interaction 的 body 查询没有 vrest gating，但仍受 arest 限制（actor 自身）

                    float targetSpriteWidthPx = target.GetSpriteWidthPxForCollision();
                    if (targetSpriteWidthPx <= 0f) continue;

                    List<PhysicsState.FlfVolume> targetBodyVolumes = target.ps.GetBodyVolumes(
                        target.CurrentFrame.bodies,
                        target.CurrentFrame.centerx,
                        target.CurrentFrame.centery,
                        targetSpriteWidthPx
                    );

                    for (int i = 0; i < nextFrame.itrs.Count && i < actorItrVolumes.Count; i++)
                    {
                        InteractionArea itr = nextFrame.itrs[i];
                        if (!IsPreInteractionKind(itr.kind)) continue;

                        PhysicsState.FlfVolume vol = actorItrVolumes[i];
                        for (int b = 0; b < targetBodyVolumes.Count; b++)
                        {
                            if (Intersect(vol, targetBodyVolumes[b]))
                            {
                                // FLF: pre_interaction 成功会触发 itr_arest_update(ITR)
                                actor.ItrArestUpdate(itr);

                                var evt = new PreInteractionEvent
                                {
                                    tickIndex = tickIndex,
                                    actor = actor,
                                    target = target,
                                    itr = itr
                                };

                                if (DebugLog)
                                {
                                    Debug.Log($"[LF2CollisionSystem] PRE_INTERACTION tick={tickIndex} kind={itr.kind} actor={actor.name} target={target.name}");
                                }

                                OnPreInteraction?.Invoke(evt);
                                CharacterStates.Instance.HandleStateEvent(actor, "pre_interaction", evt);
                                triggered = true;
                                break;
                            }
                        }

                        if (triggered) break;
                    }

                    if (triggered) break;
                }

                if (triggered) continue;
            }
        }

        public static void ProcessPostInteractionTick()
        {
            int tickIndex = GetCurrentTickIndexFallback();
            if (_lastPostProcessedTick == tickIndex) return;
            _lastPostProcessedTick = tickIndex;

            // 最小实现：从场景收集所有角色 Animator
            // 后续如要严格 determinism，应改为 SimulationWorld 的确定性列表。
            LF2CharacterAnimator[] animators = Object.FindObjectsByType<LF2CharacterAnimator>(FindObjectsSortMode.None);
            System.Array.Sort(animators, (a, b) =>
            {
                if (a == null && b == null) return 0;
                if (a == null) return 1;
                if (b == null) return -1;
                return a.StableId.CompareTo(b.StableId);
            });

            for (int a = 0; a < animators.Length; a++)
            {
                LF2CharacterAnimator attacker = animators[a];
                if (attacker == null || attacker.ps == null) continue;
                if (attacker.CurrentFrame == null) continue;

                // 只处理攻击类 ITR（先做最小闭环）
                var itrs = attacker.CurrentFrame.itrs;
                if (itrs == null || itrs.Count == 0) continue;

                float attackerSpriteWidthPx = attacker.GetSpriteWidthPxForCollision();
                if (attackerSpriteWidthPx <= 0f) continue;

                List<PhysicsState.FlfVolume> attackerItrVolumes = attacker.ps.GetItrVolumes(
                    itrs,
                    attacker.CurrentFrame.centerx,
                    attacker.CurrentFrame.centery,
                    attackerSpriteWidthPx,
                    itrZWidthPx: 0f
                );

                for (int t = 0; t < animators.Length; t++)
                {
                    if (t == a) continue;
                    LF2CharacterAnimator target = animators[t];
                    if (target == null || target.ps == null) continue;
                    if (target.CurrentFrame == null) continue;

                    // vrest on target (per attacker), arest on attacker
                    if (!attacker.ItrArestTest()) continue;
                    if (!target.ItrVrestTest(attacker.StableId)) continue;

                    float targetSpriteWidthPx = target.GetSpriteWidthPxForCollision();
                    if (targetSpriteWidthPx <= 0f) continue;

                    List<PhysicsState.FlfVolume> targetBodyVolumes = target.ps.GetBodyVolumes(
                        target.CurrentFrame.bodies,
                        target.CurrentFrame.centerx,
                        target.CurrentFrame.centery,
                        targetSpriteWidthPx
                    );

                    // ITR vs BDY overlap
                    for (int i = 0; i < itrs.Count && i < attackerItrVolumes.Count; i++)
                    {
                        InteractionArea itr = itrs[i];
                        if (!IsAttackKind(itr.kind)) continue;

                        PhysicsState.FlfVolume attackVol = attackerItrVolumes[i];
                        for (int b = 0; b < targetBodyVolumes.Count; b++)
                        {
                            if (Intersect(attackVol, targetBodyVolumes[b]))
                            {
                                if (DebugLog)
                                {
                                    Debug.Log($"[LF2CollisionSystem] HIT tick={tickIndex} kind={itr.kind} attacker={attacker.name} target={target.name}");
                                }

                                // Update rests (FLF):
                                // - attacker: itr_arest_update(ITR)
                                // - target: itr_vrest_update(att.uid, ITR)
                                attacker.ItrArestUpdate(itr);
                                target.ItrVrestUpdate(attacker.StableId, itr);

                                // Phase 1: emit events that later map to FLF hit/hit_others hooks
                                var evt = new HitEvent
                                {
                                    tickIndex = tickIndex,
                                    attacker = attacker,
                                    target = target,
                                    itr = itr
                                };
                                OnHit?.Invoke(evt);

                                // Target: hit event
                                CharacterStates.Instance.HandleStateEvent(target, "hit", evt);

                                // Attacker: hit_others + hit_stop (with state override)
                                CharacterStates.Instance.HandleStateEvent(attacker, "hit_others", evt);
                                bool hitStopHandled = CharacterStates.Instance.HandleStateEvent(attacker, "hit_stop", evt);
                                if (!hitStopHandled)
                                {
                                    // FLF: if no state_update('hit_stop'), apply default hit_stop wait increase
                                    // (FLF also creates effect_stuck; we only do the timing part here)
                                    attacker.trans.IncWait(FLF_DEFAULT_ITR_HIT_STOP, 10);
                                }

                                // FLF arest 是 attacker 全局冷却：一旦命中，后续目标无需继续检测
                                goto NextAttacker;
                            }
                        }
                    }
                }

                NextAttacker: ;
            }
        }

        private static bool IsAttackKind(int kind)
        {
            // 来自 FLF character.js/post_interaction 与 specialattack.js/weapon.js 的攻击类集合（最小闭环）
            // - 0: normal, 4: falling, 9: rebound shield, 15: whirlwind, 16: special
            return kind == 0 || kind == 4 || kind == 9 || kind == 15 || kind == 16;
        }

        private static bool IsPreInteractionKind(int kind)
        {
            // 来自 FLF character.js:pre_interaction 的交互类集合（抓取/拾取）
            // - 1: grab, 3: super grab, 2: pick weapon, 7: easy pick (attack-only in FLF, but we only emit intent)
            return kind == 1 || kind == 2 || kind == 3 || kind == 7;
        }

        private static bool Intersect(in PhysicsState.FlfVolume a, in PhysicsState.FlfVolume b)
        {
            // 对齐 FLF scene.js: intersect()
            float aLeft = a.x + a.vx;
            float aTop = a.y + a.vy;
            float aRight = aLeft + a.w;
            float aBottom = aTop + a.h;

            float bLeft = b.x + b.vx;
            float bTop = b.y + b.vy;
            float bRight = bLeft + b.w;
            float bBottom = bTop + b.h;

            if (aBottom < bTop) return false;
            if (aTop > bBottom) return false;
            if (aRight < bLeft) return false;
            if (aLeft > bRight) return false;

            float aZMin = a.z - a.zwidth;
            float aZMax = a.z + a.zwidth;
            float bZMin = b.z - b.zwidth;
            float bZMax = b.z + b.zwidth;

            if (aZMax < bZMin) return false;
            if (aZMin > bZMax) return false;

            return true;
        }

        private static int GetCurrentTickIndexFallback()
        {
            // 优先使用 SimulationTickDriver 的确定性 tickIndex；否则 fallback 到 Time.frameCount
            return SimulationTickDriver.Instance.CurrentTickIndex;
        }
    }
}
