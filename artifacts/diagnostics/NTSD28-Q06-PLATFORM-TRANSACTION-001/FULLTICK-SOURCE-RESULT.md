# Platform full tick source evidence

Status: SOURCE_THREE_REPRESENTATIVES_CAPTURED; Unity full tick/replay pending.

The existing native build completed with exit 0. Its manifest identifies formal EXE B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033 and playable source closure 07CD47A0623F23D2C439E0E85EABF2ED10F8EAE8FC7D70DDB8396C704B3D778F.

Executed platform_fulltick.exe without arguments once and with --fulltick twice. Default output is byte-identical to the original pinned 21-row witness (SHA256 9671B8D3E3BA1D48E58735EBDBE729BD291154435C8C21320DBB32799D77F9B2). Both fulltick outputs are byte-identical (4623 bytes; SHA256 3079EE34B013170CDDFAA0738CBC5E070A56826C9BF80921057963207442139F).

Three representatives: nominal, fractional_itr_negative, left_facing_motion. Each directly executes two SimulationTickDriver steps without a preceding manual candidate or motion invocation. Six lifecycleSuccess results are true. A local inspection asserted three rows, two ticks each, initial no platform link, link20 on both ticks and expected first/second tick X coordinates.

| Case | Tick1 rider integer XYZ | Tick2 rider integer XYZ | Tick2 precise XYZ |
|---|---|---|---|
| nominal | 100,-20,250 | 102,-35,247 | 102.5,-35,247.5 |
| fractional itr -2.5 | 100,-22,250 | 102,-37,247 | 102.5,-37,247.5 |
| left facing | 100,-20,250 | 97,-35,247 | 97.5,-35,247.5 |

The initial stale-reference fixture is preserved in fulltick-initial and is not accepted parity evidence. The corrected opt-in fixture starts collision reference/link/shadow at zero and platform Y=-5 with dvy535. Physics precedes candidate collection: tick1 establishes the link, tick2 consumes linked motion. Integer and precise coordinates can differ intentionally; no rounding normalization is allowed to hide this.

This is an executable witness of the declared source closure, not a recording of the formal EXE. It covers the emitted position/reference/link/shadow/history fields, not all native state or visual fidelity. Unity integrated fulltick and snapshot replay remain unverified. Existing focused, SelfCheck and Renderer evidence is reused; no new full suite or Play run was required for this source-only extension.

Next: declare the exact new test symbols before extending NTSD28Q06PlatformTransactionEditorTests. Initialize both entities from before/platformBefore, configure AI before registration, use real RunReleaseTick twice and compare emitted covered fields each tick; restore initial snapshot and compare repeat checksums. Do not compare previousX/Z as implemented Unity carriers. Keep Q09 shadow consumer return and Q06 incomplete/Q07 not migrated.
