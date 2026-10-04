<!-- CHANGE-RECORD
id: NTSD28-336B44-Q09-P08-ROOT-UNITY-SAME-STATE-001
status: CODE_WRITTEN
change-kind: CODE
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q09P08SameStateBattlePlayProbeEditor.cs
authority: 336B44 formal root EXE headless LFR and corresponding playable bpoint rendering path
evidence: docs/ai/TASKS/NTSD28-336B44-Q09-P08-ROOT-UNITY-SAME-STATE-001.md
-->

# Q09/P-08 原 Scene 双实体同局部初态诊断

脚本创建前登记。Unity 现状：旧自然物理 J→临时鼬探针于 Battle 已跑到血点命令，但目标在场景启动数 tick 后加入，X/背景/视口和源/根案例不同；旧受控相机探针仅切 HP，不走自然完整tick。现有 F03 原Scene诊断提供可复用的 `OnSceneLoaded` Play clone 测试参战者设置、暂停稳定边界、`SetInitialActor`、离散 `FrameInputSet`、原Scene SHA检查流程。本包只加专用 opt-in Editor 探针，不改这些已有脚本及任何生产文件。

预期脚本：`NTSD28Q09P08SameStateBattlePlayProbeEditor` 的请求/状态/采样/退出；仅战斗测试命名空间，读取正式暂存资源，用根当前 LFR 初态恢复两参战者，记录初态、22完整tick及血点命令/必要画面。不得新增对生产生命周期的假设、硬编码跨端 PASS或改变正式/Unity规则。受影响符号和验收/回滚见Task；实际代码、编译、运行和首差之后追加，不把预期改写为结果。

2026-10-04 实际代码：新增声明的 Editor 诊断脚本及 `.meta`（GUID `d79e516df4d54e24ab17a9d9c05a3a8c`），未改既有测试/生产。`TryStart` 只接受单一 clean Battle Scene 和唯一结果名；用 SessionState 记已消费请求，避免覆盖请求或既有结果。`OnSceneLoaded` 在测试引导 Start 前设双参战者2/9；`WaitForRoster` 暂停生产Driver后恢复根 LFR 所选局部初态、native RNG seed0及 native clock；`MeasureOneTick` 顺次提交22个离散输入并记录双方字段、RNG与中央血点命令；`Complete` 有条件原相机捕图，`Finish` 在退出Play后写一次结果并检Scene SHA。脚本前旧探针/其它用户改动未动。当前仅 `CODE_WRITTEN`：生成项目/原Editor编译、Play首差、画面和四SHA尚未验证。

2026-10-04 编译与首轮更正：首次原 Editor 编译报探针局部变量 `second` 的 CS0165，已只在探针中初始化为 `null`；随后原 Editor 的 Editor 程序集时间晚于源码，`dotnet build Assembly-CSharp-Editor.csproj --no-restore -v:q` 为 0 error / 299 warning。用户在另一全套 EditMode 测试启动后手动取消；MCP 确认 `tests.is_running=false`、非 Play、idle，Menu clean。原 Battle Scene clean 加载后 opt-in 运行 `ita-equal-hp-336b44-unity-01`，退出 Play 并保留 Battle Scene SHA `D8C01F...AFE7F`。局部初态（OID2/9、X500/540、HP500/30、RNG CRT 3374725112/3000）成功恢复；tick8 目标 HP10，tick22 目标 action0/HP10、中央血点 1 个且在实体后、1×3，生成原相机图。与正式根 LFR 逐 tick 比较发现最早差异为 tick2/3 主角 action：正式 60、Unity 65。根 trace 的 inputCurrentMask=16、事件 `native standing attack RNG 0x82`，而探针错误提交 `SimulationInputButtons.Jump` (=32)；现已把探针前两 tick 改为 `Attack`，准备新 runId 重验。首轮结果原样保存，不将此输入夹具差异记为生产首差；此包仍为 `CODE_WRITTEN`，无生产脚本改动。

2026-10-04 修正后验证：生成 Editor 工程再次 `dotnet build Assembly-CSharp-Editor.csproj --no-restore -v:q` 退出0、299 warnings/0 errors；`Tools/Validate-ChangeLedger.ps1` 退出0（先移除误写在 Record 元数据中的非脚本 `.cs.meta` code-path，`.meta` 实际新增仍在正文记录）。原 Editor 第二次刷新后持续报告 `is_compiling=true`，Editor 程序集时间仍为修正源码前，MCP Console 本次读取0条 error；尚不能宣称原 Editor 已编译或第二轮已运行。正式根/Unity第一轮六字段 130/132、RNG44/44 和画面范围已写本包 REPORT；保护文件的终态与回 Menu 待 Editor 解除编译态后检查。生产脚本、资源与 Scene 没有由本包写入。

2026-10-04 取消全套测试后再次核对：原 Editor 的 `tests.is_running=false`、`current_job_id=null`、非 Play，Battle Scene `isDirty=false` 且磁盘 SHA `D88AD2111715AB2D970A85DDDAFFAAB206DFFD3BB54B9DF071AD25901D76CDF6` 与前次保护值相同。生成的 `Assembly-CSharp-Editor.csproj` 在当前工作树执行 `dotnet build --no-restore -nologo -v:q` 为 0 error / 330 warning，证明当前探针可由该生成工程编译；原 Editor 仍报告 `is_compiling=true`、未给出编译完成时间，因此不把离线编译算作 Unity Editor 新程序集验收，不在旧程序集上重跑 Play。`Attack` 修正后的第二轮及根/Unity首差判断继续待原 Editor 恢复。
