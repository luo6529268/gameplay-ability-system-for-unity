<!-- CHANGE-RECORD
id: NTSD28-336B44-Q09-PRESENTATION-CLOCK-001
status: VERIFIED
change-kind: BATTLE_PRESENTATION_PUBLICATION_CLOCK
code-path: Assets/NTSD/Scripts/Animation/Rendering/BattleCentralRenderSystem.cs, Assets/NTSD/Scripts/Test/Editor/BattlePresentationDisplayMotionEditorTests.cs
authority: formal 336B44 playable main.cpp adjacent render snapshot publication timestamp and presentation_interpolation.cpp alpha sampling
evidence: docs/ai/TASKS/NTSD28-336B44-Q09-PRESENTATION-CLOCK-001.md
-->

# Q09 发布快照的插值时钟

脚本修改前记录。当前 Unity 在首次 `ResolveDisplayAlpha` 读取新发布版本时才设置 `displayClockStartedAt`；正式 playable 在新快照发布后立即标记时刻。这会遗漏发布到首画面的等待时间，但其原场景像素影响仍待测量。共用 writer 是 `QueueLatestPublishedFrame`，共用 consumer 是 `ResolveDisplayAlpha`；不会改变 logic World/Transform、DAT、Scene、背景相机或任何非战斗功能。

仅在两个声明脚本路径中新增单调发布时间和定向原 Scene Play 反例。生产入口用 `System.Diagnostics.Stopwatch` 从任何发布线程安全记时，读取时将已流逝时长投射回现有 Unity `Time.realtimeSinceStartupAsDouble` 坐标，保持现有 `displayClockStartedAt` 诊断字段与 Q07 探针语义。预期副作用只在 60/120 FPS 相邻快照的首画面插值 alpha；30 FPS、无相邻帧、关系或身份中断仍离散。若运行证据否定候选，不应保留行为改变。

不可回退边界：现有 dirty work、历史报告和 Scene/资源磁盘内容。验收与回滚见同 ID Task；正式 EXE 像素 A/B、其它 Q09 消费者和整场对齐不由本包自动关闭。实际脚本 diff、测试结果、未验项与风险在实施后追加。

首次测试脚本已写且生成 Editor C# 工程构建 exit0（既有引用/未赋值 warning，0 error）。原 Editor 刷新/编译后测试作业 `13b4fbdd09ff43798750cd39e13f894b` 1/1 FAIL，失败发生在进入 Play 前：Unity Test Runner 把活动场景临时置为空路径，初版测试错误要求入口已是 `NTSD_Battle.unity`。原 Editor 作业后回到 Battle Scene、非 Play、idle；该失败不构成插值 RED。现仅修正测试前置：确认临时场景 clean 后显式打开原 Battle Scene，再重跑；生产尚未修改。

第二轮作业 `03b8d19ffb1143599f51a037051092d2` 的 MCP job 状态在域重载后仍显示 running，但原 Editor 后续实际 idle/nonPlay，Unity Test Runner 将终态写入 `Temp/Goal18_LastTestResults.xml`：所选测试 1/1 FAIL，唯一失败断言为首次 120-FPS alpha 未包含发布后12ms等待；完整原件独立复制为 [RED XML](../../../artifacts/diagnostics/NTSD28-336B44-Q09-PRESENTATION-CLOCK-AUDIT-20261002/red-test-run-02.xml)，SHA-256 `202D87DDA4E6D2A8A7EBD93B176EBAB2C39744FD16387319FDEC813E15E1C0AC`。原 Scene 磁盘 SHA `93448372...7BF60` 未变、Git Scene diff为空。这是实测 RED，尚不代表正式 EXE 像素差或其它消费者。

实际改动：`BattleCentralRenderSystem.cs` 在新发布版本写入前记录单调 `pendingPublicationTimestamp`，首次采样把发布时间至采样时间的差值映射到原 Unity 时间域的 `displayClockStartedAt`，并在 `ResetRuntime` 清零；原 Q07 诊断探针仍可按原 Unity 时间域控制同一字段。`BattlePresentationDisplayMotionEditorTests.cs` 新增原 Scene 相邻发布、12ms首次读取、30 FPS 离散和逻辑 checksum 控制，并在 Editor Test Runner 临时空场景中显式载入 Battle Scene。没有触及 DAT、资源、Scene、游戏逻辑字段或非战斗代码。

原 Editor 再编译后所选 Play 测试1/1 PASS，首 alpha **0.36562121410253884**、30 FPS alpha **1**、逻辑 checksum 前后相同；邻近6项EditMode 6/6 PASS。生成 Editor 工程 exit0/CS error0，原 Scene退出clean且四保护SHA与既有基线4/4一致。MCP TestJob在进入/退出Play域重载后未正确写终态，以 Test Runner XML为准；仅在确认XML PASS且Editor非Play/idle后清除孤儿job。Change Ledger validator exit0/PASSED 1138 Records，`git diff --check` exit0。完整证据与限制见 [验收](../../../artifacts/diagnostics/NTSD28-336B44-Q09-PRESENTATION-CLOCK-001/ACCEPTANCE.md)。本Change只对共用发布时间戳作限定VERIFIED；正式EXE自然画面逐像素、其它Q09和总目标仍开放。

后继验收纠正：Task 原先声明同一发布 tick 的后续采样控制，本轮第一次GREEN只断言首样本、30FPS和checksum。已在同一测试内追加5ms后第二样本及不回退断言，生产代码未再改；记录/Task暂回`IN_PROGRESS`，待原Editor重新编译、原Scene再跑和最终validator后恢复限定关闭。此前RED/GREEN XML不修改。

最终补项：再次原Battle Scene Play的XML 1/1 PASS，同发布版本首/后alpha `0.376127274765357→0.55314848773611236`，30FPS=1、checksum相同；原Editor再次退出Play idle、Scene clean、四保护SHA基线4/4一致。最终两脚本生成Editor工程exit0/CS error0；完整证据见同ID [验收](../../../artifacts/diagnostics/NTSD28-336B44-Q09-PRESENTATION-CLOCK-001/ACCEPTANCE.md)。此前`IN_PROGRESS`是中间验收快照，本Change恢复`VERIFIED`，范围仍仅共用发布时钟；正式EXE自然画面像素/其它Q09继续开放。
