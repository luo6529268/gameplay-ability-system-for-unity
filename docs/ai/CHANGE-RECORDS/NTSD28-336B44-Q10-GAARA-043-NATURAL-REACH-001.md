<!-- CHANGE-RECORD
id: NTSD28-336B44-Q10-GAARA-043-NATURAL-REACH-001
status: VERIFIED
change-kind: CODE
code-path: Tools/NTSD28Q10Diagnostics/gaara_sand_blast_043_probe.cpp
authority: 336B44 formal playable GameSession28 and current Gaara OID16 sand_blast DAT route
evidence: docs/ai/TASKS/NTSD28-336B44-Q10-GAARA-043-NATURAL-REACH-001.md
-->

# NTSD28-336B44-Q10-GAARA-043-NATURAL-REACH-001

脚本改动前记录。当前正式 `gaa.dat` 的普通站立帧提供 `hit_Fa:240`，240→244 的末帧有 `data/043.wav`，但尚无同输入正式自然事件。Unity 正式音频根没有 043，旧 Sound 的 PCM 与正式不同；不能把静态 DAT 声明当作已证战斗首差。

仅拟新增 Task 所列 C++ 诊断脚本 `record_case`/`wmain`，从 OID16/action0 的离散“防→前→攻”记录完整 tick、音频事件与 LFR，不改任何运行时生产、DAT 数值、资源或 Scene。预期副作用为一个新诊断目录中的 CSV/LFR/编译与根回放原件。风险是组合键输入窗口或角色条件导致阴性；阴性只裁决该有限输入。验收、保护范围及不删除的前向回滚见 Task。实际文件、命令与结果须在运行后补写，状态不得预先升格。

实际改动：仅新增 `Tools/NTSD28Q10Diagnostics/gaara_sand_blast_043_probe.cpp`，负责从正式 `GameSession28` 记录自然按键、音频事件、逐 tick 状态和 LFR。未改生产、DAT、旧音频、Scene。用 `build-01/compile-argv.txt` 记录的当前 playable Core/host 编译命令，exit 0、stderr 空；两轮 40 tick CSV/LFR 各同 SHA；根目录正式 EXE 对同 LFR 的 `root-02` 回放 exit 0/pass，六公开字段 240/240 无首差。tick13 j4、tick15 043 自然可达。完整命令、SHA、原始失败尝试、限制与前向回滚见[报告](../../../artifacts/diagnostics/NTSD28-336B44-Q10-GAARA-043-NATURAL-REACH-001/REPORT.md)。根 trace 没有音频字段，Unity 自然 voice 尚待独立验证；本 Record 只关闭源/根可达性。
