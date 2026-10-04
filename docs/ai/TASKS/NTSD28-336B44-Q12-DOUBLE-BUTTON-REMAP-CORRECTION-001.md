# NTSD28-336B44-Q12-DOUBLE-BUTTON-REMAP-CORRECTION-001

状态：`RUNTIME_PENDING / SCOPED_NATURAL_INPUT_PASS`。父目标 `NTSD28-UNITY-BATTLE-REALIGNMENT-001`，Q12 代表性自然按键首差回退到 Q07 共用输入 owner。正式根 EXE SHA-256 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3`；正式 `input_state.h::InputKey28` 索引 4/5/6 为攻击/跳跃/防御。

2026-10-04 现行出口：物理控制器三键完整入口原 Editor 六项6/6定向测试，原 Battle Scene 自然鸣人首253后 J 转301，正式源码从防御消费起动作/PP/combo1/相位132/132同，前后 Scene clean 和四SHA稳定；[证据与未验证边界](../CHANGE-RECORDS/NTSD28-336B44-Q12-DOUBLE-BUTTON-REMAP-CORRECTION-001.md)。直接 Q09 诊断入口已订正但未重跑，Q12 同随机表/画面/a7实际声/重进仍待，不据此报告总目标完成。

当前原 Battle Scene 的自然鸣人防→前→跳→后续攻击代表例，物理 L 在首 tick 以 `FrameInputSet` mask16 进入，`NativeInputProxy.Current[4]=1`、动作60、combo1始终0；八次 L 脉冲后探针有限失败。旧当前版首窗口在新 Q07 下游改动前曾进入防御组合并于首253后转301。新失败原件 `Temp/diagnostics/NTSD28-Q07-RASENGAN-NATURAL-COMBO-PLAY-001/natural-first253-20261004T105755587-f2fa112e00a54ecdba14b7479498e953.json`，本轮 Play 退出非Play、Scene clean，Battle/Menu/两个配置 SHA 前后相同。

首差归属：`CharacterInputModule` 已长期将物理 J/K/L 分别编码为 `FuncKeyMask.jump/def/att` 和 `SimulationInputButtons.Jump/Defend/Attack`；正式 producer 再按 `KeyJump/KeyDefend/KeyAttack` 投影到正式索引 4/5/6。最近 Q07 Change 在 `NTSDInputStateModule` 又对人类 native 路径做一遍三键轮换，实际物理 L 被误当 Attack。Q07 当时的测试直接向 buffer 注入 `FuncKeyMask.att`，以及 Q09 探针直接提交 `SimulationInputButtons.Attack`，二者都绕过已有物理控制器适配，不能裁决玩家攻击链。

声明脚本路径：`Assets/NTSD/Scripts/Test/Editor/NTSD28NativeInputProducerMigrationEditorTests.cs` 先改为物理控制器完整入口的三键 RED；随后仅撤去 `Assets/NTSD/Scripts/Input/NTSDInputStateModule.cs` 的 Q07 二次轮换与 `Assets/NTSD/Scripts/Simulation/Input/NTSD28InputTwoPassModule.cs` 的对应人类逆轮换；`Assets/NTSD/Scripts/Test/Editor/NTSD28Q09P08SameStateBattlePlayProbeEditor.cs` 的直接输入改用既有物理 J 对应的 `SimulationInputButtons.Jump` 并说明原因。保留 Q07 测试请求路径、其它现有测试/用户改动。若 RED 暴露额外首差，先更新本 Task，不扩大改动。

验收：原 Editor 定向 RED→GREEN 三键 one-hot/按住/释放和 legacy/AI 相邻测试，生成 Editor 编译0错；当前版原 Scene 自然 L/D/K/J 首窗口一次复验并配对正式动作/PP/输入相位；必要时用现有 Q09 22tick 按真实 J 布局复用基线，但不因随机表不同反复跑。每次 Play 前后 Scene clean、四保护 SHA 与有序退出残留分别记录。`Tools/Validate-ChangeLedger.ps1` 与 `git diff --check` 通过。旧 Q07 下游补丁和由其产生的测试证书保留历史并追加更正，不删除原件。正式 DAT、图片、音频、Scene、Prefab、菜单/结果页和框架均不改。

回滚：仅针对本 Change 的精确 hunk 逆向修正，不使用 `git restore` 覆盖共用文件；若真实键/生产 Driver 仍有首差则保留失败证据，按当前正式调用链继续定位。
