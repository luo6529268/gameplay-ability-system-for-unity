<!-- CHANGE-RECORD
id: NTSD28-336B44-Q07-C053-NATURAL-DOUBLE-PRODUCER-001
status: VERIFIED
change-kind: CODE
code-path: Tools/NTSD28Q07Diagnostics/c053_natural_double_producer_probe.cpp
authority: selected 336B44 playable live GameSession28 two formal OID65 producers and one OID702 target producer
evidence: docs/ai/TASKS/NTSD28-336B44-Q07-C053-NATURAL-DOUBLE-PRODUCER-001.md
-->

# C053 三名初始角色的双自然 producer 诊断

脚本前登记。现有正式源码/根两人OID65+702自然单次Uj、原Battle Scene同条件184/184已证；两个OID875/action55人工建体的正式源码/原Scene同tick双命中亦有受控证据。尚未证明两个OID875都由正式OPoint自然生成后同tick两次Uj。

本包只新增所列离线C++诊断，输入为当前正式336B44 playable源码和`resources/runtime`，输出为新唯一诊断文件；不改正式源、Unity生产/测试、DAT、图片、Scene、Prefab或非战斗。源码 `GameSession28`支持三combatants，三名初始动作是受控设置；正式根LFR载体slot2初始动作覆盖不足，不能把源码阳性当根同态。仅有限9组12tick源搜索，阳性后同条件复跑原件SHA；阴性只说明此搜索边界。回滚仅审阅新文件及文档，删除另行审计。后续脚本实际路径、编译、结果、失败和未验项须追加。

2026-10-02 实际写入与验证：只新增 metadata 中的C++诊断。初次构建参数准备脚本在创建输出目录后因检查旧exe全路径与新exe不完全相等而断言退出，未写编译产物；不清理该目录，随后直接复用本包新目录并以完整路径下标替换准确源码和产物参数。g++正式闭包编译exit0、stderr空。两次独立9组×12tick运行exit0/stderr空，两个CSV逐SHA相同；9/9组tick7双applied/effect2/Uj156，两个自然875的owner为0/2、目标HP450。[报告](../../../artifacts/diagnostics/NTSD28-336B44-Q07-C053-NATURAL-DOUBLE-PRODUCER-001/REPORT.md)。没有Unity Scene或根三人同初态证书，物理键和命中瞬间锁存未导出；父C053/Q07/总目标开放。未改生产/DAT/Scene，未跑全量Unity测试。回滚仅审阅本包新诊断及证据，删除需另行记录和批准。
