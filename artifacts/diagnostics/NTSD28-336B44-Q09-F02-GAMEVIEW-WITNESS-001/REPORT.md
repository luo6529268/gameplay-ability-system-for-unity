# Q09/F02 原 Battle Scene Game View 证据（2026-10-03）

本包只验证原 Unity Editor 中 F02 自然拾取、轻投及高速武器 kind10 链的画面取证入口；正式 336B44 EXE 与 Unity 的逐像素同态不在本包已证范围内。未修改 DAT、PNG、场景、相机、生产逻辑或非战斗代码。

## 已观察

- 原 Editor 从已保存且干净的 `NTSD_Menu.unity` 进入既有 `NTSD_Battle.unity` Play，使用 `f02-kind10-view-20261003-01` 唯一请求，在相对逻辑 tick 30 和 39 后分别取得 [Game View tick30](../NTSD28-336B44-Q07-F02-NATURAL-SCENE-001/f02-kind10-view-20261003-01/game-view-tick30.png) 与 [Game View tick39](../NTSD28-336B44-Q07-F02-NATURAL-SCENE-001/f02-kind10-view-20261003-01/game-view-tick39.png)，均为 1920×1080 有效 PNG。SHA-256 分别为 `AE7A2F9ED87168C386973E77DA4303EA9E56864B109CF43EA685E085DAF52F0F`、`14F37BBDDEBD548D25C7F4040B047E20577F148E7AFA664F07825F8E6EE5496C`；见 [最终探针报告](../NTSD28-336B44-Q07-F02-NATURAL-SCENE-001/f02-kind10-view-20261003-01/00058.json)。截图请求后的实际 GPU 呈现相位没有独立帧号，不能仅凭文件名断言每个像素恰好属于标记 tick。
- 与同初态、同 45 个完整生产 Driver tick 的 [无截图 run-07](../NTSD28-336B44-Q07-F02-NATURAL-SCENE-001/f02-kind10-scene-20261003-07/00056.json) 对比：`before`、`after`、46 组 `samples`、47 组 `eventRows` 的序列化值均完全相同。新增截图等待没有改变已声明的规则状态或事件。
- 新报告 `status=CAPTURED`、`phase=DONE`、空错误。退出 Play 后回到干净 Menu，`orderedShutdownComplete=true`，World 对象、运行槽、池借用、活动池对象及 Sprite 均为 0，pool quiesced、World detached、无 live Driver World，四个受保护文件哈希稳定。随后原 Editor 状态为 idle、非 Play、未编译、未更新、当前 Menu。
- 两张图直接显示原项目自有背景、HUD、Tayuya 与武器所在的战斗画面；后图在 Tayuya 右侧可辨识细长武器图像。两张图左下鸣人附近均有大块不透明黑矩形。对接近纯黑（各 RGB 通道≤3）的连通区只读测量：tick30 的最大区域约从 `(216,806)` 跨 `162×123` 像素、面积 13,497；tick39 约从 `(216,806)` 跨 `124×101` 像素、面积 9,420。区域还与屏幕摇杆重叠，单靠截图不能确定其归属是 UI、角色 Sprite 还是二者叠加。

## 状态与后续

截图取证和无模拟漂移验收已通过，本 Change 只关闭“原 Battle Scene 有可审计 Game View 截图”这一子门。由于黑块遮挡，**鸣人画面表现不能判为已对齐**；武器的屏幕像素位置及正式 EXE 同条件画面也未完成定量配对。下一步按 Q09 战斗表现范围，只读定位黑块的 Scene/UI 与战斗 Sprite 呈现来源，再用最窄验证确认原因；如需改脚本或资源，另立 Task/Change，并继续遵守固定完整背景及统一空间投影例外。F02 整体、Q09、Q12 和总目标保持开放。
