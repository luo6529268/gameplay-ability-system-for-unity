<!-- CHANGE-RECORD
id: NTSD28-336B44-Q07-C051-UNARMORED-DIRECTION-001
status: RUNTIME_PENDING
change-kind: CODE
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2CharacterDatHitResolver.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B5EffectActionOverrideEditorTests.cs
authority: selected 336B44 hit_response unarmored horizontal and C051 paired root Unity first difference
evidence: docs/ai/TASKS/NTSD28-336B44-Q07-C051-UNARMORED-DIRECTION-001.md
-->

# C051 通用无甲 effect22/23 水平方向

脚本前：正式无甲普通命中使用攻击者朝向乘完整dvx，Unity共用 `ResolveStandardDamageKnockbackX` 却为effect22/23使用相对位置；右/左正式根与Unity首差见父诊断。只允许本Record列出的共用resolver与聚焦Editor测试。先RED再改生产，减伤的effect相对位置分支保持原样。预期副作用、验证及回滚见Task；不改DAT、Scene、资源、非战斗框架和既有脏文件。状态 `PLANNED`，尚未改脚本。

实际第一步：仅在声明的 `NTSD28B5EffectActionOverrideEditorTests.cs` 增加 `UnarmoredEffectHorizontalResponse_UsesFacingNotRelativePosition` 六组参数，覆盖effect23/22的左右方向及effect0相邻控制；断言伤害、冲量、倒地/后效果动作。生产尚未修改，待原Editor聚焦RED。

原Editor修前第一次聚焦job `71a2492118f6408db7219525d71ba128`六参数均FAIL；effect22/23左右均呈反号，但effect0对照也因测试实体默认`KnockbackVx=0.1`偏0.1失败。这是夹具初态错误，已仅在该测试将目标待累计冲量显式置零，待第二次RED验证四个effect失败/两个effect0通过；第一次原结果保留，不冒充有效RED。

实际生产修改：仅在 `LF2HitResolveRuntimeData.ResolveStandardDamageKnockbackX` 删除无甲路径的effect22/23相对X例外，使其复用原有的面向符号乘完整dvx默认值。没有触碰 reduced 的 `ApplyAlternateGroundKnockback/ApplyAlternateAirKnockback`，也未改DAT、Scene或资源。第二次修前聚焦job `966731e275b243f9a200a4bbffc755d2` 已启动，但原Editor进程随后退出、Hub同项目新进程启动，旧job结果尚未取到；不得称第二次RED通过。下一待新Editor稳定后实际运行GREEN及原左右raw。

2026-10-01 补验：`dotnet build Assembly-CSharp-Editor.csproj --no-restore -v:q -clp:ErrorsOnly` 成功（0 error，22 warning）；`Tools/Validate-ChangeLedger.ps1` PASS（1098 Records、44 diff代码文件）；本包两脚本 `git diff --check` 通过。生成工程编译不能代替原Editor聚焦GREEN。当前 `LoganRuntime` 仍有2974项工作区删除，恢复命令被自动审批阻断；原Battle Scene左右raw/正式内容运行时验收保持待做。

2026-10-01 后续更正：资源由外部恢复后，原Editor已MCP刷新。方向6/6 GREEN，左右完整Driver各12tick对同初态正式根动作/HP/Vx各99项和OID各33项0差；相邻夹具独立修正后17/17 PASS，生成工程0错/266警告，四保护SHA保持。0用例筛选、历史左根v1误选与MCP客户端关闭错误均留原件，不充当有效PASS。限定Driver门通过，Scene Play/自然键及护甲正式门待，状态RUNTIME_PENDING。[证据](../../../artifacts/diagnostics/NTSD28-336B44-Q07-C051-UNITY-DRIVER-001/ACCEPTANCE-C051-UNARMORED-DIRECTION-001.md)。

交付检查：Change Ledger 1099 records/45 diff代码文件PASS、exit0；相关diff检查通过。上述为已实际运行的检查，不扩大Scene/整组声明。

2026-10-01 原 Battle Scene 追加出口：独立 `NTSD28-336B44-Q07-C051-ORO-SCENE-PLAY-001` 探针在原Editor导入编译后跑右X550/左X350各12生产Driver tick，逐tick选定动作/HP/Vx/子体字段各78项同当前336B44根零差；两次退出Play clean、四保护SHA稳。此证据把本共用修复提升到受控Scene限定通过，但生产Record仍 `RUNTIME_PENDING`，effect22正式根、护甲、自然按键及完整表现未证。[Scene验收](../../../artifacts/diagnostics/NTSD28-336B44-Q07-C051-ORO-SCENE-PLAY-001/ACCEPTANCE.md)。
