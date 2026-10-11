# H07 canonical同步静态复核

只读luna侧审与主会话交叉复核；没有运行时或性能证明。

## 实际顺序

AI Prepare/Commit canonical -> FreezeProducerState -> Proxy application -> native combo/direction/builtin route -> ProjectExactStateToLegacy(runtime) -> SynchronizeNativeExactAiStateFromRuntime -> Store.Capture(runtime) -> InputState.SyncFromRuntime。

主要定位：World3058-3317；NTSD28InputTwoPassModule35-80/102-194/207-237；Store122-142/526-574；BattleAiInputWriter45-49；BattleCharacterInputWriter167-176/186-240。

## owner与字段

Store.Capture直接执行点仅核到Bind73和post-route132。TryCaptureCommon104-119复制已存在canonical行，不重新读取Runtime。41字段：history6/cooldown8/combo9/previous7/held7/target3/boundary1。

AI writer已提交canonical不能证明后续Capture重复：Freeze复制到NativeInputProxy；Proxy可能复制别的角色的proxy；native route清EdgeWindow/Previous；ProjectExactStateToLegacy62-102再写Runtime的keys/previous/cooldown/combo。最后需要回到AI store供后续读取。InputState是另一个镜像owner；UnifiedRowPublisher也是另外的投影owner。

现有PublishAiProjectionIfChanged只对History0门、Unk360、Unk3FC比较后避免投影发布，不代表41字段Capture可省。

## 有限排除与未知

- 没有找到post-route canonical sync之后再次整行Capture的直接主路径。
- 原compat progress只捕获17字段，旧候选保留原结论，不能改名作新41字段优化。
- 具体无输入/无proxy/无路由变化分支是否能免Capture尚无覆盖全部字段的changed/version证明。加一个dirty flag本身需要完整写入覆盖，不能凭静态例子保证。
- legacy AI commit的runtime镜像字段集合与indexed补写Unk360/Unk3FC/Unk400不同。本轮只记录差异，不推断为bug或擅自改规则。
- 不能把静态必要写入解释为每帧每字段必定改变；同样不能把某次相等解释成所有帧可删。

结论：当前没有足够证据对生产主路径无条件跳过全行同步。此处属于同一输入问题、已实际查阅；没有因模块或历史标签排除。
