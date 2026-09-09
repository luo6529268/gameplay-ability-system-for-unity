# NTSD28-B2-PROXY-INTEGRATION-AUDIT-001 — production proxy crosswalk

<!-- CHANGE-RECORD
id: NTSD28-B2-PROXY-INTEGRATION-AUDIT-001
status: VERIFIED
change-kind: GOVERNANCE_ONLY
code-path: NONE
authority: NTSD 2.8-Logan input_state.h, input_routing.cpp, simulation_tick_driver.cpp, battle_world.cpp and exact proxy/timer tests; current Unity input/runtime/AI owners.
evidence: CURRENT-PREVIOUS-7-MAPPABLE / EDGE-7-PHYSICAL-MAPPING-CLOSED / DEFEND-REENTRY-CDDEFENDLOCK-CLOSED / NATIVE-COMBO-10 / UNITY-LEGACY-COMBO-9-NOT-ONE-TO-ONE / PROXY-CONTROL-3-MISSING / TAIL-MISSING / AUTHORITY-TWO-PASS / UNITY-AI-COMBO-INTERLEAVED / IMPLEMENTATION-SPLIT-5 / NO-SOURCE-CHANGE
-->

> 状态：`VERIFIED / CROSSWALK_CLOSED / IMPLEMENTATION_SPLIT_DEFINED / GOVERNANCE_ONLY`

production proxy不能机械复制旧runtime；exact carrier、combo bridge、control lifecycle、AI/proxy两遍和joint
trace已拆为五包。本记录不表示proxy已接入。

