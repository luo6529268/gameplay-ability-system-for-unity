<!-- CHANGE-RECORD
id: NTSD28-Q07-CPOINT-Z-FORMAL-CONTENT-REACHABILITY-001
status: VERIFIED
change-kind: GOVERNANCE_ONLY
code-path: NONE
authority: formal NTSD2.8-Logan.exe B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033 and paired playable combat_records/BattleWorld28
evidence: artifacts/diagnostics/NTSD28-Q07-CPOINT-Z-FORMAL-CONTENT-REACHABILITY-001/REPORT.md
-->

# NTSD28-Q07-CPOINT-Z-FORMAL-CONTENT-REACHABILITY-001

Before: the D-024 position-writer inventory retained a possible difference for nonzero `cpoint.z` during held-position settlement, but current formal DAT reachability had not been established. Unity code and DAT are unchanged by this audit.

After: all 405 formal decoded DAT files were inspected as complete `cpoint:`…`cpoint_end:` blocks. The 4,019 blocks contain zero explicit `z:` tokens; `CombatRecords` defaults absent `z` to zero. Thus the nonzero branch is dormant for the inspected shipped DAT corpus. This is static current-content scope only, not a proof for dynamic overrides or future content. The exact report is linked above. No script, asset, Scene or authority file was modified; no Unity compile/Play was needed for the read-only classification. Q07/D-024 remain open. Rollback is limited to this package's documents and follows repository approval rules.
