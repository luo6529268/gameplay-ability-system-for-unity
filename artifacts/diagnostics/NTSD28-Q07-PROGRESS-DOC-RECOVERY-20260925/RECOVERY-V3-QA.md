# Progress-document recovery candidate v3 QA (2026-09-25)

The three **live** originals are unchanged from the previously preserved corrupt snapshots: their SHA-256 values equal `alignment-corrupt.bin`, `handoff-corrupt.bin`, and `state-corrupt.bin` respectively. Each consists of a short readable UTF-8 prefix followed by NUL bytes; first NUL offsets remain 446, 297 and 240, with local last-write time 2026-09-25 01:17:40. Neither Git HEAD nor the reconstruction candidates can reproduce all lost uncommitted wording exactly.

Candidate v3 is built additively from the prior review candidate, `RECOVERY-DELTA-V2-20260925.md` (recent Q07/Q10 evidence) and `RECOVERY-DELTA-V3-20260925.md` (nine active Change IDs missing from the prior handoff). No live file, previous candidate or binary snapshot was overwritten. All v3 candidates contain zero NUL bytes:

| Proposed replacement target | New candidate | SHA-256 |
| --- | --- | --- |
| `Assets/NTSD/Docs/ntsd28-logan-vs-unity-battle-alignment.md` | `alignment-recovery-candidate-v3.md` | `26DFD936E9BD3B6D2B65C9B4D4768F9376B20BEEE5896226CFB275E3C02B2805` |
| `Assets/NTSD/Docs/CODEX-CURRENT-HANDOFF.md` | `handoff-recovery-candidate-v3.md` | `132842A12ECB7E96DD17372E6E5506C385BCC98398E72FFD1B7E9566E7C812EC` |
| `docs/ai/STATE.md` | `state-recovery-candidate-v3.md` | `72A2F107B99BD761E37D183E4947FDF49D3480989FA91D491863504E8456DE54` |

A fresh text-level audit of current `docs/ai/CHANGE-RECORDS/*.md` metadata found **272 active** IDs. All 272 occur in both proposed v3 STATE and handoff; all current Record IDs occur in `docs/ai/CHANGE-LEDGER.md`. This is a coverage check only. It does not validate metadata semantics, recover every lost post-commit edit, or replace the real `Tools/Validate-ChangeLedger.ps1` run after installation.

The remaining required decision is explicit approval to replace precisely these three damaged originals with the three named v3 candidates, preserving the `*-corrupt.bin` snapshots. Repository `AGENTS.md` requires approval before overwrite. After approval, re-check source and target hashes, install only the approved candidates, run the validator and inspect the Q01–Q12 status table plus recent Task/Change links. Until then script changes requiring active Ledger/STATE/handoff registration remain governance-blocked; the battle-alignment goal remains active.
