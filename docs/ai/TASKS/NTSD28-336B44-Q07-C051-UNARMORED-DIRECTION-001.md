# NTSD28-336B44-Q07-C051-UNARMORED-DIRECTION-001

状态：RUNTIME_PENDING（方向聚焦与完整Driver限定通过，Scene/自然键门待）。父目标 `NTSD28-UNITY-BATTLE-REALIGNMENT-001` 的 G1/BATCH-04/Q07/C051。

正式依据：336B44 根 OID20→888/effect23 右X550与左X350各80tick选定字段640/640同源码，Unity完整Driver各12tick首差同在tick9目标动作186/正式180、tick12 Vx反号；[诊断](../../../artifacts/diagnostics/NTSD28-336B44-Q07-C051-UNITY-DRIVER-001/REPORT.md)。当前 playable `hit_response.cpp::accumulate_unarmored_horizontal` 的普通无甲非state2000/type4/6分支按攻击者朝向乘完整dvx；`accumulate_ordinary_horizontal` 的减伤分支才对effect22/23读攻击者/目标相对X。Unity `LF2HitResolveRuntimeData.ResolveStandardDamageKnockbackX` 无甲路径把减伤的相对X逻辑套入effect22/23，直接使本例水平冲量及后继倒地方向反转。

修改精确范围：`Assets/NTSD/Scripts/Animation/LF2Objects/LF2CharacterDatHitResolver.cs` 的 `ResolveStandardDamageKnockbackX`，及 `Assets/NTSD/Scripts/Test/Editor/NTSD28B5EffectActionOverrideEditorTests.cs` 的普通无甲共用writer聚焦正反例。先写同一effect23/dvx-10的双向非致死高fall用例，验证目标在攻击者右侧且双方面右时冲量-10/动作180、目标在左侧且双方面左时冲量+10/动作180，并以effect0/dvx-10为相邻对照。原Editor运行RED后只移除无甲路径effect22/23相对X例外；state2000、稳定化、减伤 `ApplyAlternateGroundKnockback/ApplyAlternateAirKnockback` 不动。若测试夹具暴露其它先决条件，先据正式调用链核对再改。

预期副作用：无甲effect22/23在任意OID上按朝向而非相对X施加水平冲量；由该冲量选择的倒地动作同步改变。不可改DAT数值/图片、Scene/Prefab、比例换算、菜单/结果页或非战斗框架；不增加新runtime owner或关闭阶段。现有未提交工作原样保护。验收：原Editor聚焦RED→GREEN、相邻无甲/减伤聚焦、原Unity同seed左右12tick与当前根选定字段对照，必要时原Battle Scene定向Play；生成工程及Unity编译0错、ChangeLedger validator、diff check、四保护SHA。仅局部机制验收，C051剩余护甲/Scene/物理键和Q07仍开放。回滚仅本包的两处新行，须遵守现有文件保护与删除授权。
