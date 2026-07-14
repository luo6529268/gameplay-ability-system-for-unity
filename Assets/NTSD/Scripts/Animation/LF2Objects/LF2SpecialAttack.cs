using UnityEngine;
using NTSD.App;
using NTSD.Animation;
using NTSD.Animation.LF2Tasks;
using NTSD.Extensions;
using NTSD.Tools;
using NTSD.Simulation;

namespace NTSD.Animation.LF2Objects
{
    /// <summary>
    /// 特殊攻击对象（投射物、能量球、技能生成物等）。
    /// 复刻基准是 C++ release 工程中的 Entity_FrameLogic、Entity_AI_Update 和 opoint 生成流程；
    /// Unity 这里只保留对象池、渲染引用和分层调度适配。
    /// </summary>
    public class LF2SpecialAttack : LF2Entity
    {
        // ========== 技能专属字段（不在 LF2Entity 的） ==========

        public override LF2ItrRestTracker ItrRest { get; protected set; }

        /// <summary>生命值（技能耐久/存活帧数等）</summary>
        public override LF2Health Health { get; protected set; } = new LF2Health();
        // ========== 配置字段 ==========
        private LF2LivingObject _parent;
        private int _lastState = -1;

        // ========== 状态机字段 ==========
        public bool NoBounce { get; set; }

        private bool _hitFaFired;

        private static bool IsWeaponEntity(LF2Entity entity)
        {
            return entity is LF2WeaponBase;
        }

        // ========== ILF2Object 实现 ==========
        public override LF2ObjectType ObjectTypeEnum => LF2ObjectType.SpecialAttack;

        public override void Init(LF2TaskBase taskBase, LF2ObjectRenderer renderer)
        {
            AllocateStableId();

            PS = new PhysicsState();
            PS.BindRuntime(Runtime);
            Health.BindRuntime(Runtime);
            Trans = new FrameTransistor(this);
            Frame = new LF2FrameInfo();
            Effect = new LF2EffectState();
            ItrRest = new LF2ItrRestTracker();
            Sprite = new LF2Sprite();

            if (!(taskBase is OPointCreateTask task))
            {
                Log.Error("[LF2SpecialAttack] Invalid task type");
                return;
            }

            InitializeParent(task);
            InitializeDirection(task);
            InitializeFrame(task);
            InitializePosition(task);
            InitializeVelocity(task);
            InitializeHealth();

            Renderer = renderer;
            SimulationTickDriver.Instance?.World?.Register(this);
        }

        protected override bool StateEntryEvent() => DispatchCurrentStateEvent("state_entry");

        protected override bool FrameEvent()
        {
            Generic_Frame();
            return DispatchCurrentStateEvent("frame");
        }

        protected override bool TUEvent()
        {
            Generic_TU();
            return DispatchCurrentStateEvent("TU");
        }

        protected override bool DieEvent()
        {
            Generic_Die();
            return true;
        }

        #region 通用状态处理

        private void Generic_TU()
        {
            SpecialAttackDynamics();
        }

        private void Generic_Frame()
        {
            var frame = Frame.D;
            if (frame == null) return;

            if (!string.IsNullOrEmpty(frame.sound))
            {
                PlaySound(frame.sound);
            }

            if (Frame.N == 15)
            {
                SetFrameDirect(1000);
            }
        }

        private void SpecialAttackDynamics()
        {
            if (PS == null) return;

            if (PS.vx > 0 && !PS.xBoundPositive)
                PS.x += PS.vx;
            else if (PS.vx < 0 && !PS.xBoundNegative)
                PS.x += PS.vx;

            if (PS.vz > 0 && !PS.zBoundPositive)
                PS.z += PS.vz;
            else if (PS.vz < 0 && !PS.zBoundNegative)
                PS.z += PS.vz;

            PS.xBoundPositive = false;
            PS.xBoundNegative = false;
            PS.zBoundPositive = false;
            PS.zBoundNegative = false;

            var frame = Frame.D;
            if (frame != null && frame.hit_j > 0)
            {
                PS.z += frame.hit_j - 50;
            }

            if (PS.y >= 0)
            {
                if (PS.vx > 0.0001f)
                {
                    PS.vx -= 1.0f;
                    if (PS.vx < 0.0001f) PS.vx = 0f;
                }
                else if (PS.vx < -0.0001f)
                {
                    PS.vx += 1.0f;
                    if (PS.vx > -0.0001f) PS.vx = 0f;
                }

                if (PS.vz > 0.0001f)
                {
                    PS.vz -= 1.0f;
                    if (PS.vz < 0.0001f) PS.vz = 0f;
                }
                else if (PS.vz < -0.0001f)
                {
                    PS.vz += 1.0f;
                    if (PS.vz > -0.0001f) PS.vz = 0f;
                }
            }

            PS.y += PS.vy;
            if (PS.y > 0f) PS.y = 0f;
            // C++ release：entity_type/type=3 的特殊攻击对象不追加重力。
        }

        private void Generic_Die()
        {
            var frame = Frame.D;
            if (frame != null && frame.hit_d != 0)
            {
                SetFrameDirect(frame.hit_d);
            }
        }

        #endregion

        #region 特定状态处理

        private bool State_15(string eventType, object eventData)
        {
            if (eventType == "TU")
                return ProcessState15TU();

            return false;
        }

        private bool State_3000(string eventType, object eventData)
        {
            return ProcessChaseStateEvent(eventType);
        }

        private bool State_3001(string eventType, object eventData)
        {
            return ProcessChaseStateEvent(eventType);
        }

        private bool State_3003(string eventType, object eventData)
        {
            return ProcessChaseStateEvent(eventType);
        }

        private bool State_3005(string eventType, object eventData)
        {
            return ProcessChaseStateEvent(eventType);
        }

        private bool State_3006(string eventType, object eventData)
        {
            return ProcessChaseStateEvent(eventType);
        }

        private bool DispatchCurrentStateEvent(string eventType, object eventData = null)
        {
            return GetState() switch
            {
                15 => State_15(eventType, eventData),
                LF2States.ProjectileFlying => State_3000(eventType, eventData),
                LF2States.ProjectileHiting => State_3001(eventType, eventData),
                LF2States.ProjectileTeleport => State_3003(eventType, eventData),
                LF2States.ObjectFlying => State_3005(eventType, eventData),
                LF2States.ObjectExpanding => State_3006(eventType, eventData),
                _ => false,
            };
        }

        private bool ProcessState15TU()
        {
            var frame = Frame.D;
            if (frame != null && frame.dvx != 0)
            {
                PS.vx = Dirh() * frame.dvx;
            }

            return true;
        }

        private bool ProcessChaseStateEvent(string eventType)
        {
            if (eventType != "TU")
                return false;

            ProcessChaseLogic();
            return true;
        }

        private void ProcessChaseLogic()
        {
            var frame = Frame.D;
            if (frame == null) return;

            int hitFa = frame.hit_Fa;

            if (hitFa == 10)
            {
                if (PS.vx < 0f) PS.vx -= 1.1f;
                else PS.vx += 1.1f;
                if (PS.vx > 30f) PS.vx = 30f;
                else if (PS.vx < -30f) PS.vx = -30f;
                if (PS.y <= 3f) PS.y = 3f;
                return;
            }

            if (hitFa == 11 && !_hitFaFired)
            {
                _hitFaFired = true;
                ApplyHitFa11Spawn();
                // 生成后查找最近敌人，并写入 OwnerEntityIndex。
                if (!ApplyHitFa11FindTarget())
                {
                    Health.HP = 0;
                    return;
                }
            }

            if (hitFa == 13 && !_hitFaFired)
            {
                _hitFaFired = true;
                ApplyHitFa13Spawn();
                return;
            }

            if (hitFa == 8 && !_hitFaFired)
            {
                _hitFaFired = true;
                ApplyHitFa8Spawn();
                return;
            }

            if (hitFa == 5 && !_hitFaFired)
            {
                _hitFaFired = true;
                ApplyHitFa5Spawn();
                return;
            }

            // 分支 6：oid=220，v221 上限为 0（无外层循环），v217 最大值为 7。
            // 分支 9：oid=rand(2)+221，v221 上限为 4，v217 最大值为 10。
            if ((hitFa == 6 || hitFa == 9) && !_hitFaFired)
            {
                _hitFaFired = true;
                ApplyHitFa6Or9Spawn(hitFa);
                return;
            }

            if (hitFa == 3)
            {
                if (!ApplyHitFa11FindTarget())
                {
                    Health.HP = 0;
                    return;
                }
                ApplyHitFa3Tracking();
                return;
            }

            // 根据 hit_Fa 分发逐帧追踪逻辑。
            if (hitFa == 1)
            {
                ApplyHitFa1Tracking();
                return;
            }

            if (hitFa == 2 || hitFa == 4 || hitFa == 7 || hitFa == 12 || hitFa == 14)
            {
                ApplyHitFa2_14Tracking(hitFa);
                return;
            }

            if (hitFa == 11)
            {
                ApplyHitFa11Tracking();
            }
        }

        // ========== N-11: hit_Fa=11 爆炸生成 ==========
        private void ApplyHitFa11Spawn()
        {
            double x = PS.x, y = PS.y, z = PS.z;
            double vx = PS.vx, vy = PS.vy, vz = PS.vz;
            bool facingRight = PS.dir == "right";
            int facing = facingRight ? 0 : 1;

            // 参数映射: arg_10=[ecx+10h]=z, arg_14=[ecx+14h]=y, arg_18=[ecx+18h]=x
            // 1. oid=211, frame=109, pos=self, vel=self, facing=self
            SpawnEntityDirect(211, 109, x, y, z, vx, vy, vz, facing);
            // 2. oid=221, frame=81, y-100, vel=self, facing=self
            SpawnEntityDirect(221, 81, x, y - 100, z, vx, vy, vz, facing);
            // 3. oid=212, frame=100, z+80, y-3, vz-7, facing=0
            SpawnEntityDirect(212, 100, x, y - 3, z + 80, vx, vy, vz - 7f, 0);
            // 4. oid=212, frame=100, z+100, y-3, facing=0
            SpawnEntityDirect(212, 100, x, y - 3, z + 100, vx, vy, vz, 0);
            // 5. oid=212, frame=100, z+80, y-3, vz+7, facing=0
            SpawnEntityDirect(212, 100, x, y - 3, z + 80, vx, vy, vz + 7f, 0);
            // 6. oid=212, frame=100, z-80, y-3, vz-7, facing=1
            SpawnEntityDirect(212, 100, x, y - 3, z - 80, vx, vy, vz - 7f, 1);
            // 7. oid=212, frame=100, z-100, y-3, facing=1
            SpawnEntityDirect(212, 100, x, y - 3, z - 100, vx, vy, vz, 1);
            // 8. oid=212, frame=100, z-80, y-3, vz+7, facing=1
            SpawnEntityDirect(212, 100, x, y - 3, z - 80, vx, vy, vz + 7f, 1);
            // 9. oid=211, frame=50, x-5, y-1, z-30, facing=1
            SpawnEntityDirect(211, 50, x - 5, y - 1, z - 30, vx, vy, vz, 1);
            // 10. oid=211, frame=50, x-5, y-1, z+30, facing=1
            SpawnEntityDirect(211, 50, x - 5, y - 1, z + 30, vx, vy, vz, 1);
            // 11. oid=211, frame=50, x+2, y-1, z-30, facing=0
            SpawnEntityDirect(211, 50, x + 2, y - 1, z - 30, vx, vy, vz, 0);
            // 12. oid=211, frame=50, x+2, y-1, z+30, facing=0
            SpawnEntityDirect(211, 50, x + 2, y - 1, z + 30, vx, vy, vz, 0);
            // 13. oid=211, frame=50, x-9, y-1, z=self, facing=1
            SpawnEntityDirect(211, 50, x - 9, y - 1, z, vx, vy, vz, 1);
            // 14. oid=211, frame=50, x+6, y-1, z=self, facing=0
            SpawnEntityDirect(211, 50, x + 6, y - 1, z, vx, vy, vz, 0);
        }

        private bool ApplyHitFa11FindTarget()
        {
            LF2CharacterDataWrapper savedWrapper = null;
            if (HealTimer >= 0)
            {
                var allObjs = new System.Collections.Generic.List<LF2LivingObject>(16);
                Match?.GetAllLivingObjects(allObjs);
                for (int i = 0; i < allObjs.Count; i++)
                {
                    if (allObjs[i].StableId == HealTimer && !allObjs[i].Dead)
                    {
                        savedWrapper = allObjs[i].FrameCache?.Wrapper;
                        break;
                    }
                }
            }

            if (OwnerEntityIndex >= 0)
            {
                var allObjs = new System.Collections.Generic.List<LF2LivingObject>(16);
                Match?.GetAllLivingObjects(allObjs);
                for (int i = 0; i < allObjs.Count; i++)
                {
                    var obj = allObjs[i];
                    if (obj.StableId != OwnerEntityIndex) continue;
                    if (obj.Dead) break;
                    if (obj.Health == null || obj.Health.HP <= 0) break;
                    if (obj.GetState() == LF2States.Lying) break;
                    var objFrame = obj.Frame?.D;
                    if (objFrame != null && objFrame.hit_Fa == 14) break;
                    double zDiff = obj.PS.z - PS.z;
                    if (System.Math.Abs(zDiff) > 2) break;
                    if (obj.FrameCache?.Wrapper == FrameCache?.Wrapper) break;
                    if (savedWrapper != null && obj.FrameCache?.Wrapper == savedWrapper) break;
                    return true;
                }
            }

            var candidates = new System.Collections.Generic.List<LF2LivingObject>(16);
            Match?.GetAllLivingObjects(candidates);

            int bestIndex = -1;
            double bestDist = 10000f;

            for (int i = 0; i < candidates.Count; i++)
            {
                var obj = candidates[i];
                if (obj == null || obj.PS == null) continue;
                if (ReferenceEquals(obj, this)) continue;
                if (obj.Dead) continue;
                if (obj.Health == null || obj.Health.HP <= 0) continue;
                if (IsWeaponEntity(obj)) continue;
                if (obj.GetState() == LF2States.Lying) continue;
                if (obj.FrameCache?.Wrapper == FrameCache?.Wrapper) continue;
                if (savedWrapper != null && obj.FrameCache?.Wrapper == savedWrapper) continue;
                var objFrame = obj.Frame?.D;
                if (objFrame != null && objFrame.hit_Fa == 14) continue;
                double zDiff = obj.PS.z - PS.z;
                if (System.Math.Abs(zDiff) > 2) continue;

                double dist = System.Math.Abs(obj.PS.x - PS.x) + System.Math.Abs(zDiff);
                if (dist < bestDist)
                {
                    bestDist = dist;
                    bestIndex = obj.StableId;
                }
            }

            if (bestIndex < 0)
                return false;

            OwnerEntityIndex = bestIndex;
            return true;
        }

        // ========== hit_Fa=3 per-frame 追踪逻辑 ==========
        private void ApplyHitFa3Tracking()
        {
            LF2LivingObject target = FindOwnerTarget();
            if (target == null) return;

            // x 方向追踪: dbl_443288=0.7
            if (target.PS.x > PS.x)
                PS.vx += 0.7f;
            else if (target.PS.x < PS.x)
                PS.vx -= 0.7f;

            // z 方向追踪: ±10 死区, dbl_443228=0.17
            if (target.PS.z > PS.z + 10)
                PS.vz += 0.17f;
            else if (target.PS.z < PS.z - 10)
                PS.vz -= 0.17f;

            // vx 钳制到 ±16.0：dbl_443220=16.0，40300000h=16.0。
            if (PS.vx > 16.0f) PS.vx = 16.0f;
            else if (PS.vx < -16.0f) PS.vx = -16.0f;

            // vz 钳制到 ±2.4：dbl_443210=2.4，dbl_443208=-2.4。
            if (PS.vz > 2.4f) PS.vz = 2.4f;
            else if (PS.vz < -2.4f) PS.vz = -2.4f;

            // 根据 vx 更新朝向。
            SwitchDir(PS.vx >= 0 ? "right" : "left");
        }

        // ========== N-11: hit_Fa=11 per-frame 追踪逻辑 ==========
        private void ApplyHitFa11Tracking()
        {
            LF2LivingObject target = FindOwnerTarget();

            bool targetActive = target != null && !target.Dead;
            bool selfHpPositive = Health != null && Health.HP > 0;
            if (selfHpPositive && targetActive)
                return;

            if (PS.vx < 0)
                PS.vx -= 2.0f;
            else
                PS.vx += 2.0f;

            // vx 钳制到 ±17.0（dbl_443200=17.0，dbl_4431F8=-17.0）。
            if (PS.vx > 17.0f) PS.vx = 17.0f;
            else if (PS.vx < -17.0f) PS.vx = -17.0f;

            // 根据 vx 更新朝向。
            SwitchDir(PS.vx >= 0 ? "right" : "left");
        }

        // ========== N-13: hit_Fa=13 敌方追踪实体生成 ==========
        private void ApplyHitFa13Spawn()
        {
            var factory = LF2ObjectPointFactory.Instance;
            if (factory == null) return;

            var allObjects = new System.Collections.Generic.List<LF2LivingObject>(16);
            Match?.GetAllLivingObjects(allObjects);
            var enemies = new System.Collections.Generic.List<int>(8);
            for (int i = 0; i < allObjects.Count; i++)
            {
                var obj = allObjects[i];
                if (obj == null || obj.PS == null) continue;
                if (obj.Dead) continue;
                if (obj.Health != null && obj.Health.HP <= 0) continue;
                if (obj.Team == Team) continue;
                if (IsWeaponEntity(obj)) continue;
                enemies.Add(obj.StableId);
            }

            int targetStableId = (enemies.Count > 0)
                ? enemies[RandInt(0, enemies.Count)]
                : StableId;

            double spawnY = PS.y + RandInt(0, 7) - 3;
            int facing = PS.dir == "right" ? 0 : 1;
            var op = new ObjectPoint
            {
                oid = 228, kind = 0, action = 0,
                dvx = 0, dvy = 0, dvz = 0,
                facing = facing
            };
            var t620 = LF2ReferencePool.Instance.Fetch<OPointCreateTask>();
            t620.opoint = op; t620.parent = _parent; t620.team = Team;
            t620.pos = new Vector3((float)PS.x, (float)spawnY, (float)PS.z); t620.z = (float)PS.z; t620.dir = PS.dir; t620.dvz = 0;
            t620.useDirectVelocity = true; t620.directVx = (float)PS.vx; t620.directVy = (float)PS.vy; t620.directVz = (float)PS.vz;
            factory.EnqueueCreateObject(t620);

            // 注：OwnerEntityIndex 通过 OPointCreateTask.ownerEntityIndex 传递给工厂
            DieEvent();
        }

        // ========== hit_Fa=8: oid=225 散射追踪生成 ==========
        // 每次生成 oid=225，vx=rand(21)-11, vy=3.0-rand(24)*0.25, vz=3.0-rand(24)*0.25
        private void ApplyHitFa8Spawn()
        {
            var factory = LF2ObjectPointFactory.Instance;
            if (factory == null) return;

            var allObjects = new System.Collections.Generic.List<LF2LivingObject>(16);
            Match?.GetAllLivingObjects(allObjects);
            var enemies = new System.Collections.Generic.List<int>(8);
            for (int i = 0; i < allObjects.Count; i++)
            {
                var obj = allObjects[i];
                if (obj == null || obj.Dead) continue;
                if (obj.Health == null || obj.Health.HP <= 0) continue;
                if (obj.Team == Team) continue;
                if (IsWeaponEntity(obj)) continue;
                enemies.Add(obj.StableId);
            }

            // count = (enemyCount > 4) ? (enemyCount-3)/2+3 : 3
            int enemyCount = enemies.Count;
            int count = 3;
            if (enemyCount > 4)
                count = (enemyCount - 3) / 2 + 3;

            int facing = PS.dir == "right" ? 0 : 1;
            for (int i = 0; i < count; i++)
            {
                // vx = rand(21)-11, vy = 3.0-rand(24)*0.25, vz = 3.0-rand(24)*0.25
                double vx = RandInt(0, 21) - 11;
                double vy = 3.0f - RandInt(0, 24) * 0.25f;
                double vz = 3.0f - RandInt(0, 24) * 0.25f;

                // OwnerEntityIndex = 随机敌方 StableId（若无敌方则用自身）
                int ownerIdx = (enemyCount > 0)
                    ? enemies[RandInt(0, enemyCount)]
                    : StableId;

                var op = new ObjectPoint
                {
                    oid = 225, kind = 0, action = 0,
                    dvx = 0, dvy = 0, dvz = 0,
                    facing = facing
                };
                var t692 = LF2ReferencePool.Instance.Fetch<OPointCreateTask>();
                t692.opoint = op; t692.parent = _parent; t692.team = Team;
                t692.pos = new Vector3((float)PS.x, (float)PS.y, (float)PS.z); t692.z = (float)PS.z; t692.dir = PS.dir; t692.dvz = 0;
                t692.useDirectVelocity = true; t692.directVx = (float)vx; t692.directVy = (float)vy; t692.directVz = (float)vz;
                t692.ownerEntityIndex = ownerIdx;
                factory.EnqueueCreateObject(t692);
            }

            // 自身失活
            DieEvent();
        }

        // ========== hit_Fa=5: oid=219 友方治疗生成 ==========
        // vx=(ally.x-self.x)/50, vy=0, vz=0, dir=right
        private void ApplyHitFa5Spawn()
        {
            var factory = LF2ObjectPointFactory.Instance;
            if (factory == null) return;

            var allObjects = new System.Collections.Generic.List<LF2LivingObject>(16);
            Match?.GetAllLivingObjects(allObjects);
            var allies = new System.Collections.Generic.List<LF2LivingObject>(8);
            for (int i = 0; i < allObjects.Count; i++)
            {
                var obj = allObjects[i];
                if (obj == null || obj.Dead) continue;
                if (obj.Health == null || obj.Health.HP <= 0) continue;
                if (obj.Team != Team) continue;
                if (IsWeaponEntity(obj)) continue;
                allies.Add(obj);
            }

            for (int i = 0; i < allies.Count; i++)
            {
                var ally = allies[i];
                double vx = (ally.PS.x - PS.x) / 50.0f;

                var op = new ObjectPoint
                {
                    oid = 219, kind = 0, action = 0,
                    dvx = 0, dvy = 0, dvz = 0,
                    facing = 0
                };
                var t751 = LF2ReferencePool.Instance.Fetch<OPointCreateTask>();
                t751.opoint = op; t751.parent = _parent; t751.team = Team;
                t751.pos = new Vector3((float)PS.x, (float)PS.y, (float)PS.z); t751.z = (float)PS.z; t751.dir = "right"; t751.dvz = 0;
                t751.useDirectVelocity = true; t751.directVx = (float)vx; t751.directVy = 0f; t751.directVz = 0f;
                t751.ownerEntityIndex = ally.StableId;
                factory.EnqueueCreateObject(t751);
            }

            // 自身失活
            DieEvent();
        }

        // 分支 6：vx=(enemy.x-self.x)/50，vy=-(4+rand(4))。
        // 分支 9：oid=rand(2)+221，v221 上限为 4，v217 最大值为 10。
        // 分支 9：vx=rand(21)-11，vy=-2.0-rand(40)*0.1667。
        // OwnerEntityIndex 写入 enemy.StableId。
        private void ApplyHitFa6Or9Spawn(int hitFa)
        {
            var factory = LF2ObjectPointFactory.Instance;
            if (factory == null) return;

            int cap = (hitFa == 9) ? 4 : 0;
            int max = (hitFa == 9) ? 10 : 7;  // v217: case6=7, case9=10

            var allObjects = new System.Collections.Generic.List<LF2LivingObject>(16);
            Match?.GetAllLivingObjects(allObjects);

            int spawnCount = 0;
            for (int i = 0; i < allObjects.Count && spawnCount < max; i++)
            {
                var obj = allObjects[i];
                if (obj == null || obj.Dead) continue;
                if (obj.Health == null || obj.Health.HP <= 0) continue;
                if (obj.Team == Team) continue;
                if (IsWeaponEntity(obj)) continue;
                if (cap > 0 && spawnCount >= cap) break;

                int oid;
                double vx, vy;
                if (hitFa == 6)
                {
                    oid = 220;
                    vx = (obj.PS.x - PS.x) / 50.0f;
                    vy = -(4 + RandInt(0, 4));
                }
                else
                {
                    oid = RandInt(0, 2) + 221;
                    vx = RandInt(0, 21) - 11;
                    vy = -2.0f - RandInt(0, 40) * 0.1667f;
                }

                int facing = PS.dir == "right" ? 0 : 1;
                var op = new ObjectPoint
                {
                    oid = oid, kind = 0, action = 0,
                    dvx = 0, dvy = 0, dvz = 0,
                    facing = facing
                };
                var t798 = LF2ReferencePool.Instance.Fetch<OPointCreateTask>();
                t798.opoint = op; t798.parent = _parent; t798.team = Team;
                t798.pos = new Vector3((float)PS.x, (float)PS.y, (float)PS.z); t798.z = (float)PS.z; t798.dir = PS.dir; t798.dvz = 0;
                t798.useDirectVelocity = true; t798.directVx = (float)vx; t798.directVy = (float)vy; t798.directVz = 0f;
                t798.ownerEntityIndex = obj.StableId;
                factory.EnqueueCreateObject(t798);
                spawnCount++;
            }
        }

        // ========== hit_Fa=1 per-frame 追踪逻辑 ==========
        private void ApplyHitFa1Tracking()
        {
            LF2LivingObject target = FindOwnerTarget();
            if (target == null) return;

            if (target.PS.x > PS.x)
                PS.vx += 0.85f;
            else if (target.PS.x < PS.x)
                PS.vx -= 0.85f;

            // vz 使用 ±7 死区，每次调整 0.3。
            if (target.PS.z > PS.z + 7)
                PS.vz += 0.3f;
            else if (target.PS.z < PS.z - 7)
                PS.vz -= 0.3f;

            // vy *= 0.7142857
            PS.vy *= 0.7142857f;

            bool targetIsWeapon = IsWeaponEntity(target);
            if (!targetIsWeapon)
            {
                if (PS.y + 10.0f < target.PS.y)
                    PS.y += 1.2f;
                else if (PS.y + 10.0f > target.PS.y)
                    PS.y -= 1.2f;
            }
            else
            {
                if (PS.y > 0)
                    PS.y += 1.0f;
            }

            // vx 钳制到 ±13.0。
            if (PS.vx > 13.0f) PS.vx = 13.0f;
            else if (PS.vx < -13.0f) PS.vx = -13.0f;

            // vz 钳制到 ±2.0。
            if (PS.vz > 2.0f) PS.vz = 2.0f;
            else if (PS.vz < -2.0f) PS.vz = -2.0f;

            if (PS.y > 1.0f) PS.y = 1.0f;
            else if (PS.y < -1.0f) PS.y = -1.0f;

            // 根据 vx 更新朝向。
            SwitchDir(PS.vx >= 0 ? "right" : "left");
        }

        // ========== hit_Fa=2/4/7/12/14 per-frame 追踪逻辑 ==========
        private void ApplyHitFa2_14Tracking(int hitFa)
        {
            LF2LivingObject target = FindOwnerTarget();
            if (Health == null || Health.HP <= 0) return;
            bool targetActive = target != null && !target.Dead;
            if (!targetActive) return;

            if (target.PS.x > PS.x)
                PS.vx += 0.7f;
            else if (target.PS.x < PS.x)
                PS.vx -= 0.7f;

            // 分支 7：vx 调整量翻倍。
            if (hitFa == 7)
            {
                if (target.PS.x > PS.x)
                    PS.vx += 0.7f;
                else if (target.PS.x < PS.x)
                    PS.vx -= 0.7f;
            }

            // vz 使用 ±5 死区，每次调整 0.4。
            if (target.PS.z > PS.z + 5)
                PS.vz += 0.4f;
            else if (target.PS.z < PS.z - 5)
                PS.vz -= 0.4f;

            // 分支 2/4/12/14：vy *= 0.7142857。
            if (hitFa == 2 || hitFa == 4 || hitFa == 12 || hitFa == 14)
                PS.vy *= 0.7142857f;

            bool targetIsWeapon = IsWeaponEntity(target);
            if (!targetIsWeapon)
            {
                if (PS.y + 40.0f < target.PS.y)
                    PS.y += 1.0f;
                else if (PS.y + 40.0f > target.PS.y)
                    PS.y -= 1.0f;
            }
            else
            {
                if (PS.y > 0)
                    PS.y += 1.0f;
            }

            if (hitFa == 7)
            {
                if (PS.vy < 4.0f)
                    PS.vy += 0.4f;
                PS.y += PS.vy;
                if (PS.y > -25)
                {
                    SetFrameDirect(60);
                    PS.vx = 0; PS.vy = 0; PS.vz = 0;
                    var ownerTarget = FindOwnerTarget();
                    if (ownerTarget != null) ownerTarget.HealTimer = 100;
                    return;
                }
            }

            // vx 钳制到 ±14.0。
            if (PS.vx > 14.0f) PS.vx = 14.0f;
            else if (PS.vx < -14.0f) PS.vx = -14.0f;

            // y 上限钳制（只有上限，无下限）
            if (PS.y > 1.4f) PS.y = 1.4f;

            // 分支 14：vz 钳制到 ±1.5。
            // [ecx+50h/54h] 对应 double 类型的 vz。
            if (hitFa == 14)
            {
                if (PS.vz > 1.5f) PS.vz = 1.5f;
                else if (PS.vz < -1.5f) PS.vz = -1.5f;
            }
            else
            {
                // 分支 2/4/12：vz 二次钳制到 ±2.2。
                if (hitFa == 2 || hitFa == 4 || hitFa == 12)
                {
                    if (PS.vz > 2.2f) PS.vz = 2.2f;
                    else if (PS.vz < -2.2f) PS.vz = -2.2f;
                }
            }

            // 根据 vx 更新朝向。
            SwitchDir(PS.vx > 0 ? "right" : "left");

            if (hitFa == 2)
            {
                double absVx = System.Math.Abs(PS.vx);
                int curFrame = Frame?.N ?? -1;
                if (absVx > 14.0f)
                {
                    if (curFrame != 5 && curFrame != 6)
                        SetFrameDirect(5);
                }
                else if (absVx > 7.0f)
                {
                    if (curFrame != 3 && curFrame != 4)
                        SetFrameDirect(3);
                }
            }
        }

        private LF2LivingObject FindOwnerTarget()
        {
            if (OwnerEntityIndex < 0) return null;
            var allObjects = new System.Collections.Generic.List<LF2LivingObject>(16);
            Match?.GetAllLivingObjects(allObjects);
            for (int i = 0; i < allObjects.Count; i++)
            {
                if (allObjects[i].StableId == OwnerEntityIndex)
                    return allObjects[i];
            }
            return null;
        }

        // ========== sub_402C00 等价：直接速度生成实体 ==========
        private void SpawnEntityDirect(int oid, int frameId, double x, double y, double z,
            double vx, double vy, double vz, int facing)
        {
            var factory = LF2ObjectPointFactory.Instance;
            if (factory == null) return;

            string dir = facing == 0 ? "right" : "left";
            var op = new ObjectPoint
            {
                oid = oid, kind = 0, action = frameId,
                dvx = 0, dvy = 0, dvz = 0,
                facing = facing
            };
            var t1047 = LF2ReferencePool.Instance.Fetch<OPointCreateTask>();
            t1047.opoint = op; t1047.parent = _parent; t1047.team = Team;
            t1047.pos = new Vector3((float)x, (float)y, (float)z); t1047.z = (float)z; t1047.dir = dir; t1047.dvz = 0;
            t1047.useDirectVelocity = true; t1047.directVx = (float)vx; t1047.directVy = (float)vy; t1047.directVz = (float)vz;
            factory.EnqueueCreateObject(t1047);
        }

        #endregion

        public override void Reset()
        {
            _parent = null;
            Runtime.Reset();
            ObjectId = 0;
            Team = 0;
            Health.HP = 0;
            _lastState = -1;
            _hitFaFired = false;
            NoBounce = false;
            ShotCount = 0;
            ResetSpark();
            ResetStableId();
        }

        public override void Destroy()
        {
            CreateBrokenEffect();
        }

        // ========== ISimObject 生命周期 ==========

        public override void OnFrameTransit(int targetFrameId, bool switchDirAfterTrans, int oldLock)
        {
            Frame.PN = Frame.N;
            Frame.N = targetFrameId;

            LF2FrameData targetFrame = FrameCache.GetFrameDataById(targetFrameId);
            if (targetFrame == null) return;

            bool isStateTrans = Frame.D?.state != targetFrame.state;
            if (isStateTrans)
                StateExitEvent();

            Frame.D = targetFrame;

            if (isStateTrans)
            {
                AttackingCounter = 0;
                StateEntryEvent();
                _lastState = Frame.D.state;
            }

            Trans.SyncDirectFrameData(Frame.D.wait, Frame.D.next);
            FrameEvent();

            if (!string.IsNullOrEmpty(Frame.D.sound))
                PlaySound(Frame.D.sound);
        }

        /// <summary>
        /// Transit 阶段：推进当前帧请求，语义对齐 C++ release 的 frame/wait/next 字段。
        /// </summary>
        public override void SimTransit(int tickIndex)
        {
            Trans?.Trans();
        }

        /// <summary>
        /// EntityCollision 阶段：处理攻击豁免递减和技能对象碰撞副作用。
        /// </summary>
        public override void SimEntityCollision(int tickIndex)
        {
            var fD = Frame?.D;

            if (AttackExempt > 0) AttackExempt--;

            if (GrabbedBy < 0) return;

            if (fD != null && fD.cpoint != null && fD.cpoint.kind == 2) return;

            if (ShakeTimer > 0) ShakeTimer--;
            else if (ShakeTimer < 0) ShakeTimer++;

            if (fD != null && fD.hit_Uj < 0 && NTSDGlobal.MPEnabled && Health != null)
            {
                if (Health.PP < fD.hit_Uj)
                    SetFrameDirect(fD.hit_a);
                else
                    Health.PP += fD.hit_Uj;
            }

            if (Frame?.N == 202)
                ShakeTimer = 20;

            if (fD != null && fD.hit_a > 0 && Health != null)
            {
                Health.HP -= fD.hit_a;
                if (Health.HP <= 0)
                {
                    Health.HP = 0;
                    SetFrameDirect(fD.hit_d);
                }
            }
        }

        /// <summary>
        /// 直接写入当前帧，用于 C++ release Entity_Collision / frame_tick 中不触发帧事件的跳帧。
        /// </summary>
        private void SetFrameDirect(int frameId, int waitCounter = int.MinValue)
        {
            Frame.N = frameId;
            Frame.D = FrameCache.GetFrameDataById(frameId);
            AttackingCounter = 0;
            if (Frame.D != null && Trans != null)
            {
                Trans.SyncDirectFrameData(Frame.D.wait, Frame.D.next, waitCounter);
            }
        }

        /// <summary>
        /// TU 阶段：执行技能对象的逐 tick 逻辑、物理和状态分支。
        /// </summary>
        public override void SimTU(int tickIndex)
        {
            int currentState = GetState();

            if (currentState != _lastState)
            {
                _hitFaFired = false;
                StateEntryEvent();
                _lastState = currentState;
            }

            TUEvent();

            if (Health.HP <= 0)
            {
                DieEvent();
            }
        }

        internal override bool SupportsObjectInteractionPhase() => true;

        public override void SimObjectInteraction(int tickIndex)
        {
            Interaction();
        }

        // ========== 交互方法 ==========

        /// <summary>
        /// 技能对象命中检测入口，对齐 C++ release 的 Entity_AI_Update 技能/投射物交互流程。
        /// </summary>
        public void Interaction()
        {
            if (Team == 0) return;

            var frame = Frame?.D;
            var sceneQuery = Match?.SceneQuery;
            var kindService = Match?.ItrKindService;
            if (frame == null || sceneQuery == null) return;
            if (PS == null) return;

            var itrs = frame.itrs;
            if (itrs == null || itrs.Count == 0) return;

            float spriteWidthPx = GetSpriteWidthPxForCollision();
            if (spriteWidthPx <= 0f) return;

            var itrVolumes = PS.GetItrVolumes(itrs, frame.centerx, frame.centery, spriteWidthPx, itrZWidthPx: 0f);
            int count = Mathf.Min(itrs.Count, itrVolumes.Count);

            for (int i = 0; i < count; i++)
            {
                var itr = itrs[i];
                if (itr == null) continue;

                var candidates = sceneQuery.QueryBodyHits(itrVolumes[i], this);
                if (candidates == null || candidates.Count == 0) continue;

                for (int c = 0; c < candidates.Count; c++)
                {
                    SceneQueryHit candidate = candidates[c];
                    LF2Entity target = candidate.Target;
                    InteractionArea runtimeItr = BruteForceSceneQuery.ResolveRuntimeItrForPair(
                        this,
                        target,
                        frame,
                        itr,
                        out bool zeroAttackerHpOnConsume,
                        out bool releaseHeavyHeldTargetOnConsume);
                    if (runtimeItr == null) continue;

                    var hitInfo = new SceneQueryHit(
                        target,
                        candidate.BodyX,
                        i,
                        runtimeItr,
                        zeroAttackerHpOnConsume,
                        releaseHeavyHeldTargetOnConsume);

                    if (!CanInteractTarget(runtimeItr, target)) continue;
                    ApplyReleaseSceneQueryConsumeEffects(hitInfo);
                    if (!DispatchInteractionByKind(kindService, runtimeItr, target)) continue;

                    ItrArestUpdate(runtimeItr);
                    int selfSlot = Runtime?.SlotIndex ?? -1;
                    if (selfSlot >= 0 && target.ItrVrestTest(selfSlot))
                        target.ItrVrestUpdate(selfSlot, runtimeItr);

                    if (GetState() == LF2States.ProjectileTeleport)
                    {
                        int vrest = runtimeItr.vrest > 0 ? runtimeItr.vrest : NTSDGlobal.Default.Weapon.VRest;
                        ItrVrestUpdate(target.StableId, new InteractionArea { vrest = vrest });
                    }

                    return;
                }
            }
        }

        private bool CanInteractTarget(InteractionArea itr, LF2Entity target)
        {
            if (itr == null || target == null) return false;
            if (target == this) return false;
            if (target.PS == null || target.Frame?.D == null) return false;
            if (target.Health != null && target.Health.HP <= 0) return false;
            if (Team != 0 && target.Team != 0 && Team == target.Team) return false;
            int selfSlot = Runtime?.SlotIndex ?? -1;
            if (selfSlot >= 0 && !target.ItrVrestTest(selfSlot)) return false;
            var kindService = Match?.ItrKindService;
            if (itr.kind != 8 && itr.kind != 14)
            {
                if (target is not LF2LivingObject) return false;
            }

            return true;
        }

        private bool DispatchInteractionByKind(INTSDItrKindService kindService, InteractionArea itr, LF2Entity target)
        {
            if (itr.kind == 8)
            {
                if (target is not LF2Character) return false;
                if (DeferState3005Kind8LeadIn()) return false;
                return TryApplyHit(itr, target);
            }

            if (kindService != null && kindService.IsAttackKind(itr.kind))
            {
                return TryApplyHit(itr, target);
            }

            switch (itr.kind)
            {
                default:
                    return false;
            }
        }

        private bool DeferState3005Kind8LeadIn()
        {
            var activeFrame = Frame?.D;
            if (activeFrame == null || activeFrame.state != LF2States.ObjectFlying)
            {
                return false;
            }

            // C++ release defer_state3005_kind8_lead_in：
            // state=3005 且当前/下一帧带 hit_Fa 或 opoint 时，延后 kind=8 命中。
            if (activeFrame.hit_Fa > 0 || (activeFrame.opoints != null && activeFrame.opoints.Count > 0))
            {
                return true;
            }

            if (activeFrame.next <= 0 || activeFrame.next == Frame.N)
            {
                return false;
            }

            var nextFrame = GetFrameDataById(activeFrame.next);
            return nextFrame != null
                && (nextFrame.hit_Fa > 0 || (nextFrame.opoints != null && nextFrame.opoints.Count > 0));
        }

        private bool TryApplyHit(InteractionArea itr, LF2Entity target)
        {
            if (!ItrArestTest()) return false;

            if (target is LF2WeaponBase weapon)
            {
                return weapon.Hit(itr, this);
            }

            if (target is LF2SpecialAttack specialAttack)
            {
                return specialAttack.Hit(itr, this);
            }

            if (target is LF2Character character)
            {
                if (PS != null)
                {
                    var attackerPos = new Vector3((float)PS.x, (float)PS.y, (float)PS.z);
                    return character.Hit(itr, this, attackerPos, default);
                }
            }

            return false;
        }

        /// <summary>
        /// 技能对象被命中入口，对齐 C++ release 的 Entity_AI_Update hurt pipeline。
        /// </summary>
        public bool Hit(InteractionArea itr, LF2Entity attacker)
        {
            if (itr.kind == 9)
            {
                var charData = CharacterAnimtorManager.Instance?.GetCharacterData(ObjectId);
                if (!string.IsNullOrEmpty(charData?.weapon_broken_sound))
                    PlaySound(charData.weapon_broken_sound);

                attacker.FrameDelay = -3;

                int curState = GetState();
                if (curState == LF2States.ObjectFlying) // 3005
                {
                    NoBounce = true;
                    SetFrameDirect(40);
                }
                else
                {
                    // 0x0042ED50: victim.owner_slot = attacker.owner_slot
                    var attackerSpecial = attacker as LF2SpecialAttack;
                    if (attackerSpecial?.FrameCache?.Wrapper != null)
                        FrameCache.Load(attackerSpecial.FrameCache.Wrapper);
                    Team = attacker.Team;
                    OwnerEntityIndex = attacker.OwnerEntityIndex >= 0 ? attacker.OwnerEntityIndex : attacker.StableId;
                    NoBounce = true;
                    SetFrameDirect(30);
                    PS.vx = 0f; PS.vy = 0f; PS.vz = 0f;
                }
                return true;
            }

            int state = GetState();
            bool result = false;

            switch (state)
            {
                case LF2States.ProjectileFlying:
                    result = Hit_State3000(attacker, itr);
                    break;
                case LF2States.ObjectExpanding:
                    result = Hit_State3006(attacker, itr);
                    break;
            }

            if (result)
                ApplyPostHitSelfDestruct(attacker);

            return result;
        }

        /// <summary>
        /// entity_type==0 对应 attacker 是非武器（角色）目标
        /// </summary>
        private void ApplyPostHitSelfDestruct(LF2Entity attacker)
        {
            // attacker 的 entity_type：武器类型为 1/2/3/4/6，角色为 0。
            bool attackerIsChar = attacker is LF2Character;
            if (!attackerIsChar) return;

            if (ObjectId == 201)
                DieEvent();
            else if (ObjectId == 214)
                Health.HP = 0;
        }

        private bool Hit_State3000(LF2Entity attacker, InteractionArea itr)
        {
            var frame = Frame.D;

            if (itr.kind == 14)
            {
                Trans.SetWait(0, 20);
                return true;
            }

            if (attacker != null)
            {
                if (Team == attacker.Team && PS.dir == attacker.PS?.dir)
                {
                    return false;
                }
            }

            var frameItr = GetFirstItr(frame);
            if (frameItr != null && frameItr.effect == 3)
            {
                var attackerSA = attacker as LF2SpecialAttack;
                if (attackerSA != null && attackerSA.GetState() == LF2States.ProjectileFlying &&
                    itr.effect != 3 && itr.effect != 2)
                {
                    return true;
                }
            }

            var attackerSpecial = attacker as LF2SpecialAttack;
            if (attackerSpecial != null)
            {
                if (frameItr != null && frameItr.effect != 3 && frameItr.effect != 2 && itr.effect == 3)
                {
                    PS.vx = 0;
                    // 忍偶系变身逻辑
                    int selfOid = ObjectId;
                    bool isValidTarget = selfOid == 200 || selfOid == 203 || selfOid == 205
                        || selfOid == 206 || selfOid == 207 || selfOid == 215 || selfOid == 216;

                    if (attackerSpecial.ObjectId == 209 && isValidTarget)
                    {
                        // 0x0042DC73: target.data=attacker.data
                        // 0x0042DC80: target.[+70h/74h/78h]=40
                        var karasuWrapper = CharacterAnimtorManager.Instance?.GetCharacterConfig(209);
                        if (karasuWrapper != null)
                        {
                            Team = attackerSpecial.Team;
                            OwnerId = attackerSpecial.OwnerId;
                            FrameCache.Load(karasuWrapper);
                            SetFrameDirect(40, 40);
                            Frame.PN = 40;
                        }
                        return true;
                    }

                    if (attackerSpecial.ObjectId == 213 && isValidTarget)
                    {
                        // target.team 和 target.[+354h] 继承 attacker.TrackerParent 的对应值。
                        var karasuWrapper = CharacterAnimtorManager.Instance?.GetCharacterConfig(209);
                        if (karasuWrapper != null)
                        {
                            int savedFrame = Frame.N;
                            FrameCache.Load(karasuWrapper);
                            SetFrameDirect(savedFrame);
                            Frame.PN = savedFrame;
                        }
                        var parent = attackerSpecial.TrackerParent as LF2SpecialAttack;
                        if (parent != null)
                        {
                            Team = parent.Team;
                            OwnerId = parent.OwnerId;
                        }
                        else
                        {
                            Team = attackerSpecial.Team;
                            OwnerId = attackerSpecial.OwnerId;
                        }
                        return true;
                    }

                    SetFrameDirect(1000);
                    CreateObjectAt(209, attackerSpecial);
                    return true;
                }

                if (itr.kind == 0)
                {
                    PS.vx = 0;
                    SetFrameDirect(20);
                    return true;
                }
            }

            if (itr.kind == 0)
            {
                PS.vx = 0;
                Team = attacker?.Team ?? 0;
                SetFrameDirect(30);
                return true;
            }

            return false;
        }

        private bool Hit_State3006(LF2Entity attacker, InteractionArea itr)
        {
            var attackerSA = attacker as LF2SpecialAttack;
            if (attackerSA != null)
            {
                int attackerState = attackerSA.GetState();

                if (attackerState == LF2States.ObjectFlying || attackerState == LF2States.ObjectExpanding)
                {
                    SetFrameDirect(20);
                    PS.vx = 0;
                    PS.vz = 0;
                    return true;
                }

                if (attackerState == LF2States.ProjectileFlying)
                {
                    PS.vx = (PS.vx > 0 ? -1 : 1) * 7;
                    return true;
                }
            }

            if (itr.kind == 0)
            {
                PS.vx = (PS.vx > 0 ? -1 : 1) * 1;
                if (itr.bdefend > NTSDGlobal.Gameplay.DefendBreakLimit)
                {
                    Health.HP = 0;
                }
                return true;
            }

            return false;
        }

        private static InteractionArea GetFirstItr(LF2FrameData frame)
        {
            if (frame?.itrs == null || frame.itrs.Count == 0) return null;
            return frame.itrs[0];
        }

        // ========== 辅助方法 ==========

        public void CreateBrokenEffect()
        {
            // C++ release 中 broken/effect 生成由 GameMode 层处理；这里先保留统一入口。
            BrokenEffectCreate(ObjectId);
        }

        public void CreateObject(ObjectPoint op)
        {
            if (op.oid <= 0) return;
            var task = LF2ReferencePool.Instance.Fetch<OPointCreateTask>();
            task.opoint = op;
            task.parent = this;
            task.team = Team;
            task.pos = MakeObjectPoint(op);
            task.z = (float)PS.z;
            task.dir = PS.dir;
            task.dvz = 0;
            LF2ObjectPointFactory.Instance?.EnqueueCreateObject(task);
        }

        public void CreateObjectAt(int oid, LF2SpecialAttack source)
        {
            var op = new ObjectPoint { oid = oid, action = 0, facing = 0 };
            var task = LF2ReferencePool.Instance.Fetch<OPointCreateTask>();
            task.opoint = op;
            task.parent = source;
            task.team = source?.Team ?? 0;
            task.pos = new Vector3((float)(source?.PS?.x ?? 0), (float)(source?.PS?.y ?? 0), (float)(source?.PS?.z ?? 0));
            task.z = (float)(source?.PS?.z ?? 0);
            task.dir = source?.PS?.dir ?? "right";
            task.dvz = 0;
            LF2ObjectPointFactory.Instance?.EnqueueCreateObject(task);
        }

        private Vector3 MakeObjectPoint(ObjectPoint op)
        {
            var frame = Frame?.D;
            if (PS == null || frame == null)
                return new Vector3((float)(PS?.x ?? 0f), (float)(PS?.y ?? 0f), (float)(PS?.z ?? 0f));

            double x = PS.dir == "right"
                ? PS.x - frame.centerx + op.x
                : PS.x + frame.centerx - op.x;
            double y = PS.y + PS.z - frame.centery + op.y;
            double z = PS.z + op.y;
            return new Vector3((float)x, (float)y, (float)z);
        }

        public void PlaySound(string soundId)
        {
            if (string.IsNullOrEmpty(soundId)) return;
            AppManager.Instance?.SoundPlayer?.PlaySfx(soundId);
        }

        // ========== 初始化子步骤 ==========

        private void InitializeParent(OPointCreateTask task)
        {
            _parent = task.parent as LF2LivingObject;
            ObjectId = task.opoint.oid;
            Team = task.team;
        }

        private void InitializePosition(OPointCreateTask task)
        {
            PS.x = task.pos.x;
            PS.y = task.parent != null ? task.pos.y - task.z : task.pos.y;
            PS.z = task.z;
        }

        private void InitializeDirection(OPointCreateTask task)
        {
            string dir = CalculateDirection(task.opoint.facing, task.dir);
            SwitchDir(dir);
        }

        private void InitializeFrame(OPointCreateTask task)
        {
            var wrapper = CharacterAnimtorManager.Instance?.GetCharacterConfig(ObjectId);
            FrameCache.Load(wrapper);
            int action = task.opoint.action;
            if (action == 0 && FrameCache.GetFrameDataById(0) == null)
                action = 999;
            Frame.D = FrameCache.GetFrameDataById(action);
            SetFrameDirect(action);
        }

        private void InitializeVelocity(OPointCreateTask task)
        {
            if (task.useDirectVelocity)
            {
                PS.vx = task.directVx;
                PS.vy = task.directVy;
                PS.vz = task.directVz;
                return;
            }

            PS.vx = Dirh() * task.opoint.dvx;
            PS.vy = task.opoint.dvy;

            bool hasFrameDvx = (Frame.D != null && Frame.D.dvx != 0);
            PS.vz = hasFrameDvx ? task.dvz : 0f;
        }

        private void InitializeHealth()
        {
            Health.HP = NTSDGlobal.Default.Health.HpFull;
        }

        private Vector3 MakePointCenter(LF2FrameData frame)
        {
            float spriteWidth = Sprite?.GetWidthPx() ?? 0;

            int centerx = frame?.centerx ?? 0;
            int centery = frame?.centery ?? 0;

            float x = (PS.dir == "right")
                ? PS.sx + centerx
                : PS.sx + spriteWidth - centerx;

            float y = PS.sy + centery;
            float z = PS.sz + centery;

            return new Vector3(x, y, z);
        }

        private void CoincideXYForInit(Vector3 targetPos, Vector3 selfPoint)
        {
            float vx = targetPos.x - selfPoint.x;
            float vz = targetPos.z - selfPoint.z;
            PS.x += vx;
            PS.z += vz;
        }
    }
}
