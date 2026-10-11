# Read-boundary incident — sidecar root review

Date: 2026-10-11 Asia/Shanghai
Task: NTSD-H10-AUDIO-PRODUCER-COVERAGE-REVIEW-20261011
Executor: read-only sidecar 01a126a6-b5e6-7540-ab88-b3e4e014767c (Luna); main agent responsible for integration.
Status: RECORDED / FURTHER_RESTRICTED_READS_STOPPED / EXCLUDED_FROM_EVIDENCE

The task explicitly prohibited active Q06 method-body reads. The sidecar reported that broad contextual searches and file-head reads displayed restricted implementation bodies. This is an actual read-scope violation, not merely a proposed read and not cured by excluding its results. No code/resource changes or Unity runs accompanied it. Main stopped further reads and requested this audit from already executed tool-call history only; no source was re-read to prepare the audit.

Confirmed displayed ranges, as reported from that history:
- Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleNativeWeaponPieceWriter.cs:5–15,41–45,88–105,139–144. Active Materialize/Spawn/InitializeBirth body portions. Command: rg -n -C 2 "InitializeBirth|Materialize|WeaponPiece|nativeWeaponPieceSpawn" <exact file>.
- Assets/NTSD/Scripts/Animation/Character/LF2ObjectPointFactory.cs:1–180 from Get-Content ... | Select-Object -First 180; additional contextual output452–510,537–544,665–669 from broad rg -n -i -C 3 over Assets/NTSD/Scripts using weapon/piece/wpoint patterns. These included active OPoint/factory implementation.
- Assets/NTSD/Scripts/Animation/Character/LF2WeaponPointFactory.cs:17–30,34–48,49–56 from the same broad contextual search. Implementation portions, not just declarations.
- Assets/NTSD/Scripts/Simulation/Stage/SimulationStageWaveModule.cs:1–260 requested in a multi-file Get-Content -First260 batch; further contextual output approximately78–268 from a broad stage value/type rg -C4. Active stage runtime body read; exact Q06 attribution is not established by the tool history, so do not assert every method here is Q06, but it exceeded the assigned metadata/entry boundary.
- BattleRandomWeaponDropModule.cs was also read in broader implementation ranges1–104 and190–320. This was not the explicitly named Q06 writer/factory domain, but goes beyond metadata/signature evidence; only buffer constants are retained by the main root matrix.

The exact broad query reported:
```powershell
rg -n -i -C 3 "weapon.?piece|weaponpiece|piece.?weapon|random.?weapon|weapon.?random|weapon_strength|weapon_hp|weapon_drop_hurt|weapon_strength_list|wpoint" "Assets/NTSD/Scripts" -g "*.cs"
rg -n -C 4 "class BattleStageCampaignData|class BattleStagePhaseData|class BattleStageSpawnData|BattleStageCampaignValue|BattleStagePhaseValue|BattleStageSpawnValue" "Assets/NTSD/Scripts" -g "*.cs"
```

Incomplete command details remain incomplete: the sidecar described some range reads as Select-Object ... and some broad searches with an abbreviated path array; this record does not reconstruct or fabricate missing arguments. The ranges above are its explicit audit of already displayed output, not independently re-opened source.

Excluded conclusions: current writer OID/action/slot/materialization implementation, current factory admission/World binding/strength injection, and current StageWave progression/lifecycle. The report does not use these to prove producer completeness or safe subset. Historical ChangeRecords for piece -1→999/OID0 are independent H-level evidence; main read those records and the metadata types separately. StageSpawnTaskConfigurator's spawn.Id→task.opoint.oid is a non-Q06 direct field mapping independently verified by main at lines85–89, not evidence from StageWave.

Artifact scope correction: request-sites.json / request-coverage-matrix.json are the main agent's direct-call index and allowed value-domain analysis, not a claim that every sidecar stayed within scope. Their “body not expanded” statements describe the specific indexed call analysis; the task as a whole DID encounter the reads listed here. In root-dependency-matrix.json, “bodies excluded” means excluded from accepted evidence, not that no sidecar displayed any bodies. Final TASK-RESULT.json records restrictedBodyReadOccurred=true.

No additional restricted reads, no inferred new permission, and no source edits to compensate. Future metadata work should use an exact file/range allowlist and no broad context (-C) over active runtime trees. Any further named Q06 read needed for unresolved cue values must be independently decided; this incident does not authorize it.

