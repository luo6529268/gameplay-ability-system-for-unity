# Q07/F01 原 Battle Scene 自然 CAG 命中限定见证

状态：`VERIFIED_SCOPED`。关闭此 Play 见证和 F01 的 `bdefend` 字段首差；Q07 整组、真实物理按键及整场同态仍开放。

原项目 Unity 2022.3.62f3 Editor PID11944 在 `NTSD_Battle.unity` 干净 Edit 状态，以 `Assets/Refresh` 导入新 Editor 探针；Editor.log 报 Tundra build success，未见新的 `error CS`。只提交一次请求 `guren-cag-scene-01`。探针在 Play 副本的 Bootstrap Start 前配置 Guren OID84、Lee OID7，验证生产 World 使用 `Assets/NTSD/Content/LoganRuntime`；稳定暂停在全局 tick5 后设置 Guren action150/source X500、Lee action110/source X600，二者 Z650/HP与MP500，通过共享空间投影设置物理位置。随后用中性输入执行 20 个完整生产 `SimulationTickDriver` tick。角色初态是受控 Play 夹具，并非从玩家物理按键自然选招。

[原始 Play 报告](guren-cag-scene-01.json)为 `PASS/DONE/exitedPlay=true/sceneCleanAfter=true`。相对 tick11 出现 OID619（slot50/action304），相对 tick12 Lee HP500→450、action110→186；20 tick 末 Lee HP450。对[正式 playable 源逐 tick CSV](../NTSD28-336B44-Q07-F01-GUREN-CAG-ROOT-001/source-x600/source-ticks.csv)的 Guren action、Lee action/HP、CAG slot/action/source X 六字段逐 tick 比较，**120/120 一致、无首差**；机器可读[对照](source-unity-comparison.json)。正式根 EXE 同一 LFR 的这六字段也与源码 120/120 一致，tick12 根 trace 直接记录 `applied/hpDamage50`；证据在[根见证报告](../NTSD28-336B44-Q07-F01-GUREN-CAG-ROOT-001/REPORT.md)。Unity 的 tick6–25 对应夹具的相对 tick1–20；这不是从战斗开始全状态、全场景输入、画面或声音 parity 证书，根报告本身 `nativeParityClaim=false`。

退出后 MCP 显示原 Editor 非 Play、idle；Battle Scene 报告中的前后 SHA 都是 `3A089236328ACAE1510F8A831B77D4895CC34028DDCDEBE542BEF0DA8EC235ED`，Scene clean。Menu Scene、GameConfig 与 ProjectBattleModeConfig 的保护哈希也保持原值。本包没有修改正式 EXE/DAT、Unity 生产代码、Scene、Prefab、配置或非战斗逻辑。新探针仅是 Editor 诊断载体，不进入玩家运行时。

收尾 `Tools/Validate-ChangeLedger.ps1` PASS（1051 records、共享 diff 中 11 个受管代码文件）；`git -c core.safecrlf=false diff --check` PASS。未重跑全套 Unity 测试；此前 F01 聚焦 2/2、相邻 18/18 和完整自检已有证据，本轮新增 Editor 诊断脚本由原 Editor 编译及唯一的定向 Play 验收。
