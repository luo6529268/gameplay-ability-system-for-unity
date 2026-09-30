# R06 natural death-to-revival entry, first gate

Status: `PAIRED_SOURCE_NATURAL_REACHABILITY_PASS / ROOT_LFR_INITIAL_CONFIG_GAP / UNITY_NOT_RUN`. This is a scoped BATCH-04/Q07/R06 diagnostic, not an R06 or Q07 exit.

The root formal EXE SHA-256 remained `B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033`. The diagnostic compiled the corresponding playable/core sources and reused the previously verified Sasuke OID11 action110 versus OID87 selected-armor HP3 D/R/J input, seed `0x28A55A5A`, Stage23 and X500/X550. Its only initial gameplay difference from that fixture is target `revive_lives_30c=2`. The first compile mistakenly retained the old probe source and failed with duplicate `wmain`; the corrected compile and the revised 70-tick producer both exited 0. All compile arguments and the first failure are preserved.

| Completed tick | Paired playable target and event |
|---|---|
| 15 | HP3, action3, state1, lives2; no KO. |
| 16 | Selected-armor natural hit yields HP-73, action186/state12, attacker KO count1. |
| 27 | Action231/state14, render phase30, lives2. |
| 53 | Still action231/state14, render phase4. |
| 54 | Revival event status `revived`, action212/state4, HP3, lives2→1, render phase19. |
| 70 | Action212/state4, HP3, lives1; the post-revival window remains recorded. |

The unchanged root EXE cannot replay this **same initial lives2** fixture through its present LFR CLI. The documented `main.cpp` parser has action/facing/MP slot overrides but no revive-lives override. The root trace from the 70-tick LFR shows slot1 lives1 and HP-73/action231/state14/render phase0 at tick54, while paired-source slot1 revives then. Root replay 01 (365 ticks) exits 46 at source row150 with HP checksum expected503/got427; replay 02 (70 ticks) exits 46 at final header, logical slot0 offset276 expected0/got2. Both reports and the root tick12–90/12–70 traces are retained. A playback checksum or exit cannot certify rule parity when its initial life count differs; this is a diagnostic entry/configuration mismatch, not evidence of a Unity gameplay defect.

The controlled Lee peer-average complete-Driver test remains valid and was not rerun. No Unity code, Unity Editor test, production battle logic, DAT, Scene, ProjectSettings, mode Asset or nonbattle behavior changed here. R06 still needs a same-state Unity natural complete-Driver/Play trace, including the queued branch, and Q09 owns the final visual check. The next route may use the paired official playable source as the measured rule reference with the root-EXE replay limitation stated explicitly; it must establish a separate exact Task/Change before adding a Unity diagnostic. No per-character matrix is warranted by this one shared revival gate.

Validation: corrected source compile and both source runs exited0. `git diff --check` exited0 (line-ending warnings only). `Tools/Validate-ChangeLedger.ps1` under PowerShell 7 exited0 with 988 Records, 19 governed changed code files and this C++ file covered; the PowerShell 5.1 invocation first exited1 on Git's autocrlf warning, with both outputs retained. Root formal EXE hash is unchanged. Battle/Menu Scene, GameConfig and ProjectBattleModeConfig hashes remain respectively `2EE465D83C7169A0589447F437E37CAEFF3CC6F1BA6C3AAA55B8068F2B48B77A`, `785F828C4E64182BEA214E4794B198E3C82E3C42002FDADD3932A7E061B81E13`, `0527D737A1FA38FC56B51D00DC6E96A421D3C67222546368B147C2D074CB8EA7` and `B57CFEF32CC3ECE37AC98A4A1EF04FEB2FEE4EB3CC08466A4BFD32C1EDD85B82`; all four are Git-clean. Unity compile, NUnit, SelfCheck and Play were not run for this formal-only diagnostic.
