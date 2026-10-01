# Q07/C051 护甲命中后状态离线首差复核

状态：`VERIFIED / SCOPED_POSTSTATE_PASS`。只读比较已保存的当前336B44正式根 trace 与原 Unity 完整 Driver raw；不重跑场景、不改脚本、DAT、资源、Scene 或生产战斗规则。父C051/Q07开放。

输入原件：`artifacts/diagnostics/NTSD28-336B44-Q07-C051-ARMOR-REACH-001/` 的 OID97 护甲及 OID2 无甲正式根 v2 trace 与正式源码诊断；`artifacts/diagnostics/NTSD28-336B44-Q07-C051-ARMOR-UNITY-DRIVER-001/` 中同初态两份原 Editor raw。正式源码匹配入口为 `source/ntsd28_core/src/simulation/armor_resolution.cpp::ArmorResolver28::match_type1`，Unity 对应 `BattleType1ArmorMatchResolver.Resolve` 与 `BattleOrdinaryCharacterDamageRouteResolver.Resolve`；以当前根正式 EXE SHA 336B44 身份裁决，源码仅限其 playable 闭包。

同tick按目标slot1、OID97/2比对动作、帧计数/状态/面向、HP/MP/有效与基础HP上限、复活次数、护甲HP、武器HP、motion hold、render phase、特殊命中锁存、owner与team共16字段，护甲/无甲各12tick。记录原件SHA、逐字段首差和tick3关键状态；不得把后状态一致推断成逐hit决策直接可见。只读静态回访正式匹配分支顺序及当前可达effect23，effect22仅按正式内容与调用链的实际证据标注条件边界。出口为有界后状态结论与下一个有意义的未证点，更新336B44总表及恢复文档。回滚仅撤本包新增分析文档/产物，须遵守文件操作审计；输入原件保持。

实测：两例各12 tick×16目标后状态字段192/192零差；正式源码护甲样本 `armorApplies=1`、无甲样本0，正式根分别有HP伤害5/50的applied hit。Unity raw不导出逐hit决策，故只关闭本后状态审计。具体SHA、tick3与限制见[报告](../../../artifacts/diagnostics/NTSD28-336B44-Q07-C051-ARMOR-POSTSTATE-AUDIT-001/REPORT.md)。
