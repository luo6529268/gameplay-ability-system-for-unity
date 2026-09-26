# Q07 / D-024 non-character structural epoch: scoped result

2026-09-26. Formal paired playable source fixture: `NTSD28-Q07-HITFA5-FULL-SESSION-SOURCE-001`; original Unity Editor complete-Driver fixture: `NTSD28-Q07-HITFA5-UNITY-DRIVER-001`. This package changes only `SimulationWorld.CharacterInputAll` and `SimulationAiDecisionModule.TryPrepareUnifiedExecutionPass` for the non-character producer's occupancy change and re-publication hard-failure contract. It does not change DAT values, Scene, camera, or nonbattle logic.

| Original Editor job | Result | Interpretation |
| --- | --- | --- |
| `be8e617162f5453580317ad9dcf52a60` | 5/5 passed | Existing injected postpublication failures still enter hard-breach path. |
| `f352278574024504a36125344b3f12c3` | 1 failed | Positive indexed OID219 fixture passed prior epoch hard-error point; first remaining difference at completed tick1 is child action formal `1`, Unity `0`. The `PostCommitHardBreachCount == 0` assertion ran first. |
| `3a94155d834a4562bc2cc0aa1153d0a7` | 1/1 passed | No-birth group3 control compared all eight formal source rows. |

Earlier `407963d594ab42ad882763e4f08afa45` recorded the same child-action first difference in the initial joint job. `708760...` used a wrong test name and selected zero tests; it is excluded from acceptance. Intermediate job `99b27...` failed an inappropriate end-of-whole-tick `PublishedEpochIsCurrent` assertion; later tick passes can alter occupancy after character input. The assertion was removed, with its failure retained as diagnostic history. Editor reload/compile was requested through the existing MCP bridge on the original project, without a second Unity instance.

**Boundary:** This package is `RUNTIME_PENDING`, not Q07 parity. The positive case's eight-tick source comparison, root EXE same-world setup, natural Battle Scene Play, and other occupancy producers are not established. Next work is a separate read-only pass-order/child-frame first-difference diagnosis before changing production action logic. No OID-specific exception is authorized.
