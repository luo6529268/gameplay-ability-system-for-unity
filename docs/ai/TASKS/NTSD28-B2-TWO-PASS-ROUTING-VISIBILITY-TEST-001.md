# Task Contract — NTSD28-B2-TWO-PASS-ROUTING-VISIBILITY-TEST-001

> 状态：`VERIFIED / TWO_PASS_EXPECTATION_CORRECTED / TEST_ONLY`  
> 所属计划：`NTSD28-UNITY-BATTLE-REALIGNMENT-001 / B2`  
> 建立日期：2026-09-03

## 目标

更正旧Candidate测试对同tick routing→later producer可见性的过时期望，使其与已实施的2.8
all-producer→all-routing两遍合同一致；不改production。

## 允许修改

- `Assets/NTSD/Scripts/Test/Editor/AiSensingSoACandidateEditorTests.cs`
- 本Task、Change Record、Ledger、STATE、handoff、总表。

## 验收

- 测试名称与断言明确routing mutation不泄漏回同tick later producer；
- shell最终routing结果frame6/state14仍成立；slot1保留producer时看到的cached slot0；
- 单测、完整AI sensing/decision相关组、SelfCheck与Console通过。

## 回滚

恢复旧测试名称/期望；production无改动。

## 结果

旧期望已按2.8 two-pass更正；最终AI sensing/decision/shadow 297/297、SelfCheck PASS、Console0。
