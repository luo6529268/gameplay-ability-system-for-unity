# H10 两进程三轮的具体用例候选（未冻结执行）

Task NTSD-H10-WINDOWS-PLAYER-PROCESS-BUDGET-20261011 / Change001，PLANNED。仍是一份build、普通P1和combat1000 P2两进程；每进程同PID三轮=120warm+180smoke、120warm+1800formal、120warm+1800formal。未加第三进程，不宣称此候选已通过。

## 有据代表及其限制

| 代表 | 原始证据 | 有据输入/观察 | 本包适用性 |
|---|---|---|---|
| 鸣人OID2，持有螺旋丸OID434，053连续cue | `NTSD28-336B44-RASENGAN-COMMON-REGRESSION-20261006/original-common-fix-01/REPORT.md`，`NTSD28-336B44-Q10-ORASENGAN-078-NATURAL-VOICE-001/REPORT.md` | defend两tick→right两tick→jump两tick，之后中性；旧场景053实发44次、同cue单voice、无拒绝。后续fresh Jump另有078，但本候选不为多一个cue改变时机。 | 持续与同cue重触发代表；旧87tick含诊断坐标/对手clone，不能把其44次和抓取tick作为正常Menu的固定事件数。新窗必须实际见434和多个不同tick的053、同身份voice复用，不人工spawn或直接enqueue。 |
| 我爱罗OID16，j4→043 | `NTSD28-336B44-Q10-GAARA-J4-043-NATURAL-VOICE-001/REPORT.md` | defend两tick→right两tick→attack两tick；旧相对13 action243/j4、15 action244/043，两次真实voice。 | 不同cue顺序代表。地图坐标与LocalFreeRun输入采样不同，不预称新窗也在13/15；保留实际已消费输入/事件tick与cue顺序。 |
| 君麻吕OID507，j1 stereo | `NTSD28-336B44-Q10-KIM-J1-NATURAL-REACH-001/REPORT.md`、`...Q10-KIM-NATURAL-VOICE-PCM-001/REPORT.md` | defend两tick→depth-up两tick→jump两tick；旧tick6 action311/PP250，`c/kim/w/j1.wav`，2ch/80454samples/48000Hz；已有软件PCM3:1限定证据。 | 满HP可达的原生双声道代表，避免Sakura tra所需HP100夹具。本包实际播放源/声道身份即可；不把设备听感或新位置声像纳入已通过。 |

依据全catalog旧inventory：当前330定义/1000cue，978formal与72generic实账本；多角色/技能代表不是削减加载集合依据。保守资源最坏集合使用保持完整回退的实际全catalog prepared+账本，而非声称这三个角色是全游戏最重技能。旧63scope同cue/多cue/连续tick洪峰与PCM边界按原hash/平台/范围复用，不再机械复演旧Editor组。

## 普通P1候选安排

为避免需要新P2/P3按键合同，拟每轮只投递P1已有公开物理键入口，其他两人是同队、中性输入的人类槽，不设AI、不改位置/HP/PP/动作，不复制战斗对象。真实slot inputId/selectedTeam/MatchConfig全保存。

| round | UI槽顺序（皆人类/同一合法Team，0电脑） | P1一次性按键计划 | 窗口与必需观察 |
|---|---|---|---|
| O1 smoke | Naruto2 / Gaara16 / Kim507 | warm120之后，L(defend)2个已观察逻辑tick→D(right)2→K(jump)2→中性至窗末 | sample180；实际434/053跨tick重触发、无unprepared reject/drop、声音未suppressed；实际多角色roster通过。 |
| O2 formal | Gaara16 / Naruto2 / Kim507 | warm120后 L2→D2→J(attack)2→中性 | sample1800；实际j4早于043、两cue真实voice与无拒绝；余窗完整计内存，不截短。 |
| O3 formal | Kim507 / Naruto2 / Gaara16 | warm120后 L2→W(up)2→K2→中性 | sample1800；实际j1两声道voice、同generic跨局身份、旧owned清零后冷重入。 |

三轮都经公开ResetAll再SetSeed。建议ordinary seed改用有据682973786（原候选1314149188只保留给原千人组）；这是待主审锁定的用例参数，非生产默认变更。正常LocalFreeRun/33ms保持。真实地图由Menu原配置产生且必须实际prepared/非空id/真实boundary；本包不强改Scene/资源或借用native stage23替换项目地图。

R5A须先证明三槽选择/同队操作均合法，不能在失败后改为单角色并称多角色通过。R5B须先明确InputSystem的正常loop入口。以上“2tick”是目标，不通过manual StepOneTick、暂停/改驱动、InputSystem.Update或动作赋值保证它；新实现必须记录实际投递与消费边界。若现有入口无法无侵入保证输入或事件采集，则准确报告缺口，不能按旧精确tick造PASS。

一次输入序列、一个已声明观察窗，未触发即首差FAIL；不自适应重发键组合，不尝试不同技能直到有声音。同cue重触发需至少两个不同tick的实际生产053和同identity复用证据；j4/043按顺序各至少一次；j1至少一次且正式源身份/2ch一致。次数是新诊断最低覆盖，不改变生产声音规则。同tick汇总/64voice边界由未受影响旧精确测试复用，不声称本三轮自然动作动态穷尽它们。

## combat1000 P2

维持原harness选择排序首个可用type0（实际应报告所选OID，不硬称Sakura），1000实体AI、seed1314149188、dispatch、非simulationOnly、skipLate=false。三轮仅原120+180/1800/1800。固有Manual/MobileExtended1050/帧checksum=false明确；它是数量与实际运行请求覆盖，不是多角色或普通Host性能证明。start空world/pool/slots与真实map资格后才启动，禁止删角色凑空。

## 关闭与不增加验收门

“全catalog静态生产者＋运行请求覆盖”并不要求每个producer每个角色每个技能动态穷举。现有全回退对已加载定义/19builtin/模式KO可复用；实际新窗所有请求应在其内，无unknown/reject，不裁剪cue。外部/变量producer未声明条件如实列出，不能泛化为整套未来架构重审。

每轮同一公开App实际map11关闭；runner内部RecreateWorld提前audio shutdown单列。三轮包含持续播放后close、单/多cue以及native stereo；pending取消迟到/自然Destroy已有resume03证据复用，除非新probe改变对应所有权。新Player不能靠手动ClearCache、Destroy旧对象或GC.Collect得到预算。

本方案尚缺R5A/R5B入口证明、R4完整部署及采样质量/安全恢复独立裁决，未获执行授权。具体读请求见NEW-READ-DEPENDENCIES-01.md。
