# NTSD28-B3-C25C-E-CURRENT-MP-BINDING-CORRECTION-001 — current MP binding correction

<!-- CHANGE-RECORD
id: NTSD28-B3-C25C-E-CURRENT-MP-BINDING-CORRECTION-001
status: VERIFIED
change-kind: TEST_FIRST_CONTRACT_CORRECTION
code-path: Tools/NTSD28Parity/EntityFieldContract.cs
code-path: Assets/NTSD/Scripts/Simulation/Diagnostics/NTSD28UnityEntityRawCapture.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28UnityEntityRawCaptureEditorTests.cs
authority: NTSD 2.8-Logan EntityState28 current_mp shared by native action cost/hit resource/C25c/C25e; Unity production native resource writers use LF2Health.PP/NTSDEntityRuntime.PP.
evidence: RED-4AD5958D-1-OF-3-EXPECTED / CURRENT-MP-PP-UNIQUE-BINDING / TOOL-BUILD-0-0 / TRACE-SELFTEST-21-OF-21 / RAW-SELFTEST-5-OF-5 / FORMAT-PASS / CONTRACT-SHA-1AE87A06 / UNITY-COMPILE-0 / JOINT-03A01FFB-49-OF-49 / NTSD28-91F21DA3-401-OF-401 / SELFCHECK-07-42-52-PASS / NO-RUNTIME-BEHAVIOR-CHANGE / SCENE-UNCHANGED / EDITOR-NOT-PLAYING / POST-CLEAR-CONSOLE-0
-->

> 状态：`VERIFIED / CURRENT-MP-PP-BINDING / RAW-PROJECTION-CORRECTED / NO-RUNTIME-BEHAVIOR-CHANGE`

## 改前事实

- `EntityFieldContract`与raw exporter把currentMp指向`Runtime.MP`。
- Unity生产native action transaction、damage writer、AI resource reading、C06 dead normalization与C25b newborn均使用`Health.PP/Runtime.PP`。
- 初始化通常双写同值，既有raw baseline无法区分；需要MP/PP互异红灯。

## 计划

先增强raw focused test令MP/PP互异并取得红灯，再只改contract文本与raw projection；运行工具与Unity回归，不触碰runtime producer。

## 实际改动

- `EntityFieldContract`把`vitals.currentMp`的Unity binding从`NTSDEntityRuntime::MP`纠正为`NTSDEntityRuntime::PP / LF2Health::PP`。
- `NTSD28UnityEntityRawCapture`改为输出`runtime.PP`；focused fixture固定MP200/PP173，防止未来又因同值初始化退回错误字段。
- 未修改NTSDEntityRuntime字段、action/damage/C06/C25b production writer、snapshot/checksum或战斗行为。

## 验证

- red `4ad5958dd753433790d6ac3b29e4e12d`：旧投影输出200而预期173。
- tool build0/0，self-test21/21、raw5/5、format PASS；contract SHA`1AE87A06DD3F1C8F24A089555BBDC3151F60465E8EA67DC0C56768F54CD7EDF4`。
- Unity compile0；joint `03a01ffbe5244ba2bab386dae359ae7d` 49/49；broad `91f21da3839541418140493307e65b7f` 401/401；SelfCheck 07:42:52 PASS。
- 无Play需求；Editor not playing，Scene SHA`0D74E174D37AF673CB717C699D13F3D115C65EDD332E373B6673757D5E323D77`、mtime`2026-09-04T13:12:45.1526434Z`不变；post-clear Console0。
