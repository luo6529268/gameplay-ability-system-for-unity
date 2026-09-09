# NTSD28-GT06-RECOVERY-PP-EXPECTATION-FIXTURE-CORRECTION-001 — Task Contract

> 2026-09-09 Goal 3 Part A；事前建立。当前状态：`VERIFIED / TEST_FIXTURE_ONLY / GT06_PASS / BUILDS_0_ERROR / SELFCHECK_BLOCKED_LATER_R4_HIT_02A / SCENE_UNCHANGED / GOAL4_USER_HOLD`。

## 用户授权与依据

直接采用用户提供、GLM已独立复核的Authority结论，不重新审计战斗规则。
当前正式NTSD2.8-Logan EXE B1E13AE1...D2819033、playable closure39DDDA15...6A46109保持不变。
Authority `source/ntsd28_core/src/simulation/battle_world.cpp`：
`BattleWorld28::advance_native_resources_range`入口2137；2323取`min(current_hp,500)`，
2350–2351对OID51/52将basis减半，2355使用`(500-basis)/100+1`。
Unity `LF2Entity.RunPreCollisionRecoveryPhase`在2842–2844先做HP恢复，2859–2864使用同一basis/
OID51/52与PP公式；`Simulation/Core/NTSDGlobal.cs:120–124`定义HP周期12、PP周期3、cap500、divisor100。
行号以本轮开始源码为准，仅作准确定位，没有修改Authority。

## 原状与唯一变更

`BattleRuntimeSelfCheck.ApplyRecoveryFixture:25507`设置HP100、HPBound101、PP0、WeaponCount-1、
ComboCountVic0、EnvironmentState320=0并调用恢复tick12；当前两个角色OID793/794不适用51/52半basis。
HP先到101，所以普通PP恢复是整数`(500-101)/100+1=4`。
`ExpectRecoveryFixture:25518–25524`仍断言PP20。本包只把该一处`entity.Health.PP == 20`
改为`entity.Health.PP == 4`，保留其他字段、调用、公式、常量与所有其他检查。

## 精确写入范围

- `Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs`，仅上述期望token。
- 本Task及同ID Change Record。
- `docs/ai/CHANGE-LEDGER.md`、`docs/ai/STATE.md`、
  `Assets/NTSD/Docs/ntsd28-logan-vs-unity-battle-alignment.md`，只登记本包状态。
- `Temp/`本包验证结果。

禁止修改production、ApplyRecoveryFixture、恢复公式、其他断言、Scene、Config、Authority、
已有包Record/handoff和用户交接/工具配置文件。Part B只读结论不写仓库。

## 验收、停止与回滚

1. 以instance `gameplay-ability-system-for-unity@b1b02287`核验Unity2022.3.62f3/NTSD_Battle。
   不启动第二个写Library的实例；不得与已有编译/测试/Play争用。
2. 改前以Goal2的2026-09-09T11:36:59Z GT06 FAIL作为已有RED证据，确认源文件前像未改变。
3. 只替换唯一token后运行full SelfCheck，GT06必须通过；后续既有检查若失败，只记录最终停点，禁止修复。
4. 两套Assembly build0 error，运行原始Ledger validator，Scene SHA始终保持
   `D18E75F7920E12A1FEB13A8C23949042869E679B420A3073F7C21E4D4F4A2F11`。
5. 验证文件SHA等于“改前字节仅替换指定token”预测值，增量diff限授权清单。
6. 新的非后续既有检查失败、需要production修改、diff越界或Scene hash无法保持，立即停止报告。
7. 结果如实分级；不把后续SelfCheck FAIL说成全量通过，完成后停止等待Goal4授权。

回滚须用户明确批准，且只逆向本包token与治理增量；不得整文件restore或回退其他包。
本包无资源/schema/lifecycle变更，不新增运行时所有者，无不可逆迁移。

## 执行结果

仅替换指定PP20→4 token，整个源文件SHA与事前预测完全相等。fresh full SelfCheck
12:04:37Z已越过GT06/current-DAT matrix，停在既有后续`R4-HIT-02A`，未继续修复。
两套Assembly build均exit0/0 error，实际warnings为47/129；Scene指定SHA不变。
结果文件为`Temp/NTSD28_GT06_RecoveryPpExpectation.result`，完整命令与证据见同ID Record。
本夹具包已验证，不代表全SelfCheck或production对齐完成；最终validator后停止等待用户复核。
