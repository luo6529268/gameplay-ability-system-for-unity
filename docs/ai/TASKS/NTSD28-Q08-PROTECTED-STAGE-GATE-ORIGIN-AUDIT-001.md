# NTSD28-Q08-PROTECTED-STAGE-GATE-ORIGIN-AUDIT-001

Status: `VERIFIED_STATIC_SCOPED`. Parent: BATCH-04/Q08, D-024/Q07 protected OID122/123 boundary return. This is a read-only caller/field-origin audit; no script, DAT, image, Scene, asset or nonbattle change.

Authority: root `NTSD2.8-Logan.exe` SHA-256 `B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033` and paired playable `simulation_tick_driver.cpp`, `battle_world.cpp`, `game_session.cpp`, `main.cpp`, `scenario28.cpp`, and `game_session.h`. Unity counterpart: `LF2Entity.ApplyPreFrameXBounds`, `SimulationStageRenderModule`, `BattleEcsCharacterPreFrameBoundsPass`, and project-owned `ProjectBattleModeConfig`.

Question: is the extra mode1 `selected_stage_participant_gate_178914 == 1` arm a live menu-selected production condition, or only an explicit scenario/test input in the inspected current source? This matters because the user excludes original background and both mode DAT classes and keeps the project's own stage/mode system. Do not invent a DAT value or add a production knob without a proven selected-route requirement.

Exit: exact writer/caller census, default and menu projection trace, Unity current behavior, precise limits, and next action recorded in `artifacts/diagnostics/NTSD28-Q08-PROTECTED-STAGE-GATE-ORIGIN-AUDIT-001/REPORT.md`. This audit does not certify executable GUI behavior under every input, and does not close Q07, Q08 or D-024. No Unity tests were needed because no implementation changed.
