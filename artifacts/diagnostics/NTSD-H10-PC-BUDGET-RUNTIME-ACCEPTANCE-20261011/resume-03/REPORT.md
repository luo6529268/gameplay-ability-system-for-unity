# H10 resume-03：实际启动声源身份与三轮音频运行门

结论：**RUNTIME_AUDIO_GATES_VERIFIED（本次隔离 Editor Play 的限定音频运行门）**。Task NTSD-H10-PC-BUDGET-RUNTIME-ACCEPTANCE-20261011 / Change NTSD-H10-PC-BUDGET-RUNTIME-ACCEPTANCE-001。H10父项仍OPEN，不是1000战斗实体、完整Battle地图、Windows Player、目标PC进程预算、H07或Android认证。

本版只改测试冷观察；生产规则、旧共享FindVoiceWithClip、非PcBudget前195422字节、三个recorder/计量资格/sink/emitter和ExerciseHot原字节保持。先登记[PLAN](PLAN.md)与[Operation](../../../../docs/ai/FILE-OPERATIONS/OP-NTSD-H10-PC-BUDGET-RUNTIME-ACCEPTANCE-RESUME03-20261011/RECORD.md)，保存11份真实dirty前件；修改后首次编译通过，两项新资格通过，仅执行一次新版Play组，没有追加重跑。

## 本次观察结论

每个formal/generic单次调用前后复制64槽位声源映射、启动序号和host下含inactive的所有声源状态。数组原引用仅用于身份核对；before值独立复制。实际源由恰一启动序号推进确定，所有冷证据在播放断言前CreateNew保存。六次观察均满足播放总计数+1、load/build/failure/rejection无增量、数组/槽位/source身份保持、实际clip同引用、enabled/active/isPlaying为true。

| generic轮次 | 实际启动槽位 / voice ID | 旧selector voice ID / 在播 | 本次实际在播 |
|---|---|---|---|
| 1 | 3 / -1263260 | -1263260 / true | true |
| 2 | 7 / -1263292 | -1263260 / false | true |
| 3 | 11 / -1263324 | -1263260 / false | true |

第2/3轮直接复现旧helper选中停播旧声源，而本次另一个同clip声源实际启动在播。可以关闭**本次运行的观察身份歧义**；resume-02没有相同完整快照，旧0/1FAIL和其真实原因未被追溯改写，也不据此宣布生产曾经漏播。原件：[第2轮generic全快照](play-observation-generic-cycle-2.json)、[第3轮](play-observation-generic-cycle-3.json)、[六次观察审计](playback-observation-audit.json)。

三轮监听输出每轮4096样本，outputMax均1.0，NONZERO_OUTPUT_OBSERVED；人工听感hearingVerified=false。这是Listener混合输出存在的证据，非逐cue隔离录音或音质认证。各轮waveform-cycle-N.json在generic断言前保存。

## 实际验证

| 层级 / 命令或API | 结果 | 证据 |
|---|---|---|
| 原Editor `refresh_unity(mode=if_dirty,scope=scripts,compile=request)` | 首次编译及domain reload完成；无C# error | compile-request.json、editor-compile-ready.json、compile-console-ready.json |
| `run_tests(EditMode, PcBudgetAllocationMeasurementQualification, PcBudgetRuntimeQualification)` | 2/2 PASS；0.146295s / 0.479762s | [资格XML](qualification-tests.xml)、qualification-job-final.json |
| `run_tests(EditMode, PcBudgetRuntimeThreeSessionsPlay)`，内含显式Enter/ExitPlayMode | 唯一组1/1 PASS，28.744657s | [实际XML](unique-play-tests.xml)、[Play结果](play-result.json) |
| 当前输入离线SHA复核 | 1682项与旧账本输入一致，无重新解码 | catalog-input-correspondence-resolved.json |
| 生命周期/精确清理/当前Menu恢复 | 三轮＋pending取消＋自然Destroy全部通过 | [生命周期审计](lifecycle-audit.json)、fixture-cleanup.json、scene-restored.json |

资格job `7fc9ccdf217d4a1a88a1f5f2992177a1`，Play job `8643d09589eb42699199c1dbcfc2ed5b`。Play域重载后bridge job仍显示running的旧状态；最终具名XML、play-result与Editor idle/nontest为已完成依据，未凭旧job状态重跑。Console保留既有MCP Client handler exited异常，未当作C#编译错误。未重启或启动第二Editor，未查询进程CommandLine/环境/凭据。

## 热范围证据

三轮每轮3warm＋7measured tick，测量三个互斥范围：World.QueueSound循环、prepared PresentSounds、publication＋dispatch＋prepared player。共**63个有效scope均0 GC.Alloc事件**；逐scope同线程、Internal/GC.Alloc、capacity256、Unit TimeNanoseconds，前后1MiB正控有效、empty0，无wrapped/saturated。外层pipeline采样时关闭sink内部采样，不能把父子计时相加。

普通请求、1000同cue、多cue、同帧重放及连续tick沿原ExerciseHot实际发布/dispatch执行；全部60个负载记录（含预热）实际play增量等于预期1或5，load/build/failure/rejection增量0。身份未热重建，64voice容量保持。完整原件hot-cycle-1/2/3.json，机器复核[hot-scope-audit.json](hot-scope-audit.json)。这不是63字节/GC回收停顿，也不是wholeDriver/engine或1000实体证据；没有CPU/P95收益比较，不以测试总耗时作性能结论。此前42个scope保留在resume-02，未填入本轮第三轮。

## 生命周期、所有权与现场恢复

三轮generation2→6→10，每轮22个distinct owned AudioClip、8,431,640B；generic精确key `NTSD.AudioFile::data\016.wav` 的clip ID -1263830三轮同引用。formal原件有owned/source/output分别列出，nonmono仅计一次。

三次真实Driver公开ShutdownBattleRuntime返回stage10，World已解绑、worldObjects/runtimeSlots/poolBorrowers均0；返回时22个native owned对象仍alive，pending0、tailDrained=true。随后真实空地图完成stage11并等待下一帧，owned对象归零后才新一轮。不能把返回时点标成stage5，也不把deferredDestroy写成返回瞬间归零。本次实际启动generic在每次公开关闭前后clip/voice身份不变且仍在播；精确timeSamples/dsp和enabled已先于关闭断言保存于public-close-observation-cycle-N.json。

pending取消：关闭前真实pending1；stage10返回仍pending1、tailDrained=false、1个owned native仍alive，随后公开stage11/await drain/下一帧释放；late完成stale计数0→1、stale ID -1264114已不存活。自然Unity Destroy：捕获自有host -1264116，pending1/owned -1264636，实际Destroy/OnDestroy后owned消失、pending任务完成、tailDrained、stale ID -1264638已释放。没有反射手调Unity生命周期或强写Stopped。

最终只在自有host关闭/销毁及精确cache引用/借用检查通过后，RemoveCache本轮generic016 key并去重释放clip；cache、loader、单例绑定恢复，所有本次host/owned归零，Play native baseline []→[]。当前Menu恢复clean8，稳定序列化组件快照相同，GameConfig/MM settings序列化相同；不使用跨domain instance ID证明场景身份，不声称恢复更早Menu历史。没有SaveAssets/保存场景、全局cache清理、GC.Collect或UnloadUnusedAssets。

## 版本、复用和未执行

受测源码SHA256 `d36cd18e6bffc68090891dea9874ea8c5d2ff56ae13d826dcef792e2edfd8b4e`；Editor DLL `1ac32f7bad57dcc6a28a8a6dc552f28b12c6bdb42fa9d07ab5fcd236f6d5f9c3`；runtime DLL `2a8884669c54bebdb5747e7808778912d5aa5a0bf41cbedd271141027003ab69`。源与程序集完整身份见source-assembly-before-play.json；受测源码冻结于source-version-01.cs，resume-only-source.diff和static-review.json限定本轮增量。

旧全目录228,918,567B（218.313758MiB）在1682项输入指纹一致下复用，低于已批准320MiB AudioClip对象预算；本轮8,431,640B小夹具不能替代全目录账本。Private进程稳态2GiB/过渡峰3GiB仍未测；旧Editor PrivateMemorySize64=0不可用。8GB目标PC、真实1000战斗实体/完整producer闭包、完整Battle地图卸载、Player/IL2CPP/Android未执行，不降低33ms/0持续backlog/dropped0最终门。H08 ON/M03 OFF、H07及producer暂缓、95OFF/H06不续、旧失败与46.8822ms尖峰和Menu历史UNKNOWN保持。

本轮开头FindEffect/重启表述是上下文串线，实际只读旧准备报告与治理完成记录，无相关方法体读取/源码修改/测试/进程启停，已纠正，见scope-correction.json。前次敏感命令行采集事件保持既有脱敏记录；本次未读取或复制原敏感输出，也不声称历史凭据已撤销。

下一步仅同H10独立审阅本轮原件与尚缺的Player/整进程/目标PC证据，详[NEXT-REVIEW](NEXT-REVIEW.md)。无新增执行范围或自动Goal/队列，H10父项保持OPEN。保护审计与ChangeLedger实际收尾结果随后追加。

## 最终收尾校验

实际命令：`pwsh -NoProfile -File Tools/Validate-ChangeLedger.ps1 -RepositoryRoot I:\GitHub\Unity_GAS\gameplay-ability-system-for-unity`。exit0，Change ledger validation PASSED，1422 Records、当前diff内1个受管脚本，本Change0warning；4531条其它历史warning保持，不为清理它们修改范围。原件[change-ledger.log](change-ledger.log)、[validator-result.json](validator-result.json)。`git diff --check -- <本轮11具名路径>` exit0，仅LF/CRLF提示，见diff-check-result.json。

4094项保护指纹0漂移、293个旧root/resume01/02证据0漂移、9受测源码/程序集/recorder指纹0漂移，11份真实dirty备份有效。旧非PcBudget前195422字节、三个recorder/资格/sink/emitter、ExerciseHot完全保持。无测试后源码修改，无新增生产diff。HEAD仍8d55c6b68912560c7af755c424b41d3464a1921a、暂存空、原10tracked dirty路径相同；旧untracked入口均在，新状态仅新增resume03 Operation目录，其它新增原件在已有本Task artifact根内。

首次末端审计因before文本用LF而after文本用Windows CRLF，按原始bytes比较误报HEAD/路径不同并退出1；其它哈希门已全部通过。原protection-audit.json保留，随后仅解析同一前后文本行确认内容完全相同，未重新测试/改源码/改Git，见[保护审计更正](protection-audit-correction.json)。此处是审计格式错误，不是运行失败或仓库变化。

11份初始前件及10份收尾治理当前前件均保留；最终逐文件manifest/链接有效性见final-audit.json与Operation after-manifest.json。新原件统一artifact-manifest.json记录（不含manifest自身）。本次限定交付完成，H10父项OPEN；不自动执行下一组、Player、H07或Android。


## 恢复工具范围补记（更正前述绝对表述）

本turn为核实压缩上下文与当前派发，实际调用read_thread(threadId=01a1201b-7930-7c93-8c84-5bc7baada43f, turnLimit=4, includeOutputs=true, maxOutputCharsPerItem=18000)，返回了过宽历史输出并被截断。不能保证此次历史回传没有包含先前敏感段；未进一步读取/搜索该段，未提取、使用或定向再展示凭据。本次未重新查询进程CommandLine/环境/凭据字段，也未复制原敏感payload到本补记。前文“本次未读取或复制原敏感输出”绝对表述由本段替代；不声称旧凭据已撤销/历史副本全清。准确只读动作和UNKNOWN边界见history-retrieval-scope-correction.json。此次历史恢复范围过宽属于执行记录问题，不改写已测运行结果；没有新增测试或代码变动。
