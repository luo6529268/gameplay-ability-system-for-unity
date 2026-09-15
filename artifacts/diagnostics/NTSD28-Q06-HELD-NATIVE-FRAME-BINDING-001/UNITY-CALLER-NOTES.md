# Unity held reader pre-change notes

Read-only, production unmodified in this Task.

Exact main live path: NTSDBattleTickSystem.HeldObjectProcessBefore/After -> SimulationWorld.HeldObjectProcessAll -> SimulationQueryAndLinkModule.HeldObjectProcessAll -> BattleHeldObjectWriter.RunStep12. Runtime-slot negative LinkState and reciprocal holder.TargetSlotIndex gates belong the traversal, not proposed frame binding migration.

Weapon subclasses go through LF2WeaponBase.Act -> LF2WeaponHeldStateResolver.Act. Generic entities take the other RunStep12 branch. Both terminal WeaponAct>=1000 precede raw frame write; both oldHasFrame after write incorrectly treats nativeimplicit/high as unsupported. Actual missing negatives/missing999 must still preserve action-write-before-return, relations/pose/motion and no RNG. Refill exhaustion precedes terminal in weapon path; preserve independent contracts.

Valid path copies facing/FrameDelay, integer anchors using parent and child center/wpoint, cover0/1/2 adjustsZ/Y, then throw/drop state changes. Current witness onlykind1/dvx0/cover0; no assertion of all branches.

Shared DirectWriteHeldFramePreserveWaitCounter also serves type3 target generic continuation in BattleDamageWriter. A proposed helper change requires explicit cross-caller tests; alternatively migrate exact held callsites to existing native raw setter while preserving counter andlatch. Existing BindNativeC25Action(commitLatch:false) reads native frame, writesaction andwait/next only forvaliddescriptor, retains latch; do not infer complete equivalence without invalid/lifecycle witnesses.

Old NTSD28B6WpointMissingActionContinueProductionEditorTests assumes777missing and hascurrentMissing999 setup. Source777implicit case added. Any oracle correction is separate exact record, keeps negative/actuallymissing controls and raw-write/continue semantics. Existing closure is historical limited scope, not evidence that oldcontent admission overridesD023/native model.

Remaining source cases needed before broad held exit:kind3/drop, dvx releases, oppositefacing/cover variants, timer/counter sentinels,refill/invalidrelationship protection, both logic/renderer factories, futurefullticks and orderedshutdown. Scope can be a declared framebinding task with related regression evidence; do not claim fullB6/Q06.

Dormant-cover note: current Unity two pose methods treat every nonzero cover asback; native cover2 makesnooffset. Prior HELD-RELATION-PRODUCER-DOMAIN-CORRECTION audit cover2/state12/18 zero-reachability used DirectionB Unity138 projection/older closure, so it cannot automatically certify D023 formal330 domain. Before closing full held-content alignment, re-evaluate formal reachable union; keep dormant rule owner, do not silently declare either handled or irrelevant. Current140 deliberatelycover0 and makesno cover2claim.
