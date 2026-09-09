# 给新会话 GLM 的完整提示词

下面正文可直接复制到新的 GLM 会话。

---

你将在以下 Unity 项目中接手 NTSD 2.8-Logan 战斗对齐工作：

`I:\GitHub\Unity_GAS\gameplay-ability-system-for-unity`

你的第一阶段任务不是从头重新规划或重新实现，也不是假定旧工作无效。项目已经完成了大量 B0～B6
生产脚本迁移、测试和治理记录。你必须先核验当前实际进度，阅读已经处理好的 production 逻辑和证据，
确认哪些成果可以保留，再整理真正遗漏、尚未处理、只写未验、证据过期或文档状态不一致的项目。

当前总目标是：

`NTSD28-UNITY-BATTLE-REALIGNMENT-001`

目标最终仍是按 B0→B12 完成 NTSD 2.8-Logan 到 Unity 的非例外战斗域对齐，但当前处于：

`USER_HOLD / FULL_ALIGNMENT_INCOMPLETE`

在我确认你的现状核验和增量续接清单前，只做只读审计，不修改 C#、Scene、Prefab、Config、资源、
ProjectSettings 或正式 Authority，不启动新的 implementation package。

## 一、必须先读

必须按顺序完整读取：

1. `AGENTS.md`
2. `docs/ai/CURRENT-AUTHORITY.md`
3. `docs/ai/GLM-REALIGNMENT-HANDOFF-2026-09-09.md`
4. `Assets/NTSD/Docs/ntsd28-logan-vs-unity-battle-alignment.md`
5. `docs/ai/CHANGE-LEDGER.md`
6. `docs/ai/STATE.md`
7. `Assets/NTSD/Docs/CODEX-CURRENT-HANDOFF.md`

随后读取与你发现的当前活跃、RUNTIME_PENDING、最新 VERIFIED 和关键阶段 exit 相关的
`docs/ai/TASKS/` 与 `docs/ai/CHANGE-RECORDS/`。不要只凭文件名或顶部摘要判断完成度。

## 二、唯一 Authority

战斗规则唯一 Authority 是：

- 正式 EXE：
  `J:\QQFile\NTSD2.8.3.3 zip\NTSD2.8.3.3\NTSD 2.8-Logan\NTSD2.8-Logan.exe`
- SHA-256：
  `B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033`
- playable C++/header closure manifest：
  `39DDDA154F5632C43089E2D5F1A5755ABBFBFD131A85D6B5ABF9AC00E6A46109`
- 正式入口：
  `source/ntsd28_playable/src/game_session.cpp::GameSession28::step()`
  和
  `source/ntsd28_core/src/simulation/simulation_tick_driver.cpp::SimulationTickDriver28::step(...)`

NTSD 2.4、`ntsd_release_C#`、旧 trace、旧 self-check、旧对齐结论、debug probe、候选 EXE 或未进入
playable closure 的代码不能裁决当前规则，只能辅助定位。

## 三、你的审计原则

1. 以当前 worktree 和实际 production source 为事实，不能假设已有代码都没做，也不能只信历史摘要。
2. 对每个已标记 VERIFIED 的包：
   - 找到实际修改符号；
   - 确认生产调用链仍在；
   - 确认后继修改没有覆盖或绕开它；
   - 核对测试证据覆盖了对应合同；
   - 没有反证时保留，不得重新实现。
3. 只有新鲜源码或 Authority 证据证明已有实现错误时，才把它列为“需要 correction”；不能因为你会用
   不同架构就重写。
4. 对每个 RUNTIME_PENDING 或 CODE_WRITTEN 包，明确区分：
   - 逻辑已经写好，仅缺 Play/trace/验收；
   - 测试未真正运行；
   - 依赖未满足；
   - production 仍不完整。
5. 对总表中的旧 `CONFIRMED_DIFFERENCE`、`REBASELINE_REQUIRED` 或“下一包”文字，必须与当前源码和
   后继 Change Record 交叉核对；旧行可能尚未被逐行更新，不能据此重做已完成包。
6. 不得把“没有搜到更多差异”当作完成；也不得把 isolated compile、synthetic test 或单个 focused test
   扩大为阶段完成。
7. 阶段完成只能由新的 B0～B12 exit audit 证明。

## 四、先检查的当前边界

暂停前最后一个 VERIFIED 包：

`NTSD28-B6-POSITIVE-LINK-VALIDATION-RETIREMENT-PRODUCTION-001`

它已经完成并有真实 Play/测试证据，不要重做：

- 正式 tick 删除 Authority 不存在的 `HeldLinkValidation`；
- phase occurrence 34→33；
- post-catch 直接 `PreInteraction -> StageBounds -> HeldProcess`；
- World/stress/U6/W07 positive validation owner 退休；
- obsolete compatibility entry 为无写入、无 event、0B no-op；
- lifecycle cleanup、negative held preserve 和 AI relation projection 保持。

唯一已经写但未完成验收的活跃包：

`NTSD28-B6-HELD-REFILL-MP-EXHAUSTION-PRODUCTION-001`

当前状态：

`RUNTIME_PENDING / FOCUSED_7_OF_7 / C09_RELATED_2_OF_2 / BUILDS_0_ERROR /
PLAY_PENDING / SELFCHECK_BLOCKED_UNRELATED_R2_CPOINT_SYNC /
HP_BASEMAX_DEFERRED / USER_HOLD`

不要重写它。先核对已有 resolver 和测试；如果后续获准恢复，原则上应从缺失的专门 Play acceptance 继续。

已知独立遗留：

- full SelfCheck 当前停在
  `R2-SCHED-001: T14 CPoint sync must still execute after candidate consumption`；
  旧 fixture 可能只写 compatibility catch slots，而当前 production 使用 exact catch relation fields。
  必须先核对 Authority，不能为了变绿恢复旧行为。
- `ProductionEntityStressEditorTests` 为 255 pass / 1 fail，唯一失败
  `UnifiedAiSnapshotAuthority_CaptureMapsRuntimeDiagnosticsIntoReport`，
  expected 1 / actual 2。必须独立分类，不要混入 B6 修复。

## 五、工作树安全

当前工作树约有：

- 140 个 tracked modified；
- 6 个 tracked deleted；
- 1230 个 untracked；
- 总计约 1376 条 status。

这是长周期迁移的综合工作树。untracked 中包含大量正式 Task、Change Record、测试和 manifest；六个删除项
包含用户要求不恢复的旧 campaign。禁止：

- `git reset --hard`
- `git clean`
- `git restore`
- 恢复旧 NTSD 2.4/C# 文档
- 批量格式化或清理未知文件
- 把整文件 diff 都归给最后一个 Change ID

先使用 `docs/ai/CHANGE-LEDGER.md` 理解累计文件所有权。

## 六、内容和资源边界

战斗规则 Authority 已是 NTSD 2.8-Logan，但内容值暂受 Direction B 保护。用户最终要求处理 Unity 与
2.8 的 DAT、数值、PNG、WAV 和引用差异，但尚未选择：

1. 整体切换；
2. 只补缺失；
3. 分类权。

用户决定前，只能做内容 catalog、引用图和 normalized projection 的只读审计，不能覆盖
`Assets/NTSD/Config`、PNG、WAV、Prefab、Scene 或 importer。

## 七、用户例外和排除

必须保留并最终披露的 Unity 例外：

- Unity slot/容量 profile；
- 多边形 walkable boundary；
- 当前 Unity 随机掉武器；
- 固定世界相机；
- runtime 头顶血条；
- FootSelf；
- Android/iOS 移动端取景和底部黑区。

用户排除：

- 完整 NTSD 2.8 HUD；
- 完整结果页、KO feed、scoreboard 表现；
- background DAT 多层/cycle；
- 完整角色/模式/背景/BGM 选择流程。

排除表现不等于排除其战斗逻辑。

## 八、第一阶段必须交付给我的内容

先不要改代码。请输出一份“现状核验与增量补漏报告”，至少包含：

1. B0～B12 每阶段：
   - 已验证且生产仍有效的成果；
   - 已写但待验；
   - 文档声称完成但代码/证据不足；
   - 文档仍写未完成但代码其实已经处理；
   - 真正遗漏；
   - 尚未开始；
   - 用户决策或跨阶段依赖；
   - 阶段 exit 尚缺的证据。
2. 当前 production 调用链与 Authority 的最早未闭合 first difference。
3. 所有 active/RUNTIME_PENDING 包及准确恢复点。
4. 所有已知 SelfCheck、stress、Play、trace、compile 阻塞，按“行为缺陷 / 旧夹具 /
   独立工具问题 / 外部环境”分类。
5. 一张增量依赖 DAG：
   - 已完成节点必须标为保留；
   - 待验节点优先补验，不重写；
   - 真实遗漏才创建新实现包；
   - 内容策略相关节点受用户决策 gate。
6. 建议的后续执行顺序，每一步说明：
   - 为什么是最早依赖已满足项；
   - 会读取哪些 Authority 函数；
   - 会检查哪些现有 Unity 符号；
   - 是否只需验收，还是确实需要修改；
   - 验收命令和回滚边界。
7. 明确列出哪些旧成果你认为可能有问题；每项必须给出代码或 Authority 证据，不能只给主观判断。

## 九、确认后才允许实施

我确认你的报告后，再从最早真实缺口或最早待验包继续。届时：

- 修改任何脚本前先创建或恢复准确 Task Contract / Change Record；
- test-first；
- 最小、可回滚；
- 不扩大范围；
- compile、focused、SelfCheck、必要 Play、joint trace 分级报告；
- 每次更新 Ledger、STATE、handoff 和总表；
- 未取得对应证据时不得写 VERIFIED 或“已对齐”。

你的第一条回复应先给出现状核验方法、你发现的最新有效恢复点和你准备检查的文件清单，然后开始只读审计。
不要从 B0 重新实现，不要删除或替换已有成果。

---

