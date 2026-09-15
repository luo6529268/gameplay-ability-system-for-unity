> 最新排序：held三作用域已限定VERIFIED；state10同图0已补。下一CPOINT-INPUT-ACTION-SELECTION-001：RunKind1→RunActionSelection实际只A/T/J且多次写，源advance_catch_relations八路最终选择；这是数据consumer缺口与selected-frame绑定的完整事务，不只改一个getter。后续canonical kind8单caller（dvx999为不写哨兵）按RAW-BINDING-CALLER-MATRIX排队；dynamic identity须回访held静态零域边界。

> 当前修订：held初次140/释放1150/补给242已限定验证，见父held报告；下表旧HasFrame857描述是实施前检查点，不能继续当当前代码事实。SyncHeldPose只有定义无直接caller，damaged12/10静态域核对中。当前C++ held无state12/18 unsupported分支，历史表述已纠正。下一按RAW-BINDING-CALLER-MATRIX与identity等实际caller继续。

# Remaining native frame reader audit

IN_PROGRESS / PRIORITY_LIVE_HELD_PATH_CONFIRMED. This is a read-only call-chain audit; no production changes. Inventory is lexical navigation, not proof that every occurrence is live. Unreviewed entries remain explicit.

| Family | Evidence and classification | Next ownership |
|---|---|---|
| Shared character DAT move/input/cost setters | LF2Entity RunCharacterInputRoutingPhaseForKnownCharacterDat returns for LF2Character and for world.UsesNTSD28NativeInputPipeline before calling old RunShared*; methods3697/3713/3785 belong compatibility route. | Preserve; do not repair solely for grep cleanliness. |
| DirectWriteRawFramePreserveWaitCounter | LIVE: BattleDamageWriter native impact/common writes, CPoint drop, current physics helpers; still legacy GetFrameDataById, missing low returns EmptyFrame(wait1), >=857 null. Native action raw can differ from descriptor. | Separate raw binding transaction witnesses; do not globally replace public getter. Already closed action/low-frame behavior stays protected. |
| DirectWriteHeldFramePreserveWaitCounter + weapon/nonweapon gates | LIVE: NTSDBattleTickSystem -> HeldObjectProcessAll (C09/C20) -> SimulationQueryAndLinkModule -> BattleHeldObjectWriter.RunStep12 -> LF2WeaponHeldStateResolver.Act for weapon subclasses. Both branches still HasFrame with857 bound after raw binding. | Highest next concrete source witness: held native action descriptor, validity, positioning and following tick. |
| SetFrameTickDirect / SetFrameTickRawDirect | Mixed: C25 main native path uses RunNativeC25FrameTransaction/BindNativeC25Action; old RunCommonFrameTick body excluded when nativeC25 active. Some pre-frame/noncharacter physics writers still use old setters. | Classify callers individually; cannot mark entire family dead or covered. |
| TryApplyRuntimeIdentity | LIVE: Oid5152FusionScanAll -> BattleOid5152RuntimeModule merge/split. Fixed290/112 + retained split currentAction. Module additionally has857 candidate guard. | Separate source fusion-catalog/identity witness; preserve already verified timer/pass placement. |
| TryReloadCurrentFrameDataForRuntimeIdentity | Only definition found across Scripts; no direct caller observed. | Keep; no inferred production change. |
| ReloadCurrentFrameDataFromWrapper | Only old throw transform propagation subtree caller. | Compatibility-only as observed, retain. |
| SpawnLateTransitionEffects | LIVE_COMPAT: BattleLateEntityLifecycleModule -> RunLateTailAfterNativePreviousActionCommit -> virtual tail. State13/200 spawns15 OID999, legacy RNG andSFX066. | Authority of entire transaction unresolved; cannot change one getter and claim equivalent. Do not confuse source hit-state13 response with exit particles. |
| protected ApplyCpointThrowStep10 + transforms | Only compatibility test caller found; formal BattleCpointWriter.ApplyThrow already independently corrected. | Retain subtree; don't redo CPOINT-THROW-NATIVE-RAW-BINDING. |
| Other hit prediction, AI, spawn readers | Inventory records occurrences; formal versus fallback caller needs further audit. | Still pending, not covered by this initial table. |

Native held authority: SimulationTickDriver28::step calls settle_held_refill_objects beforegeometry682 and afterhits908. battle_world.cpp parent_frame lookup then terminal weapon_action>=1000 despawns; otherwise raw child action writes before descriptor lookup, null reports unsupported and continues. Child native implicit0..998 valid, declared999 valid; negative/missing999 unavailable. A valid descriptor enables facing, frame delay and integer anchor/depth writes. Unity old HasFrame gate blocks these writes for implicit/high targets. Full downstream throw/drop/refill ordering must remain intact; source witness first.

Independent read-only identity/late/throw review completed by cpoint_acceptance. Snapshot/schemas, Unity/GAS, Scene, resources and source authority unchanged. Previous cost/physics/oracle children VERIFIED; Q06 umbrella and audit remain open.

## Additional confirmed call edges

- DirectWriteHeldFramePreserveWaitCounter is shared with BattleDamageWriter.ApplyNativeType3TargetGenericContinuation (called by native type3 post-hit continuation), not exclusively held-object synchronization. Any shared setter change must explicitly protect/test this caller or use a new scoped native writer at held callsites; do not silently broaden the migration.
- LF2WeaponHeldStateResolver.Act and BattleHeldObjectWriter.RunStep12 both first write requested raw action and then apply oldHasFrame gate. Terminal>=1000 is a separate prior branch and must remain before any raw write.
- ApplySignedCpointFrame -> SetFrameTickDirect remains called by BattleCpointWriter action branch, so SetFrameTickDirect cannot be classified universally as dead after C25 migration.
- SetFrameLogicRawFramePreserveAttacking -> SetFrameTickDirect remains called by LF2WeaponFrameLogicResolver pre-frame state handlers (40/60); need actual frameLogic caller/gates before migration.
- BattleDamageWriter.ResolveNativeType3AttackerPostHitAction reads selected hit_Fj via old public getter, then ApplyNativeType3AttackerPostHitAction writes DirectWriteFramePreserveWaitCounter. This is another explicit live native reader family; prior type3/feedback scopes do not prove high/implicit selected-frame behavior.
- Correct distinction: old GetFrameDataById returns shared EmptyFrame for missing0..856; old HasFrame false there. >=857 and negative getter returnnull. Therefore identity fixed290/112 missing fields are wrong defaults/cache identity, not null; high retained split actions can be null. Native missing0..998 uses per-indexzero frame, declared999 only, other valuesnull.

Old B6 missing-action test explicitly uses777 and-888 as missing cases. Current native777 is valid implicit; this is a historical oracle/content-boundary assumption to revisit only after fresh source witness, not justification to preserve oldHasFrame. Keep old write-before-failure/negative/terminal/reciprocal/cleanup ordering proofs and original failure artifacts.
