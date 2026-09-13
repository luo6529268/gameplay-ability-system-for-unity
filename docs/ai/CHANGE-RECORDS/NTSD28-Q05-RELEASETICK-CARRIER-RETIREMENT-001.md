<!-- CHANGE-RECORD
id: NTSD28-Q05-RELEASETICK-CARRIER-RETIREMENT-001
status: FOCUSED_TEST_PASS
change-kind: REMOVE_RETIRED_RELEASETICK_CARRIER
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2WeaponBase.cs
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2WeaponHeldStateResolver.cs
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2WeaponReleaseFlowResolver.cs
code-path: Assets/NTSD/Scripts/Simulation/Core/NTSDEntityRuntime.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Core/BattleEcsWorld.cs
code-path: Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleHeldObjectWriter.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/Checksum/BattleLockstepChecksumModule.cs
code-path: Assets/NTSD/Scripts/Simulation/Lockstep/Checksum/BattleParitySnapshot.cs
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B6LegacyGrabbedByRetirementEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B6LegacyReleaseTickRetirementEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B6WpointDvxWeaponHpPreservationProductionEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B6WpointMissingActionContinueProductionEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q05ReleaseTickCarrierEditorTests.cs
authority: NTSD28-B6-LEGACY-RELEASE-TICK-OWNER-AUDIT-001 and PRODUCER-RETIREMENT-PRODUCTION-001 VERIFIED; Q03/Q05 approved joint carrier migration; formal Logan EXE B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033.
evidence: RED_7_FAIL / FOCUSED_267_PASS / SELFCHECK_PASS / FOUR_CURRENT_PLAY_ROWS_UNCHANGED / JOINT_SCHEMA_PENDING
-->

# Q05 ReleaseTick 载体及无效参数退休

当前生产 ReleaseTick 只剩 runtime/default/copy/reset、ECS fingerprint、checksum/parity；当前 release resolver / held writer 的 stampReleaseTick 参数无读取，仅 Base/HeldState 与 caller 传递。Authority 已确认无对应字段或 semantic reader，producer 先前已 VERIFIED。本包延续 Q05 获准同窗口删除，不重写真正释放规则。

准确14脚本（8生产、5旧测试/诊断、1新测试）：移除 NTSDEntityRuntime.ReleaseTick、两个校验输出项、RuntimeFingerprint 项；移除 LF2WeaponBase.ReleaseHeldWeaponRuntimeInternal、ReleaseFlowResolver.ReleaseHeldWeaponRuntime、BattleHeldObjectWriter.ClearLinks 的无效参数及所有 caller。LinkState/Holder/Target/ThrowFrameGuard、动作/速度/RNG/HP/计数/注销/对象池保持；WeaponState/HolderCopy 后继，TrackerParent/Owner/Spawner/2F8 保持。

测试先新增成员/参数/校验字段不存在 RED，再迁移旧 ReleaseTick 保留结构断言、SelfCheck 的 preserved/expected 参数与人工 sentinel、WPoint 序列和 Goal20 report。保留全部有效关系/动作/HP/RNG 断言；reset 改验真实 Link/Holder/FrameGuard 默认。Play probe 去掉已不可能成立的 RED stamp 分支，改用当前字段不存在与实际关系/action/motion/RNG 输出，不能伪造旧 releaseTick 值。历史工件不覆盖。

验收：现有 Unity 桥接编译、定向 release/held/WPoint/snapshot/ECS/hash 测试、完整 SelfCheck、真实 NTSD_Battle paused-world 当前数据四例 Play/清理。不得把 pass-only probe 称为物理按键或整技能对齐。先保留失败，再最窄修正/复验。没有新增 lifecycle owner 或关闭事务；33ms/3ms、十一阶段和 Unity/GAS 不变。

当前12/20/23/1/1为同Q05未发布中间态；identity/双OPoint guard、统一13/21/24/2/2、旧版本拒绝/回放仍是强制后继，不能发布 baseline 或跳 Q07。保留 Scene/InputActions/Gen/Plugins/外部包/非战斗/资源/stage USER_HOLD/例外；禁止 computer-use。已有 Foot 18 缺失、Scene 旧 SHA 不修复或清理。当前 HEAD cf35dbf0 已包含前包工作；两份未跟踪 authority-content 工件保留。

回滚：预变更字节和 SHA 存 preimages/*.before.txt / prechange.json；只在用户明确批准后撤销本包精确差量，不能 git restore 全文件抹掉后续用户工作。

## 已写

RED7/7失败已保留；删除前真实Play四例PASS、before/afterObjects=4。14脚本已写，旧sentinel/校验key/无效参数移除，有效关系断言保持；reset改验真实默认。首轮修改脚本因匹配断言过宽停止，SelfCheck未写，随后精确匹配补齐；未回退其他文件。编译/focused/SelfCheck/after Play待。

267/267 focused PASS。首轮完整SelfCheck失败于遗漏的lowercase JSON旧断言 releaseTick:99，失败结果已保留；同声明SelfCheck文件仅改成JSON字段不存在，其余block/Unk/owner/relation断言保持。其他probe的局部releaseTick是真实观测时刻，不是退休runtime载体，保留。后续完整SelfCheck复验待。

## 限定出口

267 focused PASS；完整SelfCheck首轮旧JSON断言失败已留证，修正后10:05:23Z >10:04:44Z新请求PASS；前后当前数据四例Play同seed424242/tick5，除退休字段外全部row值一致，4→4清理、实际反射absent。Scene旧SHA/dirtyfalse/root14，CS0，3059保护无新缺失。详细实际命令、边界和结果见artifacts/diagnostics/NTSD28-Q05-RELEASETICK-CARRIER-RETIREMENT-001/REPORT.md；联合版本不发布。下一NTSD28-Q05-WEAPONSTATE-CARRIER-RETIREMENT-001。
