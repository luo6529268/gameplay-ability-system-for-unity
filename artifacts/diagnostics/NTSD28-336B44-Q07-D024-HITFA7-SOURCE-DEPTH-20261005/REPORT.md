# 非角色 hit_Fa7 源深度死区：修复与限定验收

2026-10-05；Change `NTSD28-336B44-Q07-D024-HITFA7-SOURCE-DEPTH-001`，状态 `RUNTIME_PENDING / SCOPED_FULL_DRIVER_PASS`。已完成共用分支修复、原Editor边界/邻例与一个完整生产Driver tick；没有原Battle Scene自然输入或正式根EXE同初态运行证书，不关闭Q07/D-024/Q12或总目标。

## 规则来源与首差

当前正式EXE再次只读核对为 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3`。正式playable闭包 `native_ai.cpp::NativeAi28::step_non_character_hit_fa` 495～526比较源整数Z的±5死区并写速度±0.4；`simulation_tick_driver.cpp`416调用，playable build.ps1纳入该文件。当前正式与Unity暂存 `decoded_dat/c/ank/a/atk.dat` SHA相同，OID875/action55为真实hit_Fa7。安可的OPoint/next链证明该内容分支可达，不证明自然输入产生本受控间距。

修前Unity以已经投影的view整数Z比较原版5像素阈值。源380/384在恒等视野整数差4，在2048×1152视野整数599/605差6，误写Vz+0.4；源380/376同理误写-0.4。后续非角色积分会让精确源Z也改变±0.4，不能只看仍被截断成380的源整数。

生产差异仅 `LF2Entity.RunNonCharacterHitFa7FrameLogic` 的共同门和两条Z读取：active target存在且双方source历史完整时，一起读SourceRuleZInt；否则整对保留旧view/raw读取。没有初始化空槽source、改变目标选择/X/Y/clamp/朝向、改其它hit_Fa或DAT，也没有角色/OID特判。前轮平台阴影补丁保留。

## 原 Editor RED → GREEN

生成命令统一为 `dotnet build Assembly-CSharp-Editor.csproj --no-restore --nologo -v:q -clp:ErrorsOnly`。

| 出口 | 实际结果 | 原件 |
| --- | --- | --- |
| 测试先行生成构建 | exit0，301 warnings/0 errors，9.36秒 | editor-red-build.log |
| 精确四参数RED，job fab30ec5c9144062893824c9d89f39f2 | completed4；fixed/gap±4两项分别误Vz±0.4，identity/gap4与fixed/gap6通过 | editor-red-result.json |
| 生产修改后生成构建 | exit0，334 warnings/0 errors，12.34秒 | editor-green-build.log |
| 同四参数＋旧active/empty两邻例，job bab0e57e9c924ecfaef23b870472fac5 | 正式summary 6/6通过、0失败，48.7978624秒 | editor-green-result.json |
| 单个完整Driver测试生成构建 | exit0，301 warnings/0 errors，9.22秒 | editor-full-driver-build.log |
| 精确单方法完整Driver，job 3ff1eaf080a1479b8c620385b11ccf3f | 正式summary 1/1通过、0失败，22.677129秒 | editor-full-driver-result.json |

RED job的result为null，计数来自completed4和两项failures_so_far，不捏造正式NUnit汇总；8845/8846为发现总数，不是执行数。GREEN四参数直接检查速度与一次既有mechanics的精确source/view增量；fixed/gap6的+0.4阳性防止“始终零速度”误过，另两项保护现有active与raw empty-slot处理。

MCP Refresh两次响应分别因domain reload发生WinError10054/短帧EOF；两次都未启动测试。随后只读确认原程序集已新、Editor idle/非Play/原Scene clean/Console0error后才启动具名测试，没有重发Refresh或重复启动job。所有终态仅读取对应handle。首次只读DAT身份路径漏decoded_dat导致FileNotFoundError，rg定位后按实际decoded目录重新只读核对；没有资源修改。

## 完整生产 tick 的边界

证据口径更正：既有wrapper除加载/注册形状外，还沿用旧schema、seed、诊断Stage23边界及部分初态；测试运行当前LoganRuntime与项目模式配置，并显式构造本案同态源位置和目标关系。独立只读复核确认它只执行一次StepOneTick，totalTicks=3仅用于构造旧夹具输入；下述通过结果不覆盖原Battle Scene自然Play或正式根EXE同初态。

`RealOid875SourceDepthDeadZoneSurvivesFullDriverTick` 复用原replay wrapper的加载、注册、生产Driver与关闭scope；旧三tickscenario的5EDA元数据仅满足旧fixture shape校验，旧source证书不作当前规则证据。运行时读取当前正式暂存LoganRuntime与项目mode Asset；Stage23是已有显式测试边界，不部署背景或默认stage资产。

callback设双方源X400、subjectZ600/targetZ604、subjectY-40、双方source整数同步、统一view投影、preassigned target0/Vz0，只执行1个完整 `driver.StepOneTick`。完成后断言subject精确sourceZ600/Vz0/viewDeltaZ0、target源604/目标槽0/两实体。wrapper正常结束实际检查World对象、占用slot和logic引用pool均0，并恢复原有singleton/内容发布。该测试没有新trace文件写出，没有切换或保存原Scene；它是原Editor内的EditMode完整Driver证据，不是原Battle Scene自然Play。

TestContext.Progress标记未包含在MCP最终result.output，报告不把缺失标记当独立trace；结论来自具名方法实际通过的断言。完整Driver没有导出全World、全部RNG或同初态正式根EXE trace。

## 保留与审阅

[final-authority-diff-and-editor-state.json](final-authority-diff-and-editor-state.json) 保存两个脚本对本ID before的精确diff、当前authority文件SHA、正式/暂存OID875 DAT身份、原Editor终态与四保护SHA。最后后验非Play/无测试、原Battle clean/root11、Console0error；Battle/Menu/GameConfig/ProjectBattleModeConfig四SHA均保持。

独立只读代理确认生产只有source共同门/两Z读取，raw槽和source不完整fallback保留，X/Y及其它分支/旧阴影修复不变；四参数同步两个整数域、检查精确source与独立view倍率，finally注销两对象。它未运行测试；source不完整fallback本轮是静态保留证明。自然同初态、正式EXE、GPU/真人键和其它未覆盖分支仍未知，仅实际首差/相关owner改变才回访，不扩大角色矩阵。

交付记录验证：`pwsh -NoProfile -File Tools/Validate-ChangeLedger.ps1 -RepositoryRoot I:\GitHub\Unity_GAS\gameplay-ability-system-for-unity` 实际退出0/PASSED，1272份Record、diff中23份受管脚本；`git diff --check`退出0。当前336B44命名范围219份Record/58份未关闭已只读重计，与总表相符；未关闭记录按REUSE44/TRIGGER14调度，P0/DEP/ONE均0，不代表58个必跑任务。本次补充只维护证据口径，没有再运行Unity测试。
