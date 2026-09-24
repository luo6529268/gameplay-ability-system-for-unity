# NTSD28-USER-SOURCE-CPOINT-THROW-POSITION-001

Status 2026-09-24: `FOCUSED_TEST_PASS / CARRIER_NOT_ACTIVE / PLAY_PENDING`; original Editor throw class 8/8 PASS. D-024, Q07 paused.

Authority: formal root EXE SHA-256 `B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033`; playable `BattleWorld28::advance_catch_relations` around `battle_world.cpp:5970-5990` writes caught integer X from catcher integer X plus raw frame-center/CPoint local offset, mirrors precise X, and leaves Z untouched. User D-024 keeps physical full-background travel ratio; DAT numerics cannot change.

Unity pre-change: `BattleCpointWriter.ApplyThrow` uses catcher physical XInt to place victim physical X/XInt but does not update initialized source-rule X. Existing production two-view witness shows physical caught X159/184 after raw catcher Vx48 while formal source outcome would remain X159.

Declared script scope: `Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleCpointWriter.cs` source-only throw X placement gated on initialized pair, and `Assets/NTSD/Scripts/Test/Editor/NTSD28Q06CpointThrowRawBindingEditorTests.cs` paired source assertions. Preserve physical landing pose, Y/Z, velocity, action, DAT, Scene, ProjectSettings and nonbattle code.

Acceptance: factor1/configured-view source caught X159 and source integer mirror after moved catcher, source Z unchanged, existing throw regression, original Editor compile and focused NUnit. Full Driver/Play/EXE still open.

Rollback: review and reverse only these script hunks, retaining existing work.

Result: throw writer now copies the formal raw integer X anchor from catcher source-rule XInt and frame-local center/CPoint X into caught source-rule precise/integer X, only when both carriers are initialized. Source Z remains unchanged as formal throw specifies, and physical output is unchanged. The paired two-view witness asserts source caught X159 at factor1 and configured view while physical X159/184 differs. Original Editor job `d28204f25ea24d21bb0471b7099263f3` passed the entire existing throw class 8/8, including immediate, following full tick and local snapshot replay cases. No full Scene Play/EXE proof or source decision-reader activation.
