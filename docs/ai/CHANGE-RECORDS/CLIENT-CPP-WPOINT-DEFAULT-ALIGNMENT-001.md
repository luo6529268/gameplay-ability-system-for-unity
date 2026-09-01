# CLIENT-CPP-WPOINT-DEFAULT-ALIGNMENT-001

<!-- CHANGE-RECORD
id: CLIENT-CPP-WPOINT-DEFAULT-ALIGNMENT-001
status: VERIFIED
code-path: Assets/NTSD/Scripts/Animation/LF2FrameData.cs
code-path: Assets/NTSD/Scripts/DatParser/Runtime/Utils/Lf2DatConverter.cs
code-path: Assets/NTSD/Scripts/Simulation/SimulationBattleBufferModule.cs
code-path: Assets/NTSD/Scripts/Test/Editor/WPointDefaultAlignmentEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
authority: User standing authorization GOVERNANCE-S0-S9-STANDING-CLIENT-AUTHORIZATION-002; Queue0d2/0d3/0d4/0d7; C++ release WPointData/parse/live consumers.
evidence: VERIFIED / TEST_FIRST_ASSERTION_RED / UNITY_COMPILE_PASS / UNITY_FOCUSED_10_10_PASS / UNITY_RELATED_232_232_PASS / BATTLE_RUNTIME_SELFCHECK_PASS / WPOINT_CORPUS_SHA_PASS / SERVER_DUAL_CONFIGURATION_PASS / GOVERNANCE_VALIDATION_PASS / CLIENT_INTEGRATION_REQUIRED / STANDING_CLIENT_AUTHORIZED / FORMAL_MARKER_FALSE / S0_NOT_VERIFIED
-->

> Status: `VERIFIED / TEST_FIRST_ASSERTION_RED / UNITY_COMPILE_PASS / UNITY_FOCUSED_10_10_PASS / UNITY_RELATED_232_232_PASS / BATTLE_RUNTIME_SELFCHECK_PASS / WPOINT_CORPUS_SHA_PASS / SERVER_DUAL_CONFIGURATION_PASS / GOVERNANCE_VALIDATION_PASS / CLIENT_INTEGRATION_REQUIRED / STANDING_CLIENT_AUTHORIZED / FORMAL_MARKER_FALSE / S0_NOT_VERIFIED`

## Plan

Change only the WPoint CLR Kind default to zero and prove missing/explicit
converter values plus the existing local fallback. The Server Task is the
complete authority/scope/test/rollback contract.

## Test-first evidence

- New focused test and `.meta` were added before production changed.
- Fresh Unity compile has zero filtered `error CS` entries.
- EditMode job `80b1ca1bb755407baa88e88688e12c43` ran 10 cases；the direct DTO,
  missing-kind converter and local fallback assertions failed exactly at
  Kind=1 versus expected Kind=0. Explicit signed kinds and every other scalar
  default passed.

## Actual change

- `WeaponPoint.kind` initializer changed 1→0.
- Added focused and SelfCheck coverage for direct default, missing/explicit
  converter values and local fallback.
- Converter and buffer production files required no edit.

## Verification

- compile0 at 12:32:27/28；focused job `63aea56535a140e1a03a02aba02d2ee5`
  10/10 PASS；related job `113db6d11aea4d03b78170234810d0bb`
  232/232 PASS；fresh SelfCheck 12:36:26 PASS.
- WPoint corpus exact hash/byte contract PASS；Server Debug/Release builds and
  all four executable suites PASS in both configurations；workflow and both
  Ledgers PASS.
- Package is VERIFIED/CLOSED；formal marker false and S0 NOT_VERIFIED.
