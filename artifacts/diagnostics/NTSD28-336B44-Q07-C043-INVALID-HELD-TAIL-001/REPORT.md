# C043 失效持有关系尾：当前权威对照

范围：G1/BATCH-04/Q07；父目标 `NTSD28-UNITY-BATTLE-REALIGNMENT-001`。正式规则权威为 SHA-256 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3` 的根 EXE 及对应 playable `battle_world.cpp` 中 `BattleWorld28::settle_held_refill_objects`；旧 B1E13 B6 保留为版本历史。

当前源码对失效负关系的两类条件（父槽越界；父实体缺失或反向槽不匹配）均只将子体 `interaction_state` 清零，并继续扫描；`linked_parent_slot` 和父体字段不由此尾清理。Unity `SimulationQueryAndLinkModule.HeldObjectProcessAll` 原先只计错并继续，负 `LinkState` 会被第二次扫描重复遇到。

修改前原 Editor 聚焦 RED job `4ca68d728543402ab94797ad876f417b`：两例均按预期失败，越界预期0/实际-1，互指不匹配预期0/实际-2。最终生产代码在共享分支只写 `held.Runtime.LinkState=0`，事件报告清理前后值及 `Outcome=cleared`；不改 DAT、Scene 或非战斗代码。

原 Editor 修改后聚焦结果：`b8ecaa395ddc47f5a3d5c577c15bb125` 七例 7/7 PASS（越界、缺失、互指、slot0、高槽、生命周期、事件、无 sink 零分配）；完整 tick 三种失效关系 job `fc2fe7b087c04432974fb0d48f163c76` 3/3 PASS；正常持有动作邻接 job `4f5d6dede24b4b5f825c1ef44f989fb2` 6/6 PASS。删除多余快照刷新后的最终生产 DLL 晚于源码，完整 tick 重验 job `5f8a272fb7244fd7b4c3143beb3e70e6` 3/3 PASS。两次用错命名空间/参数格式的筛选均为 0 用例，不计 PASS。最终生成工程 `dotnet build Assembly-CSharp-Editor.csproj --no-restore -v:q -clp:ErrorsOnly` 0 错误；`Tools/Validate-ChangeLedger.ps1 -RepositoryRoot <project>` PASS（1091 Record/38 改动代码文件）。

原 Battle Scene 的请求式 Play 结果 `Temp/NTSD28-Q07-C043-InvalidHeldTail-Play-v1.result` 为 `state=Passed/cases=7`；原 Editor 随后 `is_playing=false`、`activity=idle`。结果中的 `sceneMutation=none` 是探针固定文案，**不单独当作证据**；退出后另算 Battle Scene、Menu Scene、GameConfig、ProjectBattleModeConfig SHA-256 分别为 `3A089236328ACAE1510F8A831B77D4895CC34028DDCDEBE542BEF0DA8EC235ED`、`DD6A48A37FB8CEA9CD8A1F7738964A719E007A42F0FBB54FB48BBD0B723B9DC3`、`0527D737A1FA38FC56B51D00DC6E96A421D3C67222546368B147C2D074CB8EA7`、`B57CFEF32CC3ECE37AC98A4A1EF04FEB2FEE4EB3CC08466A4BFD32C1EDD85B82`，均与请求前保护值相同，四文件 `git status --short` 均无输出。Play 只在真实 Battle Scene 的 Unity Play 环境直接运行七项 World 检查，并未提供自然按键或正式根 EXE 的相同失效关系样本，也未运行完整 `BattleRuntimeSelfCheck.RunAllChecksStatic`；因此本包为 `SCOPED_PLAY_PASS / RUNTIME_PENDING`，C043/Q07/总目标不关闭。
