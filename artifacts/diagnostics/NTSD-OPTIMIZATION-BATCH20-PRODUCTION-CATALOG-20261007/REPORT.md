# 第20批：原生产1800相机帧与catalog重放前置检验

结论：PARTIAL / FOOT_COVERAGE_UNMET。严格production-window结果为FAIL，不能将本批写成完整通过。原场景1800 distinct camera观察已经完成；因所有样本activeFootMarkers=0，活动Foot+Health验收未覆盖，后三档catalog replay没有启动。没有更改判据、打开开关、改Scene或覆盖失败原件。M-03/H-11仍OPEN/RUNTIME_PENDING，34项高12/中14/低8，父项关闭0。

## 新鲜证据与实际命令

原Editor PID19040/6401、Unity2022.3.62f3/URP14/D3D11。refresh_unity(mode force, compile request)完成domain reload；read_console(error CS)=0。原Menu clean8 roots且无Play/tests后，manage_scene load原savedBattle，再execute_menu_item「NTSD/Validation/Optimization/Batch20 Production Catalog 1800 Cameras」。采样期间只读文件watch，不查询Editor；没有新Editor/Profiler/FrameDebugger/GPUcapture/EXT1 M0。
EditMode job5a110f803ee44ac39ac131e50c0d832a：新增7 policy＋相关54=61/61 Passed、0failed/skipped。discovered9276不是执行数。原始请求/结果：[dispatch](test-dispatch.json)、[results](test-results.json)、[compile](compile-state.json)、[Play接线](play-dispatch.json)。

## 自然生产窗口（原件FAIL，已观察事实分项保留）

[生产窗口原件](run-01/production-window-01.json)、[materialization原件](run-01/materialization-window-01.json)。

| 项目 | 实际结果 | 证据边界 |
|---|---|---|
| 主camera distinct Unity frame | 1800/1800 | 实际渲染帧，不是同步循环samples |
| 正常自然logic tick | 8→1563（1555 interval index delta） | 未forced tick、未改pause/broadphase/cadence |
| 捕获实体 | 2～4 | 当前自然低roster，非1000 AI |
| publicationChanged / alphaChanged Build | 1489 / 311，总1800 | 非新优化收益A/B |
| entity vertex上传 | 1800 API，1399552 bytes | 不含Foot/Health/index/metadata/native总流量 |
| entity backend growth/unresolved/Build failed/rejected | 0 | 分项验证，不外推所有资源 |
| actual recorded/executed CPU DrawMesh计数 | 9700，逐camera5/7/9，全部对应 | 不是真实GPU draw/batch/SetPass |
| binding | SourceTexture2D segments4/6/8；atlas/array各0 | 不外推候选预印bank/ASTC布局 |
| actual bound catalog | 1800帧均有 | 非全部角色矩阵 |
| active Health | 每帧2 | 血条活动路径已覆盖 |
| active Foot | 每帧0 | 本批关键缺口，不是Foot活动路径通过 |
| CPU read lease | 每camera-end0，两submission slots | 不证明GPU fence/consumer完成 |
| camera / observer当前线程allocation | 各0B | 仅该 envelope，不是全链0GC |
| tick/driverUpdate/late boundary bytes | 各0B | Editor collectionControl/playerLoopHardGate均false |
| global GC collection增量 | Gen0/1/2各3 | 无法场内归因到战斗；不能宣称全Editor/PlayerLoop或无collection |
| 关闭 | ordered11阶段；objects/slots/borrowers均0 | 失败仍正常关闭，非强制销毁绕门 |
| Scene | savedBattle clean/SHA不变；恢复Menu clean8 roots非Play | 没有save/Scene资源改动 |

配置renderFps120不代表测得120FPS；没有GPU时间、设备、Player/native回放同输入对照或1000 AI吞吐证书。

## 失败原因与只读线索

严格聚合判据由[probe](../../../Assets/NTSD/Scripts/Test/Editor/BattleCentralProductionWindowSceneProbeEditor.cs)的CompleteWindow在1800采满后触发；逐帧CPU draws、catalog、Health、CPU lease正常，唯一未覆盖的是activeFootMarkers。
不会以禁用Foot、虚造ShowSelfFootMarker或加入临时Scene组件来通过原配置验收。新[replay helper](../../../Assets/NTSD/Scripts/Test/Editor/BattleProductionCatalogReplayEditor.cs)已经编译、其计数policy7项通过，但Run/100/500/1000三档重放均NOT_RUN。

只读线索（本次重扫，不等同运行时根因闭合）：BattleCentralRenderSystem.cs:2159–2188通过BattleCentralEditorPreview取得Foot authoring；无候选则禁用，并要求sprite非空。BattleCentralEditorPreview.cs:407–468查找loaded-scene候选，无candidate返回false。其meta GUID8e90577df3435a946ad722ee4a8ef41b在原NTSD_Battle.unity为0匹配；GameConfig.asset:23有FootMarkerSprite引用。当前声明bootstrap路径未见动态AddComponent引用。尚未捕获本窗runtime authored-enabled/Sprite来源及command flag，因而根因仍待确认；不读Q06活跃排序/command方法体，不将静态线索当确定bug或Scene修改授权。

## 实际改动与后续门

仅原Editor probe加一个具名入口、1800 distinct-camera完成判据、逐camera catalog/辅助/实际CPU提交字段；新增一个Editor replay helper和meta，独立backend、生产catalog resolver、容量封口，若前置满足才运行64warm＋每档1800同步CPU样本。重复已取样body/shadow command（重复handle、EntityCount=0）不是World1000实体或AI；完整源commands只读比对。一个16×16非持久RenderTexture与自有Mesh在finally回收，sharedSprite/纹理/材质不销毁。当前未执行helper，不报告其0GC或资源预算结果。
原stress harness会配置新诊断World/LooseQuadtree/Manual等，本批未用其结果冒充原生产配置。

下一门：只读定位原生产Foot authoring/command gate与资源来源；若确需Scene/Prefab或专项资源接线，另列准确Task与用户选择。先解决实际覆盖前置，再决定具名新窗口；不重复已通过的低roster1800来替代高负载。100/500/1000 catalog活动辅助重放、真实1000AI全链/PlayerLoop0GC、异质角色/Q06排序首可见/透明像素、GPU/FPS与Android准入仍开放。A1未来脏区、EXT1 PROPOSED/MODIFY_REQUIRED无专项M0、Mono USER_HOLD、ATLAS bank/budget/format/segment/failclosed均不解冻。

## 安全审计

事前Task/Change/Operation、七治理头先建，8份当前字节备份与SHA核对后才改脚本。708保护文件SHA无差异，production Assembly-CSharp.dll仍A49C69CE527D7E0268661F96F6A5A28B0C79E6204872BDA6EAF1A1930270AE8D；Menu6B5BAD6D…、Battle253B2EBA…保持。没有Gen/Plugins/Server/Settings/InputActions/DAT/图片/Scene/Prefab修改，没有文件删除/移动/破坏性Git/add/commit/push。旧批19所有证据、当前未提交工作保持。
最终Ledger/diff/link/backup/保护结果与本批准确after清单见[审计](final-validation.json)、[Operation](../../../docs/ai/FILE-OPERATIONS/NTSD-OPTIMIZATION-BATCH20-PRODUCTION-CATALOG-20261007/RECORD.md)。历史warning保留，不以清理旧记录伪造零警告。

最终Ledger exit0、1316records/12governed/0error、4260历史warnings；diffcheck exit0（LF→CRLF advisory保留）、两源无trailing/conflict。最后Console error-filter返回2条MCP Client handler exited日志，原件在final-validation.json，不清Console、不称总error0；compile-CS0与61测试Passed分别保留。几次只读定位/glob或apply_patch校验/工具返回JSON解析错误均未破坏工作树；重取明确结果后核对，不以失败的调用包装成证据。
