# H10 有界 PCM 临时数组生产实施与成本尖峰交回

日期：2026-10-10。Task `NTSD-H10-BOUNDED-PCM-CONVERSION-20261010`；Change `NTSD-H10-BOUNDED-PCM-CONVERSION-001`；Operation 同Task。

## 当前结论

**CORRECTNESS_PASS_COST_SPIKE_REVIEW_REQUIRED / FINITE_TASK_STOPPED；Change BLOCKED。**

生产改动已写入、原Editor编译并实际执行：正常 `PrepareBattlePlaybackClips` 的长单声道分支已使用有界缓冲，没有新flag。真实失败资格1/1、生产正确性与声音规则10/10、唯一成本测试1/1都通过其程序断言。

但**这不是完整限定准入PASS**。唯一大尺寸首个旧算法窗口出现46.8822ms单样本尖峰；派发合同明确：“若mean门失败或异常尖峰明显则交回，不重跑找PASS/加warm/加JIT/Profiler追系统”。均值门全部通过，尖峰仍触发交回。本轮不改合同、不重测、不归因，未声明 `SCOPED_PCM_TEMPORARY_MEMORY_REDUCTION_VERIFIED`。

当前两个脚本改动保留在工作树且新转换已接正常调用，**不是默认OFF候选，也未执行回滚**。需要上游决定如何解释此旧路径尖峰对限定准入的影响，或决定精确撤回本次生产增量。代码及证据已可审阅；不会自动继续下一项。H10父项、H07与Android最终门均保持OPEN。

## 授权、原状和范围

人类在讨论会话01a11fe2-bfaa-72f3-a9ed-c8260f63b193、turn01a12638-f06e-72c3-a1b8-79f0d1799fff直接回复“可以，开始吧”，授权H10独立检验后形成准确方案交本会话实施。本会话已read_thread核验，并读取具名[派发合同](../NTSD-H10-AUDIO-MEMORY-REVIEW-20261010/execution-handoff.txt)。

实施前生产SHA256为4ace07654e247a64fda417402c1086ed5a61dce798d7e7a3d01b7c15de2d6b32，与评审完全一致。诊断脚本、loader及两程序集身份也与评审一致。两个准确脚本写域：

1. Assets/NTSD/Scripts/App/NTSDSoundPlayer.cs：PrepareBattlePlaybackClips长mono分支及private CreateBoundedBattlePlaybackClip。
2. Assets/NTSD/Scripts/Test/Editor/H10AudioMemoryDiagnostics.cs：保留旧诊断入口/证据，增加实际生产对冻结原算法、真实读取失败/owner/guard以及一次性成本组。

没有编辑loader、CharacterAnimtorManager、AppManager、声音事件/聚合/voice/gain、其它既有测试、Q06、资源、Scene或ProjectSettings。原H08默认ON等受保护。

## 原代码到新代码

原方法每条mono创建float[samples]与float[checked(samples*2)]，整条读取后复制左右声道，再创建全长双声道AudioClip。

新代码对samples<8192保留上述原块逐字归一化一致；samples>=8192调用新private helper：

- 完整块复用mono[4096]、stereo[8192]。
- 首次GetData必须成功，随后仍执行checked(totalFrames*2)，之后才创建目标。
- 目标仍使用source.name、完整source.samples、2channels、source.frequency、stream=false；创建后立即加入原ownedBattlePlaybackClips，早于首次SetData。
- offset按已处理采样帧推进；只在0<remaining<4096时创建mono[remaining]与stereo[remaining*2]，精确尾部防止环回覆盖。
- 所有块完成才返回，整cue全部完成才赋cue.BattlePlaybackClips。
- GetData/SetData失败保留原异常文本并回到原firstOwned批次catch；借用source不被销毁。入口guard、null/nonmono借用、顺序、Release/generation/OnDestroy没有改动。

private helper只有每次转换的局部数组，没有新增静态状态、池、跨clip缓存、manager或诊断开关。所有新增状态的owner是当前同步转换栈，return/异常结束即失效；native输出仍由原ownedBattlePlaybackClips持有，沿原关闭链与OnDestroy的Release释放，十一阶段合同不变。

`scoped-code.patch`和自动差异检查证明：从当前生产文本移除新增分支与helper即可精确回到本Task真实改前生产文本。冻结原方法保存于Operation/frozen-original-method.txt；Editor控制仅替换private cue类型、this的Unity owner引用及Destroy限定名，保留完整旧算法/数组/guard/publication/catch。性能对照不是评审中的分块镜像。

## 实际命令与结果

原Editor Unity2022.3.62f3、PID39108、已连接127.0.0.1:6402；脚本刷新/编译后完成真实程序集重载。整个过程非Play，NTSD_Menu始终clean/8根。未启第二实例、重启、保存或切换Scene。

| 执行 | Job ID | 结果 |
|---|---|---|
| ProductionStreamingReadFailureQualification | b32bf761e6f6429aa9b5a8eb1b4ba078 | 1/1 PASS，0.6017967s |
| 四项新生产测试及原增益/重触发/销毁用例 | 459e3e8d2d954cafb0634cd70506f335 | 10/10 PASS，1.2699338s |
| 唯一ProductionBoundedColdConversionCostGroupOnce | 1e41fa9a0b8443558ae55c042c8cb153 | 1/1 NUnit PASS，1.4844698s；仅其均值/PCM/对象断言，不覆盖人工尖峰交回门 |

命令入口为refresh_unity(scope=scripts,compile=request)、run_tests(mode=EditMode,testNames=精确数组)、get_test_job(include_details=true,include_failed_tests=true)。完整参数见commands.json；原始JSON及XML均保留。每次共享Temp XML改写前真实备份。没有重复测试组或成本窗。

## 生产正确性与真实失败

边界实际10条mono：1、2、4095、4096、4097、8191、8192、8193、16384、65537；另1条stereo借用。原方法与新方法的每个float位、左右声道映射、samples/channels/frequency/name及最终Profiler对象内存完全一致，输入位不变。覆盖整倍数无尾部、最短尾部以及小于阈值的原数组路径。

四正式WAV只解码这四条，没有全量978解码：

| 文件 | 采样帧 | 频率 | 原临时float数据B | 新临时float数据B | 新float数组数 | 原/新最终clip对象B |
|---|---:|---:|---:|---:|---:|---:|
| c/jira/w/e3.wav | 329244 | 96000 | 3950928 | 67920 | 4 | 2636720 / 2636720 |
| c/kar/w/t.wav | 259584 | 48000 | 3115008 | 67584 | 4 | 2079440 / 2079440 |
| c/dan/w/t.wav | 236265 | 44100 | 2835180 | 82668 | 4 | 1892888 / 1892888 |
| data/001.wav | 11784 | 44100 | 141408 | 92256 | 4 | 97040 / 97040 |

临时float数据是按当前源码数组长度推导的分配payload，不含数组对象头，不是Profiler采样的GC字节或RSS/峰值。最终clip对象内存由Profiler.GetRuntimeMemorySizeLong实测。

Guard测试覆盖null cue、未加载、已准备幂等、空Clips、null源、stereo借用、多clip顺序、释放后borrowed存活与destroyed owner不能创建新副本。原NTSD28OriginalCommonAudioEditorTests未修改，实跑三个同cue跨角色重触发案例、mono增益上限、DirectSound中心增益与destroyed owner回归。

真实stream=true AudioClip.GetData返回false并记录预期错误。新生产方法实测：
- 本cue首条不可读：无新副本残留、未发布。
- 本cue先成功转换一个longclip再遇不可读：新副本全部撤销、未发布。
- 此前独立cue的1个播放副本保留，三条借用source仍活；指定fixture精确native clip ID集合与失败前相同。

**动态覆盖缺口保留：**同一可读clip的后续块读取失败、合法新建目标的SetData失败、巨大clip checked溢出没有被真实触发。没有新增生产注入机制。具体调用者try/catch、先owned后写入、最后一次publication、checked及合法offset证明见[failure-coverage.md](failure-coverage.md)。这些位置只记源码证明，不称动态PASS；这是派发允许明确上报的覆盖边界。

Unity的[GetData文档](https://docs.unity3d.com/2022.3/Documentation/ScriptReference/AudioClip.GetData.html)与[SetData文档](https://docs.unity3d.com/2022.3/Documentation/ScriptReference/AudioClip.SetData.html)说明stream读取限制及越界环回；本次关键偏移与尾部由实际生产PCM位比对补证。

本次19份新fixture owner审计、191条被观察输出资源记录全部释放。批次失败期间创建后立即回收的副本另由失败前后精确native集合检查覆盖，不混算为191。清理仅本fixture准确owner，借用source留给唯一源owner finally销毁；没有全局Destroy/清缓存/UnloadUnusedAssets/GC.Collect。显式Release证据不冒称自然Unity回调或全Editor零残留。

## 唯一成本组及尖峰

相同输入内容、44100Hz，两尺寸各原→新→新→原；每窗4warm+16sample，共8窗。Stopwatch覆盖实际转换和AudioClip创建，反射/冷delegate准备、输入/cue准备、assert/readback/Profiler对象大小与cleanup在外。production-cost.started使用CreateNew，确保本任务仅一次。

| 帧数 | 窗 | 路径 | 平均ms | 实测max=P95=P99 ms |
|---|---:|---|---:|---:|
| 65537 | 0 | 原 | 0.5394375 | 1.3299 |
| 65537 | 1 | 新生产 | 0.2962500 | 0.4630 |
| 65537 | 2 | 新生产 | 0.29226875 | 0.3533 |
| 65537 | 3 | 原 | 0.8246250 | 3.3423 |
| 329244 | 0 | 原 | 4.93554375 | **46.8822** |
| 329244 | 1 | 新生产 | 1.59643125 | 2.3204 |
| 329244 | 2 | 新生产 | 1.5734250 | 1.6967 |
| 329244 | 3 | 原 | 2.1918250 | 4.1288 |

原始ticks独立复算与JSON完全一致。16sample的nearest-rank P95/P99均落max；这不是稳定长尾证书。两尺寸每个新mean均小于两个原mean中的最大值，故程序均值门PASS；新mean也均小于两个原mean中的较小值，此事实不需要剔除尖峰或重算窗口。

46.8822ms发生在大尺寸旧算法首窗，未发生在新路径。其成因未知，不能凭长数组分配就归因GC，也不能归因系统调度。保持完整原窗/所有ticks，不剔除异常，不追加Profiler/JIT/warm或重复窗。按“异常尖峰明显则交回”的独立条件，本轮停在尖峰待审。

| 帧数 | 原float容量 | 新float容量 | 原→新数据payload B | 最终clip对象B |
|---|---|---|---:|---:|
| 65537 | [65537,131074] | [4096,8192,1,2] | 786444→49164 | 527064，不变 |
| 329244 | [329244,658488] | [4096,8192,1564,3128] | 3950928→67920 | 2636720，不变 |

短源仍2个数组/各1次GetData和SetData；长整倍数只2个固定数组，长非整倍数4个。长clip单次所有这些float数组payload之和严格<98304B，数组对象头另计。该值既说明累计申请数据量上界，也作为本同步转换局部数据容量保守上界；不包含最终AudioClip、source、测试读回或进程RSS。没有减少完整双声道播放副本、声道/频率/时长/音量。

## 证据复用、未执行与保护

复用独立评审的磁盘inventory/原行为链；旧978WAV/965mono与330283044→68499492B仅数学累计估计，未在本轮重做全库。原约210MiB双声道播放数据没有被本改动移除。旧评审镜像成本不冒充本轮生产成本，旧失败/超时/同名拒写原件保留。

未执行整局正常加载、实际加载峰值/RSS测量、按局音频闭包/AB/资源格式迁移、Play自然销毁、Driver/热路径0GC、正式120+180/1800或Android设备。冷加载允许分配；热路径0GC等最终合同没有降低。

15818保护文件无漂移，包括H08默认ON、其它声明保护生产/资源/设置及35份旧H10证据；两个源码与两程序集在正确性/成本前后同SHA。10份原始dirty备份和每次编辑前字节备份有效，HEAD与staged未变。最终validator、diff与closure-audit.json另附。

唯一实施工具异常：存储长patch命令的PowerShell进程因Windows命令长度上限OS206未启动；通过结构化apply_patch保存新审计文本后执行短审计命令。不是安全审批拒绝，不曾重复生产修改/测试或丢失内容。本轮Unity编译及所有实际测试未失败。

## 交回与精确撤回边界

不继续实验，不自动调整尖峰条款，不宣称限定准入成功。需上游在完整原件上决定是否接受“旧路径孤立尖峰、候选两窗仍低于两旧均值”的有限证据，或要求撤回该生产增量。任何新实验/规则/写域须新明确决定。

如撤回，仅逆向本次Prepare中的long分支和CreateBoundedBattlePlaybackClip helper；保留其它dirty和所有诊断原件。先记录当时preimage与Operation，不整文件覆盖、不用HEAD/reset/checkout。当前尚未执行撤回。

H10仍OPEN；H08正常默认ON、M03旧候选OFF、H07与producer暂缓、95OFF和H06既有成果保持。不恢复旧Goal、自动34项或下一优化点。
