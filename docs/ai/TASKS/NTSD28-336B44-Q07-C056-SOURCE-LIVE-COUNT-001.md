# NTSD28-336B44-Q07-C056-SOURCE-LIVE-COUNT-001

状态：`VERIFIED`（仅正式源码活体数与原 Scene 对照）。父目标：`NTSD28-UNITY-BATTLE-REALIGNMENT-001 / BATCH-04 / Q07 / C056`。

当前 336B44 对应 playable `GameSession28` 的 C056 受控三 tick 已记录 OPoint 出生 0/0/0 与 1/1/0；原 Unity Battle Scene 对应记录了活体 OID213 数 0/0/0 与 1/2/2。正式源码旧探针只导出 slot2 OID，没有源侧全槽活体数，不能从累计出生直接推断活体数。

只允许修改 `Tools/NTSD28Q07Diagnostics/fusion_hold_counter_probe.cpp`：在现有逐 tick CSV 末尾增加从 `BattleWorld28` 公开实体接口枚举得出的活体 OID213 数和 World 活体总数。保持旧列、初态、输入、tick 次数与正式源码不变；正式根 EXE、Unity 生产/测试、DAT、图片、Scene、非战斗逻辑均不改。

验收：确认正式 EXE 与源码身份；用当前 playable 构建闭包的 C++17 源码重链诊断，双跑新 CSV 逐字节一致；旧列对既有 C056 源侧基线逐值相同，新增列与原 Scene 保存的活体数逐 tick 配对。若旧原始 CSV 不可得，明确记为仅与已报告字段/场景 JSON 配对，不能声称旧文件逐字节回归。结果仅关闭源活体子门；正式根 EXE 受控入口及 C056/Q07 总门仍开放。

回滚：只在审查差异后撤销本探针新增列，不触及已有用户工作或历史证据。新输出使用唯一目录，不覆盖旧文件。

2026-10-04 出口：C++17 重链 0 诊断，两次新 CSV 同 SHA；计数7/停顿3的源码/Scene活体均0/0/0，计数0/停顿3均1/2/2，结构出生两侧相同。初次资源根传参失败的原件保留。旧原始 CSV 不在工作树，未报逐字节回归。正式根受控入口与本 Scene 阶段关闭仍待。[报告](../../../artifacts/diagnostics/NTSD28-336B44-Q07-C056-SOURCE-LIVE-COUNT-001/REPORT.md)。
