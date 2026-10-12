# 本轮首差与最小夹具修正建议

状态：`MEASUREMENT_CALIBRATION_FAILED`。唯一 Play 原件和失败源码保留，不追加第二次 Play、不改生产代码。

实际首差：`ExerciseHot` 的空 scope 计数为0，紧接的 `new byte[97]` 正控经 `GC.KeepAlive` 保留后，`GC.GetAllocatedBytesForCurrentThread()` 差值仍为0，未达到 `>=97` 断言。发生在所有正式热样本之前。不能把0当成0GC，也不能据此称生产音频发生分配。尚未证明是计数API的平台行为、优化还是其他具体原因。

已定位可复用的本仓库工具：`Assets/NTSD/Scripts/Test/Editor/BattleScopedGcAllocationRecorder.cs`。本轮仅只读核其方法，不修改、不运行：

- 既有 `GC.Alloc` / Internal recorder，`CollectOnlyOnCurrentThread`，容量默认256。
- `Calibrate()` 使用触及首尾并保持存活的1MiB数组正控，以及空scope；必须看到正事件和空scope0事件。
- `HasZeroEvents` 要求前后校准均通过、scope有效、未wrapped/未saturated、0事件。rawValue及Unit保留；事件数不当GC回收停顿。
- 原已验证 recorder 证据不因本次另一计数器失效被覆盖。

最小修正提案仍只涉及本Task新增的 `H10ResidentClosureDiagnostics.PcBudget*`，不需要修改 recorder、生产或规则：

1. 将三个新scope改为复用上述 recorder；录入前后校准、ownerThread、Unit、capacity、sampleCount、wrapped、saturated和原值，避免嵌套重置同一个recorder。先单独窄资格验证有效；不取消正控。
2. 将校准本身放入 `try/finally` 原件输出范围。本次失败发生在旧 `try` 之前，首差由XML及play-result完整保留，但没有专门calibration JSON。
3. 预声明路由已证明本集合唯一实际generic文件是 `data\\016.wav`；新测试后续尚未执行的borrowed负控却使用 `data/001.wav`，该generic路径实际不存在。改选真实已准备的 `data\\016.wav`，在资格/冷准备阶段先断言其clip存在；不新建缺失文件、不更改soundRoot/controller。
4. Unity内 `Process.PrivateMemorySize64` 的本轮原值为0，列为不可用。进程预算改由后续独立Player的外部Windows采样器评价；不能把0写成通过。
5. 补充原始pre/post native ID清单、播放/转换前后资源身份、真实BattleBootstrap不存在的map-free guard和恢复的组件/config状态快照。保留“诊断空World、没有真实地图”的十一阶段边界，不能扩大为Battle场景卸载验证。
6. 源码仍是失败版，不在本轮事后改成未执行版。独立审阅同意修正后，用同Task的独立新输出子目录和新版本指纹先跑窄资格，再明确登记下一次有界Play；原唯一组不覆写，不能重跑同版刷PASS。

下一次运行仍必须补齐三轮正常prepare/play/shutdown/drain/reentry、真实local await取消/迟到、自然Destroy、same-frame/跨帧/连续tick请求与热范围0GC。它是同H10夹具修正与未完成门的继续，不是新行为合同，不需要重开R1–R4/条件native binder或旧PCM/全目录账本。
