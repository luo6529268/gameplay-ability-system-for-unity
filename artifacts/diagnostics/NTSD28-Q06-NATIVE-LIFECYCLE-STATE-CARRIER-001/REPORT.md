# 生命周期状态与帧尾部当前验收

FOCUSED_TEST_PASS / SCOPED_PLAY_PASS / SELF_CHECK_PENDING_WEAPON_PIECES。父FRAME-TRANSACTION-INTEGRATION仍IN_PROGRESS。不能宣称完整frame或完整战斗对齐。

## 实际变更

本数据Record准确26脚本：NativeRuntimeStateCode、NativeLifecycleResolutionPending、NativeLifecycleCode独立存储，默认0/false/0、Reset/canonical copy/ECS fingerprint/full checksum/parity/claimed+raw snapshot；联合版本15/23/26/2/2。Raw50已有三项从null改真实字段，47已绑定/3MISSING；工具EntityFieldContract最后补同步，未别名到HitStop或PendingFlushDestroy。旧schema/旧checksum拒绝与既有字段数值断言保留。

父Record实际生产接线：private临时terminal shadow移除，Begin/End不清Runtime真值；core arm pending/code、pending入口不重入。C25资源拒绝pending，definition成功重置pending/code/sound。普通OPoint在pending时跳过；state18读取Native前一/当前frame后提交078；终止移到cleanup之后，encoded写独立state code、清current/collision、保留action_latch及078，不改HitStun；负Link不再阻止已arm的生命周期消费。broken gate限定type1/2/4/6、标code1000后由统一生命周期移除，但两类fragment生产仍缺失。

## 证据与范围

- 初始7/7 RED：三字段缺失和上一版14/22仍接受。初轮35中33PASS/2FAIL为新fixture未推进World却请求snapshot tick1；改为当前tick0，保留raw映射正样例编号1，未修改生产capture门。
- fe9a9025ed794c68a8a409d2f6f5d4c8：41/41 PASS。原2676向量已扩到22字段（含pending/code及lifecycle后survives/current/latch/collision/078/state），仍零差异；9组调用原frame及lifecycle两个端点，不冒称其中未执行的driver particle/fragment。6个实际Late测试各覆盖Legacy/DataOriented两路径，验证857/998存活、999解析、1000删除、1101重置及成本fallback latch1101/原声音顺序/原render phase4保留。
- fd4e0a10b7174cbfad2a443ab8fcac22：386/386 PASS，395.532秒；39请求selector均实际执行，包含snapshot/回放/schema/raw/资源/两profile及上述frame测试。related-386-pass.xml。
- 工具5组88/88；真实capture第一次被unity-binding-status-mismatch拒绝，原因工具EntityFieldContract仍标三项Missing。事前扩展第26路径后修正，最终14 raw+其余74全通过。fresh native/Unity内容及15/23/26/2/2头完全相同，3tick/6实体/300字段出现：47字段相等，3MISSING共18差异；first combat.platformSourceSlot。不是无剩余差异，且sound latch仍不在raw50表。
- 完整SelfCheck本次实际运行FAIL（20:31:39Z）：CheckQueuedObjectPointPassBoundaries:20349仍期待PendingFlushDestroy；其OID100无fragment假设又与当前source内置5片冲突，当前producer确实未实现，不能仅改断言变绿。失败文件保留SelfCheck-initial-fail.result。本次请求写入成功，但请求时间归档命令漏Value且原请求已消费，未保存精确请求UTC；未因归档失败重复发请求。
- 真实旧内容Scene tick5暂停边界：注入并恢复Native state=-99/pending/code1101的完整snapshot，随后实际World.LateEntityUpdateAll执行编码重置为state=-1/current0/0781101/collision0/pendingfalse/code0，原始World checksum恢复，实体4→4。play-lifecycle-pass.json。属于实际C25 late入口和状态恢复，不是输入/物理全tick或碎片表现验收。
- 既有Q05恢复/Renderer保留与有序关闭PASS，World/slot/logic/render borrower全0，两帧Stopped，正常退出Play；结果mtime晚于cleanup请求。最终CS0/Editor idle、Scene dirtyfalse/root14且用户HUDBg x30哈希保持。源码与正式EXE未改，未使用computer-use/未开第二Editor。

## 下一唯一执行

NTSD28-Q06-NATIVE-WEAPON-PIECE-TRANSACTION-001 / READY_SOURCE_WITNESS_AND_EXACT_RECORD。必须同时闭合内置和DAT两阶段生成、真实spawn初始化/RNG/slot/时序及SelfCheck fixture的native合同，再回父Frame事务剩余全面driver/资源回放与场景验收。当前不是用户授权阻塞，不再重复做已通过的carrier单元测试作为替代进展。

当前schema处于frame campaign未完成窗口，不发布最终baseline。剩余3MISSING/其它frame reader/definition与fusion完整准入、display其余出生/post、Q07资源及Q10播放仍由总表继续跟踪。总目标ACTIVE。
