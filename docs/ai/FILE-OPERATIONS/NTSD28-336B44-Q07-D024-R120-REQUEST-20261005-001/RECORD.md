# NTSD28-336B44-Q07-D024-R120-REQUEST-20261005-001

状态：`RESTORED`。类型：既有一次性Play请求的有备份覆盖、消费、恢复；新结果文件的本次运行内状态更新。

Task/Change：`NTSD28-336B44-Q07-D024-VERTICAL-FRACTION-001` / `NTSD28-336B44-Q07-D024-VERTICAL-FRACTION-PROBE-001`。原因：已批准D-024共用Y投影的R120原Scene中间alpha出口。授权来源为用户启动战斗对齐总目标、要求使用原Editor/MCP，并确认原Editor空闲可定向Play复查；沿用本会话已有授权，不新增资源或清理范围。执行者：root当前任务。

工作目录/批准根：`I:\GitHub\Unity_GAS\gameplay-ability-system-for-unity`。准确逐文件范围：

- `Temp/NTSD28_Q07_AirborneIdleBattlePlay.request.json`：已有requested=false，备份后只覆盖为本次具名requested=true；探针消费位后恢复原字节。
- `artifacts/diagnostics/NTSD28-336B44-Q07-C023-AIRBORNE-SCENE-PLAY-001/d024-vertical-r120-20261005-01.json`：操作前不存在，只允许本次新建结果的状态/样本更新；保留终态，不覆盖旧结果。

[操作前逐文件manifest](../../../../artifacts/diagnostics/NTSD28-336B44-Q07-D024-R120-REQUEST-20261005-001/before-manifest.json)含绝对路径、存在性、字节数、SHA、Git状态、时间、保护Scene哈希及拟写请求；[请求原字节备份](../../../../artifacts/diagnostics/NTSD28-336B44-Q07-D024-R120-REQUEST-20261005-001/request-before.json)在范围外，复制后哈希核对一致。临时文件不使用HEAD作恢复来源。

拟执行：Python以UTF-8写入上述请求 `{requested:true,runId:d024-vertical-r120-20261005-01}`；已登记探针 `TryStart` 将同文件requested置false、反复Save同一新结果，单次进入原Battle Scene/退出；终态后Python读取范围外备份并逐字节写回同一请求，核对SHA。不删除文件、不改Scene、不跑Git丢弃操作。执行前还须确认当前Editor已导入新探针、Console0error、Scene clean且与manifest同SHA。

2026-10-05 执行前再核对：原Editor通过MCP刷新、DLL晚于本次脚本修改；Editor idle/非Play/非编译/测试未运行，唯一活动Battle Scene clean/root11，Console0error，磁盘SHA与manifest一致。Change Ledger1267records/18dirtycodepaths PASS。本操作开始执行；具体时间/进程、工具/API原件、写入/消费/恢复SHA另存execution/after manifest；失败也保留原件，不擅自重试或改自然时钟。

2026-10-05 实际终态：Python单次写请求及原探针消费/生成结果，原Scene `PASS/DONE`、32ticks、已退出Play；未重试。[执行前原件](../../../../artifacts/diagnostics/NTSD28-336B44-Q07-D024-R120-REQUEST-20261005-001/execution-start.json)登记执行者PID、UTC开始、MCP前状态和拟写SHA；[执行后manifest](../../../../artifacts/diagnostics/NTSD28-336B44-Q07-D024-R120-REQUEST-20261005-001/after-manifest.json)登记终态时间/PID、MCP状态、消费及恢复SHA、结果字节与Scene身份。只在结果DONE/Editor非Play且请求仍为本runId的requested=false时，将范围外备份原字节写回；68字节/SHA `7486F5BB3126869EC1D32C6033B68C6A94A19797573BCD0AEE21ED220B985BF1`严格恢复。结果SHA `A4BE804DBAC6E76FB95B463D6B97C5D9B8D4BD8698D9C51B73182B1D5DAF1FA9`，原件保留。Scene clean/root11/SHA `253B2EBA…78F9010`不变，Console0error，未删除任何文件，未执行Git丢弃或修改Scene。前后Python断言均退出0；详[限定验收](../../../../artifacts/diagnostics/NTSD28-336B44-Q07-D024-VERTICAL-FRACTION-20261005/R120-SCENE-ACCEPTANCE.md)。
