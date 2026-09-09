# NTSD 2.8-Logan → Unity 战斗对齐：GLM 进度核验与增量续接交接

> 交接日期：2026-09-09  
> 总目标：`NTSD28-UNITY-BATTLE-REALIGNMENT-001`  
> 当前控制状态：`USER_HOLD / DO_NOT_CONTINUE_IMPLEMENTATION`  
> 结论：项目已完成大量 B0～B6 基础、字段、顺序与规则迁移，但离 B0～B12 全部非例外战斗域对齐仍有显著距离。用户要求暂停现有目标，改由 GLM 先核验当前进度、现有生产脚本与证据，再整理真正遗漏和尚未处理的内容；不得从头重做已经完成的对齐。用户确认增量续接清单前，不应继续修改战斗代码。

## 1. 恢复时必须先读

按以下顺序恢复，不能只读本交接：

1. `AGENTS.md`
2. `docs/ai/CURRENT-AUTHORITY.md`
3. `Assets/NTSD/Docs/ntsd28-logan-vs-unity-battle-alignment.md`
4. `docs/ai/CHANGE-LEDGER.md`
5. `docs/ai/STATE.md`
6. `Assets/NTSD/Docs/CODEX-CURRENT-HANDOFF.md`
7. 本文件

具体行为实施前，还必须读对应 `docs/ai/TASKS/<Change-ID>.md` 和
`docs/ai/CHANGE-RECORDS/<Change-ID>.md`。总表中早期“当前结论”段落和部分旧矩阵行是累积历史，
不一定已随数百个后继包逐行重写；发生冲突时以顶部最新状态、Change Record、新鲜源码/测试证据和
正式 authority live path 为准，并用追加 correction/supersede 修正文档，不能篡改历史。

## 2. 唯一权威与禁止事项

### 2.1 战斗规则权威

- 正式 EXE：
  `J:\QQFile\NTSD2.8.3.3 zip\NTSD2.8.3.3\NTSD 2.8-Logan\NTSD2.8-Logan.exe`
- EXE SHA-256：
  `B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033`
- 对应 playable C++/header closure：82 files
- closure manifest SHA-256：
  `39DDDA154F5632C43089E2D5F1A5755ABBFBFD131A85D6B5ABF9AC00E6A46109`
- 入口：
  `source/ntsd28_playable/src/game_session.cpp` 的 `GameSession28::step()`，
  以及 `source/ntsd28_core/src/simulation/simulation_tick_driver.cpp` 的
  `SimulationTickDriver28::step(...)`。

NTSD 2.4、`ntsd_release_C#`、旧 trace、旧 self-check 和旧对齐结论只能作历史辅助，不能裁决
当前规则。候选重建 EXE、debug probe、反汇编笔记和未进入 playable closure 的代码也不能升级为权威。

### 2.2 当前内容权威

战斗规则已切换到上述 NTSD 2.8-Logan authority，但内容值暂仍受 Direction B 保护：

- 当前 Unity `Assets/NTSD/Config` 的 138 DAT 及 frozen manifest/projection 是现行内容值；
- 用户已明确要求最终处理 Unity 与 2.8 的内容、数值、PNG、WAV 和引用差异；
- 在用户选择“整体切换 / 只补缺失 / 分类权威”之前，只允许内容 catalog、引用图和 normalized
  projection 的只读审计；
- 不得直接覆盖 Config、PNG、WAV、Prefab、Scene 或 importer。

这是最终对齐的最大治理门槛之一。

### 2.3 禁止事项

- 不得修改正式 authority 目录。
- 不得恢复已从工作树删除的旧 NTSD 2.4/C# campaign 文档。
- 不得把推断、静态阅读、isolated compile 或 synthetic test 写成“已对齐”。
- 不得修改 `Assets/NTSD/Scripts/Gen/` 或 `Assets/Plugins/`。
- 不得 `git reset --hard`、`git clean`、`git restore`、覆盖/删除用户文件或恢复六个已删除项。
- 未经用户明确要求不得 push、rebase、amend、cherry-pick、切换有内容分支或清理 worktree。
- 用户此前明确禁止使用 computer-use；后继模型应继续遵守，除非用户明确撤销。
- 不得启动第二个会写同一 `Library` 的 Unity Editor。

## 3. 工作树现实

当前不是可随意清理的普通 dirty tree，而是长周期对齐工作的唯一综合工作树。2026-09-09 暂停前：

- `git status --short` 共 1376 项；
- 约 140 个 tracked modified、6 个 tracked deleted、1230 个 untracked；
- untracked 中包含大量正式 Task、Change Record、测试、manifest 和迁移工件；
- tracked deleted 多为用户已删除且禁止恢复的旧 campaign 内容；
- 多个文件的 Git diff 同时包含数十个已验证 Change ID，不能把整文件差异归给最后一个包。

后继模型必须在现状上工作，先用 Change Ledger 映射文件所有权；不要把未跟踪文件视为垃圾。

## 4. 总体进度判断

### 4.1 可以确认的阶段性成果

- Authority、恢复入口、Change Ledger、Task/Record 机制已建立。
- B0 已建立大量字段绑定、owner slot、runtime projection、trace schema、capture/comparator 和
  generation/slot 基础。
- B1 的正常 `33 ms`、F5 `3 ms`、最多两个 active interval 的 Unity host bridge，以及
  F1 pause/F2 step/F5 toggle 已写；真实物理按键与完整 cadence exit 仍不能假设全闭合。
- B2 已完成大量输入 cadence、native history、proxy two-pass、双 RNG/call-site、AI sensing/decision
  owner、owner-slot 和 joint trace 子包；阶段整体仍需 fresh exit audit，不能从子包数量推导完成。
- B3 主 pass 骨架和多项 placement 已显著重排，当前 diagnostics 为 33 个 phase occurrence；
  B3 可称 placement exit ready，但 full close 仍受后续规则、tail、spawn 和 presentation 依赖。
- B4 frame motion、teleport、physics 多分支、state12/18 transaction 和 revival consumer/exit
  已有大量 focused/joint trace/Play 证据；OPoint/revive producer、内容及跨阶段事件仍后置。
- B5 collision/hit/armor/damage/combo 规则族是当前完成度最高的大区之一：
  candidate/filter/first-body、type0～type6 status、armor/reduced hit、damage scale、resource core、
  standard rest、combo、credit/KO、legacy stats 退休等已有大量已验证子包。B5 placement exit ready，
  但 full close 仍依赖 B6/B7/B8/B10/B11/H 与最终 joint trace。
- B6 catch 主链已有实质完成：
  relation exact fields、single mixed advance、control-flow fences、settlement vaction preflight、
  held injury accounting/cover、post-settlement caughtact、negative invalid preserve、lifecycle
  atomic cleanup，以及 Unity-only positive validation pass 退休均已有真实 Unity 证据。
- B7 以后尚未形成阶段出口；仅有若干前置能力（OPoint owner propagation、slot/lifecycle cleanup、
  pool/generation 等）在早期包中完成。

### 4.2 不能宣称的内容

- 不能宣称 B0～B6 任一大阶段“全部行为完全一致”，除非新的阶段 exit audit 明确给出该结论。
- 不能宣称 full SelfCheck 通过。
- 不能宣称所有 fast path 与 authority trace 一致。
- 不能宣称内容/资源一致。
- 不能宣称 B7～B12 已完成。
- 不能宣称整个游戏逐像素一致；用户明确保留例外并排除部分原生 UI/表现。

## 5. 暂停前最后一个已验证包

`NTSD28-B6-POSITIVE-LINK-VALIDATION-RETIREMENT-PRODUCTION-001`

最终状态：

`VERIFIED / RED_4_FAIL_2_PASS_OF_6 / FOCUSED_6_OF_6 / RELATED_33_OF_33 /
C09_RELATED_9_OF_9 / B6_CATEGORY_103_OF_103 / NTSD28_229_OF_229 /
STRESS_255_OF_256_1_UNRELATED_AI_REPORT / REAL_BATTLE_GRAB_PLAY_PASS /
BUILDS_0_ERROR / SELFCHECK_BLOCKED_UNRELATED_R2_CPOINT_SYNC /
CONSOLE_0_ERROR / SCENE_UNCHANGED / PHASE_33 / NO_POSITIVE_EVENT /
LEDGER_PASS_429_373`

完成内容：

- 正式 tick 移除 Authority 不存在的 `HeldLinkValidation` occurrence；
- `BattleTickPhase.Count` 34 → 33，后续 enum 连续前移；
- post-catch 现在直接 `PreInteraction -> StageBounds -> HeldProcess`；
- `SimulationWorld` 不再构造、reset、配置或 restore positive pass；
- `ValidateHeldLinksAll(int)` 仅保留为 `[Obsolete]`、allocation-free、无写入、无 event 的兼容入口；
- 旧 pass 文件未物理删除，但 `Execute()` 被硬锁为 no-op；
- stress request/config/menu/report/teardown 与 U6 ownership surface 全部退休；
- U6 canonical owner count 9 → 8；
- W07 改为验证无 positive event 与 lifecycle atomic cleanup；
- real grab probe 顺序从四步改为三步：
  `first-held -> cpoint-weapon-sync -> second-held`；
- synthetic positive mismatch 保持完整字段，不再被单边清 `LinkState`；
- lifecycle cleanup、negative-held diagnostic、AI relation projection 和 snapshot/checksum 未改变。

关键证据：

- RED：job `8bebbd2c698540a68ab0748e1d2bc635`，4 fail / 2 pass；
- focused GREEN：`9cf0f048014e45049e589ab2a1c82262`，6/6；
- related：`7b8e526781204f5fb717888532fe2f8e`，33/33；
- B6：`919cd62af8dc4bb5863ff130b3af3683`，103/103；
- NTSD28：`0e4a70101adb49e3b5f45a7cfdeaba0a`，229/229；
- post-verify C09/held-refill：`edb6022909c5414499e9a5c2364c4c76`，9/9；
- real Battle grab Play：PASS，`positiveLinkAfter=5`；
- `Assembly-CSharp.csproj` 与 `Assembly-CSharp-Editor.csproj`：exit 0 / 0 error；
- Scene 前后 SHA-256：
  `D18E75F7920E12A1FEB13A8C23949042869E679B420A3073F7C21E4D4F4A2F11`，
  209891 bytes，last-write 未变；
- Ledger：429 records / 373 governed code files。

## 6. 唯一已写但仍未完成验收的活跃包

`NTSD28-B6-HELD-REFILL-MP-EXHAUSTION-PRODUCTION-001`

暂停状态：

`RUNTIME_PENDING / FOCUSED_7_OF_7 / C09_RELATED_2_OF_2 / BUILDS_0_ERROR /
PLAY_PENDING / SELFCHECK_BLOCKED_UNRELATED_R2_CPOINT_SYNC /
HP_BASEMAX_DEFERRED / USER_HOLD`

已写行为：

- OID123 在 child HP<=0 时仍执行 HP-2、holder MP+3、child MP cap 与 exhaustion；
- child `OrdinaryCreditGate2F4 >= 0 && child PP > 150` 时只 cap child，不读 legacy
  `KillCount`，不误写 holder；
- OID122/123 exhaustion 只消费一次 `BattleRandInt(0,7)` 写 Vx，Vy=0，保留 Vz；
- action/counter/relation/weapon-HP 现有 reset 保持。

已经实际运行：

- focused 7/7；
- C09 placement 2/2；
- 两套 build 0 error。

仍缺：

- 专门覆盖 OID122/123 refill/exhaustion 的稳定 Play 证据；
- package 自己的最终 Ledger/Scene/Console/Play 闭环；
- OID122 authoritative baseMax/HP clamp 明确仍归 B11/H，不属于本包。

用户要求暂停，因此不要自动继续这个包。GLM 的增量续接清单若选择恢复，应先读它的 Task/Record，并从
Play acceptance 开始，不要重写已经通过的 production。

## 7. 当前已知阻塞与独立失败

### 7.1 Full SelfCheck

fresh `BattleRuntimeSelfCheck` 已越过 positive-link 旧阻塞，但停在：

`R2-SCHED-001: T14 CPoint sync must still execute after candidate consumption`

位置：`CheckReleaseTickCpointSyncFollowsCandidates()`。

当前判断：该旧 fixture 只写了 compatibility catch slots，而后继 B6 production 已迁 exact catch
relation fields，因此它更像旧测试夹具没有同步，而不是已经证明 production 回归。必须先按当前 authority
和 exact-field contract 复核，再决定只改 fixture 还是另建 bug package；不得直接为了 SelfCheck 变绿而
补回旧兼容行为。

SelfCheck 同轮还会打印一个预期的 registration rollback error 日志；最终判断应以最终异常与
Change Record 说明为准。

### 7.2 ProductionEntityStress 全 fixture

`ProductionEntityStressEditorTests`：255 pass / 1 fail / 256 total。

唯一失败：
`UnifiedAiSnapshotAuthority_CaptureMapsRuntimeDiagnosticsIntoReport`
（expected 1 / actual 2）。

这与 positive-link retirement 无关，但在当前进度核验时应作为独立遗留项审计，不能隐藏。

### 7.3 Unity 多实例连接风险

当前同时存在至少两个 Unity：

- 目标：Unity 2022.3.62f3，项目
  `gameplay-ability-system-for-unity`，instance id
  `gameplay-ability-system-for-unity@b1b02287`；
- 另一个：Unity 2023.2.22f1，FPSTest，instance id `FPSTest@4a3424aa`。

端口会在 domain reload / Play 切换时短暂重分配。不能只硬编码 `6401` 并在同一 socket 上跨 reload
重试，否则可能连到另一项目。应使用 instance id 固定目标；发送 refresh/play/stop 后断开，等待
`C:\Users\Logan\.unity-mcp\unity-mcp-status-b1b02287.json` 回到 ready，再建立新连接并验证
Unity version + active scene 后继续。

暂停前目标 Editor 已退出 Play、idle、Console 0 error。

## 8. 后续仍需核验、补漏和增量续接的主要工作

GLM 不应机械沿用旧“下一包”文字，也不能把已经完成的包重新实现；应先核对现有生产代码、
Change Record 与实际测试证据，建立“已完成 / 已写待验 / 遗漏 / 尚未开始 / 用户决策阻塞”的依赖图，
再选择最早依赖已满足的真实缺口。
建议至少包含以下工作流。

### 8.1 先做治理和证据重基线

1. 枚举所有 Change Record 状态，区分 VERIFIED、RUNTIME_PENDING、历史 SUPERSEDED 与仅 owner audit。
2. 把总表中已经过时的早期结论逐行 correction，不删除历史。
3. 为 B0～B6 各做一次 fresh exit audit，禁止用“子包很多”代替阶段完成。
4. 修复/重基线 full SelfCheck 的 exact-catch fixture。
5. 独立审计 stress AI-report 1-vs-2 失败。
6. 统一列出仍无 joint trace、仍无 Play、仍只有 isolated compile 的包。

### 8.2 B6 剩余 Catch/Held/Weapon/NTSDSpec

- 完成 held-refill package 的专门 Play 闭环；
- CPoint 27-scalar schema与旧 front/back hurt consumer retirement；
- WPoint missing-action continue、terminal free、kind3 release、non-kind3 DVX weapon-HP preserve；
- DVX excluded-group exact carrier/producer/AI consumer；
- kind2 pickup relation 的 carrier → pure/system rules → atomic actual/HitPlan/legacy；
- legacy `GrabbedBy`、Tracker、WeaponState、ReleaseTick 等 producer/consumer/carrier 退休；
- NTSDSpec mass carrier、compat weapon selector、dead flute API、empty shell disposition；
- kind10/11/17/18 native impact 的 carrier → pure core → atomic production；
- held/broken weapon lifecycle与跨阶段 resource/world-KO feed。

这些包并非都可立即实施；schema/content相关项仍受用户内容策略或跨阶段依赖约束。

### 8.3 B7 OPoint / Spawn / Lifecycle / birth visibility

- 统一 OPoint spawn planner、owner/target/group/control字段；
- 逐来源验证升序 slot 动态扫描及“高 slot 当 tick、低 slot 下一 tick”；
- newborn suppression 与 Authority birth visibility；
- fusion/revival/broken particle/transient 初始化；
- pool reuse 全字段 reset、generation、stale reference；
- previous-action 后的 lifecycle resolution；
- shutdown 最后一 tick 与十一阶段有序关闭。

### 8.4 B8 Stage / BattleFlow / Results 逻辑

- 两次 depth clamp 和最终 X/Z settlement；
- 非用户例外的 type/mode/offstage removal；
- battle mode、function key、resource switch；
- victory/living groups/revival reserve；
- battle end timer 80/101/350 与结果 transition；
- 用户只排除了完整结果页表现，没有排除结果逻辑。

### 8.5 B9 Presentation

- tick 完成后的只读 `RenderSnapshot28` handoff；
- 30/60/120 render FPS interpolation，逻辑 checksum 不变；
- reverse physical-slot tie-break、entity内部命令顺序；
- spark/bleed/lives/nameplate/combo/body visibility/earthquake；
- shadow/resource mapping与缺资源行为；
- 保留用户批准的固定相机、头顶血条、FootSelf、移动端取景例外。

### 8.6 B10 Audio

- frame/hit/result audio event 的逻辑 tick 与顺序；
- Unity AudioSource pool 对应的并发、截断、loop、stop-all；
- F11/F12 音量；
- BGM/transition 与内容 catalog。

### 8.7 B11 内容、数值与资源

必须先让用户选择：

1. 整体切换；
2. 只补缺失；
3. 分类权威。

然后才可实施 405 DAT / 1255 PNG / 981 WAV authority runtime 与 Unity 138 DAT / 323 PNG
现状之间的 object/mode/background/resource catalog、schema、default、引用闭包、GUID/meta 稳定与
迁移验收。数量差只能证明覆盖不闭合，不能直接作为复制清单。

### 8.8 B12 最终 parity campaign

- 同 seed、同输入、同 tick 的 C++/Unity full trace；
- 多角色、武器、技能、抓取、投掷、复活、生成、stage、结果；
- 30/60/120 展示与逻辑 checksum；
- 长时间 33 ms 无 drift；
- slot/lifecycle/RNG 无 drift；
- fast path 开关前后 trace 一致；
- 真正玩家按键序列的 Play 验收；
- 最终例外、排除项和未知项报告。

## 9. 用户已批准例外与排除项

### 9.1 必须保留并最终披露的 Unity 例外

- Unity slot/容量 profile，而非强制 1000 物理 slot；
- 多边形 walkable boundary；
- 当前 Unity 随机掉武器；
- 固定世界相机；
- runtime 头顶血条；
- FootSelf；
- Android/iOS 移动端取景与底部黑区。

这些不是“已对齐”，而是用户批准保留的可观察差异。随机掉武器仍必须证明不会污染非例外 RNG/slot。

### 9.2 用户排除

- 完整 NTSD 2.8 HUD；
- 完整结果页/KO feed/scoreboard 表现；
- background DAT 多层/cycle；
- 完整角色/模式/背景/BGM 选择流程。

排除表现不等于排除其战斗输入或结果逻辑。

## 10. GLM 进度核验与增量续接建议

建议先输出一份“现状核验与遗漏清单”，不要第一步就改代码，也不要重新实现已通过的逻辑：

1. 以 B0～B12 为顶层，逐项读取当前 production、focused test、Task/Record 和最新运行证据，
   列出每个阶段的 verified evidence、已写待验项、真实遗漏、尚未开始项、dependency、用户决策和
   exit criteria。
2. 为 B6 尚未开始的 owner-audit后继包建立 DAG，先关闭无内容策略依赖的最早 live-path 差异。
3. 把 `RUNTIME_PENDING`、SelfCheck stale fixture、stress AI-report failure 分成三个独立队列。
4. 明确哪些结论需要 authority/Unity joint trace，哪些只需 Unity adaptation test。
5. 每个 implementation package 在脚本修改前创建独立 Task/Change，test-first，最小可回滚。
6. 已经 VERIFIED 的实现默认保留；只有新鲜证据证明其与 Authority 冲突时，才能建立 correction 包，
   不得直接推倒重做。每个包继续区分 CODE_WRITTEN、COMPILE_PASS、FOCUSED_TEST_PASS、
   RUNTIME_PENDING、VERIFIED。
7. 任何“阶段完成”必须由新的 exit audit 证明，而不是汇总旧标题。

## 11. 最终成果定义

最终交付不只是“Unity 能运行”，应至少包括：

- 所有非例外、非排除矩阵项为 ALIGNED 或有用户新决策；
- Authority live call chain 与 Unity adapter 的字段、顺序、短路、副作用闭合；
- B0～B12 每阶段独立 exit audit；
- C++/Unity full trace first-difference 为零；
- 双 RNG stream/call-site、33/3 ms host、input edge、slot/birth/lifecycle 长跑无漂移；
- compile 0 error、focused suites、full SelfCheck、真实 Play 与表现/音频验收；
- 30/60/120 presentation 不改变逻辑 checksum；
- 内容策略完成，raw/normalized manifest、resource closure 和 GUID/meta 稳定；
- Change Ledger/STATE/handoff/总表一致；
- 最终报告明确列出用户例外、用户排除、剩余未知和未运行项；
- 不使用 NTSD 2.4/C# 或 synthetic-only evidence 冒充 2.8 完成证明。

在这些条件全部满足前，总目标必须保持 `FULL_ALIGNMENT_INCOMPLETE`。

## 12. 当前暂停边界

用户已要求暂停当前目标。暂停后：

- 不启动新 Task/Change；
- 不继续 held-refill Play；
- 不修 SelfCheck/stress 独立失败；
- 不修改 production、Scene、Prefab、Config 或资源；
- 不清理工作树；
- 等用户把本交接交给 GLM，并由 GLM 给出新的审计/规划后再决定是否恢复。
