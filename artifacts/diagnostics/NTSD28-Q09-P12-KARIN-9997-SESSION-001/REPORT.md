# Q09/P-12 Karin state-9997 paired Session witness — 2026-09-28

Status: `VERIFIED_SCOPED_PAIRED_SOURCE_SESSION`. This witnesses the declared playable `GameSession28::step()` → `GameSession28::snapshot()` path with current formal content. It is not a formal root-EXE pixel capture, natural action-0 input route, Unity runtime first difference, or P-12/Q09 completion. The root EXE was only identity-hashed here: `B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033`.

The controlled setup is mode 0, selected background 23, seed 2833, Karin OID77 at player slot 0 and X20 or X500, far living opponent OID11 at X1200, camera 1333×730. The formal decoded/VFS roots both point to the current `resources/runtime`. Direct initial action417 produced no OID314 over five full steps: action417 remained through tick3 and moved to418 at tick4. Those two no-child TSVs and the first failed-root invocation logs are preserved. A separate authentic frame entry through Karin action415 (`next:417`) produced OID314/action50/state9997 at completed tick3 in both positions. There is no forced child action, state or owner in the diagnostic.

| Source facing | Karin X | Tick3 child physical facing | Snapshot facing | Snapshot left/top | Branch distinction |
|---|---:|---:|---:|---:|---|
| right | 20 | not recorded in v2 | 0 | 0 / 571 | right-facing ordinary and owner-relative clamp coincide here |
| right | 500 | not recorded in v2 | 0 | 461 / 571 | right-facing ordinary and owner-relative clamp coincide here |
| left | 20 | 1 | 0 | 0 / 571 | forced right-facing presentation proves the owner branch; left-edge clamps |
| left | 500 | 1 | 0 | 461 / 571 | owner formula gives `500 - centerx39 = 461`; ordinary left-facing placement would give `500 + 39 - width79 = 460` |

All successful cases reported selected `etc_mode=1`, child slot50/owner0, pic60 and width79 at first child tick. `render_snapshot.cpp` owner-relative branch sets facing0, clamps owner X against the 1333-pixel viewport, and uses owner Y for vertical placement; Karin's owner Y is0, Z650 and the child centerY79, so top571 is expected. The measured X20/X500 left positions and left-facing physical-to-sprite facing flip are consistent with this branch. The negative `etc_mode=0` or owner-out-of-range branch was not exercised, so its behavior remains source/test evidence only. A production change still requires an original-Editor same-state command/pixel first difference and a separate Task/Change for the project-owned etc-mode carrier and shared presentation exit. The approved fixed full-background camera must be accounted for when translating viewport clamp; no DAT or camera value is changed here.

Validation: g++ C++17 build against the 28 core sources plus paired playable `game_session.cpp` and `selection_flow.cpp` exited 0. Initial417 cases exited9 with retained no-child data; initial415 near/normal right-facing and left-facing cases exited0. The left-facing TSVs each have 13 rows and tick3 values asserted independently. Reinvoking an existing TSV returned exit3 and preserved its SHA-256. The original Editor PID11944 was later force-refreshed through its local MCP bridge (port6403) in idle/non-Play state; the request succeeded, domain reload completed, final editor state was idle/non-Play/not compiling or updating, and the error Console contained no C# compiler error (one MCP client-handler exit message). This refresh is environment readiness only: no Q09 Unity focused test, self-check, Editor Play, or formal root-EXE battle replay was performed for this Tools-only diagnostic. `NTSD_Battle.unity` and `NTSD_Menu.unity` remained outside this task's write scope; final Git status confirms no Scene diff.

Artifacts and SHA-256:

- Diagnostic source `Tools/NTSD28Q09Diagnostics/karin_state9997_session_trace.cpp`: `1393D8FCDAEEFF9A7752E81D2CD55A377DF791661908C01F8B42CC7E583FF1D0`.
- v3 diagnostic EXE: `ACF6EA8753EE3BD94B9333785C04C06F7248D38AC9F35166E841A9093D7FBF74`.
- X20 initial415 left TSV: `EC02B759C46766309F3C1EDF92C5569789F2D91EDE3345B59A0D61A44D6B86C5`.
- X500 initial415 left TSV: `7D11C6321906360F1B50F871F8E6DBFB321CA088AE4E69AE110ED09A739470DB`.
