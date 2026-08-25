# NTSD 30 Hz 战斗逻辑与 60/120 Hz 高帧率表现实施计划

> 状态：`PLANNED`（仅方案；尚未开始本计划的代码实施）  
> 创建日期：2026-08-23  
> 适用范围：`Assets/NTSD/Scripts/` 的战斗表现、URP 中央绘制、相机表现与显示帧率策略  
> 战斗规则权威：`J:\QQFile\NTSD2.4\ntsd_release` 的 release live path  
> 上位关联：`central-battle-render-system-plan.md`、`unified-battle-lockstep-ecs-server-architecture-plan.md`、`future-server-lockstep-architecture.md`  
> 不在本计划内：提高战斗逻辑频率、改变 DAT 时序、修改碰撞/AI/输入规则、实现服务器业务、T8 默认 `stage.dat` 部署、Android 真机认证。

---

## 1. 目标与完成定义

### 1.1 目标

在不改变 30 Hz 战斗逻辑的前提下，让 Unity 的 CentralOnly 中央表现链能按设备实际显示频率稳定输出 60 Hz 或 120 Hz 的平滑位置表现。

目标结构如下：

```text
30 Hz C++ release 对齐的 Battle Simulation
    ↓ 每个逻辑 tick 只发布一次不可变表现快照
Previous / Current Presentation Sample
    ↓ 60 / 120 Hz 纯表现采样，绝不反写 runtime
Central Mesh 顶点位置插值
    ↓
URP 世界相机输出
```

这里的“高帧率”仅指**显示与表现采样频率**，不是把 DAT、状态机、碰撞、输入窗口、AI、命中、对象生成或随机数改成 60/120 Hz。

### 1.2 最终效果标准

当本计划完整验收后，必须同时满足以下效果。

| 范畴 | 最终标准 |
|---|---|
| 战斗规则 | 同 seed、同输入、同 tick 下，30 Hz 逻辑 checksum、RNG、slot/generation、命中、伤害、帧号、DAT `wait`、`opoint`、输入消费顺序与高帧率关闭时一致。 |
| 位置表现 | 角色、武器、飞行物、分身和阴影等已连续存在的实体，在 60/120 Hz 设备上不再只按 33.33 ms 的离散位置跳动。 |
| 动画图片 | `effectivePic`、`frameId`、UV、pivot、`FlipX/FlipY` 仍严格遵循 30 Hz DAT 时序；四张源图仍是四张离散姿势，不伪造中间美术帧。 |
| 生命周期 | 出生、销毁、slot 重用、换 holder、抓取、投掷、状态瞬移、恢复、暂停和追帧不会把不同实体或不同关系的旧位置混合到当前实体。 |
| 相机与关联物 | 阴影、持有武器和角色主体不脱节；相机不发生双重平移；场景活动边界与逻辑边界不因高帧率表现产生偏移。 |
| 性能与内存 | 高帧率表现热路径不产生托管 GC 分配；每个显示帧不重建完整 command、Mesh、Sprite 或 DAT 查询；Mesh 构建频率仍以逻辑表现发布频率为上限。 |
| 降级 | CentralOnly 资源/提交故障、未支持的显示模式、手动回放或未完成的 Worker/联机表现时钟必须安全退回“当前帧离散显示”，不得出现鬼影、重复绘制或错误插值。 |

### 1.3 完成口径

不能仅凭“设置了 `Application.targetFrameRate = 120`”或“Inspector 显示 120”宣称完成。完整完成必须有：

1. Unity 编译 0 error；
2. 相关 `BattleRuntimeSelfCheck` 与新增 focused tests 实际通过；
3. 30 Hz 与 60/120 Hz 在同 seed / 同输入下的逻辑 trace 和 checksum 一致；
4. CentralOnly 真实 Play Mode 中验证角色、阴影、掉落武器、持有武器、飞行物和 `opoint` 对象；
5. 真实 60 Hz / 120 Hz 显示设备上分别确认实际显示率，而不是只验证请求值；
6. 预热后的高帧率表现热路径分配为 0 B，并有 CPU / GPU / draw / Mesh build 数据；
7. 所有本计划任务都达到各自的验收状态。

---

## 2. 当前源码事实与实施边界

### 2.1 已存在的正确基础

| 现有模块 | 已确认事实 | 对本计划的意义 |
|---|---|---|
| `SimulationConstants` | `SIM_TICK_RATE = 30`、`SIM_DT = 1/30`。 | 逻辑时间基准已明确，必须保持。 |
| `SimulationTickDriver` | 本地模式的外层 accumulator 已计算 `RenderAlpha = accumulator / SIM_DT`。 | 单机同步模式已有可复用的 alpha 来源。 |
| `SimulationTickHostPolicy` | `OfflineLocalTickPolicy` 一般每个 Unity Update 最多推进一个逻辑 tick；`Manual`/`NetworkLockstep` 不以 Unity wall clock 直接推进。 | 高帧率表现不能借追帧或多 tick 改变逻辑。 |
| `BattlePresentationCoordinator` | 以 `BattlePresentationFrame` 捕获实体、影子、命中记录和 command 数据。 | 可作为纯表现快照源。 |
| `BattleCentralRenderSystem` | 新表现发布时构建中央 Mesh；相同 publication version 的后续 URP 帧复用已有提交。 | 不能在 60/120 Hz 时重复完整 BuildCommands / Build Mesh。 |
| `BattleDynamicMeshBackend` | 顶点目前只包含当前位置、颜色、UV 与 atlas slice。 | 需要加入上一位置或等价平移数据。 |
| 两套 Central Shader | 分别处理普通 `Texture2D` 和 `Texture2DArray`。 | 两套必须统一支持高帧率顶点插值。 |
| `BattleCameraSafeArea` | 已按 Unity 显示帧使用 `Time.unscaledDeltaTime` 平滑相机。 | 需要避免实体插值与相机补偿叠加两次。 |

### 2.2 已确认缺口

当前 `RenderAlpha` 只用于 Inspector 只读诊断；中央 Mesh、`BattleRenderFeature` 和两套 Shader 均未读取它。因而 Unity 即使以 60/120 Hz 渲染，也只是反复绘制同一份 30 Hz 顶点数据。

当前 CentralOnly 表现链为：

```text
30 Hz tick
  -> BattlePresentationFrame
  -> BattleRenderCommand
  -> BattleDynamicMeshBackend 写当前 Position
  -> URP 每显示帧重复 DrawMesh
```

目标链为：

```text
30 Hz tick
  -> 当前表现快照 + 预分配运动历史
  -> 当前顶点 + 上一有效位置
  -> 每显示帧传入 alpha
  -> GPU 顶点 Lerp(previousPosition, currentPosition, alpha)
```

### 2.3 不可突破的硬边界

下列内容不得为了“更顺滑”而改动：

- `SimulationConstants.SIM_TICK_RATE`、`SIM_DT`；
- `NTSDBattleTickSystem.RunReleaseTick` 的权威 pass 顺序；
- `FrameInputSet`、组合键边沿、输入缓冲和输入消费时点；
- DAT `wait`、`next`、`state`、`pic`、`opoint`、hit stop 与对象生命周期；
- 碰撞、伤害、AI、RNG、checksum、slot/generation；
- 用 Unity `Transform`、相机或 Shader 结果反写战斗 runtime；
- 每显示帧重建完整 command、动态创建 `Sprite`、解析 DAT 或上传完整 Mesh；
- 为实现本计划删除 Legacy 路径或修改默认 `stage.dat` 资产部署。

### 2.4 数据与内存规则

- 战斗开始后的高帧率热路径不得使用 LINQ、字符串 key、每帧 `Dictionary`、每帧 `List` 扩容或临时对象分配。
- 运动历史优先使用按 runtime slot + generation + command role 索引的预分配连续数组；不要在热路径用 `Dictionary<RuntimeEntityHandle, ...>`。
- 运动历史必须属于具体 `SimulationWorld` / Central submission 生命周期，不能成为跨战局残留的全局静态状态。
- 若现有 CentralOnly 静态系统必须持有状态，状态也必须带 world identity、generation 和明确 reset 边界。
- 新代码必须独立建类或受清晰所有权的服务；不得仅为了拆文件新增无边界的 `partial`。
- 每一个实际代码任务开始前，按 `AGENTS.md` 建立对应 Change Record、更新 `docs/ai/CHANGE-LEDGER.md` 与 `docs/ai/STATE.md`，并在完成后运行 Change Ledger validator。

---

## 3. 产品策略与配置口径

### 3.1 逻辑频率与显示频率的固定关系

| 模块 | 频率 | 能否被 60/120 配置改变 |
|---|---:|---|
| Battle simulation | 30 Hz 固定 | 否 |
| DAT 帧推进 / `wait` | 跟随 30 Hz tick | 否 |
| 输入采样并提交到逻辑帧 | 跟随固定逻辑边界 | 否 |
| Checksum / replay / RNG | 跟随固定逻辑边界 | 否 |
| 表现命令 / Mesh rebuild | 最多一次 / 已发布逻辑表现帧 | 否，不能升到每显示帧 |
| URP DrawMesh | 设备实际显示帧 | 是 |
| 顶点插值 alpha | 设备实际显示帧 | 是 |
| 相机平滑 | 设备实际显示帧 | 是，但仅表现层 |

### 3.2 建议的显示模式

后续实现时提供以下表现模式；它们都保持逻辑 30 Hz：

| 模式 | 含义 | 认证条件 |
|---|---|---|
| `Off` | 关闭位置插值；维持当前离散 30 Hz 表现。 | 作为 A/B 基线与故障回退。 |
| `Auto` | 请求设备合理的高刷新率；根据平台、屏幕能力、质量档位和热状态选择。 | 必须显示“请求值”和“实际值”。 |
| `60Hz` | 请求最多 60 Hz 表现。 | 真实 60 Hz 输出设备上实际测得约 60 Hz。 |
| `120Hz` | 请求最多 120 Hz 表现。 | 真实 120 Hz 输出设备上实际测得约 120 Hz；60 Hz 屏幕只能验证采样逻辑，不能宣称 120 Hz 视觉验收。 |

### 3.3 默认策略提案

- 桌面默认 `Auto`：优先遵循用户 VSync / 显示器能力，不承诺超过物理刷新率。
- Android 默认请求 60 Hz；只有设备明确支持、性能和温控预算通过时才允许用户或质量档位选择 120 Hz。
- `120Hz` 是“请求目标”，不是无条件保证；实际帧率需由诊断面板和真实设备确认。
- 任何平台都不能用 `targetFrameRate` 覆盖逻辑 30 Hz 或触发多 tick 战斗推进。

---

## 4. 任务总览与依赖

```text
HFR-00 基线与门禁
    ↓
HFR-01 帧率策略与可观测性
    ↓
HFR-02 表现采样时钟与预分配历史
    ↓
HFR-03 实体身份匹配与不连续门控
    ↓
HFR-04 中央 Mesh 的 previous-position 与联合 Bounds
    ↓
HFR-05 两套 Shader / URP alpha 传递
    ↓
HFR-06 阴影、持有物、相机与特殊表现接线
    ↓
HFR-07 CentralOnly 回退与生命周期安全
    ↓
HFR-08 Dedicated Worker / 未来 Lockstep 表现时钟
    ↓
HFR-09 完整回归、性能与平台认证
```

除 HFR-00 的只读基线准备外，后续任务必须按顺序通过前一任务的验收。任何任务失败时，只能修复当前任务或回退该任务；不得借机修改战斗规则来让表现验收变绿。

---

## 5. 分任务实施与验收规范

### HFR-00：冻结当前基线与高帧率功能门

**状态：`NOT_STARTED`**  
**依赖：无**  
**目标：** 在任何高帧率代码写入前，冻结“高帧率关闭”时的当前逻辑与中央表现基线，并准备功能开关。

#### 计划范围

- 为高帧率表现建立明确的配置入口和诊断状态，但默认保持关闭或 `Off`；
- 记录 CentralOnly 的 command 顺序、实体/影子可见性、segment 数、Mesh build 数与现有逻辑 checksum 基线；
- 选择至少六组有代表性的真实表现场景：静止、奔跑、跳跃/落地、掉落武器、持有/投掷、`opoint` 实体；
- 记录当前显示设备、VSync、quality level、`Application.targetFrameRate` 和实际帧率，以区分“设置值”与“真实输出”。

#### 禁止事项

- 不改变 `SIM_TICK_RATE`；
- 不改任何战斗 pass、DAT 或实体逻辑；
- 不把现有 Web cadence 实验的结果当作 Unity Runtime 验收；
- 不因基线不够快而删减 CentralOnly 命令或隐藏实体。

#### 验收条件

1. 高帧率开关关闭时，CentralOnly 输出与修改前基线完全一致；
2. 同 seed / 同输入的逻辑 checksum、RNG、实体数量、slot/generation、frame、HP/PP 和事件序列一致；
3. 基线报告包含每个场景的 command count、segment count、Mesh build count、draw count、GC alloc 与 frame timing；
4. 没有把“请求 120 Hz”误写成“实际 120 Hz”。

#### 测试标准

- Fresh Unity compile：0 error；
- `BattleRuntimeSelfCheck`：通过；
- 新增或复用 focused Editor tests：对同输入 journal 比较 HFR Off 前后 checksum 与 Central command identity/order；
- Play Mode：每个基线场景至少一次真实重现，保存测试结果和场景前置条件；
- Profiler：记录 baseline，不能只截图不记录条件。

#### 本任务完成后的可见效果

无刻意视觉变化。这是防止后续“看起来更顺滑但战斗行为变了”的比较基准。

#### 回退条件

若仅增加配置或诊断就影响 checksum、Central command、资源绑定或显示内容，立即关闭功能门并只保留基线证据，先定位影响源。

---

### HFR-01：统一显示帧率策略与运行时可观测性

**状态：`NOT_STARTED`**  
**依赖：HFR-00**  
**目标：** 让项目有一个 NTSD 自己拥有的显示帧率策略，而不是由 QualitySettings、第三方 `MMFPSUnlock` 或多个脚本互相覆盖。

#### 计划范围

- 在 `Assets/NTSD/Scripts/App/` 建立单一职责的表现帧率策略服务；
- 暴露 `Off / Auto / 60Hz / 120Hz` 请求模式、有效请求值、实际显示帧率、VSync 状态、设备刷新率和拒绝原因；
- 审计当前 quality level 的 `vSyncCount`，明确谁最终拥有 VSync / target frame rate；
- 保证每个战局只有一个 owner 写入帧率相关 Unity API；
- 将模式、实际值、是否已开启位置插值显示为只读诊断信息。

#### 禁止事项

- 不在 `SimulationTickDriver` 中写 `Application.targetFrameRate`；
- 不通过增大 `maxCatchUpTicksPerFrame` 达到“高帧率”；
- 不让第三方性能组件和 NTSD 策略同时争夺 `vSyncCount` / target frame rate；
- 不承诺 60 Hz 屏幕能显示 120 Hz。

#### 验收条件

1. 对任意运行时状态，只存在一个明确的帧率设置 owner；
2. `Off`、`Auto`、`60Hz`、`120Hz` 的请求与实际状态可在诊断中区分；
3. 修改表现帧率模式后，逻辑 tick 仍固定为 30 Hz，普通 `LocalFreeRun` 每个 Unity Update 仍最多自动推进一个 tick；
4. 切换模式不会重置战局、改变输入缓冲或改变 checksum；
5. 不支持 120 Hz 时有明确降级理由，而不是静默谎报。

#### 测试标准

- Unit / Editor：策略输入矩阵（平台能力、VSync、请求模式、quality profile）得到确定的 effective mode；
- Runtime：连续 10 秒记录逻辑 tick 计数，确认约 300 tick 且不存在因显示频率增高而额外的 simulation tick；
- Runtime：连续切换 `Off → 60 → 120 → Auto → Off`，确认战斗 checksum 与持续输入保持不变；
- Play Mode：真实 60 Hz 显示设备验证有效帧率；真实 120 Hz 设备才可验证 120 Hz；
- 诊断：同时记录 requested Hz、display refresh、effective target、measured FPS、VSync。

#### 本任务完成后的可见效果

用户可以明确选择或观察 60/120 表现请求，但位置仍可能保持 30 Hz 阶梯式移动；这是预期的阶段性结果。

#### 回退条件

任何模式切换导致逻辑频率、输入、场景加载、相机或现有 VSync 设置不可预测时，恢复 `Off` 并保留诊断报告，先修正 owner 冲突。

---

### HFR-02：表现采样时钟与预分配运动历史

**状态：`NOT_STARTED`**  
**依赖：HFR-01**  
**目标：** 建立纯表现的 `Previous / Current / Alpha` 采样模型；不依赖可被下一 tick 重写的 `frameA/frameB` 引用。

#### 计划范围

- 将现有 `SimulationTickDriver.RenderAlpha` 作为 `OfflineLocal` 同步路径的 alpha 输入；
- 建立 world-bound、预分配的运动历史，保存上一有效表现位置、handle generation、命令角色、tick、可见性与不连续签名；
- 在逻辑表现快照发布时更新历史，在每个 URP 显示帧采样 alpha；
- 使用“一逻辑 tick 表现延迟”的前一状态到当前状态插值；不做预测外推；
- 为暂停、恢复、world 切换、长积压、跳 tick、中央提交失败建立统一 reset API；
- 在 Dedicated Worker / Lockstep 未具备正式时间戳逻辑前，明确安全降级为 `alpha = 1 / current-only`，不得假装已插值。

#### 禁止事项

- 不保存或读取可变的 `LF2Entity.Runtime` 作为渲染历史真相；
- 不直接长期引用 Coordinator 可复用的发布双缓冲；
- 不在每个显示帧分配字典、列表、闭包、字符串或 boxing 对象；
- 不以速度外推下一逻辑帧位置；
- 不把 alpha 写回 `XInt`、`YInt`、`DisplayZ`、`CameraX` 或 Transform 逻辑状态。

#### 验收条件

1. alpha 对 `0`、`0.5`、`1` 的采样结果分别等于 previous、中点、current；
2. 历史容量在战斗预热/封存前完成，战斗窗口内不扩容；
3. 同一 tick 重复渲染不会改变历史数据；
4. restore / pause / world change / tick gap 后首帧 previous 必须等于 current；
5. 关闭高帧率模式时不读取或影响运动历史结果；
6. 无论 alpha 如何变化，BattleWorld checksum 与状态完全不变。

#### 测试标准

- Unit：纯 sampler 的 `alpha=0/0.25/0.5/0.75/1` 精确数值测试；
- Unit：连续 tick、重复 tick、跳 tick、负时间、NaN/Infinity、暂停/恢复、world swap 的 reset 测试；
- Allocation test：预热后连续 10,000 次采样为 0 B；
- Editor：验证 HFR Off 与 HFR On 的逻辑 checksum 不同路径相同；
- Play Mode：在固定 30 Hz 的逻辑下打印/记录 alpha，确认 60 Hz 约每逻辑 tick 两次采样，120 Hz 约四次采样；实际数量可随真实设备帧时间波动，但比例与范围必须合理。

#### 本任务完成后的可见效果

默认不开启 GPU 插值前不应有画面变化；诊断能显示“是否有 previous/current 连续样本、当前 alpha、被 reset 的原因”。

#### 回退条件

若发现历史跨 world、跨 handle generation 或战斗后仍持有旧资源，禁用该 world 的插值并清空历史，不能继续尝试用旧数据渲染。

---

### HFR-03：实体身份匹配与不连续门控

**状态：`NOT_STARTED`**  
**依赖：HFR-02**  
**目标：** 只为真正连续的同一表现对象插值，拒绝对象池重用、出生、销毁、换 holder 和瞬移造成的错误拖影。

#### 计划范围

- 第一阶段仅将 `BattleRenderCommandType.Entity` 与 `BattleRenderCommandType.Shadow` 列入可插值候选；
- 匹配键至少包含：`RuntimeEntityHandle.Slot`、`RuntimeEntityHandle.Generation`、命令 type、`LocalSequence` 与连续 tick；
- 为 holder/link、可见性、命令角色、world identity、资源/关系切换定义不连续签名；
- 发生以下任一情况时强制 `previousPosition = currentPosition`：出生、销毁后重生、slot generation 变化、实体可见性切换、抓取/持有关系改变、投掷释放、world restore、tick gap、中央提交切换、逻辑暂停恢复；
- `OverlayGlyph` 与 `HitRecord` 第一阶段保持离散；不得为了画面“更顺滑”提前纳入。

#### 禁止事项

- 不只按 runtime slot 匹配；
- 不跨对象种类、跨 `Handle.Generation`、跨命令 type 或跨 world 插值；
- 不改变当前实体排序、`SortOrder`、segment 边界或透明绘制顺序；
- 不把持有武器从地面插值到角色手中，或从手中插值到投掷初始点。

#### 验收条件

1. slot 重用时，新对象首帧直接显示在当前正确位置；
2. `opoint` 新实体首帧无来自旧对象的拖影；
3. 销毁后不残留上一实体的像素；
4. 抓取、持有、投掷、掉落的关系边界均可观测为离散切换；关系稳定后的连续移动才恢复平滑；
5. 影子和本体使用同一 identity / discontinuity 判定结果；
6. `OverlayGlyph`、`HitRecord` 的命令数量、顺序和 DAT 时序保持当前行为。

#### 测试标准

- Unit：`RuntimeEntityHandle` generation 重用、local sequence 变化、类型变化、tick gap 的拒绝矩阵；
- Focused Editor：构造出生/销毁/slot reuse、weapon holder 切换、opoint、hit record 生命周期快照；
- `BattleRuntimeSelfCheck`：补充高帧率身份隔离断言；
- Play Mode：真实角色拾取、投掷、落地武器、Naruto 分身/螺旋丸等 opoint 过程，逐项检查无跨对象拖影；
- Trace：HFR On/Off 下逻辑 entity count、slot/generation、frame、HP/PP 与 checksum 相同。

#### 本任务完成后的可见效果

连续对象具备“可插值资格”，但所有结构性切换都会安全地立即跳到正确状态，不会产生错误运动轨迹。

#### 回退条件

任何不连续条件难以可靠判定时，宁可对该命令类型禁用插值，也不得放宽身份匹配规则。

---

### HFR-04：中央 Mesh 的 previous-position 数据与联合 Bounds

**状态：`NOT_STARTED`**  
**依赖：HFR-03**  
**目标：** 在不提高 Mesh rebuild 频率的前提下，把 previous/current 位置写入中央 Mesh，供 GPU 在显示帧插值。

#### 计划范围

- 扩展 `BattleDynamicMeshBackend.BattleQuadVertex`，添加 previous position 或等价的 position delta 顶点属性；
- 扩展 `VertexAttributeDescriptor`，为该属性分配独立顶点通道；
- 每个逻辑表现发布时，仅为有资格的 `Entity` / `Shadow` 写入 previous/current 顶点；无资格对象写入 `previous == current`；
- 保持当前帧的 sprite size、pivot、UV、atlas slice、颜色、Flip、材质、sort order 不变；
- 将每个 quad、submesh 和 Mesh 的 bounds 扩展为 previous/current 两套 quad 的并集；
- 保持命令解析、资源绑定、segment 分组和 `SetSubMesh` 的当前权威顺序。

#### 禁止事项

- 不在每个 60/120 Hz 显示帧调用 `BuildCommands`、`Build`、`SetVertexBufferData` 或 `SetSubMesh`；
- 不让 previous/current 的 UV 或图片帧在 GPU 中混合；
- 不改变 `QuadsPerChunk`、索引模板或透明 segment 顺序来迁就插值；
- 不因插值而减小 bounds 或假设实体只向某个方向移动。

#### 验收条件

1. 每个可插值 quad 均携带 current 与 previous 位置；不连续 quad 的两者完全相等；
2. current picture、UV、pivot、Flip 和 material 与 HFR Off 的 current command 相同；
3. alpha 任意变化时，当前逻辑 command 数据不被修改；
4. bounds 包含 previous 和 current 所覆盖的完整区域；
5. 60/120 Hz 期间完整 Mesh Build 次数不超过已发布逻辑表现帧次数加上明确的资源失效次数；
6. 资源解析、segment count 和 draw order 在 HFR Off / On 同一逻辑快照中一致。

#### 测试标准

- Unit：同一 command 的 previous/current quad 顶点数学、pivot 与尺寸合同；
- Unit：bounds union 覆盖水平、垂直和斜向移动；
- Unit：不连续条件下四个顶点 previous/current 完全相等；
- Editor：Texture2D 与 Texture2DArray 资源各至少一组命令验证；
- Profiler：120 Hz 显示采样期间验证 `BuildCommands` 和 `SetVertexBufferData` 约为 30 Hz 发布频率，而非 120 Hz；
- Allocation test：预热后本任务新增链路为 0 B / 表现帧。

#### 本任务完成后的可见效果

尚不要求画面已平滑；数据已准备好，且不会因为显示帧增加而把中央 Mesh 构建成本放大到 60/120 次每秒。

#### 回退条件

若顶点布局与现有 Shader/平台不兼容，先保持 current-only vertex layout 并禁用 HFR，不允许带着错位 UV、剔除或 corruption 上线。

---

### HFR-05：两套中央 Shader 与 URP alpha 传递

**状态：`NOT_STARTED`**  
**依赖：HFR-04**  
**目标：** 让普通 `Texture2D` 与 `Texture2DArray` 中央绘制均在 GPU 顶点阶段使用同一 alpha 插值位置。

#### 计划范围

- 同步更新 `NTSD/BattleCentralTransparent` 与 `NTSD/BattleCentralTransparentArray`；
- 新增统一的 `_BattlePresentationAlpha` 参数；
- 在 `BattleRenderFeature.BattleRenderPass.Execute` 每个显示帧使用现有 `MaterialPropertyBlock` 写入 alpha；
- 仅对顶点 position 做 `Lerp(previous, current, alpha)`；
- 保持 pre-multiplied alpha、纹理采样、UV、slice、材质语义、透明队列和 ZWrite 合同不变；
- 保证每条 segment 按其现有 binding mode 绑定 `_MainTex` 或 `_MainTexArray`，不双重绘制。

#### 禁止事项

- 不使用全局 Shader 参数污染其他相机或不相关材质，除非多相机所有权证明明确；
- 不在 fragment 阶段做跨图片帧混合；
- 不为普通 Texture2D 与 Texture2DArray 制定不同的 alpha 时间线；
- 不在 Shader 中读取或写入逻辑 runtime。

#### 验收条件

1. 两套 Shader 在相同 previous/current 位置和 alpha 下产生一致的顶点空间结果；
2. `alpha=0` 显示 previous 位置，`alpha=1` 显示 current 位置，`alpha=0.5` 是精确中点；
3. 高帧率关闭或不可用时，alpha / vertex 数据降级为 current-only，画面与 HFR Off 基线一致；
4. 现有普通 Texture2D 页、Texture2DArray slice、阴影和角色均能正常渲染；
5. 每个显示帧只更新 alpha / 执行现有 draw，不触发完整 Mesh rebuild；
6. 透明 segment 的顺序、draw count per display frame、资源绑定和 blend contract 保持正确。

#### 测试标准

- Shader / Editor test：读取或验证顶点属性布局、两套材质的 alpha 参数存在；
- Render test：Texture2D 与 Texture2DArray 各验证 alpha 0/0.5/1 的屏幕或渲染目标位置；
- Play Mode：同一运动路径在 30、60、120 Hz 下记录显示采样位置，60/120 的中间位置必须出现；
- Regression：HFR Off 截图/像素结果、command order、checksum 与 HFR-00 基线一致；
- Profiler：显示帧内无本任务新增托管分配，Mesh upload 频率不随显示 Hz 线性增大。

#### 本任务完成后的可见效果

在 CentralOnly、连续实体、资源正常的场景中，角色和其他实体的**位置移动**开始在 60/120 Hz 下平滑；人物图片帧仍按 DAT 的 30 Hz 节奏离散切换。

#### 回退条件

任一 Shader variant 未准备、材质不匹配或资源解析失败时，当前战局应回退为 current-only 绘制，并在诊断中给出 reason code；不得显示半帧、黑块或双重实体。

---

### HFR-06：阴影、持有物、相机与特殊表现的关联一致性

**状态：`NOT_STARTED`**  
**依赖：HFR-05**  
**目标：** 让已启用的主体插值不破坏阴影、持有物、相机、背景边界和技能表现的空间关系。

#### 计划范围

- 阴影与主体共享同一 entity identity、previous/current sample 与 alpha；
- 稳定持有关系中的武器、螺旋丸等 attached entity 可随持有者平滑移动；
- holder/link/cpoint 切换、抓取、投掷、掉落、特殊挂点切换时强制 reset interpolation；
- 审计 `BattleCameraSafeArea` 与 `NTSDRenderSpace.PresentationCameraOffset`，保证相机仅应用一次位移；
- 保持 `RenderOffsetX`、`CameraX`、`DisplayZ` 的逻辑语义不变，不把视觉 alpha 写回；
- `OverlayGlyph`、`HitRecord`、火花和烟雾第一阶段保持离散；若后续需要平滑，必须另立任务和同等级验收。

#### 禁止事项

- 不用移动全局场景或逻辑相机来伪造实体插值；
- 不让阴影单独使用另一套 alpha；
- 不跨抓取/投掷边界平滑武器；
- 不修改固定活动区域、stage 边界或角色逻辑坐标来修相机视觉问题；
- 不把 hit spark、overlay 或状态切换误当作普通实体位置连续运动。

#### 验收条件

1. 连续运动时角色、阴影、稳定持有物的相对位置与 HFR Off 当前帧几何关系一致；
2. 拾取、抓取、投掷、掉落、opoint、link state 改变时无从旧锚点飞向新锚点的错误轨迹；
3. 相机移动时实体不出现双倍位移、反向漂移或边界错位；
4. 角色不能突破逻辑左/右活动边界，也不会因相机表现而提前被阻挡；
5. 火花、overlay、烟雾仍保持当前逻辑 tick 的可观察时序；
6. `BattleVisualScale = 1.5` 的既有视觉比例不被高帧率代码改动。

#### 测试标准

- Play Mode 场景：奔跑、跳跃/落地、掉落武器、拾取/持有/投掷、飞行物、opoint 分身、opoint 持有武器；
- 用户报告回归：Naruto 防前跳螺旋丸、Naruto 防下攻分身、奔跑攻击等，每项记录输入、等待 tick、对象数量、主体/阴影/挂点位置；
- Camera test：角色分别靠近左右边界、镜头跟随、背景边界存在与不存在的场景；
- Screenshot / video：30、60、120 Hz 同一 seed / 输入对照；
- Logic trace：每个场景 checksum 与 HFR Off 相同。

#### 本任务完成后的可见效果

用户看到的将是“角色、影子、武器和稳定挂点一起平滑运动”，而不是只有角色主体顺滑、阴影或武器延后一帧。

#### 回退条件

某类特殊对象无法证明挂点连续性时，该类对象单独退回离散 current-only；不能以破坏 holder/cpoint 逻辑换取视觉平滑。

---

### HFR-07：CentralOnly 回退、资源故障与生命周期安全

**状态：`NOT_STARTED`**  
**依赖：HFR-06**  
**目标：** 保证高帧率功能不会削弱当前 CentralOnly fail-closed 资源、submission 和 Legacy 边界。

#### 计划范围

- 高帧率 v1 只在 `BattlePresentationBackendMode.CentralOnly` 且 central submission 有效时启用；
- `LegacyOnly`、`CentralShadowBuild` 以及 CentralOnly 提交不可用时，不混用两套像素所有权；
- 对 material、Texture2DArray、catalog、backend lease、world change、submission generation、camera 不可用等失败路径建立明确 reason code；
- 失败时清除或冻结运动历史，防止下一份 submission 使用旧 world / 旧 backend 的 previous 数据；
- 维持当前对象池、slot generation、资源绑定和 submission lease 释放顺序。

#### 禁止事项

- 不因中央提交失败而悄悄同时绘制 Legacy 与 Central；
- 不保留旧 backend 的 previous position 给新 generation 使用；
- 不把“画面没显示”吞成无日志无诊断的高帧率问题；
- 不删除 Legacy 组件或回退路径作为本计划的前置条件。

#### 验收条件

1. CentralOnly 正常时只有中央像素所有权；
2. 强制资源/提交失败时没有重复实体、残影、黑块、错误材质或跨战局位置；
3. 失败后的下一次有效 Central submission 首帧不插值旧数据；
4. Legacy / diagnostic mode 不会读取 CentralOnly 的运动历史；
5. 每个拒绝路径提供稳定 reason code 与可诊断计数；
6. world reset、场景重载、battle end 后没有遗留 motion history 或 renderer lease。

#### 测试标准

- Focused Editor：fake missing material、missing array material、invalid catalog、leased backend、world replacement、submission retirement；
- Play Mode：主动切换或模拟 CentralOnly 资源不可用，检查只出现一个一致的回退所有者；
- Lifecycle test：连续战局开始/结束、对象池 slot reuse、场景 reload 后的历史清理；
- `BattleRuntimeSelfCheck`：资源故障和 generation reset 路径必须有明确断言；
- Allocation test：失败路径不允许每帧重复创建异常对象或字符串。

#### 本任务完成后的可见效果

正常情况下保持平滑；异常情况下宁可回到离散当前帧，也不会产生错位、双重绘制或持续残影。

#### 回退条件

只要无法证明 Central 与 Legacy 像素所有权唯一，就全局关闭 HFR 插值，不得将半完成的回退路径设为默认。

---

### HFR-08：Dedicated Simulation Worker 与未来 Lockstep 的表现时钟

**状态：`NOT_STARTED`**  
**依赖：HFR-07**  
**目标：** 让 Dedicated Worker 与未来 `LockstepBuffered` 模式拥有独立、可解释的表现采样时钟，而不错误复用本地主线程 accumulator。

#### 计划范围

- 为每份“已完成且可显示”的表现快照记录发布序号、逻辑 tick 与单调表现时间戳；
- OfflineLocal 同步路径可以继续使用 `SimulationTickDriver.RenderAlpha`；
- Dedicated Worker 路径以实际被主线程消费的连续快照作为 previous/current 来源；
- 未来 LockstepBuffered 路径以连续确认逻辑帧和固定显示缓冲计算 alpha；
- 快照迟到、断流、跳帧、恢复、server correction 或 worker failure 时退回 current-only，绝不做基于速度的外推；
- 此任务只预留服务器接口，不实现 ACK、jitter buffer、房间、登录、重连或网络库。

#### 禁止事项

- 不让 Unity wall clock 决定 NetworkLockstep 的逻辑推进；
- 不因表现需要而提前消费未来输入或跳过未 ready 的逻辑帧；
- 不把 worker 线程中可变 runtime 引用直接交给 Unity 渲染；
- 不把未来服务器业务塞进本任务。

#### 验收条件

1. Worker / Lockstep 的表现 alpha 来源可追溯到已发布、已消费的快照，而不是猜测性的主线程 tick；
2. 快照延迟或停止时，最后一份正确快照稳定显示，不抖动、不外推穿墙；
3. 两个连续快照可用时，位置平滑；不连续时自动 current-only；
4. Worker 开关对逻辑 checksum、FrameInputSet、slot/generation 和事件顺序无影响；
5. 未来网络接口边界仅为表现时间戳 / snapshot metadata，不引入服务器业务实现。

#### 测试标准

- Worker focused test：人为延迟发布、乱序到达、跳 tick、failure / restart；
- Lockstep host-policy test：ready gap 下不推进逻辑，但表现能安全保持最后确认帧；
- Replay test：相同 journal 在同步/worker 路径下逻辑 checksum 完全一致；
- Play Mode：Dedicated Worker 开启和关闭各跑一次连续移动与对象生成序列；
- Allocation / thread test：无跨线程 Unity API、无未释放 submission lease、无 HFR 热路径 GC。

#### 本任务完成后的可见效果

开启 Dedicated Worker 或未来帧同步后，表现不会退化成错误平滑；连续快照时平滑，快照不足时稳定保持而非错误预测。

#### 回退条件

若 Worker/Lockstep 时间线无法证明正确，强制设为 current-only。单机同步路径可以继续支持 HFR，不被未完成的网络路径阻塞。

---

### HFR-09：完整回归、性能矩阵与发布认证

**状态：`NOT_STARTED`**  
**依赖：HFR-08；若 HFR-08 延后，至少需先完成 HFR-00 至 HFR-07 的单机认证**  
**目标：** 用统一证据确认高帧率表现确实提升运动连贯性，同时不回归战斗规则、中央绘制正确性、GC 或性能容量。

#### 计划范围

- 建立 30 / 60 / 120 Hz 的同 seed、同输入、同场景对照矩阵；
- 收集逻辑 checksum、command hash、实体/slot/generation、Mesh build、segment、draw、GC、CPU main thread、render thread 和 GPU 数据；
- 将中央表现热路径与完整游戏帧耗时分开报告；
- 对 100 / 300 / 500 / 1000 active entity 的场景分别测量，不把 1000 实体与普通双角色场景混为同一结论；
- 完成 Editor / Desktop 的认证；Android 真机认证由用户后续执行时补入，不得在无真机证据时宣称移动端已完成。

#### 禁止事项

- 不只凭 Stats 面板的一个瞬时 FPS 下结论；
- 不把 HFR On 的性能问题归因于逻辑，或把逻辑性能问题伪装成渲染插值成功；
- 不为达到帧率而降低 AI、跳过有效碰撞、跳过命中、缩短技能链、屏蔽对象或减少 DAT 表现；
- 不把 Editor Deep Profile 数据当作发布设备性能结论。

#### 验收条件

1. HFR Off、60 Hz、120 Hz 的逻辑 checksum / trace 完全一致；
2. Central command identity、排序、资源绑定和可见性在相同逻辑 tick 一致；
3. HFR On 的新增表现热路径在预热后为 0 B GC allocation / 显示帧；全局分配另行报告，不掩盖既有分配源；
4. 60 Hz 认证平台上，完整可见帧 P95 不高于 16.67 ms 才能宣称“该平台稳定支持 60 Hz”；
5. 120 Hz 认证平台上，完整可见帧 P95 不高于 8.33 ms 才能宣称“该平台稳定支持 120 Hz”；
6. 若某个容量场景不能达到目标帧预算，报告为“不支持该场景的该显示模式”，不能降低验收标准；
7. HFR On 下 `BuildCommands`、`Mesh Build` 与 `SetVertexBufferData` 不得随 60/120 显示帧数倍增；
8. 每显示帧的 central segment / draw 数不得因 HFR 插值本身增加；显示频率导致的总绘制频次增加属于预期，必须单独报告；
9. 人工 Play Mode 验收确认角色、影子、武器、飞行物、opoint、持有与投掷不发生错误拖影。

#### 测试标准

| 层级 | 必做测试 |
|---|---|
| 编译 | Fresh Unity compile，0 error。 |
| 自动自检 | `BattleRuntimeSelfCheck` 全量通过，新增 HFR focused assertions 通过。 |
| 逻辑回归 | 同 seed / journal 的 HFR Off / On 逐 tick checksum、RNG、slot/generation、frame、HP/PP 与事件记录比较。 |
| Central 渲染 | Texture2D、Texture2DArray、影子、实体、segment、submission、回退、bounds、alpha 0/0.5/1。 |
| 真实 Play Mode | 双角色、跑跳、武器、持有/投掷、Naruto 代表性技能、边界/相机。 |
| 性能 | 100/300/500/1000 entity × 30/60/120 请求模式，预热后至少 60 秒，记录 P50/P95/P99、GC、draw、Mesh build、CPU/GPU。 |
| 平台 | 至少一台真实 60 Hz desktop/device；120 Hz 需真实 120 Hz 输出设备。Android / Adreno / Mali 结果另列，未测不得标已认证。 |

#### 本任务完成后的可见效果

在通过认证的平台上，用户选择 60/120 Hz 后可看到已连续存在的战斗实体和阴影更平滑地移动；同时战斗技能、动画帧切换、命中、武器关系和逻辑结果仍与 30 Hz 基线一致。

#### 回退条件

- 逻辑 trace 或 checksum 不一致：立即停止高帧率默认启用，回退到 HFR Off；
- 出现跨对象拖影、阴影/武器不同步、双重相机补偿：仅禁用受影响命令类别并回到 HFR-03 / HFR-06；
- 60/120 不能达到目标平台预算：保持功能可选或降级，不能标记为“稳定支持”。

---

## 6. 插值资格与离散规则矩阵

| 表现内容 | HFR v1 策略 | 原因 | 后续扩展条件 |
|---|---|---|---|
| 角色主体 `Entity` | 插值平移 | 连续位置最能受益。 | identity / relation 连续。 |
| 掉落武器、飞行物、分身、特殊攻击 `Entity` | 插值平移 | 同属于连续实体位置。 | 出生/销毁/holder 变化时 reset。 |
| 阴影 `Shadow` | 与实体同 alpha 插值 | 避免主体与影子脱节。 | 必须与主体 identity 一致。 |
| 持有物 | 稳定关系时插值 | 跟手效果更自然。 | holder/link/cpoint 不变。 |
| `effectivePic`、`frameId`、UV、pivot、flip | 始终离散 | DAT 与美术姿势时序是 30 Hz 逻辑表现。 | 仅在未来有新增美术/骨骼方案时独立讨论。 |
| Overlay glyph | v1 离散 | 避免 UI 与实体关系错误。 | 需单独锚点合同。 |
| HitRecord / 火花 / 烟雾 | v1 离散 | 命中时序和生命周期敏感。 | 需独立 effect timeline。 |
| 排序 key / segment 顺序 | 始终离散 | 透明排序必须服从当前逻辑命令顺序。 | 不应被插值改变。 |
| CameraX / RenderOffsetX 逻辑字段 | 始终离散逻辑 | 不能用表现修正改变战斗真相。 | 仅渲染空间可按单一所有权平滑。 |

---

## 7. 必须记录的诊断字段

高帧率功能上线前，诊断面板或结果文件至少应包含：

```text
RequestedPresentationHz
EffectivePresentationHz
MeasuredDisplayHz / MeasuredFPS
VSyncState
PresentationBackendMode
CentralSubmissionGeneration
CurrentPresentationTick
PreviousPresentationTick
PresentationAlpha
EligibleEntityCommandCount
EligibleShadowCommandCount
ResetNoHistoryCount
ResetGenerationMismatchCount
ResetLifecycleTransitionCount
ResetHolderOrLinkTransitionCount
ResetTickGapCount
ResetWorldOrSubmissionCount
MeshBuildsPerSecond
SetVertexBufferUploadsPerSecond
SegmentsPerDisplayFrame
DrawsPerDisplayFrame
HfrPathAllocatedBytesPerDisplayFrame
LogicChecksum
CurrentFallbackReason
```

诊断字段应为数值、枚举或预定义 reason code；不要在高频路径拼接日志字符串。

---

## 8. 认证场景清单

以下场景必须以相同 seed、相同输入分别跑 HFR Off / 60 Hz / 120 Hz；其中 120 Hz 只能在真实 120 Hz 显示条件下给出视觉认证。

| 编号 | 场景 | 重点观察 |
|---|---|---|
| V1 | 角色静止与普通走跑 | alpha 不影响静止；跑动位置平滑；frame 图片仍离散。 |
| V2 | 跳跃、空中移动、落地 | 垂直/水平位置平滑；落地不提前或滞后。 |
| V3 | 随机掉落武器 | 武器、影子、碰撞声音与可见性不脱节。 |
| V4 | 拾取、持有、投掷、掉落 | holder 切换不从错误锚点平移。 |
| V5 | Naruto 防前跳螺旋丸 | opoint、手部挂点、角色移动跟手与攻击链。 |
| V6 | Naruto 防下攻分身 | 分身出生、投掷、落地与自主移动边界。 |
| V7 | 常规飞行物与连续命中 | 飞行平滑，不改变命中次数、间隔或结束条件。 |
| V8 | 多实体深度排序 | Z/slot 排序保持；交叉移动不引入透明顺序错误。 |
| V9 | 左右场景边界与相机跟随 | 无双倍镜头位移、逻辑边界与视觉边界一致。 |
| V10 | 暂停、恢复、战局重置、对象池重用 | 无残影、无旧 generation 位置继承。 |
| V11 | CentralOnly 资源/提交失败 | 安全 current-only / 正确回退，不双绘。 |
| V12 | 100 / 300 / 500 / 1000 active entity | HFR 热路径成本、GC、Mesh build 与 render budget。 |

---

## 9. 最终发布判定

### 9.1 可以宣称“60 Hz 表现支持”的条件

只有在目标 60 Hz 设备上同时满足以下条件，才能宣称该平台支持 60 Hz 高帧率表现：

- HFR-00 至 HFR-07 全部 `VERIFIED`；
- HFR-09 在该设备通过；
- 实际显示率约 60 Hz；
- 完整可见帧 P95 `<= 16.67 ms`；
- HFR 热路径预热后 0 B / 显示帧；
- HFR Off / On 的逻辑 trace 完全一致；
- V1 至 V11 的目标 Play Mode 场景通过。

### 9.2 可以宣称“120 Hz 表现支持”的条件

除满足 60 Hz 的所有逻辑与正确性条件外，还必须：

- 使用真实可输出 120 Hz 的屏幕或移动设备；
- 诊断确认实际表现输出约 120 Hz；
- 完整可见帧 P95 `<= 8.33 ms`；
- 在该目标场景容量下没有 HFR 引入的 GC、Mesh rebuild 倍增或错误回退；
- V1 至 V12 中声明支持的容量和内容范围均有独立报告。

### 9.3 不允许的模糊结论

以下情况只能报告“代码已写”或“某子任务通过”，不能宣称高帧率完整支持：

- 只在 Editor Game View 看到看似更顺滑；
- 只设置 target frame rate，未测实际显示率；
- 只验证角色，未验证阴影、持有物、opoint 和生命周期；
- HFR Off / On 的逻辑 trace 尚未比较；
- 120 Hz 仅在 60 Hz 屏幕上运行；
- Worker / Lockstep 路径没有正式表现时间线；
- 1000 实体性能未达到目标却只报告平均 FPS；
- 高帧率开启后通过跳过命令、降低 AI 或忽略战斗规则才达到性能。

---

## 10. 当前执行状态

截至本文创建时：

- 已完成：对当前 `SimulationTickDriver`、表现快照、中央 Mesh、两套中央 Shader、URP Render Feature、相机和 render space 的只读盘点；
- 已确认：逻辑保持 30 Hz；`RenderAlpha` 已存在但尚未接入中央绘制；当前中央顶点没有 previous-position；两套 Shader 需要同时修改；
- 尚未开始：HFR-00 至 HFR-09 的代码、测试、性能认证和真实 Play Mode 高帧率验收；
- 本文不改变：任何战斗脚本、Shader、材质、Scene、ProjectSettings、DAT、资源或服务器代码；
- 后续首个实现动作：只有在用户明确批准后，先创建 HFR-00 对应 Change Record，再从基线与功能门开始，不直接跳到 Shader 修改。

