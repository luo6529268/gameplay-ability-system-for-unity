# R8-JOINTMOVE-PROBE-001 — physical movement Play probe

<!-- CHANGE-RECORD
id: R8-JOINTMOVE-PROBE-001
status: VERIFIED
code-path: Assets/NTSD/Scripts/Test/Editor/BattlePhysicalMovementPlayModeProbeEditor.cs
authority: J:\QQFile\NTSD2.4\ntsd_release\src\input\input_handler.cpp, src\entity\frame_advance.cpp and physics.cpp
evidence: USER-APPROVED-R8-WP01G-R03 / EXISTING-F1-PROBE-PASS / EDITOR-PROBE-CODE-WRITTEN / FRESH-COMPILE-0 / F2-PLAY-PASS-20260823 / EDITMODE-257-257 / FULL-SELF-CHECK-PASS-171719 / LEDGER-PASS
-->

> 创建日期：2026-08-23  
> 当前状态：`VERIFIED / TEST-ONLY`

## Requirement

R03 F2要求真实物理方向输入到movement/jump/landing的端到端证据。现有combo probe只记录combo/frame/cooldown，
没有position/velocity/FrameInputSet/landing checkpoint，不能用其PASS扩大证明movement链。

## Planned change

- 新增一个Editor-only菜单探针及Unity `.meta`；
- 通过`InputSystem.QueueStateEvent`驱动D/K；
- 只读取LastAppliedFrameInput与live character runtime；
- 输出`Temp/NTSD_R8_PHYSICAL_MOVEMENT_PLAY.result.json`；
- 不接入任何production Update/tick，不产生默认运行时分配。

## Protected boundaries

不改C++、production input/movement、DAT、30Hz、worker、FrameInputSet、render、capacity、T8、IL2CPP或服务器。

## Acceptance

见Task合同；代码写入后先compile，再运行真实Play，最后full self-check和治理校验。

## Rollback

仅移除本Record登记的Editor probe、meta与同一Change文档记录；不触碰其他用户修改。

## Actual changes / verification

- 新增Editor-only ASCII菜单探针和meta；
- 通过Input System queue D→D+K→D→release，不直接写runtime；
- 每tick记录FrameInputSet held/pressed/released、key/prev/cd、frame/state、位置/速度/朝向和ObjectCount；
- 输出`Temp/NTSD_R8_PHYSICAL_MOVEMENT_PLAY.result.json`；
- 16:57 fresh Unity scripts refresh/domain reload完成，Console project error为0；
- 首次menu在新脚本未import时失败，`scope=all`后Editor assembly 17:00:29 fresh生成；
- 首次ready Play报告在neutral tick773后停于RightQueued，FrameInputSet 300 ticks均neutral；同会话existing
  DRA也step1=-1，证明不是movement writer first difference，而是首个synthetic state注入时点问题；
- 已按existing combo成功模式，在menu调用时角色已neutral的情况下立即queue首个D，并增加Action enabled、
  Keyboard device与CurrentMoveInput诊断；production未改；
- 修订后live trace在tick1279取得Right pressed/KeyRight/CdRight5，tick1283取得canonical Defend64、
  KeyDefend/CdJump5并进入frame210，tick1286 frame212写DAT Vx8/Vy-16.3，tick1287 airborne Vx7；探针
  误等canonical Jump32而停在JumpQueued，现按项目既有crossed contract改为Defend64；
- canonical button修正后fresh compile为0 error；
- 修订后真实`NTSD_Battle` Play于tick1080取得Right edge，tick1084取得physical K对应的
  canonical Defend edge，tick1088首次airborne，tick1091 release，tick1108落地；
- jump writer在离地前写入DAT `jump_distance=8`与`jump_height=-16.3`，首个airborne样本为
  `Vx=7`、`Vy=-14.6`，X从baseline 775推进到final 949；
- `rightInputSeen/jumpInputSeen/airborneSeen/horizontalAirMotionSeen/landedSeen`全部为true，
  ObjectCount保持8→8，报告`Temp/NTSD_R8_PHYSICAL_MOVEMENT_PLAY.result.json`为PASS；
- final refresh后Unity Console compile error为0；
- focused EditMode job `a26ba1e3136f4c73b2a17c4bd105a866`：257/257 PASS；
- `BattleRuntimeSelfCheck`：2026-08-23 17:17:19 PASS；其负路径故意产生的诊断错误已清理，随后Console error为0；
- `Tools/Validate-ChangeLedger.ps1` PASS：78 records、93 governed code files；
- scoped `git diff --check`无whitespace error；新probe逐行trailing-whitespace检查PASS。
