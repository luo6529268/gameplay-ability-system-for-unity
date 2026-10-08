# 第41批：生成准入已测分配点消除

状态：PLANNED；来源为用户要求继续六项首阶段及阶段完成边界合同第0—8节，不另扩范围。

请求准入更正：green-01在Refresh之前被旧domain消费，IL仍旧构造；作为STALE_COMPILED_ARTIFACT工具失败保留，非候选反例。cold runner补loaded admission IL版本门，green-02及affected-01只有新运行方法无直接managed构造才开始；仍同20case/同受影响matrix、不重跑RED、不覆盖旧green-01。有效20case GREEN只计新编译产物。

验证接入进一步定位：6401为droid既有socket占用，原桥不会对AccessDenied自动换port。核空闲6402后仅修复本项目MCP连接JSON（非OS权限、插件或ProjectSettings），两当前配置backup与Operation追加登记；原Editor自行读取新port启动，继续原矩阵。请求式替代不新增测试或第二实例。

验证接入更正：原Editor重载后MCP TCP listener报Socket权限错误，CLI又无Pipeline；不更改网络/插件/Editor实例。只在本批新测试文件补仿既有ICallbacks的请求入口，固定red-01/green-01/affected-01三次及上述冻结filter，无任意代码/路径执行。请求与XML/editor状态CreateNew只在本批artifact；不删request、覆盖旧输出、进入Play或自动改Scene。必须原Menu单saved/非dirty、无编译/更新/实际TestRunner运行；一次request以SessionState消费，禁止重复启动。此工具替代不增加测试矩阵，不作为性能测量。

## 目标与唯一改动

39批已恢复CPU/GC调用记录：BattleNativeDirectSpawnWriter.IsInitialActionAdmitted 235次/4700B（20B/次）。本批用测试证明原捕获lambda的编译构造，再以等价显式guard和索引扫描去掉该分配来源。仅修改该方法和独立Editor聚焦测试；不碰InitializeBirth、调用者、排序、资源、Scene、cadence、collector、声音或生产专项开关。不声称主要FPS瓶颈/H11相机两事件已修好。

路径：
- Assets/NTSD/Scripts/Simulation/Ecs/Writers/BattleNativeDirectSpawnWriter.cs / IsInitialActionAdmitted
- Assets/NTSD/Scripts/Test/Editor/BattleSpawnAdmissionAllocationEditorTests.cs / 新20具名case与实际编译IL guard
- 新测试.meta为本批创建，旧meta保持。

## 不变量与生命周期

正常action有效范围仍为0≤action<NativeMaxFrameIdExclusive(1000)，不是MaxFrameIdExclusive(857)。999仍须帧表实际存在，非999不扫描。保留null wrapper/data false、无效action短路、999空列表false、null列表/先遇null条目异常和首次命中短路；动态增删可见，不缓存。无新owner、worker、lease、slot或buffer，原有序关闭不变。确定性、RNG和副作用由原布尔行为等价门约束，不重新定义336B44规则。

## 冻结验收

1. 新fixture20case：边界7、空wrapper/data2、999位置3、动态增删1、短路1、null列表1、null条目1、先命中1、-1..1000对旧表达式逐值等价1、编译IL禁止managed构造/委托1、旧表达式IL正控制1。
2. Test-first：原生产期望19 PASS/1 IL RED；不得把编译失败计有效RED。
3. 最小实现后20 GREEN；IL逐opcode解码，不字节搜索；禁止newobj/newarr/box/ldftn/ldvirtftn。该门证明本方法直接分配源消除，不证明完整战斗0GC。
4. 受影响旧聚焦：C25b_State9996_UsesExactSynchronizedCallsAndNativeBirthDefaults、C25b_MissingDefinitionsSkipRandom_FullCapacityConsumesAllTuples、C25b_NewbornSlotVisibility_FollowsDynamicAscendingCursor参数项；RecycledCloneTaskPreservesOrdinaryOpointBirth；PooledTaskClearsNativeBirthFlag。只使用既有纯fixture/只读原件，不运行会写旧comparison文件的整类。
5. 原Editor EditMode；核非Play/非编译/无test job，刷新与同job轮询，不另启Editor、不安装包。不启动Profiler/长窗/全campaign。实际编译、focused、未测运行分别报告。
6. Tools/Validate-ChangeLedger.ps1、git diff--check、六before副本与17保护SHA/HEAD核对。

## 完成与下一步

20case和受影响门通过即可收口此分配点，不追加全角色/历史证明；H07正式千人性能及完整logic0GC、H11完整camera仍未通过，Goal保持active继续范围内下一有据技术任务。旧次数仅复盘；无收益不推广候选，不据批次完成停止阶段。

回滚仅本批准确before字节，在获明确恢复授权/新Operation后执行；不恢复HEAD、不覆盖用户其它修改。备份：docs/ai/FILE-OPERATIONS/NTSD-OPTIMIZATION-BATCH41-SPAWN-ADMISSION-ZERO-GC-20261007/backup。
