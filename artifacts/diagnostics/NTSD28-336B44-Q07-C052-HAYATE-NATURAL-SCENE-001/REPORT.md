# Q07/C052 疾风→417→211 原 Battle Scene 限定验收

状态：`VERIFIED_SCOPED_NATURAL_OPPOINT_SCENE / PHYSICAL_SELECTION_PENDING`。正式规则身份为根 `NTSD2.8-Logan.exe` SHA-256 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3`及对应 playable live source；[前置源／根报告](../NTSD28-336B44-Q07-C052-HAYATE-OPPOINT-REACH-001/REPORT.md)。此包不修改DAT、角色图、Unity生产、Scene或非战斗。

在原项目现有 Unity 2022.3.62f3 Editor、干净的原 `NTSD_Battle` 场景中，临时Play副本建立 OID73/action160/X500/Z400/team1、两名OID2/action0/X589与X619/Z400/team2，mode0/difficulty0、seed682973786，三人中性输入。原Editor导入新探针、Editor程序集含新类型，无本轮编译错误；请求消费后生产 `SimulationTickDriver.StepOneTick` 连续推进30tick，结果[原始JSON](hayate-near-scene-v1.json)状态`MEASURED_COMPARE_PENDING`、`DONE`、exitedPlay=true、sceneCleanAfter=true。

[离线逐字段比较](comparison-near-v1.json)按每tick 19个选定字段共 **570/570 零差**，包括三角色动作、两目标HP与X、OID417/211数量／首槽／动作／源规则XYZ。关键时点：tick4 OID417/slot50/action49/X531/Y-34/Z401；tick22 OID211/slot51/action160/X574/Y0/Z402；tick24 OID211/action161、两目标HP500→420/action203，均与正式源码相同。正式根回放此前的60tick只对其导出的1140字段零差，第三目标初始朝向无法由根LFR覆盖，完整初态／全World同态不主张。

Play退出后原Scene clean；Menu、Battle、GameConfig、ProjectBattleModeConfig四文件SHA前后相同。探针只导出逐tick后状态，没有导出Unity逐hit内部候选／rest，不能把HP结果当内部字段逐项一致。物理按键选到action160、完整World/表现/音频及其它正式内容入口仍待；C052、Q07、总目标保持开放。聚焦验收只运行本例，未重复全量SelfCheck。

下一可达性线索（仅静态，未作物理输入证明）：正式 `c/hay/hay.dat` 的疾风 action212/213/214 等帧写有 `hit_Fa:160`；当前 playable `input_routing.cpp::route_combo_fields` 在水平攻击组合态达到门槛时读 `hit_Fa`。应从这些正式帧的实际按键序列建立受控源／根／原Scene入口，不能把本包直接设置初始action160解释成玩家已按出该招。
