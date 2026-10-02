# Q07/C042 延迟自然抓取非零计数三方定向验收

范围：正式 OID75/action355/X500 抓取鸣人 OID2/action0，目标初始 X650 与 X800；新版正式 EXE SHA-256 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3`。本报告只验 C042 自然可达的非零动作计数路径，不代表 Q07、完整 World 或物理按键出口关闭。

| 目标 X | 抓取 tick | 投掷 tick | 投前被投者计数 | 正式源码 ↔ 正式根 EXE | 正式源码 ↔ 原 Unity Battle Scene |
| --- | ---: | ---: | ---: | --- | --- |
| 650 | 2 | 56 | 1 | 160 tick × 20 字段，3200/3200 相同 | 60 tick × 20 字段，1200/1200 相同 |
| 800 | 4 | 58 | 3 | 160 tick × 20 字段，3200/3200 相同 | 60 tick × 20 字段，1200/1200 相同 |

正式根两例 `passed=true`、`failureCode=0`；比较排除根 LFR 附加终端 tick、因载体来源不同的 `crtState` 和未选 World 字段。原 Scene 两例报告均 `CAPTURED/DONE`、`exitedPlay=true`、`sceneCleanAfter=true`，无报告错误。投掷完成后的完整 tick 中，X650 被投者计数为 1，X800 为 1；后者从投前 3 到 1 是 action181 帧入口正常重置后的结果，不能写成“整 tick 后仍为 3”。共用投掷 writer 的即时非零保留已由父任务的 392 例聚焦检查覆盖；本次 Scene 比较补自然可达触发与完整 tick 同态，不直接观测帧入口前的瞬时内部值。

只改现有 Editor 探针请求位置参数；生产脚本、DAT 数值、图片、Scene、Prefab 与配置资产没有因本子包修改。`dotnet build Assembly-CSharp-Editor.csproj --no-restore -nologo '-clp:ErrorsOnly;Summary'`：0 error、251 warning；原 Unity Editor 刷新后测试程序集时间晚于探针修改。`Tools/Validate-ChangeLedger.ps1` PASS（1134 Records；现有历史声明冗余警告保留），`git -c core.safecrlf=false diff --check` 通过。原 Battle/Menu Scene、GameConfig 与 ProjectBattleModeConfig 的前后 SHA 均相同。现有 X550 结果未覆盖，未重跑完整 SelfCheck，因为本次只修改定向探针。未记录对象池借用计数，不能以 Scene clean 推断其归零。Q07/C042 其它动作、完整 World、物理按键和整场出口保持开放。

原件：本目录 `a355-x650-source-root-comparison.json`、`a355-x800-source-root-comparison.json`、`a355-x650-source-unity-scene-comparison.json`、`a355-x800-source-unity-scene-comparison.json` 和 `protected-before.json`/`protected-after.json`；原 Scene 输出在相邻 `NTSD28-336B44-Q07-C042-NATURAL-SCENE-001/bee75-a355-x650-natural-scene-01.json` 与 `bee75-a355-x800-natural-scene-01.json`。
