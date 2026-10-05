# NTSD28-336B44-BATTLE-AUDIO-ALIGNMENT-20261006
状态 IN_PROGRESS；独立新任务，旧 NTSD28-UNITY-BATTLE-REALIGNMENT-001 维持 USER_ACCEPTED_SCOPED_CLOSURE。
需求：2026-10-06 用户提供 03_声音资源，授权修复全部角色与战斗音效（技能/帧语音、普通受击、防御/护甲、武器/特攻、对象落地/破坏、战斗配置已有提示）。
权威：正式336B44 EXE，resources/runtime/data/sound.dat 与正式可达角色/战斗DAT；中文整理音频先映射核字节/PCM，再接入正式VFS路径。不同音频来源不能静默混用；不改DAT，不导入原版背景/模式DAT、BGM或非战斗UI。
批次出口：
1. 资源映射/逐文件身份与准确部署清单；现有12 WAV保留，新增已确认Battle闭包；音频由唯一播放入口解析builtin别名，FrameSounds max20全部预热，运行时不分散修改声源。
2. 普通命中/防御/特攻和非角色反弹落地共享声音规则、事件顺序、source X；保持全部伤害/动作/运动字段，ECS预测同。已有正确帧latch、声像/增益/voice上限复用，不扩大未证候选。
3. 原Unity Editor编译、具名聚焦测试/实际clip解码与voice、必要代表Battle Play、有序退出；保护DAT/Scene/config，审查diff/ChangeLedger。仅原件/逐事件已证部分称对齐，设备与未覆盖行为如实说明。
Change：NTSD28-336B44-BATTLE-AUDIO-CUE-RESOLVER-001；NTSD28-336B44-BATTLE-AUDIO-EVENT-RULES-001（均PLANNED）。
治理：FILE-OPERATIONS/NTSD28-336B44-BATTLE-AUDIO-ALIGNMENT-20261006-PREPARE；artifacts/diagnostics/NTSD28-336B44-BATTLE-AUDIO-ALIGNMENT-20261006。
验收不要求全部角色组合逐个重跑；共同逻辑用代表例，文件内容差异用资源批量验证。菜单/结算/设置/重赛/字体/联机不加入。

当前进度：原Editor compile/39去重测试/978实际解码和voice热路径/22当前Core事件均通过。原Scene01 startup超时而非声音PASS；02正在有限等待/定向播放验收，状态快照及原件在本Task artifact。原12 WAV/meta24项Git前基线完全稳定。

2026-10-06 最新更正：Scene02提前退出由用户明确确认是他或其他任务的操作；尚未进入声音预热/战斗tick，所以不是运行音频失败也不是PASS。Scene03在已保存Battle/Editor idle前置后启动，输出新03保留01/02。Dash三RED额外1个cue，删三行后同三项GREEN，动作/速度/DAT帧a7断言通过；累积42去重case，原39和978加载未重复。独立只读复审无新增阻断；原Scene声音/普通SelfCheck整体未知仍明确未晋升。
