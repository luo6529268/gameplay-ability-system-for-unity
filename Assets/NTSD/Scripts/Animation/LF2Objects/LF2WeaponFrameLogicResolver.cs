using System.Collections.Generic;
using NTSD.Simulation;
using UnityEngine;

namespace NTSD.Animation.LF2Objects
{
    /// <summary>
    /// 武器帧逻辑解析器。
    ///
    /// 这个类只处理武器在通用帧推进之前的特殊前置逻辑，
    /// 主要是 `hit_Fa` 触发的追踪、回收、自动索敌这类行为。
    /// </summary>
    internal sealed class LF2WeaponFrameLogicResolver
    {
        private readonly LF2WeaponBase _weapon;
        private readonly List<LF2Entity> _boomerangQueryCache = new List<LF2Entity>(8);

        public LF2WeaponFrameLogicResolver(LF2WeaponBase weapon)
        {
            _weapon = weapon;
        }

        public bool ApplyBeforeWaitAdvance()
        {
            LF2FrameData frame = _weapon.Frame?.D;
            if (frame == null || _weapon.Runtime == null)
                return false;

            if (_weapon.WeaponType == 2 &&
                frame.state == LF2States.HeavyWeaponInSky &&
                _weapon.Runtime.YInt == 0 &&
                System.Math.Abs(_weapon.Runtime.Vx) < 0.1)
            {
                _weapon.SetFrameTickDirectForOwnedModule(20);
                return _weapon.Frame?.D != null;
            }

            return true;
        }

        // 每个逻辑帧先在这里做一次“武器专属预处理”，
        // 然后外层才会进入统一的 frame advance / dynamics 流程。
        public void RunWeaponFrameLogicBeforeAdvance()
        {
            int state = _weapon.GetRuntimeWeaponState();
            int hitFa = _weapon.Frame?.D?.hit_Fa ?? 0;
            int currentDataType = _weapon.GetCurrentDataObjectTypeForSimulation();

            if ((currentDataType == (int)LF2ObjectType.ThrowWeapon ||
                 currentDataType == (int)LF2ObjectType.Drink) &&
                state == LF2States.WeaponInSky &&
                (_weapon.Runtime.Vx > NTSDGlobal.Gameplay.WeaponBoomerangVxMax ||
                 _weapon.Runtime.Vx < NTSDGlobal.Gameplay.WeaponBoomerangVxMin))
            {
                _weapon.SetFrameLogicRawFramePreserveAttacking(40);
            }

            if (state == LF2States.WeaponThrowing)
            {
                _weapon.Runtime.WeaponState = LF2States.HeavyWeaponInSky;
            }
            else if (state == LF2States.HeavyWeaponInSky)
            {
                _weapon.Runtime.Vx *= 0.5f;
                if (System.Math.Abs(_weapon.Runtime.Vx) < 0.5)
                {
                    _weapon.Runtime.Vx = 0f;
                    _weapon.Runtime.WeaponState = LF2States.ProjectileFlying;
                }
            }

            if (hitFa == 4)
            {
                RunWeaponHitFa4FrameLogic();
                return;
            }

            if (hitFa == 12)
                RunWeaponHitFa12FrameLogic();
        }

        // hit_Fa=4：典型回旋追踪逻辑。
        // 目标足够接近时直接进入接住帧，否则持续修正速度朝目标飞。
        private void RunWeaponHitFa4FrameLogic()
        {
            LF2Entity target = ResolveWeaponHitFa4Target();
            if (_weapon.Health != null && _weapon.Health.HP <= 0)
            {
                ApplyWeaponHitFaNoTargetCatch();
                return;
            }

            if (target != null && target.Health != null && target.Health.HP > 0)
            {
                int dx = target.GetRuntimeXInt() - _weapon.GetRuntimeXInt();
                int dy = target.GetRuntimeYInt() - _weapon.GetRuntimeYInt();
                int dz = GetFrameLogicZInt(target) - GetFrameLogicZInt(_weapon);
                if (dx > -30 && dx < 30 && dy > 0 && dy < 80 && dz > -10 && dz < 10)
                {
                    _weapon.Runtime.Vx = 0f;
                    _weapon.Runtime.Vy = 0f;
                    _weapon.Runtime.Vz = 0f;
                    _weapon.SetFrameLogicRawFramePreserveAttacking(60);
                    target.CatchTimer = 100;
                    return;
                }
            }

            if (target == null)
            {
                ApplyWeaponHitFaNoTargetCatch();
                return;
            }

            int selfX = _weapon.GetRuntimeXInt();
            int targetX = target.GetRuntimeXInt();
            int selfZ = GetFrameLogicZInt(_weapon);
            int targetZ = GetFrameLogicZInt(target);

            if (targetX > selfX)
                _weapon.Runtime.Vx += 0.7f;
            if (targetX < selfX)
                _weapon.Runtime.Vx -= 0.7f;
            if (targetZ > selfZ + 5)
                _weapon.Runtime.Vz += 0.4f;
            if (targetZ < selfZ - 5)
                _weapon.Runtime.Vz -= 0.4f;
            _weapon.Runtime.Vy *= 0.7142857142857143; // P0-f-2b B2-3c: VALUE-BUG 5f/7f→0.7142857142857143 (baseline FrameAdvance.cs Vy*=0.7142857142857143)

            if (target.GetCurrentDataObjectType() == (int)LF2ObjectType.Character)
            {
                if (_weapon.Runtime.Y + 40f < target.Runtime.Y)
                    _weapon.Runtime.Y += 1f;
                if (_weapon.Runtime.Y + 40f > target.Runtime.Y)
                    _weapon.Runtime.Y -= 1f;
            }
            else if (_weapon.Runtime.Y > 0f)
            {
                _weapon.Runtime.Y += 1f;
            }

            _weapon.Runtime.Vx = System.Math.Clamp(_weapon.Runtime.Vx, -14.0, 14.0);
            if (_weapon.Runtime.Y > 1.4f)
                _weapon.Runtime.Y = 1.4f;
            _weapon.Runtime.Vz = System.Math.Clamp(_weapon.Runtime.Vz, -2.2, 2.2);
            _weapon.SwitchDir(_weapon.Runtime.Vx > 0f ? "right" : "left");
            _weapon.Runtime.SyncIntegerPosition();
        }

        // 优先使用记录下来的持有者/投掷者运行槽位作为追踪目标。
        private LF2Entity ResolveWeaponHitFa4Target()
        {
            if (_weapon.PickerStableId < 0)
                return null;

            SimulationWorld world = _weapon.Match;
            if (world == null)
                return null;

            return world.FindEntityByRuntimeSlotForQuery(_weapon.PickerStableId) ??
                   world.FindEntityByRuntimeSlotIncludingPending(_weapon.PickerStableId);
        }

        // 失去目标后不再精确追踪，只保留一个继续回头飞的退化行为。
        private void ApplyWeaponHitFaNoTargetCatch()
        {
            if (_weapon.Runtime.Vx < 0f)
                _weapon.Runtime.Vx -= 2f;
            else
                _weapon.Runtime.Vx += 2f;
            _weapon.Runtime.Vx = System.Math.Clamp(_weapon.Runtime.Vx, -17.0, 17.0);
            if (_weapon.Runtime.Y > 1.4f)
                _weapon.Runtime.Y = 1.4f;
            _weapon.SwitchDir(_weapon.Runtime.Vx > 0f ? "right" : "left");
            _weapon.Runtime.SyncIntegerPosition();
        }

        // 某些 type=3 对象的渲染 z 和逻辑 z 不完全一样，这里取逻辑判定坐标。
        private static int GetFrameLogicZInt(LF2Entity entity)
        {
            if (entity == null)
                return 0;

            if (entity.GetCurrentDataObjectType() == (int)LF2ObjectType.SpecialAttack &&
                entity.Runtime != null &&
                System.Math.Abs(entity.Runtime.Type3VisualZOffset) > 0.0001)
            {
                return (int)(entity.Runtime.Z - entity.Runtime.Type3VisualZOffset);
            }

            return entity.GetRenderZInt();
        }

        // hit_Fa=12：类似自动索敌回旋。
        // 没有合法目标时，原逻辑会直接让武器进入生命耗尽路径。
        private void RunWeaponHitFa12FrameLogic()
        {
            LF2Entity target = ResolveWeaponHitFa12Target();
            if (target == null)
            {
                if (_weapon.Health != null)
                    _weapon.Health.HP = 0;
                return;
            }

            int selfX = _weapon.GetRuntimeXInt();
            int targetX = target.GetRuntimeXInt();
            int selfZ = _weapon.Runtime.ZInt;
            int targetZ = target.Runtime.ZInt;

            if (targetX > selfX)
                _weapon.Runtime.Vx += 0.7f;
            if (targetX < selfX)
                _weapon.Runtime.Vx -= 0.7f;
            if (targetZ > selfZ + 5)
                _weapon.Runtime.Vz += 0.4f;
            if (targetZ < selfZ - 5)
                _weapon.Runtime.Vz -= 0.4f;
            _weapon.Runtime.Vy *= 0.7142857142857143; // P0-f-2b B2-3c: VALUE-BUG 5f/7f→0.7142857142857143 (baseline FrameAdvance.cs Vy*=0.7142857142857143)

            if (_weapon.Runtime.Y + 40f < target.Runtime.Y)
                _weapon.Runtime.Y += 1f;
            if (_weapon.Runtime.Y + 40f > target.Runtime.Y)
                _weapon.Runtime.Y -= 1f;

            _weapon.Runtime.Vx = System.Math.Clamp(_weapon.Runtime.Vx, -14.0, 14.0);
            if (_weapon.Runtime.Y > 1.4f)
                _weapon.Runtime.Y = 1.4f;
            _weapon.Runtime.Vz = System.Math.Clamp(_weapon.Runtime.Vz, -2.2, 2.2);
            _weapon.SwitchDir(_weapon.Runtime.Vx > 0f ? "right" : "left");
            _weapon.Runtime.SyncIntegerPosition();
        }

        // 先尝试复用旧目标；失效后再扫描战场，选择最近的合法角色。
        private LF2Entity ResolveWeaponHitFa12Target()
        {
            SimulationWorld world = _weapon.Match;
            if (world == null)
                return null;

            int currentTargetSlot = _weapon.PickerStableId;
            int selfTeam = ResolveHitFa12RelationIdentity(_weapon);
            int holderTeam = -1;
            if (_weapon.SpawnerEntityIndex >= 0)
            {
                LF2Entity holder = world.FindEntityByRuntimeSlotForQuery(_weapon.SpawnerEntityIndex);
                if (holder != null)
                    holderTeam = ResolveHitFa12RelationIdentity(holder);
            }

            bool needScan = true;
            LF2Entity target = world.FindEntityByRuntimeSlotForQuery(currentTargetSlot);
            if (target != null)
            {
                bool valid = IsHitFa12CharacterTarget(target) &&
                             target.Health != null &&
                             target.Health.HP > 0 &&
                             target.GetState() != LF2States.Lying &&
                             System.Math.Abs(target.HitStun) <= 2 &&
                             ResolveHitFa12RelationIdentity(target) != selfTeam;
                if (valid && holderTeam != ResolveHitFa12RelationIdentity(target))
                    needScan = false;
                if (!valid)
                    target = null;
            }

            if (!needScan)
                return target;

            world.GetAllEntities(_boomerangQueryCache);

            LF2Entity best = null;
            int bestDist = int.MaxValue;
            int bestSlot = -1;
            int selfX = _weapon.GetRuntimeXInt();
            int selfZ = _weapon.Runtime.ZInt;

            for (int i = 0; i < _boomerangQueryCache.Count; i++)
            {
                LF2Entity candidate = _boomerangQueryCache[i];
                if (candidate == null || ReferenceEquals(candidate, _weapon))
                    continue;
                if (!IsHitFa12CharacterTarget(candidate))
                    continue;
                if (candidate.Health == null || candidate.Health.HP <= 0)
                    continue;

                int candidateTeam = ResolveHitFa12RelationIdentity(candidate);
                if (candidateTeam == selfTeam)
                    continue;
                if (holderTeam >= 0 && candidateTeam == holderTeam)
                    continue;
                if ((candidate.GetState() == LF2States.Lying || System.Math.Abs(candidate.HitStun) > 2) &&
                    currentTargetSlot != -1)
                    continue;

                int dist = Mathf.Abs(candidate.GetRuntimeXInt() - selfX) +
                           Mathf.Abs(candidate.Runtime.ZInt - selfZ);
                if (dist >= bestDist)
                    continue;

                bestDist = dist;
                best = candidate;
                bestSlot = candidate.Runtime?.SlotIndex ?? -1;
            }

            _weapon.PickerStableId = bestSlot;

            return best;
        }

        // 只有“活着、非自己、非倒地、非同队”的角色才允许被这类追踪武器锁定。
        private bool IsValidWeaponHitFa12Target(LF2Entity target)
        {
            if (target == null || ReferenceEquals(target, _weapon))
                return false;
            if (target.GetCurrentDataObjectTypeForSimulation() != (int)LF2ObjectType.Character)
                return false;
            if (target.Health == null || target.Health.HP <= 0)
                return false;
            if (target.GetState() == LF2States.Lying)
                return false;
            if (_weapon.RelationTeam != 0 && target.RelationTeam == _weapon.RelationTeam)
                return false;

            return true;
        }

        private static bool IsHitFa12CharacterTarget(LF2Entity target)
        {
            return target != null &&
                   target.GetCurrentDataObjectTypeForSimulation() == (int)LF2ObjectType.Character;
        }

        private static int ResolveHitFa12RelationIdentity(LF2Entity entity)
        {
            if (entity == null)
                return 0;

            return entity.RelationTeam != 0 ? entity.RelationTeam : entity.Team;
        }
    }
}
