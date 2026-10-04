# NTSD28-336B44-Q07-C053-TOBI-NATURAL-SCENE-001

2026-10-04 退出残留补证（脚本二次修改前登记）：原Editor14tick结果已出，正式源/Unity八个映射及状态字段×14=112/112一致，OID251 tick11/slot50/action51；结果显示退出非Play/唯一Scene clean/四SHA稳，但 `liveDriversAfter=1` 是原Battle Scene序列化的 `SimulationTickDriver` 对象，既有结果未读取它是否仍绑定 World。为完成本Task已有“无World残留”出口，只在同一测试脚本新增一次性 EditMode 菜单读回：按原结果中的Battle SHA核对当前Scene，枚举现有Scene Driver 的 `World` 非空数与 Pool 组件数，写唯一新JSON，不创建/销毁对象、不重跑战斗、不改原结果；新增路径拒绝覆盖。若Scene状态/哈希已变，则只报告不可归因，不冒称关闭。生产、DAT、Scene、非战斗不改。

状态：`VERIFIED_SCOPED_SCENE`。父目标 `NTSD28-UNITY-BATTLE-REALIGNMENT-001`，G1/BATCH-04/Q07/C053。仅建立正确 OID0 Tobi 普通跳跃→`hit_Fa:510`→OID251 的原 Unity Battle Scene 定向证据，不改生产战斗规则、DAT 数据、图片、Scene、Prefab 或非战斗逻辑。

权威与前置：当前根正式 EXE SHA-256 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3`；对应 playable `GameSession28` 和 `SimulationTickDriver28` 的源探针在正式背景1诊断域使用 OID0 Tobi/X600/Y0/Z400、OID2 对手/X1100/Y0/Z400、seed682973786/mode0；tick1–4 按正式跳跃，tick8–10 按正式防＋右＋攻。正式源/根已证 Tobi tick8 入510、tick11 生OID251/slot50/action51，40tick所选640/640同；`next:0`是停当前帧，不得把受控 action0 双命中写成此自然链。Unity保留自有地图与比例投影；不引入原版背景。

Unity 现状：`NTSD28Q07C040NaturalScenePlayProbeEditor`、`NTSD28Q07C052HayatePhysicalBattlePlayProbeEditor` 已有 Battle Scene Play clone 预配置、正式内容 roster、离散 `FrameInputSet`、生产 `StepOneTick`、清理/场景哈希范式，但没有 Tobi OID0→OID251 原场景证据。已有 C052 桥接确认正式跳跃使用 legacy `Defend`，正式防＋右＋攻使用 legacy `Attack|Right|Jump`。本任务不更改这些旧探针。

声明代码路径：新增 `Assets/NTSD/Scripts/Test/Editor/NTSD28Q07C053TobiNaturalBattlePlayProbeEditor.cs` 及 Unity 自动生成 `.meta`；仅由独立 Editor 菜单触发原项目测试。新增独立、拒绝覆盖的结果在 `artifacts/diagnostics/NTSD28-336B44-Q07-C053-TOBI-NATURAL-SCENE-001/`，无请求文件，不触碰旧结果。其余修改仅本 Task、Change Record、Ledger、STATE、当前 handoff/总表状态。

实施：在单一 clean `NTSD_Battle` 原 Editor 中调用唯一菜单，Play clone 初始化前配置 roster OID0/2；待生产 World 启动并暂停稳定后，将两角色按正式源坐标/HP/MP/方向/团队/seed/mode0设为同初态。连续14个完整生产 Driver tick 输入上述离散键，并记录每 tick 的当前输入相位、Tobi action/Y、OID251 slot/action/X/Y/Z与自然出生 tick。之后离线对照正式源/根 CSV 确认首次不同 tick，不用测试特判修生产；退出 Play 后检查唯一Scene clean及运行前后 Battle/Menu/GameConfig/ModeAsset SHA。测试探针只读正式内容、写新结果，不保存 Scene。

验收：新增脚本生成 Editor 工程0 error，原 Editor 导入编译0 error；正式内容根 `Assets/NTSD/Content/LoganRuntime`；14 tick 的所选状态逐 tick 与上述正式源/根 CSV 对照，重点 tick8/action510、tick11/OID251/action51/slot及出生坐标；清理后非Play、无新借用/World残留、四保护哈希稳定。若并行写入导致哈希变化，则保留原件并把场景保护门标为未通过，不保存/回退别人的改动。此限定出口不证明物理键真实设备、40tick全World、自然action0双Uj或整个Q07完成。

回滚：若测试脚本失败，只停止该独立菜单入口并保留RED原件；任何删除新脚本或结果须另按文件操作审计与用户授权执行，不动既有 Scene/资源/用户修改。
