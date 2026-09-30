<!-- CHANGE-RECORD
id: NTSD28-Q07-CHIYO-PUPPET-AD-NATURAL-001
status: VERIFIED
change-kind: CODE
code-path: Tools/NTSD28Q07Diagnostics/chiyo_control_itr_lfr_probe.cpp
authority: formal root EXE B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033 and paired playable input_routing/HitCandidateBuilder28
evidence: docs/ai/TASKS/NTSD28-Q07-CHIYO-PUPPET-AD-NATURAL-001.md
-->

# NTSD28-Q07-CHIYO-PUPPET-AD-NATURAL-001

Before script edit: previous HP150 natural Uj produces OID854 but never asks Chiyo to enter the author frame254 that may exchange sentinel-height kind8 controls with her puppet. The formal DAT and playable call path identify `hit_ad` on Chiyo punch frame60→254, then Chiyo420/421 BDY Y8504500 and puppet standing ITR Y8504500→400→407. Only one new ordinary-input continuation will be added to the existing opt-in standalone diagnostic; default prior run remains unchanged. No new gameplay behavior is proposed. Expected side effects are a new, non-overwriting source CSV/LFR and root replay report/trace. Acceptance, first-link stopping rule, protected-file scope and rollback are in the Task Contract. Q07, BATCH-04 and the total goal remain open.

First script edit and run1: optional `attack_defend` applies two sampled attack ticks40–41 then defend42–43; unchanged default remains 90 ticks. GCC compile of 28 core/four playable plus this tool passed. Source 120 ticks naturally produced Chiyo420 at tick42, with MP400→250 and existing OID854; the puppet stayed frame109 during Chiyo420/421 and never reached400/407. Formal root EXE LFR report `passed=true/failureCode=0/nativeParityClaim=false`. This is an author-frame timing miss, not a production first difference. Before a second script edit the Task is amended to allow exactly one measured shift: attack54–55, defend56–57, when run1 observed puppet frame0 at55 and frame5 at56. Prior run1 CSV/LFR/root report remain immutable; no matrix or forced frame is authorized.

Second script edit and verification: retained default and first option, added only `attack_defend_54` selected timing. GCC 15.1 compile `-std=c++17 -O2 -municode -lz` with 28 core/four playable sources passed. Run2 ordinary input reaches Chiyo420 at tick56, puppet400 at57 and407 at68–69; root EXE LFR `passed=true/failureCode=0/nativeParityClaim=false`. Source CSV vs root trace seven groups (Chiyo action/MP, owner0 OID419/OID854 counts, puppet slot:action, frame407 indicator, input phase) each 120/120 with zero selected-field first difference. Script changed only the standalone Tools diagnostic, not formal/Unity gameplay, DAT, Scene, Assets, camera or nonbattle code. Earlier run1 negative and older HP150 90tick route remain unchanged. Only formal natural reachability is verified; original Unity Battle Play and control-only candidate result are pending under a separate Task. [Evidence](../../../artifacts/diagnostics/NTSD28-Q07-CHIYO-PUPPET-AD-NATURAL-001/ACCEPTANCE-20260928.md). Rollback: revert only this opt-in tool change under repository rules; retain evidence.
