# C042 正式 OID75 自然抓取→投掷链：限定运行见证

状态：`SOURCE_ROOT_NATURAL_PATH_PASS / C042_NONZERO_COUNTER_TRIGGER_PENDING`。当前正式根 EXE SHA-256 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3`；正式内容是 `resources/runtime` 的 OID75 `bee.dat` 和 OID2 鸣人。只用完整 `GameSession28::step()`、初始 action、近远位置和中性输入；没有强设抓取关系、被投者计数、中途动作或 DAT 值。

| 初态 | 完整源码结果 | 正式根结果 | C042 非零计数门 |
|---|---|---|---|
| OID75/action355 X500 → 鸣人 X550，80 tick | tick1 kind-3 抓取；tick55 在375帧投掷，投前被投者计数0 | 同 LFR 回放 `passed:true/failureCode:0`；80 tick 两实体/RNG已声明字段 1360/1360 零差 | 未触发 |
| OID75/action355 X500 → 鸣人 X1200，80 tick | 无抓取、无投掷 | 源码阴性控制；未重复根回放 | 未触发 |
| OID75/action378 X500 → 鸣人 X550，160 tick | tick1 kind-3 抓取；action76 持续80 tick后 tick142 在78帧投掷，投前被投者计数0 | 同 LFR 回放 `passed:true/failureCode:0`；160 tick 两实体/RNG已声明字段 2720/2720 零差 | 未触发 |
| OID75/action378 X500 → 鸣人 X1200，160 tick | 无抓取、无投掷 | 源码阴性控制；未重复根回放 | 未触发 |

action355 另扫 X520、530、540、560、580、600：X520未抓取；X530～600均 tick1 抓取/tick55 投掷，投前被投者计数仍为0。X550 的 tick54 被投者 action135/counter0，tick55 投掷选 action181，完整 tick 尾计数1；不能把尾部1误读成投掷分支保留了投前非零计数。两阳性根回放的独立 CRT state 与源码分别有80/160 tick 差异，比较范围明确排除此字段及根额外 EOF tick；其余比较见 `source-root-comparison-v1.json`。根报告的 `nativeParityClaim:false` 仅表示根自己的 LFR 校验不自动颁发完整同态证书；本包另做了限定字段比较，不声称全 World/checksum/画面相同。

首次 action355 根回放错误沿用旧 F05 启动参数 `--character 36 --lfr-slot0-action 243`，`target-550-root-v1-report.json` 留存 FAIL46。另一次用 `Start-Process -ArgumentList` 因含空格路径未正确成参而退出10、无报告，已留 stderr/stdout；最终使用准确角色75/action355和同一 LFR 的 v2 报告 PASS。action378 使用角色75/action378准确参数，v3 报告 PASS。根身份、LFR路径、报告参数均单列保存。

本包证明正式内容两种近距投掷链确实可达，并给 C042 提供正式根/源码自然路径证书；它**没有**提供 C042 被投者投前非零计数正例，也没有运行原 Battle Scene。受控 392 例机制修复仍按父包保持 `FOCUSED_TEST_PASS / RUNTIME_PENDING`。按总表 G1，保留此触发前置，转向下一条正式内容可达的 C044 负 `decrease` 首差；若后续找到自然非零计数入口，再回访 C042 的根与原 Scene，不用手设计数伪装自然验收。

收尾：本诊断源 v1/v2/v3 均以当前源码编译退出0；Change Ledger 校验退出0（1072条 Record、11个受治理代码文件），`git -c core.safecrlf=false diff --check` 退出0。Battle/Menu Scene、GameConfig、ProjectBattleModeConfig 四保护 SHA 均与基线相同；原 Editor 仍在 Battle Scene、idle、非 Play、非编译/更新。没有运行 Unity Play，因为本包尚无 C042 非零计数自然正例。
