# Task Contract — NTSD28-B0-WEAPON-HP-BINDING-001

> 状态：`FOCUSED_TEST_PASS / BUILD-0-0 / UNITY-COMPILE-0 / JOINT-6-OF-6 / REAL-DIFFERENCE-CLOSED`  
> 所属计划：`NTSD28-UNITY-BATTLE-REALIGNMENT-001 / B0`  
> 建立日期：2026-09-03

## 目标

将 `combat.weaponHp` 从 MISSING 绑定到 `NTSDEntityRuntime.WeaponFlightCounter`。权威
`EntityState28::weapon_hp_31c` 与 Unity `WeaponFlightCounter` 均由 DAT `weapon_hp` 初始化，并由武器落地、
命中伤害、特殊交互和生命周期分支消费或写回；本包只更正 B0 diagnostic contract/projection，不改变这些生产写入。

## 允许文件

- `Tools/NTSD28Parity/EntityFieldContract.cs`
- `Assets/NTSD/Scripts/Simulation/Diagnostics/NTSD28UnityEntityRawCapture.cs`
- `Assets/NTSD/Scripts/Test/Editor/NTSD28UnityEntityRawCaptureEditorTests.cs`
- `Assets/NTSD/Scripts/Test/Editor/NTSD28UnityRawCaptureEditorTests.cs`
- 本 Task/Change、Ledger、STATE、handoff、总表

## 不变量与验收

- 只补 diagnostic binding；不改 DAT、武器、物理、命中、damage、lifecycle 或其他生产逻辑。
- maturity 从 26 verified / 10 candidate / 11 missing 变为 27 / 10 / 10。
- 不同时裁决复活、平台、护甲、motion hold、环境 source、runtime/lifecycle code。
- 工具 Release build 0 warning/0 error、general self-test 21/21、raw self-test 5/5、format PASS。
- Unity 编译 0 error；focused test 必须覆盖非零和负值，不能只用默认 0 自证。
- 公共双实体真实 raw 中 `combat.weaponHp` 的 missing 差异关闭，其他真实差异不得被归一化或隐藏。
- Change Ledger validator 与 scoped diff check 通过。

## 回滚

恢复 `combat.weaponHp` 的 MISSING/null、maturity 26/10/11 和对应测试断言；无需回滚任何生产行为。

## 实际结果

- contract/projection 已绑定 `WeaponFlightCounter`，maturity 27/10/10；contract SHA
  `25C194479DBFDBE90C353352F3F464382EC0094DC3CB7A5F2EAFA098F38331C5`。
- 工具 Release build 0 warning/0 error、general self-test 21/21、raw self-test 5/5、format PASS。
- Unity fresh compile 0 error；focused job `16bf469ff22d48f9ad1ba1d977496949` 6/6 PASS，明确覆盖
  `weaponHp=37` 与耗尽值 `-1`。
- 公共双实体 raw SHA `AE935277DF097B50D6507466109E9808F4180CC9DC6527C66C82D9850240F67E`；
  真实差异 13→12、equal 34→35、difference occurrences 78→72，首差异仍为 `vitals.baseMaxMp`。
- 本包没有修改生产 writer、DAT、资源、Scene 或权威目录。
- Change Ledger validator 77 records / 14 governed code files PASS；scoped diff check 无错误，仅既有 LF→CRLF 提示。
