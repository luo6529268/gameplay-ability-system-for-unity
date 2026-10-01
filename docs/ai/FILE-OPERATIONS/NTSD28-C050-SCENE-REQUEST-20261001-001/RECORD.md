# C050 原 Scene 探针请求生命周期

Operation ID：`NTSD28-C050-SCENE-REQUEST-20261001-001`。状态：`PLANNED`。类型：本包新建临时请求文件，原 Unity Editor 精确校验后自动消费删除。关联 Task/Change：`NTSD28-336B44-Q07-C050-SCENE-PLAY-001`。需求来源：用户已启动并要求继续新版 336B44 战斗对齐；用户 2026-10-01 要求任何删除操作事前留痕。执行者：当前 Codex 主任务；原 Editor PID `105896`。工作及批准根目录：`I:\GitHub\Unity_GAS\gameplay-ability-system-for-unity`。

唯一可能删除的绝对路径：`I:\GitHub\Unity_GAS\gameplay-ability-system-for-unity\Temp\NTSD28_Q07_C050VerticalScenePlay.request.json`。事前已核为不存在、Git 未跟踪；不是用户已有内容。两轮分别以 `File.Copy(source, request, overwrite:false)` 写入新请求；只有前一轮结果、请求消失、Editor idle/Scene clean 经验证后才可启动下一轮。探针 `NTSD28Q07C050VerticalSkipScenePlayProbeEditor.PollRequest` 校验 `requested=true`、targetX 属于 520/1200、唯一 runId、单一 clean Battle Scene、新结果不存在，然后用 `File.Delete(path)` 只消费这一个请求。无通配符、递归、目录删除、DAT/图片/Scene/Prefab/已有文件覆盖。若发现请求已存在或输出已存在则不执行。

[事前逐项 manifest 与四个保护哈希](../../../../artifacts/diagnostics/NTSD28-336B44-Q07-C050-SCENE-PLAY-001/request-manifest-v1.json) 已落盘。两个源载荷在该诊断目录中，按 `FileMode.CreateNew` 保存且不在删除范围：`x520-scene-v1-request.json` 56 bytes，SHA-256 `30B7A8278DE6B534E3812BDF687D757D4C1963A9322F13381E4EE8DCF4AFC145`；`x1200-scene-v1-request.json` 58 bytes，SHA-256 `AC7A8960E827AAB9930A4C2FA0CB9988E7A4FABC6DE90D6F91B73BC614AE483E`。删除后的恢复来源就是对应源载荷；恢复时仍需核请求不存在并使用 `File.Copy(..., false)`。两轮结果预期用 `FileMode.CreateNew` 存为 `x520-scene-v1.json`、`x1200-scene-v1.json`，事前均不存在，结果不会被清理。

事前原 Editor 经本地 Unity-MCP 桥确认为 idle、非 Play、单一 `NTSD_Battle` 场景且 `isDirty=false`；新探针 `.cs/.meta` 已导入，生成 Editor 工程 `dotnet build --no-restore` 为 0 错，原 Editor 程序集时间晚于脚本。每轮执行前须重新检查相同条件。四保护 SHA 在 manifest 中；操作后核请求不存在、结果身份、四 SHA 不变、Scene clean、LoganRuntime Git 无差异。开始时间、实际 API/PID、完成或异常、后状态和范围外检查在运行后追加；本记录不证明 C050/Q07 完成。

近距第一轮预计开始：`2026-10-01T10:23:27Z`（北京时间 `18:23:27+08:00`）。该时间先于请求创建落盘；执行命令仅为 `[IO.File]::Copy($source, $request, $false)`，执行后以实际时间和结果补证。

近距实际：`2026-10-01T10:24:00.5209487Z` 用上述 PowerShell .NET `File.Copy` 创建请求，SHA 等于事前载荷 `30B7A827...AFC145`，进程为当前 Codex shell；Editor PID105896 在校验后自动 `File.Delete` 同一临时请求。输出 `x520-scene-v1.json` 已存在，`SCOPED_PASS / DONE`、三 tick、已退出Play、Scene clean，四个 SHA 相同。请求当前不存在；第二轮结果当前不存在；Editor 再次 idle、非Play且单一 Battle Scene `isDirty=false`。远距第二轮拟于 `2026-10-01T10:26:40Z` 后使用同一 `File.Copy(..., false)` 命令；本句在远距请求创建前落盘。

远距实际：`2026-10-01T10:26:54.8409026Z` 用同一 `File.Copy(..., false)` 创建唯一请求，`File.Copy` 无异常；紧接着的临时请求 `Get-FileHash` 报“Could not find file”，因为 Editor 已在读取时消费该请求，不能将此报错误记为复制失败或请求数据损坏。事前源载荷 SHA `AC7A8960...AE483E` 保留；目标请求由探针严格校验 `targetX=1200/runId=x1200-scene-v1` 后，才生成 `SCOPED_PASS / DONE` 结果。请求事后不存在、远距结果 SHA `703461345BFC369927D2E210E009081FE29D2BBA883CF81B7CA7AE751AF86642`，近距结果 SHA `09102D8182B417A8B080692F1A58C9594976B352715A71F43574970E9672A9C9`。

事后状态：`VERIFIED`（只覆盖上述两个本包新建临时请求生命周期）。[事后逐项检查](../../../../artifacts/diagnostics/NTSD28-336B44-Q07-C050-SCENE-PLAY-001/request-post-v1.json)：原 Editor PID105896 为 idle/非Play/非编译，Battle Scene `isDirty=false`；四保护 SHA 与[事前 manifest](../../../../artifacts/diagnostics/NTSD28-336B44-Q07-C050-SCENE-PLAY-001/request-manifest-v1.json)逐项相同，LoganRuntime Git 无差异；唯一请求不存在，两结果均存在。实际触及的删除仅 `Temp/NTSD28_Q07_C050VerticalScenePlay.request.json` 两次；其它文件删除、覆盖或移动均未执行。请求原先不存在，现恢复原状态；载荷备份仍在本包诊断目录。Scene 结果及逐字段根对照见[限定验收](../../../../artifacts/diagnostics/NTSD28-336B44-Q07-C050-SCENE-PLAY-001/ACCEPTANCE.md)。
