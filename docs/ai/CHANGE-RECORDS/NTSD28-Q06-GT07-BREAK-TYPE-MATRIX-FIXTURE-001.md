<!-- CHANGE-RECORD
id: NTSD28-Q06-GT07-BREAK-TYPE-MATRIX-FIXTURE-001
status: VERIFIED
change-kind: GT07_NATIVE_BREAK_TYPE_MATRIX
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
authority: Formal materialize_weapon_piece_fragments type1/2/4/6 strict negative weapon_hp gate and native pending/code1000.
evidence: Fresh SelfCheck after LC02 correction fails CurrentDatDispatchMatrix old PendingFlushDestroy and all-noncharacter assumption.
-->

# GT07当前DAT类型破碎矩阵

准确单文件CheckGameTickCurrentDatDispatchMatrix的破碎矩阵段：保留全部1..6类型与两CLR外壳，expected gate改正式1/2/4/6，3/5不触发且weaponHP保留-1；native pending/code对应结果。type0排除断言同样用native carrier。持有关系保持到生命周期消费者的断言不变，不顺手改其它GT10/输入/transform夹具。实际失败已存父fragment artifacts，生产原157向量有同样type域。验收完整SelfCheck、结果如实记录，不把source端点当完整tick。回滚仅本段且需批准。

两CLR矩阵与type0排除已改，旧原FAIL保留；待完整SelfCheck。

第二复验仍type1失败，静态定位：types临时catalog只声明790..802，GT07构造的811..816/821..826未声明，其current DAT type实际上退回CLR Other5，旧全非角色断言掩盖了错误。编辑前补充准确范围：同方法进入TemporaryRuntimeObjectConfigs之前填入12个明确OID/type条目，既有其它类型不变。继续保留两CLR及1..6域，不用override getter绕过catalog。原失败留证。

当前VERIFIED / TEST_FIXTURE_ONLY：最后完整SelfCheck实际已越过此fixture，停在后续GT08；不表示整套SelfCheck通过。完整运行原FAIL及五次请求/结果见父WEAPON-PIECE-TRANSACTION artifacts。
