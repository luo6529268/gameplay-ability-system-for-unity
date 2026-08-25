# R8-WP01G-R09 — final evidence reconciliation

> 日期：2026-08-24  
> 状态：`COMPLETE AT APPROVED UNITY EVIDENCE / DOCUMENT-ONLY / NO SCRIPT CHANGE`

## Goal

在不新增 gameplay 修复、运行 C++ executable 或把 Unity 证据夸大为 C++ runtime full-trace 证书的前提下，
以当前工作树和 R05～R08-R04 的最新证据为准，对 R8 父编排、68 个 D-ID 总登记册和总计划做一次无遗漏的
最终状态对账，输出 R8 在当前批准范围内的真实结论与仍然存在的边界。

## Scope

1. 逐项核对 `R1-SOURCE-ALL-DIFF-REGISTER.md` 的 68 个 D-ID，确认集合仍为 68、无 missing/extra；
2. 只用已经存在的 source、compile、focused、self-check、joint Play、性能和用户范围决定更新最高证据层；
3. 更正被后续包取代的旧状态，至少审计：
   - `D-LIFE-001` 的 R08 OID7/8→51→7/8 production Play；
   - `D-RENDER-003` 的 R07B pending/generation 与 R08 dormant/split 联合证据；
   - `D-SCHED-008`、`D-STEP-001` 与用户明确排除的 F1/F2 调试步进边界；
   - `R8-SPRITERANGE-001` 是否已由 R08、完整 self-check 和聚焦测试达到更高证据层；
   - `R8-WP01G`、R8 父编排及总计划中仍写成“正在重审/待 R08”的历史文本；
4. 明确区分正常战斗逻辑、用户未来替换的 AI、调试功能键、资源不可达样本、人工硬件验收、T8、IL2CPP、
   Android、服务器和 `R1-WP02` full trace；
5. 如果审计发现新的 source-confirmed、production-reachable、Unity 未实现差异，只登记独立后续 Task，
   本包不修改脚本。

## Authority / Evidence

- 唯一行为权威：`J:\QQFile\NTSD2.4\ntsd_release` 中参与 release 构建的 live battle source；本包只读，
  不运行、构建、修改、复制或向 authority 目录写入；
- 当前 Unity production source、Task/Change Record、focused/self-check、R03～R08 Play 报告与 R8-WP01E
  性能报告只证明各自覆盖范围；
- `R1-WP02` full trace保持 `BLOCKED`，不得用 Unity hash/self-check/Play 替代；
- T8默认 `stage.dat`继续暂缓；AI C++ parity、F1/F2调试步进和IL2CPP按用户决定不进入当前正常战斗主线。

## Files likely involved

- `docs/ai/RESEARCH/R1-SOURCE-ALL-DIFF-REGISTER.md`
- `docs/ai/RESEARCH/R8-WP01G-certification-synthesis-20260823.md`
- `docs/ai/RESEARCH/R8-WP01G-post-ai-non-ai-residual-audit-20260823.md`
- `docs/ai/TASKS/R8-WP01-production-certification-orchestration.md`
- `docs/ai/CHANGE-LEDGER.md`（只在现有 Change 的证据等级确需同步时更新）
- `docs/ai/STATE.md`
- `Assets/NTSD/Docs/cpp-release-vs-unity-battle-realignment-plan.md`
- 本包最终 handoff

## Unknowns

1. R08 对 overlapping sprite-range 的实际路径覆盖是否足以把 `R8-SPRITERANGE-001` 从
   `FOCUSED_TEST_PASS` 升为 `VERIFIED`，需按报告字段与调用路径核验，不能仅因整体 R08 PASS 自动升级；
2. 当前 68 项中是否仍有仅由旧文字造成的伪 pending，需逐项比对最新 evidence；
3. 无 authored state8000 样本、真实人手键盘 edge 和 C++ full trace均可能继续保留为合法未闭合证据，
   不能为了文档整齐而写成 PASS。

## Deliverables

1. 更新后的 68 项 D-ID 状态及数量校验；
2. 更新后的 R8 父编排、WP01G synthesis/residual audit、STATE 与总计划；
3. `docs/ai/RESEARCH/R8-WP01G-R09-final-evidence-reconciliation-20260824.md`；
4. `docs/ai/HANDOFFS/HANDOFF-R8-WP01G-R09-final-evidence-reconciliation.md`；
5. 若存在真实新差异，建立独立后续 Task/Change；若不存在，如实写明“无新可实施 gameplay diff”。

## Verification

- 68 个 D-ID 集合校验：register、reconciliation、missing、extra；
- 所有更新均可追溯到具体 Task/Change/evidence/result，不以记忆或旧摘要裁决；
- `RUNTIME_PENDING/UNKNOWN/INFERRED/BLOCKED/USER-DEFERRED` 不得被自动升级；
- 本包 Unity/C#、scene、config、shader、resource diff 必须为 0；
- 运行 `Tools/Validate-ChangeLedger.ps1`，并检查文档 scoped diff 与 whitespace；
- 不运行 Unity Play、性能、Player 或 C++ executable；已有 fresh R04 compile/self-check 只作为输入证据。

## Stop conditions

- 发现需要修改任何脚本、scene、config、DAT/BMP、shader 或 production asset；
- 发现必须重新运行 Play/性能/Player 才能裁决的项目；
- 需要改变 pass ordering、CentralOnly、容量、30 Hz、FrameInputSet、SoA/ECS、pool/worker/0-GC等批准边界；
- 需要恢复 AI C++ parity、F1/F2调试步进、IL2CPP、T8、Android、服务器或 C++ runtime观察；
- 发现新的 source-confirmed gameplay 差异：只登记后续包，本包停止在文档结论。

## Out of scope

任何脚本或资源修改；新的 Unity 运行；C++ executable/trace 替代方案；AI算法复刻；F1/F2/F7～F9调试功能实现；
T8默认资源；IL2CPP；Android；服务器；声明整个 C++ runtime 已完整对齐。

## Final result（2026-08-24）

- 68项最终分类完成：43/5/1/9/1/3/6，总计68；集合missing0、extra0、duplicate0；
- `D-LIFE-001`与`D-RENDER-003`依据R08/R07B更新为Unity S4；
- F1/F2三项依据用户决定移出normal-combat backlog但保留source difference；
- `R8-SPRITERANGE-001`依据完整七层已有证据升级`VERIFIED`；
- R8父编排、all-diff register、synthesis、residual audit、STATE与总计划已统一；
- 没有发现新的可实施production gameplay first difference，没有建立新脚本Change；
- R1-WP02 full trace、T8、exact DAT witness和其他排除边界继续保留。

本包没有修改或运行Unity脚本/scene/config/resource、Player或C++ authority。

最终验证：register68、reconciliation68、missing0、extra0、duplicate0；Change Ledger validator PASS
（93 records / 111 governed code files）；R09 scoped diff check PASS，仅有既有LF→CRLF提示。
