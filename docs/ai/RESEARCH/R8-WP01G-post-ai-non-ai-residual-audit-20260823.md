# R8-WP01G — post-AI non-AI residual alignment audit

> 日期：2026-08-23  
> 状态：`READ-ONLY EVIDENCE AUDIT COMPLETE / NO CODE CHANGE`

## 1. Scope decision

用户决定不再对齐C++ AI sensing、39-position decision与AI RNG，未来使用Unity状态树或行为树。
`D-INP-005/007A/007B/008/009`不再属于战斗对齐backlog；现有实现和测试保留。未来AI只需通过
30Hz canonical `FrameInputSet`接入战斗runtime。

本审计只统计非AI、正常战斗逻辑。F1/F2 debug已排除；T8 default stage.dat、IL2CPP、Android和服务器
继续排除。

## 2. Current conclusion

当前没有新发现的“C++ source已确认、Unity正常战斗production仍未实现”的非AI代码差异。
原11个可执行证据D-ID现均已推进至Unity S4或明确source-deferred边界；剩余内容为5个当前fixture/DAT不可达
exact分支、1个人工硬件输入项、1组function-key debug policy，以及全局C++ full trace blocker。另有一个
验证基础设施阻塞`R-HC-01`：恢复的正式DAT包含5个`w=21/h=-999`倒置body，旧self-check风险分类尚未识别
C++ raw inverted rectangle语义；它不是已确认gameplay production差异。

## 3. Executable non-AI runtime evidence groups

### G1 — candidate / PreInteraction adapter

| D-ID | 仍需证明 |
|---|---|
| D-SCHED-007 | `R05 UNITY JOINT S4 PASS`：candidate内容/顺序/cap/RNG/consume A/B与live Play已闭合；C++ full trace仍BLOCKED。 |
| D-PERF-001 | `R05 UNITY JOINT S4 PASS`：neutral/fail-closed/0B与fast/forced-legacy hash已闭合；C++ full trace仍BLOCKED。 |

这是最早的scheduler/collision依赖，推荐作为下一包。

### G2 — input/link and P1/P2 routing

| D-ID | 仍需证明 |
|---|---|
| D-INP-001 | `SOURCE-CLOSED / NATURAL TYPE0 PATH IS AI-DEFERRED`：自然type0 negative-link来自opoint kind2 AI child；不再伪造非AI Play。 |
| D-INP-004 | `UNITY INPUTSYSTEM S4 PASS / C++ FULL TRACE BLOCKED`：R06已补齐Player_2 Attack/Jump/Defend与numpad1/2/3 physical source，two-human Play 11/11 press/held/release/no-cross PASS。 |

2026-08-23 R06 preflight已将G2拆分；详见
`R8-WP01G-R06-g2-input-residual-preflight-20260823.md`。

### G3 — merge/split lifecycle adapter

| D-ID | 仍需证明 |
|---|---|
| D-LIFE-001 | `R08 UNITY JOINT S4 PASS / C++ FULL TRACE BLOCKED`：4500 tick、dormant、原slot/generation、split、Central双body与cleanup已闭合。 |

### G4 — central render handoff and writeback

| D-ID | 仍需证明 |
|---|---|
| D-SCHED-009 | `R07A UNITY JOINT S4 PASS / C++ FULL TRACE BLOCKED`；production phase diagnostics与actual hit writeback已闭合。 |
| D-RENDER-001 | CentralOnly cold/current/last-good/replacement fail-closed在真实URP Play保持central owner。 |
| D-RENDER-002 | `R07A UNITY JOINT S4 PASS / C++ FULL TRACE BLOCKED`；published/no-publication、RNG、age与Late幂等已闭合。 |
| D-RENDER-003 | `R07B + R08 UNITY JOINT S4 PASS / C++ FULL TRACE BLOCKED`：pending/generation/T+1与dormant/split均已闭合。 |
| D-RENDER-004 | `R07B UNITY JOINT S4 PASS / C++ FULL TRACE BLOCKED`；正式223/224 body提交且shadow command/submission不存在。 |
| D-RENDER-005 | `R07B UNITY JOINT S4 PASS / C++ FULL TRACE BLOCKED`；223/224不写额外shadow hidden，baseline角色body/shadow正常。 |

WP01D-01～07主要关闭`D-RENDER-006`的通用sprite mapping/GPU/Game/SceneView，不自动关闭上述001～005。

## 4. Exact branches blocked by current fixture/DAT

| D-ID | blocker | 当前代码状态 |
|---|---|---|
| D-MOV-005 | current authored type0 state2000 witness不可得 | exact writer已修，普通movement/jump S4 PASS |
| D-COL-004 | production oid999 gated frame无有效geometry | synthetic brute/role-aware collection一致 |
| D-COL-005B | block-aware全DAT `itr kind1=0` | generic selector/weapon grab代码已修 |
| D-HIT-005 | current-DAT/CLR shell mismatch production fixture不可得 | dispatcher与generic typed writers已修 |
| D-RENDER-006 | loaded DAT无authored state8000 witness | 5537 descriptor、GPU/Game/SceneView S4均PASS |

这些不是当前可直接修改的差异。不得改DAT或硬编码角色/技能制造witness。

## 5. Manual and optional boundaries

- `D-INP-006`：InputSystem自动S4已PASS；真实人手键盘、Game窗口焦点和OS硬件edge由用户验收。
- `D-SCHED-011`剩余的`g_init_stats/g_game_mode2`来自C++ `main.cpp` F7/F8/F9 function-key分支：
  F7把角色HP/PP置500，F8随机掉落武器，F9清武器picker；normal postframe tail/reset时点已有代码/self-check。
  它不是正常自动战斗输入；建议与F1/F2同样排除，若用户需要这些调试键再独立实现。

## 6. Evidence-only limitations

大量已写成`RUNTIME_PENDING`的D-ID已有Unity S4，剩余只是R1-WP02 C++ executable/full trace未取得，
不代表Unity仍有已知代码差异。没有安全只读trace方案前，必须保留该证据限制，但不能反复重写已通过逻辑。

## 7. Execution update and next packages

- G1 `R8-WP01G-R05`已完成到`UNITY JOINT S4 PASS / C++ FULL TRACE BLOCKED`；不重复执行；
- G2 `R8-WP01G-R06`已完成到`UNITY INPUTSYSTEM S4 PASS / C++ FULL TRACE BLOCKED`；不重复执行；
- G3 `D-LIFE-001`的source/adapter已闭合，仍只有真实production Play证书pending，不允许直接写合体状态
  制造PASS；现已建立`R8-WP01G-R08-oid5152-merge-split-central-runtime.md`，状态`PLANNED /
  APPROVAL PENDING / NO EXECUTION`；
- G4已完成只读预检并拆为R07A/R07B/R07C。R07A处理`D-SCHED-009 + D-RENDER-002`，现已完成到
  `UNITY JOINT S4 PASS / C++ FULL TRACE BLOCKED`；production worker Play、18/18、178/178、13/13与full
  self-check均PASS。详见`RESEARCH/R8-WP01G-R07A-render-writeback-joint-runtime-evidence-20260823.md`。

R07B已完成：pending/generation/T+1、current-DAT 223/224与visibility central command/submission联合Play
PASS，focused24/24+9/9+worker18/18、self-check、Console0和ledger均PASS；证据见
`R8-WP01G-R07B-central-liveness-identity-visibility-runtime-evidence-20260823.md`。OID7/8→51 dormant/split仍只归
独立R08，R08完成前不得整体关闭`D-RENDER-003`。R07C已执行：current/stale/replacement真实URP像素与
ownership PASS，cold exact self-check PASS/Play未运行；但normal Play发现B-R8-R07C-01 active submission后
late capacity seal resize异常，故R07C保持BLOCKED，repair R07C-R01为APPROVAL PENDING。

R06、R07A与R07B均已按独立Task Contract与Change Record收口，不重复执行。R07C部分证据通过但被
B-R8-R07C-01阻塞；未获repair批准前不进入R08。R08仍保持`PLANNED / APPROVAL PENDING / NO EXECUTION`。

2026-08-23更新：R07C-R01已获批并完成。Camera-preserving首次seal retirement、重复seal no-op、normal
Play Console0、R07C三态、cold self-check与Combat1000 0GC均PASS，`B-R8-R07C-01`关闭，R07C按现有
Unity S4证据收口。R08仍保持`PLANNED / APPROVAL PENDING / NO EXECUTION`，本repair没有启动R08。

2026-08-24最终更新：R08现已完成。`R8-AIROWGEN-001`关闭dormant split stale-row first difference；正式
4500-tick Play通过OID7/8→51→7/8、原slot/generation、current-half HP/HPBound、Central merged/dormant/split
和generation-safe cleanup。`D-LIFE-001`与`D-RENDER-003`因此达到`UNITY JOINT S4 PASS / C++ FULL TRACE
BLOCKED`，原11个可执行非AI证据项全部完成到当前允许证据层。

同轮full self-check在R08目标检查前被`R-HC-01`阻塞。只读C++ `hit.cpp::vertical_world_rect/bdy_world_rect/
aabb_overlap`与Unity`BruteForceSceneQuery.VerticalWorldRect/Overlap`均直接计算`y2=y1+h`并使用严格不等式；正式
OID58 frame75/76与OID10 frame75/76/77的`bdy w=21/h=-999`会形成倒置rect；普通小itr不命中，但跨过
倒置两端点的大itr仍按strict条件命中。下一建议包为`R8-WP01G-R08-R03 / R8-GEOMETRYCHECK-001`，只修
self-check风险分类并增加两侧raw overlap生产collector回归；不得改DAT、parser或碰撞production。

2026-08-24 R09 final reconciliation：R08-R03已关闭`R-HC-01`，R08-R04已把旧DAT fixture路径迁移到当前
production catalog并使完整self-check恢复PASS；`R8-SPRITERANGE-001`七层验证齐全并升级VERIFIED。68项最终
集合校验为68/68、missing0、extra0、duplicate0。当前没有新的normal-combat production代码差异；5个exact
DAT/fixture不可达分支、人手硬件edge、F7～F9 policy与R1-WP02 full trace继续如实保留。
