> **2026-10-10 当前策略草案 / STRATEGY_REVIEW_PENDING，未批准实施。** 用户指定按总清单原第一项H-01讨论具体策略，并要求额外发现的优化方向一律不跟进。本节是当前提案；下方原方案和历史证据保留，未修改原最终验收门。[范围决定](../../../../docs/ai/OPTIMIZATION-REVIEW-20261009/DECISIONS.md) · [文档Task](../../../../docs/ai/TASKS/NTSD-H01-STRATEGY-AND-SCOPE-20261010.md)。

## 本项要解决什么

让Android安装包在没有开发机项目目录、没有网络的情况下，仍能加载本局完整且正确的战斗内容。它是部署和加载兼容性工作；已有静态代码依据，不以新增CPU热点诊断或帧率收益作为必要性证明。当前没有Android运行结果，不能声称已复现某台设备的具体报错或已带来性能收益。

## 当前代码依据（2026-10-10只读核对）

| 当前入口 | 已确认事实及实施含义 |
|---|---|
| `GameConfig.asset` 的 `BattleContentRuntimeRoot`；`CharacterAnimtorManager.ResolveConfiguredLoganSource` | 从 `Application.dataPath/..` 拼接 `Assets/NTSD/Content/LoganRuntime`。正式Player需要按稳定内容key定位，不能保留开发项目根假设 |
| `BattleContentSource`；`LoganObjectCatalog.Read` | 已有相对key/路径越界保护；catalog、DAT以及目录枚举仍依赖真实文件。复用规范与定义语义，不能只把根字符串改成StreamingAssets |
| `LoganVisualContentCandidate.Capture`；`BMPLoader.LoadVerifiedImageData` | 当前候选捕获、源身份验证和图像解码都有关联文件输入。生成包需要记录来源与转换版本，并保持既有decoder/crop结果 |
| `NTSDResourceLoader.LoadBytesAsync`、音频目录读取；`NTSDSoundPlayer` 文件检查 | 已有FilePath/Resources读取与声音异步加载基础；音频仍存在文件依赖。统一入口需覆盖这些真实调用者，不能只改角色DAT |
| `NTSD28Q07WindowsContentBuildProcessor` | 已有Windows专用copy manifest及源/目标hash校验，Android分支未实现。复用现有清单的来源和校验思路，补足实际内容依赖；脚本常量1373文件/46927691字节只代表既有Windows基线，不是本轮完整闭包统计 |
| `CharacterAnimtorManager` 的已有预热、取消、generation与发布检查 | 继续使用现有owner和发布边界；不另建平行全局内容管理器，不使迟到请求发布到下一局 |

上述均为源码/配置事实，不等于Android验收。原D-023资源权威、项目自有背景/地图/模式及音频来源边界保持；默认stage.dat仍暂缓。

## 建议采用的具体路线（待用户确认）

**版本化资源清单 + 统一内容加载入口 + 随安装包分发的本地AssetBundle。** AssetBundle建议放入专用StreamingAssets子目录，Android通过Unity支持的异步包加载入口访问，业务层只请求内容key。沿用项目现有UniTask；当前manifest未声明Addressables依赖，本提案无需先增加资源框架。本次不创建任何包或资源目录，不改Packages/Settings。

1. **先形成清单与来源映射，暂不切换运行路径。** 以现有Windows清单、catalog和实际可达生产者为起点，列DAT、kind/fusion索引、角色图、公共视觉、声音以及项目地图/模式的key、来源hash、依赖和可选/必需属性。保持catalog顺序与已有可选缺失语义；检查大小写碰撞、重复key和越界。不把所有历史可选图一律改成必需。
2. **复用现有加载owner，接入Provider并先做对照。** 在 `CharacterAnimtorManager`、catalog/candidate、资源loader的实际入口接入 `IBattleRuntimeContentProvider`；Editor源读取保留为对照，正式Player实现从本地包加载。物理路径与逻辑内容身份须明确分离，不随意改写现有checksum/内容版本合同。保留同一取消、generation、发布和关闭顺序。
3. **按原方案在构建期生成等价运行内容，再打包。** DAT按现有parser/转换语义生成运行定义；视觉接入H-08已计划的预烘焙产物；声音和地图保持当前内容与表现语义。构建前验证源hash、生成器版本、产物hash、依赖闭包、目标平台和包存在性，缺失/过期拒绝。共享依赖归入公共包；具体分包与数量由依赖清单决定，不先发明细碎包预算。原源文件保留，正式Player切换不等于删除源文件。
4. **同内容对照通过后切换Player，最后做Android有限验收。** 先核定义/帧数/顺序/依赖、必要像素与播放资源对照和session关闭，再按原文执行清数据离线冷启动、缺失/坏hash受控夹具、取消重进与三次进出。对照仅复用未受改动影响的证据；不机械重跑全部H06/H07性能实验。

Unity 2022.3依据：[StreamingAssets](https://docs.unity3d.com/2022.3/Documentation/Manual/StreamingAssets.html)说明Android路径在APK/JAR内，普通File API不能直接作为通用读取方案；[AssetBundle原生加载](https://docs.unity3d.com/2022.3/Documentation/Manual/AssetBundles-Native.html)提供包与依赖加载API。具体URL组合与加载实现须按现有Unity版本和实际构建布局验证，不能机械添加file前缀。官方API能力不是本项目实机通过证据。

## 既有依赖和阶段完成条件

| 阶段 | 足够的完成证据 | 仍不代表什么 |
|---|---|---|
| 当前策略确认 | 用户确认载体与分阶段路线，准确代码/资源写域和Task/Change另行冻结 | 本轮只交草案，不授权修改或构建 |
| 清单与Provider子阶段 | 来源完整、key唯一、必需/可选正确、失败明确；源与Provider读到的定义/依赖一致，取消/旧session发布/Lease关闭满足原合同 | 不能记H-01父项完成，不能把原DAT/PNG拷贝包当成原方案全部产物 |
| 生成内容与本地包接入 | 原方案所需的运行定义、公共/角色视觉、音频及项目地图依赖齐全；hash/版本/平台检查通过，相关资源语义对照成立 | 不要求H-08/H-10所有内存优化完成；也不等于设备验收 |
| H-01最终验收 | 满足下方原Android离线冷启动、缺失拒绝、完整依赖和Lease生命周期所有条件 | 不关闭H-07性能、H-05 GPU或整个Android最终认证 |

H-08视觉预烘焙是原方案已列出的产物依赖；H-10共享声音key/依赖/所有权合同。先冻结这些共同接口不需要先完成两个父项，但真正生成或修改视觉/音频资源时须取得对应准确范围批准，ATLAS暂缓不因H-01讨论自动解除。最终Android验证还需H-02/H-04已有计划提供可运行构建与配置，不在本次策略阶段启动。

当前待用户确认的是上述“随包本地AssetBundle + 清单/Provider先行”的路线。确认后先制定第一子阶段的准确实施合同；H-08/ATLAS等依赖的资源制作与设备配置到实际接入前单独明确，不默认为已批准。没有新增优化点、没有派发执行会话任务，历史H06诊断不再续办。

---

<!-- OPTIMIZATION-REVIEW-20261009 -->
# H-01 当前评审卡（2026-10-09）

文档角色：本优化点的唯一方案；当前事实与排程以本卡和[进度](../battle-optimization-progress-tracker.md)文首为准。原正文全部保留为详细设计/验收/历史证据。评审建议不覆盖未获批准的强制门，旧“立即继续/Goal active”不授予恢复执行权限。

| 字段 | 本轮整理 |
|---|---|
| 类别 / 建议优先级 | 平台 / 中 |
| 当前状态 | 当前第一项；STRATEGY_REVIEW_PENDING；2026-10-10路线草案待确认，未实施/复测 |
| 依赖的最小范围 | Manifest/Provider接口与资源载体先确认；H-08/H-10共享闭包，不等其父项全验收 |
| 下一步与具体方向 | 已静态核对读取链；确认随包本地AssetBundle＋Manifest/Provider先行路线；实施写域与资源依赖另行冻结 |
| 完成边界 | Android离线冷启动、缺失拒绝和Lease关闭满足原方案；不要求先完成所有内存优化 |
| 方案成熟度 | 以下为文档评审；准确写域、当前代码与受影响验证须在Task中冻结，不能仅凭此卡开始修改 |
| 范围与回滚 | 保留本项原不变量；脚本前Task/Change，保存当前dirty字节。只回滚本项增量，任何覆盖/删除依原授权审计，不从HEAD恢复用户工作 |

[跨项依赖](../../../../docs/ai/OPTIMIZATION-REVIEW-20261009/DEPENDENCIES.md) · [共同执行与证据复用规则](../../../../docs/ai/OPTIMIZATION-REVIEW-20261009/README.md) · [待用户决策](../../../../docs/ai/OPTIMIZATION-REVIEW-20261009/DECISIONS.md)

## 原方案与历史证据（原始正文保留）

# H-01 Android 生产内容部署方案

> 优先级：高  
> 状态：`OPEN / SOLUTION_DOCUMENTED / IMPLEMENTATION_NOT_STARTED / WAITING_USER_APPROVAL`
> 最后更新：2026-10-06
> 本轮共同合同与启动门：[2026-10-06复核](../battle-optimization-rebaseline-and-start-gates-20261006.md)；本轮未运行本项测试/测量，实施待用户批准。
> 主登记表：[Android 移动端就绪度与 1000 AI 风险清单](../android-mobile-readiness-priority-risk-register.md)

## 问题与边界

当前 Logan 生产内容通过项目根下 `Assets/NTSD/Content/LoganRuntime` 和普通 `File.*` API 读取 catalog、DAT、PNG/BMP 与公共资源。Android Player 不具备编辑器项目目录语义，APK 构建成功不代表内容可加载。本项只解决部署/定位。D-023 的非排除 DAT/角色图片权威已确定，不能再等待整体内容权威选择；项目背景、地图、模式及既有例外保留。音频遵循独立已批准任务来源；本轮不覆盖任何资源。

## 解决方案

1. 建立版本化 `BattleContentManifest`，为战斗数据、视觉包、公共 SPARK/Shadow、声音和地图分配稳定资源 key，禁止正式 Player 保存开发机路径。
2. 建立 `IBattleRuntimeContentProvider`：Editor 实现读取生成资产，Android 实现从正式 Unity 资源包异步加载；业务层不直接依赖 Addressables、AssetBundle 或文件路径。
3. 构建期把当前正式 DAT 转为语义等价的运行时目录，把正式 PNG/BMP 等输入转为 H-08 预烘焙纹理包；源格式无关，decoder/crop/公共资源及项目模式值不得漏失。
4. Manifest 保存源内容哈希、烘焙器版本、产物哈希、依赖和包大小；过期或缺失时构建 fail-close。
5. 本局加载按 ATLAS 支柱一的保守生产者全集计算闭包，覆盖 OPoint、分身/变身、Stage spawn、掉落/碎片、公共视觉与项目KO等实际生产者；音频与H-10共用版本化依赖目录。公共包去重，显式Lease管理生命周期。

## 实施步骤

1. 只读盘点所有生产内容入口和运行时路径调用，冻结资源 key 规范。
2. 建立 Manifest schema、哈希和构建前完整性验证，不先改变运行路径。
3. 建立 Editor/Packaged Provider seam，并以同一目录结果做 shadow compare。
4. 接入预烘焙数据、纹理、声音和地图包；补取消、迟到请求和 session generation 检查。
5. Android 路径验证通过后，移除正式 Player 对项目目录和源图/DAT 的直接依赖；Editor 工具可保留源读取。涉及资源、包源、Scene或旧路径删除时另列精确清单与用户批准，不由本文自动授权。

## 验收条件

- APK 冷启动不依赖项目根、当前工作目录、外部存储或开发机绝对路径。
- 本局所有直接与递归资源依赖都能由 Manifest 唯一解析，重复依赖只加载一次。
- 缺失、哈希不符或版本不兼容时给出明确 resource key 和失败阶段，不生成空角色。
- 清除应用数据、飞行模式、首次安装和第二次进入战斗均可完成内容加载。
- 战斗退出后非共享 Lease 可回收；迟到异步请求不能污染下一局。

## 测试条件

| 测试 | 条件 | 通过标准 |
|---|---|---|
| Manifest 单元测试 | 缺失、重复、循环依赖、错误哈希 | 确定性 fail-close，诊断含资源 key |
| Editor/Player 对照 | 同 roster、地图和内容版本 | 解析出的定义、帧数与依赖集合一致 |
| Android 冷启动 | 清数据、飞行模式、ARM64 Development APK | 能进入普通战斗，无 `Assets/...` 文件访问失败 |
| 生命周期 | 连续三次进入/退出、快速取消后重进 | 无旧 session 发布、无资源泄漏和残留 Lease |
| 包闭包检查 | 解包 APK/AAB 并读取构建报告 | Manifest 声明的所有正式包均存在且哈希一致 |

## 证据与留痕

- 2026-10-06重扫：`Assets/NTSD/Config/GameConfig/GameConfig.asset:15`；`Assets/NTSD/Scripts/Animation/Manager/CharacterAnimtorManager.cs:52` 从Application.dataPath派生项目根；`Assets/NTSD/Scripts/Animation/LoganObjectCatalog.cs`读取源catalog/DAT。旧9月行号仅为历史。
- 实施时必须记录：内容 Manifest、源/产物 SHA-256、BuildReport、APK SHA-256、加载报告和三轮生命周期结果。
- 2026-09-06：方案文档建立；未修改实现，状态保持 `IMPLEMENTATION_NOT_STARTED`。
