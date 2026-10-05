# NTSD28-336B44-Q07-D024-SPARK-REQUEST-20261005-001

状态：`VERIFIED`（建立时为 `PLANNED`）。执行者：本任务 `/root`。建立时间：2026-10-05T17:26:54+08:00。工作目录：`I:\GitHub\Unity_GAS\gameplay-ability-system-for-unity`。

原因与授权：用户要求继续 336B44 战斗对齐并已确认原 Editor 空闲、可切换；Q07/D-024 火花位置修复需要原 Battle Scene 同条件 Play 验证。用户已确认保留现有 1.5 倍角色图片表现。Change ID：`NTSD28-336B44-Q07-D024-SPARK-VIEW-ANCHOR-001`。

精确范围：新建 `Temp/NTSD28_Q09_C040GameView.request-d024-spark-20261005-01.json`，内容为 `{"requested":true,"mode":"view","runId":"q07-d024-spark-green-20261005-01"}`。既有 `NTSD28Q07C040NaturalScenePlayProbeEditor.TryStart` 消费时只将此文件的 `requested` 改为 `false`，其余请求内容保留。探针另新建独立结果 `artifacts/diagnostics/NTSD28-336B44-Q09-C040-GAMEVIEW-WITNESS-001/q07-d024-spark-green-20261005-01.json` 与配套截图目录，不覆盖现有结果；不得修改 Scene/Prefab/DAT。

操作前清单：两个目标文件均不存在，Git 对上述目标及 Battle Scene 无状态差异；无旧内容需备份。原 Battle Scene SHA-256 `253B2EBAD322AA6EC18488DFBBF555BB93AD897940A342E4130E77CE778F9010`。临时请求创建后会另存精确字节副本至同一 operation 诊断目录并记录 SHA，供消费覆盖审计；恢复仅在明确需要时按副本重建，不自动恢复已消费的 requested=true。

拟执行：以 exclusive-create 建立请求并在探针消费前保存副本；原 Editor 空闲且 Battle Scene clean 时由既有 EditorApplication.update 探针消费请求、运行 Battle Play、退出后检查新结果与 Scene SHA/dirty。只保留消费后的请求，不删除。若 Scene 变 dirty、Play 正在运行或资源前置不满足，则停止，不强行重跑。

实际执行：通过本机 Unity MCP 桥接确认 Editor idle、非 Play、唯一 Battle Scene `isDirty=false` 后，用 Python `open('xb')` 建立请求和操作外原字节副本 `artifacts/diagnostics/NTSD28-336B44-Q07-D024-SPARK-REQUEST-20261005-001/request-before-consumption.json`，两者创建字节数75、SHA-256 `7B4FAC62B18F4E8D3460838B028D392B32A624A5C0D39CD0C8B3837E85985E0C`。既有 `NTSD28Q07C040NaturalScenePlayProbeEditor.TryStart` 自动消费并将 `requested` 写为 false，消费后请求字节数95、SHA-256 `B6874AB67A9F64E26D69D52722A9D3B91901790ED890445DE5448C5500739C64`；请求仍留在 Temp，未删除。其结果独立新建到预声明 runId，`CAPTURED/DONE`、进入并退出 Play、40 samples、保护文件哈希稳定，见[限定验收](../../../artifacts/diagnostics/NTSD28-336B44-Q07-D024-VERTICAL-FRACTION-20261005/SPARK-ACCEPTANCE.md)。

操作后：Battle Scene仍唯一已加载且 `isDirty=false`，磁盘 SHA-256仍 `253B2EBAD322AA6EC18488DFBBF555BB93AD897940A342E4130E77CE778F9010`；Editor idle、非 Play、Console 0 error；结果文件/截图新建无旧文件覆盖，未观察到范围外受保护文件变化。Python与MCP调用成功，无异常。无需要恢复的旧内容；若需复现请求原字节，复制副本到新请求路径并重新审批运行，不以此记录自动重跑。Change ID见本记录首段。
