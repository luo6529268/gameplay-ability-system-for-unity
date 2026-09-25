<!-- CHANGE-RECORD
id: NTSD28-Q07-SASUKE-NATURAL-PROBE-IDENTITY-001
status: VERIFIED
change-kind: Q07_SASUKE_NATURAL_PROBE_CURRENT_CONTENT_IDENTITY
code-path: Assets/NTSD/Scripts/Test/Editor/BattleComboPlayModeProbeEditor.cs
authority: current approved ProjectBattleModeConfig Asset and published Logan content identity; formal root release Sasuke skill trace
evidence: preserved preflight FAIL then original Editor unique physical L/D/J Play PASS; NATURAL-PLAY-POSTFIX-20260925.md
-->

# NTSD28-Q07-SASUKE-NATURAL-PROBE-IDENTITY-001

2026-09-25 scoped diagnostic exit: only the Sasuke preflight fingerprint selection and failure detail text in `BattleComboPlayModeProbeEditor.RunProbe` changed; DDJ, legacy and production logic remained unchanged. Original Editor recompiled and consumed a new unique request. `q07-sasuke-state15-postfix-currentmode-20260925.json` PASS with physical L/D/J at ticks2/4/6, PP500→400, frame264, four OID440/four stable IDs and formal chi.png pic0 binding; formal root gate true. Editor returned to Edit Mode/idle. The first preflight FAIL is preserved; both SHA values and unchanged Battle/Menu/GameConfig hashes are recorded at `artifacts/diagnostics/NTSD28-Q07-SASUKE-FORMAL-MANUAL-TRACE-001/NATURAL-PLAY-POSTFIX-20260925.md`. This Change's diagnostic identity task is `VERIFIED`; the Play probe does not capture state15 tick24 Vx, initial world, all continuous ticks, borrowers or pixels. Production velocity Change and Q07 remain narrower statuses/open.

Pre-edit result: `artifacts/diagnostics/NTSD28-Q07-SASUKE-NEEDLE-PHYSICAL-001/q07-sasuke-state15-postfix-20260925.json` is FAIL with `step1/2/3Tick=0`, no queued input and no observed child. The diagnostic `RunProbe` currently checks `ProjectModeFormalFingerprint` only for DDJ state audit and checks old `FormalFingerprint` for Sasuke, while the current catalog/mode publication in the same original Editor is `27CAE01489909C46A5145A5867988CCD8C10E20FEF165D847B7AC7A6DE2DE02D`. The generic failure text does not identify which operand failed, so the fingerprint is a source-level candidate, not a dynamic fact proved by this failed JSON. This is diagnostic identity drift, not a measured production skill defect.

Exact edit: in `BattleComboPlayModeProbeEditor.RunProbe`, select project-mode fingerprint for `q07SasukeNeedleAudit` as well as `q07DdjStateAudit`; retain all other preflight gates and add observed root/fingerprint/OID/hit_Fa/visual-key availability to the Sasuke failure message. Existing request poller, physical input order, output schema and production code unchanged. Expected side effect is only that this diagnostic can run against the currently selected formal content identity; if another operand fails, the new result will expose it. Task states acceptance, risk and rollback. Registered before editing this Editor script.
