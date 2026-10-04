# Q07/C053 Tobi→firz 受控初态有限可达性（2026-10-04）

**2026-10-04 更正：本包`SUPERSEDED_WRONG_OID`。** 正式`data/data.txt`的OID53指向`c/tobi/ttobi.dat`，而含frame510→512/OID251 OPoint的是OID0的`c/tobi/tobi.dat`。以下四Y阴性仅说明错误角色OID53在这些受控初态没有生成OID251，不能裁决目标OID0；不据此修改正式DAT或输入规则。正确OID0的普通输入已经在[后继源/根同版报告](../NTSD28-336B44-Q07-C053-TOBI-PHYSICAL-REACH-001/REPORT.md)中于tick11自然生成OID251，旧结果保留作历史诊断。

历史记录状态：`VERIFIED_NEGATIVE_FOR_FOUR_INITIAL_Y / NATURAL_INPUT_PENDING`。当前正式根 `NTSD2.8-Logan.exe` SHA-256 为 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3`。本诊断使用与其对应的当前 playable 源码 `GameSession28` 和正式 `resources/runtime`，没有修改正式 EXE、源码或 DAT。初始 Tobi OID53/action510 属于受控设定，不是物理按键选招。

- [首轮](run-a/summary.csv)：Y=-80/-140/-200 各40完整tick，三组均未出现 OID251；Tobi在首tick均由510变为212。首轮工具由同版源码闭包编译 exit0、stderr 空；实际逐tick见[首轮原件](run-a/ticks.csv)。
- [补充轮](run-b/summary.csv)：仅增加Y=0对照并记录初始化和 frame event，四组各40 tick均未出现 OID251。空中三组初始化action510、首tick frame event 510→212、status=held；地面组初始化action510、首tick仍510，前15tick frame event均为510→510/status=held。[逐tick原件](run-b/ticks.csv)。v2工具使用同一闭包、写入新的可执行文件，编译 exit0，未覆盖首轮可执行文件或数据。
- 正式DAT静态存在Tobi frame510→511→512、frame512 `opoint oid:251/action:51` 与firz 51→2→3→0，但这四个受控初态没有走到frame512。原因需另以完整输入链诊断；本结果不证明OID251全局不可达，也不构成Unity差异。没有阳性LFR，故不启动正式根EXE回放；原Unity Editor编译仍未完成，未运行Battle Scene Play。

下一出口只针对正式版普通跳跃状态下`hit_Fa:510`的输入消费做有限案例，再决定是否接入原Scene。既有受控OID251/action0双Uj源/根阳性保持其有限证据边界；C053、Q07及总目标开放。Battle/Menu Scene、GameConfig、ProjectBattleModeConfig四个保护文件 SHA 与本项前记录相同，未改DAT、角色图片、生产脚本、Scene或非战斗逻辑。
