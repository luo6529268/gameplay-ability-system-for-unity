# R8-TEST-001 — hit writer ShadowCompare vital/stat projection sync

<!-- CHANGE-RECORD
id: R8-TEST-001
status: VERIFIED
code-path: Assets/NTSD/Scripts/Simulation/Ecs/BattleEcsHitExecutionPlan.cs
code-path: Assets/NTSD/Scripts/Test/Editor/BattleHitExecutionPlanEditorTests.cs
authority: R4-HIT-001/R4-HIT-003 C++ release source contracts; diagnostic projection only
evidence: FULL EDITMODE FAILED + FRESH EXACT 2/2 FAILED + MASK DECODED
-->

> 创建日期：2026-08-23
> 最后更新：2026-08-23
> 类型：diagnostic shadow oracle / no production gameplay change

## 1. 状态与范围

- 当前状态：`VERIFIED / DIAGNOSTIC-ONLY`；
- 只同步`BattleEcsHitExecutionPlan` writer-effect projection；
- production `BattleDamageWriter`、candidate/consume/lifecycle、RNG与pass order无授权。

## 2. 已观察事实

- full EditMode job `20fcc884b4114ee9a1a3b7f1667c641c`执行1357项后FAILED；MCP列出的至少25项
  ShadowCompare失败全部为`writerDiff=0x70000000000000`；
- fresh-domain exact job `8d6f29aa8d8043958b29abcf58096e6e`两条converted kind9→kind0 type3用例2/2 FAILED；
- mask解码：bit52 TargetHp、bit53 TargetHpBound/TargetPp、bit54 TargetComboCountVic；
- `BattleDamageWriter`的R4-HIT-001/003 production写入已经存在；旧projection没有相同vital/stat步骤。

## 3. 计划改动

| 符号 | 改前 | 改后 |
|---|---|---|
| `ProjectStandardObjectDamageWriterEffect` | 未投影type1/2/4 adjusted vital/stat | Light/Heavy/Throw按FallDamageDiv投影HP/HPBound/Combo/DamageStat；Drink排除 |
| type3 kind0 projection family | 未投影R4-HIT-001四字段 | standard/state-sync/D1/active-D1统一投影raw injury四字段 |
| kind9 non-converted projection | 不写vital/stat | 保持不变 |
| `ShadowCompare_StandardType3DamageSupportsDeadAirTarget` | 旧断言HP=0 | 起始0、kind0 injury10后断言HP=-10；frame/fall断言不变 |

## 4. 保护边界

- 不改damage、hit、collision、candidate、held/link、opoint、lifecycle、input、AI、render或pool；
- 不修改现有测试期望来掩盖红测；
- 例外仅限整类复跑新暴露、且由R4-HIT-001直接裁决的dead-air HP旧断言；
- 不改C++ authority；
- 不把诊断修复提升为Play Mode或C++ runtime VERIFIED。

## 5. 验收与回滚

- 验收：exact → class → full EditMode → compile → self-check → validator/diff；
- 若出现非vital mask或production与projection仍不一致，回滚本Record内唯一脚本diff并标BLOCKED；
- 回滚不得删除R4-HIT-001/003 production写入。

## 6. 工作树边界

工作树已有大量R2～R7及用户资源/scene修改；仅目标文件的本Change增量属于本包，不回退、格式化或覆盖
其他内容。当前未提交。

## 7. 实际改动

- `ProjectStandardObjectDamageWriterEffect`在旧tail投影前调用专用helper；helper只对Light/Heavy/Throw按
  `FallDamageDiv`计算adjusted injury，并投影HP、HPBound、ComboCountVic及合法1/2 DamageStats；Drink不变；
- standard/state-sync/D1/active-D1四条type3 kind0投影在既有tail前统一投影raw injury四字段；
- non-converted kind9、production writer、测试断言与其他pipeline均未修改；
- fresh compile 0 error/0 warning；exact converted-kind9两条已由2/2 FAIL转为job
  `91c41aff34a746faa4517462e090bda1` 2/2 PASS；
- 整类job `9e09666033394a0b8cdb530135d85da7`执行178项，仅余dead-air旧断言expected0/actual-10；
- 当前先修订Record授权该同合同test assertion，尚未写入该测试修改。

首次单行补丁因文件内有多个相同`target.Health.HP, Is.Zero`文本，误命中约line1702的standard character
fixture；该误改已在任何有效测试证据前原位恢复。job `32394fa4d7d44cf7850d86f685daad5c`仍显示dead-air
expected0/actual-10，因此不计为修复后证据。随后使用方法上下文只修改dead-air断言。

最终focused：dead-air exact job `0bbeee8428f8406bb8f8ee06b09ba9c9` 1/1 PASS；整类job
`69e73f14e34c428eb54803db3327cf85` 178/178 PASS。随后full job
`246be3d87338446ea7a877b13f7f88f5`中本包原25+ hit failures全部消失，但套件最终被无关W07 structural
fixture一项阻塞；该失败fresh exact仍复现并已拆为R8-TEST-002。故本包先记`FOCUSED_TEST_PASS`，待002关闭后
重跑full/self-check再决定最终状态。

R8-TEST-002关闭后，current-worktree full job `6a6336d0e1e94abd9585110358012ca5`为
1357/1357 PASS、0 failed、0 skipped、170.1558765s；随后同域self-check 07:31:17 PASS，强制域重载后
fresh self-check 07:32:39 PASS。validator在代码写入后为56 Records/55 governed files PASS。该证据关闭本
diagnostic/test合同；不把R4-HIT gameplay提升为Play Mode或C++ runtime VERIFIED。
