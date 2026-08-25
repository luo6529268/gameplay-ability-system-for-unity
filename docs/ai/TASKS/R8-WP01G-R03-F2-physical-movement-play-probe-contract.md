# R8-WP01G-R03-F2 — physical movement/jump/landing Play probe

> 建立日期：2026-08-23  
> 状态：`VERIFIED / TEST-ONLY`  
> Change ID：`R8-JOINTMOVE-PROBE-001`

## Goal

新增默认不运行、仅Editor菜单触发的真实Play探针，记录物理D/K输入通过Input System、FrameInputSet和
角色runtime后产生的行走、起跳、空中水平运动与落地序列；不修改任何production input/movement逻辑。

## Scope

允许只新增`Assets/NTSD/Scripts/Test/Editor/BattlePhysicalMovementPlayModeProbeEditor.cs`及`.meta`，并更新
同一Change的治理文档。探针必须使用`InputSystem.QueueStateEvent`，不得直接写Runtime key/frame/velocity。

## Authority / Evidence

- C++ `input_handler.cpp:1555-1609,2360-2620`：current held/previous edge、walk/run/jump actions；
- C++ `frame_advance.cpp` / `physics.cpp`：frame transition、position integration与landing；
- Unity `CharacterInputModule`、`SimulationTickDriver.LastAppliedFrameInput`、`LF2CharacterStateResolver`、
  frame advance/physics passes；
- 已通过的R03 F1 DDJ/DRA真实Input System probe。

## Probe contract

1. 等待first live player处于ground neutral state；
2. queue physical D，要求FrameInputSet Right held/pressed和runtime KeyRight/CdRight；
3. queue physical D+K；按既有交叉输入合同要求FrameInputSet Right+Defend(physical K)、
   runtime KeyDefend/CdJump并进入起跳链；
4. 在首次airborne时记录DAT jump_distance/jump_height与actual Vx/Vy、Dir、X/Y；
5. K release后允许短暂保留D，再全部release；
6. require airborne horizontal displacement、nonzero authority-defined horizontal motion、最终Y/YInt=0落地；
7. 输出逐tickJSON及baseline/after ObjectCount；结束时强制release keyboard state和取消Editor callback。

## Verification

- fresh compile 0 error；
- real `NTSD_Battle` Play report PASS；
- report至少出现Right pressed、Jump pressed、airborne、landing四个checkpoint；
- Console无project error，probe cleanup完成；
- full self-check PASS；ledger validator与scoped diff-check。

## Stop conditions

- 当前角色DAT没有可用jump contract；
- first difference指向production gameplay；必须停止probe包并新建独立修复Record；
- 需要直接写runtime、改变tick/worker或修改C++。

## Out of scope

production代码、DAT、AI、collision/hit、render、T8、IL2CPP、Android、服务器、F1/F2 debug。

## Result — 2026-08-23

- fresh compile：0 error；
- Right edge：tick1080；physical K对应canonical Defend edge：tick1084；
- first airborne：tick1088；release：tick1091；landing：tick1108；
- DAT合同：`jump_distance=8`、`jump_height=-16.3`；首个airborne样本：`Vx=7`、`Vy=-14.6`；
- X：baseline775→jumpStart791→firstAir809→final949；
- 对象数：8→8；五项checkpoint均为true；
- 报告：`Temp/NTSD_R8_PHYSICAL_MOVEMENT_PLAY.result.json`，状态`PASS`；
- final compile/Console：0 error；focused EditMode 257/257 PASS；
- full `BattleRuntimeSelfCheck`：2026-08-23 17:17:19 PASS；
- Change Ledger validator PASS；scoped diff/whitespace check PASS。
