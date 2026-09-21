<!-- CHANGE-RECORD
id: NTSD28-Q06-DESTROY-POOL-OWNER-001
status: VERIFIED
change-kind: DESTROY_POOL_OWNER
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q06DestroyPoolOwnerEditorTests.cs
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2LivingObject.cs
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2Character.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q06DestroyPoolOwnerPlayProbe.cs
authority: Ordered battle shutdown/World-owned pool contract; existing Free owner-before-detach implementation; platform removed-source measured failure.
evidence: Pre-change; independent owner RED pending. Platform motion evidence retained separately.
-->

# NTSD28-Q06-DESTROY-POOL-OWNER-001

准确范围、不变量、副作用、验收和回滚见同ID Task。先新fixture RED，生产未改。三个同职责override必须共同核验，不只修平台对象。

RED job9b993b58 actual3/3FAIL: source slot removed but originalpool.ActiveCount1 instead0, control world remains1. Archived red-9b993b58. Exact three overrides now capture owner before DestroyEvent/Destroy/renderer detach and release capturedowner at existing tail. No order change, no new cleanup event. Compile/focused pending. During turn HEAD advanced externally to72ecf16e; preserved current working tree, no agent commit/reset.

Final focused jobe7476287 actual3/3PASS; exact scope and remaining Renderer/closure gate: artifacts/diagnostics/NTSD28-Q06-DESTROY-POOL-OWNER-001/FOCUSED-RESULT.md. Not VERIFIED.

Stable package check: mixed candidate actual3/3PASS and single fresh fullSelfCheck 2026-09-21T12:14:34.368890+00:00 PASS; artifacts/diagnostics/NTSD28-Q06-PLATFORM-MIXED-CANDIDATE-001/UNITY-RESULT.md. Existing relevant evidence reused. Renderer/ordered closure not yet completed; do not mark production package VERIFIED.

Pre-change Renderer extension: existing test fixture gains static VerifyForPlay/type path using actual LF2ObjectPointFactory.MaterializeObjectForStructuralWriter, logic-only toggle false and Renderer assertions. New exact PlayProbe polls request only in ready paused battle, tests types0/1/5 in independent worlds, checks scene checksum/global renderer borrowers before-after, then existing Q05 replay/ordered-shutdown request. Test cleanup Free remaining renderer entities before World shutdown; repeated Destroy asserted before cleanup. No new runtime owner, no scene edit. Production unchanged. Existing three tests retain logic-only path.

Final bounded acceptance VERIFIED. Actual Renderer3 first+reenter, isolated ownership/idempotence, scene checksum/borrowers unchanged; both Q05 restore/zero-residual/two-frameStopped closures PASS. See ACCEPTANCE.md/play-first/play-reenter. Stable SelfCheck reused. Does not certify platform/frame motion Renderer semantics or all Q06.
