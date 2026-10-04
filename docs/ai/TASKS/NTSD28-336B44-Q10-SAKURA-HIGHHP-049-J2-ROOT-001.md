# Q10 小樱高血量自然事件 `049` / `j2` 当前根回访

状态：`ROOT_SELECTED_FIELDS_165_OF_165 / FORMAL_PCM_GAP / UNITY_VOICE_PENDING`。父目标 `NTSD28-UNITY-BATTLE-REALIGNMENT-001`，新版总表 BATCH-05/Q10。仅对已有当前 playable 55 tick 输入记录做正式根 EXE 回放与文件内容比对；不改 Unity、正式发行资源或既有诊断原件。

权威：根 `NTSD2.8-Logan.exe` 必须先复核 SHA-256 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3`。已有 `NTSD28-336B44-Q10-SAKURA-STEREO-NATURAL-REACH-001/source-run-04` 是 HP500、OID1 小樱从 action0 走防2 tick→纵深上2 tick→攻2 tick 的正式源码负控制；它没有进入低血量 `tra.wav` 分支，却在源码完整 GameSession 的 tick21 发 `c/saku/w/j2.wav`、tick49 发 `data/049.wav`。该目录含对应 LFR 和逐 tick CSV，不把历史阴性误写成音频全阴性。

先只读核对源 CSV/LFR 内容身份与正式 WAV/旧 Sound 的原始字节及 PCM。然后将**这一份现成 LFR**传给正式根 EXE 的 `--headless-playback-lfr`，新建唯一 report/trace 输出文件，取得明确退出码、report `passed`/`failureCode`/声明与完成 tick。仅将根公开的 actor action/MP/camera 等同名字段与源码同 tick 对照；根公开 trace 没有 audio 字段，因此此步骤不能声称根扬声器播放 `049` 或 `j2`。输入夹具初态、HP、mode、stage、seed 必须来自 LFR 与已存 probe，而不是推断。

如果可比字段一致且 PCM 缺口真实，再独立决定是否补正式 WAV；任何资源新增先做逐文件 Task/清单，不批量搬 970 条。原 Editor 当前仍停编，不运行 Unity Play、Refresh 或 Test Runner。保持 DAT 任意数据、背景/模式、Scene、生产与非战斗路径不变。失败原件保留并如实报告；不改正式源码或诊断工具来迎合根结果。

2026-10-04 结果见[报告](../../../artifacts/diagnostics/NTSD28-336B44-Q10-SAKURA-HIGHHP-049-J2-ROOT-001/REPORT.md)：根回放退出0/PASS、55tick×3字段165/165同值；源码tick21/j2与tick49/049事件已记录，正式049/旧Sound PCM不同、j2两Unity音频根均缺。根音频与Unity voice未验。
