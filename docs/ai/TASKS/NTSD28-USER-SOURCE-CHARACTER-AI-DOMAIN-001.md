# NTSD28-USER-SOURCE-CHARACTER-AI-DOMAIN-001

Status: `FOCUSED_TEST_PASS`. D-024 character AI production position-domain correction; Q07 paused. Exact current results and limitations are in the Change Record.

Authority: formal root EXE SHA-256 `B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033`; playable `SimulationTickDriver28::step` and `NativeAi28::select_local_character_target` (strict source integer X/Z Manhattan rank, slot order). User D-024 retains a full-background camera and scales actual physical displacement without DAT edits. Existing two-profile original Editor witness selected physical-nearest slot2 versus formal source-nearest slot1 in self-check and production `CharacterInputAll`.

Pre-change: configured DataOriented profile builds SoA/unified AI rows and spatial index from physical X/Z; Legacy `world.X/Z` and spatial/brute scan also read physical X/Z. Source carrier exists and has versioned reset/copy/checksum but is not exposed to character AI. A partial reader change would mix position domains.

Declared script scope:

- `Assets/NTSD/Scripts/Simulation/Core/SimulationWorld.cs`: latch AI source-domain completeness before each AI input snapshot; preserve existing world and battle fields.
- `Assets/NTSD/Scripts/Simulation/Ai/Runtime/SimulationAiInputModule.cs`: pass-scoped domain flag, consumed by shared world.X/Z accessors and reset on clear.
- `Assets/NTSD/Scripts/Simulation/Ai/Snapshots/AiSensingSnapshot.cs`: capture domain metadata with rows, including reset/copy.
- `Assets/NTSD/Scripts/Simulation/Ai/Runtime/SimulationAiSensingModule.cs`: project X/Z for initial/fused/refresh rows from the latched domain; role indexes then use the same projected X.
- `Assets/NTSD/Scripts/Simulation/Ai/Runtime/SimulationAiDecisionModule.cs`: project owned/unified X/Z and legacy/remainder fallback consistently; source-active unified pass must avoid stale physical pending publication/rolling snapshot, recapturing its affected row from source at the existing post-input boundary.
- `Assets/NTSD/Scripts/Test/Editor/AiSensingSoACandidateEditorTests.cs`: turn the confirmed-defect witness into formal GREEN with both execution profiles; add incomplete-carrier fallback and narrow refresh/index guards.

Domain contract: at `BuildAiInputSlotSnapshot`, use source integer X/Z only when every active entity participating in that snapshot has initialized source history; otherwise use physical X/Z for the whole pass. This deliberately conservative fallback avoids mixed coordinates in thresholds and broadphase. Freeze the selection for the input phase. Rebuild source-domain unified rows each pass because source writes are not physical `BattleFrameMotionStore` mutations; recapture changed source rows at the existing post-CharacterInput refresh. Keep canonical physical stores, view scaling, frame-motion, DAT, Scene and nonbattle logic untouched.

Acceptance: original Editor formal nearest two-profile slot1 and actual `CharacterInputAll` target slot1; incomplete-history both profiles target slot2; focused index/refresh/legacy parity and no-allocation checks; compile and Change Ledger validator. Controlled formal-content full Driver human12/AI3 and production App participant-birth checks are now recorded in the Change Record. Natural Battle Play with dynamic child materialization, formal EXE parity and complete source-writer closure remain later gates. If the domain contract cannot be made coherent in these declared files, stop before expanding script scope and amend Task/Record first.

Rollback: reverse only these declared AI script hunks and witness assertions, retaining unrelated work; source carrier remains independently valid.
