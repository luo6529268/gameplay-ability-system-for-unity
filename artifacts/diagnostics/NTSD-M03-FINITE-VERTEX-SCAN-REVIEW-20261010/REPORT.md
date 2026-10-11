# M-03 新 publication 顶点有限值扫描评审

结论：SCOPED_REVIEW_COMPLETE / LOCAL_SCAN_GAIN_SIGNAL_ONLY / READY_FOR_BOUNDED_IMPLEMENTATION。

本轮用户批准“可以试试看”，延续先诊断、形成准确方案再交执行会话的约定。没有修改生产代码。新增一个 Editor 诊断文件，实际 C# 等价、已有原 guard 契约及一次局部扫描成本组已经完成。当前证据支持一个有界候选进入完整 Backend.Build 验证，不代表 M03 已优化或生产准入。

## 确定的重复与方案

`BattleDynamicMeshBackend.BattleMeshChunk.WriteQuad` 将五个已计算标量写成四个顶点：

```
v0=(left,bottom,z)  v1=(left,top,z)
v2=(right,bottom,z) v3=(right,top,z)
```

当前 Upload 逐顶点扫描12个位置分量，实际只有5个独立标量。候选仍在同一 Upload guard 位置，按四个顶点一步检查 v0.x/y/z、v1.y、v2.x。保持 IsNaN/IsInfinity 检查、异常类型和消息、每个 chunk 的 native mutation 前置位置、chunk 升序及已完成前置 chunk 的语义。无新缓存、validity flag、跨帧状态或新生命周期所有者。

private staging array 只有 WriteQuad→WriteVertex 写入，无可写数组/ref 对外出口。native Mesh 输出不是 managed staging。候选依赖此 producer 不变量；任意外部反射破坏某个重复坐标不受支持。负控专门证明：单改 v3.z=NaN 时，五值扫描可能放行、原扫描拒绝。不得把候选作为任意顶点数组的通用验证器。未来新增 writer 必须重新审查不变量。

## 新鲜证据

- 最终新 focused 1/1，job `1eee40f7c3d14d2d91d7032ebacc2a79`。内部覆盖21次真实 prepared Build、Source2D/array/page三类、0/1/4096/4097及缩小/清空/恢复；36,876个实际quad验证重复位置精确float bits与原/新判定一致，native已上传位置与CPU位置一致，publication不变。
- 27次真实 NaN/+Inf/-Inf × xyz × 三绑定，原生产 guard 抛相同异常，失败chunk的 native顶点数据保持，原/新镜像均拒绝。
- 14,641组浮点矩形组合，覆盖负/零/极小/极大/NaN/Infinity及溢出算术、inactive tail、arbitrary corruption负控。不是14,641个独立NUnit case。
- 原非有限输入3/3：job `0f82ad6191454c2aa6eb1a936e8f6a05`。原跨chunk失败计数2/2：job `97e00186a480405790315b9dbd84d412`，无效command0/4096，保留原先已完成API调用语义。
- 唯一局部成本job `946a77d96da24a20bb0106e195bee174`：1/1表示执行完成；实际收益由下表和事前判据判定。

局部成本输入为真实 Backend 已生成、冷反射复制的stride44 CPU数组；反射/Build/校验/序列化不在计时内。每窗8warm+32sample，每sample一次完整扫描。两尺寸各OFF→ON→ON→OFF，未重复寻找PASS。扫描镜像返回bool而生产guard抛异常；所有计时输入有限，因此不测异常成本。不是直接测生产 Upload、native upload 或完整 Build。

| Quad数量 | 原扫描两窗 mean ms | 分组两窗 mean ms | 原最坏P95/P99 ms | 分组最坏P95/P99 ms |
|---|---|---|---|---|
| 2000 | 0.404596875 / 0.399490625 | 0.144115625 / 0.146031250 | 0.5764 / 0.5802 | 0.1733 / 0.1952 |
| 4096 | 0.754437500 / 0.692087500 | 0.286218750 / 0.306171875 | 0.8283 / 1.0586 | 0.3438 / 0.3473 |

两个尺寸均满足事前门：两个ON均值均低于两个OFF，ON最坏P95/P99不高于OFF最坏。只能称当前Editor局部扫描收益信号，不能把64%/59%左右比例外推为整个M03或游戏提升。ABBA也不自动证明所有差值均为算法因果收益。

原有同步主线程GC recorder前后正分配/空scope校准通过，256个测量scope均有效、0事件、无wrap/saturation。unit为TimeNanoseconds，不称bytes或GC停顿。不使用此前正例失败的GetAllocatedBytes结果。全部数组指纹不变。主线程独立从原ticks重算mean/P95/P99一致，5份源码/程序集成本窗前后相同。

## 历史与限制

首次import后请求测试无结果文件，NOT_EXECUTED，不作PASS。显式compile/reload后第一实际focused失败：夹具触发无效Build后未Clear就继续，违反既有恢复用法。只修正测试Clear、保留源码及失败semantic原件；最终focused通过。已有3项guard第一次请求还包含错误namespace的另一个方法，实际只运行3项；修正namespace后独立运行2项，没有将缺席项计作PASS。一次编译reload期间连接拒绝后恢复，无第二Editor或状态强改。

原same-publication缓存已因千人89次全为新publication而排除；旧UV尺寸候选收益门失败保持OFF。本次新guard候选未接生产。命令链另发现容量预扫描整snapshot复制、DisplayMotion同handle偏移转换两条静态线索，尚无成本/命中证据，未加入本次实验或派发范围。

## 收尾与后继

158个保护文件无漂移、7份真实dirty备份有效，HEAD/暂存差异不变。原Editor Menu仍clean、8根对象。原脚本全部保持；新诊断仅显式测试调用，无运行时owner。最终validator/diff见补充收尾记录。

后继完整指令：`execution-handoff.txt`。仅Backend默认OFF候选和本Editor诊断扩展，先实际候选正反例再唯一完整Build两尺寸组；收益不足或硬门失败保留OFF并结束，不调整warm、不刷PASS。此报告就绪不等于已经派发，派发实态见dispatch-receipt.json。

Final closure: Validate-ChangeLedger PASSED1412 records/exit0, historical warnings retained; scoped git diff --check exit0 (line-ending notices only), new diagnostic no trailing whitespace. OriginalEditor Menu clean8. Change VERIFIED only diagnostic; exact handoff prepared, dispatch receipt authoritative.

Dispatch confirmed: target01a1201b-7930-7c93-8c84-5bc7baada43f new turn01a125ac-20e7-7293-9f61-7824ee924fca active/inProgress error null, cursor88. Exact saved handoff sent; heartbeat automation restored ACTIVE5min, meaningful changes only. No new execution results yet.
