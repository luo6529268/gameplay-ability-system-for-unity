# 2026-10-06 战斗音效修复与定向验收

本批已实施音效资源、统一解析/多声预热及共享声音分支修复，并通过原 Unity Battle Scene 的定向播放验证。资源/解析入口已验证；事件实现有 Unity 运行与当前 Core 对照证据，但不能宣称所有音效事件已与正式336B44逐项完全一致。kind9 的当前Core差异保持 UNKNOWN / SOURCE-INSUFFICIENT，本批未修改该候选。

旧 NTSD28-UNITY-BATTLE-REALIGNMENT-001 维持 USER_ACCEPTED_SCOPED_CLOSURE。本任务为 NTSD28-336B44-BATTLE-AUDIO-ALIGNMENT-20261006，已实施范围为 SCOPED_AUDIO_RUNTIME_PASS；正式事件确认的限制单独保留，不重新启动旧Q01-Q12/全角色矩阵。

## 已修复内容

- 用户指定03_声音资源981个WAV与正式resources/runtime/vfs的981个WAV逐文件SHA一致。按正式战斗DAT/全局sound.dat及项目现有模式Asset引用部署978个：966新增，原12保持；新增966 WAV.meta和53目录meta。无删除、音频重编码、BGM或非战斗三项菜单声音导入。
- NTSDSoundPlayer统一将战斗SFX_ddd解析为正式data/ddd.wav；路径大小写兼容，别名和直接路径复用同一PreparedSoundCue，预热后运行时不新建cue。普通非战斗播放入口保留。
- CharacterAnimtorManager按实际FrameSounds前20条收集预热，避免只预热最后一条；原武器三类DAT声音字段与项目KO配置继续接入同一入口。技能释放、帧语音、移动/落地等由DAT声明的帧声音统一得到资源支持，无角色硬编码修补。
- 普通角色受击按实际反应计数Fall==80选择001/006，在受击者位置排队。effect1保留base→032/033→base顺序和重复事件；type3攻击者DAT broken前缀使用攻击者位置。
- 普通命中的后效声音补齐：effect3/30实际后置action200→065，effect2/20/21/22实际action203→068，effect23→068；OID100已有039保持在base之前。只调整声音产生/顺序，不改动作决策或伤害写入。
- reduced hit使用命中前状态7/70/75的DAT drop，否则DAT hit，空值回退002；type3攻击者使用其broken字段，空值保持静音。移除旧OID相关固定017选音，使用共享规则；目标位置不再误取攻击者位置。
- 非角色反弹落地的三个既有分支改用固定011，不再错误使用DAT drop；所有落地阈值/速度/状态不变。
- 奔跑起跳及crouch左右起跳移除三次额外017；原DAT帧a7/012及动作/速度/方向/位移均保留。
- 共享事件的ECS shadow/data-oriented预测同步声音选择、位置及顺序，保持已有战斗状态写入。

## 实际验证

1. 原Editor Unity 2022.3.62f3 / PID19040，通过原MCP TCP6401 Refresh/编译与具名EditMode测试，无第二Editor、新Unity项目或computer-use。
2. audio-focused-green-final-result.json 25/25；post-audio-focused-final-result.json16/16；post-audio-ecs-final-result.json6/6。重叠扣除后39个具名case；dash三项RED均仅多一个cue，删除三行后dash-green-result-final.json3/3，累积42不同case通过。未跑全部8941项测试或全角色组合。
3. 978个WAV由UnityWebRequest实际加载，samples/channels/frequency与RIFF逐个一致；8种代表cue实际增加播放计数。128次已预热别名调用测得GC分配0，仅是热路径分配证据，不声称128个同时播放声音均被接纳。
4. 原Scene battle-audio-scene-03.json PASS / DONE，完整生产Driver global tick5→69、64个逻辑tick样本：自然鸣人组合输入55tick，global39/frame326的data/078.wav实际queued/assigned/playing；受控180/p3和213/a7各一tick实际播放；攻击碰撞global69李HP500→480，SFX_001在受击者X548实际assigned/playing，单次pooledPlayDelta1。受控帧不称自然受击/自然冲刺。
5. Scene03 64样本未预热cue拒绝、加载失败、缺文件跳过、voice溢出、发布拒绝计数均0；目录sealed=true，预热key2002、voice64。正常逻辑间隔0.033，现有比例1.536384096保持。
6. 关闭完成至RuntimeMapCleared，World解绑；logic object/runtime slot/borrower均0、pool quiesced。逻辑关闭后已有一次性声音仍4个在播放；退出Play后AudioSource/playing均0。因此只证明本次Play退出清理，不声称普通Scene卸载会立即停止全部一次性声音。
7. 原Scene01 startup180秒超时，声音尚未开始，保留FAIL；Scene02由用户明确确认其或其他任务停止，仍未进tick，保留中断FAIL。这两次不当作声音播放失败或PASS。Scene03允许既有全量视觉预热完成后才声音预热，并最终通过。
8. 当前Core受控25案例（普通/后效/reduced/落地22＋dash1＋kind9候选2）零anomaly，编译/运行exit0；诊断有enum未覆盖和narrowing警告。源树限定证据，没有正式根EXE的声音事件捕获。
9. final-content-guard.json：681保护项零差异；978部署WAV零差异；原12 WAV/.meta24项与开始Git基线完全一致；14原字节备份稳定；正式根EXE仍336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3。未改DAT、Scene、Input、GameConfig、模式Asset或非战斗框架。
10. Tools/Validate-ChangeLedger.ps1已实际运行通过（8个本轮剩余治理脚本diff全部覆盖）；git diff --check通过。与actual-before原字节比较已存production-diff-vs-actual-before.json，避免外部b035dc59提交包含本轮cue/资源后仅看HEAD漏报。

## 权威及剩余边界

正式根EXE是唯一游戏规则权威，哈希未变。音频资源身份和正式DAT帧/字段引用已确认。当前README_SOURCE.md声明源码为持续开发候选，与根发行EXE不能视为逐字节对应；当前Core输出只能限定为当前源码证据。

kind9当前Core的type1目标拒绝/type3目标转移均audio_events=[]，Unity对应分支有额外effect/broken音。这是当前代码实现之间的已观察差异，尚不是正式336B44的已确认差异。缺少可审计的两个早期kind9快照身份/路径及正式事件捕获；保留UNKNOWN / SOURCE-INSUFFICIENT，未改Unity kind9或上游玩法。

此前记录中的“两个早期音频hunk一致”不能由本批现存kind9原件独立证明；本报告不以该表述证明正式根源码身份，也不将当前Core trace提升为正式根事件对照。事件Change保持RUNTIME_PENDING，含SCOPED_AUDIO_RUNTIME_PASS；cue/resource Change为VERIFIED，仅限自己的实际验收范围。

未运行整体BattleRuntimeSelfCheck、正式根全音频同初态回放、设备音频输出/人工听感、全部角色/操作组合或Scene-unload专项。所运行42项聚焦测试和原Scene实际播放不能替代这些未运行证据，但也不据此展开新的全角色回归任务。

## 恢复与审计入口

- docs/ai/TASKS/NTSD28-336B44-BATTLE-AUDIO-ALIGNMENT-20261006.md
- docs/ai/CHANGE-RECORDS/NTSD28-336B44-BATTLE-AUDIO-CUE-RESOLVER-001.md
- docs/ai/CHANGE-RECORDS/NTSD28-336B44-BATTLE-AUDIO-EVENT-RULES-001.md
- docs/ai/FILE-OPERATIONS/NTSD28-336B44-BATTLE-AUDIO-ALIGNMENT-20261006-PREPARE
- docs/ai/FILE-OPERATIONS/NTSD28-336B44-BATTLE-AUDIO-ALIGNMENT-20261006-WAV-STAGE

任何恢复仍须保护并发修改和记录操作；本批未执行git restore/reset/clean/删除/stage/commit/push。
