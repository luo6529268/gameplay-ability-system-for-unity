# NTSD28-336B44-Q09-SAME-Z-CURRENT-PIXEL-001

Status: VERIFIED_SCOPED_CURRENT_SCENE_CENTRAL_GPU. Parent: current 336B44 alignment BATCH-05/Q09 P-04.

Authority: the formal root `NTSD2.8-Logan.exe` SHA-256 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3` and its playable `RenderSnapshotBuilder28::build` equal-Z ordering. The existing old-version four-image certificate applies only to Battle Scene SHA `471396E7...7B9`; the current saved Battle Scene is SHA `93448372...7BF60` at task start.

Objective: obtain a current-original-Battle-Scene, four-session CentralOnly natural-complete-tick pixel witness for the same-Z OID120/OID121 body overlap. The four variants `baseline`, `a`, `b`, `both` must use the same Scene SHA, tick, physical slots, frame/pic, RNG start and camera conditions. Independently compare all valid overlap pixels using the established bounded compositing oracle. Keep Legacy renderer, formal EXE pixel A/B, interpolation and other Q09 presentation exits open.

Owned script: only `Assets/NTSD/Scripts/Test/Editor/NTSD28Q09SameZFormalSpritePixelProbeEditor.cs`. Add an optional safe run ID to its existing request text (`variant@run-id`) so results and PNGs go to a new versioned artifact folder, without replacing old Temp JSON or historical images. A natural request without an ID must also receive a unique new folder. Check output existence before writing. The controlled branch, producer, camera setup, World cleanup and production runtime must remain unchanged.

Owned evidence: `artifacts/diagnostics/NTSD28-336B44-Q09-SAME-Z-CURRENT-SCENE-PIXEL-001/`. The pre-existing fixed `Temp/NTSD28_Q09_SameZNaturalTickPixel.request` is absent at task start. The four requests are newly created one at a time; the probe consumes and deletes each request file. Before each request, archive its exact UTF-8 text and hash in the new evidence folder, and record this automatic deletion in the run report. Do not delete or overwrite unrelated outputs.

Acceptance: original Editor imports the changed test script with 0 errors; each requested variant completes one original Scene Play and one natural full production tick, writes distinct JSON/PNG, restores temporary state or exits Play safely, and preserves Scene disk SHA. A four-image comparison must report the total eligible pixels, matches, counterexamples and limits. Failure or editor precondition must be recorded honestly. Run `Tools/Validate-ChangeLedger.ps1` and `git diff --check`. No DAT, resource, Scene, production code or non-battle edit.

Rollback: remove only this test-only run-ID routing in a separately authorized change if needed; preserve all historical images, JSON, and user edits. No Git restore/reset/clean or file deletion is authorized by this Task.

Result: four independent original Editor natural-tick captures under Scene SHA `93448372...7BF60` returned `PASS_CAPTURE`; all selected identities/tick/RNG and 4/4 cleanup gates matched. Independent whole-overlap pixel analysis found 82/82 fitting OID120 later, 44 exclusively supporting it, zero exclusively supporting the reverse. Raw captures, hashes, request deletion audit and protection evidence are in [the run report](../../../artifacts/diagnostics/NTSD28-336B44-Q09-SAME-Z-CURRENT-SCENE-PIXEL-001/scene-934483-20261002-01/REPORT.md). This Task is scoped verified; Q09/P-04 and the battle-alignment goal remain open.
