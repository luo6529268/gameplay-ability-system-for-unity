# CLIENT-FORMAL-KERNEL-OPOINT-VALUE-SEAM-001

<!-- CHANGE-RECORD
id: CLIENT-FORMAL-KERNEL-OPOINT-VALUE-SEAM-001
status: VERIFIED
code-path: Assets/NTSD/Scripts/Simulation/BattleObjectPointValue.cs
code-path: Assets/NTSD/Scripts/Simulation/BattleObjectPointValueAdapter.cs
code-path: Assets/NTSD/Scripts/Animation/LF2FrameData.cs
code-path: Assets/NTSD/Scripts/DatParser/Runtime/Utils/Lf2DatConverter.cs
code-path: Assets/NTSD/Scripts/Animation/Character/LF2ObjectPointModule.cs
code-path: Assets/NTSD/Scripts/Animation/Character/LF2ObjectPointFactory.cs
code-path: Assets/NTSD/Scripts/Simulation/BattleLogicObjectPointRuntime.cs
code-path: Assets/NTSD/Scripts/Animation/Editor/CharacterFramePreviewWindow.cs
code-path: Assets/NTSD/Scripts/Test/Editor/FormalKernelObjectPointValueSeamEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
authority: User standing authorization GOVERNANCE-S0-S9-STANDING-CLIENT-AUTHORIZATION-002; Queue0cs/0ct; C++ release OPoint live path.
evidence: VERIFIED / TEST_FIRST_MISSING_SEAM_RED / UNITY_COMPILE_PASS / UNITY_EDITMODE_52_52_PASS / BATTLE_RUNTIME_SELFCHECK_PASS / S0_WITNESS_PASS / EXISTING_LOCKSTEP_PASS / GOLDEN_CORPUS_SHA_PASS / SERVER_DUAL_CONFIGURATION_PASS / FORMAL_MARKER_FALSE / S0_NOT_VERIFIED
-->

> Status: `VERIFIED / PRE_CHANGE_SCOPE_RECORDED / TEST_FIRST_MISSING_SEAM_RED / UNITY_COMPILE_PASS / UNITY_EDITMODE_52_52_PASS / BATTLE_RUNTIME_SELFCHECK_PASS / S0_WITNESS_PASS / EXISTING_LOCKSTEP_PASS / GOLDEN_CORPUS_SHA_PASS / SERVER_DUAL_CONFIGURATION_PASS / CLIENT_INTEGRATION_REQUIRED / STANDING_CLIENT_AUTHORIZED / FORMAL_MARKER_FALSE / S0_NOT_VERIFIED`

## Plan

Introduce the exact immutable eight-int formal OPoint content value and adapt
only at legacy task/preview boundaries. Preserve source order, duplicates,
invalid-entry retention, list-over-alias precedence and all current spawn
behavior. The full path/symbol/invariant/test/rollback contract is in the
same-ID Server Task/Change Record.

## Actual files and verification

The test-first generated-project probe failed with exactly 18 expected
diagnostics for absent `BattleObjectPointValue`/adapter symbols. The declared
value/adapter/frame/converter/consumer/preview/test/SelfCheck implementation is
now written. Latest generated-project compile passed with 0 errors；the exact
fixture passed 6/6 under Unity's bundled Mono including frozen corpus SHA；Server Debug/Release and both four-suite
runs passed. Fresh Unity import/compile produced zero `error CS` entries；official
EditMode job `c1f48ca2ef7b4c4c9d1d395b19131ff2` passed 52/52, including S0
witness and existing lockstep；fresh `BattleRuntimeSelfCheck` wrote PASS at
11:12:51. Queue0cu is verified while the formal marker and S0 remain unchanged.

Fresh C++ release revalidation confirms the exact eight-field schema and parse
order in `include/dat_parser.h` / `src/data/dat_parser.cpp`, source-order and
local facing decode in `src/entity/frame_advance.cpp`, actual field/ownership
consumption in `src/entity/collision.cpp`, and release Makefile participation.
No `objectId` or OPoint `dvz` exists in that live contract.

Fresh bridge diagnosis on 2026-08-31 confirms that this is an external Editor
session gate rather than a Client-authorization gate: Unity PID `29796` is
responsive, but it owns no listening TCP endpoint; `Library/MCPForUnity/RunState`
is empty; `Assembly-CSharp.dll` / `Assembly-CSharp-Editor.dll` still predate the
new package sources; and the current Codex task exposes no `unityMCP` tool. The
request-file SelfCheck path cannot import scripts and would execute the stale
assembly, so it is not valid evidence. The narrow recovery action is to start
the MCP For Unity Stdio session in the existing Editor (do not launch a second
Editor); after port `6401` returns, continue with refresh/compile and the exact
fixture list in the Server Task Contract. The standalone focused runner was
rebuilt after the diagnostic attempt and remains 6/6 PASS.

Correction/supersession: the no-listener snapshot above was transient. The
user-confirmed active panel matched a fresh `127.0.0.1:6401 LISTENING` probe and
strict `WELCOME UNITY-MCP 1 FRAMING=1` handshake. That same bridge completed
refresh/compile, Test Runner and SelfCheck, so no session restart was required；
the earlier diagnosis must not be treated as the final state.
