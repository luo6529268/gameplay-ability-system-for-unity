# R7 repair sequence after complete inventory

> 日期：2026-08-22  
> 状态：`COMPLETE / ORDERS 1-11 CLOSED`

## Goal

在R7完整盘点后，以最小、独立、可回滚Work Package关闭已登记差异；不再边发现边修改。

## Ordered packages

| Order | WP | Goal | Production code |
|---:|---|---|---|
| 1 | R7-AI-02A | 建C++ 39-position/gate/RNG source oracle | No，test/fixture only |
| 2 | R7-AI-02B | 补HitJ optimized data contract | Yes，data rows only |
| 3 | R7-AI-02C | OID6/7/8/11 helper group | Yes，未默认接线 |
| 4 | R7-AI-02D | OID10/1、9/2、32/19/33 group | Yes，未默认接线 |
| 5 | R7-AI-02E | OID34/label464/35/36/38/39 group | Yes，未默认接线 |
| 6 | R7-AI-02F | full ordered dispatcher integration | Yes，唯一默认切换点 |
| 7 | R7-TEST-002 | stale worker human-key fixture | Test only |
| 8 | R7-TEST-003 | worker/central/ack joint fixture | Test only |
| 9 | R7-TEST-001 | static pollution isolation | Test only unless owner证明production static错误 |
| 10 | R7-BROAD-02 | production backend decision matrix | Config/code only after parity+performance evidence |
| 11 | R7-CAP-01A/B | capacity contract decision then implementation | Decision first |

## Common verification

每个脚本WP必须在修改前建立Change Record；修改后至少执行：

- governed scoped diff + `Tools/Validate-ChangeLedger.ps1`；
- Unity fresh compile error=0；
- exact focused tests；
- fresh-domain full `BattleRuntimeSelfCheck`；
- production default/fallback/optimized profile pair；
- relevant warmed 0 B、RNG、slot/generation witness；
- R8前不把任何包声明为完整battle VERIFIED。

## Completion

Orders 1–11已完成到相应证据层。R7-BROAD-02 fresh parity 88/88与same-domain self-check PASS；因缺
current-build real A/B和R8 scene parity，正式决策为保留BruteForce且不改GameConfig。

R7-CAP-01A已固定“无固定产品cap + 有限prebattle reservation + sealed strict 0 B + deterministic
admission failure”合同；fresh capacity/pool矩阵44/44与03:19:45同域self-check PASS，现有production符合，
因此R7-CAP-01B不需要实施。R7 repair sequence至此完成，下一阶段为R8。
