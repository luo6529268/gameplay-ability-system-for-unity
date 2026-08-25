# R1-SOURCE-007 — 全量盘点闭合、依赖图与分层验收矩阵

> 建立日期：2026-08-21  
> 状态：COMPLETED（静态 source inventory closure；runtime acceptance pending）  
> 类型：R1 只读汇总与验收设计；不修改任何 gameplay。

## Goal

汇总 R1-SOURCE-001 至 R1-SOURCE-006 的 C++ source contract、Unity crosswalk、静态差异、
UNKNOWN 和已批准 Unity adaptation，形成唯一的全量差异清单、跨 pass 依赖图、按子流程拆分的
修复批次与分层验收矩阵。此 Work Package 是 R1 唯一允许进入 R2 gameplay 修改前的收口门。

## Authority / Evidence

- 行为规则只能来自 J:\QQFile\NTSD2.4\ntsd_release 的 release live source；
- R1-SOURCE-001～006 的 research 文档是 source contract 的二次索引，不得覆盖其 C++ 坐标；
- Unity source、历史 C#、self-check、Authority400、checksum、fast-path proof、0 GC 与 1000 AI
  数据仅可标注已有回归/性能证据，不可单独裁决 C++ 对齐；
- R1-WP02 full trace 若仍 BLOCKED，必须如实保留，不能影响 source inventory 的完成判定。

## Scope

- 去重并规范 R1-SOURCE-ALL-DIFF-REGISTER.md 中所有 D-SCHED、D-INP、D-MOV、D-COL、
  D-HIT，以及 SOURCE-005/006 新发现的差异；
- 为每项建立 C++ source coordinate、Unity coordinate、前置状态、读写字段、同 tick consumer、
  已批准 adapter、静态/运行时证据状态、最小 fixture 和后续修复 owner；
- 绘制主调度、input、frame/physics、candidate/hit、CPoint/held/link/opoint、render handoff
  之间的 producer → consumer 依赖；
- 将可安全拆分的修复按 R2～R6 批次排序，明确每批进入条件、Change Record、代码级检查、
  joint fixture 与完成条件；
- 对每个无法从 source 单独验收的子流程，定义“待测试”的依赖链和后续联合验收时机；
- 清楚列出不适用项和 UNKNOWN，不将 Unity capacity/render adaptation 误列为 defect。

## Required Deliverables

1. docs/ai/RESEARCH/R1-SOURCE-007-dependency-graph-and-repair-batches.md；
2. docs/ai/RESEARCH/R1-SOURCE-007-subflow-acceptance-matrix.md；
3. 完整更新 docs/ai/RESEARCH/R1-SOURCE-ALL-DIFF-REGISTER.md；
4. 更新 docs/ai/STATE.md、必要的 DECISIONS 与
   Assets/NTSD/Docs/cpp-release-vs-unity-battle-realignment-plan.md；
5. docs/ai/HANDOFFS/HANDOFF-R1-SOURCE-007-inventory-closure.md；
6. 若且仅若 R1 静态盘点已真正闭合，将 R1 记为“静态 inventory complete / runtime acceptance
   pending”；不得把它写成“Unity 已对齐”；
7. 不创建 Change ID，因为本 Work Package 不改脚本。

## Completion Record

- 2026-08-21：COV-001～006 的 C++ live source contract 与 Unity crosswalk 已逐项闭合；
  COV-007 已汇总全部 D-/A-条目、UNKNOWN、producer->consumer 依赖、future repair batch
  与分层验收。
- 已创建：
  - docs/ai/RESEARCH/R1-SOURCE-007-dependency-graph-and-repair-batches.md；
  - docs/ai/RESEARCH/R1-SOURCE-007-subflow-acceptance-matrix.md。
- 已更新唯一总登记册与覆盖矩阵，使每一个历史“待盘点”状态都变为待处理、待测试、
  已映射、已批准 adapter 或 UNKNOWN。
- 未修改 Unity/C++ gameplay、renderer、scene、resource、tests 或 build；未运行 C++ executable、
  Unity compile、self-check、Play Mode、performance 或 trace。
- 结论只允许为“静态 inventory complete / runtime acceptance pending”；R2 仍须用户确认。

## Static Acceptance Contract

R1-SOURCE-007 结束前必须满足：

1. 覆盖矩阵 COV-001 至 COV-006 的每一个状态都不是“未定义”；
2. 全部已确认差异均有唯一 ID、无重复或互相冲突的描述、无孤立的 source 坐标；
3. 每条差异均能说明为何必须先修 producer、consumer 或同 tick pass ordering；
4. 每个 R2 代码批次是闭合的最小模块：有明确 owner/files、Change ID 前置条件、回归夹具、
   停止条件和不允许的范围扩张；
5. 需要用户 Play Mode 或未来 C++ trace 的项明确排队；不得写成已验收；
6. 中央表现、容量、30 Hz、FrameInputSet、SoA/ECS、worker、pool、zero-GC 目标和 T8 暂缓
   均保留为不可回退边界；
7. 文档能让新的会话不依赖聊天历史，继续任一 R2 子流程而不重启盘点。

## Stop Conditions

- 任意 COV 项仍缺少 C++ live source contract 或 Unity source mapping；
- 需要实现 trace、fixture、replay、gameplay 修复或修改 pass order 才能获得静态结论；
- 需要改变已批准 Unity implementation boundary、长期性能架构或验收标准；
- 用户提出新的 Change Request。

## Out of Scope

- 不修改 C++ / Unity gameplay、tests、DAT、scene、resource、renderer、network 或 ECS；
- 不启动 R2、R3、R4、R5、R6 或服务器阶段；
- 不运行 C++ executable、Unity compile、self-check、Play Mode、性能或 trace；
- 不解决 T8 默认 stage.dat 部署；
- 不将静态盘点的完成误报为战斗逻辑、视觉表现或性能的完整对齐。
