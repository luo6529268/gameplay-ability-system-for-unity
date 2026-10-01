# C051 护甲原 Scene Play 临时请求生命周期

Operation ID：`NTSD28-C051-ARMOR-SCENE-REQUEST-20261001-001`；状态 `PLANNED`。用户已授权持续推进新版 336B44 对齐，并要求所有删除留痕。此记录仅覆盖本包新建的单一临时 Editor 请求文件在现有原项目 Editor 消费后由本包 Poll 删除；不涉及 DAT、图片、Scene、Prefab、已有诊断原件或其它项目。关联 Task/Change：`NTSD28-336B44-Q07-C051-ARMOR-SCENE-001`。

执行后状态：`VERIFIED`，仅临时请求生命周期；上段 `PLANNED` 为事前记录，保留原文。

精确路径：`I:/GitHub/Unity_GAS/gameplay-ability-system-for-unity/Temp/NTSD28_Q07_C051ArmorScenePlay.request.json`。操作前须核该路径不存在；每次新建前再次核不存在。预期只有本任务写入的 JSON 载荷：`requested=true`、`targetOid=97` 或 `2`、唯一 runId。脚本须校验载荷和唯一输出不存在，再启动原 Editor 的 Play；请求消费后只删除该路径。每一轮事前 payload 原件、字节数和 SHA 放在 `artifacts/diagnostics/NTSD28-336B44-Q07-C051-ARMOR-SCENE-001/`，失败保留原件，不盲重跑或清理任何已有文件。请求原先不存在，删除后返回原状态；恢复来源为上述 payload 原件。

操作前还须核原 Editor 为原项目、单个 clean Battle Scene、非 Play、未编译，四保护文件 Menu/Battle/GameConfig/Mode Asset SHA 稳定，两个新结果路径不存在。单轮结果/请求消失后才可创建下一轮。最后记录执行者 PID、实际起止时间、命令、请求消失、结果、四 SHA、Scene clean 和 LoganRuntime Git 状态。此记录不授权删除其它文件，也不代表 C051/Q07 关闭。

实际操作：原 Editor PID 105896，经本地 Unity-MCP 桥读得 Battle Scene 单场景、idle、非 Play、`isDirty=false`，新脚本已导入编译。`2026-10-01T09:56:58.4624771Z` 使用 `[IO.File]::Copy(source, request, false)` 创建护甲载荷，Editor 探针在校验唯一 payload 和新结果不存在后以 `File.Delete` 仅消费该新请求；护甲结果 `SCOPED_PASS`、12 tick、退出 Play、Scene clean。确认请求已不存在、原 Editor 再次 idle/clean、四 SHA 相同后，于 `2026-10-01T09:59:10.5346730Z` 同法创建无甲载荷；Editor 同法消费，结果也为 `SCOPED_PASS`、12 tick、退出 Play、Scene clean。未发独立删除命令，两个载荷原件均保留。

[事后逐项复核](../../../../artifacts/diagnostics/NTSD28-336B44-Q07-C051-ARMOR-SCENE-001/request-post-v1.json)：`2026-10-01T18:01:52.5781330+08:00` 唯一请求不存在；两结果 SHA 分别 `808B739FB8AE3A2E3540CE373B416C209CE57471549FBBCC21584182AF6ED7D3`、`A9A6E732CE9A1D3C88F739A0C8480ADB8E36F6B034989ED33A579D2A60878659`；四保护 SHA 与事前清单逐项相同，LoganRuntime Git 无差异。[原 Editor 状态](../../../../artifacts/diagnostics/NTSD28-336B44-Q07-C051-ARMOR-SCENE-001/editor-final-state-v1.json)为 idle/非 Play、Battle Scene `isDirty=false`。两例各132选定字段同根零差的比较原件只证明受控 Scene 出口，不扩大本删除审计授权。
事前逐项清单已生成：[request-manifest-v1.json](../../../../artifacts/diagnostics/NTSD28-336B44-Q07-C051-ARMOR-SCENE-001/request-manifest-v1.json)。两份载荷各65 bytes：护甲OID97 SHA-256 `95300545E5AE0203212C4EB9B167D42978B0CE3840E7B511A84313C952D8E588`；无甲OID2 SHA-256 `587BE6E8F5A5283AEA21E51BE7B2562A90672A90C7EF156F84AE27507C1893A0`。原请求不存在、两结果不存在、原Editor PID105896在Battle Scene非Play/scene clean、脚本程序集晚于源码且生成Editor工程0错；四保护SHA见清单。尚未创建临时请求，状态 PLANNED。
