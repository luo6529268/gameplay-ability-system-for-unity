# NTSD28-B0-HIT-REACTION-BINDING-001 — hit reaction / Fall diagnostic correction

<!-- CHANGE-RECORD
id: NTSD28-B0-HIT-REACTION-BINDING-001
status: FOCUSED_TEST_PASS
code-path: Tools/NTSD28Parity/EntityFieldContract.cs
code-path: Assets/NTSD/Scripts/Simulation/Diagnostics/NTSD28UnityEntityRawCapture.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28UnityEntityRawCaptureEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28UnityRawCaptureEditorTests.cs
authority: NTSD 2.8-Logan EntityState28::hit_reaction_timer 20/40/60/80 value domain, hit writers and tail decrement; Unity NTSDEntityRuntime::Fall writer/recovery chain; distinct HitStop and HitStateCount ownership.
evidence: OLD-HITSTOP-CANDIDATE-CORRECTED / FALL-SOURCE-CHAIN-CLOSED / BUILD-0-WARN-0-ERROR / SELFTEST-21-OF-21 / RAW-SELFTEST-5-OF-5 / FORMAT-PASS / UNITY-COMPILE-0 / JOINT-6-OF-6 / FALL-60-HITSTOP-4-PROJECTS-60 / MATURITY-32-8-7 / REAL-BASELINE-9-38-54-UNCHANGED / GLOBAL-LEDGER-80-RECORDS-14-FILES-PASS / SCOPED-DIFF-CHECK-PASS / NO-PRODUCTION-WRITE-CHANGE
-->

> 状态：`FOCUSED_TEST_PASS / BUILD-0-0 / UNITY-COMPILE-0 / JOINT-6-OF-6`

## 改前事实

- authority `hit_reaction_timer` 使用20/40/60/80分档，命中时按fall累加/钳制，致死写80，并在frame tail递减。
- Unity `Fall` 使用同一20/40/60/80分档与命中/死亡/恢复链；`HitStop`另用于角色生命周期和显示计时，
  `HitStateCount`通常写45并服务破防等分支。
- 当前 contract/projection 把该字段指向 `HitStop`，neutral 0值无法揭露映射错误。

## 计划改动

- contract改为 `NTSDEntityRuntime::Fall` VERIFIED；projection输出live `runtime.Fall`。
- focused以Fall60/HitStop4防止回归；raw neutral差异集合应保持不变。
- 不改任何production writer、DAT、资源或Scene。

## 实际结果

- `combat.hitReactionTimer` 现绑定 `runtime.Fall` 并晋级VERIFIED；maturity32/8/7。
- 工具build0/0、21/21、5/5、format PASS；contract SHA
  `D28F1BBB8B8325A488F5D4AF6D3DD32CE36F58A60B92149E3AC3E7FB838632E0`。
- Unity compile0；job `42b910d17b5a47fd8aa2b4614aaa12fb` 6/6 PASS，Fall60与HitStop4分离通过。
- 新raw SHA `E0C56A7BD89D7A24001A37E0D5D2F1CAF045663D399C5B07F7309B4CD381C055`；
  comparison仍为9 unique differences / 38 equal / 54 occurrences，未虚构差异减少。
- production hit、DAT、资源、Scene与authority均未改。
- Ledger validator 80 records / 14 governed code files PASS；scoped diff check PASS。
