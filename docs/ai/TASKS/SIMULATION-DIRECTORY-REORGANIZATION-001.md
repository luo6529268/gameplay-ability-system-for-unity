# SIMULATION-DIRECTORY-REORGANIZATION-001 Task Contract

## Objective

Reorganize the physical layout of \`Assets/NTSD/Scripts/Simulation\` by responsibility while preserving every runtime namespace, type, API, GUID and behavior.

## Authorized scope

- Move exactly the 142 C# files listed in the manifest and their matching meta files.
- Create Core, Host, Runtime, Passes, Stage, DataContracts, Diagnostics, Ai, Ecs and Lockstep responsibility folders.
- Update only source-path-sensitive Editor tests/tools.
- Refresh, compile and validate through the connected Unity Editor.

## Forbidden expansion

No gameplay, namespace, asmdef, API, Scene, resource, C++, Server, rendering or performance refactor. No merging/splitting algorithm files.

## Exit criteria

Manifest and GUID audit pass; compile/focused/full/runtime validation is no worse than the recorded pre-move baseline; governance evidence is current.

## Execution result

- Implementation complete: 142/142 destinations present, 142/142 content hashes and
  GUIDs preserved, no C# remains at the Simulation root, and old source-path references
  are zero.
- Unity compile is clean. The pre/post 20-test path matrix is identical at 18 pass plus
  the same two external package-version failures.
- A clean full EditMode run executed 1585/1585 with five task-external failures; two
  Play/Stop cycles remained Scene-clean and emitted zero warning/error entries.
- Fresh SelfCheck reproduced only the existing central-render P4 failure.
- The Change remains governance-blocked solely because the repository-wide validator
  reports missing metadata in unrelated
  `CLIENT-CONTENT-FRAME-STRUCTURE-ALIGNMENT-001.md`; this task does not edit that record.
