# Q07/D-024 project stage-depth clamp: focused GREEN

Formal authority: root NTSD2.8-Logan.exe SHA-256 `B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033` and paired playable `BattleWorld28::clamp_type0_stage_depth`. Project exception: fixed full-view proportional battle coordinates, project-owned map bounds and walkable polygon; original background/mode DAT excluded. No DAT, Scene, map, camera or config asset changed.

Original Editor PID11944, saved `NTSD_Battle.unity`, project Stage physical Z237..760, World depth scale `1152/730`. Independent full Driver first-tick Play requests used source-mapped Han/Lee at source Z100 and Z600. The two reports retain the before/after source and view positions:

| Case | Start source / view Z | First-tick source / view Z | Expected view | Ordered stop / borrowers | Scene hash |
|---|---:|---:|---:|---|---|
| [near](q07-project-stage-edge-near-z100-20260929-green-a.json) | 100 / 157.808219 | 150.182292 / 237 | 237 | true / 0 | unchanged |
| [far](q07-project-stage-edge-far-z600-20260929-green-a.json) | 600 / 946.849315 | 481.597222 / 760 | 760 | true / 0 | unchanged |

The two [prechange RED reports](ACCEPTANCE.md) had near view374.005 and far view946.849. Production now leaves project `Stage.ZMin/ZMax` at physical 237/760 and converts to source-rule limits only for Stage-Z/PreFrame through the existing World `BattleSpatialProjection`; explicit source fixtures retain 180/350 semantics. A physical/source origin bit flows through the worker request and lockstep core capture/restore/checksum; local core/aggregate/checksum versions move 15→16, 31→32, 35→36.

Validation: generated `Assembly-CSharp-Editor.csproj` build 0 errors; original Editor Tundra build success/0 C# errors. Original Editor `NTSD.Test.Editor.NTSD28SourceStageDepthEditorTests` 16/16 after final version bump (`114fddd3028447e98e77656184c970ca`), covering three Stage-Z modes, both PreFrame modes, explicit source control, project near/far, missing source, noncharacter ±1 source pixel and worker/core/checksum carrier. `NTSD.Test.BattleSimulationWorkerBoundaryEditorTests` 20/20 before version bump (`1535631bb3da45a193cbd2b95d0214c5`); only version constants changed afterward, and the carrier test was repeated in the final 16/16. Full snapshot restore new test 1/1 (`5efcc9aae3014148b31f6c018830371f`) and prior aggregate-version rejection control 1/1 (`d96c9c68dbc74033a6872b0b9a567833`). An initial build had one missing `world.` qualifier, corrected before Unity compile; an earlier worker test filter selected zero tests, then the correct namespace ran 20/20. No full character matrix was run.

Protected SHA-256 after both Play reports: Battle Scene `3A089236328ACAE1510F8A831B77D4895CC34028DDCDEBE542BEF0DA8EC235ED`; Menu `DD6A48A37FB8CEA9CD8A1F7738964A719E007A42F0FBB54FB48BBD0B723B9DC3`; map `F7B5E4A44CAC05480D1CA6F67ABF623531264C1C96725D7FDD23DA50C8E60C08`; GameConfig `0527D737A1FA38FC56B51D00DC6E96A421D3C67222546368B147C2D074CB8EA7`; ProjectBattleModeConfig `B57CFEF32CC3ECE37AC98A4A1EF04FEB2FEE4EB3CC08466A4BFD32C1EDD85B82`.

Scope: this closes the project physical Stage-Z/PreFrame clamp carrier only. AI source-depth comparisons, stage-wave/results reserve, state405 center writes, no-map GameConfig unit audit and broader Q07/BATCH-04 exits remain open per the parent Task. No claim of complete battle parity follows from this package.
