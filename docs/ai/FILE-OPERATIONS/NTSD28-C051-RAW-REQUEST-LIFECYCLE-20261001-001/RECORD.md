# C051 左右raw请求文件生命周期

Operation ID：`NTSD28-C051-RAW-REQUEST-LIFECYCLE-20261001-001`。状态：`VERIFIED`（仅请求生命周期；下文保留计划与实际事件）。

需求与授权来源：用户批准并要求继续NTSD28战斗对齐，C051现有Task/Change允许原Editor左右12tick诊断；本项仅为该诊断新生成请求文件的消费后清理。遵守2026-10-01文件删除留痕要求。

Task/Change：`NTSD28-336B44-Q07-C051-UNARMORED-DIRECTION-001`。执行代理：本聊天/root；实际清理执行者：原项目Unity Editor（当前PID105896）中的 `NTSD28UnityRawCaptureEditor.PollRequest`，其finally调用 `File.Delete(RequestAbsolutePath)`。

工作目录：`I:/GitHub/Unity_GAS/gameplay-ability-system-for-unity`。唯一清理路径：`I:/GitHub/Unity_GAS/gameplay-ability-system-for-unity/Temp/NTSD28UnityTrace/unity-raw-capture.request.json`。右、左两个有界周期顺序复用该路径；每轮创建前确认不存在，不覆盖他人请求。无DAT、图片、Scene或已有结果文件删除。

前置状态：该请求文件不存在；新输出均确认不存在。将右/左payload精确复制为新请求文件，原Editor消费并清理；每轮实际启动时间、结果及请求消失状态另追加。备份及哈希先于创建落盘：

- [manifest](../../../../artifacts/diagnostics/NTSD28-336B44-Q07-C051-UNITY-DRIVER-001/request-lifecycle-manifest-v2.json)，包括逐周期路径、Bytes、SHA-256及执行入口。
- [右payload](../../../../artifacts/diagnostics/NTSD28-336B44-Q07-C051-UNITY-DRIVER-001/raw-right-x550-v2.request.json)：677 bytes，SHA `7C3ED4360AA66C7E1980B7A3F264A15B8076B994F88F4F3ADF08C45E493567EE`。
- [左payload](../../../../artifacts/diagnostics/NTSD28-336B44-Q07-C051-UNITY-DRIVER-001/raw-left-x350-v2.request.json)：672 bytes，SHA `CFCE34976C40B6BC8005ACF9E198FF07FA7480F6F1F089139A7E07FC20EC6A19`。
- 四项Scene/Asset保护哈希：[快照](../../../../artifacts/diagnostics/NTSD28-336B44-Q07-C051-UNITY-DRIVER-001/protected-resume-before.json)。请求是本次新生成的未跟踪数据，可从payload恢复；原状态为不存在，清理后应回到不存在。

计划创建命令：`Copy-Item -LiteralPath <对应payload> -Destination Temp/NTSD28UnityTrace/unity-raw-capture.request.json`，仅在目的不存在时执行。计划删除API：已有 `PollRequest` 的finally `File.Delete(RequestAbsolutePath)`，不另行清理。raw、domain、input-rng与result分别使用新 `v2` 路径，旧证据保留。

验收：每轮result实际PASS、raw完成12tick、请求已不存在，左右对当前根正式EXE声明字段同态，四项保护哈希保持且LoganRuntime无Git差异。失败也记原件，不删除输出或盲目重跑。此Operation不代表C051/Q07完整关闭。

2026-10-01T16:26:49.8198250+08:00 状态RUNNING，右周期执行：Copy-Item -LiteralPath artifacts/diagnostics/NTSD28-336B44-Q07-C051-UNITY-DRIVER-001/raw-right-x550-v2.request.json -Destination Temp/NTSD28UnityTrace/unity-raw-capture.request.json；创建前请求不存在。

2026-10-01T16:27:56.1391780+08:00 右周期结果PASS、请求已不存在；右result/raw证据保持。左周期执行：Copy-Item -LiteralPath artifacts/diagnostics/NTSD28-336B44-Q07-C051-UNITY-DRIVER-001/raw-left-x350-v2.request.json -Destination Temp/NTSD28UnityTrace/unity-raw-capture.request.json。创建前目的不存在。

2026-10-01T16:34:58.4808114+08:00 操作后：左/右result均PASS，各完成12tick；请求已不存在，原Editor PID105896存活。请求消费的既有finally清理已完成观察，未单独执行删除命令。四项保护SHA无变化，LoganRuntime Git状态无差异。按同初态根右v1/左v2分别核对action/HP/Vx各99项及OID各33项，全0差；比较报告c051-direction-v3-comparison.json。首轮v2比较误读历史左根v1朝向，原件保留并以v3更正。状态VERIFIED（仅本请求生命周期）。
