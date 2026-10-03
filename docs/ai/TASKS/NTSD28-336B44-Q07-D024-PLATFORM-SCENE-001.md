# NTSD28-336B44-Q07-D024-PLATFORM-SCENE-001

状态：`VERIFIED_SCOPED_CONTROLLED_SCENE`。父项：336B44 总表 Q07/D-024 平台重复搬运；前置为 `NTSD28-336B44-Q07-D024-PLATFORM-REACH-001`。[原Scene限定报告](../../../artifacts/diagnostics/NTSD28-336B44-Q07-D024-PLATFORM-SCENE-001/REPORT.md)保留首轮测试断言误判与第二轮通过；自然按键和父项仍开放。

当前正式 OID56/frame182 与 OID2 的受控 X200/205、Y0/−5、Z400、不同 team、mode0/seed `0x28A55A5A`、背景1、中性输入完整 `GameSession28` 前10tick，链接连续10tick、目标 X205→178；根正式 EXE 同 LFR 选定99/99字段吻合但 `nativeParityClaim=false`。这不是玩家自然按键可达证书。原 Battle Scene 现有 frame130 单 tick 平台链接已通过，尚未跑 frame182 连续运动。

只新增 `Assets/NTSD/Scripts/Test/Editor/NTSD28Q07D024PlatformBattlePlayProbeEditor.cs` 与 Unity 生成 `.meta`。唯一 opt-in 请求从已保存、干净的原 Battle Scene 进入 Play，在生产 World/完整 `SimulationTickDriver.StepOneTick` 注册正式 OID56/frame182 和 OID2，同规则初态及源→物理统一投影，连续10tick中性输入，记录每 tick 两者动作/规则 X/Y、目标平台来源/碰撞参考、实际物理 X 与期望比例。不得在初始注册后手工写动作、位移、链接或命中。对同一源码阳性 0..10 tick 逐字段比较，要求物理投影误差不超过整数显示误差、池/槽/对象回基线，退出 Play 后两 Scene SHA不变并恢复原 Menu。

不改 Unity 生产、DAT、图片、音频、地图/模式、Scene、相机、菜单、ProjectSettings 或其它用户文件。测试脚本须先生成项目编译0错，再由原 Editor 导入、唯一真实 Play、记录失败原件；成功也只关闭受控原场景连续比例，不等于自然输入、正式 EXE GPU 像素或 Q07 总完成。回滚以本 ID 新增脚本的经审查前向更正为主，保留运行证据，不作破坏性 Git 操作。
