# 第81批当前共享绑定下Brute/Role
当前结论：SCOPED_WINDOWS_COMPLETED / ROLE_CANDIDATE_GAIN / H07_PERFORMANCE_FAIL / NO_DEFAULT_PROMOTION；2026-10-09。唯一Editor冷Suite接线编译/39具名聚焦PASS后，单次菜单四窗自然完成并退出。下文旧IN_PROGRESS、NO_NEW_MEASUREMENT和WINDOWS_READY均为事前历史，不代表当前状态。

## 当前实测（同版同资源绑定，非生产默认推广）

原项目Unity 2022.3.62f3既有Editor/MCP6402；当前冻结Suite SHA `48E894A458AB0DD2E4805359246BB35883BAA37A8AF61675FAC9B9E7B5050100`，四窗期间及完成后未编辑/import源码。唯一菜单 `NTSD/Validation/Optimization/Batch81 Post Bindings Brute Role 1000 AI` 约UTC2026-10-08T16:40:50调用一次；每窗120warmup＋180sample/总tick300，OID1 Sakura、真实最小AI与base roster均1000，共享绑定均true。两组65字段request差异仅 `formalCollectorMode` / `outputPath`，不是跨版旧80性能比。原始请求、report、完整snapshot、logic GC、observation与terminal均在 [windows-01](windows-01/)；[紧凑实测摘要](measurements-summary-01.json)按原件提取，不替代原件。

| 同版窗口 | logic mean ms | logic P95 ms | CandidateCollect mean ms | 全帧SetPass | dropped debt ticks |
|---|---:|---:|---:|---:|---:|
| Dispersed1000 Brute | 82.796288 | 100.369020 | 53.352677 | 5 | 738 |
| Dispersed1000 Role | 43.718983 | 50.438150 | 14.372448 | 5 | 276 |
| Combat1000 Brute | 85.581037 | 109.151830 | 56.140123 | 5 | 701 |
| Combat1000 Role | 44.397648 | 52.709720 | 15.098668 | 5 | 271 |

两布局CandidateCollect平均下降约73%，整tick平均下降约47%—48%。Role P95仍大于精确33ms，正常debt drop仍非0；只冒烟180sample，不执行未达情况下的1800刷证书。H07阶段PERFORMANCE_FAIL、H11完整camera0GC仍OPEN，六项阶段通过4/6，34父项关闭0，59已执行（79未执行），Goal保持active；本轮未调用Goal状态变更。

四窗 `workloadValid=true` / `StoppedCleanly`，中央physical segment均值1、中央CPU Draw均值2，SetPass均值/最大值5。CPU Draw、全帧SetPass与真实GPU batch不是同一指标；没有GPU capture、Android、Player、120FPS或正常App/多角色视觉证书。Editor受控显示帧间隔均值244.245/143.295/237.753/143.164ms仅为该owned诊断环境，不是目标机FPS。

## 正确性、0GC及生命周期的准确范围

完整末tick300 checksum JSON：Dispersed配对4210038bytes / SHA `F1A2CAC863D1AA995D49BEF7DA2D5C1B30337465FA846DB0CEFAF86D8AD7B6AA`；Combat配对4208480bytes / SHA `E4916ACA8C5BE675B8192D5A6C1429029FD94DB6F838CF6C82E532B030836B12`。同一布局完整文件一致；不代替逐tick/native trace或所有规则准入。

实际collector在owned query开始前已配置并核对；sample Editor更新观测Brute16/17次、Role10/10次，expected/last mode为1/3，configured/unchanged true、mismatch false、aborted tick0。此scope只是采到的last collection，不是每tick全部路由/无fallback证明。Role conservative fallback participant峰值Dispersed4/Combat3，不能称0fallback。report顶层 `broadphaseBackend=LooseQuadtree` 是配置身份，当前有实际DirectBroadphase计时且tree阶段0；不能宣称四窗一直用tree、不能冒充新UniformGrid或Burst实现。

可靠既有observer覆盖每个同步主线程 `StepOneTick`：前后各1MiB positive control与empty negative control通过，accepted tick300、steady scope180、非steady120，coverageValid/zeroAllocationPassed均true，四窗steady allocation events0 / proven steady allocated bytes0。原value单位TimeNanoseconds，不按value推算bytes；零事件＋前后正负校准支持该scope零分配。Combat Role非steady有24事件，不称全300tick0GC。此结果不覆盖完整camera物化—上传—录制—提交/native/GPU/worker，也不豁免H11第43批FAIL。

四run关闭均按原owner十一阶段自然完成，World objects/entities/slots、active GO/pool/reference pool borrower、cleanup exceptions均0；inactive pool1050为原容量留存而非借用残留。suite终态 `MEASUREMENTS_COMPLETED` / completed4 / error empty，orderedShutdown true、originalSceneRestored true、battleSceneUnchanged true。完成后UTC16:45:47原Menu sole/clean/root8、非Play/非Paused/无编译/无测试；Scene无保存/修改。

## 剩余热点与下一边界

Combat Role实测CandidateCollect mean15.098668ms、CharacterInput10.918602ms、CharacterHitConsumePostInteraction2.152895ms、LateEntityUpdate5.529504ms。Candidate细项DirectBroadphase3.333489ms、PairExactLoop7.369211ms（含当前exact消费周边工作，不可全称纯数学）、ParticipantBodyItrBuild1.328906ms、SortDeduplicate0.432869ms。父阶段与子阶段有嵌套，不叠加P95或把几何小段收益外推成全部logic收益。

用户空间分区建议正确地指向减少不可能碰撞的候选；查询必须按实际ITR/BDY保守范围而不是固定角色所在格＋八邻格，保留特殊路径、原序/去重/RNG、exact判定和seal容量。当前existing Role-aware自适应Direct/Sweep/tree有新鲜收益信号，但生产默认仍Brute；本包没有生产默认推广授权。下一只按已测热点选择必要有据动作，并先闭合准确生产准入/授权边界；不再堆无证Brute微缓存、不重跑H06或旧30找PASS，不新增UniformGrid/Burst/EXT1/ATLAS/Mono或改变cadence。对应源码事实定位于BruteForceSceneQuery的ResolveFormalCollectorMode；无规则变化。

## 验证命令、失败与保护

原Editor `manage_asset(action=import,path=唯一Suite)`，`run_tests(mode=EditMode,groupNames=准确具名filters)` / `get_test_job`：新18RED后GREEN1实际31/31、GREEN3实际8/8，去重39/39、0skip。中间错filter 0用例job及RED全保留；不是39一次job、不是catalog10184全部跑。31/8、RED及零用例XML SHA/具体job见下文和Operation。

运行前 [windows-freeze-01.json](windows-freeze-01.json) 保存6当前相关源、Scene/HEAD/staged、680保护及8原副本核同；完成后680路径0变化、冻结源同、HEAD a10c358/staged空。终交付validator/diff结果与逐文件SHA见独立audit-final-01.json（仅审计生成，不重跑窗口）。公共XML和terminal每次自然替换前准确副本均在Operation与各run，不删除任何旧失败/结果。唯一本批C#是Editor Suite冷入口；没有runtime算法/默认、Scene/Prefab/资源/ProjectSettings/Q06方法体/Server/第三方或破坏性Git操作。

## 事前历史（原文与筛选更正保留）

IN_PROGRESS / NO_NEW_MEASUREMENT / NO_DEFAULT_PROMOTION。准确Task/Change/Operation已登记，唯一Suite冷对照入口已写/原Editor导入；新18 RED缺API→GREEN1实际31/31、0skip（新18＋旧13）；另8formal fixture筛选namespace错误未执行，需只补准确NTSD.Test.RoleAwareCollisionShadowSelfCheckTests八项。四固定千人窗口尚未启动，windows-01 ABSENT，无当前Role性能结论。原80SetPass1995→5与H07logic87.920/P95112.167/drop725保持，H07/H11 OPEN/Goal active。

RED job8aec0768f5fc43c29b3e4b8d69c9b3b1及原件red-01/tests.xml（18FAIL）；GREEN1 job7694ff84b00346498cd4fd70686163c5及green-01/tests.xml（31PASS）。公共XML覆盖前逐次副本；680保护路径0变化、HEAD a10c358/staged空保持、validator1376/5源exit0、diff exit0。无生产算法/默认、Scene/资源/Burst/EXT1修改。下一准确8聚焦后才四同版Brute/Role窗口。

当前2026-10-09追加：实时catalog证明formal fixture实际为NTSD.Test.RoleAwareCollisionFormalCollectorSelfCheckTests；中间零用例job8d511081400f4e69a95de3c70d4ff0c2不计通过、green-02-empty保留。准确job892a3c97919f4e4c937d51436285b0c9实际8/8 PASS/0skip，总计去重39具名PASS。窗口前680保护/6相关源/8原副本核同、原Menu sole clean8/idle/noTest/noCompile、HEAD/staged不变，windows-01 ABSENT。准确windows-freeze-01.json已落盘；下一仅一次Batch81四窗菜单，运行中不改/import源码。

