# R8-WP01C-02 — pickup / held / throw / weapon landing execution

> 日期：2026-08-23
> 状态：`VERIFIED / S4 PASS / S5 BLOCKED`
> Change ID：`R8-HOLDPLAY-001`

## Goal

在真实 `NTSD_Battle` Play Mode 的 production `SimulationWorld` 中取得世界武器
pickup→held→throw→landing 的 S4 证据，认证双向关系、wpoint 逻辑坐标、type 1/2/4/6
投掷字段差异、落地分流以及“落地不得额外即时命中目标”的边界。

## Scope

- 使用 live production world、正式 runtime slot/registry、`BattleInteractionWriter`、held pass、
  `LF2WeaponHeldStateResolver`、release resolver 和 frame-advance/landing writer；
- kind2 pickup 建立 holder/held/target/copy/link/pickup-count 字段，并验证轻/重拾取 frame 115/116；
- held sync 记录 weaponact、facing、holder FrameDelay、X/Y/Z integer 坐标与 cover 前后层级；
- type 1/4/6 投掷写 holder slot到 spawner，type 2 保留进入分支前的 spawner sentinel；
- type 1/2/4/6 均保留 picker sentinel，清除双向 link/held reference，并记录 frame、速度与方向输入产生的 Vz；
- 落地覆盖 type1、type2、type4/type6 的停止/弹跳字段，并验证 overlapping target 不被 landing writer 直接伤害；
- 所有 probe-owned 实体在成功或失败后 best-effort 清理并恢复 driver pause 状态。

像素挂点、图片内容、UV/slice、透明排序和前后层级的可见结果不在本包裁决，统一交给
`R8-WP01D`。用户于 2026-08-23 报告“部分技能图片错误”，已登记为 `D-RENDER-006`，
本包不得借机修改表现代码。

## Authority / Evidence

- C++ pickup：`src/entity/collision.cpp:996-1081`；
- C++ held/wpoint/throw：`src/entity/game_tick.cpp:1527-1640,1924-2006`；
- C++ landing：`src/entity/physics.cpp:228-320`，由 `frame_advance.cpp` 调用；
- Unity：`BattleInteractionWriter.TryApplyPickup`、`LF2WeaponHeldStateResolver.Act`、
  `LF2WeaponReleaseFlowResolver`、`LF2Entity.ApplyCurrentDatNonCharacterLanding`；
- 既有 `BattleRuntimeSelfCheck` 与 `PooledEntityReuseAllocationEditorTests` 是 S1～S3 证据，
  不能替代本包的 live Play S4；
- R1-WP02 full C++ trace 继续 `BLOCKED`，不得伪造 S5。

## Files likely involved

- `Assets/NTSD/Scripts/Test/Editor/BattleHeldWeaponLifecyclePlayModeProbeEditor.cs`（新增，Editor-only）；
- 对应 `.meta`；
- 本 Task、`R8-HOLDPLAY-001` Change Record、Ledger、STATE、R8 matrix、diff register 与 handoff。

## Acceptance

1. pickup 后 holder/weapon 的 link、target、held、holder/copy、team 和 pickup count 与 C++ source contract 一致；
2. held pass 的 weaponact、facing、FrameDelay 和 wpoint integer position/cover 偏移与 source contract 一致；
3. type 1/4/6 与 type 2 的 frame/spawner/picker/velocity/link-clear 差异全部有逐项结果；
4. type1/type2/type4/type6 至少各有一条 landing witness；落地不直接改变重叠目标 HP/状态；
5. probe cleanup 后 world/slot/pool/driver 状态回到探针前基线；
6. fresh compile 0 error、聚焦测试、full self-check、Play result、validator 和 scoped diff 均通过。

## Stop conditions

- live world、正式 catalog 或目标类型无法构造稳定夹具；
- 发现 link、wpoint、throw、landing 的 production first-difference；此时新增 D-ID/repair WP 并停止，
  不在认证包中修复 gameplay；
- 需要修改 production gameplay、pass ordering、DAT、scene、resource、render、pool、profile 或 C++ authority；
- probe 无法保证 best-effort cleanup。

## Out of scope

- grab/CPoint/link injury（WP01C-03）、collision/hit/damage（04）、death/respawn（05）、random/late（06）；
- `D-RENDER-006` 技能图片内容和整个 WP01D；
- 1000 实体、Player/Android、T8 默认 stage.dat、服务器；
- C++ executable、trace、hook、instrumentation 或 authority 目录写入。

## Preflight result

- 用户已明确批准“执行 R8-WP01C-02，恢复目标”；
- `R8-WP01C-01` 已取得与其范围相称的 Unity S4，满足 02 前置；
- C++ source 与 Unity writer/resolver 的字段观察点已闭合；既有自动夹具不足以证明 live Play 整链；
- 因此计划新增一个 Editor-only、显式菜单触发、无 production gameplay 修改的 Play 探针。

## Execution result

- fresh Unity compile：probe source 09:36:23，Editor DLL 09:36:40，Console compile error=0；
- final Play result 09:37:31 PASS，tick1、dedicated worker active；OID120/150/121/122的
  pickup/held/throw/landing、spawner/picker、FrameDelay、wpoint integer position与no-immediate-hit均通过；
- cleanup恢复object4、claimed2、render pool2、logic pool2；
- focused EditMode job `36440d545fe64659ae3c73ff1febf03c` 23/23 PASS；
- 09:38:54 full self-check PASS；清空预期负向夹具日志后Console error/warning=0；
- final validator 60 Records / 60 governed code files PASS，scoped diff PASS；
- persistent evidence：`RESEARCH/R8-WP01C-02-pickup-held-throw-landing-runtime-evidence-20260823.md`。

本`VERIFIED`只关闭WP01C-02的Unity S4；C++ full trace/S5、手动具体武器流程和WP01D像素表现仍未关闭。
