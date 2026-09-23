<!-- CHANGE-RECORD
id: NTSD28-Q07-WINDOWS-PLAYER-MANIFEST-V2-001
status: VERIFIED
change-kind: BATTLE_PLAYER_CONTENT_MANIFEST
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q07WindowsContentBuildProcessor.cs
authority: D-023 formal runtime bytes and current staged LoganRuntime file set
evidence: staged 1371 files/46883057 bytes all formal-exact, existing processor frozen at 1343/46594829
-->

# NTSD28-Q07-WINDOWS-PLAYER-MANIFEST-V2-001

Before/after, exact paths, dependencies, invariants, acceptance and rollback are declared in the matching Task. The original 1343-row manifest is a historical proof and must not be rewritten. This code change only updates the Windows battle-content postbuild copy gate to the new frozen current-staged manifest; it does not change gameplay, Menu callbacks, formal J: resources or Scene serialization.

Pre-change read-only audit: the old manifest rows all remain staged; exactly 28 non-meta paths were added (7 decoded DAT, 21 VFS PNG), each same-path formal SHA-256 exact. Full 1371-file staged/formal comparison reported 0 missing/size/hash differences. The existing postbuild processor would reject the current source set before producing an accepted Player.

Written: the new frozen v2 CSV contains 1371 uniquely sorted rows and 46,883,057 bytes; its SHA-256 is `5D3D9C302726541D3AFA477BC347F857E247FB047ECDB6ACAFF3857AA5EBE73F`. Independent verification checked each row against staged and formal same-relative-path SHA/size with zero differences. The only script diff changes three existing constants: manifest path, expected count and expected bytes.

Original Editor refresh/compile completed with `Tundra build success` and idle domain reload; no `error CS` in the fresh log tail. Separate Menu-first Player build/postprocess has now succeeded with 0 build errors, and independent destination verification matched all 1371 manifest rows and 46,883,057 bytes with no extras. Scoped diff check and Change Ledger validator passed. See `artifacts/diagnostics/NTSD28-Q07-MENU-FIRST-WINDOWS-PLAYER-001/REPORT.md`. This closes only the v2 content-copy gate; Q07 overall remains open.
