# NTSD28-Q07-MODE-COMBO-LIVE-TUPLE-AUDIT-001

Status: `STATIC_PRODUCTION_CONNECTION_GAP_CONFIRMED / SAME_TICK_RUNTIME_PENDING` (2026-09-22). Scope: Q07 formal content activation of an already-built Q06 combo producer and state carrier; Q09 owns the visual atlas/commands. No code, Scene, menu flow, or results page was changed.

## Formal release call chain

The formal `resources/runtime/decoded_dat/data/mode.dat` selects `data/mode/ntsd.dat`. Its `<combo>` block has `bound: 1`, `facing: 1`, `respond: 50`, `caughtact: 1`, plus `pic: sprite\combo_hits.png`, dimensions and presentation parameters. Formal `GameSession28` resolves that child and calls `NativeComboHud28::load` (`game_session.cpp:1332-1374`); when it builds World options, a present config writes `selected_mode_combo.record_present = true` and the four values (`game_session.cpp:4148-4158`). The table and image were staged with exact formal hashes in `NTSD28-Q07-MODE-COMBO-CONTENT-ENTRY-001`.

## Unity current path

`NTSD28NativeComboRuntimeState` in `Assets/NTSD/Scripts/Simulation/Runtime/BattleRuntimeState.cs:509-531` defaults to `RecordPresent=false, Bound=0, Facing=1, Respond=50, CaughtAct=0`. The production `BattleNativeComboOrdinaryProducer` requires `RecordPresent` and `Bound==1`; `BattleInteractionPipeline`'s caughtact producer requires `RecordPresent`, `Bound==1`, `CaughtAct==1`; the expiry pass requires a present record. A scoped production C# search found reads of these fields and snapshot-restore writes, but no ordinary formal-content initialization writer that reads the selected mode child and sets this tuple for a new battle World. Tests can set it explicitly; that does not activate the default production battle.

This is a **static connection gap**, not a fresh observed same-tick damage/visual trace. The existing Q06 producer/expiry/caughtact implementations and their bounded tests remain valid; they must not be reimplemented to compensate for missing configuration. A content-hash match also does not prove the record was consumed.

## Next exact implementation and exit

Trace the current formal-content publication owner into battle World creation; add a narrowly scoped parser/projection for the selected `mode.dat` child's combat `<combo>` tuple and initialize the existing `NTSD28NativeComboRuntimeState` before the first tick. Keep legacy/unconfigured behavior unchanged, and do not consume `<menu_control>`, `<menu_small>` or KO/results presentation records under this Task. Before any script edit, create the required Task/Change Record with exact paths and an observed no-config fallback. Verify parser tuple, World init, ordinary/caughtact producer gates and reset/re-entry in focused checks; then compile and compare a representative natural hit/throw in the **original** Unity project against the formal playable tick/record. Q09 separately consumes `combo_hits.png` for presentation.

## Subsequent projection step (2026-09-22)

`NTSD28-Q07-MODE-COMBO-INPUT-PROJECTION-001` added a pure, non-publishing projection for the selected two DAT files. Standalone in-memory compile and formal/staged tuple/fingerprint, malformed block, path escape and no-config checks passed. The production catalog identity, publication and World remain unchanged; Unity compile and original-Editor runtime checks are pending. The next atomic integration task therefore begins at identity + publication freshness + `ApplyMatchConfig` activation, then verifies producer gates and same-tick behavior; do not duplicate the parser or Q06 producer.
