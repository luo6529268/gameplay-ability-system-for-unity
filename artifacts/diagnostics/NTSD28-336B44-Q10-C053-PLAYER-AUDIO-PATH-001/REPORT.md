# Q10/C053 Windows Player 正式音频路径与打包限定出口

2026-10-02。状态：`SCOPED_PASS`。本包只验证当前正式 020/067 战斗 WAV 在 Windows Mono Development Player 的资源闭包与播放器路径；Q10 全部 cue、声像、实际扬声器输出和最终整场对齐仍开放。当前战斗权威为根正式 EXE SHA-256 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3`；DAT 与角色图来自同版正式 runtime，原版背景和模式 DAT 按用户例外排除。

首差见 [FIRST-DIFFERENCE.md](FIRST-DIFFERENCE.md)：旧 Windows v2 清单包含两个已排除 mode DAT，漏掉当前 `bgm.dat`、`sound.dat` 及正式 020/067 WAV；原 `NTSDSoundPlayer` 在 Player 用 `<EXE>_Data` 误找应位于 EXE 同级 `Assets/NTSD/Content/LoganRuntime/vfs` 的 WAV。新 [v3 清单](copy-manifest-v3.csv)按当前 1373 个非 meta 文件、46,927,691 字节逐原始 SHA 定义打包闭包；1348 个文件与正式资源逐字节相同，25 个 DAT 仅 CRLF/LF 换行不同，严格 CRLF→LF 后相同。未改 DAT 数值或旧 v2 清单；旧 v2 SHA 仍为 `5D3D9C302726541D3AFA477BC347F857E247FB047ECDB6ACAFF3857AA5EBE73F`，v3 SHA 为 `53FD2E7BDDDF4B4A7FDFAD66E7F82AE58313381962DE78D4A3D0A249CCA15B6E`。

生产改动：Windows postbuild 切到 v3 文件数/字节数及逐文件校验；战斗单文件 WAV 从已配置的 `GameConfig.BattleContentRuntimeRoot` 定位，和正式 DAT/图片使用同一根。旧 `Assets/NTSD/Sound` 路径及通用 `PlaySfx` 保留。Editor 测试用临时 GameConfig 实例并恢复静态引用；Development Player 仅显式 `-ntsd-q10-formal-audio-probe` 时附加 WAV 路径、帧数和播放计数检查，默认 Q07 探针行为不变。

验证：

- `dotnet build Assembly-CSharp-Editor.csproj --no-restore -nologo -v:q -clp:ErrorsOnly`：0 error（284 条现有警告）。原 Editor 刷新后两项具名 EditMode 音频路径/解码/AudioSource 测试 2/2 PASS，job `3612308b286645fc861173ccf619169a`。
- 第一次 Windows Mono Development 构建 [01 build](../NTSD28-Q07-CONTENT-MIGRATION-READINESS-001/Q10C053PlayerAudio336B44-20261002-01-build.json)成功；1373 文件逐 SHA 通过。首次 [Player 01](player-01.json)在旧 Q07 固定指纹 `FF1218…` 检查处提前 FAIL，实际当前项目 mode Asset 参与的内容身份为 `868070…`，三处发布键相同。该失败不证明音频路径有误；音频字段未执行，原件和 log 保留。
- 仅在本 Q10 显式探针改为核验当前发布 catalog 的复合身份与三处发布键一致，默认 Q07 固定指纹检查不变。第二次 [02 build](../NTSD28-Q07-CONTENT-MIGRATION-READINESS-001/Q10C053PlayerAudio336B44-20261002-02-build.json)成功，第二版 [打包审计](packaged-audit-02.json) 1373/1373 文件、46,927,691 字节、0 SHA/长度不符。
- 隐藏窗口运行第二版 [Player 02](player-02.json) `PASS`：Battle Scene World 4 对象，序列化正式根，三处发布键一致；已封存播放器从 EXE 同级正式 `vfs/data/020.wav`、`067.wav` 解码 16413/31170 帧，两次事件使 AudioSource 播放计数 +2；有序关闭 `RuntimeMapCleared`，池活跃借用 0，两帧后仍停止，卸载后已跟踪资源存活 0/30872。这是合成两事件的 Player 音频入口证明，不等于在 Player 自然 C053 命中链中听到两声；自然链的原 Battle Scene 12 tick/10 实际播放另见 [前一包](../NTSD28-336B44-Q10-C053-FORMAL-WAV-DEPLOY-001/REPORT.md)。
- 复核发现 02 版把新 Q10 字段放入基础 Q07 Report，默认 Q07 JSON 会多出字段。仅把它们移到显式 `Q10AudioReport` 后，[03 build](../NTSD28-Q07-CONTENT-MIGRATION-READINESS-001/Q10C053PlayerAudio336B44-20261002-03-build.json)、[03 打包审计](packaged-audit-03.json)及 [Player 03](player-03.json)再次 PASS，02 的路径/帧数/+2 播放/池0/资源0结果不变。[默认报告静态结构审计](default-report-schema-audit.json)与 Git HEAD 对比，基础 Report 15 字段数量、顺序和名称完全相同，Q10 字段只在显式子报告。未另跑默认 Q07 Player，因为其旧固定指纹断言在当前项目 mode Asset 内容身份下会先失败；因此默认报告结构是源码层证据，非当前内容下的默认 Q07 Player 成功证书。
- [保护哈希](protected-after.json)显示 Battle/Menu Scene、GameConfig、ProjectBattleModeConfig、EditorBuildSettings 和旧 v2 清单 6/6 与本包运行前一致。未启动第二个 Unity 项目，未用 computer-use，未删除任何文件。
- 最终 `Tools/Validate-ChangeLedger.ps1` exit 0、[完整结果](change-ledger-validator-final.txt)首行 `Change ledger validation PASSED`（1149 Records）；`git diff --check` exit 0。历史 Records 的“声明路径不在当前 diff”警告不影响本包覆盖结果；03 后[保护哈希](protected-after-03.json)仍为 6/6 相同。

未闭：其它正式可达 cue 的精确资源消费者与部署、声像/音量/停止及设备输出、Windows Player 自然 C053 命中链、其它平台资源闭包、Q10/Q11/Q12。后续以具名 cue/自然可达链增量处理，不按全量路径数盲搬 WAV。
