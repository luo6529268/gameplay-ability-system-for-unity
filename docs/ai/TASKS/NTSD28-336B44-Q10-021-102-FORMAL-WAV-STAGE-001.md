# Q10 自然可达 `data/021.wav`、`data/102.wav` 正式战斗内容暂存

状态：`FORMAL_CONTENT_STAGED / UNITY_IMPORT_AND_VOICE_PENDING`。父目标 `NTSD28-UNITY-BATTLE-REALIGNMENT-001`，新版总表 BATCH-05/Q10。此 Task 在资源写入前建立；只处理两条已有当前正式源码自然事件和旧 PCM 差异的战斗 cue。

## 权威与首差

当前正式根 EXE SHA-256 为 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3`。正式君麻吕 OID507 自 action0 防→纵深上→跳后，当前 playable 完整 GameSession 从 tick9 开始由衍生对象多次发出 `data/021.wav`；该 55 tick LFR 的根 EXE 动作/MP/相机与源码 165/165 同态，但根公开 trace 没有逐条音频字段。正式多由也 OID36/action243 命中鸣人 OID2 的 128 tick 链在 tick1、23 发 `data/102.wav`；原 Battle Scene 的生产待播事件与当前源码全部 6 条按 tick、顺序、cue、X、全局偏移 30/30 相同，正式根已通过同字节 LFR 回放，但根公开 trace 同样不导出音频。依据分别为 `NTSD28-336B44-Q10-KIM-J1-NATURAL-REACH-001` 和 `NTSD28-336B44-Q10-C032-NATURAL-HIT-AUDIO-001` 的原始 CSV/报告。

当前 Unity 正式 VFS 根缺这两个 WAV，战斗单文件查找会落到旧 `Assets/NTSD/Sound/data`。只读 PCM 核对显示两条旧 WAV 均不同于正式版：021 皆为单声道 8-bit/38400 Hz/34310 帧但 PCM SHA 不同；102 正式为单声道 8-bit/16000 Hz/35777 帧，旧版为单声道 16-bit/16000 Hz/35777 帧。文件差异不能单独证明设备听感差异。

## 精确修改范围和边界

- 只新增 `Assets/NTSD/Content/LoganRuntime/vfs/data/021.wav`、`102.wav` 及各自 `.meta`，WAV 必须与正式 `resources/runtime/vfs/data` 原始文件逐 SHA 相同；使用唯一 Unity GUID，沿用相邻正式 WAV importer 设置。
- 不覆盖、移动、删除旧 Sound，不修改 DAT 的任何数据、生产脚本、Scene、Prefab、相机、背景、模式、菜单或其它非战斗功能。复用现有 battle-only 正式文件优先解析，不写逐 cue 生产特例。
- 原 Editor 当前虽测试已停、Battle Scene clean，但持续报告 `is_compiling=true`；本包只暂存磁盘文件，不发 Refresh、Test Runner 或 Play 请求。若导入/解析暴露通用缺陷，另建准确脚本 Change Record 后处理。

## 验收、风险与回滚

写入前保存正式与旧 WAV、目标不存在状态、Battle/Menu/两配置 SHA；以独占新建方式写目标，写后复核正式/暂存全文件 SHA、两个新 GUID 的 Assets 唯一性及旧 WAV/四保护文件不变。先只能报告 `FORMAL_CONTENT_STAGED / UNITY_IMPORT_AND_VOICE_PENDING`。Editor 恢复后用原 Battle Scene 既有两条自然链分别查导入 clip、正式 SourcePath、待播到 battle voice、实际输出及有序退出；根公开音频字段、正式 EXE 扬声器、动态声像和其它 cue 仍独立待验。若失败保留失败原件与新增内容，不擅自删文件；撤销须按 `docs/ai/file-removal-audit-contract.md` 的逐文件规则审计并获得所需授权。

2026-10-04 执行结果见[报告](../../../artifacts/diagnostics/NTSD28-336B44-Q10-021-102-FORMAL-WAV-STAGE-001/REPORT.md)：两正式 WAV 各自逐 SHA 同版，新 GUID 唯一，旧 Sound/四保护文件不变；原 Editor 仍停编，本包未运行导入、Play 或设备输出。
