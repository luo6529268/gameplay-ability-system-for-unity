# Handoff — R5-CPT-01 CPoint raw frame / wait-state preservation

> 日期：2026-08-22  
> Change ID：`R5-CPT-001`  
> 当前状态：`RUNTIME_PENDING` — source preflight、最小代码、ledger/scoped diff、Unity compile与full self-check已完成；C++ trace / Play Mode待验。  
> Authority：`J:/QQFile/NTSD2.4/ntsd_release/src/entity/cpoint.cpp:35-124`、
> `src/entity/weapon.cpp:42-48`、`src/entity/game_tick.cpp:659-664`。

## 已完成 source preflight

- C++ `Makefile:20-21` 确认 `cpoint.cpp` / `weapon.cpp` 属于正式 release build；
- step10 先执行 CPoint、再执行 weapon sync；
- relation fallback、decrease escape、three action routes 与 current-frame held vaction 只写 frame /
  explicit fields，不写相邻 wait state；
- Unity `DirectWriteFrameImmediateWaitReset` / `ApplySignedImmediateFrameWaitReset` 通过
  `SetFrameTickImmediateRawDirect` 额外清 `Runtime.FrameWaitCounter`；
- Unity `SetCpointRawFramePreserveWait` 会拒绝 missing positive frame，不能作为本包七处 raw writer；
  `DirectWriteRawFramePreserveWaitCounter` / `ApplySignedCpointFrame` 能保留FWC、Frame.D、Trans mirror、
  missing raw frame和负 action facing；
- 未运行、修改、构建、复制或向 C++ authority 写入任何内容。

## 已写最小改动

- `BattleCpointWriter.RunKind1`：两个 relation frame0 fallback、decrease escape attacker/victim
  改为 `DirectWriteRawFramePreserveWaitCounter`；
- `ApplyAction`：signed attacker action改为 `ApplySignedCpointFrame`，victim vaction改为
  `DirectWriteRawFramePreserveWaitCounter`；
- `SyncCaughtByCpoint`：held current-frame vaction改为 `DirectWriteRawFramePreserveWaitCounter`；
- `BattleRuntimeSelfCheck`：state9/action、negative action、held-vaction、decrease/escape/mismatch的
  FWC expectation 改为 sentinel preservation；增加 missing caught-slot fallback sentinel。

## 已登记的独立差异

`D-CPT-003`：C++ reciprocal/kind2 mismatch 在 frame0 + skip actions/decrease 后仍可运行 dircontrol；
Unity 当前直接 return。该差异已登记，不能混入本包；本包没有改其流程。

## 首次 Unity 验证结果

- Unity scripts refresh后的filtered `error CS`=0；
- 09:00:31 首次 full self-check失败，shared-DAT simultaneous action fixture出现
  `catcherFrame=122,victimFrame=132`。原因不是FWC保持，而是首版 CPoint专用raw helper拒绝该夹具的
  missing positive frame133；
- 已在本合同内改用与旧 immediate writer相同missing-frame边界、但不清FWC的 raw direct writer，并为
  该夹具加入FWC 27/28 sentinel；
- 修正后再次 scripts refresh，filtered `error CS`=0；
- 菜单 `NTSD/验证/运行战斗运行时自检` 的结果文件
  `Temp/NTSD_BattleRuntimeSelfCheck.result` 于2026-08-22 09:08:02写入 `PASS`。

## 下一步

1. 保持 `R5-CPT-001` 为等待 C++ trace / Play Mode 的证据包，不得再扩大；
2. 按 D-009 立即预检下一条独立 source 差异；优先保持 `D-CPT-002` stats 与 `D-CPT-003` control flow 分离；
3. 若未来 C++ trace或真实场景验收失败，以 first difference 建立新合同；不得回填为“完整对齐”。

## 禁止扩大

不得改 CPoint/WeaponSync pass order、kind2 validation、throw、held/link、opoint、input、collision、render、
DAT/scene、slot/generation、ECS capacity、C++ authority、`D-CPT-002` 或 `D-CPT-003`。
