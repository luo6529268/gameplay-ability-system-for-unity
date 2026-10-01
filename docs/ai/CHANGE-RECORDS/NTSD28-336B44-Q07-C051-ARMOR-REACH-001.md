<!-- CHANGE-RECORD
id: NTSD28-336B44-Q07-C051-ARMOR-REACH-001
status: VERIFIED
change-kind: DIAGNOSTIC
code-path: Tools/NTSD28Q07Diagnostics/rai_effect23_armor_reachability_probe.cpp
authority: 336B44 playable GameSession and formal OID78 to OID447 effect23 versus OID97 armor DAT
evidence: docs/ai/TASKS/NTSD28-336B44-Q07-C051-ARMOR-REACH-001.md
-->

# C051 护甲可达性源码诊断

脚本前：Unity 当前共用无甲方向修复已在原Scene左右限定通过；护甲分支没有同条件正式可达证据，不能改 Unity 逻辑。受影响仅新独立 C++ 诊断，写入正式源只读读取及新诊断输出目录；不写 DAT、Unity 生产、Scene/资源。准确代码路径、初态、验收与回滚见 Task。候选可能因碰撞几何、护甲条件或 LFR 载体无法成立；阴性如实留原件，不扩大到合成护甲结论。状态 PLANNED，尚未写代码。

实际新增 `Tools/NTSD28Q07Diagnostics/rai_effect23_armor_reachability_probe.cpp`：固定正式OID78/action467→447/action58→54对OID97或无甲OID2、X550/700、80完整tick；记录子体、effect23命中候选与applied、护甲选择与applies/bypassed、目标HP/MP/护甲HP/速度，并导出LFR。已有输出拒绝覆盖，不修改正式源或DAT。待编译和运行；状态 CODE_WRITTEN。

初次编译 v1 使用正式Core/playable闭包参数，exit0/0 warning/0 error。两例OID97与OID2、X550各80tick均未生成子体：起始action467首tick直接到468，未执行该帧OPoint；这仅说明夹具入口跳过producer，不是护甲阴性。保留v1工具/CSV/LFR原件，同一工具在v2把初始动作前移为466，待重新编译和运行，不覆盖v1。

v2完整源码编译exit0/0 warning/0 error；OID97与无甲OID2正反各80tick，tick2生成447、tick3两者均effect23候选/applied，OID97 armor decision applies、HP500→495；OID2未选护甲、HP500→450/action180。护甲正例双跑CSV/LFR逐字节同SHA。336B44正式根两份LFR v2回放均passed/failureCode0，源/根各880已导出字段零差；首次根v1目标默认朝向不同导致无甲动作差，原件保留。338 staged/formal decoded DAT raw-byte25差均为换行，内容归一后0差。唯一代码文件就是声明的新诊断；Unity/DAT/Scene未改。状态 VERIFIED 只关闭此正式源码与根的可达性诊断，Unity护甲首差、effect22正式可达性和C051/Q07仍待。[报告](../../../artifacts/diagnostics/NTSD28-336B44-Q07-C051-ARMOR-REACH-001/REPORT.md)。
