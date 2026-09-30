# Q09 探针 GameConfig 自动退场限定验收（2026-09-28）

状态：`VERIFIED_SCOPED_DIAGNOSTIC_LIFETIME`。本项只验证两个已有 Q09 Editor 探针的测试配置归还，不关闭 Q09/P-12、Q09 或 BATCH-05，也不计入 Q07 出口。

在原 Unity Editor PID 11944 中打开保存的 `NTSD_Battle` 后，向既有请求文件提交一次 `legacyBoot=true`、slot9、X500 请求。原始 [Legacy 报告](../NTSD28-Q09-P12-KARIN-UNITY-COMMAND-001/karin-slot9-auto-exit-20260928-01.json) 的状态为 `LEGACY_BODY_OBSERVED`、后端 `LegacyOnly`，对象/槽/池借用分别为 4/4、2/2、2/2，Battle Scene 前后哈希相同。退出 Play 时新 `EnteredEditMode` 回调实际写入 [退场记录](auto-exit-retirement.txt)：`retired=1`、`remainingProbeClones=0`、`singletonIsSavedAsset=True`。退场记录在这次请求前不存在；其 SHA-256 为 `CC2FC851ADC00349C026F38FE5B15D8E4E584FFC22BAA7E87B00255D8B85A38B`。Legacy 报告自身的 `backendRestored=false` 记录于退出回调之前，不能代替该回调的结果。

随后在同一原 Editor、同一 Battle Scene 提交 `legacyBoot=false` 的 Central 对照。[Central 报告](../NTSD28-Q09-P12-KARIN-UNITY-COMMAND-001/karin-slot9-central-after-auto-exit-20260928-01.json) 为 `FALLBACK_OWNER9_COMMAND_MATCH`、后端 `CentralOnly`，命令数 7，对象/槽/借用仍为 4/4、2/2、2/2，Battle Scene 前后哈希同值。两个 JSON 的 SHA-256 分别为 `AE1119D81BAD8677584F74FE29F75D13C22581916498027A48E264AF996021F7` 和 `1C740490808794826297A14BE0AE7EB04AE67AA6512828530B75B61452DF15E4`。这证明自动退场未把随后 Central 入口留在 Legacy 配置上。

前后保护：Battle Scene SHA-256 `2EE465D83C7169A0589447F437E37CAEFF3CC6F1BA6C3AAA55B8068F2B48B77A`，Menu Scene `785F828C4E64182BEA214E4794B198E3C82E3C42002FDADD3932A7E061B81E13`，保存的 GameConfig Asset `0527D737A1FA38FC56B51D00DC6E96A421D3C67222546368B147C2D074CB8EA7`，均未改变；项目 mode Asset 在 Git 中保持 clean。第二请求完成后请求文件显示 `requested=false/running=false`，原 Editor 窗口有响应，Editor.log 记载加载了 `Temp/__Backupscenes/0.backup`；本轮最后一次退出后的 MCP 6402 端口未恢复，故没有独立的最终 `get_editor_state` 空闲快照，不把连接状态写成已验。

本轮未修改生产/测试脚本、DAT 数值、图片、Scene、保存的 Asset 或非战斗流程；只产生上述请求和证据文件。此前脚本编译与导入的 0 错误证据见 [代码/导入记录](AUTO-EXIT-CODE-20260928.md)，本轮未因无新脚本改动而重复编译或全量案例。`Q07/D-024` 的近/远碰撞域选择和 `Q09/P-12` 的其余正式画面出口仍开放。
