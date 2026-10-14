# OP-NTSD-M12-BUILD-OUTPUT-DEVIATION-20261014

Status: PARTIAL
Task: NTSD-M12-FULL-IMAGE-COVERAGE-20261014
Change: NTSD-M12-FULL-IMAGE-COVERAGE-001
Executor: delegated C#selector worker (initialbuild), main (read-onlycapture)

Observed unplanned external build output at I:/build/bin and I:/build/obj. Worker reports initial Resolve-Path failed for not-yet-existing new diagnostics root; continued MSBuild with empty SelectorWorkerRoot property. Existing csproj composes /build output from that empty root. Observed22file creation timestamps 2026-10-14T01:19:36..38Z align with reportedbuild. No before snapshot exists for I:/build, so prior existence/overwrite cannot be established from timestamps alone; do not claim byte-preservation there. Current exact paths/bytes/SHA and observed-bytebackups: artifacts/diagnostics/M12-full-image-coverage-20261014/build-output-deviation-01/observed-manifest.json. This is retrospectiveincident evidence, not preauthorization. Root cause corrected by precreated absolute taskdiagnostic root in later builds. No delete/move/cleanup/restore performed or requested. Source/current resource protection checked separately. Exact initialcommand/expansion pending worker followup; appendwhenreceived.

Final observation correction: worker outer pwsh PID61080 executed on 2026-10-14 from 01:19:36.015Z to01:19:39.906Z, exit0. Resolve-Path failed nonterminating for the not-yet-existing selector-worker directory; actual argv was [dotnet,build,Tools/NTSD28ImageDependencyTests/NTSD28ImageDependencyTests.csproj,-p:SelectorWorkerRoot=,--nologo]. Full source command and22 observed file SHA rows are in selector-worker/REPORT.md. Before existence/overwrite remains UNKNOWN. No external cleanup was performed. Later builds use exact task-local absolute roots.