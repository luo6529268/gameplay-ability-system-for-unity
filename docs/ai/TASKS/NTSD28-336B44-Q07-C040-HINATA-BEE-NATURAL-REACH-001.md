# NTSD28-336B44-Q07-C040-HINATA-BEE-NATURAL-REACH-001

状态：`VERIFIED_SCOPED_NEGATIVE / C040_STILL_OPEN`。父目标 NTSD28-UNITY-BATTLE-REALIGNMENT-001；BATCH-04/Q07/C040。正式336B44 playable `BattleWorld28::settle_catch_relations()` 在受害者动作因停顿保留时，仍用抓取者 `vaction` 对应的受害者 CPOINT 做位置。正式 OID41 雏田 `c/hin/hin.dat` 抓取帧125→126→127 的 `vaction` 为130→132→131且帧126 injury8/hurtable1；正式 OID75 奇拉比 `c/bee/bee.dat` 的受害者帧132 CPOINT与130/131坐标不同。以上只是静态候选，需完整GameSession自然运行确认。

只新增 `Tools/NTSD28Q07Diagnostics/hinata_bee_held_position_lfr_probe.cpp`，以正式 `resources/runtime`、mode0、seed682973786、雏田OID41 X500/Z400 初始action286或360、奇拉比OID75 近距/远距、双方HP/MP500、中性输入，最多120tick。记录双方动作/计数/位置/速度/HP/停顿/持有槽、抓取/持有pass与RNG，输出LFR。阳性标准：自然kind3建关系后，动作127或其它正式持有帧的`vaction`与受害者仍保留的动作不同、受害者停顿非零，而且对应受害者CPOINT坐标不同；必须从实际源tick和正式DAT核实。阳性再用同LFR运行SHA为336B44的正式根EXE；Unity对照另立任务，不能用本包推断Unity结果。

不改DAT、Unity生产、Scene、Prefab或非战斗；已有脏工作原样保留。编译当前playable闭包、运行近远及必要的不同起手控制、核身份和逐tick首差，登记实际证据与限制。回滚仅审阅本新增诊断CPP，不清理其它文件。

结果：[六例正式源报告](../../../artifacts/diagnostics/NTSD28-336B44-Q07-C040-HINATA-BEE-NATURAL-REACH-001/REPORT.md)。结算前假候选由停顿-1→0消失；v2六例结算后分源条件0，不作根/Unity自然证书。
