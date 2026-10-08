# 第42批早期相机GC调用点
结论：SCOPED_ALLOCATION_CALLSITE_DIAGNOSTIC_VERIFIED；父H11仍完整camera 0GC FAIL，H07性能未达。目标active，诊断子批完成不停止六项阶段。

实现只有两个既有Editor诊断源和一fixture/meta；无production、Scene、资源、Settings或Q06方法体修改。复用原完整camera/observer/六子块与关闭owner，新模式默认false，固定8camera边界，只CPU callstacks、无GPU/deep。

测试：red-01旧domain没有导入新fixture，0case，INVALID保留；red-02真实15case缺API RED→green-01实际33/33。camera-01长marker跨URP独立begin/end wrapper破坏Profiler LIFO，Play/cold解析皆失败，拒绝作为干净生产site复现，五原件和SHA保留。修改方向为callback内立即闭合两个point；red-03执行15case六时间API缺失→green-02 job37aac292ce4045588e0245bcb00b7425实际33/33。三个旧hot observer IL string operands仍0；Console compile error CS0。

有效camera-02：原saved Battle自然小roster，非1000AI/性能窗口。URP完整8camera，unity2869—2876、tick10—18；camera ordinal8事件1，其余0；两个recorder前后四校准全PASS，camera/observer invalid0，observer16scope/sixchildren/unattributed0。完整strict检查仍FAIL，不豁免首帧或换验证范围。原关闭11阶段完成，objects/slots/borrowers0，Scene clean/hash unchanged，恢复Menu单Scene/clean/idle/nonPlay。

raw23907522B retained；恢复settings后只在Edit Mode LoadProfile(raw,true)追加，不清history。新raw profiler索引57—65，8个完整point pair按顺序对应8个Unity frame边界；每帧GC样本数与原Scope计数相等，RECOVERED_CAMERA_CALLSITES。小导出15091B，不是39的573MB全样本导出。两个point marker warning/error查询0；before/after Profiler settings完全相等。

实际事件metadata40B、31栈地址，已解析链：BattleFootMarkerAnimation.ResolveSprite → BattleCentralRenderSystem.ResolveRuntimeFootMarkerTexture → BattleRenderPass.Execute → URP camera rendering。stack file行60为方法入口，不应认定为精确分配语句；本次源码重扫ResolveSprite无显式托管new，却按帧读取sprite.texture。未预持有帧纹理导致Unity wrapper首次物化是候选推断，下一窄域测试确认；不直接猜修表达式，不声称35全部两个事件都是它，也不称完整0GC/FPS/Android通过。本批无新FPS收益测量。

证据：camera-02/production-window-01.json、cpu-gc-state.json、cpu-gc-state-recovered.json、camera-gc-callstacks.json、cpu-gc.raw；green-02-point-boundaries.json；camera-01-rejected-attribution.json；Operation after.json。所有原有dirty/8backup/18保护/5拒绝原件SHA/HEAD保持。实现中的仪器问题已换方法验证，次数只触发复盘，不停Goal。

当前20已执行子批（22—37、39—42）＋38仅PLANNED累计21；六项阶段4/6、限定产物5/6、34父项关闭0未改变。已完成 assessment 不重开；EXT1/ATLAS/Mono/Role-aware默认切换门不解冻。下一只针对Foot六帧纹理预获取与强引用生命周期的test-first必要修复，再恢复完整严格窗口验收，不泛化资源或预算架构。

最终静态治理验证：pwsh -NoProfile -File Tools/Validate-ChangeLedger.ps1 -RepositoryRoot I:/GitHub/Unity_GAS/gameplay-ability-system-for-unity 实际exit0/error行0，4284条匹配WARNING的输出行，仍有历史声明路径不在当前diff的警告，未称零警告；git -c core.safecrlf=false diff --check exit0/messages0。实际两个Editor源332新增/1改行及新增fixture/meta由042准确code-path覆盖；其他dirty的40/41/用户项保留原owner。脚本以后没有修改，最终当前Task canonical说明已改为point pair/cold parse，与R1实现一致，不恢复失效的长marker设计。

