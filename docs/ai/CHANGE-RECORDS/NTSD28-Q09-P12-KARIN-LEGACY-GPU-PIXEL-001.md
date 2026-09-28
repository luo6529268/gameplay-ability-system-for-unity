<!-- CHANGE-RECORD
id: NTSD28-Q09-P12-KARIN-LEGACY-GPU-PIXEL-001
status: VERIFIED
change-kind: CODE
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q09KarinState9997BattlePlayProbeEditor.cs
authority: formal NTSD2.8-Logan state9997 owner visible-body path and original Battle Legacy body witness
evidence: docs/ai/TASKS/NTSD28-Q09-P12-KARIN-LEGACY-GPU-PIXEL-001.md
-->

# NTSD28-Q09-P12-KARIN-LEGACY-GPU-PIXEL-001

Pre-script record. The current opt-in Karin probe stops at the real Legacy body renderer's enabled/flip/transform values. This test-only change will add same-tick offscreen World-camera body-on/body-off PNG capture and numeric pixel attribution, with exact restoration and old request modes preserved. Expected side effects are temporary RenderTexture/Texture2D allocations during diagnostic Play and new immutable artifact files only. The Task Contract defines scope, authority, acceptance and rollback. No script edit or runtime proof claimed yet.

CODE_WRITTEN / COMPILE_PASS: the existing optional request has a new `captureLegacyPixels` flag, gated to pre-boot LegacyOnly. After a natural state9997 child and `ForceRefreshPresentation`, the probe renders the current World camera at 1280x720 with the same tick/World, toggling only the child body SpriteRenderer between two unique PNGs. It counts RGBA pixel differences and bounds; `finally` restores the body enabled state, camera target RenderTexture and active RenderTexture. Default and prior Legacy requests do not run this branch. Generated Editor build `dotnet build Assembly-CSharp-Editor.csproj --no-restore` exit0, 197 warnings, 0 errors; log in the diagnostic folder. Original Editor compile, Play, independent PNG recomputation and Scene protection checks remain pending.

VERIFIED_SCOPED_LEGACY_CAMERA_GPU_PIXELS: original Editor compiled the new test script and ran exactly one pre-boot LegacyOnly Battle request. Natural full Driver tick8 OID314/state9997/owner8 had a real enabled right-facing body; same-tick offscreen World camera body-on/body-off PNGs differed at 1,245 RGBA pixels, X459–514/Y187–255 in Unity bottom-left coordinates. Independent PNG loop matched count and bounds exactly. Camera target/active RenderTexture and body enabled were restored; owned objects/slots/borrowers 4/2/2, pause, Editor non-Play/clean Scene and protected Scene/Asset SHA were stable. Raw JSON, PNG hashes, independent analysis and caveats are in [Acceptance](../../../artifacts/diagnostics/NTSD28-Q09-P12-KARIN-LEGACY-GPU-PIXEL-001/ACCEPTANCE-20260928.md). The visible color/alpha and formal root EXE same-view parity remain unverified; P-12/Q09 aggregate open.
