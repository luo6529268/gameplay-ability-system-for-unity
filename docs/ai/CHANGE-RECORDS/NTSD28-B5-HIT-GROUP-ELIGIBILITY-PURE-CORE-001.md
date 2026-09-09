# NTSD28-B5-HIT-GROUP-ELIGIBILITY-PURE-CORE-001

<!-- CHANGE-RECORD
id: NTSD28-B5-HIT-GROUP-ELIGIBILITY-PURE-CORE-001
status: VERIFIED
change-kind: CODE_AND_TEST
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleHitGroupEligibilityResolver.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B5HitGroupEligibilityPureCoreEditorTests.cs
authority: NTSD 2.8-Logan BattleWorld28::candidate_passes_native_group_filter and classify_ordinary_hit_eligibility; EXE B1E13AE1, closure 39DDDA15.
evidence: RED7 plus one test-accessibility correction; focused52 job 9ddc93d0ccfb482ab5b091158c9feb27; B5 585 job a75c3b44cb724581b9c798da16fe2149; NTSD28 1143 job 4086db3a9f4643cd856f6471f73a5f98; build0; SelfCheck PASS 2026-09-06 21:34:24; Console0; Scene dirtyfalse/root13/SHA unchanged; Ledger334/290 PASS.
-->

> 状态：`VERIFIED / PURE_CORE_READY / PRODUCTION_UNCONNECTED`

## Authority与改前职责

- Authority：`BattleWorld28::candidate_passes_native_group_filter`及`classify_ordinary_hit_eligibility`冻结的七步顺序；正式EXE/closure身份见metadata。
- 改前：Unity旧group规则散落在query live reads中，尚无可独立验证且无分配的单一truth-table owner。

## 实际改动

- 新增`BattleHitGroupEligibilityResolver`，仅消费original kind/effect、valid frozen pair snapshot及world mode `+0x18`。
- 顺序闭合invalid、kind4/8/50、state10/13、OID212、group0/state190反转、mode1/3或state18/180 effect例外、target type、type0→type3 opposing-facing。
- 新增52个focused cases，覆盖分支、优先级、effect21/22排除、facing极性及warm 0 allocation。
- 未接producer、consumer、HitPlan、content、Scene或persistent schema。

## 验证

- RED：首次7个缺失类型/符号错误；实现后一次测试参数可访问性错误通过把公开NUnit参数改为`int`修正，production resolver未因此改义。
- focused 52/52；B5 585/585；Unity侧NTSD28 1143/1143。
- build 0 error；SelfCheck PASS；Console 0 error。
- Scene `NTSD_Battle` dirty=false/root13/SHA不变；diff check及Ledger334/290 PASS。

## 风险、未验证项与回滚

- resolver尚未接生产，当前只证明纯规则及行为中性；正式冻结snapshot生产/消费仍由后续atomic package完成。
- 回滚只需移除resolver及专属test/meta；无状态、资源或Scene回滚。
