# Q07/C051 护甲命中后状态离线对照

当前权威是根正式 EXE SHA-256 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3`、对应 playable live 源码及正式非排除 DAT。本报告只复用已保存的原件，不启动 Unity、重跑场景或修改脚本/资源。

对正式根 v2 trace 与原 Unity 完整 Driver raw 的目标 slot1 做同 tick 配对，OID97 护甲和 OID2 无甲各12 tick×16字段＝**192/192 零差**。[逐字段比较原件](poststate-comparison-v1.json)保留两侧 trace/raw SHA 与空首差表。字段为目标动作、帧计数、state、面向、HP/MP、有效/基础HP上限、复活次数、运行时护甲HP、武器HP、motion hold、render phase、特殊命中锁存、owner、team。

| 案例 | 正式源码内部诊断 | 正式根 tick3 hit 事件 | 正式根/Unity tick3 后状态 |
|---|---|---|---|
| OID97 护甲 | effect23 候选1、applied1、armor `applies` 1 | attacker slot51→target slot1、`applied`、HP伤害5 | action0、HP495、MP500、有效上限499、护甲HP0、motion hold -5，两侧相同 |
| OID2 无甲 | effect23 候选1、applied1、armor `applies` 0 | attacker slot51→target slot1、`applied`、HP伤害50 | action180、HP450、MP500、有效上限484、护甲HP0、motion hold -3，两侧相同 |

正式 `source/ntsd28_core/src/simulation/armor_resolution.cpp::ArmorResolver28::match_type1` 的 type、kind、面向、恢复间隔、bdefend、fall、injury、effect、攻击者 OID、动作/state 门顺序，在 Unity `BattleType1ArmorMatchResolver.Resolve` 有同序对应。Unity 的 `BattleOrdinaryCharacterDamageRouteResolver.Resolve` 在匹配为 `Applies` 且资源可用时选择 `ReducedType1Armor`；正式源码诊断直接给出本护甲样本的 `applies`。这是对实现调用链的静态核对，结合后状态零差提高了该样本的可信度。

**证据边界：** Unity raw 没有导出逐 hit 的 `BattleType1ArmorMatchResult.Decision`、route kind 或逐 hit 事件，正式根 trace 也没有导出目标的 bdefend 累加器。因此不能把后状态相同写成“Unity 内部决策逐字段已直接验证”。本次不为同一初态再跑场景；如后续差异指向该分支，再加准确的内部读口。正式 DAT 静态审计未找到 effect22 ITR，只在六处护甲绕过列表看到它；这不证明所有动态生成路径全局不可达，也不授权删代码。C051 的本护甲后状态限定出口通过，effect22与自然物理键、完整 World/表现及其它方向/条件仍在父 C051/Q07 门下。

下一项按总表转向已有真实首差修复、但只通过完整 Driver 的 C050：在原 Battle Scene 验证 linked rest=-1 跳过普通垂直反应的近距命中及远距对照。该项需要独立 Task/Change 和定向 Play；本包未提前改它。
