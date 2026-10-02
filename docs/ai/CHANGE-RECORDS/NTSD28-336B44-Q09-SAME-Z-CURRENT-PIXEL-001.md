<!-- CHANGE-RECORD
id: NTSD28-336B44-Q09-SAME-Z-CURRENT-PIXEL-001
status: VERIFIED
change-kind: TEST_ONLY_BATTLE_SCENE_PIXEL_EVIDENCE_ROUTING
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q09SameZFormalSpritePixelProbeEditor.cs
authority: formal 336B44 playable render_snapshot.cpp equal-Z slot order and current original Battle Scene pixel witness requirement
evidence: docs/ai/TASKS/NTSD28-336B44-Q09-SAME-Z-CURRENT-PIXEL-001.md
-->

# Q09/P-04 当前场景同 Z 像素见证

脚本修改前记录。旧四图像素证据的 Battle Scene SHA 为 `471396E7...7B9`，当前原 Scene SHA 为 `93448372...7BF60`。现有测试脚本的自然分支用固定 Temp 结果文件及旧 artifact 图片名，再运行会覆盖历史原件；本任务先解决该诊断输出边界，然后取新版当前场景像素。336B44 正式排序块与旧版逐字节同，当前 Unity 排序源码同向，但当前场景 GPU 仍未验。

仅改 `NTSD28Q09SameZFormalSpritePixelProbeEditor` 的自然请求解析、结果及图片输出路径，加安全 run ID 和不覆盖已有文件的检查。旧受控分支、实体物化、完整 tick、相机和清理逻辑保持原有职责。预期副作用是测试脚本重新导入、四次原 Battle Scene Play、各自产生 JSON/PNG，并自动删除本任务新建的单个请求文件。每次请求原文和 SHA 预先存档；无历史输出、DAT、生产逻辑、Scene 或非战斗代码写入。

验收、回滚和未覆盖范围按同 ID Task Contract。必须先核原 Editor 编译与 Scene clean，再逐项运行；四图结论不能推出 Legacy、正式 EXE 像素或整个 Q09 已通过。

脚本已写：自然请求现接受 `variant@run-id`，未带 ID 的入口自动产生新 ID；只允许安全字符、长度1～64。自然 JSON 与 PNG 改写入同一独立 run 目录，写前拒绝已有文件；结果包含 run ID。原受控路径、四图几何/时序与生产行为未改。当前 `CODE_WRITTEN`；`git diff --check` 已通过。原 Editor 编译、真实 Play 和四图验收尚未证。

限定交付：原 Editor 的 Unity MCP `refresh_unity` 返回成功，更新后的 Editor 程序集时间晚于脚本；生成 Editor C# 工程 `dotnet msbuild Assembly-CSharp-Editor.csproj -nologo -t:Build -p:Configuration=Debug -v:q` exit0，原 Editor Console `error CS` 过滤返回0项。四个独立原 Scene Play 的新 runId 结果均 `PASS_CAPTURE`，同 Scene SHA934483…、tick5→6、两槽/帧/初始 RNG、退出清理一致。全交叠区82像素中44仅支持OID120后绘制、0仅支持相反顺序，82均符合前者；四保护 SHA 不变，结束 Editor idle/Scene clean。四个临时请求均按本 Task 预记由探针自动删除，原文/SHA与实际缺席另存审计；旧四图 SHA 4/4 未变。[原始结果与计算](../../../artifacts/diagnostics/NTSD28-336B44-Q09-SAME-Z-CURRENT-SCENE-PIXEL-001/scene-934483-20261002-01/REPORT.md)。[Change Ledger 校验](../../../artifacts/diagnostics/NTSD28-336B44-Q09-SAME-Z-CURRENT-SCENE-PIXEL-001/scene-934483-20261002-01/change-ledger-validation.txt) exit0/PASSED，1137 Records、13项当前代码 diff均覆盖；`git -c core.safecrlf=false diff --check` exit0。本 Change 仅对 test-only 输出保护与当前场景限定像素证据标 `VERIFIED`；Q09/P-04 父项、正式 EXE 像素、Legacy、其它画面仍开放。
