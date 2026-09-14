# 武器碎片准入与槽位原函数证据

VERIFIED_SOURCE_MODEL_ONLY。独立workspace runner链接未改正式playable闭包，900直接出生端点+900独立完整tick，共1800行，重复stdout逐字节一致。frame错误0、lifecycle失败0、完整driver诊断0。不是正式EXE交互/画面证书，Unity尚未修复/验收。

实测准入：OID0/777/默认-1映射999，子type0..6；0/857/998（后两者未声明）能生成，-1/1000不能；999仅声明后能生成。动作失败仍已消耗坐标/方向RNG；每条默认变体有5个正上界调用，零上界不推进状态。缺目录和无槽位时，两变体仍每次先消费variant RNG，两次生成消耗2次，单变体0次。

完整driver：source20产生slot50/51会参与本tick帧尾；source70/998产生低slot50/51不参与该次已越过扫描。声明999在source20的高slot出生后同tick删除，在source70/998低slot保留。父对象在直接出生端点仍pending存在，完整driver末已删除。未把出生后/已删除后的字段当同一端点。

容量：native原1000槽中以blocker占据到仅0/1空位，Unity容量例外不变。OID151/目标999两阶段正常15内置+2DAT；无槽时builtin一次break且DAT两次continue（noSlot3），两variant消耗2；仅一槽时builtin1、DAT0、noSlot3，单/双variant分别5/7随机调用。缺999时内置一次unresolved加DAT两次unresolved。所有原始失败事件保留在native.jsonl。

调用图与下一生产任务：
- 原battle_world.cpp spawn_at (1228)检查definition与动作；materialize_weapon_piece_fragments (3173) builtin先、DAT后，DAT (3405起) variant→catalog→slot→RNG→generic spawn。SimulationTickDriver28::step (1016/1055) ascending C25后继续高slot。
- Unity BattleNativeWeaponPieceWriter.Materialize/Spawn已同次structural写入，Spawn已检查Native action边界/声明999。BattleLateEntityLifecycleModule.Execute逐当前runtimeSlot查找，具备扫描新高slot基础，仍须动态验证。
- Unity BattleLogicEntityFactory.Create (39) 与 LF2ObjectPointFactory.Create相关入口 (459)仍统一oid<=0拒绝；与原generic OID0准入冲突。BattleLogicReferencePool.Get不另拒绝OID0，会Reset后写ObjectId。
- 修改必须仅对nativeWeaponPieceSpawn专用准入，保留普通OPoint范围；先RED两factory、两profile、边缘与task回收/失败，核对Init/ModuleBind/native birth合法高帧；不得只改两条件便关闭父事务。pool不足没有native对等服务，单列Unity适配后置条件。

本轮仅一个诊断CPP，初版三处misleading-indentation警告已清理并重建，原log保留。最终build-manifest/validation记录哈希与验证。用户Scene/HUDBg x30、生产脚本/资源及schema15/23/26/2/2均保持；完整SelfCheck最新PASS沿用前一fixture证据，本轮没有新Unity测试/Play结论。

后继纠正：global presentation_generation与per-slot allocationEpoch不是同一字段。runner raw epoch改为当前每slot首次分配的1，generation独立保留；1800行除该字段外与旧输出完全相同，最终binary复跑一致。旧native-before-epoch-correction.jsonl留证，新hash见validation；无需重做生成规则结论。
