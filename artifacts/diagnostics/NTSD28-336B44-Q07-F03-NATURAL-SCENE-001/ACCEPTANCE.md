# F03 自然环境标记：原 Battle Scene 对照

状态：`VERIFIED_SCOPED`，仅关闭 Q07/F03 的当前可达环境标记写入、state12 保留和最终帧尾清。Q07 整组、C031 自然物理键与 C032 声音出口仍开放。

当前权威是根目录 SHA-256 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3` 的正式 EXE 和对应 playable 源码。正式 OID36 多由也 action243、X500/Z400，对 OID2 鸣人 X550/Z400，mode0/BG1/seed682973786、两方初始 HP/MP500、Y0、输入中立。正式源码与根 EXE 同 LFR 128 tick 的 4480 项实体/RNG 字段及 54 条按序关系事件一致；tick1 自然 kind10 产生鸣人环境标记 -20，tick60/65 state12 保留 1，tick66 最终 action230/state14 清为 0。[根对照](../NTSD28-336B44-Q07-F03-NATURAL-MARKER-001/ROOT-REPORT.md)。

原 Unity Editor 在原 `NTSD_Battle.unity` 的 Play clone 用正式暂存角色 DAT/图片内容，生产 `SimulationTickDriver.StepOneTick` 跑同初态的 128 tick。第二轮探针 `tay36-a243-x550-natural-scene-02.json` 记录开始前本场景原生时钟 `(ResourcePhase12, ResourcePhase3, FrameSequence)=(5,2,5)`，与角色、输入相位和 RNG 一同重设至 `(0,0,0)`；随后以同初态逐 tick 对正式源码 CSV。两实体各 15 字段加 6 个 Unity RNG 标量共 **4608/4608 相同，首差无**；探针 `PASS/DONE`、128 样本、原 Scene 正常退出且 clean。[完整离线比较](source-unity-comparison-v2.json)、[原始 Play 数据](tay36-a243-x550-natural-scene-02.json)。源/根的 4480 项口径不含 Unity 额外记录的 RNG `last_call_site`，不可混写成同一字段数。

首轮 `-01` 保留为失败夹具证据：从全局 tick5 重新设置演员，却未重设世界资源时钟，使定时 9 HP 扣血发生在相对 tick7，而正式初态发生在相对 tick12；该首差衍生 392/4608 字段差异，不代表生产战斗规则错误。[首轮比较](source-unity-comparison.json)。只更正已声明的 Editor 诊断脚本初态；未修改生产脚本或 DAT 数值。

原 Editor 新脚本编译完成、未见编译错误；用户确认的四个保护资产在运行前后 SHA 均保持：Battle Scene `3A089236...EC235ED`、Menu Scene `DD6A48A3...0B723B9DC3`、GameConfig `0527D737...D85B82`、ProjectBattleModeConfig `B57CFEF3...1EDD85B82`。原 Editor 最终 idle/非 Play。受影响 B4 环境与相邻类 21/21、DataOrientedCanonical 完整 tick 2/2 为本包此前证据，未重复运行；ChangeLedger 1067 Record/6 代码路径 PASS，`git diff --check` PASS。局限：这是单一自然命中链和声明字段的逐 tick 对照，不证明所有角色/技能、物理玩家按键、音频设备或 Q07 整体验收。
