# F05 自然终局持帧计数：正式根对照

当前根 EXE SHA-256 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3` 已在执行前重核。正式 source `GameSession28::step` 以 OID36 多由也/action243 X500 对 OID2 鸣人/action0 X550，Y0/Z400、team1/2、HP/baseHP 分别500/18、双方 MP500、mode0/BG1/seed682973786、中立输入连续128 tick。只设置初始角色数值，没有中途强写受伤、死亡、state14、计数或复活。

源码自然过程：鸣人 HP 第12 tick 为9，第24 tick 为0；第65 tick 仍为 action185/state12/counter2/HP-20；第66 tick 落地选 action230/state14，终局物理槽帧事件 `terminal physical participant retained state-14 lying frame` 将 counter 清为0，此后至128tick共有63次该事件。复活 lives1、queued HP0。注意第66 tick 的帧号230是 state14，并非字面 action14。

同一 source-capture LFR 经选定正式根 `--headless-playback-lfr` 重放，报告 `passed:true/failureCode:0/completedTicks:129`；EOF tick129和根独立 CRT seed 不在128tick比较窗口。正式源与根两实体256行×15可比字段，加每 tick 5 个 RNG 标量，共 **4480/4480 零差异**。63个终局门 tick 完全一致，首次第66 tick；根事件 state14/HP<=0/计数0的每 tick 观察与源码带具体 message 的事件序列吻合。[逐字段比较](source-root-comparison-v1.json)。根 JSON trace 未暴露 `revive_next_hp`，其源值全程0但不计入4480；根独立 `crt_state` 也未纳入。

这份证据只关闭 F05 的正式根自然案例。Unity 原 Battle Scene 同初态完整 Driver/Play 尚待单独子任务，F05/Q07整组仍开放。原版 DAT、Unity DAT、生产代码、场景和配置均未改。
