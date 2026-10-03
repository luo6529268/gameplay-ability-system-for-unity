# Q07/F02 原 Battle Scene Z400 自然链限定对照

状态：`SCENE_TICK_TAIL_PAIRED / INTERNAL_EVENT_AND_VISUAL_PENDING`。正式权威为 SHA-256 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3` 的根 EXE，同版 playable `GameSession28::step()`；正式背景1/Z400只用于构造双方共同纵深对照，Unity始终使用自己的地图。正式源与根初态+128tick 3096/3096选定字段一致、F02 tick39，详[正式报告](../NTSD28-336B44-Q07-F02-STAGE-DOMAIN-001/REPORT.md)。

原 Editor 的唯一 `f02-kind10-scene-20261003-03` 请求进入原项目 `NTSD_Battle.unity` Play 克隆，使用正式暂存 LoganRuntime 内容与同 seed/mode0/初态：鸣人 OID2/X200/Z400/action0，多由也 OID36/X530/Z400/action243，武器 OID600/type4/X190/Y-20/Z400/action0，耐久 `weapon_hp=250`；攻击由现有输入桥投到正式攻击位，按相同条件节奏提交。没有中途手写动作、命中、速度或持有关系。测试探针设置并核验初始 owner/team、计数、世界时钟、输入相位和投影，随后通过生产 `SimulationTickDriver.StepOneTick` 完成45个逻辑tick；原始连续JSON为该run目录`00001.json`～`00055.json`，最终快照 `00055.json`。

初态+45完整tick，三槽各17个选定字段（OID/type、动作/state、源整数X/Y/Z、Vx/Vy、关系/父子、武器耐久、HP/team、抓取源/目标）共 **2346/2346** 与336B44根LFR逐字段一致，首差无；算法与字段表见[独立比较](f02-kind10-scene-20261003-03/paired-comparison-v2.json)。tick17武器落地、18鸣人拾取关系4、24轻投、30武器释放action41/Vx55、31武器回到state1000且Vx51.401869、39武器tick尾action41，与正式版相同。正式根同时直接记录tick31～35的kind10 `applied` 及tick39帧内`40→41`；Unity本次只记录tick尾，故**不能把内部事件归因和帧内转移写成Unity已直接观测**。

源规则与物理显示域未混用：完整样本的源精确X与正式根最大差约 `1.14e-13`，Unity采用X比例 `2048/1333=1.536384096`、纵深比例 `1152/730=1.578082192`；由未取整源位置计算的实际画面坐标最大残差X `0.8133` 像素、Z `0.2329` 像素，位于整数像素同步误差内。这里检查的是逻辑表现投影值，**没有取得 Game View GPU 像素截图**。

结果`CAPTURED/DONE`，Editor已退出Play并恢复单一、干净的原Menu场景，无live Driver World；Battle/Menu/GameConfig/ProjectBattleModeConfig四个受保护文件SHA前后一致。run-01旧Z542地图限制与测试武器耐久漏初值的失败原件、run-02启动超时原件均保留。原Editor近期有其他资源/菜单变化，这两轮没有覆盖、回退或删除这些用户文件。此子门已经证明原Scene同条件**tick尾状态**限定一致，但成功出口要求的Unity逐hit kind10、帧内40→41、真实物理按键、Game View及完整池借用数据仍待；F02/Q07/Q12与总目标继续开放。

验证边界：原Editor导入并实际运行新探针，未出现脚本编译错误；第一次显式纳入新文件的生成Editor项目构建0错。之后一次生成工程重编因并行菜单任务的 `MenuCarouselTextEffect.cs` 尚未进入生成的 `Assembly-CSharp.csproj` 而失败，未归因于本F02文件；原Editor真实Play结果高于该滞后工程文件。当前全工作区 `Tools/Validate-ChangeLedger.ps1` 返回1，唯一ERROR为另一菜单视觉Record声明了不在治理目录的 `Assets/NTSD/Resources/UI/MenuCarouselText.shader`，本F02两路径无ERROR；该跨任务账本冲突未在本包修改。F02改动范围的 `git diff --check` 通过。没有为本小范围案例重复全量自检或多角色测试。
