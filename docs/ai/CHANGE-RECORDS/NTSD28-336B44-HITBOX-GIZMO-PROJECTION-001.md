<!-- CHANGE-RECORD
id: NTSD28-336B44-HITBOX-GIZMO-PROJECTION-001
status: VERIFIED
change-kind: CODE
code-path: Assets/NTSD/Scripts/Tools/NTSDHitboxGizmos.cs
code-path: Assets/NTSD/Scripts/Animation/Character/BruteForceSceneQuery.cs
code-path: Assets/NTSD/Scripts/Animation/Rendering/BattleCentralRenderSystem.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSDHitboxGizmoProjectionEditorTests.cs
authority: user P2 repeated knockback report; existing production collision/projection and presentation contract; verified canonical cached Sprite origin is unmaintained
evidence: docs/ai/TASKS/NTSD28-336B44-SAGE-P2-REGRESSION-20261006.md
-->

# NTSD28-336B44-HITBOX-GIZMO-PROJECTION-001
Pre-change scope: debug drawing only. Reuse actual prev2 collision frame and BruteForce projected rectangles, not cached SpriteX/Y/Z; share current central display motion delta without new interpolation or lifecycle state.
Unity original: Gizmo Frame.D/PS.GetBodyVolumes/GetItrVolumes/PS.sx/sy and raw ScreenPixelToWorld; canonical battle character keeps legacy origin zero, proven focused1/1. Actual hits use other production geometry; no evidence to change damage or motion rules.
Exact methods: NTSDHitboxGizmos DrawEntity/DrawBdyBoxes/DrawItrBoxes/DrawWeaponPickupBoxes/DrawVolume; BruteForceSceneQuery readonly Itr and pickup volume adapters reuse existing LocalRectWorldRect; BattleCentralRenderSystem readonly current handle/body-display-offset diagnostic accessor; focused geometry test.
Expected side effects: only displayed debug boxes now follow actual collision geometry and approved projection/camera/display offsets for all entities. All runtime writes, allocations, pass order, data, scale1.5, Scene and user edits remain unchanged. No new manager/queue/cache; central accessor uses existing lifecycle-owned motion data, validates current world/handle, no singleton creation.
Validation: narrow geometry cases with stale/poisoned caches, repeated X/Z/Y movement and frame/facing changes, identity/fixed view, body/itr/pickup; compile, original Battle repeated knockback and display evidence. Actual collision does not acquire new rules.
Rollback: exact before bytes/hunks in operation; protect concurrent changes, no broad restore.
Operation: docs/ai/FILE-OPERATIONS/NTSD28-336B44-SAGE-P2-REGRESSION-EDIT-20261006/RECORD.md (gizmo-before-manifest).

## Scope correction before integration

The implemented first fix intentionally draws the actual un-interpolated collision geometry. The proposed central display-motion accessor is cancelled: BattleCentralRenderSystem remains unmodified, and display interpolation must not become logical geometry. The original plan above is retained as the pre-change record. Production changes are limited to NTSDHitboxGizmos and readonly BruteForceSceneQuery adapters, with focused geometry tests. Agent write scope is these three files only. Root review, original Editor compilation, focused tests and repeated-knockback scene evidence remain pending; no claim of a changed actual collision rule is made.

Root integration review confirmed no production writes/pass changes and no central modification. Full original Editor import compiled the declared scripts after a diagnostic-only CS1061 repair. First imported two-case test run failed NUnit discovery validation because Category contained prohibited hyphens; root changed the new category token to NTSD28_336B44. No test body ran in that attempt, retained gizmo-imported-result01.json. The earlier zero-case run was also retained; neither is geometry PASS. Next runs exactly the same two names, then original saved Battle P1/P2 Naruto as serialized in the user scene, with three natural P2 recoveries.

Original Editor job ee8daabb8a45440cb042afbf6c9b4c13 executed exactly two named geometry cases, total2/passed2/failed0 (gizmo-green-result01.json). Scene evidence remains pending. Generated C# build after full source import and one-line probe repair passed with301 warnings/0errors; original Editor read_console filter errorCS returned0 (probe-repair-console.json).

Scene evidence correction: original saved Battle run04 `scene-e2c9120d493a42568ec2db3a526f6d98.json` passed555 complete Driver ticks, three real P2 uppercuts/airborne/landing/wall recovery, checking debug body geometry against current collision frame and projected X/Y+Z at successive ticks. Separate common boundary repair resolved actual source/view drift; this Change itself changes drawing only. Eleven-stage zero-residual shutdown, clean Scene/unchanged bytes passed. Scoped VERIFIED for logical debug geometry; central file still unmodified, display interpolation/GPU pixel alignment and pair-specific eligibility are not newly claimed.
