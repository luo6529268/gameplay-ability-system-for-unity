<!-- CHANGE-RECORD
id: NTSD28-Q06-WEAPON-BREAK-SELF-CHECK-FIXTURE-001
status: VERIFIED
change-kind: NATIVE_WEAPON_BREAK_SELF_CHECK_FIXTURE
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
authority: Formal playable materialize_weapon_piece_fragments OID100 five builtin fragments before native lifecycle removal.
evidence: Original self-check failed old PendingFlushDestroy assertion; new native 157 vectors now all births/RNG agree except independently fixed group inheritance.
-->

# 武器破碎SelfCheck夹具修正

IN_PROGRESS。仅CheckQueuedObjectPointPassBoundaries及其QueuedBoundarySelfCheckWeapon观察器：旧PendingFlushDestroy改Native pending/code1000；World明确catalog100/999与logic materializer，OID100仍是原对象，断言五片及源删除/一次sound/队列清空，最后有序关闭。直接未注册子phase只验证arm与无队列，不再宣称原版无fragment。不改变其余transition旧夹具。

原FAIL保留于LIFECYCLE-STATE-CARRIER artifacts，不通过换OID绕过原生成。验收编译、完整SelfCheck真实运行与失败后继如实记录；新production已有独立六脚本Record，本Record不扩大生产范围。fixture shutdown沿用十一阶段，未创建常驻模块，无schema变化。回滚仅单文件差量且需批准。

上述fixture/观察器已修改：100/999明确定义，五片HP/MP500、源slot清除及plain-free计数，finally有序关闭。原失败结果不变，待新完整SelfCheck。

当前VERIFIED / TEST_FIXTURE_ONLY：最后完整SelfCheck实际已越过此fixture，停在后续GT08；不表示整套SelfCheck通过。完整运行原FAIL及五次请求/结果见父WEAPON-PIECE-TRANSACTION artifacts。
