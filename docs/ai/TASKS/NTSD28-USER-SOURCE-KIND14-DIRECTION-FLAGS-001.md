# NTSD28-USER-SOURCE-KIND14-DIRECTION-FLAGS-001

Status 2026-09-24: `FOCUSED_TEST_PASS / CARRIER_NOT_ACTIVE / PLAY_PENDING`. Original Editor exact new source flag class 7/7 PASS; adjacent ECS kind14 methods 2/2 PASS. Full Driver/Play/EXE remains pending, and other source position writers still gate gameplay readers and Q07.

Historical pre-edit status: `IN_PROGRESS / SOURCE_FIRST`. Parent D-024. The source-rule gameplay readers remain inactive, and Q07 stays paused.

Authority: formal EXE SHA-256 `B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033` and playable `battle_world.cpp` kind14 collision branch, which compares integer `Position28::x/z` with strict ±5/±2 and publishes one-physics-step directional flags. `PhysicsIntegrator28::step` consumes the flags. The already-approved D-024 physical output uses a larger fixed view.

Unity pre-change: physical kind14 flags use `Runtime.XInt/ZInt`; the independent source-rule position and flags exist, and source physics consumes flags, but no production kind14 path writes them. `BattleBoundaryWriter` is the common active writer. Unregistered fallbacks exist in `LF2Entity`, `LF2CharacterDatHitResolver`, `LF2SpecialAttack`, and `LF2CharacterHitResolver`; ECS projection predicts physical flags and actual writer dispatch still routes through the same runtime paths. A source-only flag writer is needed across these exact paths without replacing physical flags or changing render output.

Declared script scope: `Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleBoundaryWriter.cs`, `Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs`, `Assets/NTSD/Scripts/Animation/LF2Objects/LF2CharacterDatHitResolver.cs`, `Assets/NTSD/Scripts/Animation/LF2Objects/LF2SpecialAttack.cs`, `Assets/NTSD/Scripts/Animation/LF2Objects/LF2CharacterHitResolver.cs`, plus focused `Assets/NTSD/Scripts/Test/Editor/NTSD28SourceKind14FlagsEditorTests.cs`. All source flag writes must be gated by both actors having initialized source-rule positions. No DAT/Scene/ProjectSettings/nonbattle changes or gameplay source-reader activation.

Acceptance: configured and factor-one world, histories with equal physical but different source integer gaps, strict ±5/±2 and both signs, no-source gate, one-step physics consumption, existing physical kind14 tests. Original Editor compile and exact focused NUnit, diff/ledger/scene-content guards. Full Driver/Play/formal EXE remains a separate gate.

Rollback: reviewed changes only in declared scripts, preserving unrelated user work and existing physical kind14 behavior.
