# Q09/P-20 正式对象 sheet 文件可用性限定核对

状态：`STATIC_SCOPED_SELECTED_RESOURCE_BYTES_MATCH / MISSING_FILE_NEGATIVE_BRANCH_UNTESTED`。本项只处理正式当前内容的已声明战斗对象 sheet，不能关闭 P-20/Q09/BATCH-05。

以正式根 `NTSD2.8-Logan.exe` SHA-256 `B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033` 和正式 `resources/runtime/decoded_dat` 为内容身份。已有 `NTSD28-Q07-ALL-FRAME-SPRITE-COVERAGE-001/frame-sprite-static-audit-20260925.json` 将正式对象范围限定为330份 DAT、773个 sheet 声明；既有 `NTSD28-B11-CONTENT-ENTRY-INVENTORY-001/native/authority-content.jsonl` 提供对应 `sprites[].path`。本轮只读抽取全部773条路径，规范 `\` 为 `/` 后去重为703张 PNG。逐一查找正式 `resources/runtime/vfs` 与 Unity `Assets/NTSD/Content/LoganRuntime/vfs`，两端缺失均为 **0/703**；逐文件 SHA-256 比较，差异 **0/703**。这是按实际声明路径核对，不以 VFS 文件总量或目录名猜测消费者。

因而，在当前这组已选择、已部署的正式对象 sheet 下，正式 `SpriteFrameResolver28::resolve` 的“文件不存在”分支没有由静态内容清单引起的必然触发，Unity 也不存在对应 sheet 文件的字节缺口。此结论不覆盖额外动态选入的内容、原版背景/模式（用户排除）、原生 HUD/UI（用户保护或排除）、坏路径、读权限/解码故障，亦不验证人为移走文件后的失败处理。`pic` 超出声明范围/容量、sheet 布局错误、图外 CLAMP 和可见 GPU 属其他独立门；703/703 SHA 相同不证明它们的运行时结果相同。

本轮没有运行正式 EXE、Unity 编译或 Play，没有修改 DAT、PNG、生产脚本、Scene、配置或非战斗功能。下一次 P-20 应只在正式可达缺图负例、隐藏/terminal 或新的同帧首差出现时开定向门；不因库存总数重复暂存当前已全同的703张。
