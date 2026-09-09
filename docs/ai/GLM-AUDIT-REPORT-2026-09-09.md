# NTSD 2.8-Logan → Unity 对齐：GLM 现状核验与增量补漏报告

> 审计日期：2026-09-09
> 总目标：`NTSD28-UNITY-BATTLE-REALIGNMENT-001 / USER_HOLD / FULL_ALIGNMENT_INCOMPLETE`
> 审计性质：只读。未修改任何 C#、Scene、Prefab、Config、资源、ProjectSettings 或正式 Authority。
> 本报告为 GLM 只读核验的落盘版本；配套新会话提示词见
> `docs/ai/GPT6-CONTINUATION-PROMPT-2026-09-09.md`。
> 用户确认本报告前，维持 USER_HOLD，不启动任何实施包。

## 0. 审计方法与证据基础

- 按指定顺序完整读取：`AGENTS.md`、`docs/ai/CURRENT-AUTHORITY.md`、
  `docs/ai/GLM-INCREMENTAL-CONTINUATION-PROMPT-2026-09-09.md`、
  `docs/ai/GLM-REALIGNMENT-HANDOFF-2026-09-09.md`、
  `Assets/NTSD/Docs/ntsd28-logan-vs-unity-battle-alignment.md`（关键段）、
  `docs/ai/CHANGE-LEDGER.md`（391 行状态扫描；validator 口径 429 records / 373
  governed files）、`docs/ai/STATE.md`、`Assets/NTSD/Docs/CODEX-CURRENT-HANDOFF.md`。
- 全文读取两个关键包 Change Record：
  `NTSD28-B6-POSITIVE-LINK-VALIDATION-RETIREMENT-PRODUCTION-001`、
  `NTSD28-B6-HELD-REFILL-MP-EXHAUSTION-PRODUCTION-001`。
- production 符号逐点核验（非仅文档标题）：`BattleTickPhase.Count=33`、
  `ValidateHeldLinksAll` Obsolete no-op、`LF2WeaponHeldStateResolver.ProcessDrinkConsumption`
  逐分支、`SimulationConstants` 33ms/3ms、OPoint/F8 owner 写入、
  `CatchSourceSlot90` 生产者/消费者链、R2 夹具与 stress 失败测试源码。
- 四态判定法：已处理（保留）/ 只写待验 / 真正遗漏 / 尚未开始，一律以
  "文档 × 当前源码 × 正式调用链 × 测试证据"四方交叉，不以文档标题单独定论。

## 1. 核验结论摘要

1. 暂停前恢复点确认无误：最后 VERIFIED 包 =
   `NTSD28-B6-POSITIVE-LINK-VALIDATION-RETIREMENT-PRODUCTION-001`；唯一活跃待验收包 =
   `NTSD28-B6-HELD-REFILL-MP-EXHAUSTION-PRODUCTION-001`。两者 production 代码均
   仍在正式调用链且与 Change Record 一致，无需重做。
2. full SelfCheck 的 `R2-SCHED-001` 阻塞判定为旧测试夹具未同步，不是 production
   回归（证据见第 5 节）。修复方向是 test-only 补 exact 字段，不得为变绿恢复
   compat 读取。
3. `ProductionEntityStressEditorTests` 255/256 唯一失败
   `UnifiedAiSnapshotAuthority_CaptureMapsRuntimeDiagnosticsIntoReport` 分类为独立
   AI-report/diagnostics 遗留，与 B6 无关，单列队列。
4. 未发现任何具备 Authority 或当前代码反证、需要重开的 VERIFIED 成果。
5. Ledger 存在三处状态不一致（纯记录问题，非实现问题），见第 3 节。

## 2. B0～B12 阶段现状

图例：✅ 已验证且 production 仍有效（保留）｜🟡 已写待验｜📄 文档与代码/证据状态
不一致｜🔴 真正遗漏（owner 已冻结、production 未实施）｜⬜ 尚未开始｜🔐 用户决策门槛

### B0 基础/字段/owner（大体闭合，个别边缘待验）

- ✅ 保留：target +0x3F8 去混淆（`PickerStableId` 复用）、direct/stage self-owner、
  F8 owner=99（`BattleRandomWeaponDropModule.cs` 已核实）、ordinary OPoint owner 传播
  （`BattleLogicObjectPointRuntime.ProcessOneLateOpoint` 内
  `task.ownerEntityIndex = spawner.OwnerEntityIndex` 已核实）、owner-slot 五路由联合
  exit trace 15 records/135 fields 零首差、direct revival defaults 1/0/0。
- 🟡 文档状态滞后：Ledger 中 B0 route 2/3/4 三行仍写
  `FOCUSED_TEST_PASS/RUNTIME_PENDING`，其实质 Play/trace 验收已由
  `NTSD28-B0-OWNER-SLOT-PRODUCTION-EXIT-AUDIT-001`（VERIFIED，
  `ROUTES_1_TO_4_REGRESSION_32_OF_32 / TARGETED_PLAY_PASS`）合并关闭。
  属"文档仍写未完成但代码/证据已处理"，建议状态刷新而非重验。
- DAT `<weapon_piece>` parser/materializer 归 B7（已声明，非本阶段遗漏）；
  hit_Fa8/9/13 独立未知。
- 阶段 exit：owner 家族已 exit；B0 整阶段无单独总 exit audit 记录。

### B1 Host/cadence（合同已写，阶段 exit 未做）

- ✅ 保留：`SimulationConstants` 33ms/3ms、SIM_DT/FAST_SIM_DT、2-interval 排空上限、
  F1/F2/F5 host control（`SimulationTickHostPolicy`/`SimulationTickDriver` 在链）。
- 🟡 AGENTS.md 明确 host control 在真实 Play cadence/物理按键验收完成前只能报告
  `RUNTIME_PENDING`——真实物理按键全流程验收未闭合。
- 阶段 exit 缺：B1 无 fresh exit audit（长时间无 drift 时间线验证）。

### B2 输入/AI（核心家族已验证，无单独阶段门）

- ✅ 保留：AI owner guard 家族（joint trace 8/48 零首差 + 双跑稳定 + targeted Play）、
  输入 cadence、native history、proxy two-pass、双 RNG/call-site 家族。
- 阶段 exit 缺：B2 无单一阶段 exit gate 记录。

### B3 主 pass 骨架/placement（placement exit ready，full close 后置）

- ✅ 保留：33 phase occurrence 骨架与 C02～C25 placement 系列；state501 退休、
  1100..1299 child propagation 退休（RED 证据在案）。
- 🟡 真正待验一项：`NTSD28-B3-C25L-STATE18-PARTICLE-OWNER-001 / RUNTIME_PENDING /
  RESOURCE_SPAWN_PENDING`——state18/19 粒子分支已写且程序化顺序已验，资源 spawn
  部分待资源前置（跨阶段依赖）。
- full close 明确后置于 B6/B7/B8 规则与 tail。

### B4 帧运动/物理/复活（revival exit 已闭合）

- ✅ 保留：frame motion、teleport、physics 多分支、state12/18 transaction、revival
  全链（gate→queued→normal floor/RNG→exit audit 13/416 零首差双跑稳定）。
- 后置：OPoint/revive producer 归 B7/H；schema 归联合方向。

### B5 碰撞/命中/伤害/统计（placement exit ready）

- ✅ 保留：exit gate audit（`B5_PLACEMENT_EXIT_READY / B5_FULL_CLOSE_DEFERRED`）及
  数十个 VERIFIED 家族（candidate/filter、type0~6、armor、reduced、resource、rest、
  combo、KO、+0x2F4、negative environment recovery、legacy stats 退休等）。
- 📄 状态不一致一项：`NTSD28-B5-HIT-GROUP-ELIGIBILITY-EXIT-AUDIT-001` 与
  `...ATOMIC-PRODUCTION-INTEGRATION-001` 两行
  `VERIFIED_CORE / LINKED_HOLDER_BINDING_CORRECTION_PENDING / FAMILY_EXIT_REOPENED`——
  其待办的 HolderCopy 绑定纠正已由
  `NTSD28-B5-KIND5-LINKED-PARENT-SLOT-CORRECTION-001`（VERIFIED）实质关闭。
  建议：补一次轻量家族复核或 Ledger correction 链接，不需重实现。
- full close 后置于 B6/B7/B8/B10/B11/H 与最终 joint trace。

### B6 抓取/持有/武器（主链基本闭合，剩余为已审计未实施族）

- ✅ 保留（按严格链序）：CPoint throw 精确子集 → entity-link lifecycle cleanup →
  invalid reciprocal preserve → catch relation exact fields → mixed advance/exact
  consumer → settlement vaction preflight → held injury accounting/cover →
  caughtact event → positive-link retirement（最后）。全部 VERIFIED 且 production
  符号在链（本次抽检 `CatchSourceSlot90` 生产者/消费者、phase 33、Obsolete no-op
  均确认）。
- 🟡 唯一活跃待验：`NTSD28-B6-HELD-REFILL-MP-EXHAUSTION-PRODUCTION-001`——
  production 已核实与 Record 逐条一致（OID123 无 HP<=0 早退；
  `OrdinaryCreditGate2F4>=0` 只 cap child PP 不读 KillCount；单次
  `BattleRandInt(0,7)-3` 写 Vx、Vy=0、不写 Runtime.Vz、`PS.zz=0` 落地）。focused
  7/7、C09 2/2、builds 0 error 已真实运行。缺：专门 OID122/123 refill/exhaustion
  Play 验收 + 包级 Ledger/Scene/Console 闭环。
- 🔴 真正遗漏（owner audit 已 VERIFIED、production HELD，按序排队的真实缺口）：
  1. WPoint kind3 release（`RunStep12` DVX→kind3 continuation + `DropRandomly` 双分支）
  2. held terminal structural free（refill 后、pose 前的无 RNG despawn→`StructuralWriter.Free`）
  3. WPoint missing-action write+continue
  4. non-kind3 DVX weapon-HP preserve（移除 `OnThrown` 重置）
  5. DVX excluded-group +0x2F8 carrier/writer/AI consumer 三包
  6. kind2 pickup relation 三包（carrier/rules→pure→atomic）
  7. CPoint kind2 hurt-action consumer 退休 + 27-scalar versioned schema（后者内容门槛）
  8. native impact kind10/11/17/18 三包
  9. NTSDSpec 五包（mass、compat weapon selector、dead flute API、B9 oscillate、empty shell）
  10. GrabbedBy / Tracker / WeaponState / ReleaseTick 退休包（carrier/快照删除部分挂
      联合 schema 13/20/23 用户方向）
  11. OID122 authoritative baseMax/HP clamp（归 B11/H）
- ⬜ held/broken weapon lifecycle 与跨阶段 world-KO feed（依赖 B7/B8）。
- 阶段 exit 缺：B6 无整体 exit audit。

### B7 OPoint/Spawn/Lifecycle/Birth visibility — ⬜ 尚未开始

仅有前置能力（OPoint owner、lifecycle cleanup、pool/generation）散在 B0/B6 包中。
DAT weapon_piece、升序 slot 扫描"高当 tick 低下 tick"、newborn suppression、十一阶段
有序关闭实施等全部未开始。

### B8 Stage/BattleFlow/Results 逻辑 — ⬜ 尚未开始

两次 depth clamp 终局、非例外 type/mode/offstage removal、battle end timer
80/101/350、结果 transition 逻辑（表现被用户排除、逻辑没有）均未开始。

### B9 Presentation — ⬜ 未开始（个别前置：C01 spark 唯一 writer 已 VERIFIED）
### B10 Audio — ⬜ 未开始
### B11 内容/数值/资源 — 🔐 用户决策门槛

整体切换/只补缺失/分类权威三选一未决；Direction B 保护中，仅允许只读
catalog/projection 审计。

### B12 最终 parity campaign — ⬜ 未开始（依赖 B7~B11 全部闭合）
### H（正式 combo tuple 激活等 schema 方向）— 🔐 与联合 schema 13/20/23、
ReleaseTick/WeaponState/Tracker carrier 删除共同挂用户方向

## 3. 活跃/RUNTIME_PENDING/关键包恢复点与 Ledger 状态

| 包 | 状态 | 准确恢复点 |
|---|---|---|
| `B6-HELD-REFILL-MP-EXHAUSTION-PRODUCTION-001` | RUNTIME_PENDING/USER_HOLD | production+focused 7/7+C09 2/2+builds 0 error 已有；从专门 Play acceptance 恢复，不重写 resolver |
| `B6-POSITIVE-LINK-VALIDATION-RETIREMENT-PRODUCTION-001` | VERIFIED | 保留；唯一后置动作已完成（C09 索引 28→27 修正后 9/9） |
| `B3-C25L-STATE18-PARTICLE-OWNER-001` | RUNTIME_PENDING | 程序化顺序已验；resource spawn 等资源前置 |
| `SIMULATION-DIRECTORY-REORGANIZATION-001` | BLOCKED（外部） | 实施完成、manifest/GUID 保持；仅全局 ledger 外部问题 |
| `BATTLE-CENTRAL-RUNTIME-FOOTSELF-001` | CODE_WRITTEN | presentation-only、用户例外关联；Unity 编译/focused 均未跑，低优先 |
| B0 route 2/3/4 三行 | Ledger 写 FOCUSED_TEST_PASS | 实质已被 owner-slot exit audit 关闭；只做状态刷新 |

Ledger 状态扫描（391 行匹配）：279 VERIFIED / 85 FOCUSED_TEST_PASS / 15
ANALYSIS_COMPLETE / 5 SUPERSEDED / 2 RUNTIME_PENDING / 2 BLOCKED / 2 VERIFIED_CORE /
1 CODE_WRITTEN。三处待刷新：B0 route 2/3/4 行、B5 hit-group 两行、B3-C25L 的
资源前置标注核对。

## 4. 最早未闭合 first difference（live path）

按权威 tick 顺序与已完成链：B6 held/WPoint 域内、已实施链之后最早的正式可达首差是
held-refill 包本身（代码已写、Play 未验，不能算闭合）；其验收闭合后，下一个已冻结
首差是 WPoint kind3 release（Authority DVX 后继续 kind3，Unity 提前 return；current
held-union kind3=811 可达）。验证面最早的未闭合点是 full SelfCheck 的 R2-SCHED-001
旧夹具（见下节，非行为首差）。

## 5. 失败/阻塞分类（五类）

### A. production 行为缺陷

当前证据下零项确认。（held-refill 差异已修复待验，属 🟡 不属缺陷确认。）

### B. 旧测试夹具（1 项，证据充分）

`R2-SCHED-001 / CheckReleaseTickCpointSyncFollowsCandidates()`
（`Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs:13242`）：

- 夹具只写 compat：`catcher.Catching=victim; victim.Catching=catcher;
  catcher.CaughtSlotIndex=1; victim.CatcherSlotIndex=0`；
- production `BattleCpointWriter.SyncHeldCpoint()` 要求
  `victim.Runtime.CatchSourceSlot90 == attacker.SlotIndex`，而 `CatchSourceSlot90`
  默认 `-1`（`NTSDEntityRuntime.cs:205`）→ 恒不等早退 → T14 sync 不执行 → 断言失败；
- 反证 production 无回归的三重证据：
  1. exact 字段合同来自 VERIFIED 的 catch-relation-exact-fields 包（RED→focused
     19/19→真实 Play 19 案例）；
  2. positive-link 退休后真实 Battle grab Play PASS（三步 post-catch 序列）；
  3. 新夹具（SelfCheck 内 newer fixture、grab probe）均写 `CatchSourceSlot90`，grab
     生产者 `TryApplyKind3Grab` 同时写 exact+compat。
- 结论：Authority 的 +0x90 exact 契约是裁决项，compat-only 读取才是要退休的旧行为。
  修复=test-only 补 exact 字段（测试脚本属 governed，仍需 Change Record）。

### C. 独立诊断/stress 工具问题（2 项）

- `UnifiedAiSnapshotAuthority_CaptureMapsRuntimeDiagnosticsIntoReport`
  （expected 1/actual 2）：计数器来源已定位（`SimulationAiDecisionModule`：
  BuildCount 每次 build 入口 +1、InitialCaptureCount 每 claimed-active slot +1）。
  两个待甄别假设：(a) 夹具期望过时——后继 B0/B2 production 在注册期/输入期新增了
  合法第二次 capture；(b) 真缺陷——单次输入内意外双 build。需单独跑该测试取具体
  失败断言甄别；不并入 B6。
- 宽回归中 6 个 `NativeInputProxy` 旧比较失败（B6 invalid-preserve 包记录在案的
  既有无关项）：独立队列重跑归类。

### D. 外部环境问题

- 双 Unity 实例（目标 2022.3.62f3 vs FPSTest 2023.2.22f1），MCP 端口在 domain
  reload 时重分配——须按 instance id 连接并验证 version+scene；
- Unity Licensing 批处理阻塞曾耗尽无凭据路径，但 2026-09-09 许可已恢复（throw 8/8
  与 held-refill 7/7 均实际运行）；当前不构成阻塞，属风险项。

### E. 用户决策阻塞

- B11 内容策略三选一（整体切换/只补缺失/分类权威）——阻塞内容迁移与 CPoint
  27-scalar schema 的 resource 部分；
- 联合 schema 方向（entity 13/roster 1→2/full 20/checksum 23）——阻塞
  ReleaseTick/WeaponState/Tracker/GrabbedBy 的 carrier 删除部分（其行为退休不受阻）；
- USER_HOLD 本身（本报告待确认）。

## 6. 增量依赖 DAG（文字版）

```text
[保留-已完成] B0 owner 家族 ─┬→ B7 OPoint/spawn（未开始）
[保留] B1 cadence(待exit) ──┤
[保留] B2 AI owner ─────────┤
[保留] B3 placement+退休 ───┼→ B3 full close（依赖 B6/B7/B8）
[保留] B4 revival exit ─────┤
[保留] B5 placement exit ───┤
[保留] B6 已实施链(9包) ────┴→ B6 剩余族（按序）：
    ①held-refill Play补验(🟡,唯一在飞)
    ②kind3 release → ③terminal free → ④missing-action continue
    → ⑤DVX weapon-HP preserve → ⑥+0x2F8 三包 → ⑦kind2 pickup 三包
    → ⑧hurt consumer退休 → ⑨native impact → ⑩NTSDSpec 五包
    → ⑪GrabbedBy/Tracker/WeaponState/ReleaseTick 行为退休
         （carrier 删除分支 → 🔐联合schema gate）
独立支线 X：SelfCheck R2 夹具修正（B类，无生产依赖，最早可做）
独立支线 Y：stress AI-report 1v2 甄别（C类）
独立支线 Z：B5 hit-group 家族状态刷新（📄，无实现）
🔐 B11 内容策略 gate → CPoint 27-scalar resource 部分、OID122 baseMax(B11/H)
B6/B7/B8/B10/B11/H 齐 → B12 最终 parity
```

规则：已完成节点一律标"保留"不回队；🟡 优先补验不重写；只有 🔴 才建新实现包；
🔐 节点必须过用户决策。

## 7. 建议后续执行顺序

### 步骤 0（治理，无代码）：Ledger 状态刷新

- 为什么最早：三个纯记录不一致（B0 route 2/3/4 行、B5 hit-group 两行、B3-C25L
  资源前置标注）会持续污染后续审计判断。
- 读取：Ledger 对应行 + `NTSD28-B5-KIND5-LINKED-PARENT-SLOT-CORRECTION-001`
  Record；修正方式=追加 correction 链接，不篡改历史。

### 步骤 1：SelfCheck R2 夹具修正（B类，test-only）

- 为什么先于一切实现：full SelfCheck 是所有后续包的公共验收工具，它停在旧夹具使
  每个新包的 SelfCheck 证据都带同一噪声。
- Authority 依据：catch settlement 的 exact `catch_source(+0x90)` 契约（已由 B6
  exact-fields 包冻结）。
- 检查 Unity 符号：`BattleCpointWriter.SyncHeldCpoint`（已核）、夹具 13242–13308。
- 动作性质：只改夹具（补 `victim.Runtime.CatchSourceSlot90` 等 exact 字段），
  production 零改动。
- 验收：focused（该 SelfCheck 段）+ full SelfCheck 推进到下一个真实断言；builds
  0 error。
- 回滚边界：单测试文件单 diff，直接可逆。

### 步骤 2：held-refill Play 补验（🟡 恢复点，不重写）

- 为什么：唯一在飞包，production 已与 Record 逐条核实一致。
- Authority：OID122/123 refill/exhaustion 分支（owner audit 已冻结，
  `HP_BASEMAX_DEFERRED` 明确排除）。
- Unity 符号：`LF2WeaponHeldStateResolver.ProcessDrinkConsumption`（已核）。
- 动作性质：只补验——专门 Play（grab milk/juice → HP/MP/RNG delta/Vx/Vy/Vz 断言）
  + 包级 Ledger/Scene hash/Console 闭环 → 如全绿推进 VERIFIED。
- 回滚边界：无 production diff，无回滚需求。

### 步骤 3：stress AI-report 1v2 甄别（C类独立）

单跑该测试定位具体断言（BuildCount vs InitialCapture）→ 按假设 (a) 修期望或
(b) 建 correction 包。与步骤 1/2 无依赖，可并行。

### 步骤 4 起：按第 6 节 DAG ①→⑪ 顺序进入 B6 剩余实现包

每包维持既有纪律：先读 owner audit Record → Task Contract → test-first RED →
最小 production diff → focused/related/SelfCheck/targeted Play → builds →
Ledger/STATE/handoff/总表。第一实现包建议 kind3 release（owner correction 包已
冻结双分支边界）。⑥⑦⑧涉及 carrier/schema 的部分与 ⑪ 的 carrier 删除分支在实施前
必须确认 🔐 gate 状态。

### 长期

B1/B2/B3/B6 各阶段 fresh exit audit（B4/B5 已有 revival/exit gate 模板可复用）→
B7 起按 GLM handoff 8.3~8.8 清单逐域推进。

## 8. 需要重开的 VERIFIED 成果

无。本次审计未发现任何具备 Authority 文件/函数/分支 + 当前生产调用链 + 具体
first difference + 测试覆盖缺口四要素反证的 VERIFIED 包。已知的唯一一次 VERIFIED
撤回（positive-link 包的 C09 test-only 索引 28→27）已在暂停前自行修正并恢复
VERIFIED，本次已核实测试现状（`LastPhaseSequenceCount==33`、index 27→FrameAdvance）。

## 9. 披露与限制

- 用户批准例外（保留差异，非"已对齐"）：slot/容量 profile、多边形 walkable
  boundary、随机掉武器（仍需证明不污染非例外 RNG/slot）、固定世界相机、runtime
  头顶血条、FootSelf、移动端取景/底部黑区。用户排除：完整 HUD、结果页/KO
  feed/scoreboard 表现、background 多层/cycle、完整选择流程（其战斗逻辑不排除）。
- 证据边界：Ledger 后段（约 240 行早期 B4/B3/B2/B1 记录）按状态扫描与 exit audit
  汇总核验，未逐条读全文；总表矩阵 4.x 中段（S/T/I 系列旧行）按"累积历史文档"
  口径对待，与后继包冲突处以 Change Record + 当前源码为准（本次抽检未发现冲突
  实例）。未运行任何 Unity 测试（USER_HOLD 限制），运行时结论全部引自暂停前新鲜
  证据链 + 本次静态核验。
- 工作树：~140 modified / 6 deleted / 1230 untracked 均未触碰；六个 deleted 未
  恢复；无任何 `git reset/clean/restore`。
