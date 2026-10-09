# 第86批直接 Battle 共享中央绑定冷接线
状态：SCOPED_DIRECT_BINDINGS_ACCEPTANCE_PASS / VERIFIED（本Task）；2026-10-09。79/79聚焦、原地rematch最终XML1/1、两完整1800camera均通过；H11选定CPU阶段收口，六项5/6，H07仍FAIL/Goal active。原失败和范围保持，详[报告](../../../artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH86-DIRECT-BATTLE-SHARED-BINDINGS-20261009/REPORT.md)，下方冻结矩阵/事前计数为历史。
## 需求、因果与授权
有效有限Goal合同第0—8节授权H11完整表现路径必要定位/修复及H07已批准范围优化。第80批菜单/压力入口本局完整视觉依赖＋现有预算内共享纹理绑定已存在；本次重扫发现BattleTestBootstrap.Start及TryHandleFirstBattleOnlyResult没有调用该冷准备能力。直接Battle的第43批结果不能代表菜单共享绑定后的路径。本包不是Role默认推广、空间网格、Burst、EXT1、ATLAS或Mono改造。
## 冻结写域
- Assets/NTSD/Scripts/Test/BattleTestBootstrap.cs：CollectDirectBattleVisualRootIds冷列表；PrepareDirectBattleCentralBindings冷helper；Start在压力Suppress返回之后且PrepareBattleRuntimeServices之前调用；直接rematch同一冷位置调用。原null→OID0、empty→无角色、负ID跳过、顺序/重复不变。保留完整逻辑配置/所有后继依赖、预算与Source回退策略。
- Assets/NTSD/Scripts/Animation/Rendering/Editor/BattleCommonAtlasBindingEditorTests.cs：仅新增13 test-first用例，旧断言/夹具保持。
- Assets/NTSD/Scripts/Test/Editor/BattleCentralProductionWindowSceneProbeEditor.cs：仅新增两冷菜单wrapper复用StartCalibratedGc(root,true,true,true)，原物化—上传—录制—提交、observer、校准、首帧及fail门不变。
治理5文件仅追加状态/回链，主进度仍原总表。资源/meta/Scene/Prefab/Settings/Input/Gen/Plugins/Server/Q06方法体不写。
## 不变量与生命周期
无逻辑规则/cadence/RNG/input/pass/排序/segment改变；不改production collector。只在既有空unsealed Preparing World且catalog无renderer lease的冷边界准备，复用原prewarm及retired资源owner和十一阶段shutdown，未新增热路径资源/队列。冷List允许分配，seal后无本包分配或扩容。原共享绑定Steady/Transition和设备预算不变，CPU lease不是GPU完成证据。
## 验证矩阵
1. 新13具名RED：9个root列表（null、empty、负、zero、多ID、重复夹负、int边界、混合）；2源接线顺序guard（首次/重开）；2菜单attribute及完整scope参数guard。反射缺API仍可编译，source guard不冒充运行证据。
2. 新13 GREEN及整个原BattleCommonAtlasBindingEditorTests、BattleObserverLiteralReadinessEditorTests、BattleObserverStageBreakdownEditorTests、BattleCentralRuntimeFootMarkerEditorTests；原Editor实际编译0CS error。
3. 已有NTSD.Test.NTSD28Q08DirectBattleResultHostPlayModeTests.DirectBattleSceneUsesRematchOwnerWithoutMenuScene独立一次，证明直接重开实际owner/World重建不被冷binding拒绝；使用原saved Battle，无Scene保存。
4. 冻结3源后原saved Battle两次1800完整camera，菜单Batch86 Direct Bindings Full GC 1800 Cameras与Batch86 Direct Bindings Reentry Full GC 1800 Cameras，新目录camera-01/02；复用第43批true/true/true flags，无Profiler设置/栈捕获/人工注入，不减scope、不跳首帧。每次camera及observer前/后positive/negative校准有效、allocation events0、两slot/Foot/Health实际覆盖、record/executed一致、growth0/lease归零、shutdown objects/slots/borrowers0、Scene磁盘SHA不变。期间不MCP采样、不改/import源码。
5. SHA/精确before副本/继承保护、diffcheck与Validate-ChangeLedger，结束恢复唯一原Menu clean8 idle。失败保留，不反复相同窗口刷PASS。
## 判断与回滚

运行协议修订（不改C#或原camera scope）：rematch-01最终XML准确1FAIL，失败为MCP NetworkStream disposed的Unhandled log；该次实际during-Play/domain-transition发起过MCP轮询，可能干扰NUnit运行，具体单个连接归因不冒充确认。两独立camera均无期间MCP而完整PASS，提供安全协议参照。按第4节有记录干扰允许仅一次rematch-02重试同一原fixture/断言/场景/冻结源；从提交后至新最终XML产出只做外部文件/PID观察，不MCP轮询、不忽略日志、不修改第三方/EditorSettings。失败仍保留，不能同构无限重试。
本包冷接线成功不自动解决H07<33ms或H11；两完整scope均符合原门才能更新H11阶段，旧43自然12event失败原件不删除/归因。尚未知GPU/Android/多角色证书不追加可选证明。需要恢复时从本Operation准确before副本经新批准操作只逆本包hunk，禁止HEAD/reset替代dirty字节。当前64执行/4of6/34父0/H07H11 OPEN/Goal active；完成脚本才计65，不因计划计执行。
