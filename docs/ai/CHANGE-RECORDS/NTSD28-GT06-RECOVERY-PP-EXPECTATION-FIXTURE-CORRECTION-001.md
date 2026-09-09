# NTSD28-GT06-RECOVERY-PP-EXPECTATION-FIXTURE-CORRECTION-001

<!-- CHANGE-RECORD
id: NTSD28-GT06-RECOVERY-PP-EXPECTATION-FIXTURE-CORRECTION-001
status: VERIFIED
change-kind: TEST_FIXTURE_EXPECTATION_CORRECTION
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
authority: 用户2026-09-09 Goal3 PartA；直接采用GLM独立复核的NTSD2.8-Logan BattleWorld28::advance_native_resources_range普通MP公式。battle_world.cpp:2323/2350-2355；Unity LF2Entity.cs:2842-2864和NTSDGlobal.cs:120-124。当前EXE/closure不变。
evidence: Only the PP20-to-PP4 assertion token changed; resulting whole-file SHA matches the precomputed BEE7CE5B82EF1A21D007B9093544F12A6B75567EC4B2C9BC00F2B7EA5AC51F72. Fresh full SelfCheck at 2026-09-09T12:04:37Z passed GT06/current-DAT matrix and stopped at the existing later R4-HIT-02A check in CheckKind10And11CharacterStatsWithoutDamage:15195; no later assertion changed. Runtime build exit0/47 warnings/0 errors; Editor build exit0/129 warnings/0 errors. Scene SHA remains D18E75F7920E12A1FEB13A8C23949042869E679B420A3073F7C21E4D4F4A2F11. VERIFIED is limited to this fixture correction; full SelfCheck remains failed. Final validator and incremental audit appended below.
-->

完整范围、不变量、Authority行号、原状、验收、停止与回滚见
[本包Task](../TASKS/NTSD28-GT06-RECOVERY-PP-EXPECTATION-FIXTURE-CORRECTION-001.md)。

## 改前与预期职责

只修复`ExpectRecoveryFixture`中过时的PP期望，不改变fixture初始化或生产公式。
HP100→101之后PP应0→4；WeaponCount-1/EnvironmentState320=0仍验证没有旧negative-WeaponCount误伤。
本Record只覆盖本轮单token增量，不接管SelfCheck整文件的既有迁移改动。
没有production、Scene、资源、schema或生命周期副作用；Part B的只读结论不在本Record记录。

## 实施与验证（追加）

- 事前Scene实测SHA为指定D18E75F7...A2F11。
- 当前状态PLANNED；改后full SelfCheck、两套build、validator与diff范围待取证。

已按前像SHA守卫进行唯一字节token替换`PP == 20`→`PP == 4`；实际源码SHA与事前预测
`BEE7CE5B82EF1A21D007B9093544F12A6B75567EC4B2C9BC00F2B7EA5AC51F72`完全一致。
其他字节（包括换行）、ApplyRecoveryFixture与production全部不变；状态为CODE_WRITTEN，验收待运行。

## 最终验收

### 精确diff

```diff
-                   entity.Health.PP == 20 && entity.ComboCountVic == 0 &&
+                   entity.Health.PP == 4 && entity.ComboCountVic == 0 &&
```

实际整个SelfCheck文件SHA等于事前仅替换该token所得预测SHA，证明其他字节保持不变。
此前已有RED来自Goal2的11:36:59Z运行；本轮没有重复Authority核验或改生产公式。

### Unity与fresh SelfCheck

- 按instance `gameplay-ability-system-for-unity@b1b02287`连接，核验Unity2022.3.62f3、
  active scene NTSD_Battle、非Play/非测试；脚本刷新重载后再核验，无第二Editor启动。
- `2026-09-09T12:03:57Z`通过已有request机制发起full SelfCheck。
- `2026-09-09T12:04:37Z`写入最终FAIL，副本保留
  `Temp/NTSD28_GT06_RecoveryPpExpectation.result`（1231 bytes）。
- GT06所属`CheckGameTickCurrentDatDispatchMatrix()`在RunAllChecksStatic第180行，
  后续`CheckHitResolveSpecialKindContracts()`在第202行；新的停点位于后者调用的既有检查，
  因此GT06已通过，没有将后续失败误写成全量成功。

```text
R4-HIT-02A: actual kind10 tick12 must raw-write frame182 while preserving PN/attacking/wait;
frame=182/182, pn=71, attacking=29, wait=17
CheckKind10And11CharacterStatsWithoutDamage():15195
CheckHitResolveSpecialKindContracts():15032
```

只记录该后续停点，不分析其根因、不改该断言或production。

### Build与Scene

```text
dotnet build Assembly-CSharp.csproj --no-restore -v:minimal -clp:ErrorsOnly
exit 0; 47 warnings; 0 errors; elapsed 00:00:15.57

dotnet build Assembly-CSharp-Editor.csproj --no-restore -v:minimal -clp:ErrorsOnly
exit 0; 129 warnings; 0 errors; elapsed 00:00:11.43
```

Scene SHA前后均为`D18E75F7920E12A1FEB13A8C23949042869E679B420A3073F7C21E4D4F4A2F11`。
未进入Play、未修改或恢复Scene。所有warning按实际输出保留，不修改其他文件消除warning。

### 关闭范围

状态：`VERIFIED / TEST_FIXTURE_ONLY / GT06_PASS / BUILDS_0_ERROR /
SELFCHECK_BLOCKED_LATER_R4_HIT_02A / SCENE_UNCHANGED / GOAL4_USER_HOLD`。
只关闭本包的过时期望，完整SelfCheck与整体战斗对齐仍未完成；本轮后停止等待用户复核。
Part B只读结果只在会话报告，不写入本Record或其他仓库文件。

最终`& ./Tools/Validate-ChangeLedger.ps1`实际exit0：

```text
Change ledger validation PASSED.
  Records: 432
  Governed code files in diff: 374
Warnings: 531
```

WARNING为既有Record声明路径不在当前code diff的提示，未修改其他记录。
本包完成后停止等待用户复核，不启动Goal4。
