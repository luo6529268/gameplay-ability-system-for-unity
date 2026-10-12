# H10 输入、声音观察与采样恢复表 v2

状态：方案定稿候选 / NOT_AUTHORIZED_TO_IMPLEMENT_OR_RUN。以本件收窄CASE-MATRIX-CANDIDATE与PREFLIGHT-ADDENDUM的待定参数；仍需主审批准，尤其旧postprocessor新增域及下面精确冷读取。没有创建脚本或执行build/Player。

## 固定执行表及真实输入证据

一份build，两进程各三轮仍不增加：ordinary O1=120+180；O2/O3=120+1800；combat C1/C2/C3同窗。同PID完成本组三轮，任何首差停止后轮/第二进程。普通seed682973786；combat App初态从P1成功模板保留，harness请求1314149188独立记录。

O1/O2/O3分别P1=2/16/507，另外两个OID同局作为人类槽中性输入；使用主审已核的公共OnJoin→SelectCharacter→OnConfirmCharacter→SelectTeam→OnConfirmTeam。三个槽全部Confirmed、实际inputId互异且匹配Slot.PlayerId、第四及之后Idle、CMC0才可继续；不能将选择中状态算完成。mode/team必须是UI合法值，若普通VS因为组队或实际结果提前结束，保存失败，不禁止胜负、不改模式延长窗口。此候选未宣称3同队一定支持完整窗；没有实测就不能写该资格PASS。

每轮一次组合：O1 L→D→K；O2 L→D→J；O3 L→W→K，末尾中性；按当前已证P1 Defend/Right/Jump、Defend/Right/Attack、Defend/DepthUp/Jump。输入只经InputSystem.QueueStateEvent，**不复用旧PulsePendingKey的重试**。本件将“每键保证2消费tick”更正为“以实际观察到的CurrentTickIndex经过2tick为下一投递的最早条件”，不承诺主线程回调刚好落在两个逻辑tick之间。

候选时序：在正常InputSystem.onAfterUpdate回调确认新状态已被InputSystem处理，记录该回调QPC/frame/currentTick；在下一次满足tick条件的同类回调投递下一状态，状态只投递一次。不调用InputSystem.Update、不写Action字段、不StepOneTick/换Manual/暂停。记录每次Queue与下一次afterUpdate的真实时间/键状态、每个LateUpdate观察到的CurrentTickIndex与LastAppliedFrameInput中已核的P1值类型成员；tick跳跃时明确中间消费未知，不插值、不把输入设备状态冒充已消费packet。回调先后/值类型提取必须在新资格中核实；若无法取得安全的实际packet成员，准确补最小数据定义读域，不能反射热读取或保留可变包引用。

输入证据判据是有完整单次投递记录、实际观察到对应已消费P1输入且目标自然cue出现；不是沿用旧Manual的6/13/15/44固定tick或事件数。若跨tick漏采导致无法确认对应消费，记INPUT_OBSERVATION_INCONCLUSIVE并停止后轮，不第二次组合试到PASS。有限输入窗口仍在原sample内，不额外加暖窗。

## P2空World路线已获只读方案认可

P1普通全组三轮成功后才输出完整MatchConfig模板；P2先相同build的真实Menu/prewarm/VS选择，**只冷只读反射一次**`CharacterSelectionController.currentGameMode`(:84)取得该实际引用（已有明确允许），核字段类型/模式选项来源/对应built asset身份。跨进程不比较instanceID；绑定build manifest中该资源来源/GUID/字节与场景选择索引，原进程对象引用不能序列化成权威。

P2创建独立MatchConfig、只清players，其它字段及列表null语义全部沿模板；直接App.SetMatchConfig→LoadBattleAdditive，不点击Fight，不订阅MatchConfirmed来截断，不写原cfg或字段。suppression原值必须false且始终不写。真实map prepared/id/boundary、空world/slot/pool和声音owner准备门齐全后，原AreReady/StartRun。每轮新副本/新owner捕获，不混称P2完整Fight点击。

## 事件tee的准确容量和处理

已核readonly PendingSoundEvent(Cue string,WorldX int,Tick int)，原sink只接IReadOnlyList。拟新probe内部`H10BudgetSoundTee`不更换preparation owner，不成为生产manager：

| 状态/缓存 | 固定上限/owner | 使用和失效 |
|---|---:|---|
| phase行 | 256/进程，probe | 冷构造，最终导出后释放 |
| 声音目录计数 | 1024项/round，当前1000；probe | 冷阶段根据实际prepared/旧目录身份建表；超1024拒绝启动，不裁cue |
| cue只读索引 | 2048键最大，probe | 冷建立已有raw ID与斜杠别名；不合并builtin与DAT身份；alias冲突拒绝。热路径仅TryGetValue，绝不Add/规范化新字符串 |
| 目标事件与实际voice证据 | 各4096行/round | 只保存目标四cue的Cue引用/Tick/X/顺序及forward前后实际voice序号/clip/source身份；溢出使本轮证据FAIL，仍转发完整原批 |
| 未知cue证据 | 32条首见记录 | 保留原string引用与tick；第一条已使闭包覆盖FAIL，不等到32才失败；不截断生产列表 |
| input状态转换 | 32/进程 | 单次按键表＋释放，无重试 |
| 已消费输入观察 | 8192/进程 | 只在CurrentTickIndex变化时存一次值；三轮目标4140tick；gap标UNKNOWN，满则FAIL |
| voice冷借用句柄和before sequence | 64/round | 复用现有native64容量；cold捕获固定数组，热逐索引读现有数组与AudioSource属性，不反射、不GetComponents |
| owned引用 | 2048/round，最多6144/3round | 旧owner释放后核Unity对象已消失，再释放本probe引用；不替代原owner销毁 |

每批观测先捕获必要计数/序号，再**无论观测溢出/异常如何都在finally把原IReadOnlyList精确forward一次给捕获的原实际声音sink**，顺序不变、不复制/改写/分组/丢弃。原forward异常原样传播，不能由诊断异常遮蔽。观测异常只置固定失败码/保存异常引用，热路径不格式化日志/扩容；完成当前原forward后，由probe下个安全外层收尾停止新增诊断输入、恢复sink、公开close，不能让一次观察失败吞声音。新增计数避免数值overflow，达到上限标失败而非扩容。

实际播放证据使用本次start sequence前后变化找到真正启动的source；不用“第一个同clip source”旧错误。原64数组/身份绑定应在冷准备后稳定，若长度/身份不满足资格则拒绝开始，不能热反射重新抓数组。目标cue与实际播放clip的关联以冷prepared身份表核实；仅PooledOneShotPlayCount总增量不够。新tee的资格必须验证forward exactly once、异常/overflow仍forward、每批顺序/载荷不变、有效GC正控/空控/自身scope0event；旧PcBudgetSink/63scope不能替代。

## 安装、恢复与最小冷读取待批准范围

主审已核Driver1816–1833：preparation owner不能换，SetSink设置的是diagnostic sink字段，无公开getter。执行会话仅rg核到了这些准确声明，**本请求不授权实际reflection或写入**：

- `SimulationTickDriver.cs:277` `_soundPresentationSinkForDiagnostics`；`:292` `_battleSoundPlayer`。仅冷阶段读取这两个字段的精确当前引用，分别在安装前/后与恢复前/后核验，不读其它Driver正文。原sink为null时，它是diagnostic override为空，不伪造为owner；tee实际forward到captured owner，恢复时仍设原null。
- `NTSDSoundPlayer.cs:78` `oneShotVoices`、`:82` `oneShotVoiceStartSequences`、`:83` `battleVoiceIdentities`。仅冷只读取得既有数组引用与长度，禁止写数组；冷准备前后须确保容量/引用稳定。必要的prepared target-cue/playback-clip关联优先复用此前PcBudget的已审冷观察方式；如需新的字段，只报精确声明后再审，不递归读播放分配器。

ordinary安装：实际BattleRunning/owner已prepared且无worker，在warm计数起点前capture原sink/owner，SetSink(tee)后核实际sink=tee且owner仍同一引用。结束sample后、公开App关闭前，同一同步收尾检查当前sink仍本tee、owner未变，然后SetSink(original)并复核。

combat安装：StartRun.Configure同步返回后、runner首次Update生成/推进前，捕获当前driver/owner再安装。不能假设Configure没有改sink。终态runner自动RecreateWorld可能先关audio/重建World；在原runner结束后先核新的当前driver仍为捕获实例、实际sink仍本tee、owner仍同一组件，才恢复原sink，再做App真实map关闭。若身份已经变化，禁止写另一个owner的字段/强制恢复，保留冲突并停止，由已有公共owner安全收尾。不能自称捕到内部stage5瞬间。

probe stage1停止输入/新观察，恢复自有sink和InputSystem订阅；stage2无worker，3不生成，4沿原owner，5只观察原audio关闭，6–10不干预资源回收，11只认可App真实map结果；之后核tail/owned refs/scene unload，释放自有数组引用。只解绑自己的订阅；只移除自己确实创建的keyboard设备，不动既有设备。冷反射方法/FieldInfo缓存由probe所有，退出失效，无静态跨进程/跨局隐藏owner。

## 采样与失败恢复锁定候选

100ms绝对QPC节拍；read-start/read-end/slot/lateness/UTC全部保留。private>0、身份精确PID+CreationTime；最大gap250ms、每连续域slot覆盖至少99%、边界距首末有效样本≤250ms；否则INVALID不是低峰PASS。跨界归更严bucket，未知runner active全段按2GiB。3GiB仅transition采样峰、320MiB对象域保持，WS另列，不推导采样间瞬时峰。

build45min只记录不killEditor；UI预热480s、选择累计120s、Battleprepare180s、warm/sample600s/round、关闭tail120s、每进程3600s。失败即停止余项，精确captured App公开关闭通过后Quit非零。无响应/关闭失败保存身份和残留，不自动kill、不清缓存、不重启、不重跑。同一Task后续若需终止具体诊断进程另按明确处置。旧processor兼容扩域未批准之前，绝不执行这张表。
