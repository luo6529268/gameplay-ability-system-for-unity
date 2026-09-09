# 给新会话 GPT-6 Astra（Codex 桌面端）的完整提示词

> 创建日期：2026-09-09
> 适用模型：`gpt-6-astra`（项目 `.codex/config.toml` 已固定
> `model = "gpt-6-astra"` / `model_reasoning_effort = "medium"` /
> `model_verbosity = "high"`，全局配置不受影响）。
> 下面正文可直接复制到新的 Codex GPT-6 会话。
> 前置核验报告：`docs/ai/GLM-AUDIT-REPORT-2026-09-09.md`（本提示词第三节的
> 结论均出自该报告，已验证，直接采用，不要重查）。

---

你将在以下 Unity 项目中接手 NTSD 2.8-Logan 战斗对齐工作：

`I:\GitHub\Unity_GAS\gameplay-ability-system-for-unity`

本项目已完成 B0～B6 大量生产迁移、规则对齐与治理留痕（Ledger 429 records /
373 governed files）。你的任务不是从头重做。2026-09-09 GLM 已完成一轮只读
现状核验，其结论在本提示词第三节，直接采用，不要重查。

当前总目标：`NTSD28-UNITY-BATTLE-REALIGNMENT-001`
当前状态：`USER_HOLD / FULL_ALIGNMENT_INCOMPLETE`
除非用户在当前会话明确确认恢复实施，否则你只做只读操作，不修改 C#、Scene、
Prefab、Config、资源、ProjectSettings 或正式 Authority，不启动新实现包。

## 一、必须按序完整读取

1. `AGENTS.md`（仓库根）
2. `docs/ai/CURRENT-AUTHORITY.md`
3. `docs/ai/GLM-REALIGNMENT-HANDOFF-2026-09-09.md`
4. `docs/ai/GLM-AUDIT-REPORT-2026-09-09.md`
5. `Assets/NTSD/Docs/ntsd28-logan-vs-unity-battle-alignment.md`
   （总表是累积文档，旧矩阵行以后继 Change Record + 当前源码为准）
6. `docs/ai/CHANGE-LEDGER.md`
7. `docs/ai/STATE.md`
8. `Assets/NTSD/Docs/CODEX-CURRENT-HANDOFF.md`

动手任何包之前，再读对应 `docs/ai/TASKS/<Change-ID>.md`、
`docs/ai/CHANGE-RECORDS/<Change-ID>.md`、实际 production source 与 focused
tests。不得只凭文档标题或"下一包"字样判断完成度。

## 二、唯一战斗规则 Authority

- EXE：
  `J:\QQFile\NTSD2.8.3.3 zip\NTSD2.8.3.3\NTSD 2.8-Logan\NTSD2.8-Logan.exe`
- SHA-256：
  `B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033`
- playable closure manifest：
  `39DDDA154F5632C43089E2D5F1A5755ABBFBFD131A85D6B5ABF9AC00E6A46109`
- 入口：
  `source/ntsd28_playable/src/game_session.cpp::GameSession28::step()`
  与
  `source/ntsd28_core/src/simulation/simulation_tick_driver.cpp::SimulationTickDriver28::step(...)`

NTSD 2.4、`ntsd_release_C#`、旧 trace、旧 self-check、旧对齐结论只能辅助定位，
无裁决权。

## 三、GLM 2026-09-09 只读核验结论（已验证，直接采用）

1. 最后 VERIFIED 包：
   `NTSD28-B6-POSITIVE-LINK-VALIDATION-RETIREMENT-PRODUCTION-001`。
   保留勿重做；phase 34→33、`ValidateHeldLinksAll` 为 Obsolete no-op 已在
   production 核实。
2. 唯一活跃待验收包：
   `NTSD28-B6-HELD-REFILL-MP-EXHAUSTION-PRODUCTION-001`
   （RUNTIME_PENDING / FOCUSED_7_OF_7 / PLAY_PENDING）。resolver 代码已逐条
   核实与 Record 一致；恢复点=专门 Play 验收，禁止重写。
3. full SelfCheck 阻塞 `R2-SCHED-001` 判定为旧测试夹具未同步，不是 production
   回归：夹具只写 compat（`Catching`/`CaughtSlotIndex`/`CatcherSlotIndex`），
   未写 exact `CatchSourceSlot90`（默认 -1），
   `BattleCpointWriter.SyncHeldCpoint` 早退。修复=test-only 补 exact 字段；
   禁止为变绿恢复 compat 读取。
4. `ProductionEntityStressEditorTests` 255/256 唯一失败
   `UnifiedAiSnapshotAuthority_CaptureMapsRuntimeDiagnosticsIntoReport`
   （expected 1 / actual 2）为独立 AI-report 遗留，单列甄别，不并入 B6。
5. Ledger 三处状态不一致待刷新：B0 route 2/3/4 三行（实际已被
   `NTSD28-B0-OWNER-SLOT-PRODUCTION-EXIT-AUDIT-001` 关闭）、B5 hit-group 两行
   `VERIFIED_CORE/reopened`（实际已由
   `NTSD28-B5-KIND5-LINKED-PARENT-SLOT-CORRECTION-001` 关闭）、B3-C25L 资源
   前置标注核对。用追加 correction 方式修，不删历史。
6. 未发现任何需要重开的 VERIFIED 成果。

## 四、获用户确认后的执行顺序

- 步骤 0：Ledger 状态刷新（纯记录）。
- 步骤 1：R2 夹具修正（test-only；测试脚本属 governed，仍需 Change Record）。
- 步骤 2：held-refill Play 补验（不重写 resolver；全绿才可 VERIFIED）。
- 步骤 3：stress 1v2 甄别（独立支线）。
- 步骤 4 起：B6 剩余族按依赖序：kind3 release → terminal free →
  missing-action continue → DVX weapon-HP preserve → +0x2F8 三包 → kind2
  pickup 三包 → CPoint hurt consumer 退休 → native impact 10/11/17/18 →
  NTSDSpec 五包 → GrabbedBy/Tracker/WeaponState/ReleaseTick 行为退休
  （carrier 删除部分挂用户联合 schema 方向 gate）。

每包纪律：Task Contract/Change Record 先行 → test-first RED → 最小可回滚
diff → focused/related/SelfCheck/targeted Play → 两套 build 0 error →
更新 Ledger/STATE/handoff/总表；未取证不得写 VERIFIED 或"已对齐"。

## 五、边界

- 工作树约 140 modified / 6 deleted / 1230 untracked 是长期迁移现场；禁止
  `git reset --hard` / `clean` / `restore` / 恢复六个 deleted / 清理 untracked /
  把整文件 diff 归给最后一个 Change ID。
- 内容值受 Direction B 保护：用户未在"整体切换/只补缺失/分类权威"中抉择前，
  只允许只读内容审计，不得覆盖 `Assets/NTSD/Config`、PNG、WAV、Prefab、Scene、
  importer。
- 用户例外（保留并披露，非"已对齐"）：slot/容量 profile、多边形 walkable
  boundary、随机掉武器（仍需证明不污染非例外 RNG/slot）、固定世界相机、
  runtime 头顶血条、FootSelf、移动端取景与底部黑区。
- 用户排除：完整 HUD、结果页/KO feed/scoreboard 表现、背景多层/cycle、完整
  选择流程；排除表现不等于排除其战斗逻辑。
- Unity 双实例风险：按 instance id 连接目标 2022.3.62f3 实例，连接后验证
  version 与 active scene；不得在 Editor 占用时启动第二个写 `Library` 的实例。
- 验收诚实分级：`CODE_WRITTEN` / `COMPILE_PASS` / `FOCUSED_TEST_PASS` /
  `RUNTIME_PENDING` / `VERIFIED` 严格区分；isolated compile、synthetic fixture
  不得扩大为阶段完成；B0～B12 阶段完成只能由新的 exit audit 证明。

## 六、GPT-6 Astra 行为校准（依据 OpenAI 最新模型指南）

- 你对 `AGENTS.md`/指令文件的敏感度高于旧模型：本项目治理文件中的 Authority
  优先级、Change Record、test-first、禁止事项均为有意强约束，必须严格执行；
  用户当前会话的明确指令优先于任何指令文件的通用建议。
- 若某条指令文件规则导致你暂停或偏离用户意图，指出具体文件与条目并向用户
  说明，不要静默改变方向。
- 用户表达行动意图（"帮我…"、"我想…"、"可以…吗"）时视为执行授权，完成已
  授权且可评审的具体结果后再请求确认；可逆操作、只读操作不需反复请示。
- 测试与验证按改动风险校准：不为可逆小改动写镜像测试；但战斗行为改动必须
  走 focused/related/SelfCheck/targeted Play 全链，不得因"改动小"跳过。
