# C044 鬼鲛自然跨零原 Battle Scene 限定验收

状态：`VERIFIED_SCOPED_NATURAL`。本报告只覆盖当前正式 336B44 内容的 OID17/action314、目标 OID2 起始 X550 近距与 X1200 远距、种子 682973786、mode0、难度0、双方 HP/MP500、Z400、中性输入前 60 个生产逻辑 tick。正式 EXE SHA-256 为 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3`；[正式源码与根同 LFR 证据](../NTSD28-336B44-Q07-C044-KISAME-NATURAL-REACH-001/REPORT.md)在这两组之外另含 action316 近距，不能自动算作 Unity 场景证据。

原项目 Unity Editor PID11944 完成脚本导入与编译后，在唯一原 `Assets/NTSD/Scene/NTSD_Battle.unity` 中先后执行两次独立 Play。新探针于每次场景加载时配置 OID17/OID2，再用正式暂存内容和生产 `SimulationTickDriver.StepOneTick` 连续采集 60 tick；两次结果均为 `CAPTURED / DONE`、60 个样本、`exitedPlay=true`、`sceneCleanAfter=true`。[近距原始报告](kis17-a314-x550-natural-scene-01.json)、[远距原始报告](kis17-a314-x1200-natural-scene-01.json)。

将正式 `GameSession28` 的同初态 `source-ticks.csv` / `source-rng.csv` 与 Unity 样本按相对 tick 对齐，逐 tick 比较双方动作、动作计数、X/Y、X/Y 速度、HP、抓取目标槽、抓取时限及五项 RNG 数值；每例 20 字段×60 tick=1200 值，两例合计 **2400/2400 一致，首差 0**。[可复算比较输出](source-unity-comparison.json)。正式根先前对这两条源码轨迹的 160 tick LFR 对照也通过，但本 Unity 结论严格限于前 60 tick。

近距 tick1 自然抓取；tick46 抓取时限为 1，tick47 变为 -6、双方动作变为 0/181、目标 HP430 且速度仍 X0/Y0，tick48 目标速度变为 X4/Y-3，均与正式源码相同。源码在 tick47 的 pending 冲量为 X4/Y-3、贡献数1；此 Unity 探针没有直接记录 pending 字段，因此本次仅通过 tick47/48 速度时序与已通过的共用 writer 受控证据支持延后结算，不单独宣称 pending 内部字段逐值同态。远距 60 tick 抓取目标始终为空，目标 HP 保持500，同正式源码阴性链一致。

两次 Play 后原 Editor 均回到 Edit Mode，最后核验为 idle、非 Play、活动 Battle Scene。Battle/Menu Scene、GameConfig、ProjectBattleModeConfig 的最终 SHA-256 分别为 `3A089236328ACAE1510F8A831B77D4895CC34028DDCDEBE542BEF0DA8EC235ED`、`DD6A48A37FB8CEA9CD8A1F7738964A719E007A42F0FBB54FB48BBD0B723B9DC3`、`0527D737A1FA38FC56B51D00DC6E96A421D3C67222546368B147C2D074CB8EA7`、`B57CFEF32CC3ECE37AC98A4A1EF04FEB2FEE4EB3CC08466A4BFD32C1EDD85B82`，与运行前相同。

本 Task 的 OID17/action314 近远自然出口已验证；C044 的其它动作、角色、长于 60 tick 的后续及 Q07 整体仍按新版总表保留开放。未修改 DAT、Scene、Prefab、生产代码或非战斗流程。
