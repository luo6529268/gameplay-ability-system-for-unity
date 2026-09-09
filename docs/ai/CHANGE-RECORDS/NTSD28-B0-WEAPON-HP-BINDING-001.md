# NTSD28-B0-WEAPON-HP-BINDING-001 — weapon HP diagnostic binding

<!-- CHANGE-RECORD
id: NTSD28-B0-WEAPON-HP-BINDING-001
status: FOCUSED_TEST_PASS
code-path: Tools/NTSD28Parity/EntityFieldContract.cs
code-path: Assets/NTSD/Scripts/Simulation/Diagnostics/NTSD28UnityEntityRawCapture.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28UnityEntityRawCaptureEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28UnityRawCaptureEditorTests.cs
authority: NTSD 2.8-Logan EntityState28::weapon_hp_31c initialization and all production consumers; Unity NTSDEntityRuntime::WeaponFlightCounter initialization and all production consumers; B0 raw entity field contract.
evidence: AUTHORITY-AND-UNITY-SOURCE-CHAIN-CLOSED / BUILD-0-WARN-0-ERROR / SELFTEST-21-OF-21 / RAW-SELFTEST-5-OF-5 / FORMAT-PASS / UNITY-COMPILE-0 / JOINT-6-OF-6 / NONZERO-37-AND-NEGATIVE-1-PROJECTION / REAL-WEAPONHP-DIFFERENCE-CLOSED / MATURITY-27-10-10 / GLOBAL-LEDGER-77-RECORDS-14-FILES-PASS / SCOPED-DIFF-CHECK-PASS / NO-PRODUCTION-WRITE-CHANGE
-->

> 状态：`FOCUSED_TEST_PASS / BUILD-0-0 / UNITY-COMPILE-0 / JOINT-6-OF-6`

## 改前事实

- B0 contract 将 `combat.weaponHp` 标为 MISSING，Unity raw 输出 `null`。
- 权威 `battle_world.cpp` 从 DAT `weapon_hp` 初始化 `weapon_hp_31c`，并在 physics、hit、opoint 与 lifecycle
  路径中持续读取和写回。
- Unity `LF2Entity` 从 `characterData.weapon_hp` 初始化 `WeaponFlightCounter`，并在对应 weapon physics、
  hit/damage、special interaction、random weapon 与 late lifecycle 路径中持续读取和写回。
- 两端现有名称不同，但状态所有权、初始化源、写入集合与可观察用途闭合；公共 neutral 基线两端值均为 0。

## 计划改动

- contract 将 `combat.weaponHp` 绑定为 `NTSDEntityRuntime::WeaponFlightCounter` 并晋级 VERIFIED。
- Unity raw projection 输出 live `runtime.WeaponFlightCounter`。
- focused tests 增加非零值与 `-1` 值投影，避免默认 0 的伪闭合。
- 不改任何生产 writer、runtime reset、DAT 或资源。

## 验收与回滚

- 验收标准见对应 Task Contract。
- 回滚只恢复 contract/projection/tests 与 26/10/11 maturity，不触碰生产逻辑。

## 实际结果

- `combat.weaponHp` 现为 VERIFIED，读取 live `runtime.WeaponFlightCounter`；maturity 27/10/10。
- 工具 build 0/0、21/21、5/5、format PASS；contract SHA
  `25C194479DBFDBE90C353352F3F464382EC0094DC3CB7A5F2EAFA098F38331C5`。
- Unity fresh compile 0 error；job `16bf469ff22d48f9ad1ba1d977496949` 6/6 PASS，非零 37 与负值 -1
  均被真实投影测试覆盖。
- 新 raw SHA `AE935277DF097B50D6507466109E9808F4180CC9DC6527C66C82D9850240F67E`；
  comparison 为 3 ticks / 6 pairs / 282 fields、12 unique differences、35 equal、72 difference occurrences。
- 生产 weapon/physics/hit/damage/opoint/lifecycle、DAT、资源、Scene 与 authority 均未改。
- Change Ledger validator 77 records / 14 governed code files PASS；scoped diff check PASS，仅既有换行提示。
