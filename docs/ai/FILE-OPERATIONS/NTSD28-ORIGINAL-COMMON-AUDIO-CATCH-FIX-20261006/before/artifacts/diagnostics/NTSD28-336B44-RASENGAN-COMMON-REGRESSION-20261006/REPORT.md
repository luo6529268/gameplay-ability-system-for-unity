# NTSD28-336B44-RASENGAN-COMMON-REGRESSION-20261006
Status: IN_PROGRESS
User report: sustained Rasengan SFX and enemy clone first-hit termination.
Initial static facts: repeated053 events originate authored nar preparation and held434 spawning518/155→156. Unity pooled player allocates another free voice for repeated cue. Formal backend semantics and common clone hit response pending.
No production edits at task start. Original root336B44 frozen; candidate audio strategy is not authority.

## 2026-10-06 实测结果与边界更正

本批尚未修复用户报告，状态 **REFERENCE_VERSION_PENDING**；不能将下面诊断通过称为两个问题已修复。

用户已确认：声音更吵、重叠/杂音；目标为敌方鸣人影分身，指手上的螺旋丸保留。当前正式根EXE fresh SHA336B44，旧总目标维持用户接受的范围收尾。当前C++ AVRepair候选不能覆盖正式行为。

| 检查 | 实际结果 | 证据与限制 |
|---|---|---|
| 直接冲刺catch普通影分身33与鸣人本体 | 原Editor两例逐tick的actor existence/OID/action/state/HP符合正式 | charge-clone-near/body-near，16tick；无原Battle按键/像素验收 |
| 自然241准备→254持球声音 | 64tick总47次053，逐tick次数相同 | prepared-hold-sound-far，完全Driver；不代表最终混音波形 |
| 持球后实际Attack29/30命中33/本体 | source/fixed-projection四例4/4PASS | 64tick；正式和Unity都在clone死后继续held434/action21；本体则进入后继并销毁434 |
| 053的Unity实际解码 | 5746采样、11025Hz、monoPCM8，全样本 (byte-128)/128 maxError=0 | 单项1/1；两voice volume0.7905694/pan-0.8/pitch1/loopfalse；该临时出口mixer=null，未测生产最终混音/设备 |
| 正式敌方鸣人实际召分身 | 敌方272经495/497→274→518→33/307生成owned clones；P1等待再实际Attack55/56；63tickcatch33，66tick300，67tick该clone销毁，434仍held/action21 | held-attack-owned-ready-clone，96tick正式根实际执行；63tick P1也实际受击、HP480，因此不是纯catch单事件；尚未与Unity的这个AI场景逐字段对照 |
| 编辑器和保护 | 原saved Battle clean/root11；681保护文件无变化；正式EXE/053哈希保持 | protected-checkpoint.json；实际新测试编译并运行，不是孤立编译证明 |
| 对照版本线索 | 系统正在运行 PID83476 J:/QQFile/NTSD2.8.3.3 zip/NTSD2.8.3.3/NTSD.exe，SHA5EDA5144…D86B | 进程事实；尚未收到用户确认这就是其对照游戏。文档正式Logan启动器仍启动336B44 |

实际original Editor exact test jobs：
- ba940c59258c495e9e3a9f74eea8fe89：3/3，78.307681秒。
- 06fccd62142748809c9de0d9999d897d：4/4，99.895807秒。
- 334aa845389942c9be618d982f5a8fc4：1/1，6.6813489秒。
- 首次newfile未导入 a1034bb8563849a5ac42777eb1530c65：执行0，不计通过。MCP Assets/Refresh后发现并实际运行。
- Assets/Refresh期间一次Win10061保留；后同6401实际八项运行，不推断原Editor忙或向用户反复要求刷新。

共享消费者：
- LF2Entity.QueueNativeC25FrameSounds 与 nar241/242…以及434/action35产生518/action155→156/053；不是需要逐角色加声音特判。
- NTSDSoundPlayer.PlayPreparedCue/AcquireOneShotVoice；正式336为每event新voice，8-bit PCM原样交XAudio2。今日AVRepair候选同cue复用/Stop/Flush属于另外实现，不能当作336既有规则。
- HeldObjectProcessAll → BattleHeldObjectWriter.RunStep12 维持 reciprocal held关系、按holder WPoint同步weaponact。
- catch33进入130/300，而普通本体131+cpoint继续主抓取链。clone终止不等于held434关系终止；standing WPoint weaponact21，原336实测也保留。
- Unity renderer没有clone特定hide门，但最终像素及正式renderer没有在本批实测，不能从entity存在直接声称屏幕可见。

正式音频后端实际二进制窄读（不是候选源码裁决）：
- submit入口0x1400e4850；缓存命中后仍0x1400e4acd CreateSourceVoice；0x1400e4ba3 SubmitSourceBuffer，0x1400e4bd3 Start。
- 0x1400e4ae8读volumePercent；0x1400e4b28 SetVolume，正增益pow10((percent-100)*38/2000)，与Unity相同。
- mono矩阵0x1400e5bce 1→2 [L/100,R/100]，stereo 0x1400e502b对角矩阵；submit无同cue Stop/Flush。mastering stereo/layout门存在。
- 053两端SHA850EC0DE7EFB86118AC40C933DF01BA98556F15065120843F7701BD1F7187735，0.521179秒。
- 声音文件/事件/PCM/voice参数未证首差；生产mixer/listener、系统增益、持续窗口混音削波及设备输出仍未知。不能说“用户听到杂音没问题”。

所有正式回放/观察仅根336B44、原资源、CLI初始动作与LFR真实输入；未注入World、更改DAT或重建晋升EXE。argv/fixture/GDB/trace/audio原件及完整创建命令留存。直接初始化254未自然OPoint、准备时未攻击、ownedclone尚未ready的早期试例均保留，不能当用户问题通过证据。

下一步已问用户实际原版启动文件路径。识别后只回到对应共享audio/catch/held消费者；如需改变336规则，将明确记录用户指定的新行为/例外，再实施共用修复。当前未改生产脚本/DAT/Scene/InputAction/资源/非战斗；新增只有一份诊断test/.meta和审计/进度文档。未发现可直接交付的另一个共享缺陷，不等于所有角色都完全一致。

Governance checkpoint: actual Validate-ChangeLedger.ps1 exit0 /1290Records /16governedcode paths; historical absent-from-current-diff warnings not errors. Actual git diff --check exit0; logs/summary saved. Beforebackups all hashchecked at final manifest. Task remains unresolved/userreferencepending, notVERIFIED.
