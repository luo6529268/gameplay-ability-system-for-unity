# M03 同 publication 命令复用：千人负载检验结果

Task: NTSD-M03-PUBLICATION-WORKLOAD-DIAG-20261010  
Change: NTSD-M03-PUBLICATION-WORKLOAD-DIAG-001  
Status: SCOPED_WORKLOAD_VERIFIED / NO_SAME_PUBLICATION_REUSE_OPPORTUNITY_IN_OBSERVED_WINDOW

## 结论

用户在上次无效采集报告后明确要求“那就继续检验啊”。本次沿用同一任务，使用已修正路径和World/Base相机筛选的观察器完成有效采集。当前千人混战的89个有效正式阶段camera记录中，完成构建89次，全部为publication变化；仅alpha变化0次，同sample重复0次。**本窗口没有可由“同publication未插值命令模板复用”消除的重复构建，不实施该缓存，不运行缓存收益组、不派生产修复。**

这项结论限定当前fixture和有效观察区间；不外推轻负载、高显示帧率或设备，也不代表M03整体无优化空间。原小规模Battle窗口alpha-only约43%为其自身历史证据，不能代替当前千人负载。生产未改，旧UV尺寸候选OFF保持，M03父项及33ms/设备等最终门仍OPEN。

## 实际执行与覆盖

- 复用原Editor、combat1000、Sakura1000、Role、DataOrientedCanonical、seed1314149188，120warm+180sample；不是旧UV候选或生产优化对照。
- 只改变新观察器输出目录为attempt-03。配置与attempt-02除outputPath外相同；实际frame/phase/presentation/detail timing均false，Profiler未开启，无JIT准备或额外warm。
- `focused-result.json`：本次源代码原Editor定向2/2通过，job `fe0ae1fba34f4fb2aa6735af0ac37012`。原计数器20/20证据复用；不宣称本次重跑了20项。
- `observation.json`：COMPLETED、150条World相机记录、单camera、单epoch2、无容量溢出、hooksDetached=true。唯一submission不可用记录发生在tick0；正式阶段全部submission有效。
- runner完整300逻辑tick/120warm/180sample正常结束。camera观察至tick298；tick300执行有序关闭，未形成尾帧camera观察。**正式分类覆盖tick120→298的89个相邻camera增量（178个逻辑tick），不是完整180tick逐camera覆盖。** 89超过事前要求60条；末尾未观察的2tick不计入构建结论。
- 分类值来自现有production完成Build累计计数的相邻差分。每行total=publication+alpha+repeated，epoch不变、frame递增、累计量不回退；failed/rejected增量均0。不是请求计数、末态猜测或计时推算。

## 正式阶段结果

| 项目 | 实测 |
|---|---:|
| 有效camera记录／构建增量 | 89／89 |
| 新publication构建 | 89 |
| 仅alpha改变构建 | 0 |
| 同sample重复构建 | 0 |
| 构建失败／准入前拒绝 | 0／0 |
| 每次source/resolved commands | 平均1992.056，最小1958，最大2037 |
| 每次segment | 1 |
| 活跃AI | 每条1000 |
| 实际TextureArray quad占比 | 100% |
| SourceTexture2D／page／other quad | 均0 |

`classification-analysis.json`和`steady-intervals.json`保存逐相邻差分及校验。binding来自已完成immutable submission内每个segment的实际BindingMode/QuadCount，短暂借用后finally释放；不是由Auto配置推断。每条resolved恰等于各binding quad之和。

累计上传顶点字节增量31,203,568是既有Build诊断的顶点payload计数；不是GC bytes、CPU耗时、GPU总带宽或可消除收益。此次没有测量候选净收益。当前每个被观察的显示帧都需要处理新快照；保存模板不会在这89次中命中，只会增加模板维护工作，具体新增开销未测。

## 已核的策略约束

源代码已复用相同Unity帧／相同publication+alpha。剩余模板策略只能针对同publication但alpha变化的构建，本次计数为0。

若其他负载未来重新评估，仍须处理BuildCommands所读viewport/camera/common binding、未插值基准与display delta、World/publication/generation和资源失效。不能把已经插值的命令直接重复使用；每个新publication的模板保存及后续复制成本必须包含在收益对照中。旧4.156ms是命令物化/插值包围耗时，不能作为本策略可全额消除的成本。

## 保护与验收边界

- `report.json`：StoppedCleanly；容量压力180tick零拒绝/丢弃，关闭activeGO/worldObjects/worldEntities/claimedSlots/objectPoolActive/referencePoolActive均0、异常0，状态恢复。
- 本次tick300完整extended-checksum JSON与attempt-02完全相同；只证明末态，不代替逐tick等价。
- `audit-final.json`：157个具名源码/Scene/Settings保护0漂移、12份恢复前备份有效、HEAD/暂存不变、观察器及两个程序集窗口前后SHA不变。
- `editor-final.json`/`scenes-final.json`：原Editor idle，NTSD_Menu clean、8根；无活动测试或Play。
- 新增Editor观察器实际运行通过，Change可标VERIFIED仅指该诊断。没有完整Driver同步校准0GC证明、生产候选准入、整tick优化或Android/GPU验收。
- attempt01/02诊断缺陷和原数据仍保留，本次有效结果接续此前INCONCLUSIVE；不改写旧失败。

最终实际运行 `Tools/Validate-ChangeLedger.ps1`：PASSED、1411 Records、exit0；历史路径WARNING保留，完整输出为`validator-final.log`。本轮具名路径`git diff --check` exit0、新观察器无尾随空白，见`diff-check-final.txt`和`audit-final.json`。本次request与attempt-02逐字段比较仅outputPath不同。

监测维持暂停，未向执行会话发送新任务。
