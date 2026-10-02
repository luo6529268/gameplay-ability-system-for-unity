# NTSD28-336B44-Q08-C009-RETURNING-GROUP-001

2026-10-02 只读 owner 边界：正式 mode0 多组返组的 `winner_group=-1` 进入原生 render snapshot；Unity战斗停战读组 mask、计时与转场，自有 `Winner/PendingWinner` 由另一结果摘要路径写入。当前无非例外战斗规则直接消费自有赢家的证据，故不因 C009 暂停计时证书新增赢家载体或修改结果页。再次单组的根/原Scene后继仍待，C009/Q08开放。[报告](../../../artifacts/diagnostics/NTSD28-336B44-Q08-C009-OWNER-BOUNDARY-20261002/REPORT.md)。

2026-10-01 原 Scene 出口更新：原 Editor 编译后以受控正式 OID304/type3 发射者的生产工厂在原 `NTSD_Battle` Play 副本生成 OID56/team2；12 个完整生产 Driver tick 的计时、输出计时、组 mask、出生数与发射者动作对正式源码同相位 72/72 零差。Play 退出、Scene clean、四保护 SHA 稳。[报告](../../../artifacts/diagnostics/NTSD28-336B44-Q08-C009-RETURNING-GROUP-001/REPORT.md)。这是返回暂停规则的限定场景证书，非物理按键自然选招、再次单组后继、结果画面或整场证明；下文较早的“Scene 待证”以本段覆盖，C009/Q08 不整体关闭。

2026-10-01 原 Scene 见证脚本前增补：新增 `Assets/NTSD/Scripts/Test/Editor/NTSD28Q08C009ReturningGroupBattlePlayProbeEditor.cs`（及 Unity 自动生成的 `.meta`），只在原项目现有 `NTSD_Battle` 的 Play 副本运行。它以 `BattleTestBootstrap` 建立单个正式 OID56/team1，暂停生产 Driver 后用既有 `LF2ObjectPointFactory.CreateObjectImmediate` 把正式 OID304/type3/action11/team2 放入生产 World，随后通过 `SimulationTickDriver.StepOneTick` 输入空包推进 12 tick，记录 `NativeResultTimer`、输出 timer、组 mask、OID304/action、OID56/team2 的出生与位置。预期前置是初态只有 team1 存活，第二 tick 产生 team2 OID56，下一 tick 起 timer 保持且 mask 两组。不得修改生产代码、DAT、Scene、模式资产、非战斗逻辑或原有 probe；不会保存 Scene。失败时保留 JSON、退出 Play 并核对 Battle/Menu/GameConfig/Mode 四资产 SHA 与 Scene clean。若原 Scene 无法以正式内容建立该条件，标 `SCENE_CONDITION_BLOCKED`，不得把源码/根证书冒充 Unity 场景通过。

2026-10-01 正式根自然出口更新：正式源码诊断/根正式 EXE 的 OID304→OID56 返组已按 12 tick × 7 个同相位字段 84/84 对齐。[原件](../../../artifacts/diagnostics/NTSD28-336B44-Q08-C009-RETURNING-GROUP-001/REPORT.md)。根自身 `nativeParityClaim=false` 且多一个终端 trace tick，证书只覆盖所列 12 tick；原 Battle Scene 自然 Play、独立赢家状态/画面、再减至单组后的时序仍待。正文下方“正式根待证”是此前阶段快照，以本段覆盖。

状态：`FOCUSED_TEST_PASS / FORMAL_ROOT_NATURAL_SCOPED_PASS / UNITY_SCENE_SCOPED_PASS`。父目标：`NTSD28-UNITY-BATTLE-REALIGNMENT-001`，BATCH-04/Q08/C009。规则权威为 SHA-256 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3` 的根正式 EXE、对应 playable live path 的 `GameSession28::step` 与 `BattleFlow28::step`。正式根同条件见证仍是独立出口，源码测试不能代替根 EXE。

已观察首差：正式 `battle_flow.cpp` 在结果 timer 已开始而第二存活阵营返回时，保留 timer、更新两组结果并清除单赢家；下一次只剩一组才从保留值继续计数。Unity `BattleResultsOutcomeHostWriter.AdvanceNativeFlowBeforeCombat` 仅在 timer 为 0 时处理多组，timer 非零时仍递增，且 `NativeLivingGroupMask` 保留旧单组。现有 `NTSD28Q08BattleFlowRedProbeEditorTests.NativePrecombatResultLatchesFirstTerminalGroupsAcrossRevival` 把旧行为写为预期。正式当前 DAT `data.txt` 将 OID304 绑定 `s/4/4.dat`、OID56 绑定 `c/hid/rea.dat`，前者 action1 有生成后者的 OPoint；正式源码的旧单元测试引用旧解包目录，不作为新版资源实测。

脚本所有权：`Assets/NTSD/Scripts/Simulation/Ecs/Results/BattleResultsOutcomeHostWriter.cs` 的共用结果倒计时入口；`Assets/NTSD/Scripts/Test/Editor/NTSD28Q08BattleFlowRedProbeEditorTests.cs` 的既有回归案例。2026-10-01 增补只读权威见证工具 `Tools/NTSD28Q08Diagnostics/returning_group_lfr_probe.cpp`：用当前正式源码、正式运行资源、OID56 单组及 OID304/action11 发射者，输出逐 host 计时/存活组/OID56 出生记录和 LFR，再将同一 LFR 送正式根 EXE。该工具不得改变正式源码、DAT、场景或生产逻辑；若前置未成立，必须保留失败原件而不伪造阳性。原测试先 RED 后 GREEN 的生产修复已完成。只处理存活组 mask、timer 暂停/恢复及输出 timer；不改结果页设置、重赛、模式资源、正式 DAT、Scene、GAS、非战斗逻辑或其它未声明脚本。

验收：同初态完整 Unity tick 的两组→单组 timer1→两组 timer 仍1、mask两组→单组 timer2、mask单组；涵盖 mode0/1 适用共享门，且初始两组 timer0、普通 timer350 退出、结果页 host 邻接不退化。编译零错，定向 EditMode、必要的原 Battle Scene 运行、四保护资产 SHA、Change Ledger validator 和 `git diff --check` 逐层报告。正式根自然 OID304 返组 LFR 与 Game View 未取得前，C009/Q08 保持 `RUNTIME_PENDING`。

风险：结果状态参与快照/校验和；本包沿用现有 mask/timer 字段，不改 schema。若 Unity 未导出当前赢家独立字段，只用 mask 的单组/多组语义声明已验证，不冒称结果页赢家已与原版画面一致。回滚仅在审阅本包改动后逆向修复上述两处脚本，不使用 `git restore`、删除或覆盖其它用户工作。

实测：原 Editor 定向 RED 1/1（Expected timer1，实际2）→同例 GREEN 1/1；相邻结果类 13/13、既有结果快照/校验和 1/1 均通过。具体作业 ID、资产 SHA 与尚缺的正式根/原 Scene 出口见[报告](../../../artifacts/diagnostics/NTSD28-336B44-Q08-C009-RETURNING-GROUP-001/REPORT.md)。
