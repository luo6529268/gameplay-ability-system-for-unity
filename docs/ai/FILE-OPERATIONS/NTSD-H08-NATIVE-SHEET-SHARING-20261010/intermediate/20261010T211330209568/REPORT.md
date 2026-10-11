# H08 native 源 sheet 共享候选：有限执行报告

日期：2026-10-10。Task：NTSD-H08-NATIVE-SHEET-SHARING-20261010。Change：NTSD-H08-NATIVE-SHEET-SHARING-001。

## 当前结论

**BLOCKED_VALIDATION_HARNESS / FINITE_TASK_STOPPED / DEFAULT_OFF。未取得 SCOPED_MEMORY_GAIN_VERIFIED。**

三脚本候选已写入、原 Editor 已成功编译并实际执行聚焦测试。首组8/8通过；补充组1/3通过、2失败。后续一次空管理器诊断明确了 EditMode 销毁回调未触达问题，但不能替代已加载 publication 的销毁后资源释放门。另一个缺图测试在进入加载器前因旧 Temp 夹具缺失而退出。四组当前内容内存比较**未启动**，无 memory.started、无 bounded-memory.json；没有修改生产默认、重跑候选寻求PASS、降低判据或改用模型数字代替实测。

这不是已证实共享算法错误，也不是候选正确性/生命周期全部通过。按具名任务的失败后停止约定，保留代码和原始失败，交回审查具体测试入口问题；不自动开始其它优化点。

## 原代码到候选代码

唯一生产脚本为 `Assets/NTSD/Scripts/Animation/Manager/CharacterAnimtorManager.cs`。

- 原来 native 每个声明独立 verified decode、像素处理、源 Texture2D 上传与 BattleAtlasSourcePixels；即使同完整路径仍重复。
- 新增实例冷开关 `ReuseNativeBodySheetsForDiagnostics`，初始false。disposed、activeNativePrewarm 或战斗边界不允许时拒绝设置；每个 NativePrewarmOperation 固定快照其值。
- 仅 ON 且已绑定 native candidate 时，把 first-declared ownership 过滤后的声明，按现有 NormalizePath(full path) 的 ordinal 语义和 expectedSHA 分组。不同路径同hash不合并，不跨 publication 缓存。
- 一组只解码/处理/上传一份源图；每声明的 grid、rect、pivot、effectivePic、ownedPics、clamp 和 Sprite 创建仍独立。原 OFF/legacy 私有单sheet入口保留，窄提取 PrepareBodySheetSpritePlan/CreateBodySheetTexture/CreateBodySheetSprites 共同逻辑。提取的 Sprite 循环文本与事前版本一致，证据 extraction-review.json。
- 原4路CPU和单上传 semaphore 保留并在finally平衡。共享解码失败按受影响声明数返回，单声明裁切失败按声明计数并阻止本组物化；原整批原子发布/late-input/generation检查继续决定是否发布。
- 没有添加热路径缓存、全局Task缓存、新资源bank、AB、资源格式、图集规划或整库去Sprite。

减少的是同次 native 加载中重复源图及处理后像素记录。当前真实四组减少量未测；不能把前期清单的184,792,652B（约176.232MiB）重复RGBA32 payload估计写成实测总内存/峰值收益。

## 所有权与关闭位置

新增bool由manager持有、每operation一份快照；分组表与plan数组只存在于本次冷加载，容量由allFileInfos.Count/组内声明数界定。Unity Texture/Sprite 继续登记原operation HashSet；提交后由原publishedOwnedResources与最后catalog lease退休路径持有。

已窄读确认现有关闭链：Stopping阶段1取消 native prewarm；worker Join 与 allocation unseal 后，阶段5 PresentationCleared 回收取消 staging，再清中央submission。提交资源仍走原 lease 退休；没有重排十一阶段。OnDestroy源码首行复位诊断开关并沿用原清理；**本轮 EditMode 自动调用触达/带publication最终释放门未通过，不能声称完整关闭零残留。**

## 实际验证

原项目 Editor PID39108、Unity2022.3.62f3、127.0.0.1:6402既有bridge。未开第二Editor、未进入Play、未切换或保存Scene。通过现有bridge调用 refresh_unity、get_editor_state、manage_scene、run_tests/get_test_job；完整具名测试参数在各 *-command.json。

| 执行 | Job ID | 实际结果与边界 |
|---|---|---|
| RED冷开关 | 58e689ae525a45c19f6cf8505f8135c4 | 预期缺少属性失败；原件保留 |
| 首组GREEN | 72c5bc467bed4a52bfbe99d179bd8c50 | 8/8 PASS，31.4047266s；包含原C#内容清单，时间不是加载/CPU收益 |
| 补充组 | a7c5c276baf440a7ba2e6f2412300cbf | 1 PASS、2 FAIL；XML实际total3，bridge进度total10687为发现总数，不是执行数 |
| 生命周期入口诊断 | 1acc14a73f374cd8bd129180076042cd | 1/1 PASS，0.6475726s；空管理器OFF/ON回调资格诊断，不是候选关闭验收 |
| 四组内存比较 | 无 | NOT_STARTED |
| 真实Play、整tick长窗、Android | 无 | NOT_EXECUTED；本任务不提供这些证书 |

首编译有4项测试脚本缺namespace错误，已仅补 `NTSD.Simulation.Presentation` using 后重载成功，旧错误保留 compile-first-errors.txt；之后首组、补充及资格诊断均在原Editor新程序集实际执行。资格诊断重载中一次bridge连接重置，随后原PID正常idle并执行成功；没有额外实例或修改工具。

首组覆盖：
1. 冷开关默认OFF。
2. 真实group worker PNG不同裁切/clamp、BMP/legacy、不同path同hash负控、cancel、changedhash、无效裁切、空图/损坏图、semaphore与局部finally释放。完整源像素、Sprite几何及catalog逻辑字段对照。
3. 当前实际C#330定义、773有效引用、703路径/56重复组冷清单逐项对账，冻结内容身份相符。
4. first-declared ownership顺序。
5. 实际native coordinator ON多definition共享，Source2D及OrderedPages像素/绑定。
6. 取消generation/staging回收/冷开关拒绝中途切换及重试发布。
7. UI解绑、旧publication最后lease前存活/释放后销毁、borrowed资源保留。
8. 原Driver关闭对未发布共享staging回收。

Source2D、OrderedPages以及补充组中到达的TextureArray实际DrawMesh输出均保存独立quad oracle对比：每种9216pixels全等、1548非背景像素，反序负控396像素不同，unresolved命令不产生segment。TextureArray与Source2D实际输出一致的断言已经执行后才遇到销毁断言失败。**这些是通过的子断言，整个array补充测试仍FAIL。**

## 两项失败及一次定位

1. `NativeCoordinatorOn_ArrayDrawMatchesSourceAndDestroyResets` 在执行当时源码386行失败：DestroyImmediate(manager.gameObject)后，ReuseNativeBodySheetsForDiagnostics仍true；后面的sheet/atlas销毁和第二次清理断言没有执行。
2. `NativeCoordinatorOn_MissingBodyPreservesOriginalPolicy` 在 Candidate 读取 `Temp/NTSD28PngAlpha/fixture.dat` 时 DirectoryNotFoundException。未进入native loader；这是未满足的夹具前置条件，不能裁定missing-body政策正确或错误。未重建/删除旧目录，缺失原因未知。
3. 补充组 `NativeCoordinatorOn_FailedGenerationKeepsOldPublication` 已PASS：对本任务新建PNG作有备份的hash变化后拒绝新publication，旧publication引用继续有效。
4. 一次 `NativeSheetReuse_DestroyCallbackQualificationOnly` 空对象对照：OFF、ON各自DestroyImmediate后旧 `spritePrewarmDisposed` 都仍false；ON开关仍true。对原OnDestroy显式调用两次后，disposed=true、开关false、staging0。说明当前夹具没有实际触达销毁回调；不证明生产Play也未触达，也不证明已加载publication都已释放。

原首组/补充组TearDown只DestroyImmediate。因此不能从Scene clean/8roots推断它们的全部native texture已回收。worker专属finally释放、显式lease退休、Driver取消staging已有各自断言；但**此前coordinator最后一代publication的销毁后零残留仍未知**。没有全局Destroy纹理、UnloadUnusedAssets或重启Editor去掩盖这一缺口。后续测量须先解决可追溯清理/生命周期夹具，不能在本状态直接开始内存对照。

## 复用与未测范围

前期 frozen inventory、真实重复加载证据、source-owner HashSet证据及本轮通过的精确子断言保留。生产代码在首次GREEN后没有再改；之后只加本已声明atomic测试文件中的补充/入口诊断，因此不机械重跑不受影响的8项。旧M03/H07/Role/FindEffect、91/93与95各自证据保持范围，不互相覆盖；没有读取活跃Q06或恢复自动Goal。

四组选择已经冻结（ten/ten.png五引用、kid/a/spi2.png不同pic区间、hir/hir.png大图双引用、w/b.png唯一负控），仅选择清单，不是收益结果。未做整330定义双份加载、总驻留/峰值测量、冷加载0GC或热路径新认证。

## 最小后续依赖与边界

仅建议审查以下同任务验证修复，不改变共享策略、规则或内存判据：
- 建立不依赖其它测试残留文件的missing-body夹具，并保留原missing→出现→旧candidate失效语义。
- 为当前EditMode fixture登记显式owner清理和准确资源身份；同时以能真正触达OnDestroy的受控生命周期入口验原有关闭门，不能将反射调用本身冒称真实销毁验收。
- 确认此前测试资源释放/隔离后，再决定是否恢复唯一四组内存比较；旧失败记录不删，不扩大预热或重复内存组挑PASS。

本轮在此具名停止，候选默认OFF。未自动修复其它模块、修改生命周期合同、开启H10或推进34项。H08父项、Windows H07阶段与Android最终认证继续OPEN；H07输入/producer暂缓、95OFF、M03候选OFF与既有Role/FindEffect限定成果保持。

## 文件与治理审计

三个源码及八个原治理文件修改前都有真实dirty备份；每次后续修改都有intermediate preimage。所有新增测试数据位于唯一Temp目录；旧shared测试XML每次运行前备份。hash失败测试修改的是本Task新建PNG，先保存generated-fixture-before副本及原路径。没有文件删除/搬移/Git回退/暂存/提交/push。

收尾原Editor为idle、非Play、无编译/测试，原NTSD_Menu clean、8roots。这只证明Editor/Scene状态，不补足上述native资源零残留门。15625保护文件初次收尾核对0漂移、11份备份哈希有效；最终validator、scoped diff、HEAD/staged与操作后manifest结果见下方收尾校验。


## 收尾校验（2026-10-10）

- 实际命令：`pwsh -NoProfile -File Tools/Validate-ChangeLedger.ps1 -RepositoryRoot I:\GitHub\Unity_GAS\gameplay-ability-system-for-unity`，exit0，1415 Records / 20 governed当前diff路径；0ERROR、4326现有WARNING，本Change无warning/error。日志change-ledger-final.log；没有为了清理历史警告扩大写域。
- `git diff --check -- <11具名既有文件及Task/Change/实施说明>` exit0；日志scoped-diff-check.log。HEAD及staged与事前完全相同；实施说明7个本地链接可解析。
- 完整15625保护文件和11份真实dirty备份收尾验证、脚本/程序集身份、逐文件操作后SHA见Operation的final-validation.json/after-manifest.json。原Editor状态见editor-final.json、scenes-final.json。
- 通过的先前8项、补充组已完成子断言、入口诊断各自留档；不重跑未受影响的证据。本次未作内存收益声明、生产默认推广、Windows H07阶段或Android认证。
- 候选状态与文件审计状态分开：Change BLOCKED；文件操作审计VERIFIED仅表示具名写入、保护/备份/Git核对完成，不表示候选生命周期通过。
