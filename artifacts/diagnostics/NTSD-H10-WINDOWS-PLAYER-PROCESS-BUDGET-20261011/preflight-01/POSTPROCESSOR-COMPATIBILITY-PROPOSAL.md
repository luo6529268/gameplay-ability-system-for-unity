# H10 单次构建的旧postprocessor最小兼容提案

状态：**PROPOSED_SCOPE_EXTENSION / NOT_AUTHORIZED / NO_CODE_CHANGE**。继续同Task `NTSD-H10-WINDOWS-PLAYER-PROCESS-BUDGET-20261011`，不新开优化点。当前已批准方案仅拟新增3脚本；以下现有processor是第四条待批准写域，不能因本提案而修改或构建。

## 已证冲突

`Assets/NTSD/Scripts/Test/Editor/NTSD28Q07WindowsContentBuildProcessor.cs` 的旧常量是copy-manifest-v3.csv、1373、46927691；所有Windows/Windows64在OnPostprocessBuild自动进入。source和destination都要求精确集合。执行会话复核旧processor和两集合：当前正式源2334条/80126190B；catalog1、decoded_dat339、vfs1994；对旧1373是extra978、missing17、共有1356条内容/长度漂移0。旧manifest SHA53fd2e7bdddf4b4a7fdfad66e7f82ae58313381962de78d4a3d0a249cca15b6e。具体路径/哈希见postprocessor-compatibility-preflight.json。

这是静态构建前合同不兼容，不是一次H10运行或声音预算失败。不能先build撞错；不能删当前978文件、补旧17文件、覆盖旧manifest、禁用/反射卸载回调或切换工具绕过。

## 推荐唯一方案：在原回调里选择具名、一次性H10清单合同

保留现有callbackOrder=1000、Windows平台门、EXE/Data存在门、完整源/目标集合校验、逐文件长度/SHA、不可覆盖copy及SPARK校验。只在这个回调中允许**由新H10 builder持有的精确一次构建上下文**选择当前清单；不是跳过回调或任何内容校验。

准确候选改域：

1. `Assets/NTSD/Scripts/Test/Editor/NTSD28Q07WindowsContentBuildProcessor.cs`
   - 原`OnPostprocessBuild`38–40附近：在求出source/output/destination且验证EXE之后、ReadManifest之前，调用新builder的`TryGetActiveH10ContentContract`；没有H10上下文时照原三常量取旧合同。上下文存在却平台/输出/nonce/manifestSHA/状态不符时直接throw，不静默回退旧清单。
   - `ReadManifest`72–97：增加仅参数化expectedCount/expectedBytes的同一校验入口；默认包装仍传原1373/46927691。CSV固定五列、targetRelative、唯一性、非负长度、64位SHA及checked总bytes门不变。`VerifySourceSet`99–108和`VerifyDestinationSet`111–118仍对所选manifest执行完整集合比较，不增加ignore/allowExtra选项。
   - 成功日志输出所选合同id/count/bytes/manifestSHA，不再把H10日志误写为旧46927691。旧constants、路径、旧CSV以及非H10行为保持原样。必要时提供一个内部、只读的合同验证入口，供新builder预检和窄资格复用同一ReadManifest/VerifySourceSet，禁止独立复制另一套宽松校验算法。
2. 拟新增的 `H10WindowsPlayerBudgetBuild.cs`（原已提写域）：持有immutable `H10ContentBuildContract` 与唯一active owner；准备和校验新manifest、安装上下文、一次BuildPipeline.BuildPlayer、finally清除上下文并恢复backend。该上下文只有Editor临时生命周期，不写EditorPrefs/SessionState/环境变量/ProjectSettings或全局永久开关。
3. 原拟新增probe/PS sampler与此兼容修正无联动改变；不改旧stress builder/bootstrap、Scene、Resources、formal源或发布默认设置。

建议最小入口名：`H10WindowsPlayerBudgetBuild.TryGetActiveH10ContentContract(BuildTarget target, BuildOptions options, string normalizedOutputPath, out H10ContentBuildContract contract)`。这是明确新代码设计，不是当前已有API。

## 上下文的精确资格与生命周期

- Owner：唯一H10 builder在本Task一次Build调用的try/finally；只允许StandaloneWindows64+Development，原Editor、请求中固定Menu/Battle顺序和下面唯一输出。
- exact output：`artifacts/diagnostics/NTSD-H10-WINDOWS-PLAYER-PROCESS-BUDGET-20261011/build-01/NTSD-H10-Budget.exe`，规范化绝对路径与请求完全相等；不是目录前缀匹配、全局bool或任意caller传入文件。
- contract绑定Task/Change、唯一nonce、请求SHA、构建前源集合/manifest SHA、固定count/bytes、输出路径及当前一次owner。nonce用于单次归属，不当作安全凭据。manifest存在本身绝不激活分支。
- 状态最多一个owner，`Prepared→CallbackEntered→Completed`；同owner重复callback、嵌套/第二build、输出不符、上下文已消费皆fail-closed。回调只读上下文并登记消费/完成；不得在callback临时接受磁盘更新后的manifest。
- `finally`清除这个owner且还原原backend。域重载/Editor异常导致上下文丢失时，本次H10输出目标拒绝为context missing，不从磁盘marker自动复活、更不开始第二build。未完成输出保留。普通未激活H10的其它build仍走原1373合同，包括它对当前2334目录的既有拒绝，不借本包修全局发布打包。
- 仅冷Editor一个上下文/2334 immutable rows，无runtime manager/cache/worker；不进入战斗11阶段。失效于finally、消费完成或domain reload。请求/清单文件本身只读，保留构建前Operation与真正dirty前件。

## 完整源/目标集合与职责

正式2334逐文件清单已经只读生成：`h10-formal-copy-manifest-proposed.csv`，五列与现有格式相同，SHA见后附；没有复制或build。

- 源：`Assets/NTSD/Content/LoganRuntime/{catalog.csv,decoded_dat/**,vfs/**}`，所有2334非meta文件，80126190B。
- 目标：EXE同级`Assets/NTSD/Content/LoganRuntime/**`，相同2334相对路径/bytes/SHA，不允许额外文件（含meta）；输出root和每个父路径拒绝reparse/逃逸/大小写碰撞。
- 原SPARK复制仍由原processor完成：源`Assets/NTSD/Sprite/UIPanels/SPARK.bmp`→`NTSD-H10-Budget_Data/NTSD/Sprite/UIPanels/SPARK.bmp`。新builder用预先冻结的该条SHA再验证，不重复copy覆盖。
- generic声音417非meta：新builder在整个Build成功及backend恢复后，按原known-domain manifest复制到`NTSD-H10-Budget_Data/NTSD/Sound/**`，CreateNew/目标同名冲突拒绝，逐文件哈希与完整目标集合复核。它不进入formal source/target的2334集合，不修改旧processor的通用职责。
- 已知部署总计仍2752条/96956488B；剩余真实raw UI消费闭包另以R4A证据补齐，不盲copy Config/Sprite树。旧1373 CSV与旧输出始终不变。

构建前复核全部源清单+SHA；callback再核；构建后formal/SPARK/generic全部复核。任何source变化、旧相冲目标、额外/缺失条目首差停止，不修资源/清理输出再重建。

## 最小资格、实际构建门与回退

实施批准后，在新builder文件内部窄资格覆盖：

1. 无上下文选择旧路径/count/bytes，旧manifest身份不变；非Windows仍按原return，不无意激活H10。
2. 唯一合法H10上下文选当前2334；wrong output/target/development/requestSHA/manifestSHA、重复consume、上下文缺失/过期拒绝。异常finally清除owner，下一未激活调用回到旧默认。无需实际建第二个Player验证default。
3. 同一实际ReadManifest/完整集合验证针对当前源正确通过；用新临时fixture而非生产目录覆盖，验证missing/extra/重复/escape/内容SHA/长度/总量错误均拒绝；同SHA已存在目标只验证、不重写，冲突不覆盖。
4. builder前后源/旧CSV/Scene/资源/Settings哈希保护；新manifest/source对应完整，未经批准没有可执行build请求。

窄资格/原Editor编译通过且其它CASE/入口/部署门完成后，仍只有原计划**一次**Mono Development build。实际原postprocessor必须记录H10合同2334全过、SPARK过，之后generic417和全部2752逐项比对/Settings原值恢复；一次失败停，不能新增build刷通过。不存在“先跳过再事后补验”。

回退范围：获批后先单独保存当时旧processor真实字节/SHA/Git状态（当前SHA记录于JSON，仅是本预检基线），登记确切Operation和新第四code-path，再实施最小diff。若需撤销，只处理本分支/参数化差异和newbuilder上下文增量，并在授权下按当时前件/归属校验执行；不restore HEAD、不改旧manifest、不删当前formal内容或失败build、不覆盖其它dirty。当前无脚本可回退，也无任何回退执行。

本提案不提升H10/H07/Android状态、不解除平台发布后置；只是让本具名新诊断build在相同严格完整性门下使用当前真实内容集合。

当前只读manifest：count 2334；bytes 80126190；SHA256 `47efe8bdebe4c7a39e970fb3a6e1eef0300cf6a865ae36201f1f875289f24877`。
