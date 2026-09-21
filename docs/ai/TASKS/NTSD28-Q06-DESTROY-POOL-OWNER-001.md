# NTSD28-Q06-DESTROY-POOL-OWNER-001

IN_PROGRESS / RED_FIRST. 来源：平台motion测试19在隔离World销毁后池借用未归零；现有有序关闭合同和World pool ownership，不改变battle rule。
准确范围：
- `Assets/NTSD/Scripts/Test/Editor/NTSD28Q06DestroyPoolOwnerEditorTests.cs`
- `Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs`
- `Assets/NTSD/Scripts/Animation/LF2Objects/LF2LivingObject.cs`
- `Assets/NTSD/Scripts/Animation/LF2Objects/LF2Character.cs`

原状：三个Destroy override在Unregister/renderer归还清空World后调用ResolveLogicReferencePool，可能找到错误singleton池或null。既有Free已先保存owner。
预期：仅在三个方法开始时保存原pool，最后向原pool.Release，保留DestroyEvent/Destroy/Renderer/Unregister顺序。无新manager、关闭阶段或schema。
验证：真实LogicEntityFactory创建type0/1/5代表，另一个World作为隔离控制；先复现原pool.ActiveCount不降，再验证原slot移除、原pool归还、控制World不变及两World有序关闭。重复destroy不能重复借还。编译及相关focused；Renderer路径和稳定包Play另验，不以logic-only证明Renderer。
不变量：不改非战斗、Scene、资源、GAS、Q05关闭顺序、已验Free；无提交/删除。
风险：DestroyEvent可能导致重入或renderer reset，本次capture必须发生在这些调用前。无新服务；使用已有pool幂等Release。
回滚：只在用户授权后撤销本Change精确新增行/fixture，保留平台及其他dirty修改。
退出：相关真实入口/池所属/重复调用通过，必要稳定生命周期回归后再报告VERIFIED。

Renderer acceptance exact paths: existing NTSD28Q06DestroyPoolOwnerEditorTests.cs (parameterized actual factory path) and new Assets/NTSD/Scripts/Test/Editor/NTSD28Q06DestroyPoolOwnerPlayProbe.cs (request-driven paused-scene isolation and Q05 closure handoff). Types0/1/5 represent three Destroy overrides; preserve Scene and user work.

VERIFIED exact Destroy owner scope; real Renderer3 twice with exit/reenter and ordered zero-residual closure. See ACCEPTANCE.md.
