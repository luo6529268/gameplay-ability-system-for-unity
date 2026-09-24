# NTSD28-USER-SOURCE-HITFA5-TARGET-VELOCITY-001

Status 2026-09-24: `FOCUSED_TEST_PASS / PLAY_PENDING`; original Editor indexed formal-content tests 8/8 PASS. D-024, Q07 paused.

Authority: formal root EXE SHA-256 `B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033`; playable `native_ai.cpp:150-208` behavior5 creates OID219 child at controller `Position28` and sets child raw Vx to integer `(friendly.position.x-child.position.x)/50`. Indexed formal `w/e.dat` frame51 is a reachable `hit_Fa5` case. Existing original Editor first-difference witness in `TARGET-HISTORY-VELOCITY-FIRST-DIFFERENCE.md` proves raw target motion 48 after gap151 needs Vx3/4 while physical screen-scaled integer gap yields Vx4/5.

Unity pre-change: `LF2Entity.RunHitFa5FrameLogic` computes child Vx from current physical integer gap and emits a direct-position task with only physical X/Z and integer mirrors. Source carrier exists but is neither read for velocity nor copied to this child birth. This is a confirmed non-perceptual D-024 gap; DAT remains immutable.

Declared script scope: `Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs` behavior5 task construction only, and `Assets/NTSD/Scripts/Test/Editor/NTSD28FixedViewRunRatioEditorTests.cs` existing source-DAT two-view and moved-history cases. When controller and target source carriers are initialized, compute the native integer quotient from their source XInt values and pass controller source precise X/Z and integer X/Z into child birth. Otherwise retain physical fallback without inventing source history. Preserve actual physical birth, velocity-to-view scaling at motion integration, entity slot, owner/team/action and all DAT tokens.

Acceptance: formal frame51 two moved histories now Vx3/4, equal-history and factor1 controls, child source birth identity, one scaled physical movement step, original Editor focused tests. Full Driver/Play/EXE remains pending; other hit_Fa modes are separate.

Rollback: review and reverse only declared script hunks, retaining other work.

Result: behavior5 now computes child raw Vx from target/controller source integer X when both source carriers are initialized, retains old physical integer fallback when incomplete, and carries controller source precise/integer X/Z into the OID219 child task. Existing staged formal `w/e.dat` frame51 tests now assert moved target Vx3/4 where current physical gaps would yield4/5, plus child source X100/Z200 and one physical view-scaled motion step. Original Editor job `a5adcc7dfefc49c796dfe35f84fe4438` passed the eight exact indexed cases, including factor1/configured, negative distance and incomplete-source fallback. Full Driver/Play/formal EXE and other hit_Fa modes remain open; Q07 stays paused.
