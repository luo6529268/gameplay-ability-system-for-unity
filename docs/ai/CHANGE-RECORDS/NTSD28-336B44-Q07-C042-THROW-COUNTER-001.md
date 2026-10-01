<!-- CHANGE-RECORD
id: NTSD28-336B44-Q07-C042-THROW-COUNTER-001
status: RUNTIME_PENDING
change-kind: CODE
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q06CpointThrowRawBindingEditorTests.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleCpointWriter.cs
authority: selected formal 336B44 playable BattleWorld28::advance_catch_relations throw branch and current 392-case source witness
evidence: docs/ai/TASKS/NTSD28-336B44-Q07-C042-THROW-COUNTER-001.md
-->

# C042 投掷计数版本回访

已在脚本修改前创建。旧 B1E13 Q06 合成见证392例被投者计数8→0，但当前336B44重链同一未改 runner 双跑各392例：无选招196例保留8，有选招196例已先清为0；这与当前正式源投掷只清抓取者以及选招独立清零一致。原始编译首轮缺 native wrap/bcrypt/Unicode/ScenarioLoader 链接参数退出1，补参数后编译退出0；首次失败保留在同证据目录。源码受控见证不等于根EXE行为。

当前 Unity 目标 writer 的 `ApplyThrow` 无条件将被投者计数清0；同类 `ApplyAction` 的选招清零不属于这条投掷修复。只将现有 Q06 测试的见证入口指向另存的新版本数据，先跑当前 Editor RED，再做共用 writer 一行修正并验证。可能影响后继帧和旧版夹具，旧版源输出与报告原样保留，任何失败不得通过改 DAT、Scene 或非战斗逻辑规避。预期副作用是无选招投掷后被投者计数按当前正式源保留；抓取者计数、投掷动作/速度/关系、选招清零、生命周期与有序关闭不变。回滚仅为本记录的测试入口和 writer 精确差量，经审阅执行。

初始待填项按下方追加结果更新；正式根/原 Battle Scene 可达性仍待。

原Editor已编译测试入口0错。`run_tests` job `f003bc8fece64871b9ba5856f6ca2db3` 两profile均按预期 RED；首差 case0 slot1 `frame.frameCounter`/`counter` 为 Unity0、当前源码8，任务所查的生产差异成立。其后只删除已声明 `BattleCpointWriter.ApplyThrow` 末尾的 `victim.AttackingCounter = 0`，保持 `ApplyAction` 的选招清零与 `LF2Entity` 其它路径未动。两声明脚本当前状态 `CODE_WRITTEN`；修后编译/聚焦/相邻/Play尚待。本证据仅受控合成 CPOINT，不能宣称正式根或自然玩家投掷已验。

2026-10-01 追加：原 Editor 修后编译0错，`ImmediateThrowMatchesNativeRawBinding` 两 profile 当前源码392例各自通过，job `c0a51ece619c4f7fb71b6c0ae1b510ff` 2/2 PASS，两个 JSON 输出 `beforeDifferences`/`differences` 空；相邻选招两 profile 与投掷生产路由 job `65bf11da796d4a3380479ff01aec4e81` 3/3 PASS。旧 B1E13 见证文件未覆盖，新旧后继完整tick另有版本差异，旧 Q06 类其余测试未逐项重基线。实际修改仅测试 source witness 常量和共用 writer 一行；未改 DAT/Scene/非战斗。正式根 LFR 无计数/关系初态承载，原 Battle Scene 自然投掷未验，状态推进为 `RUNTIME_PENDING / FOCUSED_TEST_PASS`，不得称 Q07 完成。[验收记录](../../../artifacts/diagnostics/NTSD28-336B44-Q07-C042-THROW-COUNTER-001/ACCEPTANCE.md)。

收尾验证：`Tools/Validate-ChangeLedger.ps1 -RepositoryRoot (Get-Location).Path` 退出码0（1071 Records、10受治理代码文件）；`git -c core.safecrlf=false diff --check` 退出码0；Battle/Menu Scene、GameConfig、ProjectBattleModeConfig 四 SHA-256 与保护基线相同。原 Editor `get_editor_state` 为 idle/nonPlay/noncompiling/nonupdating。未运行全量案例或原 Scene 自然投掷；运行时出口保持开放。
