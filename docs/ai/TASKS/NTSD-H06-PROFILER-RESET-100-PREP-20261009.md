> 2026-10-09T20:01:15.881870+08:00 / VERIFIED（仅准备/诊断）；清空与最终100对象两项原Editor测试均1/1PASS，Ledger最终exit0/0ERROR；1192保护0变、原Scene/设置合格，报告与Operation完整。H06准入仍BLOCKED；下方状态为历史快照。

> 2026-10-09 采集完成 / DIAGNOSTIC_PASS；文件审计收尾中。缓存已清空，最终100对象22events/call，FindEffect71/40B，raw成功；本轮非生产优化、不关闭H06准入。 [报告](../../../artifacts/diagnostics/NTSD-H06-PROFILER-RESET-100-PREP-20261009/REPORT.md)。

# H06 Profiler缓存清理与100对象诊断准备

状态：IN_PROGRESS。Change：NTSD-H06-PROFILER-RESET-100-PREP-001。需求来源：当前用户明确授权准备阶段；不是优化生产实施授权。

目标：清空无法导出的原Editor旧Profiler缓存；以100对象保留混合触发条件并做最短分配定位，输出可复核的新JSON/调用栈/必要raw。旧0GC FAIL及H06 BLOCKED保持。
范围：新增Assets/NTSD/Scripts/Test/Editor/H06CollectorPreparationEditorTests.cs（清理与100对象采集两个Explicit入口、必要私有复用函数）、自动meta、此Task/Change/Operation/诊断目录以及五个治理指针。不得修改已有测试/生产、Scene、Prefab、资源、ProjectSettings、InputActions、Server、Gen、Plugins；不读活跃Q06方法体。原EditorPID74724/6402仅idle时操作。

实施与最小验证：
1. 新脚本编译成功及原Editor程序集新鲜；清理测试保存完整设置/各ProfilerArea/旧frame范围后仅ClearAllFrames，检查缓存空且设置相同。
2. 同原失败夹具，100个参与者、76普通/24特殊、50/50阵营，原40列/16X/20Z距离；Configured Role、原碰撞框、4warm、前后1MiB正例/空例、3个collect样本。100未复现则如实结束定位，不宣称修复、不自动扩大规模。
3. 若复现，仅3次详细Role收集（不含构造/预热/校准），不开Deep/GPU；以具名marker导出分配地址/符号/行号。Profiler设置必须事前保存，异常也必须停止自有录制并恢复；新raw不能覆盖旧文件。原0GC/收益/千人验收不由100代替。
4. 复核pure fixture objects/slots0、Profiler设置相同、原Menu Scene/hash不变、生产默认及现有源文件保护、staged/HEAD不变；运行ChangeLedger validator。

owner/容量/关闭：仅Editor测试栈内持有100实体、定长3样本、局部Recorder及最多30个Profiler frame扫描；World采用原fixture注册/解封/注销契约，finally退出objects/slots0。无Runtime新增cache/queue/worker、无十一阶段改动。Profiler只有本测试临时借用，结束恢复准确事前设置并解除log handler，无后台任务。
风险：小规模改变自适应子分支，必须记录实际collector/fallback且结果仅诊断；GC调用栈有额外开销不可作性能评分；旧缓存清空不可恢复未导出细节；不再强求导出旧历史。修复生产分配来源另待用户授权。
完成条件：缓存清理有前后证据、100对象诊断结果真实落盘（复现与否均明确）、本次设置/对象/Scene释放合格、保护审计与Ledger通过。定位失败不冒充优化通过。
回滚：仅任务新增文件/文档增量，按Operation dirty备份处理；不自动删除或覆盖恢复。
