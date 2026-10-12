# PRE-FLIGHT-01 review-03：用例、精确身份与证据容量澄清

状态 **PLANNED / 只读供证与方案补齐**。同Task `NTSD-H10-WINDOWS-PLAYER-PROCESS-BUDGET-20261011` / Change `NTSD-H10-WINDOWS-PLAYER-PROCESS-BUDGET-001`。本件更正此前同队、跨进程对象身份、大小写折叠和4096行覆盖措辞；旧原件全部保留。本件没有改变生产规则、H10预算或验收合同；下述千人有限明细域是供主审冻结的明确候选，不能自行当作已批准执行方案。第四processor域仍待直接人类确认，完整实施派发尚未到达，无脚本/Settings/build/Player授权。

## 1. 公开队伍和模式源

主会话具名供证 `SelectRoleItem.GetFinalTeam`588–597：有config时最后选项为GameConfig.TeamIndependent，其余index+1；无config分支也是index+1。当前GameConfig.asset五个TeamOptions，GameConfig.cs:229常量-1。因此合法映射为index0/1/2/3→team1/2/3/4，index4→-1，不从Sprite GUID猜标签。

普通O1/O2/O3仍仅OID2/16/507三个人类槽、P1轮换另两人中性、0电脑、一次组合；固定槽1/2/3调用公开SelectTeam索引0/1/2，预期team1/2/3。每轮实际Confirmed、inputId互异并匹配PlayerId、实际team、存活阵营数及完整Running窗都要验证。已有C009只说明避免单存活阵营的理由，不证明新Player长窗；若自然终局或提前退出，保留实际状态并停止，不改HP/位置/结算/规则或再按技能。

执行会话本轮直接核 `NTSD_Menu.unity`3411–3440：m_Script GUID `b9e532a34253ab24b8de2ae799b933d3`与CharacterSelectionController.cs.meta一致，currentGameMode为gameModeId=0 / battleGameModeId=1。Scene SHA `f2ee25d075548b78380be817209152047b919c4c15da8743d75d2e0c165cbea1`。这给出真实序列化来源，不是从字段default1猜VS。序列化playerSlots只有第一个非null，不能将本静态检查称为三槽UI运行验证。

主审另具名供证：HandleOptionConfirmed index0→OnVSMode；OnVSMode仅ShowSelectCharacter并禁自身；MenuUIController.ShowSelectCharacter仅HideAll和激活selectCharacter，没有写mode。执行会话只rg定位CC的84字段、463/599配置赋值、613 setter；没有宣称全工程不存在其他setter调用。GameModeConfig是普通Serializable两字段对象，无Unity对象GUID/InstanceID；SHA `5d877a91579c10088ad7004f541eccd210b65061f0c6b151c9234dc48efd3db6`。

冷运行资格须捕获实际VS选择来源/场景build身份/currentGameMode两个值，要求与当前已核0/1及P1成功模板一致；不写mode字段。P2同进程clone沿用本轮真实currentGameMode引用，跨进程只比较值和源/build，不比较实例。P2仍不按Fight/StartBattle，使用已审空players新MatchConfig→App.Set/Load路线。静态源证据不会替代实际准备/空World/真实地图/三次关闭门。不再请求Results writer正文。

## 2. 已消费输入的真实值副本

主审按Client已声明UPM引用具名核kernel `Runtime/Abstractions/FrameInputSet.cs`22–39/51–67，SHA `e954a6cc2359b6e2522bb65965364372bed6732745fc82a7f7c286f863cfd4a0`。SimulationPlayerInput readonly struct四字段为PlayerSlot、Buttons、PressedButtons、ReleasedButtons；FrameInputSet是class，TickIndex/Players为virtual属性，Players为IReadOnlyList。仅使用这些数据合同，不改kernel或展开Server业务。

候选记录每次实际可见包的TickIndex、实际Players.Count及按原次序的每槽四值。8192个tick-observation槽/进程，每槽最多8个玩家值，冷构造固定8192×8值存储及实际count；不是8192条单玩家记录。三轮目标4140tick，仍须计实际额外观察，超限则证据FAIL。观察不到的中间tick明确UNKNOWN，不保存FrameInputSet/Players引用作历史、不重建缺tick、不用设备状态替代消费值。实际数组/属性读取的无分配资格仍需新版本测试，readonly struct声明不等于整观察路径0GC。

input转换仍32/进程、每个键状态只投递一次；下一状态最早条件为观察CurrentTick经过2，不承诺恰好两个消费tick。若可见历史不足确认关键组合消费，INPUT_OBSERVATION_INCONCLUSIVE，不增加输入重试/窗口/进程。

## 3. 三种数量和Ordinal观察索引

来源旧真实C# inventory SHA `db1d30c6fa4fbf43341797289706e644c77fe14bbc9bf2ea82ff93941557c1f9`；执行审计 `RAW-KEY-CAPACITY-AUDIT.json` SHA `2132364f7455231a93c9bd6fa9d8e09068ff5f741f3999f8e3ba6da6eabffcde`，主审独立逐项重算集合/route/flag无差。

| 数量 | 准确含义 | 不能推导 |
|---|---|---|
| 1000 | 不同原始raw cue，已包含19 builtin和2 mode；perDefinition+mode+builtin集合与routes完全相等 | 不是1000独立AudioClip或每轮发生1000次声音 |
| 978+72=1050 | 旧实测formal与generic不同AudioClip对象；合计228918567B | 不能把1050当prepared raw key个数 |
| 1968 | 全部原raw加缺失的正反斜杠观察别名 | 不是生产注册表新增或sealed资格放宽 |

19 builtin中18个是SFX_*字符串，另一个是`data\016.wav`。不同raw可共享资源：978不同battleResourceKey、19组共享资源；保持原raw/builtin/DAT身份，不因文件相同合并生产者或计数。

V3分析的lookupKey做了小写折叠，保留为历史分析，不用于实施。新的 `RAW-KEY-RESOLUTION-V4-ORDINAL.json` SHA `140a0d903de87d71b4f8a2fec6fa871d2583622c7dd4f81af4f047044cb420bb`使用精确大小写Ordinal：先放全部1000原raw，再只填缺失且唯一归属的slash alias。7对原raw产生14个冲突alias键，全部已是原raw，因此精确原raw优先，未拥有的多归属alias为0；原raw身份差0。保留SFX_001，不人为接受sfx_001。counter1024余24；index2048余80，依据只到本旧冻结集合。

若新的实际prepared raw集合变化、超限或存在尚未拥有的多归属alias，冷资格拒绝，不裁cue/扩容/猜归属。Player须记录其真实prepared raw集合及资源绑定与源的核对，不能直接把旧inventory当新运行原件。观察alias命中只定位候选元数据；事件仍原字符串forward，是否被实际生产player接受以其真实结果/计数为准，不能以观察命中证明生产prepared会员资格。热观察只TryGetValue，不Add/Trim/Replace/折叠，不注册生产alias或改变sealed规则。

## 4. 4096行的明确域；全窗计数与有限明细分开

4096是诊断预分配容量，现有证据没有给出每实体/每tick声音最大数。旧普通44 events/87tick及Gaara/Kim少量目标声只支持代表用例选择，不能变成千人最大值或4096必不满的证明。任何音频请求均继续完整同步forward一次，不能为记录预算删声音、换顺序或拆批。

拟固定以下域，**待主审冻结再实施**：

| 运行 | 对所有原事件的全窗观察 | 明细行域与限额 | 能支持的结论 |
|---|---|---|---|
| 普通O1–O3 | 每batch遍历每个事件；1024 raw counters、total/membership/alias-hit计数、按原顺序的滚动摘要；实际player拒绝/drop等现有计数分别记录 | 四个预定目标cue（053、gaa/j4、043、kim/j1）全窗事件明细最多4096/轮。每个batch forward前后64声源startSequence值副本，保存可观察到的目标clip实际新启动槽变化最多4096/轮 | 全窗实际请求属于已观察集合；目标自然事件/顺序及可见实际声源证据。摘要不等于无碰撞完整trace；不声称每次请求均被逐声源观察 |
| 千人C1–C3 | 同样逐原事件更新全窗所有raw counters/membership/顺序摘要；不限定只四cue，不假设每entity一次；实际player计数全窗独立记录 | 预先只取tee安装后的**第一个非空原batch**的前64个event值副本，标明完整batchCount、sampleCount和未采明细数；保存该同一batch forward后最多64个改变startSequence的声源槽实际状态。现有4096容量仍固定，但此域最大64，无事后挑选/追加batch | 千人全窗实际请求集合/计数及一个明确batch的实际播放代表。该64前缀不是全量明细，也不是稳态代表；记录实际tick/phase。若批内槽被多次复用，只能观察最终一次，明确不可见中间启动 |

两类都不能把player PlayCount增量直接等同实际听到或逐cue完整播放。普通代表声须匹配本次改变序号的真实source/clip和在播/输出证据；只看旧同clip source仍禁止。千人样本没有可确认新启动/实际播放，则该代表证据未完成，不换另一个batch试到PASS。此候选不改变全catalog静态+实际请求覆盖门；若主审认为原合同需要千人每事件/每启动完整行，则在实施前明示未解决并由主会话决定，不能静默以样本替代。

1000局仍是原harness固定单角色/Manual配置，不冒称多角色普通Host或H07证书。普通三局承担2/16/507持续、多cue及stereo代表；全目录静态330定义与1000raw/member闭包另列，不要求每个角色/技能动态穷举，也不宣称已穷举。

失败分类固定：

- `DIAGNOSTIC_EVIDENCE_CAPACITY_EXCEEDED`：所承诺完整记录的域溢出，整轮不可验收；不是生产dropped sound、生产queue容量或关闭回归。计数饱和也记自身容量失败，不wrap。
- `OBSERVATION_KEY_UNRESOLVED`：索引外原key，第一条就使闭包证据未满足；固定32条首见取证上限不决定是否失败。另记录实际player是否拒绝，不能仅凭观察未知宣称生产丢音。
- `OBSERVATION_EXCEPTION`/`INPUT_OBSERVATION_INCONCLUSIVE`：夹具证据失败；不抛回Driver破坏dispatch/清单清理。生产forward异常原样传播且另分类。
- 千人事前限定的64前缀未保存后续明细是已声明采样域，不隐去total、batchCount、sampleCount，不将完整域真实溢出伪装为采样。

每种情况下完整原列表仍被captured生产sink同步forward恰好一次，顺序不变。观察失败只设固定状态，原forward正常则tee正常返回，由外层安全边界停止输入、还原sink、走公开App关闭；不扩容、不新轮/新组/新进程、不重跑寻找PASS。GC资格必须包含实际tee、正控/空控、unknown/满容量/观测异常、生产异常和全载荷exactly-once；旧63scope不能替代新tee资格。

## 5. owner、失效与关闭位置

所有新表/行/counters/输入副本由本probe持有，冷构造、每轮或每进程固定容量，不成为生产manager/cache。原64 voice/source/identity数组仅借读，历史保存自己的值副本。主审已允许冷只读Driver277/292与NTSDSoundPlayer78/82/83精确字段，禁止写production数组/owner，热路径不反射。

安装前必须确认原override为null或同一captured音频owner；未知observer直接拒绝，不抢占。ordinary在ready之后warm之前安装，采样结束、公开关闭前按同Driver/owner/当前tee身份恢复；combat在StartRun同步配置后首Update前安装，harness终态可能已清audio，只对仍符合精确owner身份的本tee恢复，之后实际App map stage11。不得因为身份变化覆盖他人状态。

stage1仍由原lifecycle禁止新tick/输入，probe停止自己的输入投递；不添加新的关闭hook。ordinary在App关闭前按上述身份检查撤tee；combat原harness同步关闭可能已到stage5音频释放，probe仅在既有terminal控制返回后核身份并还原，不能声称已在stage5之前拦截。原stage1停止后不再有正常tick向tee派发；如实际仍有派发/owner漂移则资格失败，不靠插入新hook调整顺序。原pending声音/publication消费规则保持。后续stage6–10/真正App地图11沿既有公开合同；只等待拥有的clip引用销毁证据，不自行销毁生产音频。值快照直到冷导出保留然后丢自身引用；取消/异常也同一幂等清理。Process级8192输入/256phase在三轮导出后失效；轮级cue表/4096行/32unknown/64声音快照在该轮证据导出及引用检查后失效。未知owner或无法安全恢复为失败，不能写虚假恢复PASS。

## 6. 仍未完成及最小下一步

raw UI直接依赖已到Candidate输入绑定；下一只有 `RAW-UI-NEXT-READ-REQUEST.md` 表中的具名范围。旧known-domain清单2752条尚非完整raw UI部署证明。postprocessor最小兼容提案及SHA不变，第四文件写域仍由主会话处理人类确认；本执行会话不重复提问。读证闭合、此容量/样本候选获主审冻结并收到完整实施派发前，不创建脚本、不切backend、不build、不启动Player/sampler或重跑旧Editor组。

本轮复用未受改动影响的resume-03音频门和旧目录/PCM/生命周期证据，只到各自原scope。H10父项OPEN，预算320MiB对象/2GiB稳态Private/3GiB过渡采样峰不变；没有H07/Android认证、目标硬件认证或其它优化推进。
