# H06 Role候选记录链分析与方案收敛

Task / Operation：NTSD-H06-ROLE-RECORD-ANALYSIS-20261010。结论：**B / SCOPED_ANALYSIS_COMPLETE / DIAGNOSTIC_DESIGN_ONLY**。日期2026-10-10。未实施生产优化，未运行本方案诊断；本Task结束。

## 1. 结论与唯一缺口

83的既有六分支归因已经足够将本轮缩小到候选记录段，不再要求先重跑整个精确循环归因。该段不是纯复制：包含首次pair snapshot、group过滤、kind4计数、reject、nearest/tie RNG、容量/select和store/legacy写入。

**唯一决策缺口：同一批实际Role记录调用中，时间主要花在可重复的纯取值/筛选，还是必须逐候选执行的选择与写入；现有record总计时没有内部互斥子段及相应命中次数，不能区分这两种情况。** 下文只给一次有限诊断设计，不先写缓存再找收益。

不选A：新发现的Prev2读取不同于pair snapshot的Prev，但已是数组索引；只凭重复出现不能证明值得缓存。group resolver也是纯字段分支，不是FindEffect查表分配。方向内snapshot已经复用，跨对象endpoint及早拒绝路线已有反证/缺口，不能换名重试。不选C：record在83同口径抽样中最高，仍包含可能重复的纯工作；尚不足以断言整条record路线没有值得处理的块。B结束后必须形成具体A或C；仅计时失效/正确性/GC失败则具名报告失败并结束，不自动追加窗口。

## 2. 证据复用与限制

| 证据 | 复用结论 | 不允许推论 |
|---|---|---|
| [83六分支](../NTSD-OPTIMIZATION-BATCH83-ROLE-EXACT-BRANCH-TIMING-20261009/REPORT.md) | stride64按方向轮转采样；RoleCandidateRecord稳态每tick抽样累计均值D0.044411/C0.045618ms，六项中最高。其余PairAllowed0.016045/0.016596、Coarse0.006966/0.006889、ItrAllowed0.017727/0.018168、BodyGeometry0.005854/0.006194、CandidateAccepts0.001565/0.001487ms | 不能乘64估全量，不能当单次record耗时；eligible/timed覆盖包含warm+sample，不除以180。不能与91跨窗合成百分比 |
| [91](../NTSD-OPTIMIZATION-BATCH91-NATIVE-SAMPLED-STATE-WINDOWS-20261009/REPORT.md) | 历史PairExactLoop约8.115/8.357ms位于collector15.734/16.156ms内部，是定位线索 | 不能相加，不能说8ms可消除或都是几何 |
| [Role默认准入](../NTSD-H06-ROLE-DEFAULT-ADMISSION-20261009/REPORT.md) | 已有默认接线13项及双布局收益/适用0GC/关闭结论维持；D collector55.391590→15.658797，C59.901228→17.485331ms | 本轮不是新测量；默认Role通过不代表本方案通过或H07/Android通过 |
| [FindEffect](../NTSD-H06-FIND-EFFECT-ZEROGC-20261009/REPORT.md) | 查找分配修复和100/1000限定结果保留，当前分析未改其路径 | 不解释ABBA或95，不当完整Driver普遍0GC证书 |
| [ABBA](../NTSD-H07-PRODUCER-ABBA-DIAG-20261009/REPORT.md) | 首OFF完整Driver稳态2event、首tick130调用点未知，其余三窗0；独立GC FAIL保留 | 本Task不修复/重测、不自动归因record/FindEffect/95，也不以旧0GC覆盖 |

这些报告及本轮只读源码/原生来源的SHA-256存于[fingerprints.json](fingerprints.json)。哈希绑定本轮读取版本，不重新认证历史测试源。25局部collector与82有限Driver的证明边界保持；没有拿两者合成全域证明。

## 3. 当前链与重复粒度

以下行号以本轮指纹为准；路径根为Assets/NTSD/Scripts。只检查候选生产/选择/写入；不读活跃Q06排序方法体或候选消费实现。

| 调用 / 位置 | 读取字段、实际粒度 | 副作用与不能省略的顺序 | 可复用边界与失效 |
|---|---|---|---|
| Animation/Character/BruteForceSceneQuery.cs::CollectCandidatesForPairCached :6347 | 每attacker→target方向；参与者CurrentFrame/CollisionFrame、carrier、exact ITR/BDY范围；每ITR执行原ItrAllowedCore及rest等前置 | 保留原pair方向、ITR索引和BDY顺序；先pair/coarse/ITR门再body | 本方向栈上pairSnapshot及groupWitness只各建一次；方向返回即失效，不跨反向/tick |
| RecordOverlappingBodyCandidatesCached :6576 | 每ITR×目标BDY：Z、矩形、CandidateAccepts；通过才进入record计时 | body overlap统计和既有候选顺序保留；多body不自动合并为一个record | 几何准备数据已经缓存；不能把多body折成一次副作用 |
| ItrAllowedCore → BattleHitCandidateEffectTypeResolver.Accepts :8103 | ITR预筛中的effect/type纯switch；effect13/14/15/16及默认分支 | 此filter在record外，不是record的昂贵catalog来源 | 相同输入可得相同结果，但不是新的record优化点；rest前置保持原时点 |
| pair snapshot捕获 :6692附近；BattleHitCandidatePairSnapshotFactory.Capture/CapturePayload | 首个通过geometry/accept的body；attacker/target当前frame、tick frame、Frame.Prev state、OID/type、RelationTeam、action、facing、holder在场/组 | 捕获时点决定候选携带的快照；holder通过原World/Match现场解析 | 31已每方向惰性复用，不能再声称每body都重建；未知实体/缺缓存/原fallback走原路 |
| RejectRoleGroupBeforePairSnapshot :7146（92开关OFF） | 惰性方向级typed witness：OID/type/team/action/current state/facing；每次按itr.kind/effect和实时modegate判 | 只有拒绝且尚未capture时才跳过该ITR余下BDY；不能携带缺字段witness到consumer | 这是92现成候选，不是新方案；test hook使其不生效，team/modegate变化不能跨collection保存 |
| TryRecordReleaseCandidateFromSnapshot :7055 → GroupEligibilityResolver.Resolve | 每个通过geometry/accept的body：完整snapshot、kind/effect及world.ActiveModeHitGroupGate18 | group拒绝先于kind4；kind4/8/50、状态10/13、freeze column、group/mode/type/facing依原分支 | 纯判断但低成本未知；跨ITR复用必须含kind/effect/mode，不能只按pair key缓存 |
| kind4计数 :7081 | 每个group通过的body，itr.kind==4且EnvironmentState320>0 | Kind4SourceCount92=(old+1)&0xffff，在low-fall/nearest/select拒绝前也执行 | 不是纯读取；即使最终未写candidate也不能省或按方向只加一次 |
| ResolveReleaseRejectFlag :8323 | 每body；target当前state、itr.fall/kind、bodyX；nearest路径可读attacker OID/type/team/holder | low-fall及bodyX特殊拒绝；保持group/kind4之后、nearest之前 | bodyX随BDY不同；holder/team/OID不能跨身份或同tick变更复用 |
| TryRecordNearestPathCandidate :7181 | 每body；vrest/kind、target.Frame.Prev2 state、attacker type/LinkState；水平距离及现场holder位置、attacker.HitCandidateNearestDistance | 小于替换；相等才消耗同步RNG；未替换也返回handled=true；替换store单项、legacy Clear/Add、count=1 | nearest best随着每候选变化；不能按pair/ITR缓存接受结果，不能去掉同距RNG。vrest是选择分支，不可假定都走nearest |
| GetCollisionCandidateProducerCount :7251；ApplyPrev2GroundRejectFlag :8354 | 未被nearest处理时，先读实际store row/Runtime count，达到20返回；之后读目标Prev2/state和attacker type/LinkState | 不将容量提前越过kind4或nearest；非nearest reject在容量之后 | count变化于每次写入；target Prev2不是snapshot.PreviousState（Frame.Prev），不能互代 |
| AcceptReleaseSelectFlagCandidate :7588 | kind1 target.HitCandidateKind1Distance、距离；kind1朝向输入/current state；kind2/7 jump edge、LinkState/current state | kind1同距RNG，先更新目标best再检查方向/state，可能最终拒绝；容量满必须更早返回；2/7保持当前边沿读取 | 目标best由其它attacker写入；不能entity/pair级提前决定，不能移动输入消费/RNG位置 |
| SceneQueryHit构造→WriteCollisionCandidateStoreFirst :7316 | 每次真正append/replace：target slot/当前handle、bodyX、ItrIndex/RuntimeItr、消费标志、pair snapshot | requested/healthy/building门；写失败AbortBuild/healthy=false，异常诊断保留；target generation是元数据，不能改为新权威拒绝门 | 不重复解析candidate之外的全实体；attacker handle由已验证参与者传入或原回退。store epoch/generation必须现场有效 |
| CollisionCandidateStore.TryWriteAt :310 / TryReplaceSingle :348 / TryGetProducerAttackerRow :442 | 每次实际写/读，slot、row epoch、attacker generation、count，固定20条/attacker | 普通写只允许index≤count且<20；nearest可覆盖0并设count1；首个nonempty行登记。legacy是否写依原模式 | slot复用/generation/epoch变化立即失效；不可缓存成功验证跨tick，容量不足或store故障不可悄悄drop |

`GetAuthoredPrev2Frame` :8017 → `LF2FrameCache.GetNativeFrameDataById` 是Wrapper/范围检查后数组索引和预制零帧回退，不是Dictionary遍历。两处Prev2读取确实存在，但并非每record必定执行两次（nearest被处理后立即返回；其它分支受容量/reject门限制）。本轮没有把“最多可能重复”写成实测次数或节省时间。

所有entity级/方向级/ITR级复用必须区别：定义数据身份（FrameCache.Load/Clear/对象变形）、runtime slot+generation、team/holder/link/current/Prev/Prev2、vrest/rest和可变best/count/输入。31当前方向栈缓存不因此被否定；新缓存若跨越其边界，必须另证同tick稳定性，不能用线程单一或“同tick”作充分证明。94的whole-world确切LF2Character门说明非碰撞holder也可能影响安全性，不能只看当前pair类型。

## 4. 与旧候选逐一排重

| 候选 | 实际覆盖/结果 | 本轮处置 |
|---|---|---|
| [31](../NTSD-OPTIMIZATION-BATCH31-PAIR-SNAPSHOT-REUSE-20261007/REPORT.md) | cached方向内跨ITR/body惰性snapshot，栈变量；已在当前Role路径 | 不再提出方向内snapshot复用 |
| [84](../NTSD-OPTIMIZATION-BATCH84-ROLE-CAPACITY-RECORD-GUARD-20261009/REPORT.md) | kind0/vrest非0/实际producer≥20才跳record；真实32tick两布局skip0 | OFF，不再跑人工满队列寻采用证据；不能猜skip0全因nearest |
| [89](../NTSD-OPTIMIZATION-BATCH89-ROLE-SNAPSHOT-FRAME-REUSE-20261009/REPORT.md) | 捕获时复用4个current/tick frame引用；D局部-2.81%、C+5.75%，无一致收益 | OFF，不改名为frame缓存重试；Prev2不同也不自动值得做 |
| [92](../NTSD-OPTIMIZATION-BATCH92-ROLE-GROUP-SNAPSHOT-SKIP-20261009/REPORT.md) / [93](../NTSD-OPTIMIZATION-BATCH93-ROLE-GROUP-SNAPSHOT-WINDOWS-20261009/REPORT.md) | witness预拒绝省首次snapshot及余BDY；局部V2收益不抵实景无证明，D48.3097→49.9600、C57.6390→70.6142ms | OFF，不重开早拒绝。本轮若测得这块高也不能直接重推相同版 |
| [94](../NTSD-OPTIMIZATION-BATCH94-ROLE-ENDPOINT-SNAPSHOT-20261009/REPORT.md) | entity/collection endpoint缓存跨pair复用，holder仍live；最终whole-world类型守卫在局部测量后加严，仅最终focused证据 | OFF，不能把旧V2局部数据当最终版本收益或换名为端点标量缓存 |

所提B只分解当前已准入Role的record成本，不开这些优化开关、不实现替代缓存；测出旧候选已覆盖的块后仍需新机制/适用性证据，不能再刷同版长窗。

## 5. 原生来源与权威证明边界

本轮只读根正式`J:/QQFile/NTSD2.8.3.3 zip/NTSD2.8.3.3/NTSD 2.8-Logan/NTSD2.8-Logan.exe` SHA256，仍为336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3，未运行EXE。

最接近来源为`source/ntsd28_core/src/simulation/battle_world.cpp`：4644—4691 kind1容量先于选择、best更新先于方向拒绝；4708—4775 kind4源计数、low-fall、multiple/nearest、holder距离、同距同步RNG和single写；4930—4966 group纯字段门。`source/ntsd28_playable/scripts/build.ps1` 132/134列入battle_world和simulation_tick_driver编译闭包。未读其排序消费方法体。

**来源限制：README_SOURCE.md :380—381明确当前源码持续开发、不能当作根EXE逐字节对应。** 84已经保存正式336/B97闭包对这些不变量的引用，本轮复用其限定证明；本轮对当前源的窄读只是顺序/字段交叉核对，不新增正式EXE等价结论。尤其current source的tick_action_snapshot/target object type与Unity Prev2/attacker type不机械同名映射。本任务不判新规则差异、不修规则；如果未来优化需要据这些细节改变规则，必须先另补正式对应版本/可观察证据。静态诊断计数不需要改动它们。

## 6. B：一次诊断的准确设计（本轮未执行）

**要解答什么：** 在实际1000 Sakura combat Role路径中，record的纯重复工作是否形成一个可清楚界定、未被31/84/89/92/93/94覆盖的成本块，还是主要为必要nearest/select/write。答案直接决定提出一个A生产方案或以C结束此路线；不是为旧候选补PASS。

**最小实验：** 一个具名诊断Task，只有一个combat1000 fixture的匹配OFF→ON两窗，各120warm+180sample，沿当前`BuildScopedRoleAdmissionRequest(3)`生成的Role实景配置，seed1314149188、1000 Sakura、固定AI配置与现有地图/渲染/声音。运行前导出完整request和所有实际query/profile开关，比较到只有record诊断开关与输出路径不同；producer reuse、95以及84/89/92/94均OFF。不得调用会自动跑四窗的旧suite入口。只诊断，无默认晋升，无1800正式证书。布局只选combat，结论不得推广为另一布局收益。

**观测方式：** 复用83方向stride64、轮转相位与既有DetailTiming机制，不另建全局Profiler/采样线程。不再输出一次旧六分支归因来代替新信息。在相同被选方向内，对record作互斥累计：

1. 首次snapshot（及实际fallback路径标签）；
2. group gate；
3. kind4加计数与initial reject；
4. nearest判定/Prev2/距离/best/tie RNG，**到candidate构造前截止**；
5. 普通capacity/Prev2/select，含kind1 best/tie RNG，**到candidate构造前截止**；
6. 实际SceneQueryHit/store写入；
7. legacy mirror及Runtime count提交。

必要细分只对重复纯工作加计数（非逐调用加独立timer）：首次capture数、record总数、同方向后续record、同ITR后续BDY、Prev2实读、group reject、kind4加计数、nearest进入/不替换/替换/tie RNG、普通容量拒绝/ground拒绝/select拒绝、kind1 tie RNG、append/replace/store invalid及legacy touch。不得为计数额外调用原resolver、取RNG、创建字符串/集合或改条件求值次数。nearest与普通路径共用write计时，同一write只归一次。父record包含子段；只检查覆盖/残差，不将父子相加称优化收益。

**同scope：** 每tick固定大小原始long计数与elapsed（非累计warm混sample）；冷端在第120tick边界保存warm终值，稳态取正确差分，并按3个60tick块报告覆盖和七子段累计。计数与时间均标全体/被采样方向，不能混分母。记录Stopwatch频率、空边界测时底噪；不能乘64外推，也不承诺基于采样比例的整tick节省。低于测时分辨率或覆盖不足就是UNKNOWN，不追加轮次。

**owner/容量/关闭：** 诊断开关默认false，挂现有query/DetailTiming owner；只固定标量/定长计数存储，冷预备，无per-entity缓存、无新lease/worker/队列。精确容量为7计时槽和实施前冻结的固定计数槽清单（可使用具名struct字段，无动态集合），不能热增。每collection/tick按既有诊断复位；重建world/query不携带上一轮状态。关闭阶段1不再接受新采样，阶段2沿原worker Join门（本fixture main thread仍不得绕过合同），阶段4解除seal之后仅冷端读已冻结标量，阶段8清query计数/开关绑定、阶段9随world解绑；suite finally恢复设置，不新增关闭阶段，不发publication。若现有owner的清理接口不能容纳，先列准确新写域，不在此草案悄悄扩展。

**必要验证：** 原项目编译；聚焦诊断OFF不参与、stride及reset、group拒绝/kind4拒绝后计数、nearest相等/不替换/replace、kind1失败仍best变化与RNG、容量20、store故障/fallback、关闭重建。复用既有具名夹具，只增加观测断言，不整套重跑。OFF/ON同fixture逐tick输入、双RNG、checksum及最终候选顺序/载荷一致（观测/序列化放计时外），适用完整Driver可靠校准0GC、容量、fallback/abort、关闭零残留、Scene/设置保护分别报告。原用例如只提供稀疏hash，不把它称逐tick；在既定测试写域内补冷观测，无法无分配完成则停并给出具体缺口。

ABBA首OFF的未知分配未被消除；不得仅因本次另两窗0而关闭旧问题。若这次相同路径再失败，本Task的诊断资格FAIL，保留原件、停止第二窗/后续优化，不反复重跑，不扩成分配修复专项。无需为只读准备先重跑ABBA；0GC不是可取消门。

**何时足够并停止：** 两窗实际设置一致、必要正确性/GC/容量/关闭通过、七段在同sample方向内可核对且残差/计时开销可解释，三个60tick块能给出一致主要成本归属，才选下一A或C；样本不足、主要排名不稳定、仪器开销淹没信号均记INCONCLUSIVE并停止。仅当一个未覆盖的纯重复块高于测时底噪、有实际重复覆盖、能保留所有副作用和失效合同，才提出一个准确A（仍需该生产改法自己的准入门）。主要为必要选择/写或旧候选块则C。这里不预设毫秒或收益比例，不将新instrument ON/OFF差当生产优化收益。不得为了形成A继续加第三窗或第二诊断。

## 7. 下一条有限执行指令草案

以下是供讨论会话按现有人类约定直接输出的单一具名指令，本Task不发送其它会话、不执行草案、不修改监测。普通范围内步骤不再询问A/B或逐步许可；红线/新合同仍需用户决定。

> 在原项目执行 `NTSD-H06-ROLE-RECORD-COST-DIAG-20261010`，只完成本报告第6节的一次record内部成本诊断，不实施生产缓存或切换优化默认。先读CURRENT-AUTHORITY、AGENTS、本分析REPORT及其fingerprints，再核dirty与原Editor安全状态。先建独立Task、Change Record、Operation和真实dirty备份。
>
> 最小代码写域：`Assets/NTSD/Scripts/Animation/Character/BruteForceSceneQuery.cs`（上述record/nearest/store具名生产方法只加默认OFF诊断，实际分支/取值/RNG次数不变；SelectRoleBranchTiming复用）；`Assets/NTSD/Scripts/Simulation/Diagnostics/SimulationWorld.DetailTimingDiagnostics.cs`（BattleTickDetailPhase尾部新增固定槽、既有诊断数组/复位，不重排旧enum值）；`Assets/NTSD/Scripts/Test/Editor/BattleOptimizationWindowsAiSuiteEditor.cs`（新增独立两窗冷入口/请求及观察输出，复用BuildScopedRoleAdmissionRequest(3)，不复写旧83输出、不运行四窗旧入口）；`Assets/NTSD/Scripts/Test/Editor/RoleAwareCollisionShadowSelfCheckTests.cs`（复用已有副作用/顺序夹具，新增观测与0GC/关闭断言）。实施前冻结新增符号、七段边界、固定计数字段、owner和回滚。禁止改snapshot/group/effect规则、store实现或消费/排序方法体。
>
> 按第6节先编译及受影响focused，再一次combat1000诊断OFF→ON、各120+180；输出准确request/actual flags、源码指纹、逐tick与必要末态一致性、校准完整Driver0GC/容量/关闭、稳态及3×60块七段计时/同口径覆盖与测时底噪。保持producer USER_DEFERRED/OFF、95OFF、Role默认、84/89/92/94 OFF。不能将ABBA首OFF未知GC关账。任何正确性/0GC失败或无法解释数据，保留候选关闭并以本Task失败/不确定收口；不得再刷窗或顺手修其它点。
>
> 只给一个明确A生产改法或C结束结论；数据不足则具名INCONCLUSIVE终止。更新同一H06-ROLE-EXACT、唯一进度、必要依赖/Change/Operation并运行适用ChangeLedger validator。编译、focused、实景诊断、正式性能及设备证据分层。不得读活跃Q06、改Scene/资源/Settings/Input Actions/Server/Gen/Plugins、解冻专项、改AI行为/33ms/pass/publication/十一阶段关闭。若准确实施确需新增写域或规则合同，报告具体原因，由用户决定；不恢复旧Goal，不自动继续下一优化。

## 8. 本轮实际执行与停止边界

实际命令类别：`Get-Content -LiteralPath`读取Task/Operation/authority/方案与必要历史报告；`rg -n`及`rg --files`定位具名源码/文件；PowerShell here-string经`python -`按限定行号读取生产record/选择/store和原生纯字段片段；`Get-FileHash -Algorithm SHA256`核正式EXE；`git status --short --branch`、SHA比对保护。文档通过同一方式写入，未保存/执行新的项目脚本。完整保护与文档检查结果见[validation.json](validation.json)和同IDOperation。

本轮建立/续写Task、Operation、REPORT及指纹/校验证据，更新已登记七个治理文档。脚本、资源和运行行为未改；不创建假代码Change，不运行ChangeLedger validator、Unity编译、测试、Profiler、实景、正式EXE或设备测试。最终`git diff --check`和保护检查单独记录，不冒充运行验收。

本轮原生最邻近路径只静态核对，不扩大到formal source审计；不解冻Q06。没有跨会话读取/发送/控制，没有恢复监测/自动Goal或自动执行下一诊断。producer暂缓、ABBA未解分配、95OFF分别保留。Role限定准入结论不变；H06父/H07/Android开放、34父关闭0、六项首阶段5/6、94/73与95第74启动包数不变。

最终文档验证（2026-10-10）：保护清单2514/2514字节不变；7份事前备份SHA匹配；六份治理正文旧字节完整保留，INDEX旧前缀保留；24份源码/证据指纹未变。报告相对链接有效。`git diff --check` exit0，只有LF→CRLF转换提示（含已有dirty文件），未清理或格式化这些内容；HEAD/空暂存diff与事前相同。Task/Operation历史状态以最新追加为准。
