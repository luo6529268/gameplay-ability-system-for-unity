<!-- CHANGE-RECORD
id: NTSD28-336B44-Q07-C042-SELFCHECK-ORACLE-001
status: VERIFIED
change-kind: CODE
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
authority: formal 336B44 playable BattleWorld28::advance_catch_relations throw branch and C042 current-source 392-case witness
evidence: docs/ai/TASKS/NTSD28-336B44-Q07-C042-SELFCHECK-ORACLE-001.md
-->

# Q07/C042 SelfCheck 投掷计数断言版本修正

本记录在脚本修改前建立。原 Unity Editor 完整 SelfCheck 的首差为 `character raw throw mode=0: native throw clears both frame counters`，原始结果已另存。该合成例在无选招投掷前设置抓取者/被投者动作计数5/6；当前336B44正式投掷分支只把抓取者清0，受控392例中的无选招半数证实被投者保留计数。Unity共用生产 `BattleCpointWriter.ApplyThrow` 已按该规则实现，SelfCheck这一条旧断言未同步。

原状：`BattleRuntimeSelfCheck.CheckCpointThrowRawAndTransformMatrix` 断言双方计数均0。计划仅把该断言改为抓取者0、被投者6，并使失败文字准确指出预期；其它断言及生产路径不动。预期副作用只在诊断层，不能提升C042自然非零条件、C043父项或Q07整组状态。当前 Unity Editor 唯一实例中运行；保护 Battle/Menu Scene、GameConfig、ProjectBattleModeConfig，不删除或覆盖已有诊断原件。测试产出的临时结果若需覆盖，先另存并在本记录登记。

验收：生成工程编译0 error；原 Editor完整SelfCheck真实通过，或如实记录下一首差；`Tools/Validate-ChangeLedger.ps1` 及 `git diff --check` 通过；四保护文件SHA不变。回滚仅审阅本断言的精确差量；不使用 blanket restore/reset/clean。

2026-10-02 代码已写：仅 `Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs::CheckCpointThrowRawAndTransformMatrix` 的计数断言由双方0改为抓取者0/被投者6，失败文字同步改为当前正式规则。原测试仍检查4个方向模式及真实角色/共享DAT两组；生产写者、DAT、Scene和非战斗代码未改。本次尚未运行编译或修后SelfCheck，状态 `CODE_WRITTEN`；下一按声明验收。

2026-10-02 修后验收：`dotnet build Assembly-CSharp-Editor.csproj --no-restore -nologo` 退出0、0 error/281 warning；原 Editor `Assets/Refresh` 后 DLL 晚于脚本，完整 SelfCheck 第二轮已越过 `CheckCpointThrowRawAndTransformMatrix`，首差转为独立 C044 跨零旧断言（另存 `selfcheck-result-02.txt`），证明本方法的8个合成组合通过。MCP `execute_code` 因 Windows 临时编译命令过长未执行 SelfCheck，随后使用项目既有Editor菜单。测试产出的 `Temp/NTSD_BattleRuntimeSelfCheck.result` 为本轮生成物，首轮原件已先复制到 `selfcheck-result-01.txt` 再由菜单覆盖。此包状态 `FOCUSED_TEST_PASS`，完整SelfCheck、C042自然非零、C043及Q07仍开放。

后继各独立旧断言更正后，第八轮原Editor完整SelfCheck磁盘结果`PASS`（`selfcheck-result-08.txt`，SHA-256 `2F9ACB02FAA121BB2A3621951F57B4C690655337EDEE2E5AC350BE2B3BE88EA8`）；C042本合成测试口径`VERIFIED`，C042自然投前非零触发和Q07仍开放。
