# 第90批 NativeSampledState 八子步骤归因准备
> 当前结果：FOCUSED_TEST_PASS / ATTRIBUTION_READY / DIAGNOSTIC_ONLY / DEFAULT_OFF。新23＋原相邻33共56/56、0skip；本批无实际千人窗口/收益，H07仍FAIL、Goal active。下文PLANNED与测试清单是事前计划；“异常finally”本包仅完成八try/finally静态审查，未新增异常注入测试，不声称该case已跑。详[报告](../../../artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH90-NATIVE-SAMPLED-STATE-TIMING-20261009/REPORT.md)。

状态：PLANNED / DIAGNOSTIC_ONLY / DEFAULT_OFF；H07仍FAIL，阶段5/6，已执行69含本批准备，34父项关闭0，Goal保持active。89帧引用复用无一致收益，不推广、不重复cost。
## 原状与准确范围
有效六项首阶段合同第0—8节允许必要新信息诊断。88实际1000AI NativeSampledState约2.25ms，只定位整段；本次重扫NTSD28InputTwoPassModule.ProcessNativeSampledState确认含预处理/组合推进、combo route、三键route、方向route、Ground/fallback builtin、legacy projection、canonical AI同步、LF2Character.InputState同步。动作会修改frame/资源/方向，不能冻结共同frame或删除后继重读。本包只补八段观测，不删同步/引入Burst。
唯一三C#：
- Assets/NTSD/Scripts/Simulation/Input/NTSD28InputTwoPassModule.cs：原guard后读取既有World boundary timing flag与active recorder，八段RecordPhaseCall/Begin/try-finally End；原顺序、条件、动作读写、异常传播不变。AI同步仅AiControlled，InputState段按LF2Character原条件；无新owner/flag/buffer/委托。
- Assets/NTSD/Scripts/Simulation/Diagnostics/SimulationWorld.DetailTimingDiagnostics.cs：保留enum0—33，append NativeSamplePreprocess34、NativeComboRoute35、NativeThreeButtonRoute36、NativeDirectionRoute37、NativeBuiltinsRoute38、NativeLegacyProjection39、NativeCanonicalAiSync40、NativeInputStateSync41，Count42；CharacterInput/Boundary/NativeSampledState/<suffix>稳定名。四long数组Count冷构建多256bytes payload（非全RSS），栈4不变，无热扩容。
- Assets/NTSD/Scripts/Test/Editor/CharacterInputLiveSlotLoopEditorTests.cs：test-first NativeSampleTiming_具名schema、native/flag/recording gates、条件覆盖、逐tick完整输入proxy/history/checksum/RNG/资源等价、关闭重置、无效guard、异常finally和warmed直接委托0B。旧用例保留，仅85 PhaseCount34修订42且0—33 ordinal断言不变，不降低条件。
## 不变量、容量与关闭
正式336B44/closure B97、33/3/max2、seed/input/pass/RNG/checksum、publication只读、透明顺序/segment/fail-closed、seal禁扩容与11stage保持。仅原diagnosticsModule冷数组，disable/shutdown原owner，无新释放阶段。Q06活跃BattlePresentationShadowBuild方法体不读，不写ActionWriter/formal AI。各路由仍原时点重读frame。不切Role默认、不加Grid/Burst、不解冻EXT1/Mono/ATLAS，不改Scene/Prefab/资源/Importer/Settings/Gen/Plugins/Server。
## 验证与退出
原PID74724/Unity2022.3.62f3/MCP6402 Menu clean idle；原coplay连接非Unity6 Pipeline，不安装/升级包、第二Editor或Scene保存。只准确import与新case RED→GREEN，GREEN含原85 InputBoundaryTiming_/87 ProducerSnapshotReuse_及必要旧input回归；自然XML新absent phase保留。
本包不执行Play/真实1000AI/Profiler/GPU/M0/FPS；聚焦通过仅ATTRIBUTION_READY，不是热点已定位或H07通过。下一真实归因必须新准确Task/输出，不覆盖85/88；同request OFF/ON测observer开销/完整scope，不能将inclusive父子相加、局部0B替代完整逻辑证书。
## 留痕与恢复
Change NTSD-OPT-H07-NATIVE-SAMPLED-STATE-TIMING-090 / Operation同Task；准确三C#+五治理入口+公共XML共9existing写域，逐文件dirty SHA及新absent before-01无Force副本。继承89保护、after29条重核同，additional排除全部写域；附meta/依赖/实际request.json。89未带.json路径检查不能单独证明真实request，旧历史保留；本次正确路径实际absent。
恢复需用户授权准确dirty副本/hunk，不HEAD回退/删除。最后Validate-ChangeLedger.ps1、git diff --check、范围外SHA、before副本/Scene/HEAD/staged核对；唯一原总表更新事实，不新建重复总表。
最终结果与验证：原Editor56/56、0skip，01:21:21Z Ledger/diff exit0、929保护/9before核同，01:22:11Z方法外/旧tests还原对比同。详报告/audit-final-01.json，仍ATTRIBUTION_READY/DIAGNOSTIC_ONLY/DEFAULT_OFF；H07 FAIL/Goal active，下一实际子归因需独立准确Task，不将本包退出作为总目标停止边界。
