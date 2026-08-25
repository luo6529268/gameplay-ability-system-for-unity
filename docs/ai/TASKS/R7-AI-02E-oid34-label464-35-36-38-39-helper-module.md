# R7-AI-02E — OID34/label464/35/36/38/39 helper module

> 日期：2026-08-23
> 状态：`RUNTIME_PENDING / UNWIRED MODULE`

## Result

positions29–37已写入现有非partial实例module，但默认Legacy/DataOriented dispatcher均未调用。
source-derived focused 31/31、AI regression 238/238、warmed position34 100-slot scan 0 B、fresh Unity
compile、generated Editor build和fresh-domain full self-check均通过。两个02A red witnesses仍按预期失败，
因此production difference没有被误写为已关闭；完整1–39接线与联合遮蔽只由02F处理。

## Goal

以 C++ release source 为唯一行为权威，实现 39-position chain 的 positions 29–37（OID34/10/5/14、
label464 group、OID35、OID36/16、OID38、OID39/10）为现有非 partial、持久复用、纯数据实例模块的
第三段，并建立 source-derived fixtures。模块本包不得接入默认 dispatcher。

## Scope

- positions 29–37 的固定顺序、strict comparisons、RNG 短路顺序、combo/key 写入和 early-return；
- position 30 first-20 teammate scan 的 slot 升序、first-match、HP window、movement-only 和 DUJ 分支；
- position 31 target frame263/264 jump side-effect 后返回 false并继续 position32；
- position 34 first-100 team scan在 PP/Rand5 门命中后，无论是否找到求助者均 return true；
- position 36/37 多段 RNG 的条件顺序；
- 模块只读 `AiSensingSnapshot`，只写 `AiDecisionInputState` 与共享 `AiDecisionRandomStream`；
- position30/34 scan count 显式传入；C++ authority fixtures分别使用20和100，Unity扩展语义不在本包裁决。

## Authority / Evidence

- `J:\QQFile\NTSD2.4\ntsd_release\src\input\input_handler.cpp:844-1094`；
- dispatcher顺序：同文件`2160-2195`；
- `J:\QQFile\NTSD2.4\ntsd_release\include\game_world.h:13`：`MAX_OBJECTS = 400`；
- release build/live-path参与性已由R7-AI-02 inventory闭合；C++目录保持只读。

## Position contract

| Position | helper | 关键合同 |
|---:|---|---|
| 29 | OID34/10/5/14 low-HP DDJ | OID命中后总先`Rand10`，再PP和两段low-HP window |
| 30 | same group teammate guard | link/frame gate→self HP window/sameZ→前20槽first-match；movement-only也true |
| 31 | label464 long | OID group先`Rand7`再range/PP；target frame263/264写jump但false继续 |
| 32 | label464 close DDA | OID group总先`Rand7`，再close range/PP |
| 33 | OID35 long | OID35总先`Rand7`，再range/PP，写DLA/DRA |
| 34 | OID36/16 team DUJ | PP>200后`Rand5`；门命中扫描前100槽，找到写DUJ；找不到也true |
| 35 | OID36/16 range DUA | PP>260后`Rand10`，再dx/dz；只在position34门失败时可达 |
| 36 | OID38 combo | PP gate分别控制Rand5、Rand10、Rand10；前一段失败才进入下一段 |
| 37 | OID39/10 close | PP>100后Rand3；即使随机命中但facing/dz失败仍继续Rand7分支 |

## Files likely involved

- `Assets/NTSD/Scripts/Simulation/Ai/AiCharacterDecisionModule.cs`
- `Assets/NTSD/Scripts/Test/Editor/AiCharacterDecisionPositions29To37EditorTests.cs`（new）

## Unknowns

- 本包不验证positions1–39联合遮蔽，只由02F联合验收；
- C++ runtime trace仍BLOCKED；
- >399 slot没有C++ authority对应物，其adapter行为必须在02F/R7 capacity合同下单列验收；
- 真实角色DAT/Play Mode由02F/R8负责。

## Deliverables

1. `AiCharacterDecisionModule.TryEvaluatePositions29Through37`；
2. source-derived fixed-seed order/short-circuit/scan/side-effect/0 B tests；
3. compile、existing AI regression、fresh self-check、ledger证据与handoff；
4. 默认dispatcher与02A red witnesses保持原状。

## Verification

- generated Editor build与fresh Unity compile 0 error；
- exact focused fixtures全部PASS；
- position30覆盖20-slot边界、first-match、filter、movement-only和DUJ；
- position31覆盖jump continuation到position32；
- position34覆盖100-slot边界、found/no-found都early-return、position35遮蔽；
- positions36/37覆盖多段RNG calls/order；
- existing 39-position contract/red witnesses保持原状态；
- existing AI regression PASS；
- warmed scan/module path 0 B；
- fresh-domain`BattleRuntimeSelfCheck` PASS；
- `Tools/Validate-ChangeLedger.ps1` PASS。

## Stop conditions

- 需要改变outer gate、positions1–28或38–39；
- 需要默认接线、移动现有positions38/39 production helper或改变Legacy/DataOriented输出；
- 需要新增snapshot/runtime数据字段；
- 需要替position30/34引入改变slot顺序的索引或缓存；
- 需要修改C++、pass、profile、capacity、render或input binding。

## Out of scope

- default dispatcher integration（02F）；
- positions38–39 production调用的移动；
- input edges/cooldown调用点移动；
- Play Mode/C++ runtime trace/R8。
