# 第21批：生产Foot authoring前置诊断

结论：SCOPED_FOOT_AUTHORING_DIAGNOSIS_PASS。Change NTSD-OPT-M03-FOOT-AUTHORING-021 / VERIFIED（仅Editor诊断）。运行时Foot没有活动输出的配置读取阻断点已确认；没有修复生产代码，没有完成M-03/H-11父项，也没有解除独立专项门。

## 1. 当前证据与原因

原PID19040、6401 bridge、Unity2022.3.62f3；原saved Battle自然运行64个distinct WorldCamera Unity帧，tick8→63。两次配置快照及全部64帧共同证明：

| 检查 | 实际观察 | 裁定 |
|---|---|---|
| GameConfig | 实际Assets/NTSD/Config/GameConfig/GameConfig.asset，yellow/frame_01.png静态Sprite有效，六帧Sprite/texture全部有效，0.08秒 | 资源没有丢失，GUID迁移后仍能实际加载 |
| loaded authoring | BattleCentralEditorPreview总数/eligible/active均0；TryGetRuntimeFootMarkerAuthoringSettings=false | 生产配置读取链没有authoring来源 |
| central runtime | 已注册材质存在；Foot enabled=false、Sprite=null、animation frames=0 | 不是中央材质缺失；配置链明确将Foot禁用 |
| 已物化Self标志 | 每帧2条Entity命令ShowSelfFootMarker=true | 本窗口Self标志已观察到，不从Q06方法体推演其生成/排序 |
| 实际辅助输出 | Foot0，Health2，每帧一致 | Foot活动覆盖仍未满足，不是优化收益证书 |
| 分类 | NO_LOADED_AUTHORING | 仅诊断分类通过，未把Foot0改成活动验收PASS |

本次重扫代码：BattleCentralEditorPreview.cs407–468在找不到loaded candidate时返回false；该文件1158–1186虽可从GameConfig取Sprite/六帧，但要求先有Preview入口。BattleCentralRenderSystem.cs2157–2188在authoring失败分支设置enabled=false、清空动画，最终runtimeFootMarkersEnabled=false。BattleFootMarkerBatchBackend.cs183–230的disabled/null Sprite gate因此不会形成Foot输出。savedBattle中Preview脚本GUID 8e90577df3435a946ad722ee4a8ef41b匹配0，运行时也确为0；不把静态GUID线索单独作为完整运行时结论。

权威正式EXE SHA336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3保持。只消费已经物化的命令与公开/只读表现字段，不读BattlePresentationShadowBuild活跃方法体，不裁决Q06排序、first-visible、透明重叠或Self生成内部语义。实际pipeline为UniversalRenderPipelineAsset，未修改相机、纹理设置或安装包。

## 2. 实际执行与分级

- 新9个分类测试test-first：首scripts-only刷新未导入新文件，job66bae213e068471caf49bc3a85e0b6a1实际选中0，不算PASS，原件保留。完整all刷新后jobed9cf4746612469d87266bba86c6c461实际completed9/九个stub预期失败，red-results.json保留。
- 实现后原Editor具名EditMode：新9＋既有61=70/70 Passed、0failed/skipped，job7df889c313e14f18a2d0b5ced493296e。9285是discovered，不是执行数。compile-state/测试后及Play退出CS错误筛选均0。
- 原savedBattle64实际相机帧，不注入输入、不强制tick、不创建诊断World、不执行catalog replay。窗口主体2实体、4个中央物理segment、每frame5个实际CPU DrawMesh，累计320；不是GPU batch/SetPass。
- entityBuild64=publication53＋alpha11，实体vertex上传45056 bytes；不是Foot/Health/index/submesh/GPU流量。零entity增长/资源失败；两slot，camera-end CPU read lease0不等于GPU fence证明。
- camera/observer当前线程各0B，记录的tick/driverUpdate/late/playerLoop及Gen0/1/2增量均0。启动与结束反射/Resources/路径序列化有分配且在采样外；Editor collectionControl/playerLoopHardGate均false。该2实体短窗口仍不能证明完整0GC或1000AI/Android性能。
- 既有11阶段关闭正常：objects/slots/pool borrowers均0；Battle clean且SHA253B2EBAD322AA6EC18488DFBBF555BB93AD897940A342E4130E77CE778F9010保持，Menu clean8roots、原Editor idle/nonPlay已恢复。没有Scene保存、组件新增或资源修改。

实际调用：原6401 refresh_unity(all, compile=request)；run_tests(EditMode, test_names见test-dispatch.json)；manage_scene(load savedBattle)、execute_menu_item(Batch21 Foot Authoring Diagnosis 64 Cameras)、原入口正常关闭后manage_scene(load originalMenu)。无第二Editor、Profiler、FrameDebugger、GPUcapture、专项M0或instancing。

## 3. 准确改动、留痕与并发Git状态

仅扩展BattleCentralProductionWindowSceneProbeEditor.cs的Batch21入口、只读配置快照/热路径已物化Self计数/分类；旧入口和第20批严格验收条件不变。新增BattleFootCoverageDiagnosticEditorTests.cs/.meta。未改生产算法/runtime DLL；Assembly-CSharp.dll SHA仍A49C69CE527D7E0268661F96F6A5A28B0C79E6204872BDA6EAF1A1930270AE8D，当前Editor DLL为7BB73B4E1754C8A75F17B832815AE87054DCCCBEB7D1341BD00530A65064D051。

准确8个既存文件（七治理docs＋probe）修改前已Copy-Item -LiteralPath备份；733既有保护路径保持，前批741冻结路径事前全部核对，旧20失败/after/Record不写。新Task/Change/Operation与本ID产物独立，CreateNew拒绝覆盖旧输出。

执行期间观察到HEAD2cccd597dbf6cb32826eb0b6e98d7462c1eac48d→8107196b1f17ee0f7ce9e7fcbb7ffb9fc260956c的外部提交，472路径含本批中间源/doc/备份；本任务没有执行git add/commit/push，不归因特定执行者，不回退。733保护SHA均相同，staged空。不能声称HEAD未变；源文件已被外部提交纳入，最终Ledger除普通检查外用SimulateChangedPath分别覆盖本批两个脚本，不以当前diff0冒充无代码改动。新外部未跟踪native产物同样保留且不读取其规则内容。

最终validator、备份/保护/链接/Scene/DLL/source指纹和差异检查详见final-validation.json与Operation after.json（self-excluded）。Record状态仅对本诊断closed，父项仍OPEN/RUNTIME_PENDING。

最终实际静态审计：Ledger普通＋两source模拟路径检查均exit0/0error，1317Records，governed分别0/1/1；历史warning分别4309/4303/4308，未清理。733保护/8备份SHA匹配，38本批links全部有效，source whitespace/conflict检查和git diff --check通过（LF/CRLF advisory保留）。最终staged空，HEAD外部变化已按事实单独记录。

## 4. 下一步与保持的门

下一步不再重复1800低roster窗口来替代未覆盖的活动Foot或高负载验证。须独立明确生产Foot配置接入口、开关与样式归属，然后建立准确生产修复Task/Change和test-first证据。候选为从既有已绑定GameConfig直接提供runtime authoring，Preview只保留Editor或显式authoring覆盖；本轮未批准、未实施。若恢复Scene组件，则需要准确Scene授权，不能为了测试变绿自动添加或改开关。

第20批严格window FAIL、FOOT_COVERAGE_UNMET和三档replay NOT_RUN保持，不重写历史；生产修复/活动辅助压力/1000真实AI/fullchain0GC/GPU排序first-visible/120FPS/Android认证均未完成。EXT1 PROPOSED/MODIFY_REQUIRED且无专项M0；MONO USER_HOLD；PERF/ATLAS正文、bank/预算/格式/segment/fail-closed及33ms/3ms/max2、11阶段关闭不变。34项高12/中14/低8，父项关闭0。

## 5. 索引

[任务](../../../docs/ai/TASKS/NTSD-OPTIMIZATION-BATCH21-FOOT-AUTHORING-20261007.md)、[Change](../../../docs/ai/CHANGE-RECORDS/NTSD-OPT-M03-FOOT-AUTHORING-021.md)、[Operation](../../../docs/ai/FILE-OPERATIONS/NTSD-OPTIMIZATION-BATCH21-FOOT-AUTHORING-20261007/RECORD.md)、[进度总表](../../../Assets/NTSD/Docs/battle-optimization-progress-tracker.md)。

[本次静态重扫](static-bindings.json)、[70项测试](test-results.json)、[实际窗口](run-01/production-window-01.json)、[计数窗口](run-01/materialization-window-01.json)、[诊断汇总](observation-summary.json)、[Scene恢复](scene-restoration.json)、[并发Git变化](concurrent-git-state.json)。
