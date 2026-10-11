# H08 恢复验证与唯一四组内存比较

日期：2026-10-10；恢复点 resume-01。同 Task NTSD-H08-NATIVE-SHEET-SHARING-20261010 / Change NTSD-H08-NATIVE-SHEET-SHARING-001。

## 结论

**SCOPED_MEMORY_GAIN_VERIFIED / DEFAULT_OFF。** 原来阻塞的自包含缺图、真实 Unity 销毁回调和新测试资源清理已经通过；原唯一四组 source-worker 内存比较执行一次、四组均通过。候选保留默认OFF，本任务没有生产默认推广权限。

这只覆盖已声明的候选正确性与四组源图片资源收益。不是整 publication、整330定义、整局驻留/峰值、Windows H07阶段或Android证书。H08父项完整预烘焙/预算/设备门保持OPEN。

## 授权及准确改动

主讨论会话在独立审查前轮阻塞后，人类直接回复“那就接着处理”（turn01a125ea-326e-7eb1-a81a-92648af426f8），本会话已read_thread核验。按准确恢复指令，仅编辑以下两个既有Editor文件，所有改前dirty字节先备份至同Operation/resume-01。

- NTSD28B11AtomicPublicationEditorTests.cs：Candidate与缺图恢复改用已有EncodePng生成同一固定8×4像素PNG，不再读取其它测试Temp遗留；保留原缺图发布其它视图、body缺席、出现后旧candidate失效、重新Capture恢复全部断言。
- 同文件SetUp/Load/TearDown：冷记录准确operation/published/retired资源实例ID、类型、名称、generation与source root；在每次加载进度与完成/异常finally观察原owner集合。清理先解绑本fixture UI，再调用原owner入口两次，逐对象验证null，并检查staging/retired/binding0与borrowed资源存活；最后恢复原GameConfig及三单例。
- 同文件新增OFF/ON loaded-publication真实销毁测试。只给测试自己新建的原manager实例设置runInEditMode=true并激活；没有修改生产类或添加ExecuteAlways。DestroyImmediate之后、任何显式清理之前观察原OnDestroy的副作用和资源状态。
- H08ResidentVisualMemoryDiagnostics.cs：Stage新增加载前native ID基线、不与旧对象交集、精确Texture/Sprite身份和finally释放证据；内存方法读取原selected-memory-inputs固定路径/hash/owner，不再动态选组，保持同一四组与原指标、执行顺序和CreateNew单次门。

生产CharacterAnimtorManager.cs完全冻结：SHA256 `ce7acf52f9f7ed1fa37f2f4bb5bacfee674a974d7ba7bef02191418dea2c5a09`，没有因夹具问题修改生产生命周期或共享策略。原代码→候选的逻辑仍见[原报告](../REPORT.md)历史正文及[独立实施说明](../../../../docs/ai/OPTIMIZATION-REVIEW-20261009/IMPLEMENTATION-RECORDS/H08-NATIVE-SHEET-SHARING.md)。

## 为什么真实销毁入口现在有效

Unity2022.3的实例级[runInEditMode API](https://docs.unity3d.com/2022.3/Documentation/ScriptReference/MonoBehaviour-runInEditMode.html)允许该实例在EditMode接收回调。原fixture只激活后关闭对象，既不在Play也未启用此实例能力；前轮空OFF/ON未触达的失败仍保留。

本轮使用原生产manager，无替代runtime组件、SendMessage或反射伪造自动回调。两个测试都已实际加载TextureArray publication，再由Unity DestroyImmediate触发OnDestroy；在任何显式补救cleanup之前，以下全部为实际观察：

| 路径 | 拥有对象 | Sprite | Texture2D | Texture2DArray | 自动回调后 | 借用资源 |
|---|---:|---:|---:|---:|---|---|
| OFF | 40 | 30 | 9 | 1 | disposed=true、flag=false、40/40已销毁 | 存活 |
| ON | 39 | 30 | 8 | 1 | disposed=true、flag=false、39/39已销毁 | 存活 |

随后重复原cleanup仍幂等。atlas资源数量保持1→1。记录在unity-destroy-False-*.json、unity-destroy-True-*.json及measurement-summary.json。这证明当前原Editor受控实例的Unity回调和带publication释放；没有执行Play模式，更不等同于实际游戏跨Scene退出/重进或设备生命周期认证。由于EditMode入口已满足该具名验证，无需动场景或进入获准的后备Play路线。

## 原残留与本次隔离

前轮最后publication资源缺少可证明的完整实例/owner索引，旧native残留继续标UNKNOWN，没有按名称删除、全局Destroy、UnloadUnusedAssets或重启Editor。

新fixture和每次Stage在创建前枚举只读native Texture/Sprite实例ID基线；本次原owner集合中每个资源都断言不属于该基线，不复用旧对象。释放只针对新fixture准确owner，不以Scene根数判断。八份owner-cleanup记录均released=true，全部已观察owned资源null，原manager/data/ui单例和GameConfig恢复；stage八份记录均isolatedFromPreexisting=true、cleaned=true。基线对象不参与Profiler求和。

因此可以在可追溯的新owner隔离组中测对象差量；这不把旧未知写成全Editor零残留。所有新增状态都仅存在于Editor fixture/Stage冷调用，容量由小fixture实际资源集合或本组引用数界定，finally结束；没有新增生产cache/manager/queue。原phase1取消与workerJoin后phase5 staging释放合同不变。

## 实际执行

原项目Unity2022.3.62f3，PID39108，原127.0.0.1:6402 bridge；单一Editor，原Menu clean，未切换或保存Scene。

| 执行 | Job ID | 结果 |
|---|---|---|
| 修正后受影响focused | e2ca7cffa36b4db6b3a25c075af0eea9 | 8/8 PASS，8.6159649s |
| 原唯一四组source-worker内存 | 004dccf3100f496aa5b453d67f6e70c7 | 1/1 PASS，37.3170493s，内部四组全部通过 |

focused八项为缺图OFF/ON、自动销毁OFF/ON、失败保留旧publication、取消重入、UI/lease/borrowed、Driver有序关闭。精确参数在focused-command.json，最终XML在focused-tests.xml；内存精确参数在memory-command.json，最终XML在memory-tests.xml。测试总耗时不是CPU优化收益。

原Editor refresh_unity(scope=scripts,compile=request)后完成程序集重载；新方法实际被运行。调用run_tests(mode=EditMode,testNames=具名数组)，get_test_job(include_details=true,include_failed_tests=true)取最终原件。没有运行其它全量回归、Driver/1800、GC调用栈或Android工具。

## 四组同口径实测

以下是每组被准确追踪的源Texture对象 `Profiler.GetRuntimeMemorySizeLong` 合计，单位B；每组OFF全部清理后ON，ON全部清理后下一组。原固定顺序为ten→spi2→hir→唯一负控。每条源路径/hash/owner与冻结选择清单核对，当前VisualFingerprint与原inventory一致。

| 源图片 | 纹理数 OFF→ON | Sprite数（不变） | OFF对象内存 B | ON对象内存 B | 减少 B | 减少比例 |
|---|---:|---:|---:|---:|---:|---:|
| c/ten/ten.png | 5→1 | 650 | 33,282,640 | 6,656,528 | 26,626,112 | 80% |
| c/kid/a/spi2.png | 2→1 | 8 | 615,456 | 307,728 | 307,728 | 50% |
| c/hir/hir.png | 2→1 | 260 | 53,249,056 | 26,624,528 | 26,624,528 | 50% |
| w/b.png 唯一路径 | 1→1 | 6 | 5,328 | 5,328 | 0 | 0% |

sourcePixels记录数同样为5→1、2→1、2→1、1→1。其处理后RGBA32 payload是另一口径：

| 源图片 | OFF payload B | ON payload B |
|---|---:|---:|
| ten | 16,640,000 | 3,328,000 |
| spi2 | 307,200 | 153,600 |
| hir | 26,624,000 | 13,312,000 |
| unique | 2,400 | 2,400 |

不把两张表相加，也不把它们外推为全游戏节省。每组比较了完整processed pixels、Sprite rect/pivot/size/bounds/vertices/uv和catalog逻辑key；引用数量、SHA及所有者符合原冻结选择，几何/载荷一致。源worker不生成完整atlas；atlas正确性/数量/释放来自独立真实coordinator测试，不能把本表冒称完整加载链内存。

唯一marker为父目录memory.started，原结果父目录bounded-memory.json，八次Stage明细在本目录stage-*.json；没有第二组成本实验或重复窗口。原176.232MiB全56组重复payload仍是清单估算，未全量实际加载测节省。冷加载允许必要分配，本任务没有加载0GC门或新热路径0GC结论。

## 证据复用与纠错

原第一阶段8项中的分组边界、56组清单、first ownership、Source2D/OrderedPages像素等未受生产修改影响，不机械重跑；此前TextureArray与Source2D像素子断言也保留。此次重跑的是修正夹具和finally实际影响的缺图/cancel/失败/lease/关闭，并新增真实自动回调的OFF/ON加载后对照。旧补充1/3与空回调诊断保持历史FAIL/诊断范围，不改写成PASS。

内存启动前的Python身份归档辅助脚本因Windows路径字典键斜杠不一致抛KeyError，未写出before文件；其后已启动唯一内存job。修正的是外部归档助手，未改C#或重跑测量。memory-start-identities.json如实标注为dispatch之后采集；5份source/assembly LastWrite均早于内存dispatch，结束时SHA全部相同。生产SHA同时与前轮冻结值相同。这不是事前成功归档，不能伪写时间；没有影响固定组、实际候选或资源计量口径。

## 最终范围与下一步

本轮成功仅允许 **SCOPED_MEMORY_GAIN_VERIFIED / DEFAULT_OFF**。可供讨论会话审查是否另行授权正常默认接线；本Task没有执行该推广，也不自动开展预印/AB/新bank/压缩/整库去Sprite或H10。

H08父项OPEN；M03两个候选OFF，H07输入与producer暂缓，H06不续/95OFF及既有Role与FindEffect限定成果保持。没有恢复旧Goal或34项队列。旧native残留UNKNOWN及未测整局/峰值/Play/Android范围必须随后续材料保留。

治理、差异、保护/备份和最终Editor校验结果在本报告末尾记录。
