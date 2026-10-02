<!-- CHANGE-RECORD
id: NTSD28-336B44-Q07-C043-FUSION-HELD-REACH-001
status: VERIFIED
change-kind: CODE
code-path: Tools/NTSD28Q07Diagnostics/c043_fusion_held_reach_probe.cpp
authority: selected 336B44 playable fusion and held-refill pass order with formal fusion and character DAT
evidence: docs/ai/TASKS/NTSD28-336B44-Q07-C043-FUSION-HELD-REACH-001.md
-->

# C043 融合伙伴持有子体自然入口诊断

脚本修改前记录。正式 `BattleWorld28::despawn` 先清关系；正式融合直接暂存并清空伙伴槽位，所检门未拒绝其持有 kind-2 子体；Tick Driver 有融合后的第二次 held-refill。Unity C043 共享失效尾已有受控 Play，但正式根自然同条件尚缺。此项只新增一次性正式源码诊断，不改正式源、DAT 或 Unity 生产。

预期副作用：新增一个 C++ 只读诊断入口和独立输出目录。受影响路径/符号仅 Task 声明的 `c043_fusion_held_reach_probe.cpp` 与其中 `main`、配置和逐 tick 导出；无运行时产品接线。验收必须将 DAT 子体生成、合体前关系、融合和尾清分开，并保留阴性结果。不可回退边界为已有脏文件、旧报告/测试结果、DAT、场景与用户资源。回滚仅按本记录精确审阅新文件，不未经批准删除或恢复。

实际实现、验证命令/结果、风险及下一出口待运行后追加。C043/Q07/总目标保持开放。

账本重复行更正预记录：本 ID 插入 Ledger 时按分隔行做全局替换，误在第5、697、737、1471行加入相同新行4次。将仅保留首条本 ID 行、移除其余3条本包刚新增的重复行；不删其它记录、文件或用户内容。修后核对仅1条，并运行 validator。

首次运行与夹具更正：C++17第二次编译（补-municode）exit0；正式资源三组×60tick双跑逐字节相同，但子体0、融合0。原探针把伙伴直接设frame257（wait0），首tick实际先推进到258，未在入帧前触发该frame的OPoint；方向输入单次或采样错位也未进入state2。这不是C043不可达证据。保留run-01/02与首轮编译日志；下轮只在本诊断脚本改用前驱frame256自然进入257，并用两次可采样的方向按下间隔测试run门，新输出目录不覆盖旧结果。

第二轮实际阳性：修正前驱frame256后，三profile×60tick独立双跑逐CSV同SHA。late_double_tap在tick6由正式Chi OID8 frame257生成OID420/slot50并持有至tick27；tick28两角色自然方向双按进入合体7+8→51，伙伴slot1休眠，第二轮held-refill显示linked_children1/success0、缺互指诊断、子体LinkState=-1→0，parent槽仍1；tick29不重复报错。late_hold/early_hold各60tick未合体，不能将两负例当普遍不可达。此为当前正式源码完整GameSession的自然OPoint+输入阳性，不是根EXE/Unity证书。下一按Task增录LFR并调整可运输初态。

LFR首轮失败与修正：GameSessionLfr28录包增量、base/current HP100已编译0错（第一次新增链接缺-lz失败保留，补-lz通过）；run-05/06在第一个profile60tick CSV写完后finish报cannot finish before capturing a tick，原因是本诊断遗漏每step后的capture_after_step。两失败目录及CSV原件保留，不当LFR证书；下一仅补该现有Recorder API并另用run-07/08。

限定完成：最终正式源码run-07/08三profile60tick CSV/LFR逐SHA同；正式根两次同录包exit0/passed/failure0，trace逐SHA同，选定slot0/1/50的60tick×14字段840/840零差，tick28同一失效关系诊断且次tick不重复。四正式/staged DAT比较：Lee/Chi/Pup原字节同，fusion仅换行差、正文规范化同。只读诊断本身VERIFIED；Unity原Scene与C043父项仍RUNTIME_PENDING。原件与失败记录均保留。[报告](../../../artifacts/diagnostics/NTSD28-336B44-Q07-C043-FUSION-HELD-REACH-001/REPORT.md)。

交付检查：`Tools/Validate-ChangeLedger.ps1 -RepositoryRoot <project>` exit0/PASSED，1126 Records、5个当前受治理代码文件全覆盖，本新增C++路径精确命中本ID；输出见[验证日志](../../../artifacts/diagnostics/NTSD28-336B44-Q07-C043-FUSION-HELD-REACH-001/change-ledger-validation-v2.txt)。`git diff --check` exit0，仅已有文件LF/CRLF转换warning。Battle/Menu场景、LoganRuntime与GameConfig/Mode Asset的窄Git状态无修改；无文件删除、Git恢复、提交或推送。
