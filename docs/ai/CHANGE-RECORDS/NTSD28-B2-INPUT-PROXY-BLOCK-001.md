# NTSD28-B2-INPUT-PROXY-BLOCK-001 — exact 0x21-byte proxy value block

<!-- CHANGE-RECORD
id: NTSD28-B2-INPUT-PROXY-BLOCK-001
status: FOCUSED_TEST_PASS
change-kind: SCRIPT_AND_TEST
code-path: Assets/NTSD/Scripts/Simulation/Input/NTSD28InputProxyBlock.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28InputProxyBlockEditorTests.cs
authority: NTSD 2.8-Logan simulation_tick_driver.cpp exact Entity+0xBE..0xDE copy and input_state.h semantic fields.
evidence: TEST-FIRST-CS0246 / UNITY-COMPILE-0 / FOCUSED-5-5-JOB-9BDA6326B2D145FD8E5E78E8D337D161 / SERIALIZED-BYTES-33 / OFFSET-LAYOUT-PASS / DEEP-COPY-PASS / EXCLUDED-SURFACE-PASS / WARM-4096-ZERO-ALLOC / CONSOLE-0 / PRODUCTION-UNCONNECTED
-->

> 状态：`FOCUSED_TEST_PASS / PROXY_BLOCK_READY / PRODUCTION_UNCONNECTED`

精确33-byte布局、copy/exclusion和4096次zero-allocation已由focused5/5验证；entity/AI/two-pass未接。
