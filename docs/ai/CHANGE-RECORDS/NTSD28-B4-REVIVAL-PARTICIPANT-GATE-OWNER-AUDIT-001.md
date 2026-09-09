# NTSD28-B4-REVIVAL-PARTICIPANT-GATE-OWNER-AUDIT-001

<!-- CHANGE-RECORD
id: NTSD28-B4-REVIVAL-PARTICIPANT-GATE-OWNER-AUDIT-001
status: VERIFIED
change-kind: GOVERNANCE_ONLY
code-path: none
authority: NTSD 2.8-Logan BattleWorld28::advance_native_revivals, advance_reaction_timers_slot, SimulationTickDriver28 C07 floor handoff, playable direct participant defaults and object-spawn revival fields; Unity LF2Character/LF2Entity/BattleRespawnModule gates/producers; EXE B1E13AE1, closure 39DDDA15.
evidence: Authority C25 arm is type0 primary slot/lives/render-phase only and C07 entry is active HP<=0 render1..4 state14; branch priority is queued continuation only when lives<2 and nextHP>0, terminal transient removal vs primary defer when lives<2, otherwise normal floor-backed revival. Unity has two KillCount/team5/slot frame-arm gates and one C07 gate, wrong branch selection, primary deletion, plus later continuation/floor/RNG differences. Direction-B has 235 state14 frames. Direct production revival default producer is absent: pool reset writes 0/0/0 while Authority defaults 1/0/0. Five routes defined; no code/content/Scene/Authority changes.
-->

> 状态：`VERIFIED / GOVERNANCE_ONLY / KILLCOUNT_NOT_REVIVAL_AUTHORITY / THREE_ENTRY_POINTS_SPLIT / DIRECT_DEFAULT_PRODUCER_MISSING / FOUR_RUNTIME_ROUTES_DEFINED / PRODUCTION_HELD`

Authority不读KillCount；Unity三个旧gate、branch priority、primary/transient终态、queued与normal细节已拆分。
先补direct默认revival字段producer，再实施gate/branch、queued、normal及exit trace；本审计无生产改动。
