# Q07/C053 正确 OID0 Tobi 自然出生：原 Battle Scene 证据

状态：`VERIFIED_SCOPED_SCENE`。只关闭 OID0 普通跳跃输入链在原 Unity Battle Scene 的前14个完整生产 Driver tick 与 OID251 自然出生子门；Q07/C053/Q12和总目标仍开放。

权威为根目录正式 `NTSD2.8-Logan.exe` SHA-256 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3` 及对应 playable live source。正式源/根在诊断背景1域使用 OID0 Tobi/X600/Y0/Z400、OID2 对手/X1100/Y0/Z400、seed682973786、mode0，tick1–4跳跃、tick8–10防＋右＋攻；已有正式根LFR回放PASS及所选字段40tick×8字段320/320同。该背景仅用于正式诊断；Unity仍用项目自有地图，没有引入原版背景或背景DAT。正式源[逐tick CSV](../NTSD28-336B44-Q07-C053-TOBI-PHYSICAL-REACH-001/run-d/jump_from_zero-ticks.csv)在tick8进入action510，tick11生成OID251/slot50/action51。

本包只增加独立 Editor 测试脚本，菜单从原项目唯一clean `NTSD_Battle` 进入Play clone、启动前配置OID0/2正式内容，按现有输入桥接提交离散 `FrameInputSet`，由生产 `SimulationTickDriver.StepOneTick`推进14tick。正式物理跳跃对应Unity legacy `Defend`，正式防＋右＋攻对应legacy `Attack|Right|Jump`；这里验证的是战斗逻辑输入包，不是键盘设备事件。新脚本已被原Editor导入并编入`Assembly-CSharp-Editor.dll`；生成Editor工程构建exit0、0 error/299 warning。最初一次生成工程0错时新脚本尚未被项目文件收录，不作此脚本编译证据。

[原Editor结果](tobi-jump-natural-01.json)为`MEASURED_COMPARE_PENDING/DONE`且14行齐全、OID251 `childBirthTick=11`；[独立配对](tobi-jump-natural-01-paired.json)按正式CSV与Unity实际JSON逐tick比较映射输入包、输入相位、Tobi动作/Y、子体槽/动作/X/Y，共8字段×14tick=112值，差异0。tick8 Unity action510/Y-18，tick11 OID251/slot50/action51/X600/Y-110，tick13子体action2/X602/Y-100，均与当前正式源对应行同。Unity另记录子体source Z401，但正式CSV未提供这个字段，未列入112项同态结论；也未观测Unity的原生输入采样mask、全World、全部RNG状态或正式EXE画面。

Play退出后原Editor非Play、唯一Battle Scene clean/rootCount13，本次Battle/Menu/GameConfig/ProjectBattleModeConfig四文件SHA前后相同；Battle SHA `8CC5614574321908CDB89BAEB6BE60E41616EB3CA6F6DDEF35B34F723269047E`。[独立EditMode残留读回](tobi-jump-natural-01-postplay.json)核对同一Scene SHA、1个原Scene序列化的Driver、其中绑定World数0、Pool组件数0，状态`SCOPED_PASS`。原Play结果中`liveDriversAfter=1`因此是已有Scene Driver，不能误读成泄漏。未保存、覆盖或回退Scene/用户并行修改。

本结论不把静态DAT的`next:0`当作进入action0：当前336B44帧机把它解释为留帧，此自然链在已检路径后续转action60～63。受控OID251/action0双Uj只属于另一受控初态，不能由本14tick自然出生推断；真实键盘物理按键、40tick剩余状态/全World、其它交互路径与Q12最终整合仍需按总表推进。DAT数值、角色图、Scene、生产战斗逻辑及非战斗逻辑均未修改。
