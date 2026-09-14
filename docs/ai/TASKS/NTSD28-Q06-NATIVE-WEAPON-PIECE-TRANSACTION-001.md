> 最新：VERIFIED / WEAPON_PIECE_SCOPE_ONLY。NTSD28-Q06-WEAPON-PIECE-SPAWN-ADMISSION-EDGE-001后继全部证据闭合，旧“fragment未写/oid0未修/SelfCheck FAIL”为历史。返回父NATIVE-FRAME-TRANSACTION-INTEGRATION，禁止重做原见证或重开已验碎片。

当前FOCUSED_TEST_PASS / SCOPED_PLAY_PASS / FULL_TRANSACTION_INCOMPLETE；细节见同名artifacts REPORT。完整SelfCheck下一GT08仍FAIL，边缘准入未闭合。

IN_PROGRESS / TEST_FIRST；准确六脚本见同名Change Record，source witness已VERIFIED，以下READY描述为此前检查点。

# 原版武器碎片完整事务

READY_SOURCE_WITNESS_AND_EXACT_RECORD，父FRAME-TRANSACTION-INTEGRATION未关闭；LIFECYCLE-STATE-CARRIER已386/41/真实C25 late/关闭限定通过，但完整SelfCheck FAIL且fragment producer未写。当前版本15/23/26/2/2、raw47/3。先读当前authority与父报告，禁止重回已关闭的数据前置。

权威入口battle_world.cpp:3173 materialize_weapon_piece_fragments（playable闭包），在C25 078提交之后、resolve_pending_lifecycle之前。type1/2/4/6且weapon_hp<0才触发；先weapon_hp=0/arm pending code1000/broken sound，再内置表，再DAT块。healthy next>=999不得产生碎片。pending/负link不自动排除真实break，最终关系安全删除。

内置数量：101/218=7；100/213/217=5；201=3；150=13；151=15；120/124=3；121=4；122/123=9；其它0。缺OID999定义时不执行内置随机；每片先扫描50..capacity首空槽，无槽break；再x/y偏移、vy、vx及按OID/ordinal选action，callsite按source每条保留。source150/151/213高弹跳；122/123后段会再覆盖vy并消费额外随机。request基础HP/MP500，内置owner/group/facing保持spawn默认，出生后写vx/vy/vz0。

DAT阶段：groups声明顺序、amount>0；variant>1先取variant随机（早于目标定义与空槽检查），oid=-1默认999；缺定义或无槽continue（区别内置break）。位置随机Z/Y/X，随后act+framea、vx符号/幅值、vy绝对值幅值及原符号、vz符号/幅值，精确callsite/次数包括上界0行为从NativeRandom实现确认。spawn HP/MP500，owner继承source，block.team非0继承group否则0，出生后继承facing和写motion。读source3492+剩余逻辑，不从本摘要重写算法。

先新增原函数witness（准确Record）覆盖所有内置OID、无内置OID、有/无DAT、variant数、缺定义、槽耗尽、负/零/正weaponHP、健康terminal、负link、随机状态与每片完整出生字段；实际Logan三weapon_piece definitions也核对。最好同时抓立即出生及full driver高低slot当tick参与差异。不能只计数量。

Unity当前LF2Entity.TryRunLatePostOpointCleanupPhase只arm/sound，无fragment。现有NativeMetadata.WeaponPiece已在Q05接入；不要重做parser。优先复用既有logic/presentation factory，但证明基础spawn HP/MP/owner/group与OPoint百分比/继承不同，不能直接套OPoint默认事务导致额外ohp/omp修改。新增入口必须声明准确路径、原子创建/失败/slot/RNG顺序、pending队列、pool reset与十一阶段关闭；不引入跨架构改造。两个生成阶段需一起完成，不能用只实现OID100的便宜版本满足SelfCheck。

SelfCheck失败先核对CheckQueuedObjectPointPassBoundaries（20349）和QueuedBoundarySelfCheckWeapon（35609）：旧PendingFlushDestroy观察应改Native pending/code；其ObjectId固定100，原版有内置5片，不能改成无内置OID或只改assert为零。将完整World fixture显式提供100/999合法定义和必要frame，断言5片/清源slot/有序清理与声顺序，替换旧C#无fragment文本。直接未注册武器的子phase可以只验证arm，但不能推广无fragment结论。脚本修改前独立Record，保留原FAIL。

完成后同seed/source trace、两materializer/两frame路径/两profile、现有及新增fragment tests、完整SelfCheck、真实Play武器破碎/寿命终止区别/restore/关闭全0；再回父事务完整driver验收，仍未关闭其它reader/display/post/Q07/Q10。用户HUDBg x30、非战斗/Unity-GAS/Scene/资源/Server/Gen/Plugins不动，禁止computer-use，无需再次询问继续授权。
