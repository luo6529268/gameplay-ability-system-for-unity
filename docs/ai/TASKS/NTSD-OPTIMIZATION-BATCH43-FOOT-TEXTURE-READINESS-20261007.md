# 第43批：Foot动画纹理冷准备与强引用

2026-10-07 当前交付：PARTIAL / RUNTIME_PENDING；8有效RED→原Editor39/39 GREEN。01完整1800仍FAIL12迟发未知；02已声明重进完整1800、四校准PASS、camera/observer0event，容量/CPU lease/关闭三残留0、原Menu clean/idle已恢复。计数与限定结论见43 REPORT文首/terminal-audit-01.json。02PASS不消去01FAIL，不追加同构第三窗口；本Task必要两窗口已执行，H11父门未关闭。下一不是继续本批刷PASS，而是H07现有Brute残余成本有据判断，同时保留H11的01必要归因门；不改变六项完成条件。

状态：PLANNED。六项完成合同第0—8节生效，次数不是停点；H11/H07尚未完成。

依据：42 camera-02 ordinal8 metadata40B真实栈进入ResolveSprite→ResolveRuntimeFootMarkerTexture→RenderPass。方法没有显式managed构造；未持有未来动画帧Texture wrapper是待验证候选，不当作已证明精确分配表达式或35全部事件解释。

准确脚本：BattleCentralRenderSystem.cs只增加Foot纹理引用存储、RefreshRuntimeFootMarkerAuthoringSettings冷准备；原ResolveSprite/ResolveRuntimeFootMarkerTexture、选帧/回退/显示时刻、backend/segment/逻辑不改。BattleCentralRuntimeFootMarkerEditorTests.cs新增纹理持有/冷刷新/释放及采样测试；BattleCentralProductionWindowSceneProbeEditor.cs只新增独立43两次入口，复用35完整1800camera流程，独立fresh camera-01/camera-02，不启Profiler callstack capture。

owner/lifecycle：沿用中央feature/settings资源绑定缓存，持有引用而非创建Texture/Sprite/GPU资源；Refresh发生在feature注册、authoring设置及AfterSceneLoad，不在camera动画选择。存储1个reference纹理＋每帧纹理引用，准备时按配置长度分配；同长度复用，无owner/空配置清引用，替换配置替换旧引用。与既有runtimeFootMarkerSprite/frames相同绑定期，不新增shutdown owner或重排十一阶段；ResetRuntime仍只退役publication/submission，不能借机破坏仍被lease持有的mesh。Scene卸载的feature注销刷新释放此绑定缓存。CPU新数组/强引用纳入steady持有说明，无新GPU像素/复制，不改变ATLAS预算合同。配置变更是现有冷设置接口；禁止把准备方法移入RenderPass或热路径扩容。

test-first：新增8具名断言先用reflection读取尚不存在的private texture storage获得真实RED；然后最小实现，原fixture完整回归与必要容量/lease测试。不得把0case或旧domain结果当通过。之后原saved Battle完整1800camera＋必要重进1800camera；与35同完整scope、双recorder前后校准、observer六子块、首帧、两slot、活动Foot/Health、上传/DrawMesh录制执行/增长/lease、11-stage零残留、双Scene SHA保护不放宽。新代码失效旧0GC证据，故必要重新验收，不是反复测旧方案。

现有33/3ms/max2、checksum/RNG/pass、publication只读、排序/segment/failclosed及GPU完成证明保持。Q06方法体不读，只hash；不改Scene/Prefab/资源/Settings/Input/Gen/Plugins/Server，不启EXT1 M0/instancing，不解冻ATLAS/Mono。不运行1000AI/FPS窗口，本批0GC收益不能称千人性能已达。

备份9文件/21guards/HEAD在Operation before.json，原dirty字节全部保留。只apply_patch；回滚须另授权，以本批当前副本而非HEAD为恢复来源，无删除/移动/Git回退。原Editor19040/6402 Menu idle/非Play，无CLI Pipeline，不装包/第二Editor。先既有MCP刷新/RED/GREEN，必要saved Battle窗口，终态回原Menu，不保存Scene。失败原件fresh保留；到作业截止先查状态，不把截止当Goal停止。

验收：覆盖断言/编译实际通过；完整窗口可靠0event且首帧不豁免，无容量增长/读lease遗留，显示选择不变；必要关闭重进及范围外hash同。若残余GC则如实FAIL，停止采用无效判断而非停止Goal，按新证据继续范围内修复。阶段未达不能complete。

生命周期措辞更正（实施前读取后）：BattleRenderFeature.Dispose调用UnregisterFeature，随后ApplyActiveRegistration刷新并释放/替换本feature的texture引用；不能断言Scene卸载必然dispose共享URP renderer feature。上段“Scene卸载的feature注销”只在该注销确实发生时成立；绑定资源缓存可随现有feature跨World驻留，计入steady，ResetRuntime/第5阶段仍只处理submission，不新增World或GPU所有权，不借CPU清引用证明GPU消费完成。最终以实际正常关闭/重进和owner注销链为证据。

camera-02事前限定：01迟发ordinal937单簇12 event（其他1799frames0）且中途有MCP状态查询；因果未知，保留有效FAIL。原定一次重进改用Play期间纯进程外文件/PID观察，不派MCP/Console/代码，验证观察扰动假设及重进；代码/请求/scope/首帧/校准不变，不启Profiler。第二结果只支持其具体窗口，不用02零事件抹去01未知，不追加同构长窗找PASS。
