# Q07/D-024 共用追踪规则坐标入口

状态：SCOPED_FULL_DRIVER_PASS / NATURAL_RUNTIME_PENDING。总目标NTSD28-UNITY-BATTLE-REALIGNMENT-001；当前336B44总表Q07/D-024。新12项原Editor六个预期RED、共用修复后18/18 GREEN及唯一完整生产Driver1/1通过，原Scene clean/非Play/Console0error/四SHA稳；[报告](../../../artifacts/diagnostics/NTSD28-336B44-Q07-D024-TRACKING-RULE-POSITION-20261005/REPORT.md)。必要ONE完成按REUSE；自然原Scene/正式根同初态/GPU等保持未知，父Q/总目标开放。下方候选/待验/ONE1均为建立时过程快照。

## 来源与现状

用户要求所有战斗实体位移/碰撞按正式画面比例，并统一换算入口、禁止修改DAT。正式336B44 native_ai.cpp::NativeAi28::step_non_character_hit_fa 的1/3/2/4/12/14以及7分支使用源整数X/Z：1死区7/加速度0.3，3死区10/0.17，其余死区5/0.4，4在X严格±30/Z严格±10且Y严格0..80时提前停止/action60/目标E4=100。simulation_tick_driver.cpp调用且build.ps1纳入该CPP。

现有source排序入口只修目标排名，未覆盖追踪死区与回收距离。当前Unity LF2Entity相应读view X/Z；固定2048/1333与1152/730会把源gap4/6/10分别投影为超过5/7/10，源X差29/Z差9回收条件也可被拒绝。正式875/frame50、700/frame54、518/frame1、219/frame0、907/frame190、207/frame115分支均在当前indexed DAT；124/type4/frame40从天天frame248的OPoint直接出生，生产BattleLogicReferencePool.CreateNewObject把type4创建成LF2Weapon，专门12入口确为活。初次只筛type1/2无4/12不能推断全部武器无可达入口。

## 准确改动边界

- `Assets/NTSD/Scripts/Test/Editor/NTSD28Q07NonCharacterHitFa7EditorTests.cs`：新增正式内容追踪死区参数方法（恒等/固定、三种阈值、type4武器12、正向与负向边界、目标source不完整fallback）及正式219回收严格X/Z边界方法；8+4共12项先RED。同档旧875源深度4参数与active/raw两方法邻例只在GREEN作共用入口回归。可在具名GREEN后增加一个同文件受控完整Driver方法，使用既有wrapper与旧scenario诊断边界、读取当前LoganRuntime及项目mode Asset；显式改成518/frame1源X400/Z600与目标604，单生产tick后测精确源Z/Vz/view增量；不借旧EXE标签证明新版自然结果。
- `Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs`：仅在真实RED后新增一个internal共用成对X/Z读取方法，active target且双方source历史完整且当前数据为非角色时读source整数，否则整对保留原view/raw。通用1/3/2/4/12/14/非角色7调用；4回收与其后追踪共用同一结果。保留type0、目标选择/扫描、raw空槽、HP门、所有Y/action/速度常量/边界写者、旧平台阴影和7修复。
- `Assets/NTSD/Scripts/Animation/LF2Objects/LF2WeaponFrameLogicResolver.cs`：专门12调用同入口；4同入口但source不完整时继续其旧GetRenderZInt fallback，不改变原fallback取整。只有当前正式可达124/type4的12作为本轮武器证书，不能把不存在的正式4入口宣称自然验收。

## 生命周期、验证与回滚

不增manager/queue/worker/pool/cache/renderer，不改十一阶段关闭。测试复用已有World/catalog/logic pool，finally注销，完整Driver复用现有scope有序关闭及发布恢复。先存三个当前脚本字节/SHA/Git状态/四保护SHA和权威SHA；新输出CreateNew。生成Editor build、MCP原Editor刷新、精确新12项RED，首差不成立则不改生产；修复后12项加旧6邻例，必要一个完整Driver消费者，Console/Scene和四SHA后验，Change Ledger/diff检查。无全套SelfCheck或全角色矩阵。

原Scene自然输入、正式根同初态、GPU与未覆盖分支若无证保持未知，不因此无限扩测。不改DAT/PNG/WAV/Scene/Prefab/ProjectSettings/GAS/非战斗。回滚仅本ID增量前向撤销并参照before原件，需批准才使用破坏性Git；绝不恢复HEAD覆盖原7/阴影等dirty工作。

原件：artifacts/diagnostics/NTSD28-336B44-Q07-D024-TRACKING-RULE-POSITION-20261005/before-manifest.json。

2026-10-05完整Driver方法脚本前细化：选已声明518/frame1，不能选875/frame50（其dvz550会在后继frameMotion把错误Vz归零）。同测试档新增IndexedTrackingSourcePairSurvivesOneFullDriverTick；先按既有parser读正式518定义，旧wrapper仍提供schema/seed/Stage23及部分target初态并读取当前LoganRuntime/项目mode Asset。callback注销wrapper直接new且没有pool借用的slot1旧875，按既有preplaced模式new正式518、加载frame1并初始化definition/armor、HP/team/source/view/target，注册同slot1；更新该诊断Roster槽CharacterId与StableId，其余绑定保持。明确world仍只有target99/subject518两对象，并断言frame无dvz/OPoint。仅执行一个完整生产StepOneTick后检查精确源Z600/Vz0/viewDeltaZ0、target604/目标slot0与实体数；既有scope归零三计数并恢复发布。不改scenario文件，不增临时trace/请求/资源，不改变关闭主序列。先等18项GREEN终态后再写/运行本唯一消费者，不重复18项。
