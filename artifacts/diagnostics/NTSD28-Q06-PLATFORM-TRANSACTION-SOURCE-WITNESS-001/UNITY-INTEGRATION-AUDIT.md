# Q06 平台事务 Unity 接入审计

日期：2026-09-21。状态：READ_ONLY_INVENTORY / IMPLEMENTATION_NOT_STARTED。
本轮未修改生产、测试脚本、Scene 或资源，未运行 Unity 或全量测试。
依据为当前工作树及正式 playable source；源见证复用 EXTENDED-ACCEPTANCE.md 的 21/277，未重新运行，也不称 Unity 已通过。

## 已观察的接入边界

1. `Animation/Character/BruteForceSceneQuery.cs:1509` 的候选入口目前只重置 CollisionYReference。`:2100` brute-force 按实体对双向调用普通 CollectCandidatesForPair。`:5282` 普通方向以 snapshot ITR 和目标 bdy 为准，并受普通候选 carrier/coarse filter 限制。
   原版平台使用 current ITR、snapshot center/state/attacking 和目标位置点，不能直接塞进普通命中方法的过滤之后。原版每对顺序为 ordinary(a,b) -> platform(a,b) -> ordinary(b,a) -> platform(b,a)，平台可立即改变 integer Y；一个统一的前置或尾置平台遍历不能未经证明就替代此顺序。
2. 同文件 `TryCollectCollisionCandidatesLoose`、`TryCollectCollisionCandidatesRoleAware` 以及零 ITR early-return 使用预计算空间/角色信息。平台导致的同轮位置写入可能使缓存失效。实施前需决定有平台参与时采用严格 slot 顺序路径的条件，保留无平台时现有已验 fast path；不能只补 brute-force 而让默认路径绕过平台，也不能先执行有副作用的路径后无恢复地重试 fallback。
3. `Animation/LF2Objects/LF2Entity.cs:6794` 的 ApplyNativeFrameMotionForWorldPass 当前只执行自身帧速度 kernel；没有 linked-platform 位移事务。平台位移必须按 source 整数 origin、精确坐标写回和 round-to-even 规则接入，不能用 Unity Transform 搬动。
4. 物理整数同步至少存在以下实际入口：`LF2Character.cs:149` ApplyDynamics 尾部；`LF2Entity.cs:5818` RunSharedNonCharacterDatFrameAdvance 尾部；`LF2WeaponBase.cs:555` RunNativePhysicsForWorldPass 尾部。OtherObject/SpecialAttack 还会按 DAT 类型转发 shared character/noncharacter 路径。
   原版 physics_integrator.cpp:35 在最终截断整数前记录 previous XYZ。不可全局修改 SyncIntegerPosition 来记录历史：抓取、持有、投掷和 post-display 也使用此方法，它们不是该历史生产点。进一步需追踪 shared-character 和 weapon resolver 内提前整数同步，防止最终记录的是已经被覆盖的值。
5. 原版 battle_world.cpp:8281 的边界尾部另外写 previous X；previous Y/Z 并非每次 SetPosition 都更新。若新增全部 previous XYZ，必须覆盖此独立生产者；若只实现平台所需 previous Y，须明确范围，不能称完整 position-history 对齐。
6. `Simulation/Core/NTSDEntityRuntime.cs` 是 carrier reset/copy 入口；CollisionYReference 已存在且不能重复修复。新增平台 slot、shadow offset 和所需 previous Y 必须同步 snapshot/hash 生命周期，不能只使当前帧可见。当前持久化 schema 不作变更，准确 schema/restore 路径仍待进一步列清。
7. 两条已定位阴影消费者：`LF2Entity.cs:558` legacy shadow center Y 仅取 render Z；`Simulation/Presentation/BattlePresentationShadowBuild.cs:2700` central stableGroundPosition 仅取 entity.ZInt，之后用于 Shadow command。原版 render_snapshot.cpp:1623 使用 Z + render_shadow_offset_10c。应从逻辑快照传递 offset，仅调整阴影位置，不改实体 Z、排序、HUD 或相机，也不得修改同时被其他绘制消费的共享坐标而产生连带偏移。

## 下一步与验收范围

- 先完成 shared-character/weapon 物理同步和 presentation snapshot capture/copy、lockstep schema 的准确路径清单。
- 然后建立独立 Unity Task/Change，声明精确脚本后再添加 RED fixture 和实现。
- 用平台源 21 场景按分支取代表，优先普通吸附、无 bdy、current/snapshot 分离、同 pair 普通命中顺序、多平台竞争、slot0/失效 link 和 physics history；后续 fulltick/replay 与阴影分别验证。
- 单次局部修改只编译和运行受影响代表；稳定相关包集中联合验证一次。复用未变证据，不逐角色重跑，不以 API 组合源见证冒充 fulltick 或正式 EXE 画面。

Q06 仍未关闭，Q07 正式 DAT/角色图片迁移未开始。现有 environment、CollisionYReference reset、OPoint 出生已关闭职责保持，不因本审计重开。
