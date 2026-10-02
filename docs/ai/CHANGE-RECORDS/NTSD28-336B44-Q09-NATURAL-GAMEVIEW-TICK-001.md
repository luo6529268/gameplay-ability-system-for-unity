<!-- CHANGE-RECORD
id: NTSD28-336B44-Q09-NATURAL-GAMEVIEW-TICK-001
status: VERIFIED
change-kind: BATTLE_Q09_EDITOR_DIAGNOSTIC_OUTPUT_AND_VIEWPORT_METADATA
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q09NameplateNaturalPlayProbeEditor.cs
authority: formal 336B44 playable battle presentation/viewport and current original Unity Battle Scene visual evidence gap
evidence: docs/ai/TASKS/NTSD28-336B44-Q09-NATURAL-GAMEVIEW-TICK-001.md
-->

# Q09 自然 Game View 同帧取证

脚本修改前记录。原探针的 Game View JSON 写到固定 `original-battle-natural-nameplate-gameview.json`，再次执行会覆盖历史原件；PNG 虽用时间戳命名，未与该 JSON 使用共同唯一 ID。当前独立 MCP 合成截图有实际像素但没有同帧 tick、快照版本和相机映射。原脚本 SHA-256 与旧 JSON/PNG 保护哈希见同 ID Task。

本 Change 仅在已有 Q09 Editor 探针的 Game View 分支建立每次运行唯一的 JSON/PNG 路径和存在性保护，并追加已有相机/视口快照字段的只读输出。原非截图路径、自然移动条件、生产模拟、表现逻辑、DAT、资源、Scene 和非战斗功能不改。运行中的唯一副作用仍是测试探针原有物理方向键输入、短暂停止 Driver 以封存同 tick 画面，完成后释放按键和恢复 Driver。

验收、不可覆盖边界及回滚方式见同 ID Task。执行后的准确 diff、编译/原 Editor/Play/哈希、失败结果和未验证项须追加本 Record；当前不能称 Q09 或正式 EXE 像素通过。

首轮实际：生成 Editor 工程 exit0、251 既有 warning/0 error；原 Editor MCP 刷新后 `Assembly-CSharp-Editor.dll` 更新、域重载后 idle。原 Battle Scene Play 的正式资源预热与 Bootstrap 完成，现有菜单探针返回唯一 JSON `natural-nameplate-20261002-000658-653-667ea8e64ebb47ceadfeb2549223905d.json`，状态 FAIL，首差是 `The selected battle slot has no nameplate label.`，尚未输入或截图。已通过 MCP 退出 Play，Editor idle/nonPlay。旧固定 JSON/PNG未写。Task 在脚本后续修改前补充直接 Battle 无名牌时 Game View 分支仅用角色本体锚点，历史名牌验证语义不变；首轮失败原件保留。

实际脚本职责：`GameViewOutputFolder` 和每次时间戳+GUID令 Game View JSON/PNG 共享唯一 ID；保存前检查旧文件存在，避免覆盖。原非截图固定历史分支不动。截图后核 `driver.IsPaused` 且 `tickAfterScreenCapture==captureTick`；追加相机正交尺寸、aspect、pixel rect、viewport 左上角及每像素单位。首轮反馈后只为直接 Battle Game View 无名牌分支改用角色本体锚点；存在名牌时及非截图旧路径仍保留原名牌断言和公式。没有改生产、DAT、Scene、资源或非战斗代码。

最终验收：第二次生成Editor构建exit0/251既有warning/0 error，原Editor MCP刷新/域重载后原Battle Scene Play按D键自然移动、同tick482中央计划/发布/截图后稳定，唯一 JSON/PNG均有效，退出Editor idle/nonPlay/Scene clean。首轮FAIL及第二轮PASS原件独立保存，旧固定JSON/PNG SHA2/2和四保护资产SHA4/4保持；`LoganRuntime`限定Git状态无差。`git diff --check`exit0。Ledger validator首次嵌套powershell默认`$PSScriptRoot`失败，改直接调用并显式传`-RepositoryRoot`后 exit0/PASSED 1139 Records、16当前代码diff覆盖。[原件与限制](../../../artifacts/diagnostics/NTSD28-336B44-Q09-NATURAL-GAMEVIEW-TICK-001/REPORT.md)。本Change只记`VERIFIED_SCOPED_UNITY_SAME_TICK_CAPTURE`；正式EXE实际像素、名牌和Q09父项仍开放。回滚仍按Task限定，不删除失败或成功原件。
