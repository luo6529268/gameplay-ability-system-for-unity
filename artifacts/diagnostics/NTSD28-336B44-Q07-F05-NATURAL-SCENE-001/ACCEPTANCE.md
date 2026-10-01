# F05 自然终局帧计数：原 Battle Scene 对照

状态：`VERIFIED_SCOPED`，仅关闭 Q07/Q08/F05 当前可达的 type0 物理主槽死亡、state14 终局持帧计数清零机制。Q07/Q08 整组及结果页以外其他战斗出口仍开放。

权威是 SHA-256 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3` 的正式根 EXE 和对应 playable source。正式 OID36 多由也/action243 X500 对 OID2 鸣人/action0 X550，双方 Y0/Z400、team1/2、初始 HP/baseHP500/18、MP500、mode0/BG1/seed682973786，后续128tick中立输入。源码自然在 tick24 令鸣人 HP0；tick65 action185/state12/counter2，tick66 action230/state14/counter0，此后63次终局 held 帧事件。正式根同 LFR 128tick4480/4480可比字段零差、63个终局tick完全相同，root report passed/failureCode0。[正式根报告](../NTSD28-336B44-Q07-F05-NATURAL-TERMINAL-001/ROOT-REPORT.md)。

原 Unity Editor 在原 `NTSD_Battle.unity` 的 Play clone 预设同一正式暂存角色 DAT、初始 HP/复活 lives1/queued HP0、输入相位0/RNG seed，且把原场景已走过五 tick 的 native 世界时钟 `(5,2,5)` 在测量边界重置为 `(0,0,0)`。随后生产 `SimulationTickDriver.StepOneTick` 中立输入跑 **80 tick**，限定在战斗内、未延伸到结果页流程。离线把这80 tick 两实体每个16字段与每tick6个RNG标量同正式源码逐项比较，**3040/3040相同、零首差**。tick24 HP0，tick65 counter2，tick66 state14/action230/counter0，tick67及80持续0均一致。[离线比较](source-unity-comparison-v1.json)、[原始 Play 数据](tay36-a243-naruto18-natural-scene-01.json)。探针 `PASS/DONE`、80样本、退出 Play、场景 clean。

既有 F05 原 Editor DataOriented/Legacy 正反门及复活邻近类21/21通过，未重复运行。新探针在原 Editor 编译0错误；四个保护资产的运行前后 SHA 保持 Battle Scene `3A089236...EC235ED`、Menu Scene `DD6A48A3...0B723B9DC3`、GameConfig `0527D737...D85B82`、ProjectBattleModeConfig `B57CFEF3...1EDD85B82`。本子包只加 Editor 诊断脚本及 Unity 生成 meta，未改生产代码、DAT、Scene 或配置。此出口不证明不同角色/伤害途径、物理玩家按键、所有复活模式、结果页、完整 Q07/Q08 或全游戏。
