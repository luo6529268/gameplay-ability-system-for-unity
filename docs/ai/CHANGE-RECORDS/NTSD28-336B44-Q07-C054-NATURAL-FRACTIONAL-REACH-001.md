<!-- CHANGE-RECORD
id: NTSD28-336B44-Q07-C054-NATURAL-FRACTIONAL-REACH-001
status: RUNTIME_PENDING
change-kind: CODE
code-path: Tools/NTSD28Q07Diagnostics/fusion_natural_fractional_reachability_probe.cpp
authority: 336B44 playable battle_world advance_native_fusions and formal fusion record2
evidence: docs/ai/TASKS/NTSD28-336B44-Q07-C054-NATURAL-FRACTIONAL-REACH-001.md
-->

# C054 自然小数解融合可达性诊断

脚本前登记。仅新增正式源码链接诊断，不修改正式规则、DAT或Unity生产。源侧旧受控样本通过人工置计时0及精确小数得到分支，但尚缺自然减时到零且非整数的完整tick见证。本探针用正式第二条融合记录和真实输入/物理跑计时，不注入精确坐标或强制拆分。风险：OID52 action310是正式零帧、可能自然终止或静止；阴性仅限本Task所列初态和输入，不能推成规则不可达。验收、回滚见Task；失败保留原件、不为绿色改DAT。

2026-10-01 实际初版：新CPP按第二记录OID10/11→52跑240tick；v1编译0错但范围循环警告，v2编译0警告、第一原件保留。v2中性自然融合1/拆分201/小数无，后续跳跃样本缺完成tick导致返回3；v3按实际停止记terminal，编译0警告。v3 run-b/run-c完整CSV与summary各同SHA，中性和持续向右各201自然拆分但非整数0，晚跳在171停止而非已证拆分阴性。正式DAT/源码与Unity均未改。现按Task续订增加第一记录row0选项，仍只改同一诊断脚本，不覆盖旧原件。

2026-10-01 范围续证：v4第一记录X320/300无融合留原件；v5按既有正式正例X304/300，三输入均tick1融合、tick350无完成tick，剩余计时4151，未自然拆分，所有已完成tick小数计数0。v5当前脚本SHA和v3/v5的编译、两组各双跑哈希、首次失败均见[报告](../../../artifacts/diagnostics/NTSD28-336B44-Q07-C054-NATURAL-FRACTIONAL-REACH-001/REPORT.md)。此包只得有限样本阴性，不满足C054正式自然小数入口，状态`RUNTIME_PENDING`；不触发根/Unity重跑、不改生产或DAT。代码改动仅metadata单CPP；回滚保留已有脏工作树并审阅该脚本/原件。
