# H10 剩余门：有限决策准备

Task：NTSD-H10-REMAINING-GATES-DECISION-20261011。终态：**DECISION_READY / FINITE_REVIEW_COMPLETE**。

**建议保留当前生产行为，确认已有两项内存整改的限定交付，不再以未知字段或工具失败为由继续自动实现。** 本次指定范围内没有发现新的、必须立即修改生产代码的已证实际缺陷。已应用的有界 PCM 转换、正式音频源每局所有权及原 stage1/stage5 关闭继续保留，完整 catalog 预加载保持。H10 父项仍不能写为全部验收通过：明确缺口是当前声音覆盖的几个受限值来源、批准预算及原卡中尚无相应完整证据的验收层级。

这不是全项目无缺陷证明，也不取消原门；本次没有改任何代码或运行 Unity。无需为维持任务活跃而再发明一个导出、测量或优化专项。

## 可直接裁决的有限决策表

| 事项 | 建议与当前事实 | 继续所需最小条件 | 做与不做的影响 |
|---|---|---|---|
| 当前生产方案 | 保留有界 PCM、formal 每局释放、完整预加载、sealed 后未知 cue 明确拒绝 | 保持现状无需新批准 | 已验证的整改成果继续有效；不宣称新增 CPU 收益或全部 H10 完成 |
| 已知声音值覆盖缺口 | 需要时只核下面 R1–R4 四个局部值来源；IceShatter 可达性单独 R5–R6 | 人类明确批准准确 Q06 只读切片，当前并未批准正文 | 可以把已知 UNKNOWN 收敛；不读则覆盖门保持开放，不猜出漏声或补 cue |
| 数字预算与口径 | 218.313758MiB 是完整目录去重 AudioClip 对象账本，不是 RSS 或切局峰值 | 人类确定目标平台/内容负载、音频对象预算与整局总额的关系、steady/transition 口径及数值 | 批准后才有预算是否满足的可判标准；不决定则只能交付实测账本，不能宣布低端适配或新增启动拒绝阈值 |
| 当前 Menu 组件身份 | 保留 UNKNOWN_TOOL_QUERY_UNAVAILABLE；已有组件存在、clean8、磁盘一致证据 | 一个能取稳定ID和指定 SerializedProperty、不会调用任意 getter 的合法只读能力；若需修工具，应另批准确工具范围 | 能补当前一致性，不能追溯历史恢复瞬间；不补是事故证据缺口，不是 H10 性能缺陷 |
| 按局 subset / 构建期音频格式 | 作为可选后续，当前不实施 | 仅在选择该优化时明确有限 match-root/content-generation/provider 合同及值得投入的收益证据 | 可能减少预加载/驻留，但收益未证明；不做保留完整预加载，仍需满足最终预算，不能把完整加载等同低内存认证 |
| 当前阶段交付与 H10 父项 | 已有整改可按原限定范围交付；父项未证条件明确保留 | 是否现在进入原正式验收，由人类在覆盖/预算条件可判后决定 | 不继续实施也能诚实收口本次技术工作；不能把收口改写成原验收门取消 |
| 旧兼容性 13/14 | 原 FAIL 保留，归因为既有 mono-reference 夹具假设的结论仅有静态基线证据 | 只有选择修该旧测试时，另批一个准确方法的夹具/断言范围，无需改生产声音规则 | 做可补对应单项测试；不做必须继续报告13/14，不能称全部回归通过 |

机器表见 [decision-table.json](decision-table.json)。旧256/320MiB讨论示例不是已批准预算，本次没有选择新数值。

## 指定声音字段的合法证据

本次读取范围只到明确允许的模型、解析器、两个 resolver、NTSDGlobal、NTSDSoundPlayer，以及 `CharacterAnimtorManager.CollectBattleSoundIds` 本方法。受限文件只输出具名方法声明或直接声音请求行，无上下文，不使用旧越界片段。完整来源见 [field-consumption-evidence.json](field-consumption-evidence.json)、[允许源码摘录](allowed-source-excerpts.txt)、[精确字段引用行](exact-field-reference-lines.txt)。

| 字段 | 直接观察到的用途 | 声音请求消费结论 |
|---|---|---|
| `LF2ArmorData.sound1/sound2` | `LF2ArmorData.cs:35–36` 为 string、默认 null；`:64–65`复制、`:97–98`入指纹；`LoganCombatRecordDecoder.cs:37–38`原字符串赋值；`LF2CharacterData.cs:79`保存armor列表 | 指定合法文件中没有找到以这两个字段形成声音请求的消费；collector未显式遍历armor。**全项目是否未消费仍UNKNOWN**，不能将其删除、补cue或判成漏声 |
| `InteractionArea.sound` | `LF2FrameData.cs:369`为int/默认0；`:444`复制；decoder`:152`用ParseInt32OrZero赋值 | 已读声音helper依赖`itr.effect`、Fall及weapon字符串，未读取此整数。没有合法证据把它转换成SFX或WAV，不能根据字段名补规则 |
| `WeaponStrengthEntry.sound` | `LF2FrameData.cs:593`为int/默认0；decoder`:203`赋值 | 指定合法文件中只证实数据解析，没有真实请求映射；当前全域消费状态UNKNOWN |
| `NTSDGlobal.Sound.IceShatter` | `NTSDGlobal.cs:201`固定为`Battle/Ice/Shatter`；`LF2CharacterDamageStateResolver.StateFrozen:150–162`在`state_exit`确实QueueBattleSound该值 | 兼容请求存在有证据；旧1000cue未含该值，当前19 builtin也没有。**正式运行可达性仍UNKNOWN**，并非已经证实生产漏声；不因为资源文件含066就等同SFX_066 |

旧 Q05 `ITR40-STRENGTH19-RECORD-DECODING-001:22`明确当时只接内容字段、不接伤害/声音/cover行为。它支持字段引入时的职责，**不证明当前以后从未新增消费者**。旧 TYPE3 音频 oracle 说明旧路径曾收敛到 attacker 的非空 weapon_broken_sound；版本化历史不能替代当前 `ApplySpecialObjectHurtTail.cue` 的合法来源核对。

已知有限值不需要再做全量导出：当前 `LF2CharacterDatHitResolver.cs:206–297`中，effect helper只产生SFX_001/002/006/010/011/004；普通受击使用001/006及032/033；post-hit helper为065/068/null；reduced helper来自attacker.weapon_broken_sound或victim.weapon_drop/hit_sound，空值回退002。这些值与旧清单的已保存映射复用，未重算1000cue、未解码音频。

`CollectBattleSoundIds:4403–4442`继续收集三个weapon字段、frame.sound或最多20个FrameSounds、两项模式KO路径；它确实没有显式访问上述armor和数值sound字段。证明“收集器没有读取字段”与证明“运行时会请求未加载声音”是不同结论。

对于public string入口，有限的实际生产者/配置集合和未知值拒绝合同才是核验边界。`NTSDSoundPlayer.GetOrPrepareCue:528–535`在sealed状态查不到prepared项时增加rejected计数并返回null；`:540–546`仅对严格SFX_三位数做正式路径映射。本次不要求穷举所有可能string，也不将非法/外部未配置输入升级为无限的加载功能需求。

## 最小受限读取建议：尚未授权、尚未执行

精确JSON见 [minimal-read-permissions.json](minimal-read-permissions.json)。下面仅提出声音值/可达性问题，**没有申请整份 Q06 或整个 writer 的正文读取**；如读到必须继续追另一受限helper，应先具名交回。

| 编号 | 文件 / 符号 | 所需问题与最小正文范围 |
|---|---|---|
| R1 | `Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleDamageWriter.cs::ApplyWeaponDamage`，声明1289、调用1370 | `sound`的赋值、选择guard、定义身份及QueueBattleSound；不读或调整伤害/RNG/HP写入 |
| R2 | 同文件 `ApplyNativeType5HurtTail`，声明1927、调用1975 | `cue`的有限值来源及选择guard；不展开其余tail副作用 |
| R3 | 同文件 `ApplySpecialObjectHurtTail`，声明2000、调用2063 | 当前`cue`是否只来自attacker.weapon_broken_sound及其guard；不能以旧oracle或越界片段代证 |
| R4 | `Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs::QueueNativeC25FrameSounds`，声明6967、调用6986 | 仅此声音方法：`sound`列表来源、index/cap20/fallback是否与collector一致；不展开周边frame事务 |
| R5 | `Assets/NTSD/Scripts/Animation/LF2Objects/LF2Character.cs::StateExitEvent`（687）、`State_Frozen`（1623） | 仅frozen state_exit的dispatch、转发和bypass guard，确认是否到达已知DamageStateResolver.StateFrozen |
| R6 | `Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs::RunNativeC25FrameTransaction`（6748） | 仅决定frozen state_exit调用/绕过的调用与guard，不是整方法审计。R5/R6仅在决定解决IceShatter正式可达性时需要 |

armor及两个数值字段尚未得到准确受限声音消费者的定位，不能虚构一个consumer符号或因此要求所有writer全文。如果人类需要补充定位，应先明确有限文件的引用索引读域，再按结果提出正文范围。

## Menu工具缺口不是新的性能专项

完整复用上一Task的 RESULT、field-comparison、原失败响应及REPORT。本次Editor调用0。已知ExecuteCode会先compile再invoke；原内层success=false、编译器启动失败，故读取代码未执行。用户代码约5179字符**不能证明缩短代码会解决工具引用参数过长**，本次未重试、换编译器或修插件。

已有根/类型/nativeID查询不能提供稳定Scene GUID+fileID或指定SerializedProperty。普通全属性serializer可能执行任意getter，不能作为无副作用fallback。所需能力准确列在 [tool-capability-gap.json](tool-capability-gap.json)：稳定ID、指定字段、settings引用、Transform/missing-script、前后SceneSetup/实例集合及经确认的被动Current。没有这些结果就维持UNKNOWN，不能造历史恢复字段快照。

## 与H10原卡和当前阶段的对应

已重新读取唯一H10卡的“完成边界”“验收条件”“未来测试矩阵”，以及README/DEPENDENCIES当前阶段更正。旧卡下面IMPLEMENTATION_NOT_STARTED是历史正文；文首生产接入与实际证据才是当前状态。

| 原验收范围 | 当前已有证据 / 不能外推的部分 |
|---|---|
| 样本/声道/增益/聚合/retrigger/voice不变 | 既有四WAV逐样本、common声音10项、实际生产五资源及生命周期证据按原影响范围复用；兼容组仍13/14。没有必要因纯离线审阅重跑全部规则 |
| 实际声音请求覆盖与未准备请求拒绝 | 当前1000cue已导出字段/19 builtin/KO映射及sealed拒绝分支有证据；本报告列出的几个受限变量与IceShatter可达性仍缺，不能称producer完全闭合 |
| 普通/1000最坏集合steady/transition及整局预算 | 完整目录两轮各228918567B对象账本已测；无批准总预算，29采样点不证明解码staging或Player延迟销毁的绝对峰值。缺的是可判口径和相应层级，不是已证内存超标 |
| 物化/播放热路径0GC与1000声音洪峰 | 旧完整目录测量不是1000声音事件压力或0GC证书；本次不将其冒充该门。原门保留，不因准备报告空白自动造新生产功能 |
| 取消、三轮进出、退出无旧callback/clip/voice/Lease | 已有7/7生命周期、实际manager三次循环与单次隔离Play延迟销毁门；完整目录只运行两轮，不写成三轮完整战斗场景验收。是否进入原正式验收应随具名场景/预算/覆盖条件决定，不立即追加一轮刷PASS |
| Android/ARM64/设备波形或听感/平台部署 | 用户已将设备/GPU、IL2CPP/ARM64、AB部署、Android构建发布后移；保留后期门，不作为当前本地内存整改必须先启动的工作 |

已实施成果包括长mono临时float payload有界（既有测量小于98304B）、正式mono转换成功后释放owned源、非mono保留到stage5、冷准备串行和迟到结果回收。唯一五音频组distinct对象9→5、bytes减少848237；全目录对象账本218.313758MiB。它们分别是临时payload、固定组驻留收益、完整目录账本，不相互替代，也不是整进程峰值或CPU提升。

**当前授权内已无有据且可合法继续的生产实现。** 待人类决定的是上述精确读取范围、预算目标/口径，以及是否要求现在补齐Menu工具能力或进入原正式验收。可选subset/新格式不自动启动。当前成果可按限定范围交付，H10父项门保持；无需另造任务来延续执行。

## 本次执行与保护

只读命令为 `rg --files`准确路径定位、无上下文`rg -n`字段/声明/调用行、允许文件的窄范围读取、已保存报告/JSON/ChangeRecord读取及SHA256复核。`exact-field-reference-command.json`保存八个允许文件的实际搜索参数；`commands-and-read-boundary.json`列出命令与读界限。一次旧partial路径定位不存在，随后通过文件名清单定位到实际LF2Character.cs并仅读两条声明；没有因此扩大正文读域。

没有运行Unity、编译、测试、测量、导出程序或新工具搜索，没有子代理、Q06正文或旧越界片段再读取，没有改旧报告/治理或源资源。没有代码改动，ChangeLedger不适用且未运行。最终具名指纹与Git保护结果见 final-audit.json；只新建本目录文件，旧失败/未知和原验收合同均保留。本有限Task完成即交回，不自动派新点或恢复Goal/队列。
