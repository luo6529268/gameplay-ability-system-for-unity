<!-- CHANGE-RECORD
id: NTSD28-336B44-Q07-D024-REPEATED-DIRECT-MOTION-001
status: VERIFIED
change-kind: CODE
code-path: Tools/NTSD28Q07Diagnostics/d024_repeated_direct_motion_probe.cpp
code-path: Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q06FrameMotionTailEditorTests.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28Q07D024Kind8BattlePlayProbeEditor.cs
authority: 336B44 formal root/playable frame motion and D-024 fixed full-view ratio
evidence: docs/ai/TASKS/NTSD28-336B44-Q07-D024-REPEATED-DIRECT-MOTION-001.md
-->

# NTSD28-336B44-Q07-D024-REPEATED-DIRECT-MOTION-001

Created before script edits. Current formal OID92/action580 `dz:4 wait:20 next:580` and Unity's repeated physical-integer-base writer suggest accumulated fixed-view Z drift. This is a candidate, not a confirmed runtime difference. The [Task](../TASKS/NTSD28-336B44-Q07-D024-REPEATED-DIRECT-MOTION-001.md) defines authority, preconditions, exact ownership, invariants, validation and rollback. First code step is a source-only diagnostic tool; production and test scripts remain untouched until formal source/root and Unity RED establish the gap.

2026-10-03 `CODE_WRITTEN`：只新增 `Tools/NTSD28Q07Diagnostics/d024_repeated_direct_motion_probe.cpp`。它用正式 OID92/action580、另一名远距OID2、mode0/background1/Z400、seed `0x28A55A5A` 和24个中性完整 tick 导出整数/精确规则 Z 与 LFR。未改 Unity 生产/测试脚本、DAT、Scene 或其它资源。编译、正式根复播及原 Editor 首差尚待；诊断出错时保留失败原件并前向修正，不能据静态推算动生产。

2026-10-03 正式可达与测试补记：诊断编译 exit0/无输出；正式 playable `GameSession28` 的OID92/action580在background1/Z400连续24完整tick保持580，规则Z400→496，每tick+4，并导出LFR。当前336B44根正式EXE以该LFR独立headless回放，报告`passed=true/failureCode=0`；其初始+24tick的action、整数Z、精确Z对源码75/75零差（根trace另有终端tick25，不计入）。随后仅在已声明的 `NTSD28Q06FrameMotionTailEditorTests.cs` 增加正式同SHA DAT/24次共用帧运动的默认与2048×1152比例断言；生产脚本未改，原Editor RED待运行。测试使用同一action580，允许小于1输出像素的投影误差，避免把单tick相等当成长期比例。

2026-10-03 原Editor RED与生产修复：原项目EditMode中identity视口通过、2048×1152比例在tick4首次超过1px；把断言移到24次尾后原Editor重跑，`maxViewError=7.416438356164349`px，规则Z仍496。保留两次RED JSON。`LF2Entity.ApplyNativeFrameMotionTail` 的直接 `dx/dz` 只在源规则坐标已初始化时改为从物理精确坐标累加每次按D-024缩放的DAT位移，保留未初始化源域的旧整数基底；源规则坐标、取整、速度清零、延迟系数、帧/平台pass顺序均未改。既有平台后直接位移夹具在源域已初始化时改断言精确平台位置作为画面合成基底，未初始化时继续断言整数基底；另加X轴左右各24次比例用例。该修改仅覆盖当前声明的两份Unity脚本，不改Scene/DAT/资源。生成Editor工程编译exit0、0错误、304警告；原Editor GREEN、相邻测试、SelfCheck和原Battle Scene仍待。

2026-10-03 聚焦补证与Scene探针预登记：原Editor刷新后帧运动类33/33 PASS（包含正式OID92连续Z、合成左右X、平台后合成和12条历史原生夹具）。为完成Task原Scene真实Driver出口，在修改前增列现有 `NTSD28Q07D024Kind8BattlePlayProbeEditor.cs` 的有限诊断分支：独立 `caseName=direct-motion`、独立结果目录，同现有干净Scene/四文件SHA/有序关闭守护，OID92/action580和远距OID2完整24 tick。旧kind8分支及其既有结果不改语义、不重跑。该诊断扩展不得变更生产、Scene、资源或DAT；若探针无法隔离旧分支则暂停。

2026-10-03 `VERIFIED`（仅连续直接位移共同区域）：原Scene初始Z400第一轮0～20tick根/Unity 336/336声明字段零差，tick21起项目地图将源Z限为481.59722222222223而正式背景1继续484；该异常原件保留，关闭零残留与四SHA稳。正式Z300诊断首tick受正式背景下界钳制，原件亦保留。诊断工具及Scene探针随后仅增明确Z380参数，不改变旧kind8请求语义；正式源码/根Z380→476的初始+24tick三字段75/75同，原Scene同条件两槽八字段400/400同、视图比例最大误差约1.14e-13px，有序关闭World/槽/池借用/活动对象/Sprite均0、回干净Menu、四SHA稳。原Editor完整SelfCheck新结果`PASS`，MCP菜单回执虽超时但结果文件时间和内容新鲜；旧4字节PASS临时结果在菜单覆盖前复制入本包证据，未删除项目资产。源/根Z400/380、Unity RED/GREEN、首轮边界、第二轮通过及所有证据限制见[报告](../../../artifacts/diagnostics/NTSD28-336B44-Q07-D024-REPEATED-DIRECT-MOTION-001/REPORT.md)。实际生产脚本只改`LF2Entity.ApplyNativeFrameMotionTail`的源已初始化物理精确X/Z累加；测试脚本是帧运动Editor类和旧kind8探针的独立opt-in分支，Tools工具的初始Z参数为诊断；未动DAT/Scene/相机/非战斗。Q07/D-024全实体、地图边界后画面残差、正式EXE GPU和Q09/Q12仍待；任何回滚仅对本包精确增改行审查反向补丁。
2026-10-03 同包前向重开 `IN_PROGRESS`：先前Z380共同区证书保持有效；Z400场景保留的触边后物理残差经现有生产路径只读追到 `NTSDEntityRuntime.ClampStageZ`：源Z在边界481.5972222保留小数，而下一帧 `ApplyNativeFrameMotionTail` 源Z从整数481重基为485；画面旧增量仍按原始`dz=4`，源精确实际增量只有3.4027778，Clamper再投影源差后每tick余0.5972222×1152/730≈0.942466px。此为同一共用帧尾的未闭小数前态分支，不是原版背景数据要部署。先增共用X/Z小数源坐标RED，生产修为投影本tick规则精确坐标实际变化量，保留无源域旧路径、所有规则/时序/地图边界；随后更新故意物理/源锚点不同的既有单次夹具期望，并跑相邻33+、SelfCheck、Z380及Z400原Scene。未获新证据前本Change不再称全限定已闭。

2026-10-03 小数分支 RED→代码已写：原Editor具名左右方向两例均在第一tick物理X多0.91756272401426px而失败，完整RED JSON已保存；Z400原Scene另证边界后纵深残差。现 `LF2Entity.ApplyNativeFrameMotionTail` 对源已初始化X/Z先依原版整数基底写新精确源坐标，再以新旧精确源差乘共用世界比例累加物理位置；未初始化源位置仍走原物理整数基底。没有改变 DAT、Y、帧/平台pass、速度清零、源规则取整和项目边界。两个既有单次夹具因故意设置物理/源锚点不一致，已将物理期望改按本tick实际源差投影；新小数测试同时覆盖左右X及Z。生成/原Editor编译、聚焦GREEN、SelfCheck和原Scene双初态复验待。

2026-10-03 最终 `VERIFIED_SCOPED_DIRECT_MOTION`：生成Editor工程0错/306警告；原Editor帧运动35/35 PASS，包含小数源左右X/Z；改后原Battle Scene Z380两槽8字段400/400同根、画面比例最大误差约1.14e-13px，Z400首20tick336/336同、tick21起仅项目地图规则Z边界与正式背景分叉，物理Z停760、同项目源域比例误差约1.14e-13px（原2.827397px残差归零）。两个Scene运行各自有序关闭/零借用/四SHA稳、回干净Menu。完整SelfCheck于17:52:37写新PASS；MCP菜单回执曾超时，18:00已恢复并见Menu clean，但根对象从先前8变9，新增内存`BoundaryWallManager_AutoCreated`，写入者未证，保留不删；这不推翻两个Scene探针的World/池零残留。旧临时结果覆盖前已复制并留档；无项目文件删除、无DAT/Scene/地图/相机/非战斗改动。[最终报告](../../../artifacts/diagnostics/NTSD28-336B44-Q07-D024-REPEATED-DIRECT-MOTION-001/REPORT-V2.md)。本Change只关闭共同帧尾的连续直接位移及小数重基子门；Q07/D-024其它实体出口、Q09像素、Q12整场仍开。
