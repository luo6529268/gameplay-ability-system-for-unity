<!-- CHANGE-RECORD
id: NTSD28-336B44-Q09-WORDS5-GAMEVIEW-001
status: VERIFIED
change-kind: CODE
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q09NameplateNaturalPlayProbeEditor.cs
authority: 336B44 formal root identity and matching playable render_snapshot nameplate WORDS5 selection
evidence: docs/ai/TASKS/NTSD28-336B44-Q09-WORDS5-GAMEVIEW-001.md
-->

# Q09 WORDS5 原场景 Game View 定向见证

脚本修改前建立。现有探针能用正式暂存内容从 Menu additive 进入 Battle、读取同tick中央姓名牌命令并采原 Game View，但匹配固定P1组1，诊断侧选图条件仍只接1～4；它不能验证新修的普通组5生产展示。正式playable允许模式0槽0组5显示并选择WORDS5。

预期只为现有探针加组5定向入口及报告/断言，复用原有生命周期与截图机制；组1/Q07其它入口默认保持原状。脚本、Scene和资产保护、验证与回滚边界见Task。后续实际改动、编译、Play、失败和遗留证据追加，不改写预期为事后结果。

2026-10-04 实际代码：只改声明的 `NTSD28Q09NameplateNaturalPlayProbeEditor.cs`。新增 WORDS5 菜单入口与单独输出目录；`Start` 增加默认值1的诊断组参数，现有调用保持原组1，组5入口传5；Menu→Battle配置P1 team使用该参数。Capture报告当前P1关系组、同tick名字首字的对应sheet OverlayGlyph命令数；诊断侧有效表上限4→5；组5入口还要求 RelationTeam5、sheet5、绑定有效且实际中央命令存在。沿用原探针截图、Additive卸载与Play退出。未改生产、DAT/PNG、Scene或菜单。脚本前保护SHA：探针 `C0EFF79F...11CFA2`，Battle `D8C01FD3...5AFE7F`，Menu `9EAAA0B4...6C1BA`，GameConfig `0527D737...EA7`，Mode Asset `B57CFEF3...85B82`。

2026-10-04 实际验证：生成 Editor 工程编译 0 error；原 Editor 刷新并进入唯一组5 Play，原始 JSON `PASS`，逻辑 tick7 的关系组5、WORDS5 绑定、中央 OverlayGlyph 1条和 1920×1080 Game View 同帧。截图投影点 21×21 中 77 个紫色主体像素，邻近背景与P2字形对照均0；正式/Unity WORDS5 PNG逐SHA相同。探针自动退出，原 Editor idle/non-Play、测试未运行、Menu clean；双 Scene 与两配置的四SHA均同前。具体原始文件、像素算法及未证边界见[报告](../../../artifacts/diagnostics/NTSD28-336B44-Q09-WORDS5-GAMEVIEW-001/REPORT.md)。此前误启全套 EditMode 已由用户取消，不纳入通过项。本包仅诊断入口 `VERIFIED`；正式根GPU、默认剧情stage、Q09/Q12及总目标继续开放。
