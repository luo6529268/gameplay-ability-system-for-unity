<!-- CHANGE-RECORD
id: NTSD28-336B44-Q07-C054-DEFUSION-PRECISE-POSITION-001
status: FOCUSED_TEST_PASS
change-kind: CODE
code-path: Assets/NTSD/Scripts/Simulation/Passes/Oid5152/BattleOid5152RuntimeModule.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q06FusionRecordTransactionEditorTests.cs
authority: selected 336B44 playable BattleWorld28 defusion partner precise XYZ rebuilt from integer XYZ
evidence: docs/ai/TASKS/NTSD28-336B44-Q07-C054-DEFUSION-PRECISE-POSITION-001.md
-->

# C054 解融合伙伴精确坐标重建

脚本前建立。正式 `battle_world.cpp:2915-2940` 在有效解融合后复制主角整数 XYZ 并重建伙伴精确 XYZ；Unity `BattleOid5152RuntimeModule.TrySplit` 当前复制主角精确 XYZ，源规则精确 X/Z 同样直接复制。旧 Q06 测试精确/整数初态相等，未覆盖分歧。影响仅拆分伙伴位置及后继运动、碰撞、快照；不改主角位置、准入、计时、动作、DAT、Scene 和非战斗。

先在既有融合测试加小数差异样本，再最小修改共用拆分写入点。需明确主角物理显示整数与 source-rule 整数是两个独立域；不反向改变批准的画面比例。生成工程编译、原 Editor 聚焦、正式源码受控样本、原 Battle Scene 及有序退出分别记证据。当前原 Editor PID11944 在运行但无 Pipeline 包，新探针未导入；不可把编译当作 Unity runtime。回滚只审本 ID 两脚本的行级改动，保护全部预存工作。

2026-10-01 测试先写入既有 `NTSD28Q06FusionRecordTransactionEditorTests`：当前正式表行0的融合后受控主角物理精确 XYZ=321.75/-2.25/253.5、整数321/-2/253，源规则精确 X/Z=319.75/251.5、整数319/251；预期解融合伙伴两域精确值各从自身域的整数重建，主角不变。原 Editor 未导入，未运行预期 RED；这是由旧代码直接可推出的预期，不写作实测。

生产 `BattleOid5152RuntimeModule.TrySplit` 只改伙伴精确位置的五个赋值，保留整数复制和其它融合字段。`dotnet build .\Assembly-CSharp-Editor.csproj --no-restore -v:q -clp:ErrorsOnly` 成功：263警告、0错误。当前正式 fusion DAT SHA 为 `E0D7BF92F222C63C04D6728ECD423369F0604D77FF12DF958CE4AE76FEBA5FEE`，与既有夹具运行时断言相同；历史 Source4 JSON 只提供受控初态，不晋升为336B44正式运行证据。原 Editor 聚焦/Play、正式根精确位置同态和后继 tick 未验。状态 `CODE_WRITTEN / UNITY_RUNTIME_PENDING`，C054/Q07/总目标开放。

审计：`Tools/Validate-ChangeLedger.ps1 -RepositoryRoot <当前仓库>` PASS（1085 records、29 diff code files）；正常仓库 `git diff --check` 对本包脚本与同步文档无空白错误。正式根 EXE SHA 仍为 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3`。Battle/Menu Scene 与 GameConfig/ProjectBattleModeConfig 四个保护 SHA 仍分别为 `3A089236...235ED`、`DD6A48A...1DC3`、`0527D737...8EA7`、`B57CFEF3...85B82`，未编辑这些文件。`Library/ScriptAssemblies/Assembly-CSharp-Editor.dll` 最后写入仍为 2026-09-30 21:47:30 UTC；Unity CLI 显示原 Editor PID11944 运行但项目无 Pipeline 包，因此未声称原 Editor 编译或聚焦/Play 通过。

2026-10-01 后续追加：原 Editor PID11944 通过现有 MCPForUnity 本地桥接完成刷新与编译，Editor DLL 更新时间 23:29:51 UTC。C054 单项 EditMode 首轮默认初始化 15 秒超时且未执行测试；第二轮 120 秒初始化窗口 job `e19472cf691540ac8ca88552e2872fd6` 返回 1/1 Passed、0 Failed。测试器临时关闭活动场景后已在原 Editor 重新载入原 Battle Scene，磁盘 SHA 未变。当前正式 336B44 源码重新编译/运行旧融合诊断 4 例均成功，但整数/精确初态相等，不能证明小数分支的正式运行同态。状态推进为 `FOCUSED_TEST_PASS / RUNTIME_PENDING`；C054/Q07 不关闭。证据见[聚焦结果](../../../artifacts/diagnostics/NTSD28-336B44-Q07-C054-DEFUSION-POSITION-AUDIT-20261001/UNITY-FOCUSED-RESULT.md)。

2026-10-01 预登记后继诊断：临时独立 C++ 脚本 `Temp/NTSD28C054FormalFractional336B44-20261001/fusion_fractional_witness.cpp`，复制当前既有融合见证的正式表身份与原受控对象定义，仅在成功融合后、解融合前向主角注入小数精确 XYZ，不碰正式 source/resource、Unity 生产脚本或旧诊断。目标是用当前 336B44 完整 Core 链证明 C054 条件分支与后继 tick；两次运行一致及失败保留。该结果不代表正式根自然可达。若需继续改 Unity 测试/Play 探针，须先扩充本 Record 的准确路径、断言和退出验收。当前状态仍 `FOCUSED_TEST_PASS / RUNTIME_PENDING`。

实际诊断脚本仅在融合后、拆分前加主角精确 XYZ `(整数X+0.75, 整数Y-0.25, 整数Z+0.5)`，原计时介入、正式表身份、定义、后继完整 Driver 保持。当前正式 28 Core + 3 playable C++ 链接 exit0；双跑各四行/exit0/同 SHA `0A278F5512EAA402CE4B1BAF5146AC58EA80F8905133D885176A278817F78E2D`；独立40断言通过。两有效解融合行伙伴精确/整数 XYZ 均为 `(320,0,250)`，主角精确 `(320.75,-0.25,250.5)` 保持。旧诊断源文件、正式 source、DAT、Unity 脚本/场景均未因本步骤修改。证据[源码受控结果](../../../artifacts/diagnostics/NTSD28-336B44-Q07-C054-DEFUSION-POSITION-AUDIT-20261001/SOURCE-FRACTIONAL-RESULT.md)。剩余：根自然小数入口、原Battle Scene后继/退出及完整同态；Record 仍 `FOCUSED_TEST_PASS`，父 C054/Q07 开放。
