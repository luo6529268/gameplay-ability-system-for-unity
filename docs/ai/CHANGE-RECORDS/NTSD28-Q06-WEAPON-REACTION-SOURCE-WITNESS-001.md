<!-- CHANGE-RECORD
id: NTSD28-Q06-WEAPON-REACTION-SOURCE-WITNESS-001
status: VERIFIED
change-kind: AUTHORITY_SOURCE_DIAGNOSTIC
code-path: Tools/NTSD28AuthorityTrace/weapon_reaction_witness.cpp
authority: Formal battle_world.cpp resolve_confirmed_unarmored_hit/resolve_unarmored_reaction/apply_native_unarmored_attacker_post_hit; hit_response.cpp horizontal/vertical/finalize; same playable closure.
evidence: BDEFEND64 weapon raw failures and prearmor984 unarmored124 cases; current Unity weapon tail rewrites group/frame/rest, clears Fall80 and uses legacy random, absent from native non-type3 continuation.
-->

# 无护甲武器完整反应原见证

IN_PROGRESS / SOURCE_ONLY。准确单CPP，2028向量：基础432、水平累积1296、攻击者post72、资源边界192、双方左向36。类型1/2/4/6；高度/朝向/attacker state0/1002/2000、fall0/40/41、dvx/dvy、当前Vx及预存impulse、attacker type0/3/4及state3000/3007 cover、HP/injury/bdefend/scale/weak。只调用正式ordinary入口并捕获有效itr、完整raw pair、pending impulse/count、rest矩阵、输入统计、spark、audio、完整两类随机及finalize后状态。

使用合成DAT/冻结后诊断初值，不能单独证明正式输入可达。数据契约明确含raw50之外pending与rest，避免只见frame186；同seed双跑/数量断言/源不变量校验和build identity后才源限定VERIFIED。不改正式EXE/权威目录或Unity生产。回滚仅新增文件且遵守用户授权；禁止computer-use、Scene/资源/非战斗/GAS/Server变化。

2028源两遍退出0/相同SHAc5286bf0…b6198。补72个vertical边界（四types、Y -10/0/20、dvy0/8/-8、预存Y -2.5/10），区分原dvy=0分支不执行正Y夹紧，最终2100。保留initial-2028，不以当前样本未触发分支代替验证。

VERIFIED / SOURCE_MODEL_ONLY：最终2100/43202检查，两遍字节一致SHA28cad088…55a；完整范围、命令产物与限制见同ID artifact REPORT.md。
