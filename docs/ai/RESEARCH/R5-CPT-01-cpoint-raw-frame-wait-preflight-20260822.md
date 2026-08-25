# R5-CPT-01 — CPoint raw frame / FrameWaitCounter 静态预检

> 日期：2026-08-22  
> 对应差异：`D-CPT-001`  
> 预定 Change ID：`R5-CPT-001`  
> 状态：`RUNTIME_PENDING` — source preflight、最小脚本、Unity compile 与 full self-check 已完成；C++ trace / Play Mode 待验。  
> 唯一 authority：`J:/QQFile/NTSD2.4/ntsd_release` 的 release live source。

## 1. C++ release 合同

- `Makefile:20-21` 把 `src/entity/weapon.cpp` 与 `src/entity/cpoint.cpp` 列入 `ntsd_new.exe`
  release build。
- `src/entity/game_tick.cpp:659-664` 的 step10 先调用 `run_cpoint_runtime_pass(world)`，再调用
  `weapon_sync_runtime_pass(world)`。
- `src/entity/cpoint.cpp:35-38`、`47-49`、`51-60` 在 CPoint relation 失效时只写
  `atk_core.frame = 0`；没有同一分支的 wait-counter 写入。
- `src/entity/cpoint.cpp:63-78` 在 decrease escape 中只连续写 frame
  （attacker 0、victim 0、victim 181、attacker 0），并写 hit/velocity；没有 wait-counter 写入。
- `src/entity/cpoint.cpp:81-124` 的 `aaction`、`taction`、`jaction` 只写 attacker/victim frame，
  处理负 frame 的 facing，再清 attacking；没有 wait-counter 写入。
- `src/entity/weapon.cpp:42-48` 的 current-frame held CPoint sync 只写
  `vic_core.frame = cp.vaction`，必要时处理负 frame facing；没有 wait-counter 写入。

这里的 `FrameWaitCounter` 是 Unity runtime 的逻辑时序字段，不是把 C++ 字段名机械替换为同名字段。
本包的可验证 source 事实是：上述 C++ CPoint 写 frame 的位置没有任何对应 wait-state reset，而 Unity 的
当前 helper 会额外清其 runtime wait state。

## 2. Unity 现状与确定差异

Unity 当前入口是 `LF2Entity.RunCpointCheckStep10`、
`RunCpointMismatchTailStep10` 与 `RunWeaponSyncHeldStep10`，它们分别进入
`BattleCpointWriter.RunKind1`、`RunKind2Validation` 与 `SyncHeldCpoint`。

| Unity writer | C++ 对照 | 当前额外副作用 |
|---|---|---|
| `RunKind1:28` | broken caught slot 的 `cpoint.cpp:35-38` | `DirectWriteFrameImmediateWaitReset(0)` 清 `FrameWaitCounter`。 |
| `RunKind1:37` | reciprocal/kind2 relation 失败的 `51-60` | 同上。 |
| `RunKind1:53-54` | decrease escape 的 `63-78` | attacker/victim frame write 都清 `FrameWaitCounter`。 |
| `ApplyAction:177-179` | action selection 的 `81-124` | attacker/victim action frame write 都清 `FrameWaitCounter`。 |
| `SyncCaughtByCpoint:195` | held sync 的 `weapon.cpp:42-48` | victim vaction write 清 `FrameWaitCounter`。 |

`LF2Entity.SetFrameTickImmediateRawDirect:5908-5913` 是上述 immediate helper 的 reset 来源。
`SetCpointRawFramePreserveWait:5051-5067` 对不存在的非负 frame 会主动 return，因此不能用于 C++ 允许
raw-write 的 missing positive frame。`DirectWriteRawFramePreserveWaitCounter:4277-4286` 则与旧 immediate
writer拥有相同的 missing-frame 写入边界，但不清 runtime `FrameWaitCounter`；负 action 使用既有
`ApplySignedCpointFrame:4730-4742`，其签名 facing 与 raw direct frame 行为符合本项。

因此本项不是新增抽象、全局 helper 替换或 pass 改序，而是把七个 CPoint callsite 从错误的 immediate-reset
writer 收窄为已有、能保留 FWC 且支持 missing raw frame 的 direct writer。

## 3. 单独登记、不合并的发现

`D-CPT-003` 已在总登记册单独记录，不能混入 `R5-CPT-001`：

- `cpoint.cpp:51-60` 对 reciprocal/kind2 失败设置 `skip_actions` / `skip_decrease`，但不立即 return；
  同函数尾部 `182-189` 的 `dircontrol` 仍可在 `attacking == 2` 时执行。
- Unity `BattleCpointWriter.RunKind1:32-39` 在同类 relation 失败后直接 return。
- 这是独立的控制流 / 输入可观察差异，需要单独 contract、fixture 和 scope review；本包不改早退行为、
  CPoint pass 顺序、input、throw、stats 或 dircontrol。

## 4. 最小实施与验收设计

允许脚本范围仅为：

1. `Assets/NTSD/Scripts/Simulation/Ecs/BattleCpointWriter.cs`；
2. `Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs`。

现有 self-check 将把旧的 “CPoint immediate frame 清 FWC” 预期改为 source contract：

- state9 held sync、aaction / taction / jaction；
- negative action matrix；
- held current-frame vaction 的 `-131/0/131` matrix；
- decrease escape、escape-before-throw 与 reciprocal mismatch fallback。

每项都使用不同非零 FWC sentinel，并继续锁定原有 frame、facing、Trans wait、Prev2、attacking、
velocity、hit counter、CPoint link 和 missing positive raw frame 行为。不得借此改 `D-CPT-002` injury global stats、
`D-CPT-003` early return、CPoint/WeaponSync pass ordering、held/link/opoint、input、collision、
render、DAT/scene 或 C++ authority。

## 5. 证据边界

本次只能获得 C++ source contract、Unity compile 与 Unity focused/full self-check 证据。
C++ full trace 继续由 `R1-WP02 / BLOCKED` 管理，真实 Play Mode 继续待后续；因此即使代码验证通过，
`R5-CPT-001` 的最高状态也只能是 `RUNTIME_PENDING`。
