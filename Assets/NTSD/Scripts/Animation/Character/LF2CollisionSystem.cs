using System.Collections.Generic;
using NTSD.Animation.LF2Objects;
using NTSD.Simulation;
using NTSD.Tools;
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

        // ======== blocking_xz（对齐 FLF mechanics.js）========
        private static readonly List<LF2BlockingObstacle> s_blockingObstacles = new List<LF2BlockingObstacle>(64);

        internal static void RegisterBlockingObstacle(LF2BlockingObstacle obstacle)
        {
            if (obstacle == null) return;
            if (!s_blockingObstacles.Contains(obstacle)) s_blockingObstacles.Add(obstacle);
        }

        internal static void UnregisterBlockingObstacle(LF2BlockingObstacle obstacle)
        {
            if (obstacle == null) return;
            s_blockingObstacles.Remove(obstacle);
        }

        /// <summary>
        /// 对齐 FLF mech.blocking_xz()：预测下一步（PS.vx/PS.vz）是否会被 kind:14 阻挡。
        /// 注意：这里使用 Unity ground plane（X/Y）上的阻挡体（LF2BlockingObstacle），属于方案 1 的“显式障碍物”。
        /// </summary>
        public static bool BlockingXZ(LF2LivingObject actor)
        {
            if (actor == null || actor.PS == null) return false;
            return BlockingXZ(actor, actor.PS.vx, actor.PS.vz);
        }

        private static readonly List<PhysicsState.FlfVolume> s_tmpActorBodies = new List<PhysicsState.FlfVolume>(8);
        private static readonly List<PhysicsState.FlfVolume> s_tmpItr14 = new List<PhysicsState.FlfVolume>(8);

        public static bool BlockingXZ(LF2LivingObject actor, float vxPx, float vzPx)
        {
            if (actor == null || actor.PS == null) return false;
            if (s_blockingObstacles.Count == 0) return false;

            var frame = actor.Frame.D;
            if (frame == null) return false;

            float spriteWidthPx = actor.GetSpriteWidthPxForCollision();
            if (spriteWidthPx <= 0f) return false;

            // 对齐 FLF mech.blocking_xz():
            // - 用当前 frame 的 body 体积，带 offset(vx,vz) 预测下一步位置
            // - 并将 body.zwidth 置为 0（FLF: body[i].zwidth = 0）
            actor.PS.FillBodyVolumes(
                s_tmpActorBodies,
                frame.bodies,
                frame.centerx,
                frame.centery,
                spriteWidthPx,
                zwidthPx: 0f,
                offsetX: vxPx,
                offsetY: 0f,
                offsetZ: vzPx
            );

            if (s_tmpActorBodies.Count == 0) return false;

            for (int i = s_blockingObstacles.Count - 1; i >= 0; i--)
            {
                var obs = s_blockingObstacles[i];
                if (obs == null || !obs.isActiveAndEnabled)
                {
                    s_blockingObstacles.RemoveAt(i);
                    continue;
                }

                int count = obs.FillItr14Volumes(s_tmpItr14);
                if (count <= 0) continue;

                for (int b = 0; b < s_tmpActorBodies.Count; b++)
                {
                    var body = s_tmpActorBodies[b];
                    for (int k = 0; k < s_tmpItr14.Count; k++)
                    {
                        if (Intersect(body, s_tmpItr14[k]))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        // FLF default: GC.default.itr.hit_stop = 3
        // Source: I:\C++Test\NTSD\F.LF-master\LF\global.js:131
        private const int FLF_DEFAULT_ITR_HIT_STOP = 3;

        public struct HitEvent
        {
            public int tickIndex;
            public LF2LivingObject attacker;
            public LF2LivingObject target;
            public InteractionArea itr;
        }

        public static System.Action<HitEvent> OnHit;

        public struct PreInteractionEvent
        {
            public int tickIndex;
            public LF2LivingObject actor;
            public LF2LivingObject target;
            public InteractionArea itr;
        }

        public static System.Action<PreInteractionEvent> OnPreInteraction;

        public static void ProcessPreInteractionTick()
        {
            int tickIndex = GetCurrentTickIndexFallback();
            if (_lastPreProcessedTick == tickIndex) return;
            _lastPreProcessedTick = tickIndex;

            //LF2LivingObject[] animators = Object.FindObjectsByType<LF2LivingObject>(FindObjectsSortMode.None);

            LF2LivingObject[] animators = new LF2LivingObject[10];
            System.Array.Sort(animators, (a, b) =>
            {
                if (a == null && b == null) return 0;
                if (a == null) return 1;
                if (b == null) return -1;
                return a.StableId.CompareTo(b.StableId);
            });

            for (int a = 0; a < animators.Length; a++)
            {
                LF2LivingObject actor = animators[a];
                if (actor == null || actor.PS == null) continue;
                if (actor.Frame.D == null) continue;
                if (!actor.ItrRest.ArestTest()) continue;

                bool triggered = false;

                int nextFrameId = actor.Trans.NextFrameResolved();
                LF2FrameData nextFrame = actor.GetFrameDataById(nextFrameId);
                if (nextFrame == null || nextFrame.itrs == null || nextFrame.itrs.Count == 0) continue;

                float actorSpriteWidthPx = actor.GetSpriteWidthPxForCollision();
                if (actorSpriteWidthPx <= 0f) continue;

                List<PhysicsState.FlfVolume> actorItrVolumes = actor.PS.GetItrVolumes(
                    nextFrame.itrs,
                    nextFrame.centerx,
                    nextFrame.centery,
                    actorSpriteWidthPx,
                    itrZWidthPx: 0f
                );

                for (int t = 0; t < animators.Length; t++)
                {
                    if (t == a) continue;
                    LF2LivingObject target = animators[t];
                    if (target == null || target.PS == null) continue;
                    if (target.Frame.D == null) continue;

                    // pre_interaction 的 body 查询没有 vrest gating，但仍受 arest 限制（actor 自身）

                    float targetSpriteWidthPx = target.GetSpriteWidthPxForCollision();
                    if (targetSpriteWidthPx <= 0f) continue;

                    List<PhysicsState.FlfVolume> targetBodyVolumes = target.PS.GetBodyVolumes(
                        target.Frame.D.bodies,
                        target.Frame.D.centerx,
                        target.Frame.D.centery,
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
                                actor.ItrRest.ArestUpdate(itr);

                                var evt = new PreInteractionEvent
                                {
                                    tickIndex = tickIndex,
                                    actor = actor,
                                    target = target,
                                    itr = itr
                                };

                                if (DebugLog)
                                {
                                    Log.Info("[LF2CollisionSystem] PRE_INTERACTION tick={0} kind={1} actor={2} target={3}", tickIndex, itr.kind, actor.Name, target.Name);
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
            //LF2LivingObject[] animators = Object.FindObjectsByType<LF2LivingObject>(FindObjectsSortMode.None);
            LF2LivingObject[] animators = new LF2LivingObject[10];
            System.Array.Sort(animators, (a, b) =>
            {
                if (a == null && b == null) return 0;
                if (a == null) return 1;
                if (b == null) return -1;
                return a.StableId.CompareTo(b.StableId);
            });

            for (int a = 0; a < animators.Length; a++)
            {
                LF2LivingObject attacker = animators[a];
                if (attacker == null || attacker.PS == null) continue;
                if (attacker.Frame.D == null) continue;

                // 只处理攻击类 ITR（先做最小闭环）
                var itrs = attacker.Frame.D.itrs;
                if (itrs == null || itrs.Count == 0) continue;

                float attackerSpriteWidthPx = attacker.GetSpriteWidthPxForCollision();
                if (attackerSpriteWidthPx <= 0f) continue;

                List<PhysicsState.FlfVolume> attackerItrVolumes = attacker.PS.GetItrVolumes(
                    itrs,
                    attacker.Frame.D.centerx,
                    attacker.Frame.D.centery,
                    attackerSpriteWidthPx,
                    itrZWidthPx: 0f
                );

                for (int t = 0; t < animators.Length; t++)
                {
                    if (t == a) continue;
                    LF2LivingObject target = animators[t];
                    if (target == null || target.PS == null) continue;
                    if (target.Frame.D == null) continue;

                    // vrest on target (per attacker), arest on attacker
                    if (!attacker.ItrRest.ArestTest()) continue;
                    if (!target.ItrRest.VrestTest(attacker.StableId)) continue;

                    float targetSpriteWidthPx = target.GetSpriteWidthPxForCollision();
                    if (targetSpriteWidthPx <= 0f) continue;

                    List<PhysicsState.FlfVolume> targetBodyVolumes = target.PS.GetBodyVolumes(
                        target.Frame.D.bodies,
                        target.Frame.D.centerx,
                        target.Frame.D.centery,
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
                                    Log.Info("[LF2CollisionSystem] HIT tick={0} kind={1} attacker={2} target={3}", tickIndex, itr.kind, attacker.Name, target.Name);
                                }

                                // Update rests (FLF):
                                // - attacker: itr_arest_update(ITR)
                                // - target: itr_vrest_update(att.uid, ITR)
                                attacker.ItrRest.ArestUpdate(itr);
                                target.ItrRest.VrestUpdate(attacker.StableId, itr);

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
                                    attacker.Trans.IncWait(FLF_DEFAULT_ITR_HIT_STOP, 10);
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
            return SimulationTickDriver.Instance != null ? SimulationTickDriver.Instance.CurrentTickIndex : Time.frameCount;
        }
    }
}
