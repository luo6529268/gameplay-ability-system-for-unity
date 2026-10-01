<!-- CHANGE-RECORD
id: NTSD28-336B44-Q07-C050-SCENE-PLAY-001
status: VERIFIED
change-kind: EDITOR_PROBE
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q07C050VerticalSkipScenePlayProbeEditor.cs
authority: current 336B44 C050 source/root near and far trace plus original Unity Driver first-difference fix
evidence: docs/ai/TASKS/NTSD28-336B44-Q07-C050-SCENE-PLAY-001.md
-->

# C050 原 Battle Scene Play 出口

脚本前：正式近距OID24/action37→OID56/action259 tick2 kind0/dvy-5已证特殊链接门跳过普通垂直反应；原 Unity 修前额外action186首差已在通用 writer 修复，完整 Driver 近/远各18/18同当前根。现缺原 Battle Scene Play 与退出稳定性证据。本包只新增精确 Editor 探针和meta，验证近X520/远X1200同初态3tick；不改生产代码、DAT、Scene、资源和非战斗。短时进入/退出Play、World生成以及唯一新临时请求被脚本消费是预期副作用；请求删除另立事前审计。失败保留原结果、四保护SHA/Scene clean核验，不能拿Driver PASS代替Scene。回滚仅本包新文件，需遵守文件操作合同。

2026-10-01 实际脚本已写：新增 `Assets/NTSD/Scripts/Test/Editor/NTSD28Q07C050VerticalSkipScenePlayProbeEditor.cs`，仅在原 Battle Scene 的 Play 副本中按所列初态执行生产 Driver 三 tick，输出动作、HP、Vy 和停帧，并在退出后比对四个保护文件 SHA；请求 JSON 由探针消费后自动删除，须另有事前文件操作记录。尚未生成 meta、导入编译、运行 Play 或对照结果；状态 `CODE_WRITTEN`，运行风险与父任务未闭。回退方式仍为按文件操作合同处理本包独立新增文件，不触及生产脚本及用户已有工作。

2026-10-01 验收：Unity 导入后生成同名 meta，原 Editor 程序集时间晚于脚本且 idle/非编译；生成 `Assembly-CSharp-Editor.csproj` 的 `dotnet build --no-restore -v:q -clp:ErrorsOnly` 为 0 error、241 warnings。原 Battle Scene 近 X520、远 X1200 各三生产 tick 的 actor/target 动作、HP、Vy、hold 与当前 336B44 正式根分别 24/24 零差，近距 tick2 目标 action259/HP465/Vy0，远距未命中目标。两轮退出Play/Scene clean、四保护 SHA 稳、LoganRuntime Git 无差异。两次唯一 Temp 请求经事前/事后文件操作记录 `NTSD28-C050-SCENE-REQUEST-20261001-001` 覆盖。仅新增本诊断 `.cs/.meta`；生产行为、DAT、Scene 和非战斗无改。本包 `VERIFIED / SCOPED_SCENE_PASS`，父 C050/Q07/总目标仍开放；物理键、完整 World/表现未验。没有重跑全量 SelfCheck，因为生产代码未在本包改变。[验收](../../../artifacts/diagnostics/NTSD28-336B44-Q07-C050-SCENE-PLAY-001/ACCEPTANCE.md)。
