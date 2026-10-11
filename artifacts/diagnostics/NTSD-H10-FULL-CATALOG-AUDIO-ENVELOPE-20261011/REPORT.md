# H10 全目录音频内存范围实测报告

日期：2026-10-11（Asia/Shanghai）  
Task：NTSD-H10-FULL-CATALOG-AUDIO-ENVELOPE-20261011  
Change：NTSD-H10-FULL-CATALOG-AUDIO-ENVELOPE-001  
Operation：docs/ai/FILE-OPERATIONS/NTSD-H10-FULL-CATALOG-AUDIO-ENVELOPE-20261011  
结论：**FULL_CATALOG_AUDIO_ENVELOPE_MEASURED**。有限诊断完成；H10 父项 OPEN，音频数字预算 UNDECIDED。

本轮只向 H10ResidentClosureDiagnostics.cs 追加 Editor 诊断；生产代码、实际声音规则和资源不变。完整真实目录的两次公共准备/关闭已实际执行；每次去重 AudioClip 对象内存 228,918,567 B（218.313758 MiB），两轮语义元数据一致，正式输出逐局释放、72 个 generic 缓存对象跨轮复用，最终只精确释放本夹具新增的缓存与对象。没有新增生产收益声明，也没有按局裁剪准入、H07 或 Android 认证结论。

## 授权、范围与环境

按同一 H10 的人类继续授权及本轮准确委派实施；不是恢复旧 Goal 或 34 项队列。H08 默认 ON、M03 候选 OFF、H07/producer 暂缓、95 OFF、H06 不续的当前状态保持。

- 唯一可写项目脚本：Assets/NTSD/Scripts/Test/Editor/H10ResidentClosureDiagnostics.cs；旧方法体字节内容保持。本轮新增 EnvelopeFixture 和五个 CatalogAudioEnvelope 测试/辅助方法。
- 原项目唯一 Editor：Unity 2022.3.62f3，PID 39108，WindowsEditor；通过现有 127.0.0.1:6402 bridge 操作。没有第二实例、重启、Console 清理、全局缓存清理、GC.Collect 或 UnloadUnusedAssets。
- 最终 Editor idle、非 Play、非编译；NTSD_Menu.unity clean、8 roots。最终状态见 final-editor.json、final-scenes.json。
- 全部新状态归 Editor 夹具所有，容量来自本次完整 manifest；单运行锁；每轮准备途中至多 16 个观察点，本次各 9 个。沿用 player 阶段 1/5，等待所有已跟踪异步任务排空后才销毁夹具 host；无新增生产 manager/cache/关闭阶段。
- 原有缓存、音频对象、其他 soundplayer 的 preparedCues/BattlePlaybackClips/owned lists、全部配置借用、voices、跨 key 数组和场景对象纳入保护。不能证明新增且未被借用时保留对象并失败。
- 未读活跃 Q06 方法体；未写 Scene/Prefab/资源/ProjectSettings/Input Actions/Server/Gen/Plugins；无 Git reset/checkout/clean/stash/push。

## 实际配置、清单与指纹

先在真实 C# 路径生成完整配置：LoganVisualContentCandidate.Capture(source, ProjectBattleModeConfig.LoadDefault().Capture())、BuildCharacterFrameConfigsFromCatalog；fixture manager 绑定实际 publishedLoganCandidate。没有以 visual subset 代替全量 catalog，没有空造 mode/controller 或改成不存在的 soundRoot。

| 清单项 | 本次实际值 |
|---|---:|
| 正式 definitions | 330 |
| cue IDs | 1000 |
| formal 唯一身份/最终对象 | 978 |
| generic 存在文件的 cue 路由 | 77 |
| generic 唯一文件/对象 | 72 |
| builtin IDs | 19 |
| 真实 Mode KO | data/m_ok.wav、data/m_join.wav |
| soundRoot | NTSD/Sound |
| 合并唯一音频文件 | 1050，均 WAV PCM format 1 |
| 初始 generic cache | 空 |
| 初始 Editor AudioClip 对象 | 4，全部保留 |

inventory.json 保存实际身份、路由、文件 SHA-256、配置/内容身份；headers.json 保存逐文件格式、samples/channels/frequency 和理论 PCM。完整组开始前再次比较实际清单/路由/内容哈希，并绑定预检 inventory SHA。不是仅凭路径或数量认为内容一致。

当前 Menu 没有 AudioController。controller-evidence.json 用结构化 YAML 读取实际 Battle Scene：其真实 AudioController.AudioList 为空，AudioSource clip 为 0；结合实际 FindAudioItem 的 null/空列表都走 fallback，支持本轮配置下的 resolver 等价。证据绑定 Scene、脚本、meta、生产 resolver 哈希；**不证明实际 Battle Controller 生命周期或身份等价**，没有打开/改写 Battle Scene。

理论 float PCM：formal 源 116,169,868 B；formal 最终播放 226,264,216 B；generic 5,999,028 B；最终去重理论合计 232,263,244 B。理论 payload 与 Unity 对象内存是不同口径，不能互相代替。

完整组前后 full-fingerprints-before.json / full-fingerprints-after.json 的八项源码/程序集哈希相同。测量之后只收紧诊断夹具的未知 manager 保护并增加无音频负控；因此 final-audit.json 的诊断脚本及 Editor DLL 哈希与完整组不同。五个生产脚本及 runtime DLL 不变，音频收集/测量算法未改；**不声称完整组运行于最终诊断哈希，也不重复完整组来更换证据**。

## 分阶段内存与生命周期

对象内存来自 Profiler.GetRuntimeMemorySizeLong，按 native instance ID 去重。13 个非 mono 输入属于 formalOutputOrNonmono 的 978 个对象之内，不能再次相加。

| 阶段 | formal 对象 | generic 对象 | 去重对象内存 B | Editor 全部 AudioClip 数 |
|---|---:|---:|---:|---:|
| 第一轮开始前 | 0 | 0 | 0 | 4 |
| 第一轮准备完成 | 978 | 72 | 228,918,567 | 1054 |
| 第一轮阶段 5 返回 / 异步尾部排空 | 0 | 72 | 2,983,447 | 76 |
| 第二轮开始前 | 0 | 同一批 72 | 2,983,447 | 76 |
| 第二轮准备完成 | 978 | 同一批 72 | 228,918,567 | 1054 |
| 第二轮阶段 5 返回 / 异步尾部排空 | 0 | 72 | 2,983,447 | 76 |
| 最终夹具清理 | 0 | 0 个本任务新增项 | 0 | 原有 4 |

| 准备完成时分类 | 对象数 | 对象内存 B | MiB | 是否可与合计再相加 |
|---|---:|---:|---:|---|
| formal 最终输出（含非 mono 输入） | 978 | 225,935,120 | 215.468521 | 合计组成部分 |
| 其中非 mono 输入 | 13 | 3,075,304 | 2.932838 | 不可，已含在上一行 |
| generic 缓存 | 72 | 2,983,447 | 2.845237 | 合计组成部分 |
| 去重合计 | 1050 | 228,918,567 | 218.313758 | 不可重复累加 |

每轮最终 formal 计数 978，第二轮累计 1956；失败加载、缺失、未准备拒绝均为 0。逐 clip samples/channels/frequency/name 与文件头及正式转换规则核对，所有 cue alias 指向预期对象，两轮语义 JSON 一致。第一轮 formal native ID 在第二轮开始前已全部消失；两轮 generic native ID 集合完全一致。

29 个阶段快照中有两轮各 9 个准备途中快照，观察到的 pending formal 最大为 1。采样最大对象内存为 228,918,567 B。owned mono 源在采样点为 0；同步转换内部不在这些观察点之内，不能推出瞬时源占用或峰值为 0。

最终先关闭/排空，销毁夹具 host 并清掉其持有的已失效字典引用，再在 key 新增、manifest 范围、值引用不变、无其他借用的保护下精确删除 72 个新 generic key 并销毁对应 72 个 native 对象。缓存回到原空状态，原有 4 个 native clips 保留。full-cleanup.json 保存逐 key/ID 账本。

**边界：**这是音频准备/关闭的 Editor 对象内存账本，不是 RSS、整个进程内存、真实游戏稳态/切局绝对峰值、GC 停顿或音频播放设备验证。decoder/native staging、同步临时数组的实际峰值未知；本环境 EditMode 使用即时销毁，不能外推 Player 延迟销毁峰值。配置借用与 voice bindings 在此夹具为 0，没有实际声音播放验收。

## 本轮验证、失败与修复

| 验证 | 实际结果 | 证据 |
|---|---|---|
| 首次脚本编译 | 2 errors，字段名 type/frame 不存在；改为实际 catalogEntry.Type/frameId 后通过 | compile-console-01.json；后续编译记录 |
| 实际目录清单 | 1/1 PASS，36.127281 s | inventory-tests-actual.xml |
| 首次两资源资格 | 0/1，FAIL，33.657413 s | qualification-failed-tests.xml |
| 精确事件恢复检查 | 1/1 PASS，0.631737 s；记录对象已不在，无 Destroy/RemoveCache 执行 | recovery-tests.xml；qualification-delta-recovery.json |
| 修正后两资源资格 | 1/1 PASS，24.140329 s | qualification-v2-tests.xml |
| 唯一完整两轮组 | 1/1 PASS，105.624190 s | full-tests.xml；full-result.json |
| 测量后未知 manager 所有权负控 | 1/1 PASS，0.681435 s；不加载音频 | guard-tests.xml；unknown-manager-guard.json |
| 最终范围审计 | 71,283 个保护文件无漂移、10 初始 dirty 备份/29 当时中间备份有效、旧诊断正文不变、HEAD/暂存不变、scoped diff-check 0 | final-audit.json |
| ChangeLedger | PASS，exit 0；1421 records / 26 governed diff files；4263 条历史全库 warnings，本 Change 无 warning | change-ledger-final.log |

首次资格错误属于本轮夹具，不能略去：MMSoundManager.Current 原来为 null，getter 解析到已有 Menu manager；旧夹具错误地把它当作本次新建并销毁了其原 root（ID 216430）。保护断言失败并停止后续 cache 清理；正式音频 owner 与异步尾部已经关闭/排空。测试框架随后自动恢复原 Menu clean8，Menu/Battle Scene 与 meta 文件哈希不变；代理没有保存或重新加载 Scene，也没有重启 Editor。

失败时唯一新增 generic key 为 NTSD.AudioFile::data\015.wav，native ID -1243288，name 015、7153 samples、mono、44100 Hz。事件操作记录先保存现场与精确身份，然后做必要的同脚本编译/修复。两个只读 execute_code 尝试分别因 CodeDom 命令过长和 Roslyn 不可用而未执行。恢复检查时该精确 ID 和 cache 映射均已不在，因此没有再销毁对象或删除 key；消失原因没有推断为正常编译释放。原有 4 个音频对象仍在。事件原件及失败保留于 Operation/INCIDENT-QUALIFICATION.md。

修正后的资格识别已有 component 和 GameObject；只恢复自己借用时改变的被动 singleton 指针，不销毁原 manager，实际两资源加载、formal 释放、generic 精确释放、scene/cache/queue/singleton 恢复全部通过，才允许唯一完整组启动。

完整组期间源码保持冻结。晚到静态复核还指出“未在基线”不足以证明对象归夹具新建；完整组结束后移除了未触发的创建/销毁分支，未知 manager 一律保留并报错，记录的已有 borrower 也不得中途改变。独立无音频负控验证未知对象保留、第二个已知对象切换拒绝、原对象接受及 singleton 不变。完整组实际走的是原有已知 manager 路径，原测量保持有效但使用其自身哈希。

inventory-tests.xml 是异步 inventory 完成前误复制的前一个任务 Play XML，**INVALID_FOR_THIS_TASK**，保留但不作为本轮通过证据；正确原件是 inventory-tests-actual.xml。本轮没有把早期失败或上一任务旧兼容组 13/14 的既有 mono 引用断言 FAIL 改写成全通过。

## 实际命令与执行身份

通过现有 Editor bridge 发送；完整参数、时间、输出文件见 commands.jsonl。不是另启 batchmode Unity。

```json
{"type":"refresh_unity","params":{"mode":"if_dirty","scope":"scripts","compile":"request","wait_for_ready":false}}
{"type":"run_tests","params":{"mode":"EditMode","testNames":["NTSD.Test.Editor.H10ResidentClosureDiagnostics.CatalogAudioEnvelopeInventory"]}}
{"type":"run_tests","params":{"mode":"EditMode","testNames":["NTSD.Test.Editor.H10ResidentClosureDiagnostics.CatalogAudioEnvelopeFixtureQualification"]}}
{"type":"run_tests","params":{"mode":"EditMode","testNames":["NTSD.Test.Editor.H10ResidentClosureDiagnostics.CatalogAudioEnvelopeRecoverExactQualificationDelta"]}}
{"type":"run_tests","params":{"mode":"EditMode","testNames":["NTSD.Test.Editor.H10ResidentClosureDiagnostics.CatalogAudioEnvelopeTwoSessionMeasurement"]}}
{"type":"run_tests","params":{"mode":"EditMode","testNames":["NTSD.Test.Editor.H10ResidentClosureDiagnostics.CatalogAudioEnvelopeUnknownManagerGuardPreservesObjects"]}}
```

资格实际执行两次，分别为旧实现失败和已修复夹具通过；不是同版重复寻找 PASS。完整组 job e04b767d1bb74ab89095dc2e78b8c9d6，唯一 full-group.started 创建于 2026-10-10T16:58:05.1850565Z（本地 2026-10-11 00:58:05），没有第二组。测量后保护负控 job 61f5852cc2174193b53086910c14bdb5。

两轮 Prepare elapsedWithObserver 为 35.8597714 s 和 33.4307507 s，包含观察工作，且第二轮复用 generic；不作 A/B 或 CPU 优化收益。

治理命令：
```powershell
& 'C:\Users\Logan\.cache\codex-runtimes\codex-primary-runtime\dependencies\native\powershell\pwsh.exe' -NoProfile -File Tools/Validate-ChangeLedger.ps1 -RepositoryRoot 'I:\GitHub\Unity_GAS\gameplay-ability-system-for-unity'
git diff --check -- Assets/NTSD/Scripts/Test/Editor/H10ResidentClosureDiagnostics.cs
```

保护审计实际由 Operation/final-audit-code.txt 读取逐文件清单并重算 SHA-256，检查真实 dirty/intermediate 备份、旧诊断正文、HEAD、暂存和 scoped diff。没有全量回归。

## 证据复用、仍缺的覆盖与预算

未受改变的生产 converter、音量/gain/retrigger、PCM 逐样本、正式 owner stage1/5、错误尾部和实际隔离 Play 证据，复用上一正式源每局所有权任务及有界 PCM 任务；不重跑固定五资源或旧 ABBA。此轮新增的是完整目录元数据、两轮对象账本、generic 跨轮复用与精确夹具恢复，不能代替上述播放语义或未来真实切局峰值。

330 定义的完整路径继续作为当前保守回退。它不证明任意运行时 producer 的封闭世界：stage/reserve、OPoint 子目标、变身目标、随机武器、respawn/late/external OID、weapon-piece、变量字符串 cue 的完整来源合同尚未闭合。仅按玩家+全 non0+静态 visual closure 做子集仍不可准入，旧 144,100,664 B 候选 payload 估算不是可兑现收益。分类证据与明确缺口见 coverage-and-budget-review.md；本轮只定位公开字段/入口，没有读 Q06 活跃方法体或偷偷裁剪内容。

当前未找到已批准的 steady/transition 音频数字预算。执行前 available physical 25,048,027,136 B、available commit 39,606,964,224 B，高于本次 2 GiB 操作安全余量；这是安全预检，不是产品内存预算。容量计算及环境见 capacity-preflight.json / capacity-environment.json。

仅供下一轮讨论、尚未批准的数值参考：

| 对 AudioClip 对象域的候选预算 | 高于本次 218.313758 MiB 的余量 | 适用限制 |
|---|---:|---|
| 256 MiB | 37.686242 MiB | 更紧；未包含未观测 decoder/staging、managed、voices 或切局峰值 |
| 320 MiB | 101.686242 MiB | 内容增长余量较大；同样不能代表整局/平台总预算 |

这两个数字不是推荐平台预算，也未写入拒绝/裁剪策略；是否采用必须由当前总内存分配与内容增长约束决定。低于当前完整目录需求的选择还需要单独明确 producer 闭包或内容格式决策，不能降规则来满足数字。

下一步保持在同一 H10：讨论会话独立审阅本报告、确定当前阶段可接受的音频对象预算及过渡峰值口径，并决定完整回退是否作为本阶段范围，或给出精确 producer 根/动态目标合同再另行实施。本轮执行范围结束，不自动改音频格式、lazy load、裁剪、producer、H07 或其他优化点，也不要求用户再次重复批准“继续 H10”。H10 父项与 Android 最终认证仍开放。


最终治理校验已实际完成。closing-audit.json 与 Operation/after-manifest.json 补充交付时源码/程序集稳定、逐文件最终状态及全部中间备份校验；范围外71,283文件使用本轮 final-audit.json 的新鲜全量哈希证据，不因后续仅治理文档修改重复跑Unity或完整音频组。
