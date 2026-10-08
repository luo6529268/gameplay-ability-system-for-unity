# 第65批：当前包络＋拒绝绑定路径的低频分支证据

当前执行：IN_PROGRESS（2026-10-08）。已先创建Change NTSD-OPT-H07-ENVELOPE-PATH-BRANCH-TIMING-065 / PLANNED与同ID批次Operation，11当前dirty恢复清单/180guards先落盘；副本核验及test-first接续。下方READY为事前快照，不能据此称本批完成或已测收益。

状态：READY / IMPLEMENTATION_NOT_STARTED / CURRENT_PATH_EVIDENCE_PENDING（2026-10-08）。本轮仅冻结必要动作及边界，没有本批C#、测试、Unity操作或测量，不增加43已执行批数；无需逐批重新询问已批准范围。实施前必须另建唯一Change NTSD-OPT-H07-ENVELOPE-PATH-BRANCH-TIMING-065及Operation，准确当前dirty副本/清单/指纹先落盘，不得直接用旧64副本覆盖。

需求源：有效六项Goal合同0—8节、64真实MIXED/PERFORMANCE_FAIL和[本次代码取证](../../../artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH64-ENVELOPE-BINDING-ELIGIBILITY-20261008/NEXT-ACTION-FINDINGS.md)。当前PairExactLoop平均37.519/38.887ms本身超33ms；旧branch timing在cache build和direction入口均绕过包络，不能回答当前残余成本。不是新性能候选，不重开45原出口，也不重复64短窗寻找PASS。

## 准确预期代码路径（实施前再次核现状）

1. Assets/NTSD/Scripts/Animation/Character/BruteForceSceneQuery.cs
   - 新增默认false的明确诊断opt-in EnableBruteEnvelopeBranchTimingForDiagnostics；只有旧branch timing/geometry/exact/envelope及有效recorder同时满足才计时，不能成为新的collector/default选择。
   - TryBuildBruteExactCache与CollectCandidatesForBruteExactDirection在该opt-in下允许保持原包络构建/拒绝；旧opt-in=false条件与行为保持，旧58 timing case不得改弱。
   - 复用原stride/逐collection旋转offset/Last与Total coverage、四detail phase。包络入口选一次方向采样，进入CollectCandidatesForPairCached时透传该次上下文，不能重复eligible/timed、改变stride语义或漏记第一次reject binding。
   - 不修改coarse谓词、CandidateCollectionPairAllowedCached、CandidateAccepts、RecordOverlappingBodyCandidatesCached、TryRecordReleaseCandidate、ItrRest或Runtime字段；clock只包围原调用，不调换逻辑/副作用，不反写规则。原frozen cache/BeginBuild/CompleteBuild/容量fallback保持。
2. Assets/NTSD/Scripts/Test/Editor/RoleAwareCollisionShadowSelfCheckTests.cs
   - 新前缀BruteEnvelopeBranchTiming_的聚焦test-first：default与非激活门、旧timing gate兼容、新包络实际应用/拒绝覆盖、overlap原coarse后续/union外kind5、多body顺序/RNG/handle、stale binding首次原时点、stride1及64旋转exact-once/Last reset/Total、容量fallback/不增长。
   - 复用已有fixture与AssertPairSnapshotRunsEqual，比较诊断OFF/ON而非改期望。只受影响旧58 inactive timing/binding/coarse和旧stride覆盖回归；不跑旧局部cost或历史全量。
3. Assets/NTSD/Scripts/Test/Editor/BattleOptimizationWindowsAiSuiteEditor.cs
   - 唯一65菜单及fresh windows-01根，明确新mode与冷owner；复用当前envelope-binding owner只在新mode加入新opt-in/旧timing bool/stride保存和配置。旧63/64 scope与cold guard保持拒绝新诊断。
   - 四运行两布局：新诊断OFF/stride64ON；双方envelope=true/binding=true，eligibility/kind5/coarseDispatch=false，原四production admitted=true/ForceBruteForce/noWorker保持。0warm/0sample前配置，逐run核旗标恒定/包络实际应用/coverage、所有正常/失败/退出先恢复全部旧值再FinishLogicGc，幂等解除owner。
   - 在该既有文件的新/旧RequestTests中覆盖四request/越界/冷scope/漂移/复原/abort/完整GC门；原17参BeginSuite等既有契约不改弱。只能复用原完整StepOneTick GC hook，不改observer或删原FAIL。

除上述三个C#和必要治理/诊断产物，无其他脚本写域。不增加第三方依赖/新测试script/meta、driver/native probe、Scene/Prefab/资源/Settings/Input/Gen/Plugins/Server。若准确实施确需其它文件，先更新Task与备份再写，不顺手扩大。

## 生命周期、容量与0GC

仅既有Query的诊断bool、栈上采样上下文、既有coverage/phase计数；不新建heap容器、跨tick缓存、worker或队列，不改变World/HitStore所有权。原pool封口/无热扩容、submission/lease/fence/11阶段不动；诊断新字段不进入logic checksum。关闭/退出恢复的owner仍Suite已有cold引用，不在OnDestroy创建服务。完整逻辑0GC按原前后1MiB正例/空例校准、300accepted/180steady/invalid0验收，不能用nanoseconds推字节或局部reader代替；不声称H11完整camera0GC。

## 冻结矩阵与验证

- 事前保留当前3C#、公共Temp XML/result、4治理文档和原总表的精确dirty字节；保护64五源/71原XML/四窗及20hash/完整snapshot、当前Scene/资源/Settings/AGENTS/authority/Q06 hash、并行TMP文件。不使用HEAD或旧副本代替当前dirty。
- 原Editor安全窗口/同一项目及compile idle先核；先上述最窄实际RED再实现GREEN，原件freshcopy防公共回调覆盖。不启动第二Editor/重启、Unity6 Pipeline/升级/外部部署。
- 必要聚焦通过后只一次同Suite四实际1000AI、120warm+180sample。request复用原64的65字段只output不同；run外诊断旗标以实际configured/unchanged/observed/restored确认，而不是从request推应用。
- OFF/ON均包络与绑定真实ON；timingOFF coverage0，新opt-in保持但旧timing关闭时不记clock，新ON stride64有真实eligible/timed、包络direction/reject与绑定probe/reuse。固定fixture64offset每方向恰采一次；实景coverage含warm+sample要明说，不能除steady或乘64当纯cost。
- 两布局配对末snapshot/20hash同，适用candidate序列/RNG/body/handle/stale binding聚焦PASS；source规则不变，不因新诊断重建native全量campaign。capacity/reject/growth0、完整logic校准、11stage三残留0、双Scene同/恢复；warm/sample/minActiveAi1000与harness有效。
- 对照报告计时开销差值/分支原样本/coverage/剩余PairExactLoop；一次顺序差只作仪器扰动信号，不定因，不宣称新FPS收益或Android/GPU batch/120FPS证书。不追加1800、full profiler/capture或同窗复跑。
- 若仪器缺口仍无法回答，记录具体缺口/可靠域，再按已有证据决定最小调整；不把未测当PASS，不继续无据微候选、不因次数停止Goal。

## 出口与后继

本批只闭合CURRENT_PATH_BRANCH_EVIDENCE/必要仪器兼容性，不能据诊断完成关闭H07。真正H07门仍logicP95<33ms/drop0/正式1800/其余完整门；H11旧43迟发12event及61byte预算UNKNOWN不改。达到诊断出口后立即用当前证据选择必要优化，不追加可选instrument细化；无收益的64候选不推广，原算法路径/33ms/3ms/max2/权威/排序/segment/failclosed不变。

实际启动前Change/Operation/Ledger/STATE/handoff准确登记；唯一主进度仍原优化总表。EXT1/Mono/ATLAS/Role-aware推广USER_HOLD保持，不从本Task推导授权。回滚只来自新Operation当前dirty副本，需另有授权，不reset/restore/checkout/clean/stash/删除/移动/push。未实施READY不计执行批。
