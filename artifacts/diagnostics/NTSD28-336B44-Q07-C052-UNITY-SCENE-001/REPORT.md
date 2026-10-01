# Q07/C052 原 Battle Scene effect21 正间隔生产首差

状态：`VERIFIED_SCOPED_FIRST_DIFFERENCE`（仅原生产代码修复前的受控 Scene 证据）。正式 336B44 playable 的 OID211/action161、OID2/X500、OID2/X530 完整 `GameSession28::step` 第1 tick 两名受害者 HP 均500→420、action203、对同攻击者 relation rest 均44；命中序列为首目标 applied、同目标正rest rejected 且不终止、第二目标 applied。[正式源码原件](../NTSD28-336B44-Q07-C052-POSITIVE-REST-REACH-001/a161-x500-x530-v1/source-ticks.tsv)。正式根 EXE 的三实体同条件尚未加载；源为正式对应 playable 重新链接的诊断，不称根同态。

原项目当前 Battle Scene（非另建项目）唯一 Scene clean，原 Editor 导入新探针后，OID2 双角色及生产工厂创建的正式 OID211/action161，mode0/difficulty0、seed682973786、同源 X500/500/530、Z400、中性输入；生产 Driver 显式推进3完整 tick。Unity 第1 tick 第一目标 HP420/action203/rest44，第二目标 **HP500/action0/rest0**；第2 tick 第二目标才 HP470/action203，与正式源第1 tick HP420/action203 构成最早可观察差异。[近距原件](x530-scene-v1.json)。源 slot 为 actor0/targets1,2，Unity为actor2/targets0,1；只比较各目标关系与结果，不报告逐 slot checksum 同态。

远距 X650 独立 Play 同初态/3tick：第1 tick 第一目标HP420/rest44，第二目标始终HP500/rest0；[远距原件](x650-scene-v1.json)。近远两次均结果`DONE`、已退出Play、Scene clean；Menu/Battle/GameConfig/ModeAsset 四 SHA 在各次前后完全相同，Battle SHA `93448372834A1BEAF2C9ACD90E2EF17E2EA487E19F601B9815A907A974D7BF60`。测试请求首次导入有CS0165，分离两次roster检查后原Editor DLL包含新探针；此编译错误和修复保留在 Change Record。请求目前仅置 `requested:false`，无自动删除；首案旧请求 payload 已封存于 [request-x530-v1.json](request-x530-v1.json)。

Unity runner 在 effect21/current state18/19 时不查 rest 就结束整个攻击；上述源有正rest的第二条同目标候选只拒绝当前候选而继续，因此此数据支持在共用提前终止门补 `victim_rest==0`。本报告是**修复前**证据；生产脚本尚未修改，修复/回归另由 `NTSD28-336B44-Q07-C052-POSITIVE-REST-CONSUMER-001` 管理。自然角色操作→OPoint出生、根正式EXE同态和整场 Q07 仍待。
