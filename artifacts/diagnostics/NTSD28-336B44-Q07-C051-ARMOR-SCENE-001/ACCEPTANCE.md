# Q07/C051 护甲与无甲原 Battle Scene 限定 Play 验收

当前规则与内容权威为 SHA-256 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3` 的根正式 EXE、对应 playable live path 和非排除正式 DAT。正式源/根已证 OID78/action466→OID447/action54/effect23 在 tick3 对 OID97 护甲为 `applies`，HP500→495；OID2 无甲控制 HP500→450/action180。[源/根报告](../NTSD28-336B44-Q07-C051-ARMOR-REACH-001/REPORT.md)与[原 Unity 完整 Driver 报告](../NTSD28-336B44-Q07-C051-ARMOR-UNITY-DRIVER-001/REPORT.md)为本次 Scene 前置。

原项目 Unity Editor PID105896、Unity 2022.3.62f3，在一个 clean `NTSD_Battle.unity` Play clone 预 Start 配 OID78/97 或 OID78/2 roster。稳定暂停后以 seed682973786、mode0、双方 X500/550、Z400、角色 HP/MP500、面向相对、队伍1/2和12个中性输入运行生产 `SimulationTickDriver`。采用当前 `Assets/NTSD/Content/LoganRuntime`；DAT、图片、生产代码、Scene 与非战斗功能均未改。

| 案例 | 原 Battle Scene 结果 | tick3 | 对正式根逐 tick 比较 |
|---|---|---|---|
| 护甲 OID97 | [原件](armor97-x550-scene-v1.json) `SCOPED_PASS` / 12 tick / 正常退出 / scene clean | OID447 已出生，目标 action0、HP495 | [逐字段原件](scene-root-selected-fields-comparison-v1.json)：11字段×12 tick＝132/132 零差 |
| 无甲 OID2 | [原件](control2-x550-scene-v1.json) `SCOPED_PASS` / 12 tick / 正常退出 / scene clean | OID447 已出生，目标 action180、HP450 | 同上132/132 零差 |

比较字段为父动作、OID447子体数/首槽/首动作/X、目标动作/X/HP/MP/运行时护甲HP/Vx。根 trace 只取当前336B44正式根 v2；两结果均为独立新输出，未覆盖旧案例。护甲逐 hit `applies` 来自正式源码内部分支；Unity Scene 导出的 HP/action 等结果同态，不能据此声称逐 hit 内部字段也已对齐。

原 Editor 已通过本地 Unity-MCP 桥刷新导入，新程序集时间晚于本包脚本；生成 Editor 工程 `dotnet build Assembly-CSharp-Editor.csproj --no-restore -v:q -clp:ErrorsOnly` 为0 error/238 warning。两例均为真实 Play 和生产 Driver；[最终 Editor 状态](editor-final-state-v1.json) idle、非 Play、Battle Scene clean。[临时请求事前/事后审计](../../../docs/ai/FILE-OPERATIONS/NTSD28-C051-ARMOR-SCENE-REQUEST-20261001-001/RECORD.md)显示唯一新请求已被原 Editor 精确消费，载荷原件保留；[保护核验](request-post-v1.json)四 SHA 与事前相同、LoganRuntime Git 无差异。未使用 computer-use 或第二个 Unity 项目。

[Change Ledger 校验原件](change-ledger-validation-v2.log)：PASS，1103 Records，当前差异中47个受治理代码文件均有记录；相关文档 `git diff --check` 返回0。首次尝试捕获 validator 的 Write-Host 输出仅生成2字节空行文件 `change-ledger-validation-v1.log`，该文件保留为失败捕获历史，不作为通过证据；v2 使用全流重定向完整保存真实输出。既有大量“历史 Record 路径不在当前 diff”警告不属于本包代码错误。

`NTSD28-336B44-Q07-C051-ARMOR-SCENE-001 / VERIFIED / SCOPED_SCENE_PASS` 只关闭本受控 Scene 包。自然物理按键、逐 hit 内部字段、effect22 当前正式内容静态无 ITR 的可达性边界，以及完整 World/画面仍待；父 C051、Q07 与总目标均保持开放。
