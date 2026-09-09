# Task Contract — NTSD28-B3-C25G-FRAME-BODY-001

> 状态：`VERIFIED / C25G_COMMON_CORE / DOWNSTREAM_BEHAVIOR_ROUTED`
> 依赖：`NTSD28-B3-C25G-FRAME-BODY-OWNER-AUDIT-001 / VERIFIED`

## 目标

让C25g exact-character ECS与virtual fallback消费同一frame-body core，并关闭不依赖B4后续carrier、B7 lifecycle、B10 audio或B11/H内容的三项current-Authority差异：terminal physical participant保留state14、type3 state3007 HP规则、删除Unity独有heavy-weapon grounded/low-Vx早退。

## 权威

- `battle_world.cpp::step_frames_range`及`apply_native_type3_frame_hp_drain`。
- formal EXE `B1E13AE1...9033`；playable closure `39DDDA15...6109`。

## 允许路径

- `Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs`
- `Assets/NTSD/Scripts/Simulation/Ecs/Passes/BattleEcsCharacterFrameTickPass.cs`
- 新focused Editor tests及被新Authority supersede的既有frame SelfCheck/fixture。
- 本Task/Record/Ledger/STATE/handoff/总表与C25g manifests。

## 不变量

- C25g保持在C25f与C25h之间；C25h/C25j owner及transient render marker不变。
- motion hold普通对象冻结、type3例外；negative relation和kind2 cpoint整段冻结。
- terminal gate只适用于slot0..19、current DAT type0、HP<=0、state14、`HP2Orig<=1`且`RespawnCount<=0`。
- state3007活体不消费hit_a；死体HP归0并选择`hit_d`，0时fallback10，随后同pass继续frame counter。
- exact-character仍计入ECS diagnostics并保留zero-allocation，但不维护第二份算法。

## 明确不做

- 不实现collision-Y next999、multi-sound、negative transition HP/MP cost、terminal pending/free或armor。
- 不修改Config/DAT/parser/model、Scene/Prefab、ProjectSettings或Authority。

## 验收

- test-first覆盖terminal hold、state3007 alive/dead、heavy extra移除、exact/fallback single-core source guard。
- 既有frame/ECS/late/C25相关回归、NTSD28 broad、SelfCheck、compile0、Console0；无Play要求，Scene unchanged。

## 回滚

恢复ECS独立算法与RunCommonFrameTick原分支，并删除本包focused test；不得回退C25f/h/j或AI render-phase包。

## 实施与证据

- `BattleEcsCharacterFrameTickPass`保留exact资格与diagnostics，但唯一行为调用为`RunNativeC25FrameBodyForWorldPass`；fallback的`RunCommonFrameTick`也调用同一核心。
- 正式C25 marker下新增terminal low-slot dead-state14 hold、type3 state3007 alive/dead规则，并隔离Unity旧type2 grounded/low-Vx早退；direct legacy compatibility仍保留旧分支。
- test-first job `040d55a915a44217bec73385021e3ee7`为4/4预期失败；实现后`df142da97807480c953ed5b03352b4b2`为4/4 PASS。
- frame/late/C25相邻job `23753dd8e8d54052b6a205c4348ef87c`为85/85；NTSD28 broad `9c09b270aaa3413499b72a36935594f7`为432/432。
- 2026-09-05 10:50:29 BattleRuntimeSelfCheck fresh PASS；post-clear Console error=0。
- Scene保持SHA `0D74E174...23D77`、203477 bytes、UTC mtime `2026-09-04T13:12:45.1526434Z`；未Play、Authority只读。
- next999 collision-Y、blink table、negative transition cost、多sound及terminal pending/free仍明确未完成，分别进入B4/B7/B10/B11/H。
