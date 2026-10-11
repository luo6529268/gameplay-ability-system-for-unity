# H07 Indexed nearest cursor reuse：有限候选收口

**COMPLETED / INCONCLUSIVE_NO_LOCAL_GAIN_SIGNAL / NOT_ADOPTED**。Change NTSD-H07-INDEXED-NEAREST-CURSOR-REUSE-001 ABANDONED，含义是本版结束采用，不是删除代码或改写测试结果。候选以未接生产的internal诊断实现保留。原生产public入口/调用完全不变，无新的生产默认或准入。

## 实际代码：原来→候选→减少什么

- AiSensingKernel.cs新增 internal TryFindNearestCursorReuseForDiagnostics，签名与带policy的public一致；非Indexed原样委派public。复用原IsIncluded、IsGroundTarget、IsAirTarget、TeamAllowed、LowerBound、CaptureHandle和int Abs。
- 新增private FindIndexedGroundCursorReuse/FindIndexedAirCursorReuse。当前call/team span保存left/right cursor、slot、dx、refresh布尔；只在该侧移动后下一有效迭代重读该侧X差，选中后直接复用selectedDx。原break后不额外预取，耗尽侧不读索引数组。
- 原逻辑每轮重算左右dx，选中后再次算dx；候选消除这些重复算术/读取，不减少候选访问、目标资格检查、visits或随机调用。没有改变索引构建、HP/state/HitStop、非角色3000方向/NaN拒绝、left<=right tie、smallslot tie、10000/249/250/40界限、state9、Ground facts及Air覆盖、epoch/generation/identity。
- **所有原kernel文本完整保留**：移除独立新增179行后与改前源码相同。两个public、TryFindNearestCore、原Indexed loops/helpers及所有生产调用未改。引用搜索仅候选自调用和本测试冷反射字符串。
- 仅另一写域AiSensingKernelEditorTests.cs新增冷一致索引helper、语义/0B/一次成本测试。无World/Writer/Store/AI/旧Suite/H07InputCostDiagnostics/资源/Scene/设置改动，无Q06体读取。
- 候选局部值归当前栈帧；函数返回失效、下次重读快照，无static/global flag/cache、跨tick数据、容量增长或新增关闭资源。原十一阶段不变；Stage1停止查询后无新drain。测试snapshot固定冷数组由测试拥有，不注册World或Scene对象。

## 实际C#验证

原Editor PID39108 / Unity2022.3.62f3 / TCP6402 单实例，全程EditMode，未Play/保存Scene/启动第二实例。
1. RED job09f13dc464a74960879c7ceb0ef97878：1项预期失败，明确候选入口缺失。
2. 编译后 job9dbc866f292f4a64aa15f5ed5a1a06bf：本文件原/新18/18 PASS，包括256固定seed随机夹具16,351次原Indexed/候选全部字段+两visits对照，正常范围FullScan payload对照；线性visits不强行等同。另有明确边界、可用性、空ready索引、非Indexed委派、调用间位置/state/team/index/epoch/handle变化及int极值原Indexed语义。
3. 精确补充projectile方向/等X/NaN jobdf11369ae7f1448abe040a48b1555edf：1/1 PASS，x=-10/0/10 × vx=-1/0/.001/.002/1/NaN共18明确组合。只加此测试，没有重做旧18项或全库回归。
4. 两布局原/候选同scope热循环：每路径8*1000 warm、32*1000 measured，GC.GetAllocatedBytesForCurrentThread增量0、容量/数组引用和完整快照序列化不变。测试断言/反射/日志/序列化在测量范围外。
5. 新冷索引构造完整生成Ground/Air team spans、Special membership和living-team统计，每次用项目原ValidateIndexedContract核验；不能以遗漏索引修正生产。故意ready-empty违例只用于验证既有“不回线性”合同，明确不作正常成本夹具。
6. cost helper在语义门后加入为Explicit，测试过的production/fixture方法未修改，最新程序集/源码SHA在source-and-assemblies-before-cost.json。旧review离线23,870等模型只作准备，不冒充这些C#结果。

## 唯一局部成本组

实际 job5ce4867a7a634c1ab0b07984a36f6610 完成1/1测试，**NUnit PASS仅表示执行、0B/容量/结果门通过；收益判据为INCONCLUSIVE**。
两个immutable1000行snapshot，seed20261010，spacing120/12；Team=slot%4，固定state/HitStop、X/Z分布，固定self升序和phase(self%3==0?1:2)。各布局只OFF→ON→ON→OFF一次；每窗8轮1000query warm＋32轮sample；delegate冷绑定，Stopwatch只包整轮1000query，内部仅结果轻量聚合与原查询，无新细分计时/hook。GC包整个32轮测量循环，数组预备在前。CreateNew started标记禁止重复同组；原件local-cost.json未覆盖。

单位为 **ms/1000query**，P95/P99为32个整轮样本排序后线性插值，绝非整tick：
| 布局spacing | 窗 | Mean | P95 | P99 | GC bytes |
|---|---|---:|---:|---:|---:|
| 120 | 0 OFF | 1.158872 | 1.224240 | 1.307652 | 0 |
| 120 | 1 ON | 1.198494 | 1.288170 | 1.373608 | 0 |
| 120 | 2 ON | 1.202287 | 1.284125 | 1.322180 | 0 |
| 120 | 3 OFF | 1.313500 | 1.671780 | 1.688836 | 0 |
| 12 | 0 OFF | 1.256872 | 1.320775 | 1.357391 | 0 |
| 12 | 1 ON | 1.296725 | 1.333350 | 1.368883 | 0 |
| 12 | 2 ON | 1.288997 | 1.316230 | 1.332387 | 0 |
| 12 | 3 OFF | 1.247253 | 1.265915 | 1.294229 | 0 |

两布局全部8窗GC0B、容量/快照不变、各布局结果聚合四窗相同。无校准过滤/改warm/JIT准备/Profiler/重测。
- spacing120：OFF mean自身差0.154628ms（相对两OFF均值12.508484%），ON自身差0.003794ms（0.316043%）。ON两mean都高于较快OFF，所以mean separation FAIL；tail gate PASS。合并均值看似-2.895625%，但被OFF自身漂移影响，**不能据此宣布收益**。
- spacing12：OFF自身差0.009619ms（0.768232%），ON差0.007728ms（0.597754%）。ON两mean都高于两个OFF；最大ON P95/P99也高于最大OFF，两门FAIL。合并ON均值+3.258498%，只是本组局部观测，不宣称统计因果退化。
- 事前严格判据未更改：两个ONmean均小于两个OFFmean且maxON P95/P99<=maxOFF。两个布局均未满足；不重新测量、不微调另一版、不接生产。
- 不外推原NearestSearch约1.54ms、输入约12ms或整tick节省。原细分诊断父子计时不能相加，也不能用本局部纯算术窗代替完整Driver。

## 命令、保护及缺失验收

实际工具：get_editor_state；manage_scene(action=get_loaded_scenes)；refresh_unity(scope=scripts,compile=request,wait_for_ready=false)；run_tests(mode=EditMode,testNames指定本文件/具名方法)；get_test_job(include_details=true,include_failed_tests=true)。具体job/参数/原响应在同目录JSON。
Shell：Tools/Validate-ChangeLedger.ps1 -RepositoryRoot I:/GitHub/Unity_GAS/gameplay-ability-system-for-unity；定向git diff --check；Python只读报告重算、SHA/原文本检查。成本运行期间未并发运行shell负载、validator或哈希审计。
Audit-final：15,547保护文件无漂移；10份真实dirty before备份全部SHA通过；两源码和两个程序集成本前后无变化；所有原kernel文本未动。原Editor回idle/非Play/非测试/Menu clean8，Scene/资源/Settings保护成立；没有Battle runtime创建，未伪称新十一阶段实景验收。
Operation同名，准确两脚本及文档写域；所有旧dirty/未跟踪、旧Task/失败保留，无删除/移动/clean/reset/checkout/stash/commit/push。原报告与操作中间版本均可追溯。最终validator/diff结果在下方补记。

**缺失且未授权执行**：生产调用接线、逐tick输入/双RNG/完整checksum、完整Driver GC与成本/关闭、正式120warm+1800、Windows H07正常33ms/drop0/no sustained backlog、Android最终认证。本版无收益，不提出自动生产推广或后继实施。
旧17字段兼容候选OFF与Driver tick130/162两event失败、producer USER_DEFERRED/OFF、95OFF、H06不续/冻结专项/旧Goal取消均保持。本有限Task已结束；后续由讨论会话独立审查、人类决定，不自动发送消息或恢复队列。

Final governance validation: Validate-ChangeLedger PASSED exit0; 4371 historical warnings retained. Scoped git diff --check exit0, CRLF warnings only. No further C# or performance executions after the one local group.
