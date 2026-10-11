# H08 原生源 sheet 共享默认准入

日期：2026-10-10。Task `NTSD-H08-NATIVE-SHEET-SHARING-ADMISSION-20261010`；Change `NTSD-H08-NATIVE-SHEET-SHARING-ADMISSION-001`；Operation 同 Task。

## 结论

**SCOPED_DEFAULT_ADMISSION_VERIFIED：已应用到正常 native 配置加载入口，新 CharacterAnimtorManager 默认共享同一 publication 内的相同源 sheet。显式 cold OFF 对照／回退保留。**

此决定依据人类在讨论会话 01a11fe2-bfaa-72f3-a9ed-c8260f63b193、turn 01a1260a-3126-7e61-9704-992782cea6d4 的直接授权“那你来处理呗，应用到项目中”（已独立 read_thread 核验）。先前候选阶段 DEFAULT_OFF 是历史边界，本具名准入任务替代该边界。未启动旧 Goal、自动队列或 H10。

本次是默认接线验收：原 Editor 受影响测试 2/2 + 9/9 通过，旧像素、分组和唯一四组内存证据复用。H08父项的完整预印、整局驻留／加载峰值预算和设备条件仍 OPEN；不是 Windows H07 阶段或 Android 最终验收。

## 改动和实际生产调用链

原正常链已只读核实：

`LoadingPrewarmController.PrewarmOnceCoreAsync -> CharacterAnimtorManager.PrewarmConfiguredLoganContentAsync -> PrewarmConfiguredLoganContentCoreAsync -> LoadLoganContentWithScopeAsync -> NativePrewarmOperation.ReuseNativeBodySheets -> LoadCharacterSpritesCoreAsync`。

生产脚本与真实 dirty 备份逐字节核对，唯一变化是：

```diff
- private bool reuseNativeBodySheetsForDiagnostics;
+ private bool reuseNativeBodySheetsForDiagnostics = true;
```

实例首次创建为 true；现有操作开始时取得不可变快照。显式冷设置 false 仍选择原逐声明路径；活动加载中或 disposed 后设置均拒绝，OnDestroy 清为 false，下一个新实例重新为 true。没有给已完成 publication 原地切换策略，没有增加新的生产字段、缓存、队列或资源 owner。分组键、像素处理、atlas、generation、ownership、lease、取消和十一阶段关闭代码均未改。legacy非native分支保留。

准确四个脚本：

- CharacterAnimtorManager.cs：上述初始值。
- H08ResidentVisualMemoryDiagnostics.cs：冷默认／显式OFF／销毁拒绝／新实例默认测试。CandidateOutput以下旧内存实验正文完全相同。
- NTSD28B11AtomicPublicationEditorTests.cs：旧对照夹具明确OFF；单独提供不设置开关的生产默认夹具入口；本次证据写入新目录，旧原件保留。
- NTSD28B11SourceCacheCallerEditorTests.cs：真实 public 配置入口三代 owner 集成测试；准确缓存／资源 finally 清理；既有 Candidate 反射参数从2补齐实际3参数。测试计数使用既有 API 的 long 类型。

所有新增集合仅为Editor夹具：三代小fixture的证据、6个body entry与UI像素、精确native owner记录、测试前缓存引用快照。随测试TearDown释放，不接生产。真实资源仍由既有operation/publication持有；phase1取消和workerJoin、phase5 staging清理/最后lease退休、manager原OnDestroy合同不变。

## 实际验证

原项目 Unity 2022.3.62f3，PID39108；已有 TCP bridge 127.0.0.1:6402。测试前、两批之间及结束均检查 idle、非Play、无其它测试；NTSD_Menu始终clean／8根。没有新开Editor、切换/保存Scene或进入Play。

| 验证 | Job ID | 实测结果 |
|---|---|---|
| 新默认／显式回退／真实配置入口 | 36fbfdd75ee946a7ba26a5b01bc619c0 | 2/2 PASS，3.1320953s |
| 取消、旧publication、lease、关闭、实际销毁、配置缓存边界 | 7cce05f779164ca3ba3f02ee7b320894 | 9/9 PASS，8.3033916s |

精确命令见 `commands.json`；最终原件为 `tests-entry-poll1.json`、`tests-lifecycle-poll2.json` 和对应XML。测试时长是执行时间，不是优化收益。

真实入口测试以临时GameConfig选取自包含2定义／共享8×4图片fixture，调用 **public PrewarmConfiguredLoganContentAsync**。没有用ForOwners、fakepublication或setter=true替代正常入口。

| 世代 | 调用前配置 | 实际operation快照观察 | 56/57实际源Texture | 候选缓存 |
|---|---|---:|---|---|
| 第一代 | 新实例默认true，无setter开启 | 6次均true | 同一Texture，Sprite各自独立 | 初次0hit |
| 同实例重入 | 默认true | 已有publication直接复用 | publication与Sprite实例不变 | hit增加，0次新publication事件 |
| 第二代 | 新实例先验证true，再显式cold false | 6次均false | 两个独立Texture | hit=1，但重新生成本owner publication |
| 第三代 | 销毁第二代后，新实例默认true，无setter开启 | 6次均true | 恢复同一Texture | hit=1，未复用旧OFF publication |

三代SourceCacheKey一致；manager/data/ui键与内容身份一致，2定义数据存在。全部6个body entry、UI几何／catalog，以及每代16组共300个处理后源图／atlas裁片／UI像素比对值一致。各代Sprite/Texture ID、根目录和源键已记录 `configured-default-*.json`。

第一、二代均通过实际Unity DestroyImmediate触发原OnDestroy；在任何显式fallback cleanup之前断言disposed、flag=false、staging/retired/binding清空、所有本fixture已观察资源销毁、borrowed shadow存活，并验证disposed不能重新开启。最终第三代由原幂等owner清理。整个集成fixture118条资源记录已释放。

另外9项覆盖：ON取消世代释放与retry、输入hash失效保留旧publication、UIlease保留至退休与借用资源保留、OFF/ON已加载TextureArray publication实际Unity销毁、Driver有序关闭回收staging、同源并发单publication双callback、输入捕获取消不晚发布/存缓存、全局owner更换不能晚发布。

本次10份owner-cleanup共502条资源观察记录全部destroyed，三个原单例与GameConfig恢复；4份配置缓存清理均baseline=0／final=0／restored=true。记录数不是全Editor唯一native对象数。未知旧native残留继续UNKNOWN_UNTOUCHED，未执行全局清理。

## 失败与修正原件

首轮编译失败：新测试两处把现有long缓存计数赋给int（CS0266，209/281）。只把新测试字段／局部变量改为long，生产初始值及其它代码未变。见compile-attempt1.txt及Operation中间备份。修正后重新请求脚本编译并实际加载新程序集，全部11项执行通过。

重载期间一次TCP连接被拒绝，等待原Editor恢复后继续，没有启动第二实例。最终Console查询保留MCP Client handler exited连接诊断，因此不声明“Console零异常”；它不是测试失败或C#编译错误。测试原始失败历史未删除。

## 复用与未执行

复用[上一阶段报告](../NTSD-H08-NATIVE-SHEET-SHARING-20261010/resume-01/REPORT.md)、首轮8/8、恢复8/8、56组静态inventory及唯一一次四组source-worker比较。理由：共享算法及其所依赖的解析、裁切、分组和资源代码未变，只有新实例初始选择变化；新实际配置入口、缓存/owner和相关生命周期已单独覆盖。

上一阶段的源Texture对象 `Profiler.GetRuntimeMemorySizeLong`，本次未重跑：

| 固定组 | 源Texture数OFF→ON | 对象内存B OFF→ON | 减少 |
|---|---:|---:|---:|
| c/ten/ten.png | 5→1 | 33,282,640→6,656,528 | 80% |
| c/kid/a/spi2.png | 2→1 | 615,456→307,728 | 50% |
| c/hir/hir.png | 2→1 | 53,249,056→26,624,528 | 50% |
| w/b.png唯一负控 | 1→1 | 5,328→5,328 | 0% |

这是有界源worker对象内存，非完整publication、330定义、整局/加载峰值或新CPU收益。56组176.232MiB仍为估计，不提升为实测。旧memory.started及bounded-memory.json和其它原证据均纳入哈希保护，未清理／覆盖。

未执行完整330加载、Play进出/场景转换、Driver120+180/1800、热路径GC采样、Android/IL2CPP/GPU或设备验收；本改动位于冷实例初始选择，未新增热路径或改变0GC合同，不生成新的0GC证明。

## 审计、回滚与后继

脚本前Task/Change/Operation与14份真实dirty备份已建；每次编辑有中间原字节备份和SHA。实际四脚本diff见Operation/scoped-script-final.patch。15731保护文件无漂移，原备份全有效；4源码/2程序集测试前后身份一致，HEAD及staged未变。最终审计和适用validator见closure-audit-final.json、validator-final.txt；未提交/push/reset/checkout/stash/clean或删除历史。

若将来撤回，本次授权只覆盖将该初始值精确改回原句；先追加Operation与当时字节备份。不得用HEAD覆盖此前dirty共享实现。此轮门已通过，未执行回滚。

下一项只审阅本默认准入报告。H08父项预印、按局加载与预算需独立准确方案；原优先序中的H10并未自动开始。H07输入、producer暂缓／OFF、95OFF、M03未采用和H06既有成果均保留，不恢复旧Goal或34项自动实施。


收尾审计补充：首次closure-audit.json的严格“初始批量快照必须等于逐次首备份”检查失败，仅命中进度表。已逐行核对，初始批量快照采集到本Task自己的PLANNED文首之后；更早的逐次编辑备份完整保存了真正改前字节（SHA256 d28544ecefb151b5bc1f62f72e552263c017b38e820357c1207daf08fcfaf279）。除新增本Task两行文首外无差异、无内容丢失。保留两份原件和首次失败审计；preimage-resolution.json明确首备份恢复来源，最终结论以closure-audit-final.json为准。不是生产/测试失败，未重跑Unity或内存实验。
