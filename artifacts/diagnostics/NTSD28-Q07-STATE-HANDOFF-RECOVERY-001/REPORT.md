# 2026-09-24 STATE / handoff 写入故障与恢复

本轮在回写 `NTSD28-Q07-FRAME-PREVIEW-FORMAL-INDEX-001` 时，PowerShell 字节前缀写入错误使 `docs/ai/STATE.md` 和 `Assets/NTSD/Docs/CODEX-CURRENT-HANDOFF.md` 在最新两条 Q07 说明后被 NUL 字节覆盖。首次发现于 `Tools/Validate-ChangeLedger.ps1` 报大量 active Change ID 缺失；这是文档损坏，不是这些 Change ID 被关闭或删除。发现后停止脚本工作并检查两个文件，损坏时 SHA-256 分别为 `496a8a7abd7daf44cced8dc6975157ee992d1cfb1650ff1291c3d9b28d4b605d` / `277962809cd38946c11b060c9d57a06f152d6e4a98293a916c47f9cd0b07ee4f`，首个 NUL 均在偏移 877。

恢复使用原仓库 `HEAD` 中两个文件的**完整正文**（820,954 / 710,574 字节），前置当前未损坏的 Q07 对齐总表顶部八项进度（5,217 字节）。写入先经过临时文件、逐字节验证、再单文件替换。恢复后文件长度分别为 826,171 / 715,791 字节，SHA-256 分别为 `a4f2de94275fde1ba71904a57e6281a6df3cacddb5d1ab9c435d766438f48b58` / `ef60421b8080f4114df26dff0677c09dc3ea7a5546add98e402117b1270c7c70`，均无 NUL，且完整 `HEAD` 正文逐字节是其后缀。`git diff --numstat` 对两个文件各为 `+16/-0`，无正文删改。

损坏前未提交的顶部流水账原文不能逐字节证明已全部恢复；其中 Q07 模式替换、资源排除、旧图片扫描、Inspector 和预览窗口的当前事实已由对齐总表、各 Task/Change Record 和本次恢复前仍完整的诊断报告覆盖。请将 `STATE.md`/handoff 顶部视为从可验证资料重建的当前摘要，而非原字节备份。恢复后 `Tools/Validate-ChangeLedger.ps1` 重新 `PASSED`（780 Records / 15 governed code files），`git diff --check` 通过。没有因此改动 C#、DAT、图片、Scene 或 Prefab。

随后在两文件顶部追加了指向本报告的恢复说明；最终 SHA-256 分别为 `63A6ABEA8B2598CBD2B6858C970F2B957FDED055CAC83905C78CF1996B2B4EE9` / `8699BA720DE46D7EE1D1435799CB583576D59ED04A4972E360E97D52C5E6C72C`，仍无 NUL 且完整 `HEAD` 正文保持逐字节后缀。上段长度/哈希是追加说明前的重建检查点。
