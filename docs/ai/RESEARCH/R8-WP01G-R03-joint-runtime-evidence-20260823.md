# R8-WP01G-R03 — physical input → movement → interaction joint runtime evidence

> 日期：2026-08-23  
> 当前状态：`COMPLETE AT AVAILABLE EVIDENCE / C++ FULL TRACE BLOCKED`  
> Authority：`J:\QQFile\NTSD2.4\ntsd_release` release live source（只读）

## 1. 结论边界

本轮取得了Unity真实`NTSD_Battle`中的物理输入、移动/跳跃/落地、拾取/持有/投掷、抓取/CPoint/link、
碰撞候选/命中/伤害联合运行时证据。所有Play报告均为PASS，探针结束后world entity、slot、对象池、
逻辑池及被临时修改的全局状态均恢复到基线。

该证据证明当前Unity模块在本轮夹具覆盖的路径上能够联合运行；它不是C++ executable/full trace，不能把
未被当前DAT或场景触达的`D-MOV-005 type0 state2000`、`D-COL-005B generic kind1`、`D-HIT-005 CLR/current-DAT
mismatch`或`D-LIFE-001 oid51 merge/split`分支升级成C++ runtime VERIFIED。R1-WP02继续BLOCKED。

## 2. F1 — physical input edge / combo

| 输入 | 边沿证据 | authored目标 | 结果 |
|---|---|---|---|
| L→S→K（DDJ） | current fresh combo step 1/2：tick1603/1604；L/S/K press attempts 1/1/1 | tick1616进入frame271；对象数8→peak20 | PASS |
| L→D→J（DRA） | current fresh combo step 1/2：tick2225/2226；L/D/J press attempts 1/1/1 | tick2238进入frame263；对象数9→peak10 | PASS |

输入由`InputSystem.QueueStateEvent`进入正式`CharacterInputModule → FrameInputSet → runtime key/prev/cd →
combo/frame`链，不直接写角色runtime或目标frame。

## 3. F2 — movement / jump / landing

报告：`Temp/NTSD_R8_PHYSICAL_MOVEMENT_PLAY.result.json`

| checkpoint | tick / value |
|---|---|
| baseline | current fresh run对象数8 |
| Right edge | tick1049；D press attempt2 |
| physical K edge | tick1053；K press attempt1；canonical Defend bit→KeyDefend/CdJump |
| jump writer | DAT `jump_distance=8`、`jump_height=-16.3` |
| first airborne | tick1057，Vx7，Vy-14.6 |
| release | tick1060 |
| landing | tick1077，Y/YInt=0 |
| cleanup | 对象数8→8；五项checkpoint全部true |

第一次有效trace已证明frame210→211→212及DAT velocity writer；最初FAIL仅因test-only probe误等canonical
`Jump32`，而项目保留的交叉输入合同把physical K携带在`Defend64`后写`KeyDefend/CdJump`。修正只发生在
Editor probe，production未改。

完成后证据审计又发现一次性`QueueStateEvent`在fresh Editor/MCP会话中可落在FrameInputSet采样边界之外，
导致DDJ/F2首键报告被覆盖为FAIL。`R8-JOINTINPUT-PROBE-002`仅在canonical edge尚未出现时，以最多8次
release→press物理状态脉冲重试并记录attempt；不调用InputSystem.Update、不写runtime。current三份报告已
重新fresh PASS；F2的D需要attempt2，直接证明该修正解决的是探针采样可重复性。

## 4. F3 — interaction / hit joint

### 4.1 Pickup / held / throw / landing

报告：`Temp/NTSD_R8_WP01C_02_HeldWeaponLifecycle.result.json`

- type1 light、type2 heavy、type4 throw、type6 drink均完成pickup、held pose、type-specific throw与landing；
- overlap target HP/frame保持333/0，证明spawn/pickup阶段没有immediate hit；
- baseline/final：ObjectCount 8/8、claimed slots 6/6、object pool active 2/2、logic pool active 6/6；
- `cleanupCompleted=true`，无cleanup error。

### 4.2 Grab / CPoint / held injury / link

报告：`Temp/NTSD_R8_WP01C_03_GrabCpointLink.result.json`

- valid grab建立reciprocal relation，caught duration 300→299；
- held injury把victim HP 20→-10、HPBound 90、combo30，并产生killStat +1、damageStat +30；
- CPoint位置actual `(116,19,201)`等于expected；mismatch throw、escape direction、正负link residue尾处理通过；
- 全局统计恢复；ObjectCount/slot/pool/logic均8/6/2/6→8/6/2/6；cleanup PASS。

### 4.3 Collision / ordered hit / damage / abort

报告：`Temp/NTSD_R8_WP01C_04_CollisionHitDamage.result.json`

- matrix总候选10，覆盖character/weapon/special、hit-confirm abort、caught gate、effect21 abort、raw frame、
  random-weapon boundary no-op；
- produced sound 6，RNG calls 9；
- stats、RNG、pending sounds、rests、hit-plan mode全部恢复；
- ObjectCount/slot/pool/logic均8/6/2/6→8/6/2/6；cleanup PASS。

## 5. First-difference decision

本轮没有观察到production gameplay first difference，因此没有创建或修改production gameplay Change。
唯一脚本改动是`R8-JOINTMOVE-PROBE-001`的Editor-only F2证据探针。

## 6. 证据层级

| 层级 | 状态 |
|---|---|
| C++ release source contract | 已闭合到本包入口/顺序；C++目录只读 |
| Unity crosswalk | Input→FrameInputSet→runtime→movement→interaction/hit已闭合 |
| fresh Play F1/F2/F3 | PASS |
| fresh compile | final refresh后0 error；清理self-check预期负路径诊断后Console error=0 |
| focused EditMode | current final 8个相关类257/257 PASS；job `bf16f84db0b346809407bfe7a01dbc83`；一次W05瞬时失败经隔离8/8与全组复跑转绿 |
| full BattleRuntimeSelfCheck | current 2026-08-23 17:33:15 PASS |
| C++ executable/full trace | BLOCKED / 未取得 |

治理校验：`Tools/Validate-ChangeLedger.ps1` PASS（79 records / 93 governed code files）；scoped diff/whitespace
检查PASS。

## 7. 未扩大结论

- 不声明全部战斗逻辑完整对齐；
- 不处理T8 default stage.dat、IL2CPP/Player、Android、服务器、F1/F2 debug；
- 不改变CentralOnly、Texture2DArray、dynamic Mesh、URP、1.5×、fixed camera、扩展容量、30Hz、
  FrameInputSet、SoA/ECS、pool/worker/0-GC边界。
