# Q07/D-024 WPOINT 持有距离共用比例投影（2026-10-02）

追加的非武器限定证据：原Battle Scene OID8→OID420/type3自然链的21持有tick画面比例误差X0.915229/Z0.232877输出像素，旧字段1320/1320同，退出clean/借用0/六SHA稳；现有336B44根EXE trace与新场景指定LFR源坐标经载体Z偏移归一后246/246同。[独立报告](../NTSD28-336B44-Q07-D024-CANONICAL-HELD-SPATIAL-WITNESS-001/REPORT.md)。此前“canonical未证”文字为当时快照；其它挂点、完整World及可见画面仍待，父包继续`RUNTIME_PENDING`。

后续限定更新：原Battle Scene拾取诊断已通过独立相位配对/源出生见证，物理J→K注入链拾取与18持有tick比例误差X/Z<1输出像素，见[新报告](../NTSD28-336B44-Q07-D024-PICKUP-PHASE-SOURCE-WITNESS-001/REPORT.md)。下文原“不拾取/Scene待验”是当时快照，保留为旧失败事实；canonical非武器、根EXE和其它挂点仍待，本包继续`RUNTIME_PENDING`。

状态：`FOCUSED_TEST_PASS / SELF_CHECK_PASS / ORIGINAL_SCENE_PICKUP_GATE_FAILED / RUNTIME_PENDING`。当前战斗规则权威为根正式 EXE SHA-256 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3` 与其 playable live source；固定完整背景及战斗位移比例是用户批准的 D-024 项目例外。当前对应源码重编的 OID2→OID120 自然拾取24tick CSV与旧版本化源样本逐SHA相同，详[只读/源码审计](../NTSD28-336B44-Q07-D024-WPOINT-SPATIAL-AUDIT-20261002/REPORT.md)。重建探针不是正式根 EXE 同条件证书。

修改三个脚本：`BattleHeldObjectWriter.SyncHeldFrameAndPosition`、`LF2WeaponHeldStateResolver.ApplyHeldWPointSync` 和现有 `NTSD28Q07HeldWeaponDualDomainEditorTests`。两个生产持有写者先保持正式源规则X/Z写入，再统一通过持有者所在 `SimulationWorld.SpatialProjection.SourceDeltaToViewX/Z` 转换双方**源整数相对位移**，以持有者当前物理整数坐标为锚点写被持有物体物理X/Z；未新增比例常量、角色/OID分支，未修改DAT数值、Y、关系、动作、生命周期或Scene。原测试改读当前336B44重核CSV，并把旧“物理间距等于源原始整数偏移”断言改为比例间距，容许整数锚点误差小于1像素。

test-first：生成Editor工程与原Editor均0编译错误；原Editor具名 EditMode `NTSD.Test.Editor.NTSD28Q07HeldWeaponDualDomainEditorTests.NaturalPickupMovingHeldWeapon_KeepsRuleAndScaledViewDomains` 先 RED，job `3c4b587494774c8ba039dd5db90613ac`，tick2期望X间距`19.972993248312079±1`、实际`12.463615903976006`。修复后同名job `d9581b88929b4d66bbb8d92173f0ffdd` **1/1 PASS**。24完整Driver tick中23持有tick，物理相对X/Z对源整数间距×世界比例的最大绝对偏差分别 `0.992927`/`0.936987` 输出像素；例如tick2实际X间距`19.436609`对目标`19.972993`，Z实际/目标同为`1.578082`。红/绿CSV的15个共同非物理字段×24tick为 **360/360相同**，目标测试逐行对新版源码CSV的动作、关系和源规则位置通过。此例直接覆盖LF2WeaponBase持有路径；canonical非武器路径仍缺单独比例断言。最新绿CSV在旧测试归档目录，文件名 `held-weapon-20261002T024124403-3276edc9a1df44a9a266be119734e2c0.csv`；旧CSV原件保留。

完整 `BattleRuntimeSelfCheck.RunAllChecksStatic()` 在原Editor请求式运行返回 `PASS`，旧/新临时结果先后归档于[审计证据目录](../NTSD28-336B44-Q07-D024-WPOINT-SPATIAL-AUDIT-20261002/)，同SHA `2F9ACB02FAA121BB2A3621951F57B4C690655337EDEE2E5AC350BE2B3BE88EA8`。请求式入口仅在归档后清理旧Temp结果并生成新结果；没有删除项目资产或用户文件。保护Battle/Menu Scene、GameConfig、ProjectBattleModeConfig与两份旧源/Unity样本，实测SHA **6/6不变**。

原 Battle Scene使用现有物理J键拾取探针新唯一请求运行50tick，结果 `OBSERVED_DIFFERENCE`：`firstPickupTick=-1`，新场景相对tick1动作60/link0，旧2026-09-26同键样本动作115/link101；差异发生在本包持有写者入口之前，不能归因本包，也不能报武器持有原Scene通过。两次结果对应的Scene SHA不同，且这次未开源规则出生选项、临时武器 `pixelSourceBirthInitialized=false`，所以该探针即便拾取也不足以验本包投影分支。原件保留，详[原场景门槛](../NTSD28-336B44-Q07-D024-WPOINT-SPATIAL-AUDIT-20261002/ORIGINAL-SCENE-GATE.md)。该次Play有序退出、Scene clean，原Editor现 idle/nonPlay/noncompiling。故两个写者的生产代码已编译、LF2WeaponBase分支的当前正式内容同态完整Driver聚焦及完整SelfCheck已证；**canonical非武器比例、真实Scene持有物理键、Legacy可见画面及更多武器/挂点未验**，本包不能标完整 `VERIFIED`，Q07/D-024和总目标开放。

交付检查：`Tools/Validate-ChangeLedger.ps1` 退出0、报告 `PASSED`（1145 Records）；`git diff --check` 退出0且空白错误0。本包未删除项目资产；工作区其它历史删除项没有被恢复或清理。
