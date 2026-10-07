> 2026-10-07 第25批必要证据更正：当前Editor已知存活1MiB数组仍由GC.GetAllocatedBytesForCurrentThread读0，positive control未响应。第23批未有校准，原0B读数/断言事实保留，但不再作为完整热路径0GC验收；H11更正为EVIDENCE_PENDING，容量/显示/成本/lease/关闭保持。M03显示/成本/GameConfig Foot限定通过不撤回，不称0GC。原父项均OPEN，不重跑历史或改production。详第25批REPORT。

> 2026-10-07 第23批限定收口：NTSD-OPTIMIZATION-BATCH23-FULL-CHAIN-CLOSURE-20261007 / SCOPED_FIRST_STAGE_H11_M03_ACCEPTANCE_PASS；NTSD-OPT-H11-M03-FULL-CHAIN-CLOSURE-023 / VERIFIED（本Task）。原Editor24/24；两活动aux完整CPU桥各1800sample当前线程0B/growth0/CPUlease0；原Battle重进1800camera每帧Foot2/Health2，两slot、DrawMesh11504录制=执行、1800Build，11阶段三残留0/Scene同/Menu恢复。第13批A-B五指纹同、第15批消费域及动态/像素等限定证据复用；不称global/native/GPU/1000AI/Android。新子批2/8完成，六项首阶段H11/M03限定交付2/6，34父项关闭0；下一H06/H07与M13/M14，不加本条目可选验收，专项门保持。

# H-11 表现缓存容量预热与完整热路径 0GC 方案

> 优先级：高
> 状态：`OPEN / RUNTIME_PENDING / CACHE_PREWARM_AND_CAPACITY_GUARD_FOCUSED_PASS`
> 最后更新：2026-10-06
> 主登记表：[优化风险登记表](../android-mobile-readiness-priority-risk-register.md)
> 共同合同与启动门：[本轮复核](../battle-optimization-rebaseline-and-start-gates-20261006.md)
> 用户已批准按文档开始；子批01/02/03/04/05见[统一进度](../battle-optimization-progress-tracker.md)。
> 下面父项完整验收条件不等于子批已通过；实际执行证据单列。

## 当前事实与缺口

[已验证-代码] `Simulation/Presentation/BattlePresentationDisplayMotion`
有初始16长度缓存、PrepareCapacity及Array.Resize；
`Animation/Rendering/BattleCentralRenderSystem.PrepareBattleCapacity`
子批01改前预热backend/submission/catalog，未调用本实例DisplayMotion.PrepareCapacity；
改后已在同一入口按entityCapacity接入，原Editor聚焦正例通过。
物化调用DisplayMotion.Prepare；子批02增加sealed runtime slot准入上限，
prior/current count及handle槽位在generation和缓存写入前检查。

`BattleDynamicMeshBackend.PrepareCapacity`准备chunk/segment；
改前Upload在描述缓存不足时new SubMeshDescriptor[]；子批01已按每chunk保守segment
上界提前准备同一数组，预热与高水位复用正例通过。子批02将预热command容量封口，
Build在geometry mutation之前拒绝超限；未封口的独立诊断fallback仍保留。
`BattleCentralSubmission`在CopyFrom前按entity/hit/command/motion count硬限检查；
`PrepareFrameImmediate`在捕获前及命令物化后无分配预检，超限整份拒绝并复用旧fail-closed。
命令物化内部其它缓存、native/GPU存储和完整提交路径尚未封闭，不宣称全部增长已封闭。
子批03将脚下marker/头顶bar接入同一slot封口，分别按原可绘制Entity条件预检，
禁用/无marker纹理/无有效health不误拒绝；正常sealed接受路径不再增长数组或重建Mesh。
Health direct Build按输入record数硬限；BuildFromFrame按实际可绘制bar数硬限，两口径区分。
子批04为BattleCatalogCentralResourceResolver.PrepareCapacity补sealed检查，参数校验后、
Ensure/Prepare/limit写入前拒绝；原Unseal后可准备。原bounded cache skip仅停止memoization，
不丢解析、不拒绝draw；原Configure清理/换材质/失效与存储复用保持，无新缓存或owner。
这是可达分配路径，不等于本轮测得每帧GC或已定位唯一热点。
子批05已确认existing managed chunk在native Mesh被销毁后，原PrepareCapacity只准备数组，
EnsureMesh延后到Build/GetChunkMesh。现已在原EnsureChunk后访问Mesh，将原恢复前移到seal前；
活Mesh不重建/unused高chunk不提前恢复，原所有权/Dispose保持。真实无Domain Reload重进未验收，
战斗中突发native Mesh丢失的原懒恢复仍可能分配，未误称所有native分配已封闭。
已有预热或首次构建可能覆盖某个场景，不能据此覆盖晚生成、高slot及新segment高水位。

## 父项解决方案（分批实施，未全部完成）

1. 清点物化→插值→命令解析→Quad→submesh→上传→录制→提交的全部managed/native缓存，
   登记owner、容量单位、最大高水位、预热入口、seal、生命周期和释放方法。
2. 复用已有PrepareCapacity入口，将DisplayMotion按runtime slot上限预热；
   描述缓存按物理segment保守上界预热。不同单位不混用：
   entity slots、presentation commands、segments、QuadsPerChunk各自记录。
3. 每个既有submission/backend slot的允许容量、overflow reason和拒绝策略显式化；
   seal后禁止显式或隐式扩容，禁止局部截断或部分发布掩盖容量不足。
   超限遵循现有fail-closed合同；不擅自修改segment合并或新建instancing路径。
4. 新增长度/Native/GPU存储必须计入steady/transition账本，覆盖两slot及读lease保持数据；
   复用/释放仍受consumer生命周期合同约束，CPU lease归零不是GPU完成证明。
5. 将真实高水位与增长次数纳入报告；现有CapacityGrowthCount不能未经审计就宣称覆盖全部数组。

## 实施前门与验收

- 用户批准最小Task/Change和准确调用路径；确认slot/command/segment上限及启动预热成本，
  不能以4096 QuadsPerChunk充当submission容量。容量数值可在获批基线阶段定，但seal前必须冻结。
- 指定workload在seal后完整物化—上传—录制—提交热路径0B managed allocation、
  0隐式/显式增长；测量范围与诊断自身分配分离，不豁免残余热路径分配。
- 延迟生成分身、高slot复用、交替纹理/材质、StrictOrderedDraw、跨chunk、
  空帧→密集帧→空帧均无未预热增长。
- 容量正例不拒绝；超限负例整份失败、不发布半帧，不损坏旧有效slot。
- checksum、first-visible、publication顺序、UV采样边界、透明层级和11阶段关闭不回退。

## 未来测试矩阵

| 测试 | 条件 | 通过标准 |
|---|---|---|
| 容量边界 | 0/上限/上限+1；slot与command/segment分别覆盖 | 分配只在seal前；超限fail-closed |
| 冷热路径 | 新World/复用World、30与高显示帧率 | 插值与非插值均覆盖，不只重复首帧 |
| 结构压力 | 晚生成/销毁/slot复用/多资源run/chunk边界 | 0GC/0增长，无ghost/错绑 |
| 生命周期 | 两slot读lease、关闭/重进、取消 | 无覆盖消费中数据、无残留/悬挂 |
| 内存 | 两slot缓存/上传/峰值 | 符合批准预算，native与managed分别记账 |
| 正确性 | 固定publication+输入A/B | 模拟逐位一致；表现已声明合同不变 |

## 留痕与回滚

2026-10-06 后续启动：NTSD-OPTIMIZATION-BATCH01-CAPACITY-20261006，
Change NTSD-OPT-H11-CACHE-PREWARM-001。仅补现有DisplayMotion和chunk描述缓存预热，
test-first聚焦验证；不改slot/segment语义、GPU API或Q06排序。
硬容量/seal/overflow、完整热路径0GC及Scene/设备门继续开放。
实际结果见[本批报告](../../../../artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH01-CAPACITY-20261006/REPORT.md)。

2026-10-06 子批01验收：test-first 7项原状检查中6项按预期失败，修补后7/7通过；
原有submesh/bounds回归8/8通过。高slot interpolation lookup重复64次0B managed allocation，
仅覆盖该lookup，不等于物化—上传—录制—提交全路径0GC。原Editor实际编译且无新error CS；
未做真实Battle enter/exit/re-enter、完整M0、native/GPU峰值或Android测试，父项仍OPEN。

2026-10-06 子批02限定验收：NTSD-OPTIMIZATION-BATCH02-CAPACITY-SEAL-20261006，
Change NTSD-OPT-H11-CAPACITY-SEAL-002；[报告](../../../../artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH02-CAPACITY-SEAL-20261006/REPORT.md)。
新16项先RED再16/16 GREEN；旧预热/mesh/bounds/LatestFrame/两motion共30/30。
硬限使用逻辑command/slot/count而非物理数组或4096 chunk长度；0/精确/超限及prior/current
槽位覆盖。整帧拒绝保留旧有效submission/read lease，不切Legacy、不提交半帧。
Prepare封口后禁止重预热，原End解除；未更改GPU完成判据/segment/Q06排序/关闭顺序。
64次varying-alpha DisplayMotion.Prepare含自身预检测得0B，仅局部范围；新增CPU扫描成本
未测。父项剩余为其余物化缓存/完整0GC/内存/GPU/原Battle关闭重进/设备门，不重复称硬限未实施。

2026-10-06 初次整理历史：新增方案，静态确认分配路径，当时未运行GC测试。
2026-10-06 子批03限定验收：NTSD-OPTIMIZATION-BATCH03-AUX-CAPACITY-SEAL-20261006，Change NTSD-OPT-H11-AUX-CAPACITY-SEAL-003；
[报告](../../../../artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH03-AUX-CAPACITY-SEAL-20261006/REPORT.md)。
15项先RED再15/15 GREEN，旧两批/几何/物化/辅助作者配置回归61/61。
logical17不混32数组余量，0/disabled/filtered/4096 marker与1365 bar物理边界均覆盖；
超限在辅助存储/mesh mutation前拒绝，四生产fail-closed出口保留last-good/read lease。
64次交错帧Foot+Health BuildFromFrame含自身预检与mesh上传局部0B；非完整central0GC。
只加缓存metadata和边界检查，geometry/UV/颜色/锚点/排序/GPU完成/关闭阶段不改。
额外CPU扫描耗时未知；原Battle/完整链0GC/其余物化缓存/全局预算/Android仍开放。

只做容量/所有权修补，不同批改算法、GPU API或Q06活跃排序方法。

2026-10-06 子批04限定验收：NTSD-OPTIMIZATION-BATCH04-RESOLVER-SEAL-20261006，Change NTSD-OPT-H11-RESOLVER-SEAL-004；
[报告](../../../../artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH04-RESOLVER-SEAL-20261006/REPORT.md)。
有效RED新15项10控制PASS/5预期guard失败；旧test trusted helper旧27签名1失败。
生产仅3行sealed guard，测试helper仅2行适配当前28参数MotionAnchor，未改断言或Q06 body。
GREEN新15/15+适配旧1/1，相关105/105通过，去重120项；初两次fixture失败保留，不算生产证据。
缓存逻辑0/1/17不混物理Dictionary/hash余量，32种资源64轮strict/Prepared+
Configure no-op局部0B，冷未命中与满缓存仍解析全部字段，不扩缓存，不把skip当丢draw。
原主Editor PID19040域重载后端口改6401，Menu clean非Play，无第二实例。
完整物化/上传/录制/提交0GC、其他物化缓存、真实Battle退出重进、预算/native/GPU/1000AI/Android仍pending。
回滚保留原路径及本批逆向补丁来源，按用户批准执行，不用破坏性Git。

2026-10-06 子批05限定验收：NTSD-OPTIMIZATION-BATCH05-NATIVE-MESH-PREWARM-20261006，Change NTSD-OPT-H11-NATIVE-MESH-PREWARM-005；
[报告](../../../../artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH05-NATIVE-MESH-PREWARM-20261006/REPORT.md)。
新12项有效RED10预期失败/2控制PASS，单PrepareCapacity三插入行修后12/12，旧120/120。
1/17/4096/4097容量required chunk失效恢复、存储/有效Mesh身份、零/选择性/sealed/Unseal通过；
恢复的stride44、UInt16、初始inert descriptor/index/bounds保持，不Build/提交预热结果。
17/4097 × Ordered/Strict恢复后首次Build+8轮稀疏/空/密集共25调用含resolve/write/mesh Upload局部0B；
通用API先暖但恢复后首次Build未豁免，不是RenderPass/完整central0GC、原Battle进退重进或GPU证据。
其他物化缓存/突发native丢失/完整链/总内存native/GPU预算/1000AI/Android仍开放。
