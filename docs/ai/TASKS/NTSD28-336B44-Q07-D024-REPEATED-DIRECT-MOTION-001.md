# NTSD28-336B44-Q07-D024-REPEATED-DIRECT-MOTION-001

状态：`VERIFIED_SCOPED_DIRECT_MOTION`（覆盖先前IN_PROGRESS/共同区域限定快照）。正式源/根、原Editor小数RED→整类35/35 GREEN、原Battle Scene Z380共同区域24完整tick及Z400触边后画面投影归零、完整SelfCheck均已验；地图边界规则差异保留用户例外。其它D-024/Q07/Q09/Q12出口仍开。父目标为 336B44 总表 Q07、D-024 战斗实体统一比例域；不改变固定完整背景相机、项目地图、DAT、非战斗逻辑或既有例外。[最终验收证据](../../../artifacts/diagnostics/NTSD28-336B44-Q07-D024-REPEATED-DIRECT-MOTION-001/REPORT-V2.md)。

权威与触发：根正式 EXE SHA-256 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3`；其 playable `battle_world.cpp::apply_frame_motion` 对当前帧 `dz` 从规则整数 Z 重建精确 Z，逐 tick 取整。正式 `resources/runtime/decoded_dat/data/data.txt` 的 OID92/type0 指向 `s/0/iru.dat`；该 DAT 的 action580 是 `wait:20 next:580 dz:4`，与 Unity 暂存原始文件 SHA 一致。用户 D-024 要求在保留固定完整背景的情况下，让战斗实体的实际画面位移按正式视口比例一致。

现状候选：`LF2Entity.ApplyNativeFrameMotionTail` 每次从物理 `Runtime.ZInt` 加 `dz × DepthScale`，而 `Runtime.SourceRuleZInt` 单独按原版整数域递推。若 action580 在连续完整 tick 保持且没有其它重置，1152/730 视口的反复物理取整可能累积偏差。静态数学不是当前正式根/Unity Play 首差；必须先用同 DAT、同初态、同 tick 实测。

代码所有权：诊断只新增 `Tools/NTSD28Q07Diagnostics/d024_repeated_direct_motion_probe.cpp`；若正式可达且 Unity 出现首差，再修改共用 `Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs::ApplyNativeFrameMotionTail` 与现有 `Assets/NTSD/Scripts/Test/Editor/NTSD28Q06FrameMotionTailEditorTests.cs`。不增加 OID92 特判。若发现链接平台同构缺陷，先记录具体消费者和复测，避免把不同 pass 混作一个未经验证的补丁。

原Editor已有RED与生产聚焦GREEN后，原Battle Scene验证允许仅扩展现有 `Assets/NTSD/Scripts/Test/Editor/NTSD28Q07D024Kind8BattlePlayProbeEditor.cs` 的独立 opt-in 诊断分支：保留原kind8请求和既有报告，新增 `caseName=direct-motion` 对OID92/action580远距OID2跑24完整Driver tick、规则/画面比例断言、四保护SHA和有序关闭。此扩展在脚本修改前已写入同Change Record；不增加新生产入口。

验证出口：正式 playable 源会话导出 initial+20 tick 的 action、规则整数/精确 Z、输入及 LFR，根 EXE 用该 LFR 复播并比选定字段；原 Editor 用正式暂存 OID92、原 Battle Scene 的生产 Driver 对照相同初态/20 tick 和画面比例。若 RED 成立，测试先覆盖连续帧而非仅首 tick，修共享投影后跑聚焦及相邻帧运动测试、生成/原 Editor 编译、必要的 SelfCheck 和 Scene 重验，检查有序关闭、Battle/Menu/GameConfig/Mode 四保护 SHA 和 Ledger。正式 EXE 实际 Present/GPU 及整场验收仍属 Q09/Q12。

风险：当前单 tick 测试故意使用物理/规则源位置不一致的夹具；任何共享修正必须解释这类历史夹具和原生整数取整，不能用数学表替代真实会话。失败原件保留。回滚仅针对本 Change 的精确代码行做经审查的反向补丁，保留其它工作区修改；不使用破坏性 Git 操作。
