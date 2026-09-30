# Q09/P-20 飞段自然 frame430 原Unity逻辑限定验收

状态：`FOCUSED_TEST_PASS / NATURAL_BATTLE_PIXEL_PENDING`。本包只关闭飞段自然输入到frame430的原Unity完整Driver逻辑门；P-20、Q09、BATCH-05和总目标仍开放，Q07/D-024碰撞域选择独立。

正式依据是根 `NTSD2.8-Logan.exe` SHA-256 `B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033` 的[同输入LFR轨迹](../NTSD28-Q09-P20-HIDAN-NATURAL-FRAME430-001/formal-root-frame430-trace.jsonl)，普通攻击tick1–2、跳跃9–10、防御+前+攻击13–14，飞段OID24/action0在tick16进入action430/pic119/PP150。原Unity复用现有JSON驱动完整 `SimulationTickDriver`，只在Editor测试诊断入口新增独立 `ntsd28-q09-hidan-frame430-natural/1.0` 精确schema，并用[六行物理输入场景](hidan-frame430-natural.json)表达同一序列；不改生产输入路由或资源。

原项目Unity Editor PID11944、`Assets/NTSD/Scene/NTSD_Battle.unity`、非Play经绑定`gameplay-ability-system-for-unity@b1b02287`的MCP新鲜核验。唯一`refresh_unity(force/all, compile=request)`返回成功，域重载时实例曾暂离线；原Editor进程保持活跃，程序集写于本轮，实例恢复后确认场景非Play/idle。随后唯一原Editor请求被消费，[结果](unity-frame430.result.txt)为PASS；[raw](unity-frame430.raw.jsonl)、[输入相位/RNG](unity-frame430.input-rng.jsonl)均有30个完成tick，不把另一`test`项目的MCP默认实例算作本项目证据。

Unity与根EXE逐tick选定actor action、currentMP、输入相位和combo0共30×4=120项，独立[比较](unity-vs-root-selected-compare.json)零差；两端首次action430都是tick16。Unity raw头声明`certificateEligible:false`，因此此处只证明这四个选定逻辑字段，不证明位置/碰撞全态、Unity Battle自然Game View或同帧本体像素。正式frame430 pic119在首sheet声明范围内但超5×11容量，既有共享容量门聚焦4/4通过；实际战斗画面是否无本体仍需单独Play像素见证。

本轮只修改既有Editor raw-capture脚本的独立schema/精确fixture门，保留该脚本先存Q08/R06未提交修改；新场景和输出均在本包目录。两个Scene、GameConfig与ProjectBattleModeConfig的[前](protected-before.json)/[后](protected-after.json)SHA-256四项相同。DAT、PNG、Scene、配置Asset、生产脚本及非战斗逻辑未改。`Tools/Validate-ChangeLedger.ps1` PASS（1003 records/34 diff code files），`git -c core.safecrlf=false diff --check` exit0。
