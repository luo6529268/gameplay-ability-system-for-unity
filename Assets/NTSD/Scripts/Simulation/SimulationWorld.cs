using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Animation.LF2Tasks;
using NTSD.Extensions;
using NTSD.LevelEditor;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NTSD.Simulation
{
    /// <summary>
    /// NTSD 鎴樻枟瀵硅薄鐨勭‘瀹氭€фā鎷熻皟搴﹀櫒锛屾寜 SimOrder 鍜?StableId 淇濊瘉姣忓抚鎵ц椤哄簭绋冲畾銆?
    /// </summary>
    public class SimulationWorld
    {

        /// <summary>鍚屼竴 SimOrder 鐨勫璞℃《锛涘彧鏈夋《鍐呭鍙樺寲鍚庢墠寤惰繜閲嶆柊鎺掑簭銆?/summary>
        private class Bucket
        {
            public List<ISimObject> items = new List<ISimObject>();

            public bool dirty = false;

            public void EnsureSorted(System.Func<ISimObject, int> stableIdSelector)
            {
                if (dirty)
                {
                    items = items.OrderBy(stableIdSelector).ToList();
                    dirty = false;
                }
            }
        }

        /// <summary>鎸?SimOrder 寤虹珛鐨勬ā鎷熸《锛汼ortedDictionary 淇濊瘉 pass 椤哄簭銆?/summary>
        private SortedDictionary<int, Bucket> _buckets = new SortedDictionary<int, Bucket>();

        /// <summary>娉ㄥ唽瀵硅薄鏃舵敞鍏ョ殑妯℃嫙涓婁笅鏂囥€?/summary>
        private SimContext _context;

        /// <summary>缁欐病鏈夋樉寮忚繍琛屾椂 ID 鐨勫璞¤嚜鍔ㄥ垎閰?StableId銆?/summary>
        private int _nextAutoStableId = 100;

        /// <summary>閬嶅巻妗跺揩鐓ф湡闂村欢杩熷鐞嗙殑娉ㄩ攢璇锋眰銆?/summary>
        private readonly List<ISimObject> _pendingUnregister = new List<ISimObject>();

        /// <summary>涓栫晫姝ｅ湪閬嶅巻妯℃嫙瀵硅薄鏃朵负 true銆?/summary>
        private bool _ticking = false;

        private readonly List<LF2Entity> _entityScratch = new List<LF2Entity>(128);

        private int GetRuntimeStableId(ISimObject obj)
        {
            return obj is LF2Entity entity ? entity.Runtime.StableId : obj.StableId;
        }

        private void RefreshRuntimeSnapshot(ISimObject obj)
        {
            if (obj is LF2Entity entity)
                entity.RefreshRuntimeSnapshot();
        }

        private List<int> GetBucketKeySnapshot()
        {
            return _buckets.Count > 0 ? new List<int>(_buckets.Keys) : null;
        }

        public ILF2SceneQuery SceneQuery { get; private set; }

        public INTSDItrKindService ItrKindService { get; private set; }

        /// <summary>瀵归綈姝ｅ紡鐗?ntsd_rand() 琛屼负鐨勭‘瀹氭€ч殢鏈烘暟鐢熸垚鍣ㄣ€?/summary>
        public DeterministicRng Rng { get; private set; }

        public SimulationWorld()
        {
            _context = new SimContext(this);
            ItrKindService = new NTSDItrKindService();
            SceneQuery = new BruteForceSceneQuery(this);

            // C++ 姝ｅ紡鐗堥€氳繃 ntsd_srand() 鍒濆鍖栧叏灞€鎴樻枟闅忔満鏁般€?
            // 褰撳墠鎴樻枟鍦烘櫙浣跨敤鍥哄畾绉嶅瓙锛屼繚璇佸悓杈撳叆涓嬬粨鏋滃彲澶嶇幇銆?
            Rng = new DeterministicRng(0x4E545344u);
        }

        /// <summary>灏嗗璞℃敞鍐屽埌瀵瑰簲 SimOrder 妗讹紝骞惰皟鐢?OnAdded 鐢熷懡鍛ㄦ湡閽╁瓙銆?/summary>
        public void Register(ISimObject obj)
        {
            if (obj == null)
            {
                Debug.LogError("[SimulationWorld] Cannot register null object");
                return;
            }

            int simOrder = obj.SimOrder;
            if (!_buckets.TryGetValue(simOrder, out Bucket bucket))
            {
                bucket = new Bucket();
                _buckets[simOrder] = bucket;
            }

            if (bucket.items.Contains(obj))
            {
                Debug.LogWarning($"[SimulationWorld] Object already registered: SimOrder={simOrder}, StableId={obj.StableId}");
                return;
            }

            bucket.items.Add(obj);
            if (obj is LF2Entity registeredEntity)
            {
                registeredEntity.SetRuntimeSlotIndex(obj.StableId);
                registeredEntity.RefreshRuntimeSnapshot();
            }
            bucket.dirty = true;

            obj.OnAdded(_context);

            Debug.Log($"[SimulationWorld] Registered: SimOrder={simOrder}, StableId={obj.StableId}, Type={obj.GetType().Name}");
        }

        /// <summary>娉ㄩ攢瀵硅薄锛涘鏋滃綋鍓嶆鍦?tick 閬嶅巻锛屽垯寤惰繜鍒版湰杞?pass 缁撴潫鍚庣Щ闄ゃ€?/summary>
        public void Unregister(ISimObject obj)
        {
            if (obj == null)
            {
                Debug.LogError("[SimulationWorld] Cannot unregister null object");
                return;
            }

            if (_ticking)
            {
                if (!_pendingUnregister.Contains(obj))
                    _pendingUnregister.Add(obj);
                return;
            }

            UnregisterImmediate(obj);
        }

        /// <summary>浠庢《涓珛鍗崇Щ闄ゅ璞★紝骞惰皟鐢?OnRemoved 鐢熷懡鍛ㄦ湡閽╁瓙銆?/summary>
        private void UnregisterImmediate(ISimObject obj)
        {
            int simOrder = obj.SimOrder;
            if (!_buckets.TryGetValue(simOrder, out Bucket bucket))
            {
                Debug.LogWarning($"[SimulationWorld] Object not found in buckets: SimOrder={simOrder}");
                return;
            }

            bool removed = bucket.items.Remove(obj);
            if (!removed)
            {
                Debug.LogWarning($"[SimulationWorld] Object not found in bucket: SimOrder={simOrder}, StableId={obj.StableId}");
                return;
            }

            bucket.dirty = true;
            obj.OnRemoved(_context);

            if (bucket.items.Count == 0)
                _buckets.Remove(simOrder);

            Debug.Log($"[SimulationWorld] Unregistered: SimOrder={simOrder}, StableId={obj.StableId}, Type={obj.GetType().Name}");
        }

        private void FlushPendingUnregister()
        {
            if (_pendingUnregister.Count == 0) return;
            foreach (var obj in _pendingUnregister)
                UnregisterImmediate(obj);
            _pendingUnregister.Clear();
        }

        /// <summary>涓哄姩鎬佸垱寤哄璞″垎閰嶇‘瀹氭€х殑 StableId銆?/summary>
        public int AllocateStableId()
        {
            return _nextAutoStableId++;
        }

        /// <summary>
        /// 鎵ц閫愬璞′覆琛?tick锛歍ransit銆乷point 浠诲姟鍒锋柊銆乀U銆?
        /// opoint 鍒涘缓鍑虹殑瀵硅薄鍙互杩涘叆鍚屼竴甯у悗缁?pass銆?
        /// </summary>
        public void SerialTickAll(int tickIndex)
        {
            _ticking = true;
            try
            {
                var bucketKeys = new List<int>(_buckets.Keys);

                foreach (var key in bucketKeys)
                {
                    if (!_buckets.TryGetValue(key, out Bucket bucket)) continue;
                    bucket.EnsureSorted(GetRuntimeStableId);

                    var snapshot = bucket.items.Count > 0
                        ? new List<ISimObject>(bucket.items)
                        : null;

                    if (snapshot == null) continue;

                    foreach (var obj in snapshot)
                    {
                        if (obj == null) continue;
                        obj.SimTransit(tickIndex);
                        RefreshRuntimeSnapshot(obj);
                        obj.SimTU(tickIndex);
                        RefreshRuntimeSnapshot(obj);
                    }
                }
            }
            finally
            {
                _ticking = false;
                FlushPendingUnregister();
            }
        }

        /// <summary>
        /// FramePostProcess 涔嬪悗鐨勯€愬疄浣撳悗鍗婃鏇存柊銆?        /// 瀵归綈 C++ release run_late_entity_update 鐨勯亶鍘嗗舰鐘讹細姣忎釜瀹炰綋渚濇鎵ц鑷劧鎭㈠銆佸疄浣撶鎾炲拰鍚庢湡娓呯悊銆?        /// </summary>
        public void LateEntityUpdateAll(int tickIndex)
        {
            _ticking = true;
            try
            {
                var bucketKeys = GetBucketKeySnapshot();
                if (bucketKeys == null) return;

                foreach (int simOrder in bucketKeys)
                {
                    if (!_buckets.TryGetValue(simOrder, out Bucket bucket)) continue;
                    bucket.EnsureSorted(GetRuntimeStableId);

                    var snapshot = bucket.items.Count > 0
                        ? new List<ISimObject>(bucket.items)
                        : null;

                    if (snapshot == null) continue;

                    foreach (var obj in snapshot)
                    {
                        if (obj == null)
                        {
                            Debug.LogWarning($"[SimulationWorld] Null object in bucket SimOrder={simOrder}, skipping");
                            continue;
                        }

                        if (obj is LF2Character character)
                        {
                            character.RunLateSpecialPreCollision();
                            RefreshRuntimeSnapshot(character);

                            character.RegeneratePreCollisionStats(tickIndex);
                            RefreshRuntimeSnapshot(character);
                        }

                        obj.SimEntityCollision(tickIndex);
                        RefreshRuntimeSnapshot(obj);

                        var opointFactory = LF2ObjectPointFactory.Instance;
                        if (opointFactory != null && obj is LF2Entity entity)
                            opointFactory.ProcessOpointSpawn(entity);

                        // 保留非 DAT opoint 的队列路径，例如 hit_Fa、碎片和随机掉落等直接生成请求。
                        opointFactory?.FlushTasks();

                        obj.SimLateTick(tickIndex);
                        RefreshRuntimeSnapshot(obj);
                    }
                }
            }
            finally
            {
                _ticking = false;
                FlushPendingUnregister();
            }
        }

        /// <summary>
        /// C++ release AI_Process2：遍历 link_state&lt;0 的被持有对象，并按 holder 当前 wpoint 同步/释放。
        /// </summary>
        public void HeldObjectProcessAll(int tickIndex)
        {
            GetAllEntities(_entityScratch);
            if (_entityScratch.Count == 0) return;

            for (int i = 0; i < _entityScratch.Count; i++)
            {
                LF2Entity held = _entityScratch[i];
                if (held == null || held.Runtime.LinkState >= 0) continue;

                LF2Character holder = FindCharacterByStableId(held.Runtime.HolderStableId);
                if (holder == null || holder.Runtime.TargetSlotIndex != held.StableId)
                {
                    held.Runtime.LinkState = 0;
                    held.Runtime.HolderStableId = -1;
                    held.GrabbedBy = 0;
                    RefreshRuntimeSnapshot(held);
                    continue;
                }

                LF2FrameData holderFrame = holder.Frame?.D;
                WeaponPoint wpoint = holderFrame?.wpoints != null && holderFrame.wpoints.Count > 0
                    ? holderFrame.wpoints[0]
                    : null;
                if (wpoint == null)
                    continue;

                if (!holder.ReleaseHeldObjectByWPoint(held, wpoint, out var actResult))
                    continue;

                if (actResult.NeedsKind3Drop)
                {
                    var dropPoint = new WeaponPoint
                    {
                        kind = 3,
                        x = wpoint.x,
                        y = wpoint.y,
                        weaponact = wpoint.weaponact,
                        cover = wpoint.cover
                    };
                    holder.ReleaseHeldObjectByWPoint(held, dropPoint, out actResult);
                }

                var attackResult = actResult.AttackResult;
                if (attackResult != null && attackResult.HitUid != 0 && attackResult.ARest > 0)
                    holder.ItrRest.Arest = attackResult.ARest;

                RefreshRuntimeSnapshot(holder);
                RefreshRuntimeSnapshot(held);
            }

            _entityScratch.Clear();
        }

        private LF2Character FindCharacterByStableId(int stableId)
        {
            if (stableId < 0) return null;

            for (int i = 0; i < _entityScratch.Count; i++)
            {
                if (_entityScratch[i] is LF2Character character && character.StableId == stableId)
                    return character;
            }

            return null;
        }

        /// <summary>灏嗙疮璁″嚮閫€鍐欏叆 living object 閫熷害锛屽苟娓呯┖鏈抚鍑婚€€绱鍣ㄣ€?/summary>
        public void FramePostProcessAll()
        {
            var bucketKeys = GetBucketKeySnapshot();
            if (bucketKeys == null) return;

            foreach (int simOrder in bucketKeys)
            {
                if (!_buckets.TryGetValue(simOrder, out Bucket bucket)) continue;

                var snapshot = bucket.items.Count > 0
                    ? new List<ISimObject>(bucket.items)
                    : null;

                if (snapshot == null) continue;

                foreach (var obj in snapshot)
                {
                    if (obj is not LF2LivingObject living) continue;
                    if (living.FrameDelay != 0) continue;

                    if (living.HitCount > 0)
                    {
                        float denom = living.HitCount + 1;
                        living.PS.vx = living.KnockbackVx * 2f / denom;
                        living.PS.vy = living.KnockbackVy * 2f / denom;
                        living.PS.vz = living.KnockbackVz * 2f / denom;
                    }
                    living.KnockbackVx = 0f;
                    living.KnockbackVy = 0f;
                    living.KnockbackVz = 0f;
                    living.HitCount    = 0;
                }
            }
        }

        /// <summary>鍦ㄤ氦浜?pass 鍓嶆帹杩?vrest/arest 鍐峰嵈璁℃暟銆?/summary>
        public void VrestTickAll(int tickIndex)
        {
            var bucketKeys = GetBucketKeySnapshot();
            if (bucketKeys == null) return;

            foreach (int simOrder in bucketKeys)
            {
                if (!_buckets.TryGetValue(simOrder, out Bucket bucket)) continue;

                var snapshot = bucket.items.Count > 0
                    ? new List<ISimObject>(bucket.items)
                    : null;

                if (snapshot == null) continue;

                foreach (var obj in snapshot)
                {
                    if (obj is LF2LivingObject living)
                        living.ItrRest?.Tick();
                    RefreshRuntimeSnapshot(obj);
                }
            }
        }

        /// <summary>鎵ц鐢ㄤ簬鍛戒腑鍜屾敾鍑荤鎾為€昏緫鐨?post-interaction pass銆?/summary>
        public void PostInteractionTickAll(int tickIndex)
        {
            var bucketKeys = GetBucketKeySnapshot();
            if (bucketKeys == null) return;

            foreach (int simOrder in bucketKeys)
            {
                if (!_buckets.TryGetValue(simOrder, out Bucket bucket)) continue;

                var snapshot = bucket.items.Count > 0
                    ? new List<ISimObject>(bucket.items)
                    : null;

                if (snapshot == null) continue;

                foreach (var obj in snapshot)
                {
                    if (obj == null) continue;
                    obj.SimPostInteraction(tickIndex);
                    RefreshRuntimeSnapshot(obj);
                }
            }
        }

        /// <summary>鎵ц鐢ㄤ簬鎶撳彇銆佹嬀鍙栫瓑鏃╂湡妫€娴嬬殑 pre-interaction pass銆?/summary>
        public void PreInteractionTickAll(int tickIndex)
        {
            var bucketKeys = GetBucketKeySnapshot();
            if (bucketKeys == null) return;

            foreach (int simOrder in bucketKeys)
            {
                if (!_buckets.TryGetValue(simOrder, out Bucket bucket)) continue;

                var snapshot = bucket.items.Count > 0
                    ? new List<ISimObject>(bucket.items)
                    : null;

                if (snapshot == null) continue;

                foreach (var obj in snapshot)
                {
                    if (obj == null) continue;
                    obj.SimPreInteraction(tickIndex);
                    RefreshRuntimeSnapshot(obj);
                }
            }
        }

        /// <summary>鎸夌‘瀹氭€ф《椤哄簭鏀堕泦 living object銆?/summary>
        public void GetAllLivingObjects(List<LF2LivingObject> dst)
        {
            if (dst == null) return;
            dst.Clear();

            var bucketKeys = GetBucketKeySnapshot();
            if (bucketKeys == null) return;

            foreach (int simOrder in bucketKeys)
            {
                if (!_buckets.TryGetValue(simOrder, out Bucket bucket)) continue;
                bucket.EnsureSorted(GetRuntimeStableId);

                for (int i = 0; i < bucket.items.Count; i++)
                {
                    if (bucket.items[i] is LF2LivingObject living)
                    {
                        dst.Add(living);
                    }
                }
            }
        }

        /// <summary>鎸夌‘瀹氭€ф《椤哄簭鏀堕泦鎵€鏈?LF2 entity銆?/summary>
        public void GetAllEntities(List<LF2Entity> dst)
        {
            if (dst == null) return;
            dst.Clear();

            var bucketKeys = GetBucketKeySnapshot();
            if (bucketKeys == null) return;

            foreach (int simOrder in bucketKeys)
            {
                if (!_buckets.TryGetValue(simOrder, out Bucket bucket)) continue;
                bucket.EnsureSorted(GetRuntimeStableId);

                for (int i = 0; i < bucket.items.Count; i++)
                {
                    if (bucket.items[i] is LF2Entity entity)
                    {
                        dst.Add(entity);
                    }
                }
            }
        }

        public int ObjectCount
        {
            get
            {
                int count = 0;
                var bucketKeys = GetBucketKeySnapshot();
                if (bucketKeys == null) return 0;

                foreach (int simOrder in bucketKeys)
                {
                    if (!_buckets.TryGetValue(simOrder, out Bucket bucket)) continue;
                    count += bucket.items.Count;
                }
                return count;
            }
        }


        public SimContext Context => _context;

        /// <summary>
        /// 褰撳満涓婃鍣ㄦ暟閲忎綆浜庢寮忕増闃堝€兼椂闅忔満鐢熸垚鍦烘櫙姝﹀櫒銆?
        /// 鐢熸垚浣嶇疆浠庡彲璧拌竟鐣屽尯鍩熶腑閲囨牱銆?
        /// </summary>
        public void RandomWeaponDropTickAll(int tickIndex)
        {
            int weaponCount = 0;
            var bucketKeys = GetBucketKeySnapshot();
            if (bucketKeys == null) return;

            foreach (int simOrder in bucketKeys)
            {
                if (!_buckets.TryGetValue(simOrder, out Bucket bucket)) continue;

                var snapshot = bucket.items.Count > 0
                    ? new List<ISimObject>(bucket.items)
                    : null;

                if (snapshot == null) continue;

                foreach (var obj in snapshot)
                {
                    if (obj is LF2WeaponBase wb)
                    {
                        int wt = wb.WeaponType;
                        if (wt == 1 || wt == 2 || wt == 4 || wt == 6)
                            weaponCount++;
                    }
                }
            }
            if (weaponCount >= 4) return;
            if (Rng.NextInt(0, 200) != 0) return;

            var manager = CharacterAnimtorManager.Instance;
            if (manager == null) return;

            var candidates = new System.Collections.Generic.List<int>();
            for (int oid = 100; oid < 200; oid++)
            {
                var wrapper = manager.GetCharacterConfig(oid);
                if (wrapper == null) continue;
                if (oid == 122 || oid == 123)
                {
                    if (Rng.NextInt(0, 2) == 0) continue;
                }
                candidates.Add(oid);
            }
            if (candidates.Count == 0) return;

            int selectedOid = candidates[Rng.NextInt(0, candidates.Count)];

            var factory = LF2ObjectPointFactory.Instance;
            if (factory == null) return;

            var boundaryManager = BoundaryWallManager.Instance;
            if (boundaryManager == null || !boundaryManager.TryGetWalkableBounds(out var walkableBounds))
                return;

            var charData = CharacterAnimtorManager.Instance?.GetCharacterData(selectedOid);
            int flyFrame = -1;
            int minFrame = int.MaxValue;
            if (charData?.frames != null)
            {
                foreach (var f in charData.frames)
                {
                    if (f == null) continue;
                    if (f.frameId > 0 && f.frameId < minFrame) minFrame = f.frameId;
                    if (flyFrame < 0 && f.frameId > 0 && (
                        f.state == LF2States.WeaponInSky ||
                        f.state == LF2States.WeaponThrowing ||
                        f.state == LF2States.HeavyWeaponInSky))
                        flyFrame = f.frameId;
                }
            }
            if (flyFrame < 0) flyFrame = minFrame != int.MaxValue ? minFrame : 0;

            int xMin = Mathf.RoundToInt(walkableBounds.xMin * SimulationConstants.PIXELS_PER_UNIT);
            int stageWidth = Mathf.RoundToInt(walkableBounds.width * SimulationConstants.PIXELS_PER_UNIT);
            int zMin = Mathf.RoundToInt(walkableBounds.yMin * SimulationConstants.PIXELS_PER_UNIT);
            int zMax = Mathf.RoundToInt(walkableBounds.yMax * SimulationConstants.PIXELS_PER_UNIT);
            if (stageWidth <= 60 || zMax - zMin <= 60) return;

            // C++ release 鑷劧鎺夎惤锛歺 = r1 * ((bg.width - 60) / 30) + r2 + 30銆?
            // z = r3 * ((zmax - zmin - 60) / 30) + r4 + zmin + 30銆?
            int r1 = Rng.NextInt(0, 30);
            int r2 = Rng.NextInt(0, 30);
            int r3 = Rng.NextInt(0, 30);
            int r4 = Rng.NextInt(0, 30);
            float lf2X = xMin + r1 * ((stageWidth - 60) / 30) + r2 + 30;
            float lf2Z = r3 * ((zMax - zMin - 60) / 30) + r4 + zMin + 30;
            const float lf2Y = -500f;

            var spawnTask = LF2ReferencePool.Instance.Fetch<OPointCreateTask>();

            spawnTask.opoint = new ObjectPoint
            {
                    oid = selectedOid,
                    kind = 0,
                    action = flyFrame,
                    x = Mathf.RoundToInt(lf2X),
                    y = Mathf.RoundToInt(lf2Y),
                    dvx = 0,
                    dvy = 0,
                    facing = 0,
            };
            spawnTask.parent = null; spawnTask.team = 0;
            spawnTask.pos = new UnityEngine.Vector3(lf2X, lf2Y, 0);
            spawnTask.z = lf2Z; spawnTask.dir = "right"; spawnTask.dvz = 0;
            factory.EnqueueCreateObject(spawnTask);
        }
    }
}
