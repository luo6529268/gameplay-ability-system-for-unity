# C045 正式 OID65 自然抓取伤害入口（只读）

当前权威根 EXE SHA-256 336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3；正式 `resources/runtime/decoded_dat/c/ank/ank.dat` 与项目选中同文件的 SHA-256 均为 7BEEFED713CC85CDF2BF36399FE158B63E822038FBED09AE79901BB6B3E81AC5。DAT 未改。

静态候选链：正式 OID65 action348 的 kind-3 抓取 `catchingact:371 / caughtact:130`，action371→372→373→374→375→376；另一入口 action357 的 kind-3 抓取 `catchingact:366 / caughtact:130`，366→…→376。action376 是 state9/kind-1，`injury:100 / vaction:137 / hurtable:1 / decrease:7 / cover:1`，该 CPOINT 未声明 recover，因此当前 parser 投影默认 recover0。正式 playable `BattleWorld28::settle_catch_relations()` 正伤害后按 recover0 给抓取者 `motion_hold_timer=2`、被抓者=-3；cover1只参与位置/Z/朝向。Unity `BattleCpointWriter.ApplyHeldInjury` 当前按 Cover1 跳过抓取者 `FrameDelay=2`，静态字段与消费分歧明确。

这仍不是运行时首差：需确认正式根/当前源码从348或357的近距输入能自然抓取、关系维持到376、当 tick 抓取者计数为0、被抓者有效HP，以及伤害/停顿真实发生。下一包先用当前正式DAT近/远有界完整 GameSession/Driver 诊断动作、双方关系/HP/停顿和音频，并尽可能同LFR对照根；若自然阳性，再在原 Battle Scene 同初态定位首差，独立 Task/Change 后修共用写者。C040 当前受害者中心与 `vaction` 帧CPOINT分源另需 victim hold非零且当前动作不同于 vaction；本次仅据 DAT 坐标不同不能判为自然首差。C043 静态未见新写者差异，暂不优先。
