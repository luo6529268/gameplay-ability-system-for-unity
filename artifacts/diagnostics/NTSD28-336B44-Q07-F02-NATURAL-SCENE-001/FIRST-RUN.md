# F02 原 Battle Scene 首轮有限验证

原始逐次快照保存在 `f02-kind10-scene-20261003-01/00001.json` 至 `00017.json`，均为唯一新文件。用户确认 Menu 已保存且 Editor 空闲后，原 Editor 导入了新测试探针；探针进入原 Battle Scene 的 Play 克隆，以 Naruto OID2、Tayuya OID36、武器 OID600 初始化并完成相对 tick0～7。正式来源对照为 `NTSD28-336B44-Q07-F02-KIND10-RESET-001` 的 X530、Z542 正例。

**观测首差：** 相对 tick1，Unity 自有地图的物理纵深限制把两角色源 Z542 改为 Z481、武器改为 Z482；正式版该例仍是 Z542。这说明该初始坐标不适合在本项目当前地图中直接验证同条件战斗。相对 tick8，测试查询发现 slot2 不再是原武器，探针报 `FAIL` 并停止，未到正式版的拾取/投出/kind10/F02阶段。首轮未取得武器退休原因的直接状态字段，不能据此判定生产行为错误。

源码诊断发现测试夹具直接 `new LF2Weapon` 并设置 `Health.HP=250`，但未执行正常初始化中 `LF2Weapon.OnHealthInitialized` 对独立 `Runtime.WeaponFlightCounter=weapon_hp` 的写入。正式暂存 `w/6.dat` 的 `weapon_hp=250`，故计数保持默认 0 是武器提前退休的强推断，下一轮通过新增计数捕捉和正常初值验证。另以地图允许的初始 Z 与正式版重新配对，不改地图、DAT 或生产战斗代码。

本轮 `enteredPlay=true`、`exitedPlay=true`、`noLiveDriverWorldAfterExit=true`，返回单一干净 Menu；Battle Scene、Menu Scene、GameConfig、ProjectBattleModeConfig 四个受保护文件 SHA-256 前后相同。结果只证明首轮安全退出与上述首差；原 Battle Scene 正例、Game View/实体碰撞画面、物理键和 Q07/F02 总门均未关闭。
