# Q06 display 出生依赖回访

2026-09-21，只读现状审计完成。正式EXE/closure仍为CURRENT-AUTHORITY声明的B1E13A…D2819033 / 07CD47…778F。未修改生产或运行Unity。

## 已关闭的职责不重做

DISPLAY-PROGRESSION的四字段递推及slot可见边界已验证；旧REPORT中OPoint尚未实施的表述为历史。OPOINT-SPAWN-VITALS-TRANSACTION已VERIFIED，两个PostInitLiving均调用BattleSpawnVitalsWriter，已初始化两个HP显示、两个累计显示及四step。BattleNativeWeaponPieceWriter也有独立出生初始化。融合事务、runtime copy/reset及snapshot恢复均有后续独立证据，不能重新当成未实现。

原display/post源码见证的native-build-manifest.json身份与当前正式closure一致，可复用980/2379函数向量；不为无变化源函数重复构建全部矩阵。

## 确认的剩余入口

正式BattleWorld28::spawn_at（battle_world.cpp:1251–1278）创建新EntityState，两个HP显示均=request.hp；battle_world.h:120–127累计显示/四step默认0。

1. AppManager.SetupBattleCharacters:294–301调用ModuleBind、Initialize；LF2Character.Initialize:1107只设置生命/资源，两个显示仍为reset后的0。首次display tick追上不等于出生正确。
2. SimulationStageWaveModule.TrySpawnStageCharacterDirect:748–789最终Initialize(hp,500)，同样缺显示初值。正式game_session.cpp:1703–1720直接将Stage最终hp放入SpawnRequest。
3. TrySpawnStageEntityWithFactory:697–744、ApplyStageSpawnRuntimeContract:675–690、TrySpawnResultsReserveEntry末尾会在工厂OPoint初始化后覆写最终HP，只改真值导致显示保留旧OPointHP。需要在各真实出生事务最终值确定处初始化，不能调用带百分比/lives/weapon副作用的OPoint writer。
4. state9996五克隆：BattleLateEntityLifecycleModule:734–797先走普通OPoint工厂，native分支又把HP/MP改10但不改显示。正式battle_world.cpp:2998–3015采用EngineProfile initialized_object_hp/mp=500，并未在该分支随后改10。还有普通OPoint ohp/omp缩放可能误用于direct clone。这里是完整出生资源语义差异，禁止简单把display改10来掩盖。

## 保持的边界

Snapshot shell在CreateSnapshotShell临时Initialize后，由TryCopyEntityRuntime恢复八个显示字段；不得在最终恢复后再次清零。池Reset只负责清旧状态，不应猜下次HP。融合不重新初始化display。普通Initialize与Stage子包不改AppManager菜单/流程、默认stage.dat、资源文件、Scene或框架。

## 明确顺序

1. 新ORDINARY-STAGE-DISPLAY-BIRTH子包：普通character与Stage最终HP的显示出生初始化；先真实入口RED，代表普通HP137、污染字段/复用、Stage direct与factory最终HP、snapshot新shell恢复非零八字段。
2. 单独STATE9996-DIRECT-SPAWN-RESOURCES源见证与整个出生事务：保留已验数量/扫描时点/RNG；核HP/MP/显示及OPoint stats不应介入的边界。不重复通用OPoint资源算法。
3. 回父NATIVE-DISPLAY-PROGRESSION，确认全部已识别出生入口都有证据后才关闭。
4. 既有NATIVE-POST-DISPLAY-RESOURCE-TRANSACTION随后接入。当前Late模块display后直接computer/frame，没有完整post owner；已有2379向量保留，补previous405非零motion等未覆盖前提。完整mode输入仍Q08，正式内容Q07，默认stage.dat USER_HOLD。

post合同保持原先：保存当前descriptor→frame_0mp仅action写→previous078 state62/63/64/65/66/405、lives4000..4999、group3640..3645→按保存descriptor做full restore→HP/max/MP上限。没有下限clamp，不能挪进HP setter或把display反写真值。

Q06/总目标ACTIVE，Q07未开始。独立effect_fall_review确认上述入口及clone差异；未以静态差异声称完成运行时验收。
