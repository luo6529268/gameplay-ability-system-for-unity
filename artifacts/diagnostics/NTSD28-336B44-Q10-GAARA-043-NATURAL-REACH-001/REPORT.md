# Q10 我爱罗 043 自然可达性

结论：当前 336B44 playable 中，OID16 从 action0 按“防 2 tick→前 2 tick→攻 2 tick”自然进入 sand_blast；tick13 产生 `c/gaa/w/j4.wav`，tick15 到 action244 并产生 `data/043.wav`。这是正式战斗音频的可达事件，不是只从 DAT 静态声明推断。

- `build-01` 用当前 playable Core/host 闭包编译独立探针，exit 0、stderr 空。`run-01`/`run-02` 各执行 40 tick，CSV SHA-256 均为 `AF0A14A63C53DBAD7C7A500D08F71FAD7699950D2724D1C3D4276889B237F3CC`，LFR SHA-256 均为 `E22F52123DA1E7287959AB9964AEEAF58D8C42185A556990D904053BE9D1D9C1`。
- 根目录正式 EXE 按同 LFR 的 `root-02` 回放 exit 0，报告 `passed=true`、`failureCode=0`。`comparison.json` 对 40 tick 的 phase/action/MP/X/Y/Z 共 240 项比对，首差为空；正式公开 trace 不含 audio 字段，因此根 EXE 的设备发声尚未由该 trace 证明。
- 正式与 Unity 暂存的 `c/gaa/gaa.dat` SHA-256 均为 `AF81C7FCC483F7CD7566EA65CE27B005BECF72B680FF4E4B1D7635EA21DAA37A`。Unity 正式 VFS 缺少自然事件中的 j4、043；旧 Sound 没有 j4，旧 043 的 PCM 与正式版不同。tick10 的 007 旧 PCM 相同，故不在本包复制。
- `protected-before.json` 初次写错配置资产路径，原件保留；正确路径及哈希在 `protected-before-v2.json`。首次 GUI 回放的 `root-01` exit 文件为空，故另以显式进程等待取得 `root-02` 的有效 exit 0；未用首次 exit 作结论。

本诊断只新增 `Tools/NTSD28Q10Diagnostics/gaara_sand_blast_043_probe.cpp`，未改生产代码、DAT、Scene 或旧资源。它闭合自然可达性，不闭合 Unity 自然声音播放；接入和下一验证见同主题 WAV 暂存 Task。

后续补证：两正式WAV已暂存，原Scene自然j4/043均同tick进入正式clip与池化voice，详[后续场景报告](../NTSD28-336B44-Q10-GAARA-J4-043-NATURAL-VOICE-001/REPORT.md)。上段“未闭合”是本次源/根诊断自身出口，并非当前总表状态。
