# Q07/F02 高速武器物理动作 40

状态：`UNITY_FOCUSED_PASS / RUNTIME_PENDING`。父目标 `NTSD28-UNITY-BATTLE-REALIGNMENT-001` 与 Q07 整组仍开放。

原始可恢复证据：[修复前 RED job](original-red-job.json)、[首次 GREEN job](first-green-job.json)、[最终聚焦及相邻 50/50 job](final-focused-job.json)、[本轮全量 SelfCheck FAIL](full-self-check-failure.txt)、[运行前已有的旧 PASS 文件](preexisting-self-check-result.txt)。最终 job JSON 的 `progress.total=8750` 是测试目录发现数，实际执行数由 `result.summary.total=50` 给出。

权威为根目录正式 `NTSD2.8-Logan.exe` SHA-256 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3` 对应的 playable live path：`physics_integrator.cpp` 在 X/Z 积分和接地减速后，以原始物理入口帧的 state1000 与 type4/6、严格 `Vx<-9 || Vx>9` 选动作40；其后若符合正向穿地落地条件，动作0/60/70覆盖它。`battle_world.cpp` 只写最终动作，F02 本身不清帧计数。正式 runtime catalog 中 OID600=`w/6.dat` type4，frame0/state1000/hit_Fa0，frame40/state1002。Unity 已暂存相同内容，但本包没有改 DAT。

原 Unity 的普通 `LF2Weapon` 物理路径没有此选招；旧预帧 resolver 被 `hit_Fa>0` 门控，无法覆盖 OID600。共享非角色路径在摩擦前选招，并提前改掉落地所读帧。原 Editor job `9d122170aa6c4530a84107c1d2c9666d` 对新增 25 例及既有 5 例共运行30例，11例失败：普通 type4/6 与正式 OID600 漏选40，共享地面临界/落地早选。另4例 type4 X 断言漏算已有 `0.2*Vx` 额外位移，是夹具错误；仅改正断言。

本包在 `CharacterMechanics.SelectFastWeaponActionAfterFriction` 收口同一规则。普通武器和共享路径均在共用物理积分后读取减速后的 Vx，但保留原进入帧 state；符合 type4/6 落地时先用原帧完成覆盖动作，未落地才写40。旧预帧重复选招删除，hit_Fa4/12 仍保留。原 Editor 修正编译通过，同一测试 job `3bbb667a5eb5461aab7e98c6f633e773` 30/30 PASS，其中正式暂存 OID600 无 hit_Fa 的直接物理案例 PASS。该案例不等于完整 Driver tick 或正式根 EXE 同态。

第一次修复后编译曾因测试文件漏引入 `NTSD.Simulation` 而报 CS0103；补引用后重编译成功。旧 `BattleRuntimeSelfCheck` 直接调用预帧 resolver 的两处断言已改为“物理前不切40”。全量 self-check 通过原 Editor 文件请求实际运行，但在更早的 `CheckBattleSpritePrewarmTransactionContracts` 失败，原始结果保存在 `Temp/NTSD_BattleRuntimeSelfCheck.result`；因此它未执行到 F02 的 `CheckFrameLifecycleWeaponFrameLogicContracts`。最终原Editor job `643237600e314a539880670c50aab69a` 在F02、B4 identity-X extras和源坐标物理三类共50/50 PASS，包含对上述既有武器分支的隔离调用。全量FAIL仍保留，不处理图片预热基线。旧全量结果 `PASS` 的修改前副本保存在同目录 `preexisting-self-check-result.txt`，其原时间为2026-09-29 03:29:39。

验证边界：正式根 EXE 同初态完整 tick、Unity 正式 OID600 完整 Driver tick、自然 Battle Scene Play 及整个 Q07/Q12 均未验。用户的完整背景相机、比例位移、非战斗模式/背景例外不变。未改 scene、DAT、PNG、WAV、config 或非战斗流程。最终原Editor状态为 ready/非重载；正式 EXE、Battle/Menu Scene、GameConfig 和 ProjectBattleModeConfig 的 SHA-256 与包前记录一致，后两者分别为 `0527D737...CB8EA7`、`B57CFEF3...D85B82`。`Tools/Validate-ChangeLedger.ps1` 最终 1045 records / 99 governed code files PASS；`git -c core.safecrlf=false diff --check` PASS。现有工作树仍含此前未提交修改，本包未清理、恢复、暂存或提交它们。

独立只读审查未发现本包新增代码的可执行缺陷；审查指出原 Unity job 终态需要落盘以便恢复，已按上方文件保存。审查不是正式根/自然 Play 验收。

2026-09-30 后续独立 Q09/P-20 测试夹具更正已让全量自检越过上述图片预热断言，但该新鲜运行在更后的 `R3-AI-LIFE-01` 零血 AI 输入断言处 FAIL；见 [后续报告](../NTSD28-336B44-Q09-P20-BMP-SELFCHECK-GRID-001/REPORT.md)。F02 的隔离武器分支仍是 50/50 聚焦证据，不能由此把全量自检或 F02 整包升为 PASS。

2026-09-30 F02 补验：独立 C017 旧零血 AI 断言修正后，全量自检进入本包 `CheckStateTransformLandingMatrix`，旧期望帧60/Vy0失败；按正式 `PhysicsIntegrator28` 的 action40 后硬着地覆盖，将合成夹具仅改为最终帧0/Vy-7、原Vx8.4/耐久15。原Editor重编译后自检通过此矩阵，在后面的 C012 旧特殊命中锁存尾断言失败；[改前失败](pre-transform-matrix-self-check-failure.txt)、[改后下一失败](post-transform-matrix-self-check-next-failure.txt)。整份 SelfCheck 仍 FAIL；F02 正式根同状态、完整 tick 和自然 Play 仍未验。

随后独立 C012 旧帧尾断言按正式规则修正，原 Editor 新鲜[全量 SelfCheck 返回 PASS](post-c012-full-self-check-pass.txt)。这是本包目前最高的自动自检证据，覆盖旧 F02 武器分支与合成落地矩阵；正式根 EXE 同状态完整 tick、Unity 暂存 OID600 完整 Driver tick、自然 Battle Play 仍待，F02/Q07 整包不能关闭。

后续只读核对正式 LFR 回放边界：`GameSessionLfr28::snapshot_entity` 的物理槽只保存 OID、阵营、整数位置、MP、owner、base HP 等，不保存运动速度；正式回放公开覆盖项只有初始 action/facing/MP 等，没有初始 Vx。因 F02 必须用 `|Vx|>9` 触发，把自定义高 Vx 源码 probe 录成 LFR 后让正式根 EXE 回放，不能构成**同初态**权威对照。下一证据先用所选336B44 playable live `GameSession28`/`SimulationTickDriver28` 源码在相同 OID600/速度下跑完整 tick，并让 Unity 生产 Driver 做配对；正式根 EXE 出口仍须由自然可达的武器生成链或等效正式入口独立补证。仅以当前 50/50、自检 PASS 或无速度的根 LFR 不能关闭 F02。

该下一证据已在独立[完整 tick 报告](../NTSD28-336B44-Q07-F02-OID600-FULL-TICK-PAIR-001/REPORT.md)补齐：正式 playable 源码探针四组，原 Unity Editor 生产 Driver 四组 4/4，追加 2048 宽统一投影后五组 5/5 PASS。正式根 EXE 同态、自然武器与 Battle Scene Play 仍未证，F02/Q07 保持 `RUNTIME_PENDING`。
