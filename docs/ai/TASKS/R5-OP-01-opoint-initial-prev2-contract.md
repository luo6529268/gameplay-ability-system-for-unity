# R5-OP-01 — normal opoint child initial Prev2 contract

> 建立日期：2026-08-22  
> 状态：RUNTIME_PENDING — 最小initializer/cache修复、Unity编译与full self-check已通过；C++ trace / PlayMode待验。  
> 对应差异：D-OP-001  
> Change ID：R5-OP-001

## Goal

使 Unity normal opoint child 的出生 history 与 C++ Release 一致：current frame采用
authored action，但 Prev2在出生 tick保持reset默认0，仅由下一 collision snapshot
镜像 current frame。

## Authority / Evidence

- release participation：`J:/QQFile/NTSD2.4/ntsd_release/Makefile:17,22,32`；
- reset contract：`include/game_world.h:216-258`；
- spawn writer：`src/entity/collision.cpp:1271-1369`；
- pass order：`src/entity/game_tick.cpp:630-632,1646-1652`；
- Unity mapping：`LF2Character.InitializeFromOpoint`、
  `LF2WeaponBase.InitializeFrame`、
  `LF2OtherObjectLifecycleModule.InitializeFrame`、
  `SimulationWorld.CaptureCollisionFrameSnapshotsAll`。

## Scope

允许脚本文件：

1. `Assets/NTSD/Scripts/Animation/LF2Objects/LF2Character.cs`
2. `Assets/NTSD/Scripts/Animation/LF2Objects/LF2WeaponBase.cs`
3. `Assets/NTSD/Scripts/Animation/LF2Objects/LF2OtherObject.Lifecycle.partial.cs`
4. `Assets/NTSD/Scripts/Animation/LF2Objects/LF2SpecialAttack.cs`
5. `Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs`

## Required behavior

1. Character / weapon / other normal opoint child materialize 后 current frame仍为action；
2. materialize 边界的 `Frame.Prev2` 与 `Runtime.PrevFrame2` 为0；
3. `Frame.Prev2D`解析frame0（存在则frame0 data，否则null），不得错误指向action data；
4. 下一 collision snapshot把Prev2/Prev2D/Runtime.PrevFrame2镜像为current frame；
5. SpecialAttack不新增action→Prev2 writer，只为reset 0补齐frame0 `Prev2D` cache；
6. slot、identity、current frame data、position/velocity、registration与pool行为不变；
7. 不改变 approved extended-capacity与CentralOnly边界。

## Verification

| 层级 | 验收 |
|---|---|
| S0 source | reread C++ reset/spawn/pass order与Unity全部initializer/collision snapshot owner。 |
| S1 focused | production factory创建四种nonzero-action child，验证birth→next snapshot矩阵。 |
| S2 governance | Record、ledger、STATE、diff register、main plan、handoff；validator/scoped diff。 |
| S3 Unity | current Editor script compile 0 error；full `BattleRuntimeSelfCheck` PASS。 |
| S4 honesty | 最高状态`RUNTIME_PENDING`；C++ trace / real Play Mode继续待验。 |

## 已写实现与实际结果

- Character / WeaponBase / OtherObject birth Prev2已由action收窄为0，Prev2D解析frame0；
- Character在ModuleBind加载wrapper后重新解析frame0 cache；
- SpecialAttack保持Prev2=0并补齐frame0 cache；
- production factory创建Character/LightWeapon/Other/SpecialAttack四种nonzero-action child，
  materialize时current=action且history=0，下一CollisionSnapshot后history=current；
- UnityMCP force refresh触发fresh Tundra build success（23.19s），Assembly-CSharp更新至17:14:38，未检出`error CS`；
- full `BattleRuntimeSelfCheck`于2026-08-22 17:15:48在fresh assembly上写出`PASS`；16:54:37旧程序集PASS已作废；
- C++ runtime trace与real PlayMode未取得，状态不得高于`RUNTIME_PENDING`。

## Stop conditions

- 需要改变spawn pass/cursor、factory registration、slot/generation、pool或current action；
- frame0 data映射无法在现有 FrameCache合同中表达；
- focused fixture要求修改scope外 gameplay；
- compile/self-check无法在列明文件内最小修复；
- 需要修改、运行、构建或写入 C++ authority。

## Out of scope

action0 DAT adapter、kind2 relation、multi-opoint、newborn/free lifecycle其余差异、R1 trace、
R6 render、server/lockstep、performance、Android、T8、physical input与完整PlayMode认证。
