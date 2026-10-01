<!-- CHANGE-RECORD
id: NTSD28-336B44-Q07-C052-POSITIVE-REST-REACH-001
status: VERIFIED
change-kind: SOURCE_DIAGNOSTIC
code-path: Tools/NTSD28Q07Diagnostics/effect21_rest_reachability_probe.cpp
authority: current 336B44 playable battle_world.cpp effect21 victim_rest gate and formal OID211 DAT
evidence: docs/ai/TASKS/NTSD28-336B44-Q07-C052-POSITIVE-REST-REACH-001.md
-->

# C052 正间隔后继候选 source 诊断

脚本前状态：正式 `battle_world.cpp` effect21 state18/19 + rest0 终止门、`a/fir/fir.dat` 双 effect21 ITR、`c/nar/nar.dat` action203 state18 已只读确认；原 Unity 提前终止没有 rest 读取。几何是否形成同 attacker/target 双候选、第一击是否将目标改为203并写正 rest、后继第二目标是否还会消费，均未运行证实。仅新增上述 C++ 诊断脚本；输出是新目录，正式源码、DAT、Unity生产/Scene/非战斗保持不动。预期副作用仅编译一个候选 EXE 与生成 CSV/摘要；不能自动晋升为正式根行为。验收为完整 `GameSession28::step` 双跑同输入和候选/rest 顺序原件；阴性也必须保留边界。回滚若需删除新脚本或输出，先按文件操作合同审计，不能触及用户既有脏文件。

2026-10-01 实际脚本：新增 `Tools/NTSD28Q07Diagnostics/effect21_rest_reachability_probe.cpp`，只读加载正式 runtime、以 OID211/action160或161 和两名 OID2 配置完整三 slot 会话，逐 tick 导出动作、HP、victim rest、候选数量与逐候选终止/状态，再封存 LFR。使用新输出目录，拒绝已有目录；未改原正式源码、Unity或 DAT。状态 `CODE_WRITTEN`，尚未编译运行；不能推断阳性或首差。

2026-10-01 最终限定验证：`g++` 使用当前正式 28 Core + playable 编译参数及本诊断脚本 exit0，`compile-output-v1.txt` 为空、候选 EXE 新路径落盘。正式 OID211/action161 与 action160→161、目标 X500/X530 均在完整 `GameSession28::step` 得到三个有序候选：slot1 applied、slot1 relation-rest rejected/terminates0、slot2 applied；两目标HP各420，受击动作203，目标1尾 rest44。两正例各独立双跑，CSV/hits/LFR 三文件的 v1/v2 SHA 全同；X650 第二目标阴性 HP500。实际验证和未验项详[报告](../../../artifacts/diagnostics/NTSD28-336B44-Q07-C052-POSITIVE-REST-REACH-001/REPORT.md)。仅本受控 source 子包 `VERIFIED / SCOPED_SOURCE_POSITIVE`；根正式EXE三实体/自然OPoint、原Unity生产首差、物理键/Scene未验，C052/Q07/总目标开放。没有改生产或 DAT，未运行 Unity 测试。

2026-10-01 当前根正式 EXE SHA336B44已复核。将本包生成的两个三实体LFR直接送入根 EXE，近/远均退出0且headless报告passed=true/failureCode=0，独立逐tick比较根trace与源码CSV的攻击者动作、两目标动作/HP合计30/30一致。根报告nativeParityClaim=false，30/30是手动字段比较，不宣称逐hit完整同态。自然OPoint仍待；本Record的源码诊断VERIFIED不自动关闭父C052。根argv、report、trace原件在本包artifacts目录。
