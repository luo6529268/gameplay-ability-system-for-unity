# GOVERNANCE-NTSD28-GOAL1-LEDGER-STATUS-CORRECTION-001

<!-- CHANGE-RECORD
id: GOVERNANCE-NTSD28-GOAL1-LEDGER-STATUS-CORRECTION-001
status: VERIFIED
change-kind: GOVERNANCE_ONLY
code-path: NONE
authority: 用户于2026-09-09仅解除Goal 1的USER_HOLD，授权按既有闭合Change Record追加Ledger状态correction；直接采用GLM已确认结论，不重新审计production。
evidence: 已完整读取B0 owner-slot production exit、B5 kind5 linked-parent correction及B3 C25l state18 particle owner的Change Records，并读取两份B5旧Record的2026-09-08 correction。Ledger已追加限定范围的有效状态与闭合证据链接，旧行和旧Record原文保留；B3维持RUNTIME_PENDING/RESOURCE_SPAWN_PENDING。治理结论不新增或扩大运行时证据；validator结果随后追加。
-->

## 授权、范围与不变量

本包只有治理记录变更：`docs/ai/CHANGE-LEDGER.md` 与本Record。没有代码路径。
现有未提交修改与未跟踪文件属于既有工作；本包不接管整文件历史diff。
不修改STATE、handoff、旧Change Record、GLM报告、continuation prompt或工具配置。
用户本轮精确文件清单优先于通用多文档同步规则；Goal 1以外仍为USER_HOLD。

## 追加correction的依据

- B0 direct self、F8 owner99、ordinary OPoint owner三条旧行的待验状态由
  [NTSD28-B0-OWNER-SLOT-PRODUCTION-EXIT-AUDIT-001](NTSD28-B0-OWNER-SLOT-PRODUCTION-EXIT-AUDIT-001.md)
  合并关闭。准确证据：`VERIFIED / OWNER_TRACE_EQUAL_15_RECORDS_135_FIELDS /
  DOUBLE_RUN_BYTE_STABLE / TARGETED_PLAY_PASS / ROUTES_1_TO_4_REGRESSION_32_OF_32 /
  UNITY_RUNTIME_COMPILE_PASS / OUT_OF_PROCESS_COMPILE_PASS / B0_OWNER_PRODUCER_EXIT_READY`。
  Full SelfCheck当时仍被无关CPoint阻塞；不扩大为完整B0、B8物理F8或全战斗parity。
- B5 hit-group exit与atomic integration的2026-09-08 HolderCopy binding重开项由
  [NTSD28-B5-KIND5-LINKED-PARENT-SLOT-CORRECTION-001](NTSD28-B5-KIND5-LINKED-PARENT-SLOT-CORRECTION-001.md)
  关闭。准确证据：`VERIFIED / RED_0_OF_5 / FOCUSED_5_OF_5 / RELATED_280_OF_280 /
  TARGETED_PLAY_5_CASES / BUILDS_0_ERROR / SELFCHECK_BLOCKED_UNRELATED /
  SCENE_UNCHANGED / EXACT_LINKED_PARENT_BOUND / HOLDERCOPY_UNCHANGED`。
  只恢复到linked-parent binding已纠正的范围；不宣称HolderCopy carrier/schema或整个B5关闭。
- [NTSD28-B3-C25L-STATE18-PARTICLE-OWNER-001](NTSD28-B3-C25L-STATE18-PARTICLE-OWNER-001.md)
  明确尚未做OID999资源存在时7/1粒子、最低slot、四次tuple和同tick newborn Play。
  `RUNTIME_PENDING / PROGRAMMATIC_ORDER_VERIFIED / RESOURCE_SPAWN_PENDING`仍准确；原行不改。

以上均为继承的闭合记录证据，不是本轮重跑所得。

## 验收与回滚

验收：新增correction可唯一链接上述证据；旧Ledger全部历史文字与旧行保留；B3原行不变；
执行`& ./Tools/Validate-ChangeLedger.ps1`并在此追加实际输出。
回滚仅能在用户明确批准后逆向本轮新增的correction区块与Record，不回退任何旧成果或用户内容。

## 实际文件与状态

- `docs/ai/CHANGE-LEDGER.md`：追加当前correction区块，覆盖五个旧行的过时待办解释，并记录B3核对不变。
- 本Record：保存授权、证据映射、限定验收与回滚边界。
- 状态`VERIFIED`仅表示上述纯治理映射已按现有记录核对，不是新的战斗运行时认证。

## A项 validator 实测

2026-09-09执行`& ./Tools/Validate-ChangeLedger.ps1`，exit `0`：

```text
Change ledger validation PASSED.
  Records: 430
  Governed code files in diff: 373
```

输出同时保留旧Record声明路径不在当前diff的WARNING；未为消除历史warning改动其他文件。
