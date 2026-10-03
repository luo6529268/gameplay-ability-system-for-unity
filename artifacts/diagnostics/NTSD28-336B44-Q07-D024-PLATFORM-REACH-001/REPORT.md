# Q07/D-024 正式 frame182 平台非零运动受控可达性

状态：`VERIFIED_SCOPED_CONTROLLED_REACH`。正式根 EXE SHA-256 为 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3`。只读当前对应 playable `GameSession28` 与正式 `resources/runtime`；正式 `c/hid/rea.dat` 和 Unity 暂存同路径原始 SHA 均为 `27F0B91F92C09BEB9BBE11127D0EF4AC3B0267F1978737F94C76841BC010F75D`。本包没有修改 DAT 或正式 EXE。

正式源码 `battle_world.cpp::rebuild_geometric_hit_candidates` 的 kind50 不同组平台操作在目标 X/Z、前后 Y 和碰撞参考满足时写 `platform_source_slot_f4`；其扫描不读取 ITR 的 `y:564000`。`BattleWorld28::apply_frame_motion` 随后在链接且目标 Y 等于碰撞参考时读取源当前帧 `dvx`，从目标规则整数 X 重基。当前正式 DAT 的非零候选是 OID56/frame182 的 `dvx:-3`。

唯一新增 [诊断源码](../../../Tools/NTSD28Q07Diagnostics/d024_formal_platform_reach_probe.cpp) 用固定 mode0、seed `0x28A55A5A`、正式背景1/Z400、中性输入，受控把 OID56 源置于 X200/Y0/action182，把不同 team 的 OID2 置于 X205或208、Y−10/−5/0/5。八例各十完整 tick。只有 X205/Y−5 产生连续十 tick 平台链接和非零搬运：第1 tick 建链，目标从第2 tick 起每 tick 减3，X205→178；X205/Y−10只建链一 tick、目标不移动；其余六例未建链。原始 [首轮表](source-run-01/summary.csv) 与 [逐 tick 数据](source-run-01/source-ticks.csv) 保留。加入 LFR 后独立 source-run-02 的两份 CSV 与首轮逐字节 SHA 相同，编译两次 exit0。

阳性 [LFR](source-run-02/positive-source.lfr) 经当前正式根 EXE `--headless-playback-lfr` 回放 exit0、`passed=true`；根报告明确 `nativeParityClaim=false`，其默认路径比声明十 tick 多完成一个 tick。仅对共有初态及 tick1–10 的两实体九字段逐 tick比较，[99/99 同值、首差无](root-playback-01/paired-selected-fields.json)：动作、规则 X/Y、目标碰撞参考、平台来源和阴影偏移。根 [原报告](root-playback-01/root-report.json) 与 [trace](root-playback-01/root-trace.jsonl) 保留。

这里的 frame182 是测试设置的合法初始动作，**没有证明普通玩家从自然按键能进入它并持续十 tick**。正式背景1只供源码共同纵深前置，不进入 Unity 项目。原 Battle Scene 同初态和画面比例是独立 Scene Task；本源 Task 的限定出口已完成，Q07/D-024、Q09/Q12及总目标仍开放。
