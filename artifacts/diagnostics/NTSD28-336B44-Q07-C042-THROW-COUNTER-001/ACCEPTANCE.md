# C042 投掷计数：受控机制验收

状态：`FOCUSED_TEST_PASS / RUNTIME_PENDING`。本报告只关闭当前 336B44 源码和 Unity 共用投掷 writer 的受控即时计数首差；不关闭 Q07、自然投掷链或正式根 EXE 同态。

## 权威与首差

- 当前根正式 EXE SHA-256：`336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3`。其对应 playable 构建闭包的 `BattleWorld28::advance_catch_relations()` 投掷分支只清抓取者 `frame.frame_counter`；输入选招另有被投者计数清零。
- 未改动的既有 C++ 诊断 runner 重新链接当前 Core/Playable 源码，双跑各 392 行、字节相同，JSONL SHA-256 `2C14DDE683FB8832084BC0190013D2C17A677D07686D80369E4217E1AEE69FFC`。未选招 196 例被投者 8→8；已选招 196 例 8→0；392 例均投掷且抓取者计数清零。见 `source-counter-validation.json`、`source/first.jsonl`、`source/repeat.jsonl`。首轮编译缺链接参数的失败保存在 `compile-output.txt`，修正编译命令见 `compile-argv-v2.txt`。
- 旧 B1E13 Q06 同 runner 的 392 例全部 8→0，属于历史版本见证，原文件未覆盖。当前源与旧源的后继完整 tick 还存在其它新版规则差异，不能将旧后继 tick PASS 晋升为 336B44 结论。

## Unity 改动与验证

- 测试类 `NTSD28Q06CpointThrowRawBindingEditorTests` 的 source witness 入口改指当前版本另存目录；`BattleCpointWriter.ApplyThrow` 仅移除被投者计数的额外无条件清零。`ApplyAction` 的输入选招清零不变。DAT、Scene、Prefab 和非战斗逻辑均未修改。
- 原 Editor 刷新后，修改生产 writer 前的 `ImmediateThrowMatchesNativeRawBinding` 两个 profile 均 RED（job `f003bc8fece64871b9ba5856f6ca2db3`）：case 0 被投者 `frameCounter/counter` Unity 0、当前源 8。见 `red-job.json`。
- 一行 writer 修正后原 Editor 编译成功、0 C# 错误；同两 profile GREEN 2/2（job `c0a51ece619c4f7fb71b6c0ae1b510ff`），各覆盖 392 例，输出 `immediate-Authority400.json` 与 `immediate-MobileExtended.json` 的 `beforeDifferences`、`differences` 均为空。相邻选招两 profile 和投掷生产路由 3/3 PASS（job `65bf11da796d4a3380479ff01aec4e81`）。见 `green-job.json`、`adjacent-job.json`。
- `Tools/Validate-ChangeLedger.ps1` 退出码0，1071条 Record、当前 diff 10个受治理脚本文件均通过覆盖检查；完整输出保存在 `ledger-check.txt`。`git -c core.safecrlf=false diff --check` 退出码0。Battle/Menu 两 Scene、GameConfig 与 ProjectBattleModeConfig 的 SHA-256 均与本轮保护基线一致；原 Editor 最终为 Battle Scene、idle、非 Play、非编译/资源更新。

## 尚未证明

正式根 LFR 初态不携带被投者 frame counter/抓取关系覆盖，受控合成源矩阵不能冒充根 EXE 同状态证明。原 Battle Scene 自然 OID75 抓取→投掷及后继完整 tick 尚未验收；同测试类的旧 Q06 `FollowingFullTickMatchesNativeLifetime` 等其它出口未按 336B44 逐项重基线，不宣称它们通过。下一步按总表 G1 找当前正式内容可达的自然投掷入口，取得正式根/原 Scene 同 seed/tick 首差，再处理 C040/C043/C044/C045 各自所属 writer。Q07 和总目标保持开放。

正式 OID75 `bee.dat` 已只读确认 action355 kind-3 成功转358/130，358～375 连续 kind-1 链至375投掷；这只是下一条运行探针的候选，不提升 C042 状态。[候选记录](NATURAL-CANDIDATE.md)。
