# NTSD28-336B44-Q07-D024-SCENE-REQUEST-20261005-001

> 2026-10-05 16:51:43.8549668+08:00：`RUNNING`；下文 `PLANNED` 是执行前快照。已复查目标与Scene SHA、原Editor idle/非Play/Battle单Scene clean、新结果不存在。

> 2026-10-05 16:54:26.4912970+08:00：`VERIFIED`。PowerShell PID 124728 在 16:52:26.9315654+08:00 将唯一临时目标写为 `requested=true/runId=d024-vertical-green-20261005-01`（提交后 SHA `44AEB6D1B82E753DC7FDDBE165D0B96283495FF91271B7F4CF2A9F3459F2629C`）；原 Editor 的既有探针自动将其置回 false，生成唯一[结果](../../../../artifacts/diagnostics/NTSD28-336B44-Q07-C023-AIRBORNE-SCENE-PLAY-001/d024-vertical-green-20261005-01.json)，`PASS/DONE`、退出 Play、Scene clean。PowerShell PID 142812 在 Editor idle/非Play/Scene clean、结果终态及旧请求 runId 未被其它进程改动的复查后，将同目录 `before-request.json` 逐字节写回；目标最终 SHA 与备份/操作前同为 `7486F5BB3126869EC1D32C6033B68C6A94A19797573BCD0AEE21ED220B985BF1`，Scene 前后 SHA 同为 `253B2EBAD322AA6EC18488DFBBF555BB93AD897940A342E4130E77CE778F9010`。新结果 SHA `F3D090762B3508CB21AE3442DCE44CA41AABFE39A6B3739F64A3BBA75AF3C5DF`，未覆盖旧诊断；实际操作成功退出0、无异常，未观察到范围外 Scene 内容变化。原始命令输出留在本聊天工具调用，结果与本记录双向回链；未修改资产 GUID/.meta。

状态：`PLANNED`。类型：仅覆盖一个已有忽略的临时 Play 请求文件并在探针结束后按逐字节备份恢复，不删除项目资源。Task/Change：`NTSD28-336B44-Q07-D024-VERTICAL-PROJECTION-001`。原因：对生产 Y 投影修复在原 Battle Scene 复用同一 OID85/action212 自然正例，使用唯一 runId `d024-vertical-green-20261005-01` 写新结果；用户已启动战斗对齐总目标、允许原 Editor 空闲时定向 Play，且确认该角色本体尺寸保持现状。执行者：本聊天 root；工作目录 `I:\GitHub\Unity_GAS\gameplay-ability-system-for-unity`，批准目标根目录即该仓库。

逐文件目标：`I:\GitHub\Unity_GAS\gameplay-ability-system-for-unity\Temp\NTSD28_Q07_AirborneIdleBattlePlay.request.json`；相对路径 `Temp/NTSD28_Q07_AirborneIdleBattlePlay.request.json`，普通文件 68 bytes，Git `!! Temp/`（忽略），操作前 SHA-256 `7486F5BB3126869EC1D32C6033B68C6A94A19797573BCD0AEE21ED220B985BF1`，内容为 `requested=false`、旧 runId `d024-vertical-20261005-01`。无 `.meta`/GUID/资产引用。范围外保护 `Assets/NTSD/Scene/NTSD_Battle.unity` SHA-256 `253B2EBAD322AA6EC18488DFBBF555BB93AD897940A342E4130E77CE778F9010`，原 Editor Battle 单 Scene clean、非 Play。

恢复来源：同目录 `before-request.json` 是操作前从上述目标只读复制出的逐字节备份；2026-10-05 16:51:16+08:00 已核源/备份 SHA-256 均为 `7486F5BB...85BF1`。记录与备份在目标 `Temp` 范围之外。恢复以该备份逐字节写回，仅在探针消费后且 Editor idle/非 Play 时进行；如果目标在操作期间出现其它进程的新改动，停止恢复并保留现场。

拟执行：先复查原 Editor idle、Battle 单 Scene clean、Console 0 error 与场景 SHA；用 PowerShell/.NET `File.WriteAllText` 仅对上述目标写 `{"requested":true,"runId":"d024-vertical-green-20261005-01"}`；现有 `NTSD28Q07AirborneIdleBattlePlayProbeEditor` 的 `Poll/TryStart` 自动将同一文件写回 `requested=false` 并生成**新** `artifacts/diagnostics/NTSD28-336B44-Q07-C023-AIRBORNE-SCENE-PLAY-001/d024-vertical-green-20261005-01.json`（事先核不存在）；完成后复查 Editor 非 Play、Battle Scene clean/SHA不变，再从备份恢复目标并核双哈希。预定命令不会触及其它文件；若失败记录原始结果，绝不覆盖已有诊断结果。

开始时间：2026-10-05 16:51:43.8549668+08:00。实际命令/进程、结束时间、退出码、目标及范围外操作后 SHA、异常与结果：待执行后追加。
