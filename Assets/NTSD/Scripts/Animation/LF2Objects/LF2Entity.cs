using NTSD.Animation;
using NTSD.Animation.LF2Tasks;
using NTSD.Simulation;
using UnityEngine;

namespace NTSD.Animation.LF2Objects
{
    /// <summary>
    /// 鎵€鏈夋垬鏂楀疄浣撶殑鎶借薄鍩虹被锛屾壙杞?C++ release 瀹炰綋杩愯鏃剁殑鍏叡瀛楁銆佸抚鍏ュ彛鍜?Unity 娓叉煋妗ユ帴銆?
    /// </summary>
    public abstract class LF2Entity : ILF2Entity
    {

        /// <summary>瀵硅薄鍚嶇О銆?/summary>
        public string Name { get; set; }

        /// <summary>瀹炰綋绋冲畾 ID銆?/summary>
        public int StableId
        {
            get => Runtime.StableId;
            protected set => Runtime.StableId = value;
        }

        /// <summary>瀵硅薄 ID銆?/summary>
        public int ObjectId
        {
            get => Runtime.ObjectId;
            set => Runtime.ObjectId = value;
        }

        /// <summary>闃熶紞 ID銆?/summary>
        public int Team
        {
            get => Runtime.Team;
            set => Runtime.Team = value;
        }

        /// <summary>鐢熸垚鑰?StableId锛?1 琛ㄧず鏃犵敓鎴愯€呫€?/summary>
        public int OwnerId
        {
            get => Runtime.OwnerStableId;
            set => Runtime.OwnerStableId = value;
        }

        /// <summary>琚姄鍙栫姸鎬併€?/summary>
        public int GrabbedBy
        {
            get => Runtime.GrabbedBy;
            set => Runtime.GrabbedBy = value;
        }

        /// <summary>kind==2 tracker 鏍囪銆?/summary>
        public int TrackerFlag
        {
            get => Runtime.TrackerFlag;
            set => Runtime.TrackerFlag = value;
        }

        /// <summary>kind==2 tracker 鐖跺璞″紩鐢ㄣ€?/summary>
        public LF2Entity TrackerParent { get; set; }

        /// <summary>褰撳墠鍛戒腑浣跨敤鐨?itr slot 绱㈠紩锛岀敤浜?spark 璁℃椂銆?/summary>
        public int CurrentItrIndex { get; set; }

        /// <summary>瀵硅薄绫诲瀷鏁存暟鍊硷紝鐢卞瓙绫?ObjectTypeEnum 鍐冲畾銆?/summary>
        public int ObjectType => (int)ObjectTypeEnum;

        /// <summary>瀵硅薄绫诲瀷鏋氫妇锛岀敱瀛愮被瀹炵幇銆?/summary>
        public abstract LF2ObjectType ObjectTypeEnum { get; }

        /// <summary>C++ release 瀹炰綋杩愯鏃跺瓧娈甸暅鍍忋€?/summary>
        public NTSDEntityRuntime Runtime { get; } = new NTSDEntityRuntime();

        private static readonly DeterministicRng FallbackRng = new DeterministicRng(0x4E545344u);

        /// <summary>C++ release 瀹炰綋绫诲瀷鍊笺€?/summary>
        public virtual int ReleaseEntityType => ObjectType;

        /// <summary>瀵硅薄绫诲瀷鏋氫妇鍏ュ彛銆?/summary>
        public LF2ObjectType Type => ObjectTypeEnum;



        /// <summary>鐗╃悊鐘舵€併€?/summary>
        public PhysicsState PS { get; protected set; }

        /// <summary>褰撳墠甯т俊鎭€?/summary>
        public LF2FrameInfo Frame { get; protected set; } = new LF2FrameInfo();

        /// <summary>DAT 甯ф暟鎹紦瀛樸€?/summary>
        public LF2FrameCache FrameCache { get; protected set; } = new LF2FrameCache();

        /// <summary>甯ц浆鎹㈠櫒銆?/summary>
        public FrameTransistor Trans { get; protected set; }

        /// <summary>鏁堟灉鐘舵€併€?/summary>
        public LF2EffectState Effect { get; protected set; } = new LF2EffectState();

        /// <summary>Sprite 璧勬簮寮曠敤銆?/summary>
        public LF2Sprite Sprite { get; protected set; }

        /// <summary>娓叉煋鍣ㄥ紩鐢ㄣ€?/summary>
        public LF2ObjectRenderer Renderer { get; protected set; }

        /// <summary>妯℃嫙涓栫晫寮曠敤銆?/summary>
        public SimulationWorld Match => SimulationTickDriver.Instance?.World;



        /// <summary>甯у欢杩熻鏁板櫒銆?/summary>
        public int FrameDelay
        {
            get => Runtime.FrameDelay;
            set => Runtime.FrameDelay = value;
        }

        /// <summary>C++ release Entity::attacking锛屽抚绛夊緟/鏀诲嚮鐘舵€佽鏁板櫒銆?/summary>
        public int AttackingCounter
        {
            get => Runtime.AttackingCounter;
            set => Runtime.AttackingCounter = value;
        }

        /// <summary>鍛戒腑鍋滃抚/閿佸畾璁℃暟銆?/summary>
        public int HitStun
        {
            get => Runtime.HitStop;
            set => Runtime.HitStop = value;
        }

        /// <summary>绱鍑婚€€ X 閫熷害銆?/summary>
        public float KnockbackVx
        {
            get => Runtime.KnockbackVx;
            set => Runtime.KnockbackVx = value;
        }

        /// <summary>绱鍑婚€€ Y 閫熷害銆?/summary>
        public float KnockbackVy
        {
            get => Runtime.KnockbackVy;
            set => Runtime.KnockbackVy = value;
        }

        /// <summary>绱鍑婚€€ Z 閫熷害銆?/summary>
        public float KnockbackVz
        {
            get => Runtime.KnockbackVz;
            set => Runtime.KnockbackVz = value;
        }

        /// <summary>闇囧睆璁℃椂鍣ㄣ€?/summary>
        public int ShakeTimer
        {
            get => Runtime.ShakeTimer;
            set => Runtime.ShakeTimer = value;
        }

        /// <summary>鏀诲嚮璞佸厤璁℃暟鍣紱瑙掕壊绫绘敼鐢?HitCounters 瀛樺偍銆?/summary>
        public virtual int AttackExempt
        {
            get => Runtime.AttackExempt;
            set => Runtime.AttackExempt = value;
        }

        /// <summary>鐢熸垚鑰呭疄浣撶储寮曪紝opoint 鐢熸垚鏃跺啓鍏ャ€?/summary>
        public int OwnerEntityIndex
        {
            get => Runtime.OwnerSlotIndex;
            set => Runtime.OwnerSlotIndex = value;
        }

        /// <summary>寮瑰皠/鐢熸垚璁℃暟銆?/summary>
        public int ShotCount
        {
            get => Runtime.ShotCount;
            set => Runtime.ShotCount = value;
        }

        /// <summary>C++ release ai_controlled 标记；角色生成后由输入准备阶段消费。</summary>
        public bool AiControlled
        {
            get => Runtime.AiControlled;
            set => Runtime.AiControlled = value;
        }

        /// <summary>itr 鏀诲嚮鍐峰嵈璺熻釜鍣ㄣ€?/summary>
        public virtual LF2ItrRestTracker ItrRest { get; protected set; } = null;

        /// <summary>鐢熷懡鍜岃祫婧愮姸鎬併€?/summary>
        public virtual LF2Health Health { get; protected set; } = null;

        /// <summary>HP 鎭㈠璁℃椂鍣ㄣ€?/summary>
        public virtual int HealTimer
        {
            get => Runtime.HealTimer;
            set => Runtime.HealTimer = value;
        }

        /// <summary>C++ release kill_count锛?1 琛ㄧず鏅€氬疄浣擄紝>=0 琛ㄧず鍏宠仈鐨勭敓鎴愯€?褰掑睘妲姐€?/summary>
        public int KillCount
        {
            get => Runtime.KillCount;
            set => Runtime.KillCount = value;
        }

        /// <summary>C++ release weapon_count锛氳鑹插彈绗涘瓙鍛戒腑鏃朵负璐熷€硷紝姝﹀櫒渚х敤浜庨琛?绗涘瓙绱Н銆?/summary>
        public int WeaponCount
        {
            get => Runtime.WeaponCount;
            set => Runtime.WeaponCount = value;
        }

        /// <summary>C++ release fall_damage_div锛氳惤鍦版寔缁墸琛€鍒嗘敮鐨勪激瀹崇缉鏀鹃櫎鏁般€?/summary>
        public int FallDamageDiv
        {
            get => Runtime.FallDamageDiv;
            set => Runtime.FallDamageDiv = value;
        }



        /// <summary>闃村奖 SpriteRenderer锛岀敱娓叉煋鍣ㄦ敞鍏ャ€?/summary>
        public SpriteRenderer ShadowRenderer { get; private set; }

        /// <summary>娉ㄥ叆闃村奖娓叉煋鍣ㄥ紩鐢ㄣ€?/summary>
        public void SetShadowRenderer(SpriteRenderer sr) => ShadowRenderer = sr;

        /// <summary>鏇存柊闃村奖浣嶇疆鍜屾樉绀虹姸鎬併€?/summary>
        public void UpdateShadow(int renderFrame = 0)
        {
            if (ShadowRenderer == null || PS == null) return;

            int state = Frame?.D?.state ?? -1;
            int oid = ObjectId;
            bool hide = state == 3005
                     || state == 9997
                     || oid == 223
                     || oid == 224;

            ShadowRenderer.enabled = !hide;
            if (!hide)
            {
                var t = ShadowRenderer.transform;
                Vector2 groundPos = PhysicsState.ToUnityGroundPoint(PS.x, PS.z);
                t.position = new Vector3(groundPos.x, groundPos.y, t.position.z);
            }
        }



        /// <summary>褰撳墠娲昏穬 spark slot 鏁伴噺銆?/summary>
        public int SparkSlotCount { get; private set; } = 0;

        /// <summary>鏈€澶?spark slot 鏁伴噺銆?/summary>
        public const int MaxSparkSlots = 10;

        private readonly int[] _sparkTimers = new int[MaxSparkSlots];
        private readonly float[] _sparkWorldX = new float[MaxSparkSlots];
        private readonly float[] _sparkWorldY = new float[MaxSparkSlots];
        private readonly float[] _sparkWorldZ = new float[MaxSparkSlots];

        /// <summary>命中时追加新的 Spark 记录。</summary>
        public void AddSparkSlot(int timerInitial, float worldX, float worldY, float worldZ, int currentRenderFrame = -1)
        {
            if (SparkSlotCount >= MaxSparkSlots) return;
            int slot = SparkSlotCount;
            _sparkTimers[slot] = timerInitial;
            _sparkWorldX[slot] = worldX;
            _sparkWorldY[slot] = worldY;
            _sparkWorldZ[slot] = worldZ;
            SparkSlotCount++;
        }

        /// <summary>读取指定 Spark 记录的年龄。</summary>
        public int GetSparkTimer(int slotIndex) => _sparkTimers[slotIndex];

        /// <summary>读取指定 Spark 记录的世界坐标。</summary>
        public Vector3 GetSparkWorldPos(int slotIndex)
            => new Vector3(_sparkWorldX[slotIndex], _sparkWorldY[slotIndex], _sparkWorldZ[slotIndex]);

        public void AdvanceSparkSlot(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= SparkSlotCount) return;
            _sparkTimers[slotIndex]++;
        }

        public bool RemoveSparkSlotIfTail(int slotIndex)
        {
            if (slotIndex != SparkSlotCount - 1) return false;
            RemoveSparkSlot(slotIndex);
            return true;
        }

        private void RemoveSparkSlot(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= SparkSlotCount) return;
            int tail = SparkSlotCount - 1;
            if (slotIndex < tail)
            {
                System.Array.Copy(_sparkTimers, slotIndex + 1, _sparkTimers, slotIndex, tail - slotIndex);
                System.Array.Copy(_sparkWorldX, slotIndex + 1, _sparkWorldX, slotIndex, tail - slotIndex);
                System.Array.Copy(_sparkWorldY, slotIndex + 1, _sparkWorldY, slotIndex, tail - slotIndex);
                System.Array.Copy(_sparkWorldZ, slotIndex + 1, _sparkWorldZ, slotIndex, tail - slotIndex);
            }
            SparkSlotCount--;
        }


        protected void ResetSpark() => SparkSlotCount = 0;



        /// <summary>Unity 保留的状态事件入口；具体行为以 C++ release 运行时为准。</summary>
        protected virtual bool StateExitEvent() => false;
        protected virtual bool StateEntryEvent() => false;
        protected virtual bool FrameEvent() => false;
        protected virtual bool TransitEvent() => false;
        protected virtual bool TUEvent() => false;
        protected virtual bool DieEvent() => false;
        protected virtual bool DestroyEvent() => false;

        /// <summary>获取当前状态。</summary>
        public virtual int GetState() => Frame.D?.state ?? 0;



        public virtual void SwitchDir(string dir)
        {
            if (PS == null) return;
            if (PS.dir == "left" && dir == "right") { PS.dir = "right"; Sprite?.SwitchLR("right"); }
            else if (PS.dir == "right" && dir == "left") { PS.dir = "left"; Sprite?.SwitchLR("left"); }
        }

        public virtual void SwitchDir(DIRECTION direction)
            => SwitchDir(direction == DIRECTION.LEFT ? "left" : "right");

        public virtual int Dirh() => PS?.dir == "left" ? -1 : 1;

        public virtual int Dirv() => 1;

        protected virtual string CalculateDirection(int facing, string parentDir)
        {
            int face = facing >= 20 ? facing % 10 : facing;
            if (face == 0) return parentDir;
            if (face == 1) return parentDir == "right" ? "left" : "right";
            if (face >= 2 && face <= 10) return "right";
            if (face >= 11 && face <= 19) return "left";
            return parentDir;
        }



        /// <summary>鍙楀埌 itr kind=10/11 鏃剁殑鍙楀姏澶勭悊锛岃鑹插拰姝﹀櫒鍏辩敤銆?/summary>
        public virtual void FluteForce()
        {
            if (PS == null) return;
            float mass = NTSDSpec.GetMassOrDefault(ObjectId);

            const float lowLevel = -140f;
            const float midLevel = -160f;
            const float highLevel = -180f;

            Effect.Super = true;
            PS.vx = 0;
            PS.vz = 0;

            if (PS.y > lowLevel)
                PS.vy = (PS.vy <= 0) ? -7.5f : -PS.vy / 2f;
            else if (PS.y <= lowLevel && PS.y > midLevel)
                PS.vy -= mass / 2f;
            else if (PS.y <= midLevel && PS.y > highLevel)
                PS.vy += mass / 2f;

            switch (ObjectTypeEnum)
            {
                case LF2ObjectType.Character:
                    if (Frame.N >= 55) ImmediateFrame(40);
                    break;
                case LF2ObjectType.HeavyWeapon:
                    if (Frame.N >= 5) ImmediateFrame(1);
                    break;
            }
        }



        /// <summary>鍐欏叆瀹炰綋浣嶇疆銆?/summary>
        public void SetPos(float x, float y, float z)
        {
            if (PS == null) return;
            PS.x = x; PS.y = y; PS.z = z;
        }

        /// <summary>鍒涘缓姝﹀櫒鐮寸纰庣墖鏁堟灉銆?/summary>
        public virtual void BrokenEffectCreate(int id, int num = 8)
        {
            SpawnBrokenWeaponFragments(id);
        }

        protected void SpawnBrokenWeaponFragments(int sourceOid)
        {
            int count = BrokenWeaponFragmentCount(sourceOid);
            if (count <= 0 || PS == null) return;

            var factory = LF2ObjectPointFactory.Instance;
            if (factory == null) return;

            for (int i = 0; i < count; i++)
            {
                int x = (int)PS.x + RandInt(0, 7) - 3;
                int y = (int)PS.y + RandInt(0, 7) - 3;
                float vx = RandInt(0, 11) - 5f;
                float vy = BrokenWeaponFragmentVy(sourceOid, i);
                int frame = BrokenWeaponFragmentFrame(sourceOid, i);

                var task = LF2ReferencePool.Instance.Fetch<OPointCreateTask>();
                task.opoint = new ObjectPoint
                {
                    oid = 999,
                    kind = 0,
                    action = frame,
                    facing = PS.dir == "right" ? 0 : 1,
                    x = 0,
                    y = 0,
                    dvx = 0,
                    dvy = 0,
                    dvz = 0
                };
                task.parent = null;
                task.team = Team;
                task.pos = new Vector3(x, y, PS.z);
                task.z = PS.z;
                task.dir = PS.dir;
                task.useDirectVelocity = true;
                task.directVx = vx;
                task.directVy = vy;
                task.directVz = 0f;
                factory.EnqueueCreateObject(task);
            }
        }

        private static int BrokenWeaponFragmentCount(int oid)
        {
            if (oid == 101 || oid == 218) return 7;
            if (oid == 100 || oid == 213 || oid == 217) return 5;
            if (oid == 201 || oid == 120 || oid == 124) return 3;
            if (oid == 150) return 13;
            if (oid == 151) return 15;
            if (oid == 121) return 4;
            if (oid == 122 || oid == 123) return 9;
            return 0;
        }

        private float BrokenWeaponFragmentVy(int oid, int fragmentIndex)
        {
            if (oid == 150 || oid == 151 || oid == 213)
                return -(RandInt(0, 20) / 2f) - 8f;

            if (oid == 100 || oid == 101 || oid == 201 || oid == 120 || oid == 121 ||
                oid == 122 || oid == 123 || oid == 124 || oid == 217 || oid == 218)
            {
                if ((oid == 122 || oid == 123) && fragmentIndex >= 3)
                    return -(RandInt(0, 18) / 2f) - 4f;

                return -(RandInt(0, 8) / 2f) - 6f;
            }

            return 0f;
        }

        private int BrokenWeaponFragmentFrame(int oid, int fragmentIndex)
        {
            if (oid == 150) return RandInt(0, 4) + (fragmentIndex < 5 ? 0 : 4);
            if (oid == 100) return RandInt(0, 4) + (fragmentIndex < 2 ? 10 : 14);
            if (oid == 213) return RandInt(0, 4) + (fragmentIndex < 2 ? 150 : 154);
            if (oid == 101)
            {
                if (fragmentIndex < 5) return RandInt(0, 2) * 4 + RandInt(0, 4) + 20;
                return RandInt(0, 4) + 30;
            }
            if (oid == 151)
            {
                if (fragmentIndex < 2) return RandInt(0, 4) + 40;
                if (fragmentIndex < 5) return RandInt(0, 4) + 44;
                if (fragmentIndex < 8) return RandInt(0, 4) + 50;
                return RandInt(0, 4) + 54;
            }
            if (oid == 120) return RandInt(0, 4) + (fragmentIndex < 2 ? 54 : 30);
            if (oid == 124) return RandInt(0, 4) + 170;
            if (oid == 121) return RandInt(0, 4) + 60;
            if (oid == 122)
            {
                if (fragmentIndex < 1) return RandInt(0, 4) + 70;
                if (fragmentIndex < 3) return RandInt(0, 4) + 80;
                return RandInt(0, 4) + 74;
            }
            if (oid == 123)
            {
                if (fragmentIndex < 1) return RandInt(0, 4) + 160;
                if (fragmentIndex < 3) return RandInt(0, 4) + 164;
                return RandInt(0, 4) + 74;
            }
            if (oid == 217 || oid == 218) return RandInt(0, 4) + 174;
            return 0;
        }

        /// <summary>姝ｅ紡鎴樻枟闅忔満鏁板叆鍙ｏ紝瀵瑰簲 C++ release 鐨?ntsd_rand()銆?/summary>
        public int BattleRandInt(int minInclusive, int maxExclusive)
            => RandInt(minInclusive, maxExclusive);

        protected int RandInt(int minInclusive, int maxExclusive)
        {
            var rng = Match?.Rng;
            if (rng != null) return rng.NextInt(minInclusive, maxExclusive);
            return FallbackRng.NextInt(minInclusive, maxExclusive);
        }

        /// <summary>妫€鏌?itr arest 鍐峰嵈鏄惁鍏佽鏀诲嚮銆?/summary>
        public bool ItrArestTest() => ItrRest == null || ItrRest.Arest <= 0;

        /// <summary>鍛戒腑鍚庢洿鏂?arest 鍐峰嵈銆?/summary>
        public void ItrArestUpdate(InteractionArea itr)
        {
            if (ItrRest == null) return;
            if (itr != null && itr.arest > 0)
                ItrRest.Arest = itr.arest;
            else if (itr == null || itr.vrest <= 0)
                ItrRest.Arest = NTSDGlobal.Default.Character.ARest;
        }

        /// <summary>妫€鏌ユ寚瀹氭敾鍑昏€呯殑 vrest 鍐峰嵈鏄惁缁撴潫銆?/summary>
        public bool ItrVrestTest(int uid) => ItrRest == null || !ItrRest.HasVrest(uid);

        /// <summary>鏇存柊鎸囧畾鏀诲嚮鑰呯殑 vrest 鍐峰嵈銆?/summary>
        public void ItrVrestUpdate(int attackerUid, InteractionArea itr)
        {
            if (ItrRest == null || itr == null) return;
            int vrest = (itr.arest > 0) ? itr.arest : itr.vrest;
            ItrRest.SetVrest(attackerUid, vrest);
        }

        /// <summary>鏇存柊鍑婚璺緞鐨?vrest 鍐峰嵈锛屽浐瀹氬啓 45銆?/summary>
        public void ItrVrestUpdateKnockdown(int attackerUid, InteractionArea itr)
        {
            if (ItrRest == null || itr == null) return;
            ItrRest.SetVrest(attackerUid, 45);
        }

        /// <summary>绔嬪嵆鍐欏叆鎸囧畾甯э紝缁曡繃 wait 鎺ㄨ繘銆?/summary>
        public virtual void ImmediateFrame(int frameId)
        {
            if (Frame == null || Trans == null) return;
            LF2FrameData targetFrame = FrameCache?.GetFrameDataById(frameId);
            if (targetFrame == null) return;

            Frame.PN = Frame.N;
            Frame.N = frameId;
            Frame.D = targetFrame;
            AttackingCounter = 0;

            if (Frame.D != null && Frame.D.pic >= 0)
                Sprite?.ShowPic(Frame.D.pic);

            Trans.SyncDirectFrameData(Frame.D.wait, Frame.D.next);
        }

        /// <summary>姣忓抚鏃堕棿鏇存柊鍏ュ彛锛屽悇瀛愮被鎸夐渶瑕侀噸鍐欍€?/summary>
        public virtual void TUUpdate() { }

        /// <summary>鎸夊抚 ID 鑾峰彇甯ф暟鎹€?/summary>
        public virtual LF2FrameData GetFrameDataById(int frameId)
            => FrameCache?.GetFrameDataById(frameId);

        /// <summary>璇锋眰璺宠浆鍒版寚瀹氬抚銆?/summary>
        public virtual void TransitionToFrame(int frameId, int wait = 0)
            => Trans?.Frame(frameId, wait);

        /// <summary>鑾峰彇纰版挒鐢?sprite 瀹藉害锛屽崟浣嶄负鍍忕礌銆?/summary>
        public virtual float GetSpriteWidthPxForCollision() => 0f;



        public abstract void Reset();
        public abstract void Init(LF2TaskBase task, LF2ObjectRenderer renderer);

        /// <summary>浠?SimulationWorld 娉ㄩ攢鑷韩銆?/summary>
        public virtual void UnregisterFromWorld()
        {
            SimulationTickDriver.Instance?.World?.Unregister(this);
        }

        /// <summary>閿€姣佸綋鍓嶅璞＄殑鍙琛ㄧ幇銆?/summary>
        public virtual void Destroy()
        {
            Sprite?.Hide();
        }

        /// <summary>FrameTransistor 妫€娴嬪埌 next=1000 鏃惰皟鐢紝瀛愮被鍙疄鐜伴攢姣侀€昏緫銆?/summary>
        public virtual void OnTransitDestroy()
        {
            DestroyEvent();
            Destroy();
            if (Renderer != null)
            {
                LF2ObjectPool.Instance?.Release(Renderer);
                Renderer = null;
            }
            LF2ReferencePool.Instance?.Release(this);
        }

        /// <summary>甯ц浆鎹㈠洖璋冿紝瀛愮被瀹炵幇鍏蜂綋甯у垏鎹㈤€昏緫銆?/summary>
        public virtual void OnFrameTransit(int targetFrameId, bool switchDirAfterTrans, int oldLock) { }



        public int SimOrder => SimOrderConstants.GetSimOrderByObjectType(ObjectTypeEnum);

        public virtual void OnAdded(SimContext ctx)
        {
            RefreshRuntimeSnapshot();
        }

        public virtual void OnRemoved(SimContext ctx)
        {
            Runtime.SlotIndex = -1;
        }

        public virtual void SimTransit(int tickIndex) { }
        public virtual void SimTU(int tickIndex) { }
        public virtual void SimPostInteraction(int tickIndex) { }
        public virtual void SimPreInteraction(int tickIndex) { }
        public virtual void SimEntityCollision(int tickIndex) { }

        /// <summary>妯℃嫙鍚庢湡鏇存柊锛岄粯璁ゅ埛鏂版覆鏌撴繁搴︺€?/summary>
        public virtual void SimLateTick(int tickIndex)
        {
            if (PS != null) Sprite?.SetZ(PS.z + PS.zz);
        }



        /// <summary>鍒嗛厤绋冲畾 ID銆?/summary>
        protected void AllocateStableId()
        {
            StableId = SimulationTickDriver.Instance?.World?.AllocateStableId() ?? 0;
            Runtime.StableId = StableId;
        }

        /// <summary>閲嶇疆绋冲畾 ID銆?/summary>
        protected void ResetStableId()
        {
            StableId = 0;
            Runtime.StableId = 0;
        }

        /// <summary>鍐欏叆杩愯鏃舵Ы浣嶇储寮曘€?/summary>
        public void SetRuntimeSlotIndex(int slotIndex)
        {
            Runtime.SlotIndex = slotIndex;
        }

        /// <summary>鍒锋柊杩愯鏃跺瓧娈甸暅鍍忋€?/summary>
        public void RefreshRuntimeSnapshot()
        {
            RefreshRuntimeFromEntity();
        }

        protected virtual void RefreshRuntimeFromEntity()
        {
            Runtime.StableId = StableId;
            Runtime.ObjectId = ObjectId;
            Runtime.ObjType = ObjectType;
            Runtime.EntityType = ReleaseEntityType;
            Runtime.Team = Team;
            Runtime.OwnerSlotIndex = OwnerEntityIndex;
            Runtime.OwnerStableId = OwnerId;
            Runtime.GrabbedBy = GrabbedBy;
            Runtime.TrackerFlag = TrackerFlag;
            Runtime.Frame = Frame?.N ?? 0;
            Runtime.WaitCounter = Trans?.WaitCounter ?? 0;
            Runtime.NextFrame = Trans?.Next ?? 0;
            Runtime.AttackingCounter = AttackingCounter;
            Runtime.FrameDelay = FrameDelay;
            Runtime.HitStop = HitStun;
            Runtime.AttackExempt = AttackExempt;
            Runtime.HealTimer = HealTimer;
            Runtime.KillCount = KillCount;
            Runtime.ShotCount = ShotCount;

            if (PS != null)
            {
                Runtime.X = PS.x;
                Runtime.Y = PS.y;
                Runtime.Z = PS.z;
                Runtime.Vx = PS.vx;
                Runtime.Vy = PS.vy;
                Runtime.Vz = PS.vz;
                Runtime.SpriteX = PS.sx;
                Runtime.SpriteY = PS.sy;
                Runtime.SpriteZ = PS.sz;
            }

            if (Health != null)
            {
                Runtime.HP = Health.HP;
                Runtime.MP = Health.MP;
                Runtime.PP = Health.PP;
                Runtime.PPMax = Health.MaxPP;
                Runtime.PPBound = Health.PPBound;
                Runtime.HPLost = Health.HPLost;
                Runtime.HPBound = Health.HPBound;
                Runtime.MPMax = Health.MaxMP;
            }
        }

    }
}
