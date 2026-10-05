# 非角色 hit_Fa7：源深度死区

状态：`SCOPED_FULL_DRIVER_PASS / NATURAL_RUNTIME_PENDING`；总目标 `NTSD28-UNITY-BATTLE-REALIGNMENT-001`，当前336B44总表Q07/D-024。四参数RED已证固定gap±4误加速度；修后原Editor6/6及单个完整Driver方法1/1通过，源/view同态、正常关闭零残留、生成0错、Console0error/原Scene clean/四SHA稳。必要ONE完成；[报告](../../../artifacts/diagnostics/NTSD28-336B44-Q07-D024-HITFA7-SOURCE-DEPTH-20261005/REPORT.md)。原Scene自然输入/根EXE等仍未验，只在相关改动或实际首差回访，不重开旧raw空槽任务或角色矩阵。下方ONE_PENDING/尚需为建立时过程快照。

## 来源与原状

正式根EXE为336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3。当前playable闭包的 `native_ai.cpp::NativeAi28::step_non_character_hit_fa` 495～526以源整数position.z作±5死区、写native motion.z±0.4；`simulation_tick_driver.cpp`416调用，playable build.ps1纳入native_ai.cpp。非角色7重复X的两次0.7、Y门槛/速度/帧和clamp均保持。

Unity `LF2Entity.RunNonCharacterHitFa7FrameLogic` 的self/targetZ直接读view整数；当source380/384、当前2048×1152投影时，view整数599/605，候选差异为源距离4不加速、view距离6错误加0.4。后续 `CharacterMechanics.StepNonCharacterBattleLogic` 把Vz同时加source精确Z并按现有倍率加viewZ，故必须检查精确源位置，不能只检查仍截断为380的源整数。

正式OID875/type3的c/ank/a/atk.dat含55/56 hit_Fa7；安可511/512/513可OPoint生成875/action50继而next55。它证明正式内容分支可达，不证明自然输入恰好产生本案间距。原Scene自然同初态/根EXE直接运行尚未取得。

## 准确脚本边界

1. `Assets/NTSD/Scripts/Test/Editor/NTSD28Q07NonCharacterHitFa7EditorTests.cs`：新增 `RealOid875DepthDeadZoneUsesSourceCoordinates`，四参数仅identity/gap4、fixed/gap4、fixed/gap-4、fixed/gap6；正式875/action55和99 active target，X相同/Y-40。双方用已有World投影及SetSourceRulePosition/SyncSourceRuleIntegerPosition建立同态。检查Vz、Y/action/目标slot/实体数和一次既有非角色积分的精确source/viewZ；finally解绑两对象，不生成资源/临时trace文件。修后仅另跑既有active-target和empty-slot两方法作共享owner邻例。
2. `Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs::RunNonCharacterHitFa7FrameLogic`：只有在上述参数确认真实RED之后，将active target且双方源历史完整的一对Z操作数切到SourceRuleZInt。否则保留原view/raw操作数；不初始化空槽源历史、不混合source与raw域，不改目标选择、X/Y、速度常量、生成、其它hit_Fa或全局坐标helper。

DAT/图片/Scene/Prefab/ProjectSettings/模式Asset、当前1.5倍图片、固定全背景、GAS和非战斗不改；不得新建服务、manager或关闭阶段。现有World/entity owner与注销路径保持。

## 验收、风险与回滚

2026-10-05完整Driver消费者脚本前增量：仍只在已声明的 `NTSD28Q07NonCharacterHitFa7EditorTests.cs` 新增 `RealOid875SourceDepthDeadZoneSurvivesFullDriverTick`，复用现有 `WithLoganScenarioForReplayTests` 与旧三tick875夹具的加载/注册形状；其5EDA元数据只用于旧夹具shape校验，不能裁决新版规则。运行时必须使用当前正式暂存LoganRuntime与项目模式Asset，内存scope结束恢复所有既有singleton发布，磁盘DAT/资源/Scene不写。callback明确改成当前合法源X400/Z600与604、subjectY-40、双方source已初始化、view经统一World投影、preassigned target0/Vz0；Stage23边界是测试夹具，不加入默认stage资产。只执行一个生产完整Driver tick（非直接调用分支/physics），检查subject source精确Z600/Vz0、target源604、view增量0、target slot0/两实体，并由已有driver scope有序关闭归零。不从EditMode完整Driver推出原Battle Scene自然键或GPU；不跑之前六例。本必要出口为ONE1，旧57份不重排。

先存两个精确脚本的当前字节/SHA/Git状态和四保护SHA，再写四参数测试；生成Editor编译、MCP原Editor刷新及具名4项RED后才生产最小修复。同4项GREEN＋现有两方法邻例，Console0error、原Scene clean/nonPlay及保护SHA稳；运行Ledger validator和diff检查。只证明本分支与一次积分，完整Driver、自然输入及根EXE同初态仍按证据报告，不能据此关闭Q07或总目标。没有全套SelfCheck或全角色矩阵要求。

风险为source历史不完整时操作数混域，故必须双方共同门；保留raw槽语义的旧证据边界。回滚只前向撤销本新方法和本次Z读取增量，参考本ID before原件；不git restore/reset/clean、不回退既有阴影等dirty工作，失败原件保留。
