# H06 Profiler清理与100对象分配定位报告

日期：2026-10-09。Task/Operation：NTSD-H06-PROFILER-RESET-100-PREP-20261009。Change：NTSD-H06-PROFILER-RESET-100-PREP-001。

## 结论与证据边界

原Editor旧Profiler内存历史已经按用户授权清空；100对象短诊断成功复现分配，JSON、解析调用栈和新raw均已导出。最终三次collect各22个GC.Alloc事件，每个40B，单次880B；共66事件/2640B，全部首个可解析managed frame为LoganKindCatalog.FindEffect()第71行。本次只完成准备和诊断，不是0GC通过、优化修复、千人资格或性能/Android认证。

没有修改生产优化逻辑、已有测试或Suite，没有切换普通默认BruteForce；H06限定准入仍BLOCKED，95默认OFF，旧Goal与另一个会话均未恢复。没有读取活跃Q06方法体。调用栈中的Q06文件/符号/行号仅来自Profiler解析元数据。

## 实际执行及结果

原有Editor PID74724，Unity2022.3.62f3，项目原MCP桥127.0.0.1:6402。未启动第二Editor，未进入Play，未切换/保存Scene。由于Unity CLI无Pipeline而既有MCP可用，使用既有MCP的manage_asset精准import新脚本、run_tests EditMode具名入口及get_test_job；没有安装/升级工具。

| 步骤 | Job ID | 结果 | 证据 |
|---|---|---|---|
| 首次清空旧缓存 | a75502e05f5348de91824b657cf82967 | 1/1PASS；frame范围0/7→-1/-1；设置完全相同 | reset-tests.xml；reset-3dcd7b51179349d68ac34c10b5f663aa/history-reset.json |
| 首轮100诊断 | ba2d06104500485cafb5486914b1dfea | 1/1PASS，但preflight29受测试断言污染；仅详细marker22/call有效 | capture100-result-01.json；revision-01-assertion-contamination/；capture-100-96a75c3ff7ef4304a6be807665be2104/ |
| 保存首轮raw后清空本轮缓存 | e31b17726bf142d4b747b7381b2eabe8 | 1/1PASS，5.7383224s；设置相同 | reset2-tests.xml；reset-06339ee6fe764cb0b581e2ea0b3427de/history-reset.json |
| 修正探针后100诊断 | 78eebe0088994b8ead7b77c04fd6eaac | 1/1PASS，2.0642197s；preflight22/22/22与详细栈22/22/22一致 | capture100-revision02-tests.xml；下述最终目录 |

最终目录：[capture-100-77aa1947b69544459e9c0a347e33577d](capture-100-77aa1947b69544459e9c0a347e33577d/)。

- preflight.json：100对象，76普通角色/24特殊攻击，双方各50；沿用原混合夹具40列、16X/20Z间距、原ITR/BDY。角色/阵营阈值按100调整，避免全部变成同种对象。
- Configured Role实际为ForceRoleAware（mode3），fallback0、aborted=false，pair97；容量拒绝增长0→0。
- 4warm、前后1MiB分配正例/空例校准均通过；3次collect范围不含构造、预热、校准、快照准备及断言。Recorder Unit=TimeNanoseconds，其rawValueTotal不是字节量，不能用它计算分配大小；40B取自详细GC.Alloc metadata。
- 仅3个H06.Preparation100.Collect marker，CPU/Editor分配栈录制；Deep/GPU关闭。不是完整Driver或实景tick计时，测试总耗时不作优化收益。
- callsites.json：66事件，每条均解析到managed frame；frame0/1/2各22。汇总见[allocation-summary-final.json](allocation-summary-final.json)。
- capture.raw：9,703,396B（约9.70MB/9.25MiB），SaveProfile成功、errors=[]；首轮9,912,324B及所有旧失败产物均保留。
- cleanup.json：完整本轮设置及所有ProfilerArea前后相同，objects=0、slots=0、errors=[]。

## 分配位置及推断

实际首个managed frame：`Assets/NTSD/Scripts/DatParser/Runtime/Models/LoganKindCatalog.cs:71`，`LoganKindCatalog.FindEffect(int effect)`。

调用者：`BattleKindTableRules.RejectCandidate()`，再由角色候选收集链消费。这里仅依据已保存调用栈定位，不打开Q06方法体。

已只读核对FindEffect：`Records`为`ReadOnlyCollection<LoganKindRecord>`，第71行为`foreach (LoganKindRecord record in Records)`。由“该行＋40B分配”的证据，集合枚举器的分配/装箱是具体优先核验方向；本轮未捕获分配对象类型，因此不把装箱机制写成已证明事实。后续最小改法候选是保持现有顺序/首个匹配/重复effect/null语义的索引遍历，须另获生产修改授权并建立独立Record后执行。

不能据此断言旧千人夹具的234事件全部同源，也不能解释95完整Driver的GC；它们需要各自范围的证据。此次成功说明清空历史并缩短采集后可以定位本例，不证明旧>2GB导出失败单纯由“角色太多”导致。

## 纠错保留与不重复验证

首轮preflight在recorder.Begin后执行Assert，导致29计数含测试自身分配。已在同一准备脚本中把断言移至End之后，补齐每条栈须有解析managed frame的检查；修订前源码/XML和raw完整保留，没有删除失败或重新解释29为纯collect。最终22是修订后独立复核值。

没有重跑此前四场景32tick一致性、91/93实景、95资格、H07长窗或Android，生产字节未改所以不机械重跑。后续是否修FindEffect由用户另行决定；当前已有足够调用点证据，不再继续堆同类短采集。

## 修改范围、释放与文件审计

唯一新增源码：`Assets/NTSD/Scripts/Test/Editor/H06CollectorPreparationEditorTests.cs`（两个Explicit入口ClearAuthorizedProfilerHistory/Capture100AllocationCallsites及私有助手），同名meta由Unity生成。最终源SHA-256：ED0F5AE09BD207762A32B15A6168751AF5A5406D247806D865B204A1CE959798。原Editor新程序集重载后执行最终PASS，没有用静态编译代替实际测试。

本轮准确恢复开始时已知设置；不声称恢复另一个会话更早未完整保存的设置。最终Editor快照：[editor-final-state.json](editor-final-state.json)：idle、非Play、无编译/重载/测试，原Menu clean、8roots，Profiler/GC栈录制停止，error CS过滤结果0。

[protection-check-final.json](protection-check-final.json)：1192个既有保护文件0变化（包括旧H06诊断产物），6份事前dirty备份SHA相同，HEAD与staged不变。五个已有治理文件只新增本任务指针；未删除任何磁盘文件。公共Temp测试XML的自然替换均先备份，修改历史与恢复来源见[Operation](../../../docs/ai/FILE-OPERATIONS/NTSD-H06-PROFILER-RESET-100-PREP-20261009/RECORD.md)。旧未导出内存细节无法恢复，此限制已由用户明确批准清空覆盖。

治理validator与最终逐文件清单见本报告末尾收尾记录。

## 最终收尾校验

状态：VERIFIED（仅缓存清理/100对象定位准备）。任务完成时间：2026-10-09T20:01:15.881870+08:00。

- 实际命令：`pwsh -NoProfile -File Tools/Validate-ChangeLedger.ps1 -RepositoryRoot I:\GitHub\Unity_GAS\gameplay-ability-system-for-unity`，exit0、1392 Records、10个现有diff代码路径受记录覆盖、0ERROR、4265 WARNING；新准备脚本准确覆盖于本Change，没有本Change/新脚本的WARNING。大量历史Record“不在当前diff”等提示完整保留，没有为清理历史而扩大改动。证据：change-ledger-final-revision03.log/summary.json。
- 前两次Windows PowerShell5.1调用分别在默认参数初始化、Git换行stderr兼容性处中断；原日志仍保留。最终使用既有pwsh7.6.5运行相同validator，没有改脚本或Git配置。
- `git diff --check -- <本轮具名治理文档>` exit0；只有LF/CRLF提示。新准备源码无行末空白；本轮16个链接有效，5份原治理正文完整字节保留，见governance-check-final.json。
- 最终逐文件身份和6份原dirty备份/1192保护检查见Operation的after-manifest.json/final-validation.json。没有Git暂存/提交/push、删除/移动/恢复或另开优化批次。

下一步仅建议：若用户批准生产修复，独立修FindEffect的枚举分配；先保原查找语义与同一100范围0GC，再回到原千人混合collector证据确认剩余来源。H06四场景既有一致性证据按实际受影响路径复用，不自动重跑H07/95/Android。此次没有这一步的实施授权。
