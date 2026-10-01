# C042 OID75 自然抓取→投掷原 Battle Scene 限定验收（2026-10-01）

**结论：** 当前正式336B44的 OID75 `bee.dat` action355/X500→鸣人OID2/X550，自然抓取→tick55投掷路径在原 Unity Battle Scene 的前60逻辑tick中，对正式源码15个实体字段及5个RNG字段 **1200/1200相同、首差0**。本包验证已修共用投掷路径的一个自然消费者；投掷前被投者动作计数为0，**没有触发 C042 非零计数保留分支**，故 C042 父门及 Q07/总目标继续开放。

[正式源/根可达性报告](../NTSD28-336B44-Q07-C042-NATURAL-THROW-001/REPORT.md)记录同一初态的完整源码与根同LFR回放各80tick、1360/1360声明字段零差；正式根身份为SHA-256 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3`。本轮原 Battle Scene 使用项目已暂存的正式OID75/2内容，Play克隆设置roster和source规则位置/种子，暂停后经生产`SimulationTickDriver.StepOneTick`以中性输入推进；没有强设抓取关系、被投者计数或中途动作。

| 时点 | 正式源 | 原 Battle Scene |
| --- | --- | --- |
| tick1 | kind3抓取，OID75动作358，抓取目标槽1 | 动作358、抓取目标槽1 |
| tick54 | 被投者动作135、计数0 | 动作135、计数0 |
| tick55 | 投掷后被投者动作181、尾部计数1 | 动作181、尾部计数1 |

[原始Play JSON](bee75-a355-x550-natural-scene-01.json)为`CAPTURED/DONE`，有60条样本；[逐tick比较](comparison-v1.json)比较双方action/counter、X/Y、速度、HP、抓取者目标槽/抓取时限，以及CRT state/calls与同步RNG counter/index/calls，每tick20值、数值容差0.001、差异0。源的`thrown_relations`事件由完整源码报告在tick55记录；Unity此处以动作、速度和计数尾部见证投掷效果，未把最终动作计数1当作分支前非零计数。

新探针原Editor Tundra编译成功、0 C#错误；Play结束后`exitedPlay=true`、`sceneCleanAfter=true`，Editor idle、非Play、非编译。Battle Scene SHA `3A089236328ACAE1510F8A831B77D4895CC34028DDCDEBE542BEF0DA8EC235ED`、Menu Scene `DD6A48A37FB8CEA9CD8A1F7738964A719E007A42F0FBB54FB48BBD0B723B9DC3`、GameConfig `0527D737A1FA38FC56B51D00DC6E96A421D3C67222546368B147C2D074CB8EA7`、ProjectBattleModeConfig `B57CFEF32CC3ECE37AC98A4A1EF04FEB2FEE4EB3CC08466A4BFD32C1EDD85B82`，与运行前一致。没有修改生产、DAT、Scene、模式资产或非战斗功能。

此限定结果不覆盖 action378 的长持有投掷、投前非零计数、全部80tick、物理玩家输入、正式根与Unity全World或可见画面。下一步仍按G1找能自然形成投前非零计数的正式可达前置；找不到时维持条件项，转下一条可达 Q07/Q08 差异，避免用手设计数冒充根自然证书。
