# NTSD28-336B44-Q07-C053-AUX-TYPE3-DOUBLE-REACH-001

状态：`FOCUSED_TEST_PASS / UNITY_RUNTIME_PENDING`。父目标 `NTSD28-UNITY-BATTLE-REALIGNMENT-001`，G1/BATCH-04/Q07/C053。正式源码91组×12tick中56组同tick双Uj，代表案例两次40tick捕获及336B44根EXE独立回放通过；原Editor编译仍停滞，Unity原Scene待验。详[报告](../../../artifacts/diagnostics/NTSD28-336B44-Q07-C053-AUX-TYPE3-DOUBLE-REACH-001/REPORT.md)。

权威：336B44 根正式 `NTSD2.8-Logan.exe` 及其对应 playable `GameSession28::step`/Core 命中写者，正式 `resources/runtime` DAT。已有OID65/action511→OID875/action50→55与OID702/action553→OID808/action150单Uj源/根/Unity局部通过；受控双OID875源/Unity逐hit局部通过，但正式根初始第三、四槽无法覆盖所需action55。当前正式 `data/data.txt` 将OID251标为type3、`a/fir/firz.dat`；其action0声明effect2的kind0 ITR。静态声明仅是新候选，不是双命中证据。

目标：以两个可由根LFR覆盖动作的受控初始角色OID65/702，保持其自然OPoint产出OID875/808；额外加入slot2/OID251/action0/team1作为**受控辅助攻击体**，只筛有限X/Y站位是否在同一完整GameSession tick对OID808产生两名不同攻击者的两次applied/Uj。它不是三角色物理选招或纯自然整场证书。若正式源码阳性，复跑同seed、保存完整LFR，送原336B44根EXE回放，再按可比字段与逐hit首差决定Unity原Scene下一步；阴性只限定本矩阵，不改生产或DAT。

准确代码路径：仅新增 `Tools/NTSD28Q07Diagnostics/c053_aux_type3_double_probe.cpp`；诊断输出新建于 `artifacts/diagnostics/NTSD28-336B44-Q07-C053-AUX-TYPE3-DOUBLE-REACH-001/`。不修改既有诊断、正式源码/EXE、Unity生产或测试脚本、DAT、角色图片、Scene、相机、背景、模式Asset、GAS或非战斗。输出路径如已存在必须拒绝覆盖。

搜索边界：固定seed682973786、mode0、背景1、安科X610/Y0、JiraX500/Y0、Z400；辅助OID251初始action0、X670～730步长5、Y-90～-30步长10，共91组，每组最多12完整tick。只对确实取得同tick两次applied且目标Uj读取有鉴别力的阳性做40tick LFR和根回放；没有阳性则停止该矩阵，不无限拓宽。

验收与风险：正式31+相关 C++ 闭包编译0错，矩阵记录完整/无静默terminal，阳性需逐hit攻击槽/候选/effect/Uj、目标逐tick动作/HP、两次同seed确定性；根必须自身报告passed/failure0并核对实际支持第三初始槽/action0，不能仅凭源码推断。OID251可能提前碰撞、改变战局或不活到目标tick，均保留真实结果。即使阳性，Unity原Scene和玩家物理键仍待，C053/Q07保持开放。回滚只审本Task新增诊断与文档；删除任何产物须按文件操作审计另获授权，不触碰用户现有工作。

2026-10-04 已新增声明的C++诊断：`make_config`仅装入OID65/702/251三初始对象，`run_case`逐tick筛目标OID808的两名攻击者Uj，主程序固定91组并拒绝已有输出路径。尚未编译或执行；正式DAT/Unity生产/Scene未改。实际编译和矩阵结果随后记入Record与报告。
