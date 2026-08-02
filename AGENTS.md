# Agent Guide (Unity / NTSD)

## 1. 项目定位

本仓库是基于 EX Gameplay Ability System 的 Unity NTSD 战斗运行时复刻项目。

- Unity 版本：`2022.3.4f1c1`
- Unity 实现目录：`Assets/NTSD/Scripts/`
- 当前工作范围：战斗场景与战斗 runtime
- 唯一战斗逻辑权威：`J:\QQFile\NTSD2.4\ntsd_release_C#`
- Unity 是实现目标；权威工程用于判定规则、顺序、字段和可观察行为

本文件中的规则适用于仓库根目录及其全部子目录；若更深目录存在自己的 `AGENTS.md`，则更深目录可补充局部约束，但不得改变本文件规定的唯一战斗逻辑权威。

## 2. 唯一权威与优先级

处理战斗逻辑时，按以下优先级判断：

1. 用户在当前任务中的明确要求。
2. `J:\QQFile\NTSD2.4\ntsd_release_C#` 中可定位的正式 C# 行为。
3. Unity 当前实现与测试，只用于确认现状和验证移植结果，不能反过来定义权威行为。
4. 项目文档和历史记录，只用于任务跟踪；与权威 C# 源码冲突时必须更新文档，不能修改权威结论。

除上述 C# 目录之外的旧实现、历史资料和旧对齐结论，都不能作为当前战斗逻辑依据。用户没有明确要求历史比较时，不要读取、引用或据此实现。不要因为 Unity 现有行为更方便而偏离 C#；也不要把旧项目中的名称机械替换成并不存在的 C# 类型、字段或方法。

无法在权威 C# 工程中确认的行为必须标为“待确认”，不得凭经验补写成正式战斗规则。若 Unity 框架限制导致实现方式不能逐行对应，允许采用 Unity 适配，但逻辑时序、状态变化和最终可观察结果必须与 C# 一致。

### 2.1 历史定向例外（严格限于用户指定事项）

`J:\QQFile\NTSD2.4\ntsd_release_C#` 仍是唯一的一般战斗逻辑权威。不得将 C++、反汇编、旧实现、历史日志或表现结果提升为一般权威，或据此扩展其他规则。

仅在用户明确指定并且文档记录了范围时，允许把以下历史材料作为单一问题的定向对照：

- Naruto 防下攻的已指定历史定向例外；
- 跳跃水平动量（frame 211 -> 212）的已指定历史定向例外。

这些例外只用于其已记录的行为核验，不改变 C# 的一般优先级，也不授权对相邻技能、输入、物理或 pass 顺序作类推。

## 3. C# 权威入口

开始对齐前，先从与问题最接近的入口追踪实际调用链，不要根据方法名猜测语义。

| 领域 | 权威入口 |
|------|----------|
| 战斗主循环与 pass 顺序 | `J:\QQFile\NTSD2.4\ntsd_release_C#\src\BattleCore\Simulation\GameTick.cs` |
| 碰撞后的命中结算 | `J:\QQFile\NTSD2.4\ntsd_release_C#\src\BattleCore\Interaction\HitResolve.cs` |
| 帧推进与帧内规则 | `J:\QQFile\NTSD2.4\ntsd_release_C#\src\BattleCore\Frame\FrameTick.cs` |
| 输入、组合键与 AI 输入链 | `J:\QQFile\NTSD2.4\ntsd_release_C#\src\BattleCore\Input\InputRuntime.cs` |

这些文件是定位入口，不代表只需要查看这四个文件。实现具体行为时，应继续追踪它们调用的数据模型、resolver、碰撞、对象生成、状态统计和实体生命周期代码，直到字段读写与调用顺序完整闭合。

### 对齐工作顺序

1. 在权威 C# 中定位入口、调用者、被调用者和字段定义。
2. 记录完整前置条件、分支顺序、常量、状态写入、统计副作用和对象生命周期副作用。
3. 在 Unity 中定位对应 pass、实体类型、runtime 字段与表现层接口。
4. 先补齐数据契约，再整体移植行为；不得只搬局部扣血、位移或生成片段。
5. 验证编译、自动自检和目标场景行为。
6. 只在获得对应证据后更新对齐文档状态。

## 4. 战斗范围

### 属于当前范围

- 战斗主 tick 与各 pass 的先后顺序
- 玩家输入、按键边沿、组合键、输入缓冲和 AI 输入
- 角色、武器、飞行物、特殊攻击、其他对象和影子实体的战斗生命周期
- 帧推进、状态事件、移动、速度、落地、边界和朝向
- bdy/itr/cpoint、抓取、持有、投掷、碰撞候选与命中判定
- 伤害、硬直、击飞、倒地、死亡、复活、PP/HP 与战斗统计
- opoint、武器生成、分身、火花、烟雾和与战斗时序相关的对象池行为
- stage 规则中会改变战斗模拟结果的部分
- 为还原战斗可观察行为所必需的渲染层级、挂点、位置同步和阴影同步

### 默认不属于当前范围

- 主菜单、角色选择、加载流程、结算页和普通 HUD
- 与战斗模拟无关的编辑器预览或工具界面
- 音频、美术和通用渲染重构，除非它们直接阻断目标战斗行为的验证
- 完整联机、回滚和大规模移动端渲染改造

若范围外模块直接影响战斗输入或模拟，可以做最小必要修复，但必须说明边界，不能借机扩大任务。

## 5. Unity 实现边界

以下部分保持 Unity-native：

- `MonoBehaviour` 生命周期接入
- `SpriteRenderer`、材质、排序层和相机等表现层
- GameObject 与组件组织
- 对象池及资源异步加载
- Inspector 配置与编辑器测试入口

Unity 适配层不得改变权威 C# 的战斗结果：

- `Transform`、Animator、Unity Physics 和渲染帧状态都不能成为逻辑真相。
- 逻辑实体位置、速度、朝向、帧号、HP/PP、link/holder/target 等必须由战斗 runtime 维护。
- 表现层只读取逻辑快照并刷新显示，不得把插值、镜头位移或渲染排序反写到战斗状态。
- 战斗逻辑不得通过移动全局场景或镜头来伪造实体移动；相机与背景表现不得导致其他实体或阴影跟随玩家产生逻辑位移。
- `.instance` 已具备按需创建能力的管理器或服务，不应仅为测试而预先固化到场景中。
- 对象池复用必须完整重置战斗字段、显示状态、父子关系和排序信息。

## 6. 主循环与固定逻辑帧

战斗 pass 顺序只能以权威 C# 的 `GameTick.cs` 为准。旧文档中声称“主循环已经完全对齐”的结论不能代替重新核验。

当前底层原则：

- `SimulationTickDriver` 是 Unity 侧逻辑帧入口。
- 固定逻辑频率为 30 Hz，即 `SimulationConstants.SIM_DT = 1f / 30f`。
- Unity 的 `Update`、`LateUpdate` 和 `FixedUpdate` 只是外层引擎回调，不定义战斗规则。
- 本地自由运行可由 `Time.unscaledDeltaTime` 累积驱动，但单个逻辑 tick 内不得使用 `Time.deltaTime` 或 `Time.fixedDeltaTime` 决定规则结果。
- `FixedUpdate()` 不直接推进战斗逻辑。
- `LateUpdate()` 只做表现刷新或插值，不写回逻辑真相。
- `SparkRenderFrame` 等战斗表现计数若参与规则，必须跟随逻辑 tick，而不是渲染帧。
- 卡顿时保持固定步长，通过最大追帧数和积压上限处理过载，不改变单 tick 的 dt。

处理烟雾、武器、分身、opoint、hit spark 或命中时序问题时，优先检查 `GameTick.cs` 对应 pass 在 Unity 的映射与生成可见边界。处理输入响应、回放或联机预留时，再检查 `SimulationTickDriver` 与输入提供者。

## 7. 输入、回放与联机预留

当前单机实现可以继续使用现有输入缓冲，但不得破坏后续固定帧输入边界：

- 每个逻辑帧的输入必须是离散、可记录和可重放的数据。
- 按下、按住、释放和组合键窗口必须分别映射，不能用渲染帧轮询替代逻辑帧边沿。
- 输入消费顺序必须与权威 C# 一致，不能为了“更灵敏”而跨 pass 提前消费。
- 后续 `FrameInputSet` 应包含该 tick 所有玩家的输入。
- `LocalFreeRun`、`LockstepBuffered` 和 `Manual` 模式应共享同一个逻辑 tick 入口。
- 回放入口最终需要支持 `ResetWorld(seed)`、逐 tick 输入、状态快照与 checksum。
- checksum 至少覆盖实体数量、stable id、oid、frame、位置、速度、HP、team、link、holder 和 target。
- 网络层未来只同步输入与校验数据，不以 Unity Transform 作为主要同步真相。

这些是接口边界，不授权在普通战斗 bug 修复中一次性实现完整联机或回滚。

## 8. T8 与资源部署

T8 的 stage 战斗逻辑和生产接线可以继续验证，但默认 `stage.dat` 资产部署已由用户明确暂缓：

- 不要把默认 `stage.dat` 缺失当作当前战斗逻辑 backlog。
- 不要为了让测试变绿而私自加入或生成默认资产。
- 需要 stage 数据的测试必须明确使用测试夹具或报告资源前置条件。
- 用户恢复该任务前，文档状态应保持“逻辑已实现/已验证到现有资源边界，默认资产部署暂缓”。

## 9. 验收与诚实报告

任何“完成”“已对齐”或“无剩余差异”的结论都必须有新鲜证据。最低验证层级如下：

1. **编译**：Unity 脚本编译为 0 error。
2. **自动自检**：`BattleRuntimeSelfCheck` 能实际运行并通过目标检查。
3. **定向运行时验证**：在真实战斗场景复现对应角色、输入、对象生成、命中或状态序列。
4. **权威对照**：同一场景的可观察结果与 C# 调用链和字段变化一致。

必须区分以下状态：

- “逻辑已写”：代码存在，但尚未成功运行。
- “编译通过”：只证明当前编译链没有错误。
- “self-check 通过”：只证明覆盖到的断言通过。
- “定向运行时通过”：目标场景已按步骤复现并符合预期。
- “已对齐”：权威调用链、自动检查和必要的定向运行时验证均有证据。

隔离编译器的 0 诊断、单个单元测试或静态阅读都不能单独证明战斗行为正确。若 Unity 被编译错误、资源缺失、编辑器连接或场景前置条件阻塞，必须明确报告“未完成运行时验收”，不得把阻塞包装成完成。

对于玩家报告的组合技、持有武器、层级、跟手、奔跑攻击或阴影异常，必须使用报告中的具体角色和按键序列做 Play Mode 验证。只检查对象是否生成不足以证明技能完整正确。

## 10. Build / Test

构建和测试由 Unity 驱动。先设置 Unity Editor 路径：

```powershell
$env:UNITY_EXE = "C:\Program Files\Unity\Hub\Editor\2022.3.4f1c1\Editor\Unity.exe"
```

运行 EditMode tests：

```powershell
& $env:UNITY_EXE -batchmode -nographics -quit `
  -projectPath "$PWD" `
  -runTests -testPlatform EditMode `
  -testResults "$PWD\TestResults-EditMode.xml" `
  -logFile "$PWD\UnityTest-EditMode.log"
```

验证战斗 runtime 时，优先使用仓库已有的 `BattleRuntimeSelfCheck` Editor 入口或请求文件机制，并读取最终结果文件与 Unity Console。若需要 Play Mode 人工或自动输入测试，应记录场景、角色、按键序列、等待 tick 和实际结果。

不得在已有 Unity Editor 占用项目时强行启动第二个会写入同一 `Library` 的实例。能使用现有 Editor 或 UnityMCP 时，先确认连接和编译状态；连接成功本身不等于行为验收完成。

## 11. Coding Conventions (C# / Unity)

- 缩进 4 空格，使用 Allman braces。
- 类型、方法和属性使用 `PascalCase`。
- 局部变量使用 `camelCase`。
- 私有字段使用 `camelCase` 或 `_camelCase`，跟随相邻代码风格。
- `using` 顺序：`System.*`、其他 .NET、`UnityEngine`/`UnityEditor`、项目命名空间。
- Inspector 字段优先使用 `[SerializeField] private`。
- 异步流程优先沿用 UniTask。
- 避免每帧分配；使用现有池、缓存和复用容器。
- 结构化 DAT/配置数据使用现有 parser 与数据模型，不做脆弱的字符串拼接解析。
- 只在复杂时序或不明显契约处写简短注释，不给自解释代码增加旁白。
- 新增字段前先在权威 C# 中确认语义、默认值、重置时机和所有读写方。
- 修复共享战斗行为时添加或更新聚焦的 self-check；高风险跨 pass 改动需要更广验证。

## 12. NTSD 模块结构

| 路径 | 用途 |
|------|------|
| `Assets/NTSD/Scripts/Animation/LF2Objects/` | 角色、武器、特殊攻击及其他战斗对象 runtime |
| `Assets/NTSD/Scripts/Animation/Character/` | 角色专项逻辑、命中计数和 itr rest |
| `Assets/NTSD/Scripts/Animation/LF2Tasks/` | 对象操作任务基础设施 |
| `Assets/NTSD/Scripts/Animation/Manager/` | 角色动画与资源管理 |
| `Assets/NTSD/Scripts/Animation/` | 动画数据、parser、loader 和 animator |
| `Assets/NTSD/Scripts/DatParser/` | NTSD DAT 解析 |
| `Assets/NTSD/Scripts/Input/` | 组合键、按键事件池和输入基础设施 |
| `Assets/NTSD/Scripts/Simulation/` | 确定性 tick、世界状态、输入缓冲和模拟上下文 |
| `Assets/NTSD/Scripts/Define/` | 公共枚举和常量 |
| `Assets/NTSD/Scripts/NTSD_Extensions/` | NTSD 专用 GAS 扩展 |
| `Assets/NTSD/Scripts/App/` | 应用和战斗启动流程 |
| `Assets/NTSD/Scripts/Load/` | 资源加载与全局 tick 接入 |
| `Assets/NTSD/Scripts/Test/` | 战斗 self-check、测试 bootstrap 与测试夹具 |
| `Assets/NTSD/Scripts/UI/` | UI 控制器；仅直接影响战斗模拟时进入当前范围 |
| `Assets/NTSD/Scripts/Tools/` | 引用池、日志、单例等工具 |
| `Assets/NTSD/Scripts/TimeWheel/` | 定时调度 |
| `Assets/NTSD/Scripts/LevelEditor/` | Editor-only 关卡边界工具 |

主要 partial class 约定：

- `LF2Character.cs`：核心类定义。
- `LF2Character.Generic.partial.cs`：通用行为。
- `LF2Character.States.partial.cs`：状态机逻辑。
- `LF2Character.Hit.partial.cs`：命中与战斗逻辑。
- 其他 partial 文件按职责扩展；修改前先搜索全部同名 partial，避免重复字段或遗漏调用链。

### 禁止直接修改

- `Assets/NTSD/Scripts/Gen/`：自动生成代码。
- `Assets/Plugins/`：第三方包。

除非用户明确要求更新生成器或第三方依赖，否则不要编辑这些目录。

## 13. 文档与进度维护

主要战斗对齐记录：

- `Assets/NTSD/Docs/csharp-vs-unity-battle-alignment.md`
- `Assets/NTSD/Docs/HANDOFF-codex-battle-alignment.md`

记录差异时至少写明：

- 权威 C# 文件、类型和方法。
- Unity 对应文件、类型和方法。
- 前置条件与可复现输入。
- 预期结果、实际结果和差异。
- 数据契约或 pass 顺序依赖。
- 当前状态与验证证据。

不要在 `AGENTS.md` 维护逐次实现流水账。长期有效的规则留在这里；具体任务状态、差异清单和测试结果写入上述对齐文档。发现旧文档与本文件的唯一权威规则冲突时，应在当前任务范围内更正该文档，不能继续传播旧结论。

## 14. Future Mobile Rendering Note

大型移动端渲染重构不属于普通战斗逻辑修复。用户明确要求继续该计划时，再读取项目记忆中的 `Unity NTSD future mobile rendering overhaul plan`。

计划边界：

1. 先减少 GPU 上传尖峰：每张 BMP sheet 只加载一个 `Texture2D`，多个帧 sprite 共享该 texture。
2. 再将逐帧 `SpriteRenderer` 切换替换为基于 sheet/source-rect 的 quad 表现方案。
3. 最后评估统一的战斗 render command/batch renderer，覆盖角色、武器、效果、阴影和火花。

渲染重构只能改变表现与资源效率，不能改变战斗逻辑 tick、碰撞、输入、对象生成顺序或 runtime 真值。

## 15. 工作树与提交安全

仓库可能包含用户尚未提交的修改：

- 开始前查看 `git status`，区分任务内与任务外修改。
- 不回滚、不覆盖、不格式化与当前任务无关的用户改动。
- 若目标文件已有用户修改，先读懂并在其基础上工作。
- 不使用 `git reset --hard` 或其他破坏性命令。
- 不清理未知未跟踪文件。
- 测试或 Unity 自动生成的变更若不属于任务，不要擅自纳入提交。
- 用户要求提交时，只提交已核对的目标修改；提交前再次检查 diff 与验证结果。

## 16. 完成前检查

交付战斗逻辑任务前逐项确认：

- 权威依据来自 `ntsd_release_C#` 的真实调用链。
- Unity 实现没有把表现状态当作逻辑真相。
- 编译为 0 error。
- 相关 `BattleRuntimeSelfCheck` 已实际运行并通过，或已诚实报告阻塞。
- 用户报告的具体战斗操作已完成定向运行时验证，或明确标记未验证。
- 对齐文档状态与证据一致，T8 默认资产部署仍保持暂缓。
- 没有修改 `Gen/`、`Plugins/` 或其他无关用户文件。
- 最终报告区分“已写”“编译通过”“自检通过”和“运行时已验证”。

只有这些证据与任务风险相匹配时，才能声明对应差异已经对齐；不能把局部静态检查扩大成整个战斗系统已完全一致。
