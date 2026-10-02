# Q10/C053 正式战斗 WAV 的 Windows Player 内容闭包

状态：`VERIFIED / WINDOWS_PLAYER_SCOPED_PASS`。03构建/Player复核通过，默认Q07基础报告15字段结构保持；其它Q10出口开放。Windows Player 正式 020/067 WAV 内容闭包、路径、解码与合成战斗播放入口已验证；Q10 整组及设备/其它 cue 仍开放。[结果](../../../artifacts/diagnostics/NTSD28-336B44-Q10-C053-PLAYER-AUDIO-PATH-001/REPORT.md)。父目标 `NTSD28-UNITY-BATTLE-REALIGNMENT-001`，新版总表 BATCH-05/Q10，兼顾 BATCH-04/Q07 的 Windows 内容出口。唯一战斗权威为根正式 EXE SHA-256 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3`、对应 playable live path 和正式 `resources/runtime`；项目原背景/地图和模式 Asset、D-024 比例域及其它用户例外不变。[首差](../../../artifacts/diagnostics/NTSD28-336B44-Q10-C053-PLAYER-AUDIO-PATH-001/FIRST-DIFFERENCE.md) 已核现有 1371 文件 v2 打包清单与当前 1373 文件源不一致，且播放器以 Player `Application.dataPath` 误找正式 WAV。

精确写范围：

- 新增 `artifacts/diagnostics/NTSD28-336B44-Q10-C053-PLAYER-AUDIO-PATH-001/copy-manifest-v3.csv`，以当前 1373 个非 meta 暂存文件建立稳定按相对路径 Ordinal 排序的原字节 SHA/长度清单；每项必须有正式同相对路径，原字节相同或严格 CRLF→LF 后逐字节相同。明确不纳入用户排除的两类 mode DAT 和背景 DAT，保留 v1/v2 历史清单。
- 只改 `Assets/NTSD/Scripts/Test/Editor/NTSD28Q07WindowsContentBuildProcessor.cs` 的版本化清单路径、确切文件数和总字节数，维持原有失败即停止、无覆盖和目标目录逐 SHA 校验。不得改非 Windows 平台逻辑。
- 只改 `Assets/NTSD/Scripts/App/NTSDSoundPlayer.cs` 的正式战斗单文件根解析，使其与已配置的 `GameConfig.BattleContentRuntimeRoot` 同根，在 Editor 与 Windows Player 都解析到 `vfs/data`。旧 `Sound` 和通用 `PlaySfx`、封存缓存、声音参数与战斗事件顺序不变。不得硬编码两个 cue 或改 DAT 数值。
- 只按验收需要扩展 `Assets/NTSD/Scripts/Test/Editor/SoundPresentationDispatchEditorTests.cs` 的正式内容根聚焦断言，及 `Assets/NTSD/Scripts/Test/NTSD28Q07WindowsPlayerRuntimeProbe.cs` 的**显式 opt-in** Q10 报告字段：建成 Player 后验证正式两个 WAV 路径、采样帧和可用播放器，复用现有有序关闭/借用0检查。默认 Q07 probe 行为与输出不变。
- 记录/状态限本 Task、同 ID Change、Ledger、STATE、handoff、新版对齐总表及诊断结果。保留现有所有未提交修改、场景、配置、老 WAV、资源和打包输出。

验收：新清单精确覆盖当前文件集合，1373 行且总字节数与磁盘相同，无重复、越界、缺失或多余；全部正式同路径存在且原字节同或仅换行等价；v2 SHA 不变。原 Editor 编译 0 error 与具名聚焦测试通过；使用现有原 Editor 的唯一新 runId Windows Mono Development 构建，postbuild 对新清单源/目标逐 SHA 通过，非战斗 Menu/Battle 设置及保护文件 SHA 不变。若运行 Player，使用隐藏窗口而非 computer-use，显式 Q10 opt-in 报告真实正式 clip 路径/帧数、战斗 World 与关闭借用0；绝不把静态清单或 Editor Play 当成 Player runtime 通过。运行 `Tools/Validate-ChangeLedger.ps1` 和 `git diff --check`。

风险与回滚：新清单若错会阻断 Windows 构建；Player 端正式路径若错会落回旧 WAV。失败保留原始构建/运行报告，不覆盖或清理输出。回滚仅按此 Change 的精确文件差异审查并遵循仓库显式批准规则；本任务不执行删除、Git restore/reset/clean 或旧资源替换。




