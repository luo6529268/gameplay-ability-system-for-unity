# NTSD28-B3-C25M-PREVIOUS-ACTION-COMMIT-001 — C25m previous-action commit

<!-- CHANGE-RECORD
id: NTSD28-B3-C25M-PREVIOUS-ACTION-COMMIT-001
status: VERIFIED
change-kind: TEST_FIRST_BEHAVIOR_ALIGNMENT
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs
code-path: Assets/NTSD/Scripts/Simulation/Passes/LateLifecycle/BattleLateEntityLifecycleModule.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28C25MPreviousActionCommitEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28C25LState18ParticleOwnerEditorTests.cs
authority: NTSD 2.8-Logan simulation_tick_driver.cpp C25l then previous_action_078 commit before C25n/o/p; EXE B1E13AE1, closure 39DDDA15.
evidence: TEST-FIRST-CS1061-X3-CS0103-X1 / COMPILE0 / FOCUSED3 / RELATED69 / NTSD28-BROAD450 / SELFCHECK-PASS-2026-09-05T03:58:35Z / SCENE-UNCHANGED / CONSOLE0 / LEDGER-PASS / TERMINAL-PATH-B7
-->

> 状态：`VERIFIED / SURVIVOR_COMMIT / TERMINAL_PATH_PENDING_B7`

## 实际改动

- non-early-terminal survivor在C25l后capture old Prev并立即`MirrorLatePrevFrame`，随后才进入cleanup与virtual compatibility tail。
- 新`RunLateTailAfterNativePreviousActionCommit`用try/finally设置仅调用栈有效的old-Prev context，仍调用原virtual `RunLateTailBeforePrevFrame`；生产LF2Character和探针覆写均未绕过。
- `SpawnLateTransitionEffects`的state13/200兼容branch通过context读取提交前Prev；direct compatibility调用无context时仍读取当前Prev。
- C25p被放到tail flush之后、derived runtime snapshot之前，完成态快照可观察healing。
- pool reset显式清理transient context；不新增runtime/snapshot/checksum carrier。

## 验证

- 红灯：3个预期`CS1061`和1个预期`CS0103`证明wrapper/context缺失。
- focused job `157e233e5f834399a8de2002fd933bb3` 3/3。
- 首次related `5a01775e37534803b15c9e235c439cbd`仅失败旧C25l placement断言；按新Authority顺序更正为l→m→cleanup并登记本Record。最终related `4312b2469c784ba99027e56946b47262` 69/69。
- broad job `2c0b4d93b08b4ecaa2ec5da96f293628` 450/450；SelfCheck于`2026-09-05T03:58:35Z` PASS。
- clear后Console error0；Scene SHA/length/mtime仍`0D74E174...D77 / 203477 / 2026-09-04T13:12:45.1526434Z`；diff-check无whitespace error；Ledger 208 Records / 189 governed code files PASS。

## 未关闭

early terminal slot仍在C25k前被处理，不能执行Authority terminal C25l/m/n/o顺序；该路径依赖B7 formal pending/code producer与C25o，本包不宣称闭合。
