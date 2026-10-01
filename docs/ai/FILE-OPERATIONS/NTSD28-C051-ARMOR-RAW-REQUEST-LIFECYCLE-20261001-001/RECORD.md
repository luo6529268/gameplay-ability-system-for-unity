# C051 护甲 raw 临时请求生命周期

Operation ID：`NTSD28-C051-ARMOR-RAW-REQUEST-LIFECYCLE-20261001-001`；当前状态 `VERIFIED`。下文保留事前计划与实际事件。需求与授权来源：用户已启动并要求继续当前 NTSD28 战斗对齐，要求所有删除留痕；`NTSD28-336B44-Q07-C051-ARMOR-UNITY-DRIVER-001` 已声明原 Editor 定向 raw 验证。此项只记录本轮**新建**临时请求在现有消费者读取后自动清理；沿用此前 C051 左右 raw 请求生命周期的已批准工作方式，不触碰任何旧资源。

Task/Change：`NTSD28-336B44-Q07-C051-ARMOR-UNITY-DRIVER-001`。执行代理：当前聊天/root；实际读取与清理执行者：原项目 Unity Editor 中 `NTSD28UnityRawCaptureEditor.PollRequest` 的 finally `File.Delete(RequestAbsolutePath)`。工作目录及批准根目录：`I:/GitHub/Unity_GAS/gameplay-ability-system-for-unity`。唯一会被删除的精确路径：`I:/GitHub/Unity_GAS/gameplay-ability-system-for-unity/Temp/NTSD28UnityTrace/unity-raw-capture.request.json`。该路径创建前已核不存在，右/对照两周期顺序复用；每次重新创建前再次确认不存在。

操作前[逐项 manifest](../../../../artifacts/diagnostics/NTSD28-336B44-Q07-C051-ARMOR-UNITY-DRIVER-001/request-lifecycle-manifest-v1.json)记录绝对路径、原状态、两份恢复 payload 的字节数/SHA、各自四个全新输出路径以及 Menu/Battle/GameConfig/Mode Asset 哈希。新请求无预存内容，恢复来源即以下范围外 payload；无 Git 已跟踪请求文件、无 meta/GUID。不得删除或覆盖 DAT、图片、Scene、旧 raw/结果或其它路径。

- [护甲 payload](../../../../artifacts/diagnostics/NTSD28-336B44-Q07-C051-ARMOR-UNITY-DRIVER-001/armor97-x550-request-v1.json)：705 bytes，SHA-256 `9B27D48FE94992BD96DCE168CC1A80E772A35F5B21DEE2A14ADA9577EC85E6C8`。
- [无甲 payload](../../../../artifacts/diagnostics/NTSD28-336B44-Q07-C051-ARMOR-UNITY-DRIVER-001/control2-x550-request-v1.json)：710 bytes，SHA-256 `6AD063AD3978DE71D037097CE7E0605EBF1BEAF4EFCCCCAFCB50E04D3185AA64`。

拟执行：在原 Editor idle、原 Battle Scene 保持且两份结果均不存在时，`Copy-Item -LiteralPath <单个 payload> -Destination Temp/NTSD28UnityTrace/unity-raw-capture.request.json`；已有 Editor 消费、写新 raw/domain/input-rng/result，finally 自动 `File.Delete` 仅该临时请求。先核第一轮结果和请求消失，再创建第二轮。无需单独清理命令；若任一前置不满足则不创建。

验收：每轮消费后请求不存在、结果及输出可查、原 Editor idle；四保护 SHA 不变、LoganRuntime Git无差异。失败保留原件，不删除输出或盲重跑；此操作仅为临时请求清理的授权与审计，不代表 C051/Q07 关闭。实际开始/结束时间、Editor PID、命令、结果和哈希在执行后追加。回滚路径：请求原先不存在，自动删除后恢复原状态；payload 和结果原件不删。

2026-10-01T17:32:50.1812033+08:00 状态 RUNNING：原Editor PID 105896，护甲周期创建前request/result不存在；执行 Copy-Item -LiteralPath artifacts/diagnostics/NTSD28-336B44-Q07-C051-ARMOR-UNITY-DRIVER-001/armor97-x550-request-v1.json -Destination Temp/NTSD28UnityTrace/unity-raw-capture.request.json。

2026-10-01T17:33:29.1950908+08:00 护甲周期结果PASS，request已不存在，四项新输出存在；无甲周期创建前request/result不存在，执行 Copy-Item -LiteralPath artifacts\diagnostics\NTSD28-336B44-Q07-C051-ARMOR-UNITY-DRIVER-001\control2-x550-request-v1.json -Destination Temp/NTSD28UnityTrace/unity-raw-capture.request.json。

2026-10-01T17:37:16.1036388+08:00 操作后：无甲周期result PASS；两周期raw/domain/input-rng/result共8个新输出存在，请求均已由原Editor PollRequest 消费后清理，当前精确请求路径不存在。执行者原Editor PID105896；MCP读取实例 gameplay-ability-system-for-unity@b1b02287 为原Battle Scene、idle、非Play、ready。四保护SHA前后相同，LoganRuntime Git无差异，比较两组各12tick×11字段132/132零差。核验见 artifacts/diagnostics/NTSD28-336B44-Q07-C051-ARMOR-UNITY-DRIVER-001/request-lifecycle-after-v1.json 与 unity-root-selected-fields-comparison-v1.json。未执行独立删除命令，未触碰其它路径。状态 VERIFIED（仅临时请求生命周期）。
