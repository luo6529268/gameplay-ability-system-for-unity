<!-- CHANGE-RECORD
id: NTSD28-Q09-OID434-CENTRAL-COMMAND-001
status: VERIFIED
change-kind: EDITOR_DIAGNOSTIC_ONLY
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q07Oid434NaturalBindingProbeEditor.cs
authority: formal Logan ras.dat frame396 pic36 and Unity production CentralOnly presentation/central submission path
evidence: original Editor Play central plan tick54 and one exact OID434/36 frozen Entity command PASS; camera draw and pixels pending
-->

# NTSD28-Q09-OID434-CENTRAL-COMMAND-001

Pre-edit state: Q07 live actor/child binding is proven, but the observer exits Play at the same Editor callback as the target completed tick. It has not checked `SimulationWorld.BattlePresentation.PublishedFrame`, `BattleCentralRenderSystem.CurrentPixelFramePlan`, frozen command materialization or actual camera pixels. Add an opt-in hold-and-read phase after the entity hit, without advancing another logic tick. Match the target by stable ID and runtime slot as well as key `(434,36)` to avoid mistaking the earlier OID434/action35 for the visible child. Side effects are a unique request/report and temporary Play only. A missing plan/command remains FAIL, not automatically a production defect until timing and owner are audited. Validation and rollback are in the Task Contract.

Post-edit/validation: only the request-driven Editor diagnostic was extended. The original Editor imported it with Console error0; regenerated Editor C# build had 0 errors/22 incremental warnings. Unique original-Battle-Scene Play `oid434-central-command-20260925-1` PASS: logic tick54 OID434 stable ID121/slot50/action396 bound `(434,36)`, production current pixel plan owner Central/mode CentralOnly/display tick54/generation52, live submission frozen frame tick54 and exactly one matching Entity command among seven at 48×48/sort order5. Editor exited Play, Battle/Menu/GameConfig SHA unchanged. JSON hash and limits: `artifacts/diagnostics/NTSD28-Q09-OID434-CENTRAL-COMMAND-001/ACCEPTANCE-20260925.md`. The report's inherited `scope` sentence is narrower than its Q09 fields. Camera acquisition, executed draw, GPU pixel and formal EXE A/B remain unverified. Rollback limited to new opt-in diagnostic extension under protected-file rules.
