# NTSD28-336B44-Q10-GAARA-043-J4-FORMAL-WAV-STAGE-001

状态：`VERIFIED / FORMAL_CONTENT_AND_SCOPED_NATURAL_VOICE`。父目标 `NTSD28-UNITY-BATTLE-REALIGNMENT-001 / BATCH-05 / Q10`；自然事件证据为 `NTSD28-336B44-Q10-GAARA-043-NATURAL-REACH-001`，不扩大到静态清单的其它 WAV。

触发：正式根 EXE SHA `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3` 对应 playable 源码，正式 OID16/action0 的“防2→前2→攻2”在两次相同40tick运行中于 tick13 发 `c/gaa/w/j4.wav`、tick15 发 `data/043.wav`，CSV/LFR逐字节相同；根EXE回放同LFR `passed=true` 且公开动作/MP/位置/输入相位240/240同。根公开trace无audio，不称根设备录音。正式/Unity `gaa.dat` 逐SHA同版。Unity正式VFS两cue均缺；旧Sound没有 j4，043 的PCM与正式不同。`data/007.wav` 同链tick10虽正式VFS缺，但旧PCM同版，本Task不复制它。

精确接入路径：仅新建 `Assets/NTSD/Content/LoganRuntime/vfs/c/gaa/w/` 与 `w.meta`、`j4.wav` 与其 `.meta`，以及 `Assets/NTSD/Content/LoganRuntime/vfs/data/043.wav` 与其 `.meta`。正式 WAV 原字节复制；三个 meta 使用独立且全 Assets 不重复的 GUID，沿用同格式战斗音频/文件夹 importer。目标当前均不存在，不能覆盖。不得改旧 `Sound`、DAT、脚本、Scene、Prefab、地图、模式、菜单或其它资源。若出现目标或 GUID 冲突，停止而不覆盖。

验收：执行前记录正式两文件SHA/格式、目标不存在、旧043 SHA/PCM和四保护文件SHA；执行后两WAV与正式逐SHA相同、GUID唯一、四保护文件与旧Sound不变。随后只做原Editor安全状态下的导入/AudioClip检查；自然tick13/15的 `PendingSoundEvent`→正式 `AudioClip`→战斗voice/SourcePath 是运行时出口，未跑之前保持 `RUNTIME_PENDING`。生产播放器已有共用正式优先入口，不加我爱罗分支；若这一入口发生可复现失败，再按新首差建代码包。没有删除或覆盖授权；若需撤销，先按文件操作审计合同记录并取得相应授权。

当前结果：[暂存与导入原件](../../../artifacts/diagnostics/NTSD28-336B44-Q10-GAARA-043-J4-FORMAL-WAV-STAGE-001/REPORT.md)。正式/暂存两 WAV 分别同 SHA，GUID 唯一，五项保护哈希稳定；原 Editor 刷新后两者均识别为 `UnityEngine.AudioClip`。尚未执行原 Battle Scene 的自然 event→clip→voice 见证，不得将此 Task 或 Q10 标成声音已播放。

后续限定补证：[原Scene自然voice报告](../../../artifacts/diagnostics/NTSD28-336B44-Q10-GAARA-J4-043-NATURAL-VOICE-001/REPORT.md)已在同正式动作窗口中见tick13/15的两正式clip各启动池化voice，且退出clean/保护哈希稳。上一段是部署当时快照，现由此补证覆盖；Q10整体及设备PCM仍不能称完全一致。
