# Q07/D-024 kind0 自然火花纵向出口（2026-10-05）

状态：`VERIFIED (scoped)`；仅关闭正式可达 kind0 火花源事件 Y→固定视口 Y 的本次首差。父 Q07/D-024、Q09、Q12及总目标仍开放。用户确认继续保留 Unity 现有 `BattleVisualScale=1.5` 与战斗实体显示尺寸；本修复不改图片尺寸、DAT、相机、背景或 Scene。

正式规则依据为当前根正式 EXE SHA-256 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3` 对应 playable 构建闭包的 `battle_world.cpp::append_confirmed_native_spark`：先在源规则整数域合成 target Z、hitY 和一次 Y CRT 抖动，然后第二次 CRT 算 X。正式第25相对 tick 可绘制火花为 host slot2、源屏幕Y352、host源Z400，即向上48/730画面高。原 Unity 同输入 Play 旧命令相对 host 向上约48/1152，首差和 SHA 见 [首差](SPARK-FIRST-DIFFERENCE.md)。

本次测试先行：原 Editor 定向 job `6b278df4410949b19baff71bf52ce13d` 在未修生产时 identity 例 PASS、固定视口例 `expected 15 / actual 21` RED；生成 Editor 编译先因测试把 `ulong CrtCalls` 声明成 int 出现两条类型错误，修正测试类型后 0 error。生产改动只在 `BattleNativeHitSparkWriter` 共用 `ProjectSourceEventY` 出口将源整数事件 Y 一次投到 view，`BattleEcsHitExecutionPlan` 预测器复用它；CRT 顺序与调用数不变。原 Editor GREEN job `de5cbf0bbffd40d28c30648ab2680ee0` 2/2，旧 identity 数组与 CRT 邻例 job `02b7a87427be41e699c4540cdba2ca3a` 2/2；两者详情保存在本目录 JSON。

原项目唯一 Battle Scene 以同一 C040 mode0/seed0、角都25/奇拉比75/凯97、40个自然完整 tick 复验；新[原始结果](../NTSD28-336B44-Q09-C040-GAMEVIEW-WITNESS-001/q07-d024-spark-green-20261005-01.json)为 `CAPTURED/DONE`、相对tick25/全局tick30一条 pic0 火花。与[修复前结果](../NTSD28-336B44-Q09-C040-GAMEVIEW-WITNESS-001/q01-spark-natural-20261004-01.json)逐项比较，40个 sample 除启动全局 tick 字段外其余字段 **0差**；source/view比例、mode、difficulty、contentRoot均相同。画面命令中 `stableId=102,pic=0,sortOrder=3,x=-3.407465,width=99,height=79` 均不变，仅 world Y 从 `-7.88000059` 变 `-7.60000038`。按当时 ScenesCamera Y=-4.8 与逻辑视口 550px/100px 每世界单位逆算，新火花逻辑视图 Y≈555；host视图 ZInt631，向上约76/1152=6.59722%，对正式48/730=6.57534%只剩整数截断误差约0.02188个百分点；修前48/1152=4.16667%。不能把此局部离屏/合成画面样本称为根 EXE 实际 GPU Present 全像素一致。

探针已退出 Play，原 Battle Scene `isDirty=false`，磁盘 SHA-256 前后均 `253B2EBAD322AA6EC18488DFBBF555BB93AD897940A342E4130E77CE778F9010`，原 Editor idle、Console 0 error、受保护四文件哈希稳定。有序关闭完成，结果中 `enteredPlay/exitedPlay=true`。Unity `refresh_unity` 已直接通过本机 MCP 桥接执行两次，无需用户手动刷新。

限制：未独立证明未初始化 SourceRulePosition 的逆投影回退与 legacy `LF2CharacterDatHitResolver.SpawnSpark` 在当前自然可达路径的完整同态；它们仅在实际阳性首差出现时作为后续条件门。此证据不关闭真实着地、Y向命中响应、R120原 Scene或 Q07/Q09/Q12 的其它出口。
