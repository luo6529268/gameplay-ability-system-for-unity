# Task Contract — NTSD28-B4-REVIVAL-EXIT-AUDIT-001

> 状态：`VERIFIED / REVIVAL_TRACE_EQUAL_13_RECORDS_416_FIELDS / DOUBLE_RUN_BYTE_STABLE / RELATED_54_OF_54 / TARGETED_PLAY_PASS / BUILDS_0_ERROR / SELFCHECK_BLOCKED_UNRELATED / SCENE_UNCHANGED / B4_REVIVAL_EXIT_READY / PRODUCTION_UNCHANGED / B7_H_PRODUCER_PENDING`
> 依赖：`NTSD28-B0-DIRECT-REVIVAL-DEFAULTS-PRODUCTION-001`、
> `NTSD28-B4-REVIVAL-PARTICIPANT-GATE-KILLCOUNT-CORRECTION-001`、
> `NTSD28-B4-REVIVAL-QUEUED-CONTINUATION-PRODUCTION-001`、
> `NTSD28-B4-REVIVAL-NORMAL-FLOOR-RNG-PRODUCTION-001` 均为 `VERIFIED`。

## 目标

在不修改双方生产行为的前提下，建立当前 NTSD 2.8-Logan Authority 与 Unity production revival
入口/出口的同 seed、同 tick、逐字段规范化 trace，闭合 direct 默认、C25 render-phase arm、C07
queued/terminal/normal 三分支以及 transient slot removal/physical slot reuse。只有 trace 同序同值、双端双跑
字节稳定、focused/相关回归/build/真实 `NTSD_Battle` Play 与 Scene 不变证据齐备后，才允许把 B4 revival
专项出口标为 ready。

## Authority 与调用链

- 正式 EXE SHA-256：
  `B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033`；playable closure：
  `39DDDA154F5632C43089E2D5F1A5755ABBFBFD131A85D6B5ABF9AC00E6A46109`；source capture：
  `07CD47A0623F23D2C439E0E85EABF2ED10F8EAE8FC7D70DDB8396C704B3D778F`。
- C25：`SimulationTickDriver28::step(...) -> BattleWorld28::step_frames()`，只在 eligible type0 physical
  participant 的 state14/HP<=0/lives>1/render<1 条件下把 render phase 向零推进后 arm 到30。
- C07：`SimulationTickDriver28::step(...) -> BattleWorld28::advance_native_revivals(floors)`，entry 为
  active state14/HP<=0/render1..4；严格 lives-first 分派 queued continuation、terminal primary retain / transient
  despawn、normal revival。
- normal revival使用与C06相同的本slot effective floor、physical slot顺序的same-group type0 peers、
  `sumX!=0`、同步RNG `0x90/mod51/-25` 后 `0x91/mod31/-15`，只写precise X/Z。

## 修改范围

- `Tools/NTSD28AuthorityTrace/revival_exit_capture_main.cpp`
  - 链接既有只读 Authority source capture helper，输出规范化 revival trace；输出仅写 Unity `Temp/`。
- `Assets/NTSD/Scripts/Test/Editor/NTSD28B4RevivalExitAuditEditorTests.cs` 及 `.meta`
  - 通过 Unity production adapter/C25/C07 路径构造同一 case 序列，输出 Unity trace并逐字段比较。
  - 提供 EditMode request runner 与真实 Play request runner，不改 Scene。
- 本 Task/Change、Ledger、STATE、handoff、CURRENT-AUTHORITY 与总对齐表。

## Trace 范围与不变量

- schema、Authority identity、scenario、seed、tick/input marker必须一致；records必须严格同序。
- 至少覆盖：direct slot0 defaults；C25 primary lives2、primary lives1、transient lives2；queued explicit
  controller、missing controller fallback、inactive controller fallback；terminal primary retained、terminal transient
  removed；normal nonzero-sum peer、sumX-zero no-RNG；transient removal后同slot复用。
- 比较active/generation/oid/action/frame/counter/hold、HP/effective/base/MP、lives/queued HP/MP、group/controller、
  render/visual、precise与integer XYZ、RNG call count/callsite等共同出口字段；只比较双方已存在且本包权威闭合
  的字段，不为凑相等引入新生产schema。
- Authority必须调用production `step_frames` / `advance_native_revivals`；Unity必须调用production
  `LateEntityUpdateAll` / `PostFrameAdvanceDeathCleanupAll` 及 direct adapter，不得在测试中手写预期出口。
- 不修改 production C#、content、DAT、Scene、Prefab、ProjectSettings、正式 Authority 目录或历史 R8 probe。
- queued revival字段 producer仍属于 B7/H；本包只以显式fixture输入验证既有consumer，不将producer缺失包装为完成。
- 旧 `BattleDeathRespawnAiIntegerPlayModeProbeEditor` 仅是历史NTSD2.4夹具，不作为本包证据；未经批准不删除。

## 验收

1. Authority runner由既有 `Build-AuthoritySourceCapture.ps1` 可选runner入口构建，identity/manifest guard通过；
   连续两次输出字节一致。
2. Unity focused trace连续两次字节一致；Authority/Unity records逐字段完全相等，comparison
   `firstDifference`为空。
3. B0 direct defaults、B4 gate/queued/normal及相关C25/C07/physics/native-RNG focused回归通过；两套
   C# build 0 error。
4. 在真实 `NTSD_Battle` Play 环境执行同一trace；Console无本包error，退出Play后Scene SHA、dirty和root
   count不变。
5. 实际运行 full SelfCheck；若仍被已知独立CPoint mode0 victim-Vz断言提前阻塞，必须如实记录，不能冒充
   full SelfCheck pass，也不能据此否定已直接覆盖的专项trace。
6. `Tools/Validate-ChangeLedger.ps1`通过，且本包声明路径的`git diff --check`无新增格式错误。

## 回滚

只移除本包新增Authority runner、Unity test/meta与对应文档增量；不得回退前四个已验证producer/consumer包，
不得触碰用户工作树、正式Authority目录或历史夹具。

## 实际结果（2026-09-09）

- Authority runner由既有build helper链接当前source capture闭包；正式EXE与source manifest guard通过。
  runner/binary SHA分别为`AAD627BC214D304B88DFA75AEE8F87476C0D97A8CEDE92D023286F10BA6547E8`
  与`38D6C949CD1AC46D200D4381955A2DA2EAE85CD0298BCC0A8CD2F1E4E28AE21C`。默认原runner也重新
  构建成功，证明helper默认入口未回归。
- 初次runner的12-vs-13断言暴露缺少peer出口记录；补入`normal-peer-preserved`后固定13条。初次joint
  comparison随后只在两个non-participant fixture的`lives`出现首差：Authority裸`spawn_at`默认1，而
  OPoint-style显式`reserve=0`输入应为0。只在Authority诊断fixture补入该输入，不改任何生产逻辑。
- Authority两次输出均`PASS records=13`，capture SHA均为
  `9240BCD7BFF7B15B40B0096975944452E0B74D80AD562FB3EC8C5DFC6FF1C298`；Unity focused两次均
  `1/1`，Unity capture SHA均为
  `58C9A740193C04EF39C49F700CA32AF19EE6FADB80347166D80DB3DC1242D3C8`。
- 最终comparison为`equal-revival-trace / 13 records / 416 fields / firstDifference empty`，SHA
  `E8C800F59F8CE1AF6030C2426D676005525301569C17AF5223FA824B92E78A2D`。覆盖direct defaults、
  C25三类gate、queued explicit/missing/inactive fallback、terminal primary/transient、removed slot reuse、
  normal nonzero/sumX-zero及peer保持，并核对两端RNG calls/callsite与CRT初始化调用数。
- Unity联合相关回归实际`54/54`；两套进程外build均0 error，随后增量复跑均为`22 warnings / 0 errors`；
  Unity脚本刷新后Console compile error为0。
- 05:02:18 +08在真实`NTSD_Battle` Play环境执行同一13/416 trace通过；前后Scene SHA均为
  `50FD4D8FAF2CC5C630886CEFD4742A5FD068E1F7AADEE89809AD19B7B85AFF3C`，`dirty=false`、root13，
  Play已退出且目标run Console 0 error。
- 05:03:45 +08 full SelfCheck实际执行，仍提前停在既有CPoint mode0 victim-Vz断言；该独立阻塞未隐藏，
  也不否定已直接覆盖的B4 joint trace与Play证据。
- 本包没有修改战斗production runtime、content、Scene、Prefab、ProjectSettings或正式Authority目录。B4
  revival consumer/exit已ready；OPoint `reserve/join/join_reserve/join_pic` producer与公开schema仍归B7/H。
- Change Ledger validator通过：`409 records / 341 governed code files`；本包新增代码/文档无新增尾随空格。
