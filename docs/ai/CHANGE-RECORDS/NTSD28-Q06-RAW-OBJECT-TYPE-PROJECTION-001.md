<!-- CHANGE-RECORD
id: NTSD28-Q06-RAW-OBJECT-TYPE-PROJECTION-001
status: VERIFIED
change-kind: RAW_TYPE_PROJECTION_CORRECTION
code-path: Assets/NTSD/Scripts/Simulation/Diagnostics/NTSD28UnityEntityRawCapture.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28UnityEntityRawCaptureEditorTests.cs
authority: Formal BattleWorld28 spawn_at object_type is catalog type, not Unity legacy coarse Runtime.ObjType.
evidence: Weapon piece 157 vectors first comparison: 606 non-character births projected type1 instead of native type0..6.
-->

# raw完整对象类型投影修正

IN_PROGRESS / TEST_FIRST。准确两脚本，仅ProjectEntity identity.objectType改读当前DAT type getter，补七类型回归。Runtime.ObjType作为已有粗分类保持；不改snapshot/checksum实体数据，不改schema15/23/26/2/2与raw50结构。47项绑定中此项先前被过度认证，新证据说明旧纯角色trace不能覆盖非角色类型；旧证据保留但不得扩大。

验收：七类型先RED，修正后本类+武器157两profile+现有raw相关tests；source wrapper formal identity保持。只修诊断投影，没有battle结果副作用/新增服务或关闭职责。回滚限这两处差量且需批准。非战斗和资源不变。

已运行七类型RED，type2..6五失败/type0/1通过，red.xml保留。投影已改读GetCurrentDataObjectTypeForSimulation，旧Runtime.ObjType不变，待联合测试。

当前VERIFIED / DIAGNOSTIC_PROJECTION_ONLY：七类型RED五失败→七类型通过；49联合与两profile157+3原函数出生完整type比较通过。不变更Runtime粗分类、规则或schema，父完整SelfCheck仍有独立GT08旧fixture失败。
