# CLIENT-CPP-ITR-PARSER-DEFAULTS-ALIGNMENT-001

<!-- CHANGE-RECORD
id: CLIENT-CPP-ITR-PARSER-DEFAULTS-ALIGNMENT-001
status: VERIFIED
code-path: Assets/NTSD/Scripts/Animation/LF2FrameData.cs
code-path: Assets/NTSD/Scripts/DatParser/Runtime/Utils/Lf2DatConverter.cs
code-path: Assets/NTSD/Scripts/Test/Editor/ItrParserDefaultsAlignmentEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
authority: User standing authorization GOVERNANCE-S0-S9-STANDING-CLIENT-AUTHORIZATION-002; Queue0cy/0cz/0d0; C++ release ItrData/parse_itr.
evidence: VERIFIED / TEST_FIRST_ASSERTION_RED / UNITY_COMPILE_PASS / UNITY_FOCUSED_6_6_PASS / UNITY_EDITMODE_212_212_PASS / BATTLE_RUNTIME_SELFCHECK_PASS / S0_WITNESS_PASS / EXISTING_LOCKSTEP_PASS / ITR_CORPUS_SHA_PASS / SERVER_DUAL_CONFIGURATION_PASS / CLIENT_INTEGRATION_REQUIRED / STANDING_CLIENT_AUTHORIZED / FORMAL_MARKER_FALSE / S0_NOT_VERIFIED
-->

> Status: `VERIFIED / PRE_CHANGE_SCOPE_RECORDED / TEST_FIRST_ASSERTION_RED / UNITY_COMPILE_PASS / UNITY_FOCUSED_6_6_PASS / UNITY_EDITMODE_212_212_PASS / BATTLE_RUNTIME_SELFCHECK_PASS / S0_WITNESS_PASS / EXISTING_LOCKSTEP_PASS / ITR_CORPUS_SHA_PASS / SERVER_DUAL_CONFIGURATION_PASS / CLIENT_INTEGRATION_REQUIRED / STANDING_CLIENT_AUTHORIZED / FORMAL_MARKER_FALSE / S0_NOT_VERIFIED`

## Plan

Correct only absent Z width and one-value action-pair defaults at the mutable
DTO/converter boundary, then run the exact Server contract test matrix. No
immutable Itr seam or runtime collision change.

## Actual result

Changed only zwidth default0→15 and one-value pair secondary→0. Fresh Unity
compile0；focused job `6dab2590ca2b42469bf08670687b6e51` 6/6；final job
`53db60de214d49c982be616e17518057` 212/212；SelfCheck12:09:45 PASS；
corpus SHA and Server dual PASS. Queue0d1 verified；marker/S0 unchanged.
