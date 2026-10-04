# Q10 小樱高血量 `data/049.wav`、`c/saku/w/j2.wav` 正式战斗内容暂存

状态：`FORMAL_CONTENT_STAGED / UNITY_IMPORT_AND_VOICE_PENDING`。父目标 `NTSD28-UNITY-BATTLE-REALIGNMENT-001`，新版总表 BATCH-05/Q10。此 Task 在新增资源前建立，仅处理已证当前源码自然事件、正式根同 LFR 可比状态与 Unity 资源缺口的两条 cue。

当前正式根 EXE SHA-256 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3`。小樱 HP500、OID1/action0 的防→纵深上→攻 55 tick 链，当前 playable 在 tick21 发 `c/saku/w/j2.wav`、tick49 发 `data/049.wav`；正式根同 LFR 退出0/PASS，动作/MP/相机X 165/165，根 trace 无逐条音频。证据：[根回放报告](../../../artifacts/diagnostics/NTSD28-336B44-Q10-SAKURA-HIGHHP-049-J2-ROOT-001/REPORT.md)。正式049和旧Sound PCM不同；j2正式存在、Unity正式VFS和旧Sound皆缺。

精确变更仅新增 `Assets/NTSD/Content/LoganRuntime/vfs/data/049.wav`、`Assets/NTSD/Content/LoganRuntime/vfs/c/saku/w/j2.wav` 与两个对应 `.meta`。WAV原始字节必须分别等于正式 `resources/runtime/vfs` 同路径文件，GUID全Assets唯一，沿用同目录 AudioImporter 格式。复用 battle-only 单文件正式根优先解析；不覆盖、移动或删除旧Sound，不改 DAT 的任何数据、生产脚本、相机、Scene、Prefab、背景/模式或非战斗路径，不为单 cue 写生产特例。

写入前确认两目标及meta不存在，记录正式/旧 WAV、已存在上级meta、Battle/Menu/两配置 SHA 与原 Editor clean/非Play/无测试状态；独占新建后逐SHA、WAV头、GUID唯一性和保护文件/旧Sound不变均需复核。原 Editor 持续编译时不请求刷新、测试或 Play，本包最多标 `FORMAL_CONTENT_STAGED / UNITY_IMPORT_AND_VOICE_PENDING`。Editor恢复后分别用既有小樱高血量输入完整生产链核对 tick21/49 的正式SourcePath、AudioClip、battle voice、Mixer输出及有序退出；根EXE扬声器、其它cue与全混音另验。失败原件保留；任何撤销/删除按 `docs/ai/file-removal-audit-contract.md` 建逐文件记录并取得所需授权。

2026-10-04 执行结果见[报告](../../../artifacts/diagnostics/NTSD28-336B44-Q10-SAKURA-049-J2-FORMAL-WAV-STAGE-001/REPORT.md)：两正式WAV各自逐SHA同版、GUID唯一，旧Sound及四保护SHA不变；原Editor仍停编，导入与voice待验。
