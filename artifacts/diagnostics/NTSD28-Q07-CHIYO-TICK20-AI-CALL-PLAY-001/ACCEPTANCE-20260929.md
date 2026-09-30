# Q07 千代动态槽与随机调用首差限定验收

状态：`VERIFIED_SCOPED_TICK18_SLOT_FIRST_DIFFERENCE / Q07_OPEN`。本包只关闭两次原 Editor 定向诊断，不证明 Q07、D-024 或完整战斗同态，也没有修改生产战斗规则。

正式权威是根 `NTSD2.8-Logan.exe` SHA-256 `B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033`、paired playable `simulation_tick_driver.cpp::SimulationTickDriver28::step` 和已核根回放 `../NTSD28-Q07-CHIYO-PUPPET-AD-NATURAL-001/run2/formal-root-replay-trace.jsonl`。原 Editor 仅运行保存的 `NTSD_Battle` Scene、正式 LoganRuntime、既有 direct-canonical 千代/鸣人固定输入及完整生产 Driver。可选探针记录相对 tick17–20 活动动态槽、OID850 AI 实体字段，以及 tick20 直调与接受提交的随机调用；两次均在 tick20 结束，未重跑 120 tick 自然链。

原始报告：`../NTSD28-Q07-CHIYO-CONTROL-NATURAL-PLAY-001/chiyo-ai-tick20-20260929-01.json`，SHA-256 `74F02B49C1B56F51C22AA6059FEDEA226B7FB4E03E5B780BD18523CD18631C49`；补槽报告 `../NTSD28-Q07-CHIYO-CONTROL-NATURAL-PLAY-001/chiyo-ai-slots-20260929-02.json`，SHA-256 `DDE7991FDD28DD3649C0B8F17EE39F124422D9543F30B0B566D42C640F706EAE`。两份均为20样本、`PASS_SCOPED_TICK20_AI_AUDIT`，相对 tick20 同步RNG前0后6、直调观察0、接受AI调用6：`0x14, 0x3c, 0x1c, 0x1e, 0x1f, 0x38`（十进制20/60/28/30/31/56）。这六次的逐调用计数1–6和本 tick 总增量6一致；观察接口没有逐调用实体 owner 字段，不能单凭它断言每次均归属 OID850。`ObjectAiTargetSlot3F8=-1` 是对象目标字段，不能当作该 tick AI 所选目标的负证据。

第一个已观察实体槽差异在相对 tick18：正式 OID419/type3/action300 占 slot50；Unity 活动动态列表只有 OID419/action300 占 slot51，tick17 末无活动动态实体。tick19 正式 OID419/slot50/action301，且正式 spawn 事件 OID850→slot51、OID204→slot52；正式 OID850 当 tick 已 action311。Unity tick19 OID419/slot51/action301，OID850/slot50/action310；tick20 OID850/slot50/action311、OID419/slot51/action301、OID204/slot52/action60。相同输入相位与千代动作在 tick17–20 一致，OID419 数均匹配，但动态 slot 次序不一致。

正式 `simulation_tick_driver.cpp` 930–936/948–1036 的升序槽晚期循环明确允许新生高槽在同一 tick 后续处理；Unity `BattleLateEntityLifecycleModule.Run` 71–77/226–257/334–357 也按升序槽扫描、在 OPoint 后刷新队列。现在最强的共享机制候选是 Unity 首先把 OID419 放在 slot51，使其生成 OID850 时回填已扫过的 slot50，于是 OID850 本 tick 不再推进，次 tick 的 AI 前置动作不同。**但还没有第18 tick 的分配事件或 slot50 占用/释放时间记录，不能宣布该机制已证，更不能按 OID419/850 特判。** 下一精确门只审第18 tick 的 slot50/51 search、claim、release、spawn 时序；若确为通用空槽分配/回收顺序首差，再以共享规则修复及相邻正反例验收。

验证：生成 `Assembly-CSharp-Editor.csproj` 两次构建 exit0；原 Editor 程序集时间均晚于对应探针源码、MCP 过滤探针错误0；两次原 Battle Play 均成功退出，`stopped/detached/exitedPlay/sceneCleanAfter=true`，World对象/runtime slot/pool borrower各0。Battle/Menu Scene、GameConfig 与 ProjectBattleModeConfig 四个保护 SHA 与入场相同。`Tools/Validate-ChangeLedger.ps1` 与 `git diff --check` 在本包最终文档同步后复核；未运行全量 SelfCheck、其它角色或最终 Q12 验收。
