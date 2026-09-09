# NTSD28-B3-FRAME-MOTION-OWNER-AUDIT-001 — C04/C05/C06 owner审计

<!-- CHANGE-RECORD
id: NTSD28-B3-FRAME-MOTION-OWNER-AUDIT-001
status: VERIFIED
change-kind: GOVERNANCE_ONLY
code-path: NONE
authority: Promoted NTSD 2.8-Logan C04 apply_frame_motion, C05 resolve_native_teleport_state and C06 step_physics plus immediate dead-resource normalize; EXE B1E13AE1, playable closure 39DDDA15.
evidence: TASK-CONTRACT-CREATED / AUTHORITY-C04-GLOBAL-SLOT-BARRIER / AUTHORITY-C05-GLOBAL-SLOT-BARRIER / AUTHORITY-C06-NESTED-PHYSICS-NORMALIZE / UNITY-C04-PARTIAL-WRITER-IN-C03 / UNITY-NONCHAR-C04-C06-INTERLEAVED-IN-SIMTU / UNITY-C05-HALF-CADENCE-AND-STATE500-501-EXTRA / C04-MISSING-LINKED-PLATFORM-DELAY-DXYZ / C06-FRAME-DELAY-LINK-CPOINT-GATE-DIFFERENCE / NEXT-NTSD28-B3-C04-FRAME-MOTION-PRODUCTION-OWNER-001 / NO-PRODUCTION-CHANGE / AUTHORITY-READ-ONLY
-->

> 状态：`VERIFIED / C04-C06-OWNERS-CLOSED / NEXT-C04-PRODUCTION-OWNER`

## 已知首差

OID production split后，修复版权威 C03 后立即进入独立升序 C04 frame motion；Unity C03 后仍先运行
`EarlyFrameAdvanceSpecialsAll`，随后 `SerialTickAll` 按 entity 交织 Transit/SimTU/snapshot。

## 审计结论

| 边界 | 修复版权威 | Unity 当前 | 结论 |
|---|---|---|---|
| C04基础frame velocity | C03全部slot完成后，独立升序slot读取当前frame/facing/depth intent，写`motion.x/y/z`；不看frame wait。 | exact `LF2Character`在`BattleEcsCharacterInputPass`/`LF2Character.RunCharacterInputRouting...`末尾写；shared character-DAT shell也在C03写；non-character在各自`SimTU`内写。 | 当前C03被C04 writer污染，且没有全slot barrier；必须抽成production唯一owner。 |
| C04完整motion | 基础`dvx/dvy/dvz`后，同一slot处理linked-platform位移、`delay_timer_134`四分之一缩放、frame float `dx/dy/dz` positional displacement与对应轴清零。 | 现有frame模型只有`dvx/dvy/dvz`；未发现`dx/dy/dz`、platform source或delay carrier/consumer。 | 仅移动旧writer不能宣称C04行为完整；缺口归B4数据/算法包，内容字段进入B11策略前先建carrier/parser只读证明。 |
| C05 teleport | C04全barrier后独立升序slot；每tick检查state400/401；候选排除self、要求type0/HP>0并按battle group选最近敌/最远友；Y复制target或self collision reference，随后精确坐标同步并清三轴。 | `EarlyFrameAdvance`在当前显式C04 phase前；受`FrameToggle != 0`隔tick gate；未排除self，Y固定0；同时夹带state500/501 definition/child transform。 | placement与行为均不同。下一个C05包须只接native teleport；旧combined入口只能留direct兼容。 |
| state500/501 extra | 当前playable C05只处理400/401；C25a definition transition只处理state8000..8999，正式源码中无500/501对应战斗路径。 | `BattleEarlyFrameAdvanceModule`每tick可能切definition、frame并传播到child。 | 新确认的Unity-only可观察差异；不是既有用户例外，必须从production移除或由用户另行明确保留。 |
| C06 physics transaction | C05全barrier后按slot执行`step_physics`并立即`normalize_native_dead_character_resources`；motion-hold/relation-negative/state10等gate属于physics自身。 | `SerialTickAll`逐entity执行Transit→SimTU→snapshot；character physics在Transit，non-character把C04+physics+landing/state/death放在SimTU；frame delay/link/cpoint会提前跳过整个组合，dead-resource normalize无同序owner。 | 不能整体搬`SerialTickAll`；C06必须在C04/C05后另建per-slot transaction，frame/state尾留C25。 |

首个可实施包确定为`NTSD28-B3-C04-FRAME-MOTION-PRODUCTION-OWNER-001`：只建立显式C04 phase、从
production C03和non-character physics入口移除重复基础`dvx/dvy/dvz`写入，并保留direct compatibility。
完整linked-platform/delay/`dx/dy/dz`不伪装成已实现，另建B4依赖包。

test-first至少覆盖：C03后velocity仍不变、C04后一次变化；slot0与slot50 barrier；timer/frame-delay不阻止C04；
depth tie；production同tick不重复；direct `CharacterInputAll`/`SimTU`兼容入口保持；actual phase首差推进到C05。

## 未关闭边界

- C04完整linked-platform/delay/positional字段仍是B4数据与行为缺口。
- C05 native teleport与Unity state500/501 extra单独处理，不并入C04包。
- C06物理/normalize与C25 frame/state拆分风险高，必须在前两项后处理。
