<!-- CHANGE-RECORD
id: NTSD28-336B44-Q07-C017-ROOT-INPUT-001
status: VERIFIED
change-kind: CODE
code-path: Tools/NTSD28Q07Diagnostics/zero_hp_input_lfr_probe.cpp
authority: selected formal 336B44 root EXE and declared playable GameSession28/InputRouter28; formal OID2/OID14/OID222 DAT
evidence: docs/ai/TASKS/NTSD28-336B44-Q07-C017-ROOT-INPUT-001.md
-->

# NTSD28-336B44-Q07-C017-ROOT-INPUT-001

Created before code. Existing Unity C017 shared gate fixes have focused/full-tick proof but no root same-state proof. The new diagnostic runs bounded selected-source sessions and serializes real sampled inputs for root replay. It may investigate natural Kankuro poison-bomb contact but must report failed reachability honestly. No production rule or user resource is modified. Exact scope, invariants, validation, dependencies and rollback are in the Task. Original Battle Play remains a separate gate.

First diagnostic revision compiled exit0. Controlled source standing/terminal/positive cases ran12 ticks. The formal standing replay failed46 at its first HP checksum because LFR records base HP and playback initializes current HP from that base, not initial current HP0. This is not a Unity rule failure or valid same-state proof. Natural source poison35 reached contact tick7, HP0 tick19, standing tick27, Attack65 tick28, but attempted180 ticks crossed the result freeze at164 and recorder rejected the nonadvancing row. Preserve those outputs. Revision2 limits natural recording to40 ticks and aligns its initial base HP to current HP35, as required by the real LFR carrier; no formal code or DAT changes. Controlled HP0 formal claims remain unavailable through this carrier.

Revision2 compiled exit0, natural40ticks source/root8fields320/320 without difference, frozen formal replayexit0/passedtrue. Root directly prints tick7 applied damage30, tick19 HP0, tick27 idle0, tick28 HP0/currentAttack16/action65 input applied. Positive control12ticks96/96/rootPASS. Both controlledHP0 root cases remain genuine46 failures because currentHP is not serialized, and are not parity claims. Diagnostic responsibility and changed code only the declared tool; no Unity rule/DAT/source/EXE edits. Evidence: artifacts/diagnostics/NTSD28-336B44-Q07-C017-ROOT-INPUT-001/REPORT.md. VERIFIED applies only to the natural root trace witness; parent C017 and Unity Scene remain open.

Shared ChangeLedger validator PASS1054 records/14 governed codefiles, git -c core.safecrlf=false diff --check exit0. artifact-hashes.json verifies protected fourScene/config hashes and threeDAT formal/staged equality; current cpp SHA equals revision2 compiled snapshot. Scene Play is owned by its separate Record and not claimed here.
