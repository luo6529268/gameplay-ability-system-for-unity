> 2026-09-21 READ_ONLY_RETURN：父NATIVE-FRAME-TRANSACTION-INTEGRATION已限定VERIFIED，WAIT_PARENT_FRAME_JOIN解除。post/display亦已验；现恢复R06/R12完整materializer审计。见artifacts/diagnostics/NTSD28-Q06-REMAINING-DEPENDENCY-RETURN-20260921/AUDIT.md；先准确source witness Record，再新增脚本，未实施生产。下文WAIT仅历史。

# 普通OPoint剩余消费者回访

WAIT_PARENT_FRAME_JOIN，属于原NATIVE-FRAME-RUNTIME-READER-MIGRATION生成组，不抢当前C25L关闭出口。深度/lives具体首差已由独立Task处理；不能据此标普通OPoint全部对齐。

从当前ObjectSpawnPlanner28::plan_frame及BattleWorld28 materialize_spawn_intents实际链，核对Unity两个late caller、两factory及初始化：opoint.Team覆盖、kind1 Dvz及z_states/z_oid/z_movement消费、double multi-spawn spread、facing模式、Native高动作及声明/缺失999准入、born action/latch/collision镜像、链接kind2/owner/攻击豁免、失败RNG和资源前置。BattleObjectPointValue已保存24字段；数据已保存不等于每项已消费。Z两caller以前同时漏point.Z，已在深度子修复补齐，不重做。

仅列待核验入口，除已实测Z/lives以外不将静态疑点写成已确认bug。复用前OPoint vitals3716证据，重点补未覆盖完整出生字段和调用上下文，准确Record后成组修复。保留未配置World预览fallback、非战斗/Unity-GAS/资源/Scene及所有例外。

新增已确认源资格待消费：battle_world.cpp:7760 effect仅kind1且>0写render_phase；7766 type0或5且parent ordinary_credit_gate_2f4!=2时，无kind限制，reserve/join/join_reserve/join_pic覆盖出生1/0/0/visual。新210 weaponHp见证的type0/5默认reserve0即为lives0，不能将generic出生1/0/0误报为最终所有OPoint的复活字段已对齐。紧接其后stats.defend正且非100写incoming_damage_scale_340也须回访。上述不由weaponHp字段修复冒充完成；用户授权范围内按完整分支与载体消费者另立准确Record接入。
