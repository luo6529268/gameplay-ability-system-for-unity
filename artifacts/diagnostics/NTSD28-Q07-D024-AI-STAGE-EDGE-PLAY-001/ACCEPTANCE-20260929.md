# Q07/D-024 AI stage near-edge Battle Play acceptance

Status: `VERIFIED_SCOPED_AI_STAGE_NEAR_BOUNDARY`. This verifies the project-map AI stage-depth supplier and strict near-edge decision in original Unity Play. It does not close all AI behavior, the far-edge branch, stage wave/state405, Q07/BATCH-04 or the master alignment goal.

Authority: root NTSD2.8-Logan.exe SHA-256 `B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033`, paired playable `native_ai.cpp::step_abnormal_target` at the strict `target.position.z < stage_bounds.z_near + 10` test. User D-024 project map/full-view exception applies. The World `BattleSpatialProjection` is the sole ratio/anchor source; project physical near237/far760 corresponds to source integer near151/far482 for the AI comparison. No original background or mode DAT was used.

The diagnostic extension to `NTSD28D024AiChildScenePlayProbeEditor` ran in the original Editor PID11944, Unity 2022.3.62f3, through the production Menu prewarm, AppManager additive Battle, formal Logan content identity, AI Lee versus human Naruto, and 76 complete `SimulationTickDriver.StepOneTick` ticks in each independent Play. Once AI acquired runtime target slot0, the probe set source X500/520 and Z160 or Z161/155 with `world.SpatialProjection` and set human hit stop8, so the abnormal-target depth branch could be observed on tick76. The reports record target slot0 before/after and require hit stop>2 after the tick. The generic `aiTargetSlot` field belongs to the older child-spawn route and remains -1 in this diagnostic; `stageEdgeAiTargetBefore/After` are the relevant target fields.

| Independent report | Human source/view Z | AI source/view Z | Rule near/far | Target before/after | AI depth input | Result |
|---|---:|---:|---:|---:|---|---|
| `q07-ai-stage-edge-z160-20260929-a.json` | 160 / 252.493150684931 | 155 / 244.602739726027 | 151 / 482 | 0 / 0 | down1, up0 | PASS |
| `q07-ai-stage-edge-z161-20260929-a.json` | 161 / 254.071232876712 | 155 / 244.602739726027 | 151 / 482 | 0 / 0 | down0, up1 | PASS |

Both reports are in `artifacts/diagnostics/NTSD28-USER-D024-AI-CHILD-SCENE-PLAY-001/`. Both used battle mode0 and the same formal content identity; both ended with ordered shutdown complete, Menu restored and pool borrowers0. The five protected files retained SHA-256: Battle Scene `3A089236328ACAE1510F8A831B77D4895CC34028DDCDEBE542BEF0DA8EC235ED`, Menu Scene `DD6A48A37FB8CEA9CD8A1F7738964A719E007A42F0FBB54FB48BBD0B723B9DC3`, SunagakureMap `F7B5E4A44CAC05480D1CA6F67ABF623531264C1C96725D7FDD23DA50C8E60C08`, GameConfig `0527D737A1FA38FC56B51D00DC6E96A421D3C67222546368B147C2D074CB8EA7`, ProjectBattleModeConfig `B57CFEF32CC3ECE37AC98A4A1EF04FEB2FEE4EB3CC08466A4BFD32C1EDD85B82`.

Validation layers: generated `dotnet build Assembly-CSharp-Editor.csproj --no-restore -v:q -p:WarningLevel=0` exit0 / 0 errors (22 preexisting MSBuild assembly-reference warnings); original Editor rebuilt its Editor assembly after the edited source, reloaded, and entered both Play sessions; no second Editor or computer-use. Previous focused Stage+AI tests passed30/30 in original Editor and are not reclassified as Play. No full root EXE versus Unity same-map visual A/B was run; this project-map exception uses the paired playable rule and controlled Unity inputs. Next dependent Q07 packages remain stage-wave/results reserve, state405 center and GameConfig-only units.

Final script governance: `Tools/Validate-ChangeLedger.ps1` exit0 (1026 records, 66 governed dirty code files), focused `git diff --check` exit0. No commit or push.
