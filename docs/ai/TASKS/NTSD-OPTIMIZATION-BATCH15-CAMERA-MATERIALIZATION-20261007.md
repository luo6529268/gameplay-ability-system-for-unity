# 第十五批 M-03 相机物化消重 Task Contract

最终审计（2026-10-07）：Tools/Validate-ChangeLedger.ps1 exit0，1311 Records/9 governed script覆盖/0error；4260全库历史warning，本Change/新fixture0warning，四宽过滤warning为旧Record声明路径而非本包缺项。514事前保护/10准确备份/三末期补保护0drift；175本地链接0missing、git diff --check exit0、34=高12/中14/低8、编译identity0drift、HEAD与staged不变。原Menu clean/8roots/单Scene/非Play。依据已回链REPORT；生产和父项继续RUNTIME_PENDING。

Task NTSD-OPTIMIZATION-BATCH15-CAMERA-MATERIALIZATION-20261007 / SCOPED_CAMERA_MATERIALIZATION_PASS；Change NTSD-OPT-M03-CAMERA-MATERIALIZATION-015 / RUNTIME_PENDING（动态/像素/完整链/高负载/设备门保留）。
需求：用户“开始执行下一批的任务”，承接子批14实证每显示帧LateUpdate和相机两次Build。
原Editor PID19040/port6401，Unity2022.3.62f3/URP14；开始原Menu clean/nonPlay。

## 准确实施范围

只修改以下自编写脚本：
- Assets/NTSD/Scripts/Animation/Rendering/BattleCentralRenderSystem.cs：新增host presentation helper。保留Queue和原ResolveDisplayAlpha时钟取样；仅interactive/non-batch、CentralOnly、>30FPS、已有有效非stale中央plan且原相机/feature/material近期观察有效时，推迟几何到既有camera入口。Explicit Flush API、相机采样公式/入口、容量/slot/lease/segment/failclosed保持。
- Assets/NTSD/Scripts/Simulation/Stage/SimulationStageRenderModule.cs：PresentLatestFrame调用上述helper；不改RenderDispatch/tick/sort/body/音频/生命周期。
- Assets/NTSD/Scripts/Test/Editor/BattleCentralProductionWindowSceneProbeEditor.cs：复用既有观察探针，只扩展具名batch15 before/after独立输出菜单。保留batch14入口/证据；新输出CreateNew，单轮显式开始，不依赖自动第二轮衔接。
新增 BattleCentralPresentationDeferralEditorTests.cs（Animation/Rendering/Editor目录）及独立meta，测试先行。
维护进度总表、风险登记、M03独立方案、handoff、STATE、Ledger、文件Operation索引七文档。

## 消费链核验与不变量

CentralOnly旧Renderer、overlay、spark按mode立即绕过，不依赖当次geometry；legacy继续使用原Flush与LastResolvedDisplayAlpha。
冷启动、world切换、stale plan、缺/失活feature/material/camera、近期路由失效、<=30FPS、EditMode/batch、Legacy/CentralShadowBuild继续原路径。
TryAcquireSubmission既有pending版本拒绝保留，不能提交旧publication；camera先materialize再acquire。
仅消除未被生产早期消费者使用的重复几何；不减少物理segment/CPU DrawMesh或承诺GPU batch/FPS收益。
不引入runtime cache/worker/新slot；existing pending/display clock与11阶段关闭Reset负责生命周期，不改变停止接单/回收顺序。
精确33ms/3ms/max2、逻辑逐位、输入/RNG/pass、publication只读、排序/first-visible语义不重新定义。
不读写活跃Q06 BattlePresentationShadowBuild方法体。EXT1仍PROPOSED/MODIFY_REQUIRED，无专项M0/instancing；
MONO USER_HOLD、ATLAS bank/预算/格式/segmentmerge/failclosed门不变；不改PERF/ATLAS正文。
Server/Gen/Plugins/Scene/Prefab/资源/Settings/InputActions排除。

## 验收与副作用

先新测试RED（旧Flush实际行为，反射仅绑定窗外），再最小生产编辑；编译与具名focused tests、相关latestframe/capacity/report/submesh/memory/motion回归。
在同一saved Battle、同观察版本，改前一个自然96tick窗口，改后两次显式自然96tick；统计Build/camera与uploaded bytes，不从CPU draw推导GPU batch。
校验publication隔离、合法segment/bounds/stride44、零growth/failed/rejected、slot正常复用、原分配计数/相机envelope分别记录。
按11阶段正常关闭，objects/slots/borrowers0，Scene clean/SHA同；结束回原Menu clean/idle。
不以Editor0B当全路径/设备0GC，不称1000AI/120FPS/Android/全World/native/GPU像素验收。
原Scene打开和短时Play是受控验证副作用；不保存Scene，不打断外部Play或真实Runner；不开第二Editor。
新探针模式每次独立菜单开始、最多2048样本、启动600s/总900s，失败保存原件；不覆盖任何既有证据。

## 恢复与审计

[Operation](../FILE-OPERATIONS/NTSD-OPTIMIZATION-BATCH15-CAMERA-MATERIALIZATION-20261007/RECORD.md)与before.json记录10个当前dirty/现存文件准确字节及514保护文件。
新增文件和证据均唯一。未授权删除/恢复/回退；如需回滚必须另获授权后按before backup恢复，不使用HEAD/reset/checkout/clean/stash。
[报告](../../../artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH15-CAMERA-MATERIALIZATION-20261007/REPORT.md)记录命令、原件与验证边界；交付前Validate-ChangeLedger及保护SHA/Scene/HEAD/staged检查。
