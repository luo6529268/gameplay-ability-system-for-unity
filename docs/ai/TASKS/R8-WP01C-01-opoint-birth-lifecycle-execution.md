# R8-WP01C-01 — opoint birth / newborn / basic lifecycle execution

> 日期：2026-08-23  
> 状态：`VERIFIED / S4 PASS / S5 BLOCKED`  
> Change ID：`R8-OPLIFE-001`

## Goal

在真实 `NTSD_Battle` Play Mode 的 production `SimulationWorld` 中，用正式 DAT catalog、
`LF2ObjectPointFactory`、`BattleStructuralWriter`、runtime slot table、逻辑对象池和表现对象池，取得
character、weapon、special attack、other 四类 opoint 的 producer→consumer S4 证据，并认证出生帧、
`Prev2`、slot/generation、扫描游标边界、释放和同槽复用。

## Scope

- 目标 OID：type0 character `33`、type1 weapon `120`、type3 special attack `203`、type5 other `999`；
- 使用 Editor-only 显式探针在 live world 注册一个最小 fixture producer，producer 本身不替代被测 factory；
- 每类通过完整 production `SimulationTickDriver.StepOneTick` 到达 late opoint producer/consumer；
- 记录 source tick、producer slot、spawn slot/generation、OID、current frame、runtime frame、Prev2、
  object type、active/resolve 状态和对象池计数；
- 用最低空闲 slot 控制一条 producer 后高 slot 的 same-pass witness，以及一条 producer 前低 slot 的
  next-pass witness；
- 释放实体后验证旧 handle 立即失效，再以同一最低空闲 slot 生成 replacement 并验证 generation 前进；
- 所有 probe-owned producer/filler/spawn 在结束或失败时 best-effort 清理，恢复 driver paused 状态。

## Authority / Evidence

- C++：`src/entity/frame_advance.cpp::process_opoint_spawn/spawn_from_opoint` 与
  `src/entity/game_tick.cpp::game_tick` 的 slot-order/free lifecycle source contract；Makefile release 参与性
  已由 R1 登记；
- Unity production：`SimulationWorld.LateEntityUpdateAll`、`LF2ObjectPointFactory.ProcessOpointSpawn`、
  `BattleStructuralWriter`、`RuntimeSlotTable`；
- 既有 `W05OpointLifecycleEditorTests` 仅为 S1～S3 隔离证据，不能替代本包 S4；
- R1-WP02 full C++ trace保持BLOCKED，不伪造S5。

## Files likely involved

- `Assets/NTSD/Scripts/Test/Editor/BattleOpointLifecyclePlayModeProbeEditor.cs`（新增，Editor-only）；
- 对应 `.meta`；
- Change Record、Ledger、STATE、R8矩阵和handoff。

## Acceptance

1. 四个 production OID 均由正式 factory 在 live world 创建为预期 CLR/object type；
2. action0、runtime/current frame、Prev2=0、有效 slot/generation 均有逐条结果；
3. high-slot newborn 在创建 tick 的剩余 late scan 可被观察，low-slot newborn直到下一 tick才被观察；
4. release 后旧 handle 不再解析；replacement 复用同 slot 且 generation 不同；
5. probe cleanup 后 live world 只保留探针前的非探针实体，且无 probe-owned active pool object；
6. fresh compile 0 error、聚焦验证/完整self-check不回归、Play结果文件PASS、validator PASS。

## Stop conditions

- live catalog没有目标OID/config，或当前Unity Editor未运行；
- 需要修改 gameplay、factory、slot、pool、pass ordering、scene、DAT或资源；
- first-difference指向production逻辑；此时新增D-ID/repair WP并停止，不在本包修复；
- 需要运行/构建/修改/写入C++ authority；
- probe不能保证best-effort cleanup或会污染用户场景。

## Out of scope

- pickup/held/throw、grab/CPoint/link、hit/damage、death/respawn；
- 中央像素/阴影/排序视觉验收；
- 1000实体性能与Player/Android；
- T8默认stage.dat。

## Current preflight result

- 用户已明确回复“批准执行 R8-WP01C-01，恢复目标”；
- 现有 W05 测试覆盖结构但不是S4，因此需要独立Editor-only Play探针；
- 目标 OID 均存在于 `Assets/NTSD/Config/data.txt`；运行时仍需验证当前 live catalog 已加载；
- 当前系统进程未发现 Unity Editor，脚本写入和静态验证可继续，实际编译/Play 将等待 Editor 启动。

## Execution result

- Unity Editor随后恢复并通过socket6401完成fresh force-all compile；新Editor DLL晚于probe源码，0 error；
- Play结果`Temp/NTSD_R8_WP01C_01_OpointLifecycle.result.json`为PASS，tick356→359；
- OID33/120/203/999分别解析为type0/1/3/5与预期CLR，frame/runtime/Prev2均为0；
- 同槽53的generation为1→3→5→7，release均拒绝旧handle并恢复world/render pool/logic pool；
- high slot52→53在creation tick攻击计数0→1；low producer53→spawn52在creation tick为0、next tick为1；
- cleanup后object6、claimed4、object pool2、logic pool4均回到baseline；
- W05 EditMode job `3b8e08105d0946bca58d88e5ed6ef990` 8/8 PASS；
- full self-check 09:06:51 PASS；final ledger validator/diff尚待。

Final governance：`Validate-ChangeLedger.ps1` PASS（59 records / 59 governed code files），scoped
`git diff --check` PASS。`R8-WP01C-01`已关闭；full C++ trace和extended >399 Play仍是明确未关闭边界。
