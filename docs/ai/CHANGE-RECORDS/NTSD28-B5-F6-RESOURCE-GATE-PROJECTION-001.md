# NTSD28-B5-F6-RESOURCE-GATE-PROJECTION-001 — F6 active projection and registration inheritance

<!-- CHANGE-RECORD
id: NTSD28-B5-F6-RESOURCE-GATE-PROJECTION-001
status: VERIFIED
change-kind: TEST_FIRST_PRODUCTION_GATE_PROJECTION
code-path: Assets/NTSD/Scripts/Simulation/Host/SimulationTickDriver.cs
code-path: Assets/NTSD/Scripts/Simulation/Core/SimulationWorld.cs
code-path: Assets/NTSD/Scripts/Simulation/Runtime/SimulationRegistryModule.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B5F6ResourceGateProjectionEditorTests.cs
authority: NTSD 2.8-Logan GameSession28 submit_native_function_key/project_process_globals_to_entities and BattleWorld28 spawn inheritance; EXE B1E13AE1, closure 39DDDA15.
evidence: WORLD-RULES-CARRIER-VERIFIED / AUTHORITY-ACCEPTED-TOGGLE-PROJECTION-AND-SPAWN-INHERITANCE-READ / TEST-FIRST-COMPILE-RED-CS1061-X3 / FIRST-FOCUSED-FIXTURE-PREPARING-X2-CORRECTED / COMPILE0 / FOCUSED5 / RELATED69 / LATE-OPOINT-ISOLATED1 / NTSD28-BROAD655 / SELFCHECK-PASS-2026-09-05T13:49:24Z / CONSOLE0 / SCENE-UNCHANGED / LEDGER-PASS
-->

> 状态：`VERIFIED / ACTIVE_AND_REGISTRATION_PROJECTION_ALIGNED`

本包只连接F6 accepted toggle的active-slot投影与成功registration继承；不连接hit-resource production
transaction，不改变numeric rules、mode mapping、content或表现资源。

focused test先写入，fresh compile取得3个预期`CS1061`；没有范围外编译错误。
首次focused的2项失败来自测试Driver未从`Preparing`进入`Running`；夹具按既有生产测试更正为先
unpause激活再pause，不修改生产合同。

- accepted F6仅在gate实际变化时投影occupied runtime slots；locked/rejected F6不写。
- 成功registration在首个live tick前继承当前world gate；entity runtime与raw runtime同步。
- fresh compile 0 error；focused `956f84f2bd494b819eb2d9be373fc981` 5/5；clean related
  `1cf51a91de7e48f1ba1d0774c835a724` 69/69；精确NTSD28 broad
  `7ba4b7e76daf4628ae5258b2a15cb739` 655/655。
- related首轮的非NTSD28 `LateOpoint`共享池顺序失败已由isolated
  `6ac818e969b949c9a184ae6b98da7c67` 1/1排除本包回归。
- BattleRuntimeSelfCheck `2026-09-05T13:49:24Z` PASS；Console 0；Scene unchanged；Ledger PASS。
