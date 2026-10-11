# H10 formal audio session ownership — 有限实施报告

日期：2026-10-11（Task 按获批日期保留 20261010）
Task：NTSD-H10-FORMAL-AUDIO-SESSION-OWNERSHIP-20261010
Change：NTSD-H10-FORMAL-AUDIO-SESSION-OWNERSHIP-001
Operation：NTSD-H10-FORMAL-AUDIO-SESSION-OWNERSHIP-20261010

**H10_FORMAL_SESSION_OWNERSHIP_APPLIED。** 正式单文件音频源已接入每局所有权与既有 stage1/stage5 关闭；没有默认 OFF 开关。当前通过编译、明确范围的正确性/生命周期、唯一五音频驻留比较和一次最小隔离 Play。完整 H10 producer 音频依赖闭包、整局加载/驻留预算仍 OPEN；本报告不构成 H07、Android、IL2CPP、整局 0GC 或发布认证。

授权来自人类讨论会话“那就先把H10完成了啊”，thread 01a11fe2-bfaa-72f3-a9ed-c8260f63b193，turn 01a1265b-425f-7b63-9a21-f2b5634cf553。准确四脚本交付合同见 [原指令](../NTSD-H10-RESIDENT-CLOSURE-20261010/execution-handoff.txt)；Task/Change/Operation 在修改前建立。没有恢复 Goal 或 34 项连续实施。

## 生产变更与边界

1. NTSDSoundPlayer：只有新加载的正式 single-file 使用既有 loader 的空 key 路径，由本次 cue generation 独占。单声道转双声道完整成功发布后才退役 owned mono source；非 mono 源作为播放输出持有到 stage5。配置 fallback、generic/folder 和全局缓存中的旧 clip 继续借用。
2. 每个 player 的 cold prepare 串行：新请求使旧 generation 失效，等旧 prepare/pending local I/O 收尾；旧附着资源先申请 Destroy，再跨帧等待，最后才能开始新 generation。迟到结果先落 local，核对 owner/controller/generation/当前 cue，再 attach；失效结果静态回收。stage5 完成与 I/O tail drain 分开记录。
3. SimulationTickDriver 在 Preparing 捕获准确 sound owner；原 stage1 拒绝准备/战斗声音接单，原 stage5 清 publication 后停止/解绑引用 owned 输出的 voices、释放声音资源。stage11 完成后清 owner 引用。诊断 sink 不得替换已捕获 owner。
4. AppManager 仅补必要绑定与无 driver 关闭分支，覆盖 Driver 与 AppManager 的两种创建顺序。OnDestroy 使用同一幂等清理，迟到 I/O 不得复活销毁 owner。

实际生产写域仅 App/NTSDSoundPlayer.cs、Simulation/Host/SimulationTickDriver.cs、App/AppManager.cs；测试写域仅 Test/Editor/H10ResidentClosureDiagnostics.cs。相对真实 dirty 前像的 [最终差异](production-delta-final.patch) 可复核。没有改 loader、LoadTask、全局缓存算法、角色 manager、Scene/Prefab/资源/ProjectSettings/Input Actions/Server/Gen/Plugins，没有读活跃 Q06 方法体。

新增状态全部由 NTSDSoundPlayer 持有：cold request/generation、pending counter、prepare/closed/disposed/shutdown 标志、正式源列表和纯数值诊断。列表只在冷准备按已收集 cue 容量保留，mono 成功后即移除；stage1 失效、stage5 detach/clear，OnDestroy 幂等回收。无新热路径池或 cache，无新增关闭阶段。声音增益、声道数学、64 voices、重触发、cue/事件选择保持；先前有界 PCM helper 的归一化 SHA-256 保持 d1d98ac38f3e065d1ce45e463358189957a98e1173bd6681316392134b4f8b58。

独立静态复核发现并在最终门前修复了：旧输出 Destroy 必须早于冷重入 NextFrame；诊断 sink 的 owner 替换漏洞；当前 controller 改变导致准备返回却未 sealed 时必须显式失败。详 [独立复核](independent-static-review.md)。

## 实际验证与失败保留

使用原项目唯一 Unity 2022.3.62f3 Editor（PID 39108），通过现有本机 127.0.0.1:6402 bridge 执行；没有启动第二个 Library 实例。每次运行前核 editor idle、非用户 Play、原 Menu clean8，每次测试前备份共享 Temp XML，不清 Console。完整参数/时间见 [commands.jsonl](commands.jsonl)。

| 层级与原始结果 | 实际结果与限制 |
|---|---|
| 编译/热重载 | PASS；第一轮 List.EnsureCapacity 编译失败已保存，改成等价 cold Capacity reserve 后编译通过。最新 preflight 改动重新编译通过，final-console 只有既有 MCP transport 异常记录，无编译 error。 |
| [契约 RED](red-tests.xml) | 1 执行、1 预期失败：旧生产没有 BeginBattleSoundShutdown；发生在 loader 之前，不能当旧版内存失败证据。 |
| [首次真实生产 source/PCM/stage5](green-first-tests.xml) | 1/1 PASS。该请求中的旧音频类 namespace 当时错误，未选中，明确 NOT_EXECUTED；随后正确选中运行。 |
| [兼容性组](compat-tests.xml) | 14 执行、13 PASS、1 FAIL。10 项 NTSD28OriginalCommonAudioEditorTests、1 项完整有序关闭、2 项 Preparing-owner/continuation 通过。失败保留，详下段。 |
| [生命周期组](lifecycle-tests.xml) | 7/7 PASS。实际取消 pending 本地加载、await 期间真实 Unity OnDestroy、并发准备只保留最新 generation、同 controller 冷重入、controller 改变失败/重建、借用 fallback/generic、两种 Driver 创建顺序/无 Driver/诊断 owner 替换拒绝。 |
| [null/真实转换失败](failure-tests.xml) | 1/1 PASS。真实 streaming AudioClip.GetData 错误、owned input 释放、之前成功输出保留到 stage5、borrowed mono 不销毁、无配置 missing fallback 为空。不是实际异步解码器故障注入。 |
| [唯一资源比较](resource-tests.xml) | 1/1 PASS；五个冻结 WAV，旧隔离 cached owner 与实际生产 owner 串行，逐输出 PCM/metadata 相等，owned 最终归零。未重跑。 |
| [Play 预检失败](play-preflight-failed-tests.xml) | 1 FAIL，进入 Play 前 sceneCount 预期错误。查本机 Test Framework 1.1.33 EditModeLauncher 自动加空 untitled additive 场景；marker/场景变更/Play 均未发生。修正为原 Menu clean8 加框架 empty clean 场景，原失败保留。 |
| [唯一实际隔离 Play](play-tests.xml) | 1/1 PASS，7.529020 s；实际 EnterPlayMode/ExitPlayMode，2 个固定音频、临时空场景，原 Menu 未作为 Play 场景，不修改任何 Scene 资产或 Settings。 |

兼容性失败为 SoundPresentationDispatchEditorTests.BattleStereoVoice_UsesClipSpecificMatrixAndRetainsGainAfterVolumeChange，line129 要求 voice 引用原 mono clip。真实 dirty 前像的 PlayPreparedCue 已使用新建 stereo BattlePlaybackClips，测试 helper 只注入 Clips/IsLoaded。独立静态证据确认此测试前置假定在本 Task 前已不适用；这是**静态基线归因，不是旧版本实跑的因果证明**。该旧测试超出四文件写域，保持原样；不能写成 14/14 PASS 或“所有声音回归通过”。现有符合 stereo 路径的 MonoPlaybackAcceptsDirectSoundCenterGain 已实际通过。

生命周期的三次 manager prepare/play/shutdown 外加三次 builtin 冷重入均跟踪真实 source/output IDs；关闭后 native reference/Resources ID 均消失。pending1 时 stage5 可完成而 tail 尚未 drain，await 返回后 pending0/stale ID 消失。Editor fixture 的 player host inactive，验证调用、voice 绑定和 PCM，不证明可听波形；真实 Play 延迟销毁另列。

Play [结果](isolated-play-deferred.json)：data/001.wav 的 owned mono source 在申请 Destroy 时仍活着，下一帧消失；c/kim/w/a1.wav 保持 source/output 同一 native ID。调用实际 public PrepareBattleCuesAsync(null) 冷重入后，stage5 返回尚有两个待销毁 output；voices 已解绑，下一帧全部消失，等待期间未新增 load。等待中主动关闭请求，未加载额外 builtin 音频。host 实际 Destroy 后归零，global cache 不变，没有 AppManager/Driver/战斗启动。MCP get_test_job 的缓存状态在 domain reload 后仍停留 running/result null；**最终 Test Framework XML 的 1/1 PASS、实际 JSON 和 Editor tests.is_running=false 为完成证据**，不以 bridge job 缓存冒充完成。一次连接拒绝发生于正常 reload，未重启 Editor 或重跑 Play。

## 唯一固定资源组结果

冻结四个 mono：c/jira/w/e3.wav、c/kar/w/t.wav、c/dan/w/t.wav、data/001.wav；非 mono 对照 c/kim/w/a1.wav。文件身份见 [WAV 指纹](fixed-wav-identities.json)，完整分阶段数据见 [bounded-resource-stages.json](bounded-resource-stages.json)。

旧组在隔离 loader 中使用原非空 cache key 和冻结旧 PCM 方法，新组实际调用现有生产 cue/load/convert；旧组所有 owned 对象销毁后才运行新组。两组输出 name/samples/channels/frequency、float 位 SHA 完全相等。

| steady 去重口径 | 旧持有方式 | 本次生产方式 |
|---|---:|---:|
| distinct native AudioClip 数量 | 9 | 5 |
| distinct native object bytes | 7,626,693 | 6,778,456 |
| 输出对象 bytes | 6,778,456 | 6,778,456 |
| 仍附着的 source 数量 | 5 | 1 |
| 关闭后 owned native objects | 0 | 0 |

减少 **848,237 B（约 0.809 MiB、该组 distinct bytes 的 11.122%）**，恰为四个不再重复持有的 mono source。非 mono 的 source 与 output 同一对象，source/output 计数不可相加。Profiler 对象值不是进程 RSS、整局驻留总量、Unity 解码峰值或设备峰值。

| WAV | source object B | output object B | 临时 float payload B |
|---|---:|---:|---:|
| c/jira/w/e3.wav | 332,084 | 2,636,720 | 67,920 |
| c/kar/w/t.wav | 262,424 | 2,079,440 | 67,584 |
| c/dan/w/t.wav | 239,105 | 1,892,888 | 82,668 |
| data/001.wav | 14,624 | 97,040 | 92,256 |
| c/kim/w/a1.wav | 72,368 | 72,368 | 0 |

临时 payload 是数组数据量解析值，旧/新均沿用已通过的 bounded converter；不是新增 Profiler peak 实测。大于对象驻留的总耗时不能从这里推导。

本组仅保留一次阶段时长，不作 CPU 性能结论：按上表顺序，旧 load ms 为 18.8100/60.4383/5.3249/10.1894/7.6925；新为 7.6085/15.4999/6.8778/4.9625/9.4148。旧 conversion 合计 4.8490ms，新合计 5.1087ms；新测量包装含 private reflection 与 source release、旧为 typed conversion，且单次 I/O/Editor 排程不等价。60.4383ms 原样保留，原因未测；**不宣称 CPU 加速或成本无回退证书**，未另开 ABBA/长窗寻找 PASS。

资源组前后四源码/两程序集 SHA 均相同。之后只修改 Play 测试的框架场景预检，生产三源码与 runtime assembly 保持冻结；最终精确差异见 final-fingerprints.json。

## 命令与复用

实际 bridge 参数（通过上述本机 TCP JSON framing 执行，详情含 job IDs 见 commands.jsonl）：

- get_editor_state {}；manage_scene {"action":"get_loaded_scenes"}。
- refresh_unity {"mode":"if_dirty","scope":"scripts","compile":"request","wait_for_ready":false}。
- run_tests {"mode":"EditMode","testNames":[准确方法]}；get_test_job {"job_id":"...","include_details":true,"include_failed_tests":true}；read_console {"action":"get","types":["error"],"count":10,"format":"detailed"}。
- 唯一 Play testNames=["NTSD.Test.Editor.H10ResidentClosureDiagnostics.FormalAudioOneIsolatedPlayDeferredDestroy"]，job39ec998c69e04f56b7921dfe14468e68。
- 唯一资源 job47d6bf93e70240729073bb61fda6eebf；生命周期 job47f794746d404b92a7f77bf3bb1d5470；失败清理 job2d65b5a584bf4f6bb31578cd124c2374。

审计执行脚本原文保存在 Operation/final-audit-code-v2.txt（以 Python exec 执行该文本）；其命令包括 git status --short --branch --untracked-files=all、git diff --no-ext-diff --cached --binary、git rev-parse HEAD、git diff --no-ext-diff --check -- 准确 scoped 文件。Change Ledger 原命令：
```powershell
pwsh -NoProfile -File Tools/Validate-ChangeLedger.ps1 -RepositoryRoot 'I:\GitHub\Unity_GAS\gameplay-ability-system-for-unity' *> 'artifacts/diagnostics/NTSD-H10-FORMAL-AUDIO-SESSION-OWNERSHIP-20261010/change-ledger-final.log'
```
最终校验事实另附在报告末尾，原日志为准。

证据复用：先前四 WAV 的 bounded PCM 数学、chunk 边界、错误资格、原声道/增益合同不受此次所有权改动影响，helper 指纹和本次真实输出位相等支持复用，不重复成本组或全量 978 个解码。对被影响的 owned source、stale await、stage1/5、controller、voice 解绑、Play deferred Destroy 全部取得新证据。现有完整 tick、输入/RNG/checksum、H06、H08、0GC 与 Android 证据均不被本轮小音频组替代。

未执行：整局 producer 闭包加载、完整 battle Play/多人/长窗、H07 120warm+180/1800、Driver 0GC、异步 decoder 故障注入、设备/GPU/IL2CPP/Android、音频可听波形验收。没有额外创建实验或重新清理旧数据。

## 审计与下一步边界

[保护审计](protection-audit.json) 已核 15,873 个范围外文件无哈希漂移、14 份真实 dirty 原备份有效；误匹配的 H10 .md.meta 曾在初期加 notice，已在任何 refresh 前准确恢复原字节，最终再次核对一致，未保留 GUID 改动。该错误和早期只读 git 子进程停止均在 Operation 记录；没有把历史错误删去。后续 [final-audit.json](final-audit.json) 记录全部中间备份、HEAD/staged、精确输出哈希和 scoped diff。

本次有限 Task 到此收口，交讨论会话审阅本报告；完整 H10 尚需对 producer 实际音频依赖闭包和整局预算作决定，不能把 visual closure 当作完整 audio closure，也不自动裁剪 cue 或延迟到首次播放。H08 默认 ON、M03 候选 OFF、H07/producer 暂缓、95 OFF、H06 不续保持，不启动其他优化点、Goal 或队列。

## Final audit result (2026-10-11)

Validate-ChangeLedger.ps1: exit0 / PASSED,1420 records,26 governed code files;4263 historical warnings,0 warnings naming this Change. Full log: artifacts/diagnostics/NTSD-H10-FORMAL-AUDIO-SESSION-OWNERSHIP-20261010/change-ledger-final.log. Final-audit-v2 exit0: protected15873 unchanged; dirty preimages14 valid; intermediate backups48 valid at audit timestamp; corrected .md.meta exact; HEAD and staged diff unchanged; scoped git diff --check exit0. Runtime3 sources/runtime assembly unchanged since unique resource group; only Play preflight fixture+Editor assembly changed. No subsequent script edits. Final documentation-only preimages and hashes recorded in closeout-manifest.json after this appendix.
