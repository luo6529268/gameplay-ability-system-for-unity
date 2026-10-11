# H10 剩余门有限决策准备

Task: NTSD-H10-REMAINING-GATES-DECISION-20261011
Creation status: IN_PROGRESS
授权：当前 H10 持续授权，以及讨论会话 01a11fe2-bfaa-72f3-a9ed-c8260f63b193 的一次性离线决策准备指令。

目标：在明确允许的非受限源码中，核 armor.sound1/sound2、InteractionArea.sound、WeaponStrengthEntry.sound 的值与声音请求消费者证据；基于旧索引限定 IceShatter、BattleDamageWriter 三变量、C25 sound 的剩余读权限；对照 H10 原卡和当前阶段合同，输出能直接审阅的有限决策表。

唯一写域：本新 artifacts/diagnostics 目录。Operation 为 CREATE_NEW_ONLY；只创建 Task、JSON、读取摘录和报告，排他创建，不替换、删除或移动任何已有内容。没有项目脚本变更，无需代码 Change Record/ChangeLedger；末尾 RESULT 和 final-audit 记录终态，创建合同保持原样。

允许源码范围：LF2CharacterData 及对应 armor/InteractionArea/WeaponStrengthEntry 类型、LoganCombatRecordDecoder 对应字段解析、LF2CharacterDatHitResolver、LF2CharacterDamageStateResolver、NTSDGlobal、NTSDSoundPlayer，以及 CharacterAnimtorManager.CollectBattleSoundIds。先 rg --files 找准确文件；按符号读取窄范围，不使用 -C/-A/-B。受限消费者只用旧 request-sites/ChangeRecord 或准确单文件的方法声明/直接 QueueSound 调用行，不展开方法体；旧禁读越界片段不作证据。

不运行 Unity、测试、编译、测量；不新增导出程序、工具探索、插件/包修改或安装；不委派子代理。不重算旧1000cue全图。现有 Menu UNKNOWN 原样保留，不把工具失败当性能问题。不给预算数值，不裁声音、不改规则、不新增架构或自动后继。

验收：逐字段注明已消费/有限值/无合法证据及准确出处；每个确实未完成项对应最小批准或外部条件；明确可选做与不做影响，已证明实际缺陷与未知分开。报告不能降低 H10 原验收条件；设备/AB后置不冒充已通过。当前范围没有有据可实施修复时直说，不新造技术任务。最终复核具名文件指纹和任务外 Git 状态，结束并交回。
