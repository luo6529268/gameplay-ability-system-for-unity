# Q07/F02 原 Battle Scene 保存后定向回访（2026-10-03）

状态：`VERIFIED_SCOPED_UNITY_INTERNAL_EVENT`。本轮只使用已有 Editor 请求探针，没有修改 Unity 生产脚本、DAT、图片、Scene 或非战斗文件。正式规则身份为根 `NTSD2.8-Logan.exe` SHA-256 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3` 及其对应 playable live source。

用户确认 Menu 已保存且 Editor 空闲后，[预检](../f02-preflight-20261003-08/00002.json) 直接从原 Editor 读到单一 `NTSD_Menu.unity`、`isDirty=false`、非 Play、非编译，四个受保护磁盘文件前后 SHA 一致。随后只运行本 F02 案例：[最终原始报告](00056.json) 是原项目 Battle Scene 的 45 个完整生产 `SimulationTickDriver` tick，mode0、seed `0x28A55A5A`、正式暂存 LoganRuntime，初始鸣人 OID2/X200、Tayuya OID36/X530、武器 OID600/X190、共同规则 Z400。正式背景1只用于 native 同域诊断；Unity 使用项目自己的地图。未启动第二个 Unity 项目或 Editor。

运行结果 `CAPTURED/DONE`，初态加 45 tick 共 46 个样本；武器 tick17 落地、tick18 拾取、tick24 轻投、tick30 释放，与此前[同域根/Scene 配对](../RUN-03-REPORT.md)一致。本轮新增的实际帧内 observer 在 Unity 直接记到 kind10 `applied=true` 七次，按相对 tick 分布为 `31:1、32:1、33:1、34:2、35:2`；[当前正式 playable 的命中事件 CSV](../../NTSD28-336B44-Q07-F02-STAGE-DOMAIN-001/bg1-z400-source-01/relation-hits.csv) 同样是这七次，正式根 LFR 的对应 tick 尾状态此前已与源码配对。Unity 还直接记到 tick39 的武器帧写入 `1→40→41`；正式源码[逐 tick CSV](../../NTSD28-336B44-Q07-F02-STAGE-DOMAIN-001/bg1-z400-source-01/source-ticks.csv) 在 tick39 前为动作1、后为41。正式根 trace 不公开帧内 `40` 写入，故不把这条 Unity 内部事件写成根 EXE 已逐写入见证。

把本次 46 个样本与此前已对正式根 2346/2346 字段配对的 `f02-kind10-scene-20261003-03` 相同字段逐值比较，**5198/5198 个共有样本字段相同、首差无**；本次探针较旧报告新增 `frameEntryAction` 和 `vrestFromSlot1` 字段，所以未把不同 schema 的整条 JSON 强行当作逐字节相同。本轮没有重新跑正式根 128 tick 或全项目测试。

Editor 有序关闭 `Completed/RuntimeMapCleared`，World 对象、Runtime slot、pool borrower、活动池对象及 Sprite 均为0，pool quiesced、World detached、无 live Driver World；已退出 Play、回到单一干净 Menu，Battle/Menu/GameConfig/ProjectBattleModeConfig 四个受保护磁盘文件本轮前后 SHA 一致。当前 Menu SHA 与旧运行不同，属于已确认保存的新基线，本轮没有恢复或覆盖它。

这一限定出口补齐了 F02 原 Battle Scene 的 Unity 帧内 kind10 与帧写入观察；物理设备按键、正式根 EXE 实际 GPU Present、严格同 alpha 画面、阴影及其它 Q07/Q09/Q12 情况仍待，父阶段和总目标不关闭。
