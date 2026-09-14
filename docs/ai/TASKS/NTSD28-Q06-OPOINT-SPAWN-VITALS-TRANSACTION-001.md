> 当前VERIFIED / OPOINT_VITALS_AND_DISPLAY_BIRTH_ONLY，证据同ID artifacts/REPORT.md；下一ZERO-FRAME-CACHE-CONTRACT，再返回父display补其余出生初始化。

> 当前IN_PROGRESS / TEST_FIRST，准确五脚本Record已建立。

# Q06 OPoint出生资源事务前置

READY_FOR_EXACT_PRECHANGE_RECORD。显示递推13/980、相关181证据闭合、完整SelfCheck/真实两tick Play及关闭已通过；父display出生仍未完成。属于完整DISPLAY-PROGRESSION的出生初值硬前置，也提前满足Q06完整OPoint materializer的一部分；不得等Q07内容部署才修。

当前source battle_world.cpp:7670-7706：point.hp/mp>0优先，否则OID5/52使用10/5，其余500/500；stats.ohp/omp>0再以int64乘后除100（向0），结果作为SpawnRequest.hp/mp。spawn_at:1272-1278写HP/effectiveMax/base、MP、stats.max_mp.value_or(request.mp)及displayHp/displayMax=request.hp。所有type同样，不按weapon_hp冒充current HP。之后weapon_hp和parent/link等副作用继续既有后继，不混入本事务。

当前Unity两个PostInitLiving：BattleLogicEntityFactory、LF2ObjectPointFactory只对OID5/52硬写10/5，忽略hp/mp和百分比；Other.InitializeHealth使用weapon_hp。需复用已解析ObjectPoint.hp/mp及NativeMetadata.Stats，准确追踪两个caller各同步/异步入口的初始化与注册时点；不能在已有Health setter或BindRuntime中随每次HP变化同步display。

先原函数materialize_supported_spawns定向向量/真实当前DAT见证（现display-witness仅spawn_at初值不能替代这层选择规则），再建立准确Task/Change路径，RED→原子出生HP/MP/display与两materializer接线。声明当前MP/legacy MaxPP/MPMax等镜像写入边界，不把cap字段混成current MP。正常bootstrap/Stage.Initialize、snapshot-shell原地恢复、clone复制/复活必须分开审查，不重置被复制/恢复的显示历史。

测试：positive/zero/negative point hp/mp、OID5/52及普通、百分比0/1/50/100/150、截断、stats.max_mp absent/0/positive、两工厂和type0/武器/特效、非默认pool复用、最初可见tick与已扫描/未扫描slot、完整checksum回放、相关display/HP/MP回归与SelfCheck/真实Play有序关闭。保留其他OPoint未实现字段的明确状态，不宣布全materializer完成。

此文件是后继准确Record前的Task，不授权未声明脚本更改。禁止computer-use/非战斗/框架/Scene/资源更改，用户HUDBg x30与例外/13,21,24,2,2/十一阶段保持。完成后回到DISPLAY-PROGRESSION补所有出生初值及联合验收，再继续POST-DISPLAY事务。
