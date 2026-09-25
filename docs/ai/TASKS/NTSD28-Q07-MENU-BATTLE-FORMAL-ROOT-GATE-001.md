# NTSD28-Q07-MENU-BATTLE-FORMAL-ROOT-GATE-001

2026-09-25 scoped exit `VERIFIED`: 原项目Editor双配置Menu回调Play均PASS。正式根进BattleRunning/World2且有序返回；空根旧Menu预热/选人成功、Battle入口拒绝、World0/Stopped/返回Menu/池借用0。原根已在Play内恢复，双Scene/GameConfig磁盘SHA不变。结果及SHA见同ID `ACCEPTANCE-PENDING-20260925.md` 顶部更新。本状态只关闭战斗入口根门，不清除其他旧reader或521项删除门，不关闭Q07。

2026-09-25 diagnostic scope amendment before its script edit: extend the existing `NTSD28Q07MenuSceneCallbackPlayProbeEditor.cs` request with an explicit empty-root rejection mode and refresh only its stale project-mode identity expectation for the normal formal-root mode. In Play, set the GameConfig root to empty **in memory**, run the real Menu prewarm/selection/transition, assert battle refusal before `BattleRunning` and ordered shutdown, then restore the prior root in `finally` before exiting Play. Preserve the old probe's normal mode and all Scene/asset files. This is test code for the same battle-entry behavior, not a Menu production change. Its path is added to the Change Record before touching it.

2026-09-25 当前状态：`RUNTIME_PENDING`。原项目 Editor 重编译并 idle；仅 AppManager 两行根门槛已写，双配置真实 Menu Play 尚未运行。详 `artifacts/diagnostics/NTSD28-Q07-MENU-BATTLE-FORMAL-ROOT-GATE-001/ACCEPTANCE-PENDING-20260925.md`。下方 `IN_PROGRESS` 是实施前合同。

Status: `IN_PROGRESS`. Parent: `BATCH-04/Q07`, old-content retirement entry gate.

Observed gap: `BattleTestBootstrap.LoadCharacterDataAsync` rejects an empty formal content root, but `LoadingPrewarmController.PrewarmOnceCoreAsync` can still complete its existing legacy Menu prewarm on an empty root. `CharacterAnimtorManager.ValidateConfiguredContentForBattleAsync` then returns a null key for a completed legacy publication; `AppManager.InitializeBattleAsync` accepts that key before Battle activation. The current serialized `GameConfig.asset` has a nonempty formal root, so this is a reachable configuration branch, not a measured default-configuration failure. Source and limitations are in `artifacts/diagnostics/NTSD28-Q07-OLD-ASSET-REFERENCE-REFRESH-001/CURRENT-LEGACY-READER-REACHABILITY-20260925.md`.

Change only the production Menu→Battle **battle-entry** guard in `Assets/NTSD/Scripts/App/AppManager.cs`. Before accepting the publication key or starting Battle initialization, reject an empty `CharacterAnimtorManager.ConfiguredContentRoot` with a clear error. Preserve the Menu's legacy prewarm, Inspector/preview tooling, existing nonempty Logan validation and the established shutdown-on-failure path. Do not edit DAT, PNG, Scene, `GameConfig.asset`, or old resource files. This gate does not authorize deleting any old file.

Acceptance: original project Editor compiles without new errors; a focused empty-root Menu→Battle check rejects before BattleRunning and preserves nonbattle Menu prewarm; a configured formal-root Menu→Battle check continues to pass; Battle/Menu Scene and GameConfig disk hashes stay stable; Change Ledger and scoped diff checks pass. If either dynamic check is unavailable, retain `RUNTIME_PENDING` and describe exactly which boundary is unverified.

Risk: an in-memory or external empty-root configuration that previously entered Battle through legacy content will now be rejected. Rollback is the precise AppManager guard hunk under repository approval rules; no existing user data is overwritten.
