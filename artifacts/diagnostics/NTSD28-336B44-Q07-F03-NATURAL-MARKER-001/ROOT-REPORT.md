# F03 自然环境标记正式根对照

2026-10-01 本地日期；当前正式根 EXE SHA-256 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3`。正式 `catalog.csv` 的 `id=36,type=0,source_path=c/tay/tay.dat` 是多由也 CS2；该 DAT 正式/Unity staged 解码 SHA-256 都是 `677E8DF36E21706D506D4DEC50DBD543877085182A8658416CA56A633F7D796B`。`id=210` 是无关 type3 `a/kat/kat2.dat`，首次三组用错 ID 的结果保留为无效夹具，未计入本结论。

当前正例只在初态设置正式角色多由也 OID36/action243 X500/Z400 与鸣人 OID2/action0 X550/Z400，双方 Y0、HP/MP500、team1/2，背景1、mode0、seed682973786；随后128 tick 双方中立输入。没有设置命中、环境标记、伤害、落地或结果。源 `GameSession28::step` 的实际 `relation_hits` 在 tick1 对目标自然应用 kind10，把 `environment_state_320` 写为-20。正式根用这次 source-capture 的完整 LFR、相同索引和内容重放，报告 `passed:true/failureCode:0/completedTicks:129`；独立 CRT seed 与 EOF tick129 不在比较窗口。

| tick | 源与正式根共同见证：鸣人动作/state/Y/HP/环境标记 |
|---|---|
| 1 | 182 / 12 / -2 / 500 / -20 |
| 59 | 181 / 12 / -14 / 464 / -20 |
| 60 | 185 / 12 / 0 / 444 / 1 |
| 65 | 185 / 12 / 0 / 444 / 1 |
| 66 | 230 / 14 / 0 / 443 / 0 |
| 67 | 230 / 14 / 0 / 443 / 0 |

完整 tick1–128 的两实体256行×15字段（含环境标记）与每tick5个RNG标量共 **4480/4480 字段一致，差异0**；54条关系事件在 tick、攻击者、目标、kind、状态与顺序上逐项一致。根报告PASS本身只证明LFR校验通过，逐字段和事件比对另见 `tay36-a243-x550-source-root-128-comparison.json`。tick66正是最终帧离开state12且尾部清0的有效F03出口；tick60、65仍state12而保留标记1是相邻控制。原 Unity Battle Scene、物理键自然选招、C032声道6和整场World仍待独立验证；这份报告只关闭正式源/根自然分支。
