# 第22批：生产 Foot GameConfig 接线限定验收

结论：SCOPED_RUNTIME_FOOT_CONFIG_PASS。仅本批接线及固定矩阵通过；M03/H11父项仍OPEN/RUNTIME_PENDING，六项阶段执行中，新子批1/8完成。旧20严格FAIL/21诊断及专项门保持。

## 实际改动与证据

无显式authoring时中央RefreshRuntimeFootMarkerAuthoringSettings读取既有GameConfig，借用Sprite/帧数组/duration，不复制或创建生产资源；显式覆盖（含禁用/样式）优先。复用六帧80ms/unscaled表现时间、Self过滤、地面锚点、batch backend。仅三个声明脚本，无Scene/Prefab/资源/设置/Preview/Q06 body变更。新增独立Batch22入口复用原严格门，只读DetectPipeline实际URP，不改管线。

- 原Editor2022.3.62f3全refresh/domain reload后error CS检索0；固定三类实际32/32 Passed，新9+旧23，0fail/skipped，job5f97de1b112442929da6bd49e3637e36。9294只discovered。
- RED原件52bf1cf1e14e4cb99491d1816b35bb22实际9：5预期fallback失败、1新增测试误断言、3pass。禁用仍可保留Sprite，只纠正新断言，不改Preview语义。
- 原saved Battle1800 distinct camera，tick8→1499，两slot；每帧Foot2/Health2，entity2～6、command4～8。CPU DrawMesh录制=执行=11815，badCoverage0/growth0；1800Build=1434publication+366alpha，拒绝/失败0；body vertex1501280bytes，不包含辅助/index/native/GPU流量。
- camera envelope/observer current-thread各0B；纹理选择4096 warm循环0B。报告tick/driverUpdate/latePresentation/playerLoop账面0B，但Editor两硬门false，global gen0/1/2 collection各3，不能称全部PlayerLoop/线程/native/GPU或Android0GC。
- production catalog重放100/500/1000重复body及shadow，EntityCount=0、重复handle、无AI。每档64warm+1800sample；Foot/Health各100/500/1000，局部0B/0growth/0非零allocation sample。SourceTexture2D物理段200/1000/2000；Graphics.Execute不是生产RenderPass。
- 三档mean CPU桥0.8910/4.3988/8.9698ms，max2.0323/17.5691/15.8447ms：是Editor诊断成本，非GPU、生产前后收益、1000AI或120FPS证书。
- 十一阶段关闭objects/slots/borrowers0；Battle clean/SHA253B2EBAD322AA6EC18488DFBBF555BB93AD897940A342E4130E77CE778F9010前后同，原Menu8root/clean恢复，Editoridle/nonPlay。
- 本批30准确保护文件、11 before副本SHA保持；HEAD8107196b1f17ee0f7ce9e7fcbb7ffb9fc260956c未变。不重写第21批733历史保护口径，无删除/移动/弃改/commit/push。

生产DLL1B80F9829E5365C98359D10D9719FF722CAFAE67FB4A5582BE2FA797F93B967A；Editor DLL964C9EE6D7E3F0131421B42026D05C0B1FAB68C0362B9EA6AB831EDCECE5EB66。初read-only execute_code因CodeDOM argv长度失败不算运行证据，改用已有MCP/编译菜单，无安装包/新Editor。现有桥接client退出异常另列，不清Console/日志。

## 留痕与范围

实际原桥验证命令为 refresh_unity(mode=force,scope=all,compile=request)、run_tests(EditMode,固定三类)、get_test_job、execute_menu_item(Batch22)、manage_scene(load/get_active)；完整请求/返回及原数据见 test-requests、compile-preflight、scene-lifecycle、red/green-results 和run-01。治理命令 `Tools/Validate-ChangeLedger.ps1 -RepositoryRoot $PWD` 最终exit0/PASSED（1318Record，3脚本全覆盖），既有4284个无当前diff warning保留不清理；`git diff --check -- <11准确目标>` exit0，仅CRLF提示。最终校验与hash见 final-validation.json / Operation after.json。

[Task](../../../docs/ai/TASKS/NTSD-OPTIMIZATION-BATCH22-RUNTIME-FOOT-CONFIG-20261007.md) / [Change](../../../docs/ai/CHANGE-RECORDS/NTSD-OPT-M03-RUNTIME-FOOT-CONFIG-022.md) / [Operation](../../../docs/ai/FILE-OPERATIONS/NTSD-OPTIMIZATION-BATCH22-RUNTIME-FOOT-CONFIG-20261007/RECORD.md) / [主进度](../../../Assets/NTSD/Docs/battle-optimization-progress-tracker.md)。
本批达标即收口，不再重复Foot诊断。下一只按六项合同完成H11/M03选定链/A-B/关闭重进剩余、H06固定四工作负载准入、M13/M14有界评估、H07Windows报告。最多7剩余新子批，不扩其他28项。Q06排序/first-visible、GPU完成、全局预算/设备不由本批闭合；EXT1仍PROPOSED/MODIFY_REQUIRED，无专项M0，Mono/ATLAS不解冻。

