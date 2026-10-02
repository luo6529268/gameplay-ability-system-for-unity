# NTSD28-336B44-Q07-C051-EFFECT22-SELFCHECK-ORACLE-001

状态：`VERIFIED`（仅SelfCheck合成口径；C051父出口开放）；父目标 `NTSD28-UNITY-BATTLE-REALIGNMENT-001`；新版 BATCH-04/Q07/C051。

## 当前首差与权威

原 Unity Editor 第四轮完整 `BattleRuntimeSelfCheck` 在 `CheckStandardCharacterDamageAlignmentContracts` 的合成 `effect22` 终值断言失败，原件为 `artifacts/diagnostics/NTSD28-336B44-Q07-C043-FUSION-HELD-SCENE-001/selfcheck-result-04.txt`。该测试攻击者为普通 Other、朝右、state0、X0；角色目标 type0、X10；`dvx=8`，没有 state2000/type4/6 分支或旧冲量。断言仍要求 `KnockbackX=-8`。当前正式336B44 playable `battle_world.cpp` 的普通无甲命中调用 `HitResponseResolver28::accumulate_unarmored_horizontal`，其该条件分支在 `hit_response.cpp` 中按攻击者朝向取完整 `dvx`，故 X 应为 `+8`。减伤响应的 effect22/23 相对X专支不属于此路线。Unity `LF2HitResolveRuntimeData.ResolveStandardDamageKnockbackX` 已按 C051 移除误套分支，现返回面右 `+8`。

只改 `Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs::CheckStandardCharacterDamageAlignmentContracts` 这一合成断言的 X 期望 `-8f→8f`，并在失败信息中输出 actual/shared 快照以定位其余字段；保持 Y12、动作203及 actual/shared 同态约束。若复验显示其它字段首差，先独立归因，不批量调整测试。不改生产、DAT、图片、Scene、菜单或比例映射。脚本前建立同名 Change Record 与 Ledger、STATE、handoff；脚本后编译、原 Editor 刷新和完整 SelfCheck，再做账本/差异/四保护SHA检查。旧四轮结果和本轮新结果都保留。

预期副作用只在测试口径，不提升 C051 自然 effect22 可达性或 Q07 完成状态。回滚仅经审阅反向调整本断言精确差量，不使用整体 Git 恢复。
