# H10 R1–R6 获批窄读结果

Task：NTSD-H10-NARROW-AUDIO-COVERAGE-20261011。

**终态：STATIC_VALUE_GAPS_R1_R4_CLOSED_R5_R6_BOUNDARY_REMAINS。** R1–R4 四个具名声音值来源现已收口到静态覆盖层，前提是实体仍引用已准备的同代定义/帧内容，且准备后没有外部改写。R5/R6 的冰冻正式路径仍受三个准确helper的读取边界限制，不能证明可达或绕行。**本轮没有发现足以提出生产修复的已证缺陷，不提出代码写域、不加cue、不改规则。** 该结论只覆盖本次批准片段，不是全域无漏声或H10验收通过。

本次人类批准取代的是R1–R6提案的未授权状态；旧提案JSON保留原历史。本轮全部结论来自本次精确记录的片段及此前合法材料，没有使用旧越界片段。没有运行Unity/测试/编译/测量或委派子代理。

## 1. 原文件身份与实际读取范围

| 原文件 | 本轮始终一致的SHA-256 |
|---|---|
| `Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleDamageWriter.cs` | `18c0d8d89237e47c809c78b7e35d743e0d1f12e6b7c10780ada4d50821abb70f` |
| `Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs` | `70e3da17fbdc144076555940f6d37a3b39cd76a14759b30ba15766e46eb7ad5b` |
| `Assets/NTSD/Scripts/Animation/LF2Objects/LF2Character.cs` | `7e59a58765dbccd2c214ad976dc177c76a5f54fef34164145395edb59a89fd3c` |

[read-log.jsonl](read-log.jsonl)逐次保存目的、符号、方法区间、实际输出行号、原SHA和逐行文本；[read-summary.json](read-summary.json)去重汇总。方法区间只用于限制检索，**不表示整段正文已读**。

| R项 | 实际输出行号 | 说明 |
|---|---|---|
| R1 ApplyWeaponDamage | 1289、1366、1368–1370 | 声明、声音type guard、赋值、非空guard、请求；共5行 |
| R2 ApplyNativeType5HurtTail | 1927、1971、1973–1975 | 同上，共5行 |
| R3 ApplySpecialObjectHurtTail | 2000、2059、2061–2063 | 同上，共5行 |
| R4 QueueNativeC25FrameSounds | 6967–6988 | 获批完整小型声音方法，共22行 |
| R5 StateExitEvent / State_Frozen | 687、689、1623、1625 | 两声明与两个派发/转发语句，共4行 |
| R6 RunNativeC25FrameTransaction | 6748；6767–6768、6770、6772、6775、6779、6782、6785；6808–6809、6811；6820、6831、6834、6839、6843、6853、6855、6857、6859、6862、6866、6875 | 声明、条件、action-binding/exit名称的调用行，共24行；未输出事务其他写入 |

R6中的`else`仅按原行保存，不凭片段猜其完整负条件或分支可达性。原方法存在的其他提前返回/事务条件不在本轮证明范围内。每个声音点下面的guard均指**局部声音选择条件**，不是整个伤害事务到达该点的充分条件。

## 2. R1–R3：三个变量实际来自同一个已收集字段

三个片段的赋值都是：

```csharp
LF2HitResolveRuntimeData.ResolveCharacterData(attacker)?.weapon_broken_sound
```

R1/R2的局部type条件为`attacker.GetCurrentDataObjectTypeForSimulation() == 3`；R3为与`(int)LF2ObjectType.SpecialAttack`比较。随后均经`!string.IsNullOrEmpty(value)`才调用`attacker.QueueBattleSound(value)`。没有另一个未知数值转字符串过程，也没有在这三个赋值中读取目标对象的声音字段。

此前合法已读且本次SHA未变的`LF2CharacterDatHitResolver.cs:200–204`说明`ResolveCharacterData`优先取living对象的`_FrameDataWrapper.characterData`，否则取`entity.FrameCache.Wrapper.characterData`。因此definition identity是**当前攻击者实际绑定的wrapper/cache定义**；不是仅凭方法名把局部变量归到某个全局常量。

collector的`CollectBattleSoundIds:4415–4417`收集每个已加载定义的weapon hit/drop/broken字段。旧1000cue映射中，`weapon_broken_sound`有169个已保存字段出现、21个不同cue；本次只摘取这些旧行，没有重新导出catalog或重建全部映射。准确definitionId、cue、formalIdentity、formalKey、generic路由保存在 [cue-mapping-evidence.json](cue-mapping-evidence.json)。例如：

| 原字段cue | 原清单formal identity/key |
|---|---|
| `c\shino\w\e1.wav` | `c\shino\w\e1.wav` / `NTSD.FormalBattleAudioFile::c\shino\w\e1.wav` |
| `data\017.wav` | `data\017.wav` / `NTSD.FormalBattleAudioFile::data\017.wav` |
| `data\068.wav` | `data\068.wav` / `NTSD.FormalBattleAudioFile::data\068.wav` |

21个是broken字段子集合，不与旧“hit/drop/broken合计24个不同字符串”冲突；169是字段出现数，不是播放次数。该集合包含非type3定义，是对这三个producer的保守上界，**不是已证明的本局最小集合**。formal/generic身份和alias沿用旧记录，不根据文件名重新发明别名。

判定：**R1/R2/R3 STATIC_VALUE_COVERAGE_CLOSED_UNDER_SAME_GENERATION。** 如果运行时绑定不在已准备catalog中的定义，或外部改写字段，这一条件证明不再适用；本次没有证明任意动态输入都在闭包内，也没有将这些未发生的假设当作新功能任务。

## 3. R4：帧声音与collector一致的来源、上限和fallback

实际方法按如下内容边界工作：

- 使用`Frame.N`作为action；action小于0、大于等于999或等于`Runtime.NativeSoundActionLatch`时返回。
- 更新已有sound-action latch，再用`FrameCache.GetNativeFrameDataById(action)`取得帧；null时返回。
- `!frame.UsesLoganFrameNumbers && frame.FrameSounds.Count == 0`时请求`frame.sound`并返回。
- 其他情况读取`Math.Min(20, frame.FrameSounds.Count)`个条目；逐条跳过null/empty，请求`frame.FrameSounds[index]`。

这与此前合法已读collector`:4427–4434`的内容来源、legacy fallback及cap20一致。旧记录的976个不同frame cue及其identity/alias映射复用；旧统计`allFrameSoundValuesHaveFrameSounds=true`、现内容maximumCapturedFrameSounds=1也只保留为该内容指纹下的历史事实。

判定：**R4 STATIC_VALUE_COVERAGE_CLOSED_UNDER_SAME_GENERATION**，条件是native FrameCache查到的是这代已准备定义的帧，且没有准备后修改内容。没有扩读`GetNativeFrameDataById`或重新验其全生命周期。

注意该小型声音方法会更新既有latch，并非无状态getter。本次只读、不运行或修改它；静态内容覆盖不代替新鲜逐tick声音事件次数、retrigger、checksum或运行时顺序验收。

## 4. armor和整数sound字段的结论边界

在R1–R3获批的三个**实际声音值生产片段**中，值直接取攻击者`weapon_broken_sound`，局部guard只检查对象type与该字符串是否为空；没有使用`armor.sound1/sound2`、`InteractionArea.sound`或`WeaponStrengthEntry.sound`。

因此这三处原变量未知项不能再作为“可能经armor/int映射产生其他cue”的未解释缺口。**这不证明全项目没有其他armor/int消费者**；本次没有读取其他damage分支，也不为这些数据字段发明整数→SFX规则。此前关于它们的全域状态仍是准确的未知范围，不新增实现。

## 5. IceShatter：兼容请求确定，正式可达/绕行仍未证

R5直接读到：

```csharp
return RunStateExitPhase() || DispatchCurrentStateEvent("state_exit");
```

由C#短路语义可确认：前者返回true时不会求值后者，返回false时才调用后者。**但本次没有读取前者的返回条件，也没有读取后者把当前状态路由到哪个状态方法。** 不能从这行推断真实frozen退出一定短路，或一定到达`State_Frozen`。

`State_Frozen`确实直接转发到`_damageStateResolver.StateFrozen(eventType,eventData)`。此前合法证据确认该resolver在`state_exit`时请求`NTSDGlobal.Sound.IceShatter`，即`Battle/Ice/Shatter`。该常量不在旧1000cue中仍是事实，但目前只是**兼容请求与清单的差异**，不是已观察的正式漏声。

R6在获批事务内未匹配到直接`StateExitEvent/state_exit/Frozen`调用；实际条件分支调用的是`BindNativeC25Action`，既有false也有true实参。没有读取binder实现或参数定义，**不把这两个布尔值解释为启用/禁用状态回调**。没有直接调用也不能证明没有间接调用。故不能宣布当前正式路径绕行，更不能扩为“所有模式永不触发”。本轮实际运行次数为0。

剩余只到以下三个已观察callee，详 [remaining-symbols.json](remaining-symbols.json)：

| 准确剩余符号 | 已证明的调用位置 | 尚需回答的最小问题 |
|---|---|---|
| `RunStateExitPhase` | LF2Character.cs:689，StateExitEvent | frozen/native退出时哪些guard决定短路返回值；不需要其他状态行为 |
| `DispatchCurrentStateEvent` | 同上 | state_exit是否及何时路由到State_Frozen；不需要其他状态体 |
| `BindNativeC25Action` | LF2Entity.cs:6775等七个已保存调用行，RunNativeC25FrameTransaction | 参数及调用中的frozen state_exit回调/绕过语义；不需要整个frame事务 |

本次未越过批准方法区间去定位或读取这些callee的定义；**调用位置已确定，声明文件位置尚未新核**，报告不猜所属文件。后续若人类决定解决这一剩余问题，应先准确定位声明，再限定到上述问题所需片段；当前没有这三者正文的新增批准，不自动读取，不建议整个Q06解冻。

## 6. 可据此作出的决定与保留条件

1. 四个旧“局部变量来源未知”的记录可在**静态、同代定义前提**下关闭；旧报告不改写，本新报告追加更正及来源。完整逐R矩阵见 [r-evidence-matrix.json](r-evidence-matrix.json)。
2. 本轮没有确认生产缺陷，因而没有合理的最小生产补丁可提交；不加IceShatter cue、不改decoder/collector、type或声音规则。
3. 冰冻可达性剩余准确三callee，停止在读取许可边界；不用重复覆盖已解决R1–R4，也不为此重跑全音频准备或性能组。
4. Menu仍UNKNOWN_TOOL_QUERY_UNAVAILABLE；不重试execute_code、换编译器或修插件。预算仍UNDECIDED；218.313758MiB只是原完整目录对象账本，不从示例选预算。不裁catalog、不恢复其他优化项或旧Goal/队列。
5. H10原卡的预算、0GC、普通/1000最坏集合、关闭和后期平台门继续按原范围保留；本次静态收口不是H07、Android、Player、声音可听性或完整H10验收。

## 7. 命令、保护与未执行

开始先读CURRENT-AUTHORITY，使用`rg -n`在三份确切文件上定位七个批准方法声明；随后Python只在其方法区间匹配声音赋值/调用、callback名称及词法包含该调用的条件，输出的每行均见read-log。R4整方法读取是明确批准例外，其余未输出大方法全文；没有全runtime上下文扫描或`-C/-A/-B`检索。

只读取旧metadata-coverage.json并选出已有weapon_broken_sound映射行；未写新音频/metadata导出程序。旧映射SHA为`c684058b13046ce34e9a3328773e3ce69201e97d0d4b13046fdc66215f08418b`，原inventory SHA继续为`db1d30c6fa4fbf43341797289706e644c77fe14bbc9bf2ea82ff93941557c1f9`。既有PCM/每局所有权/完整音频218.314MiB证据按原影响范围复用，不重测。

保护命令为`git status --short --branch`、`git rev-parse HEAD`、`git diff --cached --binary`、`git diff --binary`及SHA256；原dirty未恢复/清理/暂存。前后具名文件、目标源码、旧提案/报告与Git状态对比见final-audit.json。无代码改动，ChangeLedger不适用且未运行；无Unity、测试、编译、测量、资源变更或子代理。

本有限Task交付到此。机器终态见 [RESULT.json](RESULT.json)，不自动追加后继。
