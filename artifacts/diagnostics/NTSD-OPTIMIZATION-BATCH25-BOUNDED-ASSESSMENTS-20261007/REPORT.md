# 第25批有限准入评估最终报告
状态：SCOPED_ADMISSION_ASSESSMENTS_COMPLETE；H06 NO_DEFAULT_PROMOTION，M13 NO_IMPLEMENTATION_THIS_PHASE，M14 DEPENDENCY_NOT_READY。不是三个优化已实施，也不关闭父项。

## 证据与范围
原Unity2022.3.62f3/PID19040，Menu非Play EditMode；固定job bce31e5a9584493dbc8e0e75efd99c3f 实际18/18 Passed、0fail/skipped、214.689s。9321为发现数，不是执行数。8新case+10既有case，参见focused-tests.json；输出只去掉长日志，断言/状态不删。raw run-01共33工作负载报告（早期Record写31是计数笔误，现更正），成本每request两份固定采样，不择优重跑。编译已由这些实际执行证明，不把连接/Console过滤当编译验收。

## H06：四类collector准入
| collector形状（1000 participant） | BruteForce ms | Direct/Sweep ms | tree ms | optimized pair |
|---|---:|---:|---:|---:|
| Dispersed | 955.314 / 1089.610 | 19.155 / 21.599 | 19.709 / 18.042 | 500 |
| Combat | 1137.745 / 1247.251 | 23.450 / 23.600 | 22.995 / 21.996 | 980 |
| Concentrated | 2105.633 / 2393.483 | 1094.628 / 1381.464 | 1476.504 / 1386.085 | 261150 |
| OPointBurst-shaped | 964.942 / 1044.530 | 20.113 / 24.845 | 25.204 / 23.292 | 975 |

两列中的两个数字分别为固定replicate1/2均值。成本scope仅CaptureCollisionFrameSnapshotsAll→CollectCollisionCandidatesAll→EndCollisionCandidateConsumption，fixture、反射、NUnit结果复制/逐字段比较不计入。参与者无AI/tick/input，不是H07真实1000AI或全逻辑预算。
四形状的两个候选与BruteForce候选序列、target slot/generation handle、hit字段/itr身份和RNG逐项相同；无aborted/fallback participant。既有zero/one/many ITR、generation reuse/exception隔离、input validation、occupancy mutation/brute RNG fallback实际通过。
Direct家族本地ForceDirect+现有8192规则，四形状实际sweepTicks=1；treeTicks=1为另家族，原production空值仍BruteForce、阈值262144/8192不改。
optimized pair只是该路径自己的broadphase diagnostic，BruteForce该字段0不表示没比较/没候选，不能把0当baseline成本或漏检测。
Dispersed/Combat/OPointBurst-shaped存在明显局部collector收益；Concentrated实际261150 pair、耗时仍1.1–1.5秒，不达33ms，即使没有fallback也会退化。不能用少pair或优化开启名义判准入。
OPointBurst-shaped仅760角色+240specialAttack既有fixture的空间形状，不是实际OPoint生成、同tick可见或生命周期链。完整hit消费/OPoint/stats/checksum、双RNG/native336与完整LogicP95/设备证据未闭合。裁定：有限准入评估已完成，但NO_DEFAULT_PROMOTION / AUTHORITY_AND_FULL_TICK_EVIDENCE_PENDING。保留现有生产默认，不重写树、不另开全量对齐campaign。

## M13：声音成本与采用判断
同tick1000事件，unique1/8/128当前Ordinal扫描比较分别1999/12464/182996。实际PresentSounds两均值分别0.275/0.282ms、2.098/2.067ms、21.641/21.603ms。使用正式路径身份、fixture合成静音clip，不是WAV解码/真实混战或端到端音频耗时。
playCount delta每16sample分别16/128/2048；reject0，voice capacity64。128cue两rep各voiceDropDelta2048，是现有64 voice上限/替换语义的诊断计数，不代表Core声音事件丢失，不更改此合同。
既有同tick/跨tick、独立cue、voice replacement、销毁owner四项通过。较多cue下成本明显升高，但含mix/voice上限替换，不能将总时长全部归因字符串扫描。H07唯一真实Dispersed smoke dispatch/suppress均0，Combat无有效窗，当前没有真实多cue热点证据。
裁定：NO_IMPLEMENTATION_THIS_PHASE，不自动优化。只保留一个未来最小候选：预分配cue-id/首次出现顺序表，先保证Ordinal、tick、整数mix/重触发/voice上限等价，再由用户选后继实施；不新增音频系统/资产/本阶段算法。

## M14：校验成本与immutable/lease前置
当前真实Candidate.Images=906、全部存在；每次Assert必读图片输入长度合计29939600 bytes（约28.55MiB），是可证明图片hash逻辑读取下界，排除catalog/DAT/WORDS/SPARK/模式/公共图等，不是物理磁盘读字节或cache命中测量。
一次Capture（自身含Assert）12000.889ms；两Assert3829.933/3656.405ms。当前Editor/cache-state成本，包含catalog/解析/转换/日志/hash，不是纯IO、冷启动收益或最大startup瓶颈证明。
本次重扫：LoganVisualContentCandidate.cs:321/358/363-404/444-452，CharacterAnimtorManager.cs:116/213/1463/1689。Candidate是输入定义快照，File.OpenRead的using在每个hash后关闭，不持有跨阶段资源读lease；configuredContentGeneration是取消/owner状态，不证明磁盘字节不可变。四处调用是不同代码入口，不能机械宣称每次startup必调用四遍或省掉三遍。
裁定：DEPENDENCY_NOT_READY / KEEP_ALL_GUARDS。重复成本值得后续受控优化，但当前可变文件源/不可变包身份/完整输入Manifest及跨阶段read lease未闭合；保留全部stale-input/hash/catalog守卫，不缓存、不以path/mtime/size替代身份，不实现H01整体链。

## 必要GC证据正对照与更正
真实Capture创建大量managed对象却API报告0，引发唯一必要正对照，不重测原33报告。job f9b933f265b34e8b89878611bf440858 实际1/1通过（测试只要求准确观察，不要求counter有响应）；Allocation-Counter-Control.json记录明确存活1MiB数组、reported allocatedBytes0、allocationCounterResponded=false。
结论：当前Editor/该API在该正对照未具备可靠0GC证据，三评估不以0B宣称零分配。不能推断所有运行环境同样无效；历史同API没有保存正对照，亦不能继续把旧0B作为已闭合0GC合同。
第23批两桥/实际camera的容量growth0、draw录制=执行、显示/lease/关闭证据保持，原raw/assert事实不删，但H11完整热路径0GC验收撤回到EVIDENCE_PENDING / RUNTIME_PENDING。M03本阶段显示/成本/GameConfig Foot方向限定通过，不认证0GC。此为新鲜测量反例的证据修正，不是渲染发生实际分配或规则回归的证明；不借机全面重跑历史/改production。
measurement-summary.json早期allocationEvidenceStatus保留为事前待校准快照，最终以counter原件和本节为准。

## 本阶段停点
最终保护：164非写域文件hash不变、14现有文档副本＋1本批新探针控制前副本hash保持、HEAD同8107196；Menu/Battle双Scene同，原Menu8roots clean/idle/nonPlay，实际19/19。默认validator1321records/7 C#覆盖/0errors，4281历史warning独立保留；git diff --check退出0，仅行尾提示。没有claim整个项目warning0。final-validation是该具体时点的证据。
新批4/8已用；M03限定实施交付+H06/M13/M14三评估完成=4/6。H11有效0GC采样未闭合；H07仍PARTIAL/3of3到限，仅120+180 Dispersed无效certificate窗口、Combat配置失败/四formal未跑。Goal未达成，不complete，不扩展剩余28父项。
下一需明确选择一个独立有界后继窗口：可靠GC采样证据 + H07 suite局部生命周期恢复/完整固定Windows报告。H07超出旧3/3不能自动续修；不默认broadphase、不解冻EXT1/Mono/ATLAS。当前没有SAFE_READY的既定完整报告入口，不补可选矩阵填充进度。
无production代码改变，本批仅新Editor诊断/进度；Scene/资源/原dirty保护最终见final-validation.json。11阶段runtime未在本批运行（EditMode不绑定生产World），各独立collector对象/slot cleanup已断言0，音频fixture finally Dispose恢复。不是新Play/runtime验收。
