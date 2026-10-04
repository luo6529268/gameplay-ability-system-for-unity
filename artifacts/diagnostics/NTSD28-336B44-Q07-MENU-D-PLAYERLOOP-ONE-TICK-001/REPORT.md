# Q07 Menu D PlayerLoop 单 tick 诊断

状态：`VERIFIED_DIAGNOSTIC_ONLY`。当前权威仍为根目录 SHA-256 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3` 的正式 EXE 与对应 playable 路径；本诊断没有取得正式版与 Unity 的战斗位移同条件结论。

[原始预检](preflight-01.json)记录原 Editor 为单一干净 Menu、非 Play/非编译，且保护 Menu/Battle 两 Scene、GameConfig、ProjectBattleModeConfig 四个文件的 SHA-256。只用原 Editor 6401 桥接刷新脚本、进 Play，并运行独立 `Run Menu D PlayerLoop One Tick Trace` 一次。生成 Editor 项目编译 0 error/297 warning；原 Editor 程序集时间晚于新脚本且 Console 无编译错误。

[原始单 tick JSON](playerloop-one-tick-20261003-181502-798-27aed0780e29468e81731453847804dc.json)显示：排队时 Dynamic 更新序号1978，后续正常 PlayerLoop Dynamic 更新到1979；P1 `Player_1/Move` 已启用，绑定 `/Keyboard/d#1`，当前 Keyboard device ID 也是1。生产完整 Driver 仅由 tick3 推进到tick4。推进前 `keyboard.dKey.isPressed=true`，但 `MoveAction.ReadValue<Vector2>().x=0`、回调保存的 `CurrentMoveInput.x=0`、`activeControl` 为空；tick4 canonical held/pressed/released 均0，P1 规则源X403.544921875、视图X620不动。首个**已观测**断点在这次合成事件的键盘设备状态与 InputAction 值之间。Unity Input System 1.7.0 的 `InputSettings` 文档及 `InputManager` 源码说明 Editor 在 Game View 未获焦点时会按设置路由键盘事件；原 Editor 焦点标志为 false。这是可能的测试条件，不是已证明的唯一原因，也不证明玩家真机 D 键或生产位移有故障。

探针按原清理出口退出 Play；之后原 Editor idle、非 Play、单一干净 Menu。四保护文件 SHA-256 与预检逐一相同。未改正式 DAT/图片、生产脚本、Scene 或非战斗逻辑。此合成注入路线已按 Task 停止，不重复阴性长跑；Q07 物理键与 F02 可达入口仍开放，后续应选有正常 InputAction 响应的既有原 Battle Scene 物理键路径做定向见证。此报告只关闭探针的诊断范围，不关闭 Q07/Q09/Q12 或总目标。
