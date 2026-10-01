# Q07/C051 effect23 护甲自然内容入口筛选

状态：VERIFIED_SCOPED_SOURCE_ROOT / UNITY_PENDING；父 C051/Q07 均开放。当前正式源码与根 EXE 已证同初态真实护甲命中，Unity 尚未对照；不改 Unity 生产、DAT、Scene 或非战斗模块。

权威为根正式 `NTSD2.8-Logan.exe` SHA-256 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3`、对应 playable build 闭包的 `GameSession28::step`、`BattleWorld28`/`ArmorResolver28` 及正式 `resources/runtime`。当前正式 `decoded_dat/data/data.txt` 索引 OID78=`c/rai/rai.dat`、OID447=`c/rai/a/rai.dat`、OID97=`c/guy/gei.dat`；OID78/action467 自然 OPoint OID447/action58→54，后者有 kind0/effect23/bdefend80 ITR；OID97 类型1护甲仅列 effect5，理论上不会按效果值直接绕过。**静态条件不是实际护甲生效证据。**

新增且仅新增 `Tools/NTSD28Q07Diagnostics/rai_effect23_armor_reachability_probe.cpp`。沿既有当前源码 GameSession/LFR 诊断构建参数，固定 seed682973786、mode0、双角色OID78/action466→467/X500 与 OID97/action0/候选X550/Z400、不同队伍、HP/MP500，中性输入最多80tick，导出逐tick父/子/目标字段及命中状态、护甲决定和 LFR。增加同位置 OID2 无甲控制以辨别子体与碰撞门；若 X550 阴性，可在明示的第二位置 X700 做一次有界筛选。每例独立只写一次的输出目录，源工具拒绝覆盖。先找当前正式源码阳性，再决定是否送336B44根同LFR及原Unity；阴性不误写成规则不可达。初次v1直接置467未生成子体，原件保留；v2从前帧466进入OPoint帧，不将v1当护甲阴性。

验收：新工具以当前 playable 28 Core/相关 playable 源编译0错；正式运行内容路径正确；独立输出前后 SHA 和结果可追；逐tick明确是否生成OID447、发生 effect23 候选、护甲决定是否 applies、HP/MP/速度如何变化。只有正式完整tick出现真实护甲 `applies` 且根同输入支持时，才进入 Unity first-difference 与必要生产修复。回滚仅新增工具文件，需按文件操作合同另行记录并获授权；原件保留。不得修改 DAT 数据或创建第二 Unity 项目。

实测：v2 编译 exit0/0 warning；OID97/action0/X550 于 tick3 effect23/applied/armor applies 各1、HP500→495，OID2无甲对照同tick HP500→450/action180。源码护甲正例双跑 CSV/LFR SHA一致；当前根 LFR 两案各80tick×11字段=880/880零差。v1直接初态467未spawn、首次根目标朝向不符的原件均保留；更正见[限定报告](../../../artifacts/diagnostics/NTSD28-336B44-Q07-C051-ARMOR-REACH-001/REPORT.md)。effect22正式DAT静态无ITR、Unity同态待，父任务不关闭。
