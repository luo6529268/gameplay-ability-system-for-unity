# NTSD28-PP-RECOVERY-LOW-THRESHOLD-INCLUSIVE-BOUNDARY-001

<!-- CHANGE-RECORD
id: NTSD28-PP-RECOVERY-LOW-THRESHOLD-INCLUSIVE-BOUNDARY-001
status: VERIFIED
change-kind: TEST_FIRST_PRODUCTION_BOUNDARY_CORRECTION
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B5OrdinaryCreditGate2F4CorrectionEditorTests.cs
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Passes/BattleEcsCharacterRecoveryPass.cs
authority: 用户2026-09-09 Goal8明确授权；Goal7 PartB只读审计；current NTSD2.8-Logan battle_world.cpp:2311-2321无stats默认regen_mp0子集inclusive150资格。
evidence: Test-first RED completed18 only three gate0/PP150 cases failed152-vs150; production two comparison tokens corrected; GREEN18/18, B5 names777/777 (existing759+new18), NTSD28 category247/247 (existing229+new18), fresh full SelfCheck13:48:37Z PASS. Both builds0 error (47/104 warnings), specified Scene unchanged. PLAY_NOT_PERFORMED_BOUNDARY_ONLY; inclusive150 boundary only VERIFIED.
-->

[事前Task](../TASKS/NTSD28-PP-RECOVERY-LOW-THRESHOLD-INCLUSIVE-BOUNDARY-001.md)声明完整范围、不变量、验证、停止与回滚。
原状：Legacy RunPreCollisionRecoveryPhase与ECS ApplyAuthorityRecovery用PP>=150阻断，原PP200 fixture未覆盖边界。
计划：test-first新增18边界用例后，只把两处>=改>，保留其余字节。
实际新脚本变更/验证将在取得后追加。无新模块，无shutdown事务/顺序变动。
当前仅新增CharacterRecovery_PpThresholdBoundary的18个TestCase，复用原InitializeCharacter/Derived类型；HP400/HPBound400、tick3，断言实际ECS/fallback路径与精确PP，finally unregister。两处production仍保持原>=，RED待执行。

## Test-first RED

2026-09-09，job eceb11262cbc4831b3c0610e62634f2c，具名边界group单跑一次，completed18，status failed，未capped的failures仅3项：Legacy/Ecs/Derived各自gate0 initialPP150 expectedPP152 actualPP150。其他15组无失败。MCP result=null，计数依据completed18与完整failure列表；不伪造summary。
原始响应：Temp/Goal8_Boundary_RED_Started.json、Temp/Goal8_Boundary_RED_Result.json。此前仅新增测试，production两文件SHA与本轮初始一致。
Test新增块移除后SHA1882B5A8CFC8D9BA8F5F9717DD86AF8D96205720AC743E2299C9768CC90899F4与原文件一致；RED前Editor build0 error/104 warnings。

RED后实际生产改动：LF2Entity.RunPreCollisionRecoveryPhase与BattleEcsCharacterRecoveryPass.ApplyAuthorityRecovery各一个比较符>=改>；字节逆替换等于原文件。所有cap/其他资格/公式/OID/phase未动，等待GREEN与回归。

## GREEN与构建

边界GREEN job59c6b1ccbad3407db9a2cd892bcd334f，18/18 PASS、0 failed/0 skipped，1.0914349s；结果Temp/Goal8_Boundary_GREEN_Result.json，三路径gate0/PP150均152，其余输入保持预期。
production修改后dotnet build Assembly-CSharp.csproj --no-restore -v:minimal -clp:ErrorsOnly：exit0，47 warnings/0 errors，10.38s。
production修改后dotnet build Assembly-CSharp-Editor.csproj --no-restore -v:minimal -clp:ErrorsOnly：exit0，104 warnings/0 errors，5.82s。
后续B5/NTSD28/full SelfCheck进行中，不提前声明完整验收。

B5名称组groupNames=[NTSD28B5]，job e26253a2b1b641609ad6628f484ec321，777/777 PASS，0 failed/0 skipped，192.7798471s。包含原759与本次新增18；完整结果Temp/Goal8_B5_Result.json。未重跑刷绿，NTSD28分类与SelfCheck仍待。

NTSD28分类categoryNames=[NTSD28]，job a47229bdf25e4e518fa1d787046830a1，247/247 PASS，0 failed/0 skipped，8.9928528s。逐条结果确认原229+新增18全部通过；Temp/Goal8_NTSD28_Result.json。已随后请求fresh full SelfCheck。

## 最终代码范围证明

- test原SHA1882B5A8CFC8D9BA8F5F9717DD86AF8D96205720AC743E2299C9768CC90899F4；现SHA598A2A1F0D2E7EB37BF8709498228F5683D82E3FB2F779AB76392041030EDE19；仅移除新增方法块即还原原SHA。
- LF2Entity原SHA9F23FB5870EF4755B397E15B523F53BF3032FCE37E5C32D0CEFD9C554902B215；现SHA053AA47392ABD24FB93BFCF954D65AE001E5BF4BD324CE3F7116F15BFE2DF547；只逆替换一个比较符即还原原SHA。
- RecoveryPass原SHA12C22D31C751D48B66D311ED66652B12CC0E812E50FB59ACA7EEA279C2404297；现SHAA62CF6ACA7DCFCCE71886C3912DE6092AF959B776D5C3D139F63809BD449DB0A；只逆替换一个比较符即还原原SHA。
- 没有修改其余资格条件、cap、公式、OID51/52、生产路径选择、timer/regen消费、Scene或任何资源。

## 最终验收与边界

fresh full SelfCheck于2026-09-09T13:48:37Z返回PASS，结果Temp/Goal8_PPBoundary_SelfCheck.result（4 bytes）。未出现修正后既有测试失败，没有触发硬停止条件。
指定Unity实例2022.3.62f3/NTSD_Battle，最终dirty=false/root13，Scene SHA D18E75F7920E12A1FEB13A8C23949042869E679B420A3073F7C21E4D4F4A2F11保持。不进入Play、不启动第二实例。
PLAY_NOT_PERFORMED_BOUNDARY_ONLY：用户明确免除纯数值门槛包Play要求且禁止进入Play；真实Editor三路径数值矩阵、完整相关组与full SelfCheck支撑仅该边界验收，不代表整个C25资源算法或表现/输入验证。
当前状态：VERIFIED / PP150_BOUNDARY_ONLY / RED_3_FAIL_15_PASS / BOUNDARY_18_OF_18 / B5_777_OF_777 / NTSD28_247_OF_247 / FULL_SELFCHECK_PASS / BUILDS_0_ERROR / SCENE_UNCHANGED / PLAY_NOT_PERFORMED_BOUNDARY_ONLY / GOAL9_USER_HOLD

最终审计：Tools/Validate-ChangeLedger.ps1 PASS，437 Records / 374 governed code files / 531 warnings，exit0；Temp/Goal8_ChangeLedger_Validation.log。授权文件git diff --check exit0，仅Git换行提示。
逐路径SHA比较本轮初始与最终工作树：Temp外恰8个授权文件变化（3脚本、本Task/Record、Ledger、STATE、对齐总表）；其他用户工作与Scene未变，无超范围diff。每个生产文件只逆替换一个比较符即还原原SHA，test移除新增块即还原原SHA。
PLAY_NOT_PERFORMED_BOUNDARY_ONLY，理由：用户仅授权纯数值门槛且禁止进入Play，以Editor边界矩阵/相关回归/full SelfCheck验证；不覆盖完整C25算法。
