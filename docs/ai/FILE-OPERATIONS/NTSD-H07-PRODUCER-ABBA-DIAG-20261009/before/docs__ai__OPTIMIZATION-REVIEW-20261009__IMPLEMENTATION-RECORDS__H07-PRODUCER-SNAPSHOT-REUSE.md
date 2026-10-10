# H07-INPUT：既有生产者快照复用，本輪未准入

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
