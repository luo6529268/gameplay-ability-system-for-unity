# NTSD28-B5-RECOVERY-STATUS-CONSUMERS-NO-STATS-001

<!-- CHANGE-RECORD
id: NTSD28-B5-RECOVERY-STATUS-CONSUMERS-NO-STATS-001
status: VERIFIED
change-kind: TEST_FIRST_SHARED_PRODUCTION_RECOVERY_CONSUMERS
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleRecoveryStatusWriter.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B5RecoveryStatusConsumerEditorTests.cs
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Passes/BattleEcsCharacterRecoveryPass.cs
authority: 用户Goal9明确授权无stats三timer消费；NTSD2.8-Logan battle_world.cpp:2165-2176/2216/2321/2355-2358及C25c-before-C25h正式调用链。
evidence: Step0 consumer-before-decrement confirmed; render phase mapped HitStun/HitStop. Actual RED69 executed with capped25 failures and null summary (>=25 observed); GREEN69/69, B5 original777+69=846/846, NTSD28 original247+69=316/316, fresh full SelfCheck14:36:14Z PASS. Runtime47/Editor104 warnings both0 errors; specified Scene unchanged. No natural positive producer for HP-double/MP-bonus, Editor forced-carrier evidence only. Three consumers only VERIFIED.
-->

[完整事前合同](../TASKS/NTSD28-B5-RECOVERY-STATUS-CONSUMERS-NO-STATS-001.md)声明新增writer/test/.meta、仅两consumer调用段、不变量、回滚、关闭契约和验收。
## 步骤0只读结论

Authority simulation_tick_driver.cpp:983 pre-display resource → 1018 step_frame_slot → 1025 advance_reaction_timers_slot。
battle_world.cpp:2165-2176/2216/2321/2355-2358读取timer；3739/3741递减mp-bonus/hp-double；3756-3757在body未skip且HP>0时递减weak。
Unity BattleLateEntityLifecycleModule.cs:119-143恢复 → 155-173 frame body → 178-181 reaction/status tail；437/439递减bonus/double，450条件递减weak。
结论：同实体本tick消费在递减前，与native一致；timer=1当tick有效、tail后0，focused必须实际证明。保持既有phase派发、body-skip、递减owner不变。

render_phase资格已建模，不标DEFERRED_RENDER_PHASE_ELIGIBILITY：NTSD28-B3-C25F-J-RENDER-PHASE-BINDING-001已唯一绑定render_phase_008到Runtime.HitStop/LF2Entity.HitStun（LF2Entity.cs:285-288）；两恢复方法现有HitStun<0早退等价无stats子集render_phase>=0，原样迁移至shared writer并覆盖negative/zero边界。
stats.bound例外继续明确后置，不添加新资格。

自然producer：BattleDamageWriter.ApplyConfirmedInputStatuses:257/278-284通过itr.weak给WeakTimer12C赋正值；另外两字段在Unity production只有载体/reset/copy/checksum及C25h decrement，未找到自然正值writer。Authority core也只见default/read/decrement，正值赋予来自source tests。三分支不存在可完整自然触发的生产链。
PLAY_NOT_PERFORMED_NO_NATURAL_PRODUCER：允许Editor定向强制carrier覆盖全部三路径、timer顺序和边界；不声称weak完全没有producer，不为Play添加production timer赋值或新资源。

## 原状与计划
两条recovery重复固定HP+1与普通PP公式，未消费三个已有carrier。先以生产LateEntity入口新增三路径focused RED，再提取shared HP/MP writer并接入，保留negative environment夹层。
无新状态/字段/生命周期owner，详细测试结果和实际改动随后追加。

RED前实际新增：NTSD28B5RecoveryStatusConsumerEditorTests.cs，23个显式golden scenario × Legacy/Ecs/Derived=69用例；由生产LateEntity入口驱动，Assert.Multiple同时检查HP/PP、三个timer尾部值、route与不变carrier。writer与两个production入口尚未修改。

测试兼容性纠正（production未改）：当前Unity NUnit无Assert.Multiple，新测试曾有1个CS0117，因此第一次过滤请求cbcdd34090a643efb92ed13e1f0182ec只返回0 tests，不能计RED。保留Temp/Goal9_EmptyDiscovery_BeforeTestCompile.json；仅新测试去掉Multiple wrapper，改项目支持的逐项Assert，不改golden值，待编译/实际RED。

实际RED：新程序集已包含该class后，jobb178d66e4d414aafb8ffd700fc8fdb18执行69项，status failed。MCP result=null且failure list capped25，实际只可报告>=25失败，不宣称推算45/24为实测。已返Legacy/Ecs失败显示weak HP期望100实101、weak phase3 PP期望50实55、double HP期望102实101、bonus PP期望153实152等；完整响应Temp/Goal9_Status_RED_Result.json。production仍未改。新测试兼容性修正后Editor build0 error/104 warnings。

RED后实际生产：新增无状态BattleRecoveryStatusWriter的HP/MP两方法；LF2Entity仅RunPreCollisionRecoveryPhase、ECS仅ApplyAuthorityRecovery接线，方法外字节不变。原HP→negative environment→MP顺序保留，原stepWait/period/no-op/render/cap/PP150/OID/公式保留；新增weak优先分支/普通MP抑制、HPdouble+2、MPbonus+1。无timer写入或新carrier。

实现内收敛（GREEN/既有回归运行前）：保留原两个调用端的普通MP资格块及exact +2F4 reader归属，只共享新增weak抑制与原公式/bonus增量。由此保留既有gate-owner源检查，不修改既有测试；语义与原计划相同，supersede步骤0中“迁移render门控到shared writer”的位置说明。render门控保持调用端，资格值/顺序不变。

GREEN：job2dec32214c0e4ae390562e00d62e26c8实际69/69 PASS，0 failed/0 skipped，1.216968s；结果Temp/Goal9_Status_GREEN_Result.json。包括三路径timer1消费后归0、weak的baseHP等号/高值不增、render负值门控、PP150/151/cap、timer0及负值基线。两套production后build均0 error：runtime47 warnings/7.32s，Editor104 warnings/4.59s。B5/NTSD28/full SelfCheck继续验收。

B5名称组groupNames=[NTSD28B5]，job96431f4c168549a38b9cc134f7f2f6eb，846/846 PASS、0 failed/0 skipped，179.5978656s；原777+新增69。Temp/Goal9_B5_Result.json保存完整逐项结果，未出现既有失败。随后运行NTSD28分类。

## 最终验收

- NTSD28分类categoryNames=[NTSD28]，job80197bea071d476187496f83dd042077，316/316 PASS、0 failed/0 skipped，8.9267995s；原247+新增69。Temp/Goal9_NTSD28_Result.json保存逐项结果，两组实际拆分均经结果数组核验。
- fresh full SelfCheck于2026-09-09T14:36:14Z返回PASS；Temp/Goal9_RecoveryStatus_SelfCheck.result（4 bytes）。无修正后既有失败，未触发硬停止。
- 实际build命令均为dotnet build <Assembly-CSharp.csproj或Assembly-CSharp-Editor.csproj> --no-restore -v:minimal -clp:ErrorsOnly；runtime47 warnings/0 errors/7.32s，Editor104 warnings/0 errors/4.59s，exit0。
- 指定instance 2022.3.62f3/NTSD_Battle，最终dirty=false/root13，Scene SHA D18E75F7920E12A1FEB13A8C23949042869E679B420A3073F7C21E4D4F4A2F11保持。
- PLAY_NOT_PERFORMED_NO_NATURAL_PRODUCER：weak存在encoded-hit writer，但HP-double与MP-bonus未发现自然positive赋值，无法自然完成三timer矩阵。按用户许可仅Editor强制carrier，通过真实LateEntity生产入口验证，不伪造producer、不进入Play、不启动第二实例。
- 最终脚本SHA：writer14064BF306E9C3AA210E5A68896975CA456F193CF03124FEAFD116C91C8CC6DE；test887EDE85485BF5F965BB3E88C3D72E4AFEEE8D0532067C302AF8862C2F759044；entity918EDA5FDCFE7CFB1B136336A1EB16B8F9DD4EFBF512D81D6BF6E0FA69489B37；recovery8D154426DC3CBA48DD0E9C8FC159A449FC3B92C2623C2A1C321ADD27DF13495A。
- 最终逐路径SHA审计：Temp之外恰11个事前声明文件变化（2既有consumer、2新增脚本及2meta、本Task/Record、Ledger/STATE/总表）。未动其余用户工作/资源/Scene/测试或Authority。
- 仅无stats/chp/cmp三个consumer被验证；完整mode/stat/schema/drain/post-resource仍后置，不是整个C25恢复域已对齐。

当前状态：VERIFIED / NO_STATS_THREE_CONSUMERS_ONLY / FOCUSED_69_OF_69 / B5_846_OF_846 / NTSD28_316_OF_316 / FULL_SELFCHECK_PASS / BUILDS_0_ERROR / SCENE_UNCHANGED / PLAY_NOT_PERFORMED_NO_NATURAL_PRODUCER / GOAL10_USER_HOLD

最终治理验证：Tools/Validate-ChangeLedger.ps1 PASS，438 Records / 376 governed code files / 531 warnings，exit0；Temp/Goal9_ChangeLedger_Validation.log。授权既有文件git diff --check exit0，仅Git换行提示；新增脚本已实际编译并进入GREEN与两组回归。
