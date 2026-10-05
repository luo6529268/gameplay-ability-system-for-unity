# D-024 自然落地规则：复用修复后原 Scene 证据

状态：`SCOPED_NATURAL_LANDING_RULE_PASS`。2026-10-05 只读复核发现，既有修复后原 Battle Scene 的 32 tick 样例已经包含自然落地及下一动作。此前 `ACCEPTANCE.md` 的“32 tick 未覆盖着地后的源 Y/floor 相邻状态”是报告遗漏，按本页更正；不另开落地 Play 或脚本改动。

权威仍为正式根 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3`；本轮重新读取 EXE 字节确认身份不变。用户确认保留现有 `BattleVisualScale=1.5` 战斗实体显示尺寸，D-024 共用位移/碰撞比例合同保持。

原件为 [修复后原 Scene JSON](../NTSD28-336B44-Q07-C023-AIRBORNE-SCENE-PLAY-001/d024-vertical-green-20261005-01.json)、[源实体 CSV](../NTSD28-336B44-Q07-C023-C024-ROOT-FRAME-001/guren388-airborne-child/source-entities.csv)、[源 RNG CSV](../NTSD28-336B44-Q07-C023-C024-ROOT-FRAME-001/guren388-airborne-child/source-rng.csv)及[正式根 trace](../NTSD28-336B44-Q07-C023-C024-ROOT-FRAME-001/guren388-airborne-child/root-trace.jsonl)。路径、逐文件 SHA、比较字段及结果见 [本轮比较 JSON](landing-reuse-comparison-20261005.json)。没有覆盖原件。

| 自然 OID85 / slot51 | 落地前 | 落地 tick | 后续动作 |
| --- | ---: | ---: | ---: |
| 相对 tick / Unity 全局 tick | 26 / 31 | 27 / 32 | 28 / 33 |
| action / state | 212 / 4 | 215 / 15 | 213 / 5 |
| counter | 0 | 1 | 1 |
| 源 Y / Vy | -5 / 8.5 | 0 / 0 | -13 / -11.3 |
| 源 X / Z | 742 / 402 | 742 / 402 | 757 / 402 |
| HP / owner / team | 500 / 0 / 1 | 500 / 0 / 1 | 500 / 0 / 1 |

源 CSV 与修复后 Unity 的 107 行实体 ×14 字段、32 tick ×6 RNG 标量共 **1690/1690 相同**；正式根与 Unity 的同样实体字段、每 tick 5 个 RNG 标量共 **1658/1658 相同**。速度浮点比较绝对容差为 `1e-12`，整数严格同值。修复前/后完整 32 个 `samples` JSON 也完全相同。

正式根比较保留既有 LFR 的限制：独立 CRT seed 未随 LFR 携带，因此根 trace 的 `crtState=3374725112` 与源/Unity 的 `1758127634` 初态不同。32 tick 的 CRT 调用计数均保持3000，期间没有消费 CRT；这32项不纳入1658项同值结论，原始差值在 JSON 中完整保留。不能据此声称完整 RNG、World 或 checksum 已同态。

这关闭本自然样例的**落地规则邻例**：源 Y 回到地面0、Vy 清零、动作215及随后213未受视图 Y 投影影响。原探针仅在相对21/23采集本体/阴影命令，没有落地 tick 的画面命令或 GPU 截图；落地画面仍未知，真实 Y 向命中响应和原 Scene R120 中间 alpha 也未由本页验证。kind0 火花已由独立 [验收](SPARK-ACCEPTANCE.md)限定通过。

本轮原 Editor 经已有 Unity MCP 桥接只读返回 Battle Scene clean、非 Play、idle、非编译、无测试运行、Console 0 error；Battle Scene 磁盘 SHA 仍为 `253B2EBAD322AA6EC18488DFBBF555BB93AD897940A342E4130E77CE778F9010`。没有刷新、进入 Play、跑 NUnit、修改生产脚本、DAT、图片、Scene 或非战斗代码。按当前总表条件门继续，不因旧 Record 状态补跑已覆盖的落地规则。

实际验证：PowerShell here-string 经 `python -` 只读读取上述五份既有原件，按 `(relativeTick, slot)` 配对14个实体字段及所声明RNG标量，并用独占创建模式写入本轮JSON，全部断言通过、退出0。`pwsh -NoProfile -File Tools/Validate-ChangeLedger.ps1 -RepositoryRoot I:\GitHub\Unity_GAS\gameplay-ability-system-for-unity` 退出0、`Change ledger validation PASSED`；仍有历史Record声明路径不在当前diff中的警告。文档首次追加产生三处EOF空行，已仅调整本轮新增段落后重跑 `git diff --check` 退出0。未重跑编译、NUnit或SelfCheck，因为本轮没有脚本修改。
