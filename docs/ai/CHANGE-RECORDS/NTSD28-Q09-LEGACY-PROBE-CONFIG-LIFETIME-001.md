<!-- CHANGE-RECORD
id: NTSD28-Q09-LEGACY-PROBE-CONFIG-LIFETIME-001
status: RUNTIME_PENDING
change-kind: CODE
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q09KarinState9997BattlePlayProbeEditor.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q09Etc998FallbackBattlePlayProbeEditor.cs
authority: observed original Editor owner9 CentralOnly preflight failure and GameConfig.Instance singleton setter contract
evidence: docs/ai/TASKS/NTSD28-Q09-LEGACY-PROBE-CONFIG-LIFETIME-001.md
-->

# NTSD28-Q09-LEGACY-PROBE-CONFIG-LIFETIME-001

Pre-script record. Both Q09 opt-in Legacy probes clone the saved GameConfig before Scene load; neither restores the static Instance. The next owner9 CentralOnly request observed a non-Asset LegacyOnly clone before any fixture, despite the saved Asset being CentralOnly. Declare the minimal test-only lifecycle correction in both existing scripts. No runtime repair is claimed yet. Exact acceptance, protection and rollback are in the Task Contract.

Post-edit scope: both declared Editor-only probes now recognize only their own identifiable `DontSave` LegacyOnly GameConfig clone. The pending opt-in request selects the saved CentralOnly Asset or a new in-memory LegacyOnly clone before Scene load; an `EnteredEditMode` callback restores the saved Asset when it finds that exact clone. It refuses unrecognized config objects. The generated Editor build passed with 0 errors/199 warnings; original Editor imported the scripts and a fresh CentralOnly request completed after the previous leaked clone, then a post-fix LegacyOnly request completed. Scenes and saved GameConfig/mode Asset hashes were unchanged, and Editor exited Play idle. The callback's post-exit static singleton identity has not been independently observed because a read-only MCP `execute_code` attempt failed at its external Mono invocation; this is an observation gap, not a reported production failure. Status remains `RUNTIME_PENDING` for that exact witness; do not repeat unrelated gameplay/GPU cases. [Acceptance](../../../artifacts/diagnostics/NTSD28-Q09-P12-KARIN-OWNER9-FALLBACK-001/ACCEPTANCE-20260928.md). Rollback remains a separately approved targeted test-only edit.
