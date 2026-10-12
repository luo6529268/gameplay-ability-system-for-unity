# H10 PC 暂定目标落盘与一次有界 Play 验收

Task：`NTSD-H10-PC-BUDGET-RUNTIME-ACCEPTANCE-20261011`。Change：`NTSD-H10-PC-BUDGET-RUNTIME-ACCEPTANCE-001`。

**已完成 PC_TARGETS_ADOPTED 和当前输入对应下的320MiB AudioClip对象预算复用；运行验收未通过，状态为 RUNTIME_CALIBRATION_FAILED。** 唯一Play组在分配正控失败处停止，没有正式热样本，没有三轮完成，也没有通过 `RUNTIME_AUDIO_GATES_VERIFIED`。这是本轮测量夹具资格问题，不是已经证明的生产音频分配缺陷。H10继续保持OPEN，下一是同H10精确夹具修正的独立审阅；本报告不自动暂停H10、不转其他优化点。

## 批准与预算口径

人类在讨论会话 `01a11fe2-bfaa-72f3-a9ed-c8260f63b193` 对具体提案回复“可以，我觉得没什么问题”，已记录为 [PC暂定工程目标决定](../../../docs/ai/DECISIONS/NTSD-PC-TEMP-ENGINEERING-TARGETS-20261011.md)。此前UNDECIDED记录保留为历史，不再作为当前预算批准阻塞。

| 项目 | 当前批准 |
|---|---|
| 基础候选 | Windows11 x64、8GB RAM、SSD、CPU初筛4核8线程、1280×720/60FPS；核显具体型号待验证 |
| 推荐候选 | Windows11 x64、16GB RAM、SSD、CPU初筛6核12线程、1920×1080/60FPS；GPU型号待验证 |
| prepared AudioClip常驻 | owned/borrowed原clip和输出按真实对象ID去重，≤320MiB=335544320B，13个nonmono不重复计 |
| 整个游戏进程 | Private Bytes/private commit稳态≤2GiB、过渡采样峰≤3GiB；尚未实测可行性 |
| 三轮退出 | 每局owned归零且不逐轮积累；generic按原合同借用/复用 |

这些是暂定工程目标、尚未认证的配置，不是发布的最低配置。核数不是性能证明；当前开发机是i5-13600KF，完整OS/RAM/GPU/电源信息见 [hardware-environment.json](hardware-environment.json)，CPU持续功耗/性能未测，不能认证8GB目标。60FPS是显示目标，33ms逻辑间隔保持。Private与working set不能相加；decoder/staging/voice/temp另列，320MiB不是完整音频子系统峰值。没有增加生产预算拒绝阈值、裁剪、lazy或格式/音质变化。

## 完整对象账本复用

实际离线核了1050个音频路径、330个DAT及catalog/config/controller，共1682项，全部SHA匹配。第一次审计错误地把DAT解析到vfs，330项返回路径不存在；随后依据 `BattleContentSource` 的真实 `decoded_dat` 根修正映射，原错误原件保留，没有把它解释成文件丢失。没有解码或重跑全目录。

最终对应见 [catalog-input-correspondence-resolved.json](catalog-input-correspondence-resolved.json)。旧完整两轮账本仍为978formal +72generic，去重 **228918567B（218.313758MiB）≤335544320B**，余量106625753B。该对象预算门按未变化输入复用通过，不外推全producer、整进程或目标机。

R1–R4、条件native binder、既有PCM位级和逐局owned证据没有重开；原13/14失败、旧峰值、Menu工具UNKNOWN及先前事件均保留。没有读取新的Q06 producer方法体。

## 实际编译、资格与唯一 Play

原Editor Unity2022.3.62f3，PID39108。每项运行前均通过本机127.0.0.1:6402现有bridge确认idle、非Play/非编译更新/无其他测试，Menu clean8。没有第二Editor、重启、execute_code重试或工具修复。

| 门 | 实际结果 | 原件 |
|---|---|---|
| 编译 | 第一次两个类型错误、第二次一个错误，限新增夹具类型引用修正；第三次编译接受、domain reload完成，无C# error | compile-console-01/02/03.json；compile-state-03.json |
| 窄资格 | 1/1 PASS；原cache空、loader无pending/queued、无活动音频、Menu/setup和真实资源/diagnostic seam有效；未解码 | qualification.json；qualification-tests.xml |
| 唯一Play组 | 0/1 FAIL；测试实际19.719154s，分配正控失败后停止 | unique-play-tests.xml；play-result.json |
| 完整三轮、热0GC、实际isPlaying/输出、取消/自然Destroy边界 | 未执行到，不通过 | 无相应PASS原件 |
| Player/真实千人实体/进程2/3GiB/8GB/Android | 未执行 | Player仅只读前置方案 |

采用EditMode测试显式EnterPlayMode/ExitPlayMode，不使用bridge的PlayMode模式；已核后者会调整domainreload选项。调用工具前全场景clean，因此没有触发其自动保存dirty Scene分支。当前任务没有改Settings或保存Scene资产。

bridge job因跨domainreload在查询时仍返回stale `running`，但Editor已idle、tests.is_running=false，具名NUnit XML已记录本次0/1失败，且本任务play-result/scene-restored原件完整。本报告以实际具名XML和原件为准，没有把job提交成功当测试通过，也没有修工具或重复轮询刷状态。

### 首差

`GC.GetAllocatedBytesForCurrentThread()` 空scope差值0；紧接的 `new byte[97]` 正控经 `GC.KeepAlive` 后差值仍0，`>=97`断言失败。失败位于正式热测循环之前，因此有效热样本数为0。不能由此声明0GC，也不能声称生产音频分配失败。计数API/优化等具体底层原因未证明。

可复用的 `BattleScopedGcAllocationRecorder` 已只读定位：当前线程GC.Alloc recorder、1MiB触及正控、前后校准、容量/溢出fail-closed。尚未替换或运行。准确修正建议见 [MEASUREMENT-REPAIR-REVIEW.md](MEASUREMENT-REPAIR-REVIEW.md)。另发现尚未执行的borrowed负控使用了不存在的generic `data/001.wav`；本次实际可用且新增的generic为 `data\\016.wav`。这是夹具选材问题，不创建缺失资源或改root来掩盖。

### 已观察的准备和失败清理（不冒充三轮验收）

- 在await前经 `SetSoundPresentationSinkForDiagnostics` 捕获正确player owner。诊断World profile Authority400，runtime容量400，实际publication容量51200；没有改变生产容量。一个诊断emitter已注册，但热tick尚未开始。
- 第一轮公开 `PrepareBattleCuesAsync(minimal manager)` 完成：固定4WAV+一个nonmono代表及公开入口自动添加builtins；22个owned clip身份、去重8431640B，nonmono引用只计一次。实际soundRoot仍为NTSD/Sound；controller-null仅沿已核真实Battle空AudioList的fallback等价边界。
- 失败清理走公开 `ShutdownBattleRuntime()`：report stage10、World解绑、remaining objects/slots/pool borrowers全部0；空诊断地图完成调用返回stage11。此处没有真实Battle地图，不能作为生产场景地图卸载证书。
- 返回时22个owned native对象仍活，pending0、tail已drained；下一帧确认全部消失及voice无旧clip绑定。保留了stage返回与延迟Destroy的区别。
- 最终只移除本次预声明且精确同对象的 `NTSD.AudioFile::data\\016.wav`，native ID `-1255906`；全部新host自然Destroy，cache/loader/fixture内singleton绑定恢复，fixture前后AudioClip身份集合断言相等。没有ClearCache/UnloadUnusedAssets/GC.Collect或全局Destroy。
- [fixture-cleanup.json](fixture-cleanup.json) 的cleanupFailure为空；[scene-restored.json](scene-restored.json) 及after-play-scenes.json确认原Menu路径、active/setup、clean8恢复。跨Play发生场景实例重建；不据此宣称先前历史事故已完整恢复。

Unity内Process.PrivateMemorySize64本轮原值为0，视为不可用，不作为内存预算通过数据。没有waves/听感、热播放、取消迟到或跨三轮积累证据。

## 实际命令与写域

精确参数、时间、原始响应见 [commands.jsonl](commands.jsonl)。实际入口包括：

```text
get_editor_state {}
manage_scene {"action":"get_loaded_scenes"}
refresh_unity {"mode":"if_dirty","scope":"scripts","compile":"request","wait_for_ready":false}
read_console {"action":"get","types":["error"],"count":10,"format":"detailed"}
run_tests {"mode":"EditMode","testNames":["NTSD.Test.Editor.H10ResidentClosureDiagnostics.PcBudgetRuntimeQualification"]}
run_tests {"mode":"EditMode","testNames":["NTSD.Test.Editor.H10ResidentClosureDiagnostics.PcBudgetRuntimeThreeSessionsPlay"]}
get_test_job {"job_id":"具名job-id，见commands.jsonl","include_details":true,"include_failed_tests":true}
git status --short --branch
git rev-parse HEAD
git diff --cached --binary
git diff --binary
powershell -NoProfile -ExecutionPolicy Bypass -File Tools/Validate-ChangeLedger.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File Tools/Validate-ChangeLedger.ps1 -RepositoryRoot I:\GitHub\Unity_GAS\gameplay-ability-system-for-unity
pwsh -NoProfile -File Tools/Validate-ChangeLedger.ps1 -RepositoryRoot I:\GitHub\Unity_GAS\gameplay-ability-system-for-unity
```

另执行内联Python SHA/路径映射/备份/XML核对及硬件只读CIM。没有调用旧R8 builder或Player。

唯一脚本增量为 `H10ResidentClosureDiagnostics.cs` 的PcBudget*测试/局部helper；旧方法体未变，生产脚本未改。脚本前Task/Change/Operation已登记，十个原写域文件真实字节备份以及每轮必要中间版本、共享Temp XML前件已保存。保护审计检查4094条具名指纹，无漂移；Play前后8项源码/程序集指纹一致，10个初始备份有效，10个tracked改动路径全部位于声明范围。HEAD8d55c6b…/ahead5与暂存区保持。status-before里的14条untracked均保留，其中12条是任务前内容、另2条是记录status之前已创建的本任务artifact/Operation目录；不把这14条误称为14个旧文件。证据见 [protection-audit.json](protection-audit.json)、[final-audit.json](final-audit.json)、[scoped-diff.patch](scoped-diff.patch)。源码保持实际失败版本，不事后用未执行修正版覆盖测量指纹。

ChangeLedger最终 **PASS，exit 0**：现有PowerShell7执行原validator，1422条Record、1个受管代码改动被当前Change覆盖。本Change没有warning；4531条其他Record声明路径不在当前diff的warning保留，未修改或清理。实际日志为 [change-ledger-pwsh7.log](change-ledger-pwsh7.log)。首次WindowsPowerShell调用在默认RepositoryRoot求值处失败；第二次显式传root后，Git的LF/CRLF提示被该shell提升为NativeCommandError；两次exit1原件 [change-ledger-final.log](change-ledger-final.log) 和 [change-ledger-explicit-root.log](change-ledger-explicit-root.log) 均保留。没有改validator、Git配置、源码换行或禁用任何校验。`git diff --check` exit0，仅出现上述换行提示。

## 同 H10 剩余门与后续方案

1. 在相同单诊断文件写域修正校准与真实generic负控，窄资格通过后，再经独立审阅登记新的有界Play版本；本轮不重跑。
2. 完成实际播放、有效热范围0GC、三轮公开关闭/重入、取消/迟到和自然销毁边界。普通/1000条请求与真实1000实体必须分开报告。
3. 按 [PLAYER-PRECONDITIONS-REVIEW.md](PLAYER-PRECONDITIONS-REVIEW.md) 审阅普通/真实千人Player同进程三轮与外部Private采样。当前Standalone配置为IL2CPP；零Settings改动不能声称构建Mono，旧R8还会改Burst/SaveAssets，故没有调用。新Mono路线的精确最小暂态backend改动仍需Settings域的具体确认，不能泛泛以“继续H10”授权。
4. 完整producer实际请求覆盖、真实1000实体整局/进程预算、目标PC硬件、Android/GPU/AB/IL2CPP后期证据保持各自门。当前限定成果不等于H10父项、Windows H07阶段或Android最终认证。

H08 ON、M03 OFF、H07输入/producer暂缓、95 OFF、H06不续保持。没有恢复Goal、34项队列或自动实施其他点。
