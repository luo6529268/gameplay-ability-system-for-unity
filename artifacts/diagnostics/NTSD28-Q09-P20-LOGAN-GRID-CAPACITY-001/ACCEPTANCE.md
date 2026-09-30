# NTSD28-Q09-P20-LOGAN-GRID-CAPACITY-001 限定验收

> **2026-09-29 更正：** 下文“飞段 pic119 被 hid2 首sheet拒绝、hid6不兜底、正式不发布本体”的具体结论无效。正式 DAT parser 按 `row*col` 累计运行时范围；pic119 实属 hid6。详[纠正记录](CORRECTION-20260929.md)与[原 Battle 自然命令](../NTSD28-Q09-P20-HIDAN-BATTLE-COMMAND-001/ACCEPTANCE.md)。保留下文原始验收文本供追溯；通用容量门的有限测试结果不受此特例解释更正替代。

状态：`FOCUSED_TEST_PASS / NATURAL_PIXEL_PENDING`；Q09/P-20、R17、BATCH-05及总目标均未整体关闭，Q07仍是最早未闭工作组。

正式根 `NTSD2.8-Logan.exe` SHA-256 为 `B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033`。配对 playable `render_snapshot.cpp::SpriteFrameResolver28::resolve` 对命中的第一张声明sheet先按 `row*col` 容量拒绝，且不尝试后续重叠sheet；`RenderSnapshotBuilder28::build` 因而不发布该帧本体图片。正式 `decoded_dat/c/hid/hid.dat` 首sheet为pic62–126、5×11，frame430的pic119在其范围内但局部序号57超过容量55；后续pic117–128的sheet不兜底。frame430被正式DAT的 `hit_Fa` 引用，但尚无正式根自然技能同帧像素证据。

旧Unity `BuildIndexedSpriteRects`按范围建立65个索引；正式PNG预热可对容量外的索引进入clamped-cell发布。新聚焦测试先在原Editor作RED，job `0780abf10c1246658da86de118cd21fd`，expected55/actual65；[原始结果](original-editor-red.json)。生产修复在Logan PNG加载入口限制索引数量到声明容量；非Logan/BMP和catalog既有范围行为显式保留，第一声明sheet归属未变。

原Editor PID11944的MCP全量刷新实际导入脚本，Tundra重编译生产与Editor程序集0 C#错误。精确GREEN job `a2417ae9e2844655a5f03c789260c292`：新容量用例、旧OID32边界用例、重叠sheet归属及catalog反序完成用例均PASS，4/4；[原始结果](original-editor-green.json)。`Tools/Validate-ChangeLedger.ps1` PASS；`git -c core.safecrlf=false diff --check` exit0。Editor终态为原Battle Scene、idle、非Play。四保护资源在[前](protected-before.json)/[后](protected-after.json) SHA-256逐项一致。

限制：此证据证明通用索引容量与相邻旧路径，不证明正式根frame430画面、原Battle自然技能可见性、missing-file/hidden-pic/terminal全矩阵，也不影响Q07/D-024碰撞域选择。DAT、PNG、Scene、配置Asset与非战斗脚本均未修改。下一次只在自然frame430或同类正式可达显示场景需要时补成对Play/像素，不将此4例扩跑全角色矩阵。
