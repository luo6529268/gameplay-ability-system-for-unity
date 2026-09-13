<!-- CHANGE-RECORD
id: NTSD28-Q05-WEAPONSTATE-CARRIER-RETIREMENT-001
status: FOCUSED_TEST_PASS
change-kind: REMOVE_RETIRED_WEAPONSTATE_CARRIER
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2WeaponBase.cs
code-path: Assets/NTSD/Scripts/Simulation/Core/NTSDEntityRuntime.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Core/BattleEcsWorld.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/Checksum/BattleLockstepChecksumModule.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/Checksum/BattleParitySnapshot.cs
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B6LegacyGrabbedByRetirementEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B6LegacyWeaponStateRetirementProductionEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B6WpointMissingActionContinueProductionEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q05WeaponStateCarrierEditorTests.cs
authority: Q03 JOINT-FIELD-MATRIX; NTSD28-B6-LEGACY-WEAPON-STATE-OWNER-AUDIT-001 and BEHAVIOR-RETIREMENT-PRODUCTION-001 VERIFIED; official Logan B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033.
evidence: RED_4_FAIL_2_PASS / FOCUSED_282_PASS / SELFCHECK_PASS / OID124_PLAY_UNCHANGED / JOINT_SCHEMA_PENDING
-->

# Q05 WeaponState载体退休

准确10脚本，5生产/4旧测试与诊断/1新测试。当前runtime WeaponState只有field/copy/reset、WeaponBase reset零赋值、ECS RuntimeFingerprint和checksum/parity，没有生产行为reader；已VERIFIED前包退休1002→2000→3000平行逻辑，真实state来自frame.state。

删除上述载体与序列位置，保留CurrentFrameState/GetResolvedWeaponStateForExternalUse、FrameLogicResolver/HeldStateResolver以及全部frame/state/速度/动作/HP/随机数/关系行为。实际帧状态参数weaponState、LF2States枚举不是退休载体，不能机械删除。前包ReleaseTick等既有未提交差量保留。HolderCopy单独后继。

先新增absence/三输出guard RED与实际帧状态回归；旧保留carrier测试改验实际Frame值copy/snapshot/checksum/parity，保留真实敏感性。SelfCheck人工sentinel及独立state不变断言移除，实际动作、velocity、durability、cleanup断言保持。旧held报告从伪独立state改为实际follow/throw/drop/damaged行为与frame观察，不能伪造旧reservedWeaponState=0。旧Play witness删reserved输出，保留真实state1002/hit_Fa12/vx14及清理；历史工件不覆盖。

验证：当前Unity桥接编译，新测试+现有weapon-state/held/impact/landing/snapshot/ECS/hash聚焦回归、完整SelfCheck；已有R4 request在真实Battle世界调用R3 OID124/action40两次pre-frame pass，改前/改后有效观测比较。非物理按键/整技能/整tick物理或全native-world parity证明；旧内容fixture不得当新Logan全部部署完成。

没有新增manager/queue/worker，十一阶段不变。保留Unity/GAS/非战斗、33ms/3ms、Scene/InputActions/Gen/Plugins/外部Server、资源/stage.dat USER_HOLD与例外；禁止computer-use，仅现有Editor桥接/日志/结果。Scene旧SHA和Foot18既有缺失不动。

Q05未发布窗口12/20/23/1/1；剩余HolderCopy、identity/双OPoint guard、统一13/21/24/2/2、旧版本拒绝/回放/Play必须继续，不发布中间baseline或提前Q07。回滚需明确批准，仅按preimages字节/SHA精确撤销本包差量，不覆盖用户及前包工作。

## 已写

新测试RED4失败/2实际帧解析通过；修改前真实R3 Play两观察PASS，已保留。准确10脚本落盘，五生产仅载体与序列删除；旧held改验实际frame/有效links与速度，copy/hash/snapshot敏感性改用真实WeaponFlightCounter并新增运行时JSON absence。旧SelfCheck全部有效动作/velocity/durability/cleanup断言保持。编译/focused/SelfCheck/after Play待。

## 限定出口

282/282 PASS，完整SelfCheck新请求10:14:50Z→结果10:15:29Z PASS；当前真实Battle中OID124/action40两次pre-frame Play前后除删除reservedWeaponState外全部有效观察相同，state1002/hitFa12/vx14、对象4→4，R4四释放用例也PASS。CS0/Scene旧SHA/dirtyfalse/root14、保护3059无新增缺失。准确10脚本与前包14合并仅16，前包差量保留。命令和边界见artifact REPORT；下一NTSD28-Q05-HOLDERCOPY-CARRIER-RETIREMENT-001，联合版本不发布。
