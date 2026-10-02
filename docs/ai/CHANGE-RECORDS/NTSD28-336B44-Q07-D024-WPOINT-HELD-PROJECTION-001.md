<!-- CHANGE-RECORD
id: NTSD28-336B44-Q07-D024-WPOINT-HELD-PROJECTION-001
status: RUNTIME_PENDING
change-kind: CODE
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleHeldObjectWriter.cs
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2WeaponHeldStateResolver.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q07HeldWeaponDualDomainEditorTests.cs
authority: 336B44 formal held-object WPOINT source and user D-024 full-view ratio requirement
evidence: docs/ai/TASKS/NTSD28-336B44-Q07-D024-WPOINT-HELD-PROJECTION-001.md
-->

# Q07/D-024 武器及非武器持有挂点物理比例

2026-10-02 canonical补证：正式OID8→OID420/type3自然持有链在原Battle Scene再跑60tick，21个真正持有tick比例最大误差X0.915229/Z0.232877像素，旧22字段×60tick 1320/1320同、退出clean/借用0/六SHA稳；本包生产`BattleHeldObjectWriter`已进入此非武器链。先前武器OID120 18持有tick也已原Scene限定通过。后续只读根EXE trace还给出指定LFR源坐标246/246同，更多挂点、完整World和画面未验，本父Record仍`RUNTIME_PENDING`。[canonical报告](../../../artifacts/diagnostics/NTSD28-336B44-Q07-D024-CANONICAL-HELD-SPATIAL-WITNESS-001/REPORT.md)。

2026-10-02 补充：独立诊断包 `NTSD28-336B44-Q07-D024-PICKUP-PHASE-SOURCE-WITNESS-001` 在原Battle Scene将受控出生前输入相位1→0配对、临时OID120源位置初始化后，物理J→K注入链相对tick2拾取，18持有tick比例最大误差X0.536384/Z约0像素；前7tick对当前正式源码原值75/77，2格空关系哨兵不同。先前不拾取由诊断初态错拍解释；旧失败原件保留。只增加本武器Scene限定证据，canonical非武器、根EXE同态和其它挂点仍待，父包维持`RUNTIME_PENDING`。[报告](../../../artifacts/diagnostics/NTSD28-336B44-Q07-D024-PICKUP-PHASE-SOURCE-WITNESS-001/REPORT.md)。

交付检查：`Tools/Validate-ChangeLedger.ps1` 退出0，`Change ledger validation PASSED`（1145 Records）；`git diff --check` 退出0、空白错误0。原Editor同PID105896回到 idle/nonPlay/noncompiling 的 Battle Scene。Git工作区仍有其它历史删除/修改项，本包没有删除项目资产；仅请求式SelfCheck清理已先归档的Temp旧结果，原因与副本在报告中登记。

2026-10-02 当前验收：生成Editor工程0错、原Editor导入0错；目标EditMode从RED tick2 X12.463616 vs比例目标19.972993变为GREEN 1/1，当前336B44源码重核CSV的24完整tick源字段及红绿共同非物理字段360/360不变；23持有tick最大物理比例偏差X0.992927/Z0.936987像素。完整SelfCheck PASS，六保护SHA稳。原Battle Scene物理J键探针50tick未拾取，首tick新动作60/link0相对旧版动作115/link101，入口发生在本包持有写者前；失败原件保留、场景有序退出clean。canonical非武器比例及真实Scene/Legacy持有画面未验，因此状态 `RUNTIME_PENDING`，不得写完整对齐。临时SelfCheck旧结果先复制，现有请求入口清理Temp旧结果后写新PASS，两个副本同SHA；未删项目资产。[完整报告](../../../artifacts/diagnostics/NTSD28-336B44-Q07-D024-WPOINT-HELD-PROJECTION-001/REPORT.md)。

生产代码已写：`BattleHeldObjectWriter.SyncHeldFrameAndPosition` 和 `LF2WeaponHeldStateResolver.ApplyHeldWPointSync` 都只在双方源规则位置初始化的既有分支中，于写完源X/Z后通过持有者当前World的 `BattleSpatialProjection.SourceDeltaToViewX/Z` 转换源整数相对位移，再以持有者物理整数位置为锚点更新持有物理X/Z。原Y/cover/动作/关系/速度/缺源分支不动，无OID特判或新比例常量。生成编译、原Editor目标GREEN、SelfCheck及场景待验。

test-first RED：生成Editor工程 `dotnet build Assembly-CSharp-Editor.csproj --no-restore -nologo -v:q` 退出0、0 error；原Editor刷新后EditMode job `3c4b587494774c8ba039dd5db90613ac` 的具名测试按期失败，首差tick2比例X期望19.972993248312079±1、实际12.463615903976006。测试读取当前336B44源码重核CSV；这是旧物理偏移首差，不是编译或源规则字段差。[RED原件](../../../artifacts/diagnostics/NTSD28-336B44-Q07-D024-WPOINT-SPATIAL-AUDIT-20261002/TEST-FIRST-RED.md)。

2026-10-02 test-first写入：仅更新 `NTSD28Q07HeldWeaponDualDomainEditorTests` 使用当前336B44源码重核CSV，并将持有期间物理间距断言从旧raw整数差改为源整数差乘共用视口比例，容许1个输出像素整数锚点误差。现待原Editor同名单例RED；生产两个写者尚未改。

本Record在任何脚本修改前建立。原Unity canonical `BattleHeldObjectWriter.SyncHeldFrameAndPosition` 与LF2WeaponBase运行时 `LF2WeaponHeldStateResolver.ApplyHeldWPointSync` 均用原始DAT中心/WPOINT/cover偏移直接写物理X/Z，另行写同正式版的源规则位置；当前世界已配置水平2048/1333、纵深1152/730投影。原Editor现有OID120完整Driver测试使用当前正式内容，但断言旧raw物理整数间距；tick2源X间距13、物理12.463616、项目比例目标19.972993，Z源1/物理1/目标1.578082。当前336B44对应源码重编探针24tick源CSV与旧版源CSV逐SHA同，只为有界源规则证据，尚非根正式EXE本体证书。[审计](../../../artifacts/diagnostics/NTSD28-336B44-Q07-D024-WPOINT-SPATIAL-AUDIT-20261002/REPORT.md)。

计划先改具名聚焦测试读取新版CSV并断言比例，取得旧代码RED；随后让两个共有WPOINT持有写者在源规则位置提交后调用已有世界 `SpatialProjection.SourceDeltaToViewX/Z`，不新增比例配置、不改DAT数值，保留身份/缺源/Y/cover关系。真实物理挂点、武器攻击空间可能改变，需完整tick源字段、原Editor SelfCheck及具名场景证据核验。验收、保护、风险和回滚详同ID Task。当前 `PLANNED`；脚本尚未改。
