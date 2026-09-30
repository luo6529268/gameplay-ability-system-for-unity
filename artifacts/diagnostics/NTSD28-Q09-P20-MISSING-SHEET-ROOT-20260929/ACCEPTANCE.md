# Q09/P-20 正式根 EXE 缺单张角色 sheet 运行见证

状态：`VERIFIED_SCOPED_FORMAL_MISSING_SHEET_RUNTIME / UNITY_REPAIR_PENDING`。Q07/BATCH-04 仍是最早未闭组；P-20/Q09/BATCH-05/总目标均开放。

正式根 `NTSD2.8-Logan.exe` 的 SHA-256 为 `B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033`。复用已通过的自然 Hidan 30 tick LFR（SHA-256 `846213B9534D8444A04B188B66603C17F50E3B3862CD98D20707B45FD8A59E3D`），不添加 action override。独立资源根位于 `C:\Users\Logan\AppData\Local\Temp\NTSD28-Q09-P20-missing-hid6-root-20260929`：`decoded_dat`、非 `c` VFS 文件夹及其他角色目录仅指向正式资源；`catalog.csv` 和 Hidan 其余九张 PNG 是校验相同的副本；只有 `vfs/c/hid/hid6.png` 缺失。正式原文件未移动、删除或改写。首次尝试跨盘 hardlink 被系统拒绝，后改复制这十个小文件，九张 PNG 与目录逐 SHA 一致，未将失败的 hardlink 当成有效夹具。

以 `--resource-root` 和 `--complete-vfs-root` 指向该隔离根，运行**未改动的正式根 EXE**的 `--headless-playback-lfr`。新[报告](formal-root-missing-hid6-report.json)为 `passed:true / failureCode:0 / declaredTicks:30 / completedTicks:31`，诊断 trace 写入32行。报告 SHA-256 为 `AD00F8ED6B91B32430C53BD92B317E80EA2783430C38381BE2F5A1564B77526D`；新[trace](formal-root-missing-hid6-trace.jsonl) SHA-256 为 `6AE31ED931BDD6D2324E35E0E0CB6A5454D307444CA4EB95B46DA5C7057D311D`。Windows GUI 子系统的 PowerShell 直接调用即时返回且当时报告未落盘；随后新鲜读取报告、trace并确认正式进程已退出，最终状态以文件为准，不把即时 `$LASTEXITCODE=null` 当成功码。

机器[逐行对比](comparison.json)（SHA-256 `676E5078A6205D7D3FD88E9178078C23C10064B06F2AE6F843E25602DF47EB3A`）读取既有完整资源根 [root trace](../NTSD28-Q09-P20-HIDAN-NATURAL-FRAME430-001/formal-root-frame430-trace.jsonl)，32/32 对齐 tick，World `stateHash` 差异 **0**；16 行的 `render.sprites` 从完整根的2变为缺图根的1，恰在 tick16–31，slot0先自然进入action430/pic119，随后使用同一缺失sheet中的pic120–126。tick16 实测 actor action430/pic119/MP150，World hash `b0f7fb5b4573eef8`。这是根 EXE 对缺失角色本体sheet仍加载战斗、只省略相关绘制命令的可观察证据；headless trace不等于正式GPU同视口像素。

原Unity Editor聚焦测试 `NTSD28B11VisualContentCandidateEditorTests.MissingRequiredImage_DoesNotCreateCandidate` 已1/1 PASS，固定的是目前缺图拒绝整份候选。正式根运行证据现已补齐；该Unity生产差异仍未修复。下一包须按[Task](../../../docs/ai/TASKS/NTSD28-Q09-P20-MISSING-SHEET-ROOT-001.md)的通用候选/新鲜度边界修复，不得为了让测试通过而复制图片、吞掉所有文件异常或修改DAT。此包未修改 Unity、正式source、DAT、PNG、Scene、Prefab、模式Asset或非战斗逻辑，未运行Unity测试/Play。
