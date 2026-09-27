# Q08 结果设置提前写旧战斗字段：原 Editor 完整 tick 见证

状态：`FOCUSED_TEST_PASS / UNITY_ONLY_CURRENT_STATE_WITNESS`。父级 `BATCH-04 / Q08` 继续开放。

现有 `NTSD28Q08CombatLethalPrecombatTimingEditorTests` 夹具增加一个测试：第一完整 tick 先让两个组均存活，再使同一攻击帧自然重叠致死，等待 Unity 结果页由旧 `BattleEndPhase` 激活；P1 Attack 进入设置，Left、Left 导航到 cursor0。最后一个完整 tick 分别给 Attack pressed edge 和无按键。两例共用相同角色、frame、KO、页内导航和 native timer 进程，没有修改生产代码或 DAT 数值。

| 完整 tick 最后一步 | 角色 `FallDamageDiv` 前→后 | native result timer | native transition |
|---|---:|---:|---:|
| 无 Attack | 0→0 | 14 | 0 |
| Attack pressed edge | 0→100 | 14 | 0 |

原项目 Editor PID11944 通过 MCP `Assets/Refresh` 编译，`Assembly-CSharp-Editor.dll` 更新至 2026-09-27T04:08:49Z，晚于测试源码；`Editor.log` 为 Tundra build success，无 C# error（现有 warning 保留）。精确 EditMode job `ffc645602a7444d3aaf6348a44a3c007` 为 1/1 PASS、0 failed/skip，原始 MCP 结果在 [editor-job.json](editor-job.json)。第一次调用测试时原 Editor 正处于程序集 reload，端口6402拒绝连接；等同一原 Editor status `ready`、端口归属 PID11944 后才发送唯一成功 job，没有启动第二 Editor 或复制项目。末态 `get_editor_state` 报 idle、non-Play、非编译、无测试运行。

这证明 Unity 自有结果设置的一个真实完整 tick 路径在正式结果尚未转场时改写旧 World 角色战斗字段。正式发行 host 不提供对应结果设置动作，因此它**不是**已测出的原版与 Unity 同输入 first difference，也不授权删除 Unity 自有结果页。`PendingHostAction` 未见生产 consumer；后继生产包必须定义下一场设置的实际消费 owner，并在保持自有 UI 的同时阻止旧 World 的战斗字段被结果页提前更改。旁支 Q07 代表案例、已过自然 KO/timer350/70tick 寿命不重跑。

下一场配置调用链复核：`SimulationTickDriver.TryDispatchOrdinaryResultTransition` 在普通命令2时调用 `AppManager.TryReturnToCharacterSelectionFromBattleResult`，后者有序卸载 Battle 并将菜单选择页 `ResetAll`；新 Battle 的 `AppManager.InitializeBattleAsync` 再以 `CurrentMatchConfig` 调用 `SimulationTickDriver.ApplyMatchConfig`，它重置旧 World 后重新从配置赋 `Difficulty`、`StageIdx` 等。生产 `SetMatchConfig` 调用者是受保护的 `CharacterSelectionController`；没有找到把旧结果页 `PendingHostAction` 或旧角色 `FallDamageDiv` 带入新 `MatchConfig` 的生产路径。因此不能只删 `ApplyFallDamage` 后声称结果设置已正确迁移。任何需要改菜单/选择流程的方案都超出当前已授权的非战斗边界，必须先形成具体影响和可审阅方案；其余Q08正式可达战斗规则可独立推进。

保护检查：Battle Scene SHA-256 `2EE465D83C7169A0589447F437E37CAEFF3CC6F1BA6C3AAA55B8068F2B48B77A`，Menu `785F828C4E64182BEA214E4794B198E3C82E3C42002FDADD3932A7E061B81E13`，项目模式 Asset `88E10D43B047952FD3053A87B7E5F60D0F87A6CD1A23313F37EA1503C686F55C`，均同前。仅一个 Editor test 脚本改动；无生产、Scene、DAT、PNG、ProjectSettings 或非战斗改动。原 Task 曾要求保留 XML，但此 MCP job 只提供结构化原始 JSON；没有伪造 XML，已在 Task/Record 作格式更正。
