# Q07/D-024 自然 CPOINT 持有距离比例首差（2026-10-02）

更正（2026-10-02）：下面“tick2～60 共59个持有 tick”是按尚未清零的 `caughtSlot` 计数；动作核对后，真正持有为 tick2～55 共54tick，tick56～60已投掷。首个持有tick的红色首差仍成立；生产修复与绿色验收见[后继报告](../NTSD28-336B44-Q07-D024-CPOINT-HELD-PROJECTION-001/REPORT.md)。下方保留当时诊断快照，不作为当前生产状态。

状态：`VERIFIED_DIAGNOSTIC_FIRST_DIFFERENCE / PRODUCTION_FIX_PENDING`。战斗规则权威为根正式 `NTSD2.8-Logan.exe` SHA-256 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3` 及对应 playable 源码。比例变换是用户批准的项目完整背景例外，正式源码仍用于定义源规则位置。

正式 `battle_world.cpp:6262-6294` 的 CPOINT 持有末尾用源规则整数 `position.x/z` 与 DAT 中心/挂点计算被抓者坐标。既有 OID75/action355、目标 OID2/X650 自然链在正式源码、正式根和原 Battle Scene 的选定**源规则**字段同态；本轮只扩原场景探针，追加实际物理 X/Z 与共用 `SpatialProjection` 的期望值，使用唯一结果 `bee75-a355-x650-spatial-witness-01.json`，旧结果及请求另存保护。

原 Battle Scene 新 Play：`CAPTURED / DONE`，完整 60 tick，tick2～60 共59个持有 tick；水平比例 `2048/1333 = 1.53638409602401`，纵深比例 `1152/730 = 1.57808219178082`。旧/新探针共有实体字段逐 tick 比较 **2880/2880 相同**，因此本次加字段没有改变该自然链已证的源规则结果。首个持有 tick 的具体读数：

| 同一 tick2 | 抓取者 | 被抓者 | 两者间距 |
| --- | ---: | ---: | ---: |
| 正式源规则整数 X（Unity 同态） | 630 | 639 | 9 |
| 当前 Unity 物理 X | 970.50 | 979.00 | 8.50 |
| 按用户视口比例，9 源像素应对应的物理间距 |  |  | 13.827457 |
| 当前 Unity 物理 Z 间距 |  |  | -1.232877 |
| 按比例，源 Z 间距 -1 应对应的物理间距 |  |  | -1.578082 |

源规则精确浮点位置在抓取者运动后与整数相差0.5；若按精确源浮点间距8.5计算，投影期望为13.059265，仍大于实际8.5。因此无论按正式渲染使用的整数位置，还是按精确源位置衡量，持有相对距离都没有随项目全视口同比放大。`BattleCpointWriter.SyncHeldPosition` 当前先用物理 X/Z 的整数锚点直接加 DAT 中心/挂点偏移，随后独立写入源规则位置；`NTSDEntityRuntime.SyncIntegerPosition` 只截断物理值，不会自动把源规则坐标投影回物理值。此处为已复现的**比例域首差**，并非正式战斗规则字段或输入相位首差。

生成 Editor 工程 `dotnet build Assembly-CSharp-Editor.csproj --no-restore -nologo` 退出0、0 error、251个既有警告；原 Editor 经 MCP 刷新后完成 Play，退出 idle/nonPlay/noncompiling、Scene clean。Battle/Menu Scene、GameConfig、ProjectBattleModeConfig 及旧 X650 JSON 的 SHA 5/5 不变，见 `protected-before.json`/`protected-after.json`。诊断只改 Editor 探针，不改生产、DAT、图片、Scene 或非战斗逻辑。

下一步必须在独立生产 Task/Change 中修复共用持有位置写入，优先让物理相对距离经 `SimulationWorld.SpatialProjection` 转换，同时保持正式源规则 X/Z 和身份比例下的既有值；用同一自然链新唯一 Play 对照 RED/GREEN，并检查其它持有/武器挂点写者是否有同一类型的比例缺口。Q07/D-024、Q09表现和总目标仍开放；本报告不证明正式 EXE 与 Unity 全画面逐像素一致。
