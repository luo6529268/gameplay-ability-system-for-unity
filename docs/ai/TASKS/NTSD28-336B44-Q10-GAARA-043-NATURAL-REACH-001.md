# NTSD28-336B44-Q10-GAARA-043-NATURAL-REACH-001

状态：`VERIFIED / SCOPED_SOURCE_AND_ROOT_REACH`。父目标 `NTSD28-UNITY-BATTLE-REALIGNMENT-001 / BATCH-05 / Q10`，服从 336B44 总表的首差触发与停排规则。

权威与前置：正式根 EXE SHA-256 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3` 及其 playable live source。正式 `data.txt` 将我爱罗 `c/gaa/gaa.dat` 编为 OID16；该 DAT 的普通站立帧 `hit_Fa:240`，`sand_blast` 240→244，末帧声明 `data/043.wav`。正式与 Unity 当前暂存 DAT 的语义需在运行前核对。Unity 当前没有正式暂存的 `data/043.wav`，旧 `Sound` 的同路径 PCM 与正式文件不同。静态声明和预估按键都不是自然发声证据，不能据此修改资源。

精确代码范围：仅新增 `Tools/NTSD28Q10Diagnostics/gaara_sand_blast_043_probe.cpp` 的 `record_case`/`wmain`。复用当前 playable `GameSession28`、`GameSessionLfr28`，mode0、OID16/action0、远距中性 OID7，从真实离散“防→前→攻”输入出发，不手动设 action240/244、不插入音效。最多一个固定短窗口；每 tick 记录动作、输入相位、选定状态与 `last_tick().audio_events`，生成 LFR。不得改正式源码/EXE、DAT、Unity 生产/测试脚本、WAV、图、Scene、模式或非战斗功能。

出口：用正式 playable Core/host 编译闭包编译该独立探针，按同配置双跑并逐字节核对 CSV/LFR。若实际到 244 并发出 043，再由当前根 EXE 回放同 LFR，核对公开动作/位置/MP 等字段并记录根 trace 不含音频的限制；只有这时才把 Unity PCM 缺口列为 Q10 首差候选并另开准确资源接入。若本次输入阴性，记下动作链、原因与有限边界并停止，不扩大站位/随机扫描。相关保护文件只读 SHA；运行 Change Ledger validator 与定向 diff check。回滚采用前向更正并保留原始样本，未经文件操作审计不得删除新旧内容。

执行结果：[原件报告](../../../artifacts/diagnostics/NTSD28-336B44-Q10-GAARA-043-NATURAL-REACH-001/REPORT.md)。40 tick 双跑 CSV/LFR 分别逐字节同 SHA，正式根 EXE 回放 exit 0/pass；phase/action/MP/X/Y/Z 240/240 同、首差为空。自然 tick13 j4、tick15 043 阳性。根公开 trace 不含声音字段，正式设备发声和 Unity 场景 voice 未由本 Task 验证；后者转入精确 WAV 暂存 Task。
