# Q07/F02 高速武器物理动作 40

状态：`UNITY_FOCUSED_PASS / RUNTIME_PENDING`。父目标 `NTSD28-UNITY-BATTLE-REALIGNMENT-001` 与 Q07 整组仍开放。

原始可恢复证据：[修复前 RED job](original-red-job.json)、[首次 GREEN job](first-green-job.json)、[最终聚焦及相邻 50/50 job](final-focused-job.json)、[本轮全量 SelfCheck FAIL](full-self-check-failure.txt)、[运行前已有的旧 PASS 文件](preexisting-self-check-result.txt)。最终 job JSON 的 `progress.total=8750` 是测试目录发现数，实际执行数由 `result.summary.total=50` 给出。

权威为根目录正式 `NTSD2.8-Logan.exe` SHA-256 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3` 对应的 playable live path：`physics_integrator.cpp` 在 X/Z 积分和接地减速后，以原始物理入口帧的 state1000 与 type4/6、严格 `Vx<-9 || Vx>9` 选动作40；其后若符合正向穿地落地条件，动作0/60/70覆盖它。`battle_world.cpp` 只写最终动作，F02 本身不清帧计数。正式 runtime catalog 中 OID600=`w/6.dat` type4，frame0/state1000/hit_Fa0，frame40/state1002。Unity 已暂存相同内容，但本包没有改 DAT。

原 Unity 的普通 `LF2Weapon` 物理路径没有此选招；旧预帧 resolver 被 `hit_Fa>0` 门控，无法覆盖 OID600。共享非角色路径在摩擦前选招，并提前改掉落地所读帧。原 Editor job `9d122170aa6c4530a84107c1d2c9666d` 对新增 25 例及既有 5 例共运行30例，11例失败：普通 type4/6 与正式 OID600 漏选40，共享地面临界/落地早选。另4例 type4 X 断言漏算已有 `0.2*Vx` 额外位移，是夹具错误；仅改正断言。

本包在 `CharacterMechanics.SelectFastWeaponActionAfterFriction` 收口同一规则。普通武器和共享路径均在共用物理积分后读取减速后的 Vx，但保留原进入帧 state；符合 type4/6 落地时先用原帧完成覆盖动作，未落地才写40。旧预帧重复选招删除，hit_Fa4/12 仍保留。原 Editor 修正编译通过，同一测试 job `3bbb667a5eb5461aab7e98c6f633e773` 30/30 PASS，其中正式暂存 OID600 无 hit_Fa 的直接物理案例 PASS。该案例不等于完整 Driver tick 或正式根 EXE 同态。

第一次修复后编译曾因测试文件漏引入 `NTSD.Simulation` 而报 CS0103；补引用后重编译成功。旧 `BattleRuntimeSelfCheck` 直接调用预帧 resolver 的两处断言已改为“物理前不切40”。全量 self-check 通过原 Editor 文件请求实际运行，但在更早的 `CheckBattleSpritePrewarmTransactionContracts` 失败，原始结果保存在 `Temp/NTSD_BattleRuntimeSelfCheck.result`；因此它未执行到 F02 的 `CheckFrameLifecycleWeaponFrameLogicContracts`。最终原Editor job `643237600e314a539880670c50aab69a` 在F02、B4 identity-X extras和源坐标物理三类共50/50 PASS，包含对上述既有武器分支的隔离调用。全量FAIL仍保留，不处理图片预热基线。旧全量结果 `PASS` 的修改前副本保存在同目录 `preexisting-self-check-result.txt`，其原时间为2026-09-29 03:29:39。

验证边界：正式根 EXE 同初态完整 tick、Unity 正式 OID600 完整 Driver tick、自然 Battle Scene Play 及整个 Q07/Q12 均未验。用户的完整背景相机、比例位移、非战斗模式/背景例外不变。未改 scene、DAT、PNG、WAV、config 或非战斗流程。最终原Editor状态为 ready/非重载；正式 EXE、Battle/Menu Scene、GameConfig 和 ProjectBattleModeConfig 的 SHA-256 与包前记录一致，后两者分别为 `0527D737...CB8EA7`、`B57CFEF3...D85B82`。`Tools/Validate-ChangeLedger.ps1` 最终 1045 records / 99 governed code files PASS；`git -c core.safecrlf=false diff --check` PASS。现有工作树仍含此前未提交修改，本包未清理、恢复、暂存或提交它们。

独立只读审查未发现本包新增代码的可执行缺陷；审查指出原 Unity job 终态需要落盘以便恢复，已按上方文件保存。审查不是正式根/自然 Play 验收。
