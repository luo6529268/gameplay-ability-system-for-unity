# Q07/C051 effect23 护甲原 Unity 完整 Driver 限定对照

当前规则权威为 336B44 根正式 EXE、对应 playable live path 与正式非排除 DAT。[前置正式源/根证据](../NTSD28-336B44-Q07-C051-ARMOR-REACH-001/REPORT.md)已证 OID78/action466→OID447/action54 的 effect23 在 tick3 对 OID97 真正使 armor decision `applies`，无甲 OID2 为对照；根两例各80tick×11字段880/880同源。

原项目 Unity Editor 使用当前 `Assets/NTSD/Content/LoganRuntime`、seed682973786、mode0、X500/550、Z400、OID78/action466面右与目标/action0面左、HP/MP500、12个中性输入生产 Driver tick。仅扩展既有 C051 正式 schema 的两组精确参数允许式，生产代码、DAT、Scene 和非战斗逻辑未改。原 Editor 导入编译的程序集时间晚于脚本修改，生成 Editor 工程 `dotnet build Assembly-CSharp-Editor.csproj --no-restore -v:q -clp:ErrorsOnly` 为0 error/235 warning。

| 案例 | Unity 原件 | tick3 观察 | 对336B44根逐tick选定字段 |
|---|---|---|---|
| OID97 护甲 | [场景](armor97-x550-scenario-v1.json)、[raw](armor97-x550-unity-v1.raw.jsonl)、[结果](armor97-x550-unity-v1.result.txt) PASS | 目标 action0、HP495；子体OID447存在 | [比较原件](unity-root-selected-fields-comparison-v1.json) 12tick×11=132/132，首差无 |
| OID2 无甲 | [场景](control2-x550-scenario-v1.json)、[raw](control2-x550-unity-v1.raw.jsonl)、[结果](control2-x550-unity-v1.result.txt) PASS | 目标 action180、HP450；子体OID447存在 | 同上132/132，首差无 |

11字段：父动作、OID447子体数/首槽/首动作/X、目标动作/X/HP/MP/运行时护甲HP/Vx。Raw header 的 `formalAuthorityExeSha256` 为当前336B44、50个binding字段已绑定，`certificateEligible=false`；这是当前 Unity EditMode 完整 Driver 的已导出字段证据，不代表原 Battle Scene Play、全 World、自然物理键或画面一致。Unity raw 本身不导出逐 hit armor decision，护甲内部 `applies` 证据来自正式源码；Unity 的 HP/action 可观察结果与正式根同态，不能把结果反推为所有内部路径均同态。

MCP `execute_code` 首试被此安装的动态编译器报“文件名或扩展名太长”，没有产生 raw 文件。[首试结果](mcp-armor-v1.result.json)保留。后用原 Editor 已有请求入口；其 `PollRequest` 自动清理本轮新建请求的完整[事前/事后审计](../../../docs/ai/FILE-OPERATIONS/NTSD28-C051-ARMOR-RAW-REQUEST-LIFECYCLE-20261001-001/RECORD.md)为 `VERIFIED`。两份请求原件保留，临时请求当前不存在，8个新输出均存在。[Editor 状态](editor-final-state-v1.json)为原项目 Battle Scene、idle、非 Play；[保护复核](request-lifecycle-after-v1.json)显示四 SHA 前后相同且 LoganRuntime Git 无差异。没有启动第二 Unity 实例或使用 computer-use。

本诊断 `VERIFIED / SCOPED_DRIVER_PASS`。正式护甲分支的原 Battle Scene Play、逐 hit 内部字段、自然物理键及所有其它 C051 路径仍待；C051/Q07/总目标保持开放。下一步在原 Battle Scene 对同初态做定向 Play，或先决定是否需要比当前已导出字段更深入的 armor writer 首差。
