# H10 声音来源与按局集合只读审查

Task：NTSD-H10-AUDIO-PRODUCER-COVERAGE-REVIEW-20261011  
日期：2026-10-11（Asia/Shanghai）  
状态：**REVIEW_READY_WITH_BOUNDARY_INCIDENT**。交付只读证据；未实施新优化，H10 父项状态未改。

**结论：当前证据支持保留完整 catalog 预加载，尚不足以准入按局声音子集。** 这次已把缺口收窄到具体值来源/依赖边，而非笼统的“producer 未知”：现有静态 collector 的1000 cue全部有metadata来源；一条兼容请求常量不在清单中、数个受限局部变量未闭合、armors等额外声音字段未包含在旧清单结构、stage/reserve/piece目标图与运行期输入集合缺失。本轮没有把任何未知项判定为真实生产漏声，也没有新增收益或预算合格线。

**执行偏差必须单列：**根集合只读子代理的宽上下文搜索带出了受限 Q06 writer/factory 方法体。已停止进一步读取并记录准确已知范围；这些片段从结论证据中排除，但读取本身确实越界，不能称本轮完全满足禁读要求。见 [READ-BOUNDARY-INCIDENT.md](READ-BOUNDARY-INCIDENT.md)。本报告的可证字段/静态边由主代理独立核对允许范围，禁止把被排除片段当作当前 writer 正确性证明。

## 1. 范围与证据等级

仅新增本 Task 的 artifacts/diagnostics 目录文件；旧报告、源码、资源、场景、设置、治理状态均未改。没有运行 Unity/测试/测量/编译，也没有调用运行中的 Editor、创建/清理对象、解码音频或恢复 Goal/自动队列。当前目录 TASK.md 保留事前合同；最终只读审计见 final-audit.json，任务实际状态见 TASK-RESULT.json。

- **A**：当前可读字段、直接值赋值、非受限 helper/collector 源码。
- **M**：既有实际 C# inventory / 资源测量原件的离线比对。证明记录配置，不代表本轮 runtime。
- **H**：历史 ChangeRecord 合同/既有测试；不能自动升级为当前完整运行证明。
- **U**：未保存的值/动态边、受限体没有合法审查或入口可达性未证。
- 直接调用行可证明字符串实参或字段名；不能凭方法名推断分支、顺序或动态目标。读取偏差涉及的实现体不纳入 A 级证据。

主要机器清单：
- [request-sites.json](request-sites.json)：58条精确请求/转发/声明索引，含7条声明、51条调用。
- [request-coverage-matrix.json](request-coverage-matrix.json)：每条来源、有限值、collector关系、manifest identity/key、未知原因；转发器与真正值生产者分开。
- [metadata-coverage.json](metadata-coverage.json)：1000 cue逐条原始definition/frame/weapon/builtin/mode来源及formal/generic映射。
- [root-dependency-matrix.json](root-dependency-matrix.json)：9类根/依赖边、实际字段、冻结时点与阻断条件。
- [additional-metadata-gaps.json](additional-metadata-gaps.json)：旧inventory未导出的armor及数值sound字段。
- [review-summary.json](review-summary.json)：集合统计、明确缺失常量、旧对象账本映射限制。

## 2. 实际收集链及离线结果

CharacterAnimtorManager.CollectBattleSoundIds（:4403）遍历全部 TotalCharacterFrameConfig.Values：收集weapon_hit/drop/broken；legacy无FrameSounds时取frame.sound，其他情况取最多20个FrameSounds；再加入PublishedLoganCatalog.ModeComboInput.KnockoutFeed两字段。NTSDSoundPlayer.PrepareBattleCuesCoreAsync（:337-340）合并19内建cue。完整准备结束才在:378设battleCatalogSealed。

本轮复用的inventory SHA-256：
db1d30c6fa4fbf43341797289706e644c77fe14bbc9bf2ea82ff93941557c1f9。

| 内容 | 离线结果 | 严格含义 |
|---|---:|---|
| 正式定义 | 330；type0为158，其余172 | 真实已保存C# catalog，不是旧CSV猜类型 |
| collector cue | 1000 | 每条均找到已保存metadata/builtin/mode来源 |
| formal唯一身份 | 978 | 多个cue可为同一formal identity alias |
| generic现存路由 / 唯一文件 | 77 / 72 | 复用旧实际路径与对象账本 |
| frame.sound非空字段出现 | 8864 | 字段出现数，不是播放/事件次数 |
| FrameSounds前20字段出现 | 8864；本内容每帧最大1 | 现有内容没有多声帧越过20的样本；不能外推所有未来内容 |
| frame不同字符串 | 976 | 全部在1000 cue清单内 |
| weapon hit/drop/broken非空字段出现 | 各169 | 合计24个不同字符串，全在清单内 |
| builtin / mode KO | 19 / 2 | 全部已映射 |
| 已导出声音字段缺失cue / 无来源cue | 0 / 0 | 仅对旧inventory已导出的字段成立 |

旧 perDefinition 仅含 id/type/includesPublishedModeCues/cues/weapon/frames，不含OPoint图、stage选择、armor数组、weapon-piece Fields、运行期根集合。不能将上述0缺失扩大成“所有声音请求0遗漏”。

既有两轮完整组去重对象账本228,918,567B（218.313758MiB）保持原有效范围，未重跑。旧stage clip行未保存完整formal identity/native ID配对，仅name/samples/channels/frequency等元数据；其中19个元数据tuple不是唯一。故不得凭同名clip猜路径→对象双射。本轮没有证明安全subset，也没有给出可兑现的新驻留收益；旧144,100,664B候选仍是估算。

## 3. 请求参数最终来源与覆盖矩阵

下列路径均相对于仓库根。完整逐条机器矩阵保留每个调用行和实参，不仅是本表的合并展示。

| 请求入口 / 行号 | 最终值来源 | collector与实际identity | 证据与剩余限制 |
|---|---|---|---|
| LF2CharacterDatHitResolver.cs:142；LF2CharacterDamageStateResolver.cs:397 | 字面量SFX_039 / SFX_006 | builtin→data\039.wav / data\006.wav | A+M；只证明值域覆盖 |
| LF2CharacterDatHitResolver.cs:221 | itr.effect switch :211-219 →SFX_001/002/006/010/011/004 | 全在19builtin中，各映射data\NNN.wav | A+M，变量cue已收敛为有限常量 |
| 同文件:265/269、268 | Fall==80选择006/001；effect1附加033/032 | 四值均在builtin清单 | A+M；非“任意baseSound” |
| 同文件:245/256；LF2Entity.cs:4394 | 当前wrapper.characterData.weapon_hit_sound / weapon_broken_sound；ResolveCharacterData:200-204使用living wrapper或FrameCache | 对保存330定义的这些字段全覆盖；24个不同weapon字符串在机器清单逐条映射，例如data\017.wav等 | A+M；动态换定义/修改字段后的值不由旧snapshot证明；Entity的受限cleanup体未用于结论 |
| LF2Entity.cs:4464/5913/6190/6229/6316/6362 | 066/016/011/006等直接字面量 | builtin→对应formal identity | A调用行+M；不审该Q06分支的触发/执行顺序 |
| LF2Entity.cs:5508；LF2SpecialAttack.cs:785 | 局部soundId直接来自Frame?.D?.sound（:5504/:781） | captured frame字段全部匹配；例如data\003.wav与角色custom路径 | A+M，条件是当前Frame来自该代定义 |
| LF2Entity.cs:6978；LF2LivingObject.cs:381；LF2OtherObject.ReleaseFlow.partial.cs:25；LF2WeaponBase.cs:516 | frame.sound / Frame.D.sound | 同上 | 调用行字段+M；不能把完整native帧事务称已审 |
| LF2Entity.cs:6986 | QueueNativeC25FrameSounds里的局部sound | 旧前20 FrameSounds值全在清单，但本调用体未合法展开 | **U**：该变量的真实列表来源/索引/边界需具名审查，不能由函数名补写 |
| LF2Weapon.cs:111/127；LF2WeaponBase.cs:1045 | public auto-property WeaponDropSound / WeaponBrokenSound | 默认空；未找到authored生产赋值，只有一个测试写WeaponBrokenSound | **U**：这些property不是有证据的characterData字段别名；外部非空setter值无固定manifest保证，未更改默认或补赋值 |
| LF2SpecialAttack.PlaySound / LF2WeaponBase.PlaySound / LF2Entity.QueueBattleSound / World.QueueSound | caller提供非空string，转发/封装PendingSoundEvent | 转发入口无独立cue集合 | 不宣称任意外部string可被full catalog涵盖；sealed NTSDSoundPlayer未知cue增加rejected计数并返回null，未添加lazy load |
| BattleResultsWriter.cs:39/88/130/146/152/158/167/209/214/277/283/299 | 001/002/004/006；:130的下一行是TableCursor==10选择004/006 | 全builtin | A+M，无额外动态sound配置 |
| NTSDBattleTickSystem.cs:611 | team1/5缓存路径←ConfigureNativeKnockoutAudio←Driver在tick0从同Published Mode KnockoutFeed写入 | 实际data/m_ok.wav、data/m_join.wav→normalized identity data\m_ok.wav/data\m_join.wav；collector同时收两字段 | A+M；模式KO有具体传值链，不与stage spawn OID混淆 |
| BattleDamageWriter.cs:1265 | ResolveStandardPostHitSound纯helper（DatHitResolver:273-280）仅065/068/null | builtin覆盖非空返回值 | helper值域A+M，Q06调用事务不在结论范围 |
| BattleDamageWriter.cs:2654 | 已索引的ResolveReducedHitSound helper：attacker broken，或victim drop/hit，空值退002（DatHitResolver:283-297） | 对当前定义字段+builtin002条件覆盖 | helper域可证；只凭调用索引不称整条Q06路径闭合 |
| BattleDamageWriter.cs:1370/1975/2063 | ApplyWeaponDamage.sound / ApplyNativeType5HurtTail.cue / ApplySpecialObjectHurtTail.cue | 尚无合法完整有限值证明 | **U**：具体三个局部值来源；未用广域上下文补读 |
| LF2CharacterDamageStateResolver.cs:156 | NTSDGlobal.Sound.IceShatter = Battle/Ice/Shatter（NTSDGlobal:201） | **不在1000 cue，也不在19builtin**；Assets/NTSD/Sound/Battle/Ice/Shatter/shatter_1_066.wav存在。没有自动alias到SFX_066/data\066.wav | **明确清单差异，但正式可达性U**；LF2Character State_Frozen/dispatch存在，外层native退出路径可能绕过旧状态回调，未证明实际发生漏声，不擅自新增cue或改规则 |

数值sound字段不能被自动解释为路径：InteractionArea.sound、WeaponStrengthEntry.sound由decoder解析int；LF2CharacterData.armors中的sound1/sound2为额外string字段。当前collector未显式遍历这些字段，旧inventory也没有保存它们。它们最终是否作为声音请求值使用，需要真实consumer证据；本轮既不把它们忽略成已覆盖，也不发明SFX_NNN转换规则。准确字段位置见additional-metadata-gaps.json。

正式resolver在NTSDSoundPlayer:540-546仅对严格SFX_三位数做data/NNN.wav转换；:557-580按normalized identity复用/登记alias。文件名都带066不等于Battle/Ice/Shatter自动在formal字典中有别名。

## 4. 本局根、固定边与真正阻断项

正常入口可直接追到：CharacterSelectionController.StartBattle:455-475创建MatchConfig并传AppManager；AppManager.ApplyMatchConfig:167→players.use.characterId根:189-198→准备服务/真实角色创建:199-200→音频准备:204→内容身份复核:212→allocation seal:219。SetupBattleCharacters:284-347先检查角色config，随后用slot.characterId取逻辑对象并ModuleBind该frameData。**真实玩家创建者已定位，不再用声音collector代替它。**但旧音频-only夹具没有保存一局MatchConfig/实际玩家根，不能把该测量称按局配置。

| 根/边 | 当前可证固定事实 | 阻止子集准入的具体缺口 |
|---|---|---|
| 玩家 | UI MatchConfig.players→use/characterId→实际创建/绑定；发生在audio seal前 | 没有该测量对应的具体players值；其他创建入口不由玩家列表限定 |
| OPoint | visual helper递归frame.opoint.Value.Oid/opoints[].Oid；仅存在config才Include，缺失静默不入 | 旧audio snapshot没有OID边；runtime overrides/child传播未合法核定；**未声称存在已观察的越界child** |
| 静态变身 | state9995→50；4000..4999→state-4000；8000..8999→state-8000 | 这是三条visual helper资源边，不代表完整运行期变身规则；KindInput动态目标映射未在音频证据中 |
| stage | MatchConfig.stageCampaignFilePath/stageSeriesId；spawn.Id/Act/Hp/Times/X/Y/Ratio/Join；helper赋spawn.Id→task.opoint.oid、Act→action | 没有本局实际campaign/series/phase的spawn ID表；mode KO两个声音不是stage spawn表。默认stage部署仍暂缓，不制造资源任务 |
| reserve | OPoint.Reserve/JoinReserve/JoinPic；runtime ReserveCommittedTotal/Hp为计数矩阵 | 这些字段不是目标OID表；reserve→最终晚生成目标的传播边未证。stage.Join不能冒充join_reserve |
| 随机武器 | buffer固定候选范围[100,200)，容量100；当前catalog只有101/120/121/122/123/124/150/151，均非type0 | **随机本身并不阻止保守集合**：可囊括全部8项；仍缺World provider及该代定义不变的证明。旧全non0候选已含这8项，不能把随机选择当作虚构的新缺口 |
| respawn/late/external | 可定位RunRespawn及object-id创建接口；QueueSound外部string入口无类型级cue限制 | 缺允许definition集合/来源epoch/封闭世界合同；不推断实际引入catalog外对象 |
| weapon-piece | metadata有block.Fields/Groups[].Piece/Fields/Variants；H级历史合同给默认-1→999与native专用OID0；当前full包含0、50、999 | 旧音频snapshot没有实际Fields与piece→目标alias图；历史边不是当前writer体证明。普通OPoint的非正OID条件不得套到native piece |

因此不是“所有动态producer都永远无法静态处理”，而是**已有固定边可以保守加入，尚缺的几条字段/目标映射及根选择还不能据现有保存物关闭**。没有safe subset，不提供离线差量收益数字。保留全catalog的成本已测；预算是否接受它是另一问题。

## 5. Menu 恢复证明更正

完整证据链及最小后继检查见 [INCIDENT-EVIDENCE-BOUNDARY.md](INCIDENT-EVIDENCE-BOUNDARY.md)。

- 首次失败原件证明root216430丢失；场景及meta四项SHA保持。
- 现存Editor.log LF行5068821确实记录Menu加载；前面的首次版本入口源码3137、job/XML时段/seed区别于v2入口3149及Loaded行5347069。该Loaded行没有自己的jobID/时间戳，关联来自版本/执行顺序和原件，已明确限制。
- TestFramework1.1.33保存/恢复SceneSetup的源码支持设计路径；恢复方法没有组件相等断言。
- 已观察loaded/clean/root8不等于manager组件逐字段恢复；sceneRestoredByFramework=true为常量，mmSoundManagerRestored=true曾与root缺失同时存在。
- 缺恢复后稳定组件ID/script GUID、settingsSo、AudioSourcePoolSize/PoolCanExpand、active/enabled/层级/缺失脚本快照。旧完整报告的“精确恢复”只保留其明确检查范围，不能外推这一层。
- 不撤销后续修正资格和完整音频组各自新基线上的结果，不重开旧加载实验。当前场景只读组件一致性检查能补“现在”，不能追溯历史瞬间。

## 6. 当前H10收尾差距与最小后继

| 分类 | 具体项 | 本轮判定 |
|---|---|---|
| 已应用且已验证到声明范围 | 有界PCM临时payload、逐样本/增益/retrigger；formal session owner、stage1/5/取消/迟到/并发准备；完整目录两轮对象账本及generic复用/精确增量释放 | 复用原报告/指纹，不重跑五资源或旧ABBA；旧成本尖峰、兼容13/14中的FAIL与本轮早期资格FAIL仍保留 |
| 当前授权可继续的明确技术证据工作 | 当前Menu组件被动身份检查；完善已保存metadata导出；在允许范围证明IceShatter正式可达性和声源有限值；必要时明确列Q06读域交讨论会话裁决 | 不将“继续H10”当自动扩读Q06授权，不改变producer/播放规则 |
| 未闭合的原有技术验收 | 所有合法请求覆盖、条件适用的0GC/取消/三轮/洪峰；decoder/staging及Player延迟销毁的transition峰值 | 当前账本不能代证。先核旧证据适用性与当前阶段必要性，不能全部机械重跑，也不能自行取消这些门 |
| 真正产品决策 | audio/整局steady与transition预算、增长余量、超预算处置；当前阶段是否接受保守full preload | 没有总内存分配依据就不选256/320MiB；操作前2GiB余量也不是产品预算 |
| 已明确后移 | Android/ARM64真机、GPU/IL2CPP证书、AB部署与交付载体、构建期PCM格式 | 不作为本只读任务的阻塞，不另起设备/部署/格式优化 |

**建议不新增生产裁剪/惰性加载/格式改动。** 目前没有经覆盖证明、可兑现收益且不改规则的子集候选。这个结论只针对现有证据，不是说H10没有优化空间。

交回讨论会话的一项最小后继为：**一次当前Menu管理组件的非破坏只读验收**。它直接补已发生事故留下的当前状态证据，不再加载音频：

- 精确写域：仅一个新diagnostics目录的JSON/报告；C#、资源和Scene零写域；使用现有Editor被动查询能力，不新增持久插件/脚本。
- 读取目标：NTSD_Menu scene GUID；SoundManager GO fileID762446124、MMSoundManager fileID762446125/script GUID614edb33c38bcf949846d5675f29f9fb；settingsSo GUID07ea3ff88e1ecb84d91f58aa804badc6/localfileID11400000；AutomaticallyUnparentOnAwake1、AudioSourcePoolSize10、PoolCanExpand1；parent/active/enabled/缺失脚本。
- 错误处理：先确认原唯一Editor idle/nonPlay/非编译；状态/身份不符则保存所见并停止，不调用.Instance、不创建/重载/保存/修复；不得为查询成功改变用户现场。
- 最窄验证：枚举包含inactive的真实组件、GlobalObjectId与SerializedObject只读字段；前后scene dirty/root/实例集合相同；不调用ApplyModifiedProperties/SetDirty/播放/Destroy。若工具不支持被动字段读取则如实返回，不改脚本绕过。
- 验收声明只到“当前组件与未变化场景配置一致”，不倒推旧恢复瞬间，不等同H10整项或平台验收。本任务**尚未执行**此后继。

音频subset路线若继续，需要先补具体root/alias/声音来源证明；其后继写域和验证必须由讨论会话独立裁决。本轮到证据矩阵与报告为止，没有自动派发或实施。

## 7. 实际离线命令与检查

实际使用rg --files定位、rg -n精确调用/声明、允许文件的窄行读取；离线Python只读取既有JSON/哈希，新增当前目录报告与清单。可复算配方为 [offline-review-recipe.txt](offline-review-recipe.txt)，实际调用：

~~~powershell
@'
from pathlib import Path
exec(Path('artifacts/diagnostics/NTSD-H10-AUDIO-PRODUCER-COVERAGE-REVIEW-20261011/offline-review-recipe.txt').read_text(encoding='utf-8'))
'@ | python -
~~~

配方为审计文本，不新增项目执行脚本，内部CreateNew防止覆盖输出；已执行一次并完成58行/1000cue比对。探索中几次旧路径定位未命中、旧JSON键名猜错导致KeyError，均为只读命令失败，随后按实际路径/keys纠正；不是Unity/编译/测试失败，也未对源文件进行“修复”。上述sidecar越界检索另记，不掩作普通路径错误。

本轮没有C# diff，所以不把旧ChangeLedger PASSED1421冒充新代码验收，也不为纯artifact任务新增伪代码Change。最终审计比较5540个源码/文档/既有证据指纹、旧8项生产/诊断/程序集指纹、HEAD和任务外Git状态；结果见final-audit.json。Unity、focused、Play、正式性能和设备均为 **NOT_RUN_THIS_TASK**。

