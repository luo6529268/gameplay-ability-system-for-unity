# H07-INPUT：既有生产者快照复用，用户暂缓

最新决定（2026-10-10）：**USER_DEFERRED / PRODUCTION_DEFAULT_OFF**。用户要求先记录并推进下一项；本候选不再自动追加实验、修复或推广。已有两轮结果保持其原范围；下面“当前结果/下一步”按记录日期理解，不覆盖本决定。

结论：当前局部producer刷新变快，但混战整tick回退未解释，生产默认OFF。此项已按具名授权完成判定，不自动继续长窗；第一项Role限定准入保持，H07/Android未通过。

原问题：87/88已有候选能在输入生产结束后省去一次仍然有效的角色基础Runtime快照自复制，但一直未进入普通生产默认。

当前生产代码（本轮没有修改）：
```csharp
public bool EnableCharacterInputProducerSnapshotReuseForDiagnostics { get; set; }
```
该bool默认false。RefreshCharacterInputSnapshotsAfterProducer中的既有候选仅在flag开启、exact LF2Character、AiControlled且基础快照仍有效时跳过RefreshRuntimeSnapshot；存在测试mutation hook仍走完整刷新。后续AI unified/shared/SoA行更新保持原顺序。

本轮实际修改的是冷测量入口。改前只有88历史入口和历史输出目录；新增以下真实方法：
```csharp
internal static ProductionEntityStressRequest BuildScopedProducerAdmissionRequest(int index)
{
    ProductionEntityStressRequest request = BuildProducerSnapshotReuseRequest(index);
    request.outputPath = ScopedProducerAdmissionOutputRoot + "/" + index.ToString("D2") + "-" +
        request.action + (index % 2 == 0 ? "-off" : "-on") + "/report.json";
    return request;
}
```
新增菜单wrapper沿既有BeginProducerSnapshotReuse、owner Apply/Observe/Restore。新模式bool只归原Editor Suite所有，容量1、finally及原终态清，不增加runtime缓存，不改变十一阶段关闭。改前真实dirty字节见同名FILE-OPERATIONS/before。

为何这样测：固定当前Role+DataOrientedCanonical+Sakura1000，其余合同不变；只让冷owner切既有复用OFF/ON。新增六项请求等价测试证明不夹带算法/配置变动，既有15项guard等影响域测试与owner测试共44/44，设置前后另2/2。

当前结果（120warm+180sample每窗，一次四窗）：
| 布局 | producer OFF→ON ms | tick OFF→ON ms | ON P95 ms |
|---|---:|---:|---:|
| 分散 | 1.514758→0.468723 | 62.096542→45.343205 | 55.435655 |
| 混战 | 1.111011→0.578927 | 50.428038→57.714222 | 138.610895 |

实际ON观察skip最大1000、OFF0；四窗可靠完整Driver稳态0GC、同布局20hash/完整末态一致、容量/十一阶段关闭通过。旧producer方法hash一致，87/88逐tick限定Driver资格按原scope复用；当前15项含输入双RNG/字段回退重新验证。不是用末态替代所有tick，不是正式EXE全域证明。

混战ON同时发生collector、late、NativeSampledState变慢。现有数据只证明广泛时间波动，未确定因果，不能把猜测环境噪声当豁免。因此拟议生产冷策略ConfigureProductionCharacterInputSnapshotReuse及Host接线均未实施，无生产改前/改后片段可展示，不称已优化默认路径。没有重新跑同版寻找PASS。

下一步只作为建议：H07-INPUT利用91已有归因定义更高价值的准确成本/字段/失效边界；若要定位此次波动，须先形成独立问题与新实验如何改变决定，由用户选择。两项授权到此结束，不实施95、新AI行为或其它路线。

[报告与所有命令/证据](../../../../artifacts/diagnostics/NTSD-H07-PRODUCER-SNAPSHOT-ADMISSION-20261009/REPORT.md) · [Task](../../TASKS/NTSD-H07-PRODUCER-SNAPSHOT-ADMISSION-20261009.md) · [Change](../../CHANGE-RECORDS/NTSD-H07-PRODUCER-ADMISSION-WINDOWS-001.md) · [H07-INPUT方案](../H07-INPUT.md)。唯一进度仍在battle-optimization-progress-tracker.md，不新增父项或旧完成批次。

## 2026-10-10 追加：独立ABBA诊断结束，生产仍OFF

旧“不准入”与原四窗记录保留。新Task NTSD-H07-PRODUCER-ABBA-DIAG-20261009只诊断，Change NTSD-H07-PRODUCER-ABBA-DIAG-001 VERIFIED仅冷接线/记录。
原冷owner用index奇偶设reuse并强制边界计时true；新增显式case，将序列下标与开关分离：
```csharp
sequenceIndex = index,
action = "combat1000",
reuseEnabled = index == 1 || index == 2,
inputBoundaryTimingEnabled = false,
```
新增ApplyProducerSnapshotReuseCase按case应用两flags；原Apply包装仍使用奇偶与计时true，不改变历史入口。QueryValid检查实际World flags和owner下标；Restore恢复原值、释放case。每run case最多4个、当前owner引用1个，只Editor冷状态；生产World/Host/guard/AI行未改，11阶段不变。

33/33聚焦（17新/16影响域旧）＋设置前后各1/1通过。唯一combat ABBA每窗120+180，实际开关、skip/refresh及边界calls0证实，不是ABAB。OFF均值90.524507/92.575576ms，ON92.838902/77.320909ms；OFF差2.051069ms，ON差15.517993ms，未复现稳定开关惩罚，也不据均值合并宣称收益。跨多相位同时波动和其它进程累计CPU活动只说明非隔离环境，未证明tick抢占/系统争用原因；新旧组同时变更顺序、运行时间、细分计时，不能独立归因计时关闭。

新失败：首OFF可靠完整同步Driver稳态2 GC.Alloc事件，首tick130，校准/覆盖有效；后三窗稳态0。调用点/分配字节未知，没有调用栈；窄gate/CollectionCount0不覆盖此失败，也不能把事件次数当停顿解释长尾。本组不能宣称0GC通过。四窗20hash/末态含旧混战snapshot一致、容量/关闭/设置恢复通过。

本次不生产接线/不修生产/不追加窗口。若还要恢复候选准入，先由用户决定独立的首次tick130调用点取证和可比较基线方案；没有足够证据提出具体生产修复，也不自动转下一热点。

[新诊断报告](../../../../artifacts/diagnostics/NTSD-H07-PRODUCER-ABBA-DIAG-20261009/REPORT.md) · [新Task](../../TASKS/NTSD-H07-PRODUCER-ABBA-DIAG-20261009.md) · [新Change](../../CHANGE-RECORDS/NTSD-H07-PRODUCER-ABBA-DIAG-001.md)。旧报告及失败原件未覆盖，不新增父项或旧完成批次。

## 2026-10-10 用户暂缓决定与恢复条件

用户原话：“那这个优化点，就先记录起来，我们先推进下个优化点，如何”。

- 已确认：省掉部分producer Runtime自复制确实有局部耗时下降；保持语义的guard和后续AI行发布仍保留。没有稳定整tick收益证据，生产开关保持false，没有生产准入完成状态。
- 保留未知：首轮混战整tick回退原因未确定；新ABBA未复现稳定惩罚，也不能证明收益。首OFF完整Driver稳态2次GC.Alloc、首tick130调用点/字节未知，作为独立基线问题挂账；不是已修复、不是GC停顿证明，也不归因本候选或95。
- 暂缓范围：本版候选的收益复测、生产接线和默认推广；不删除候选代码、旧失败或原始报告，不把它改成“永久无效”。FindEffect修复及Role已准入成果保持。
- 重开条件：用户明确选择恢复，并存在会改变判断的新依据，例如明确分配调用点且同fixture完整scope可靠0GC、能辨别小收益的可比较基线，或影响本成本块的实质代码变化。须先提交具名假设和有限实验；仅“再测一次看看”或换顺序后均值变好不够。推广仍需原正确性/0GC/容量/关闭/可辨收益门。
- 下一项：[H06-ROLE-EXACT：现有Role精确配对成本](../H06-ROLE-EXACT.md)。本轮只整理方案，不把候选暂缓作为必须先完成的依赖；若将来用受此次GC影响的fixture认证新候选，仍须解决对应资格缺口。

[唯一进度](../../../../Assets/NTSD/Docs/battle-optimization-progress-tracker.md) · [本次文档Task](../../TASKS/NTSD-OPTIMIZATION-DEFER-NEXT-PLAN-20261010.md)。本次不新增旧完成包或父项，不改变历史Change的范围及状态。
