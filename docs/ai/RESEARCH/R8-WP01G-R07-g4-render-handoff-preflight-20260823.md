# R8-WP01G-R07 — G4 central render handoff / writeback preflight

> 日期：2026-08-23  
> 状态：`READ-ONLY PREFLIGHT COMPLETE / NO SCRIPT CHANGE`  
> Authority：C++ Release live source（只读）

## 1. 结论

G4 当前没有新发现的“C++ source 已确认、Unity production 尚未实现”的代码差异。现有实现已经分别
关闭到 source + 自动证据层，普通 CentralOnly Game/SceneView submission 也有 S4 Play 证据；尚缺的是把
pass 顺序、hit-record writeback、liveness/generation、current-DAT shadow identity、visibility cache 与
CentralOnly fail-closed ownership 放进真实 production tick 的联合运行证书。

由于六个 D-ID 的风险和前置不同，不能用一个大探针一次裁决。G4 拆为：

1. `R8-WP01G-R07A`：`D-SCHED-009 + D-RENDER-002`，先证明 render callback 时点和会影响下一 tick
   hit-record capacity/RNG 的 writeback；
2. `R8-WP01G-R07B`：`D-RENDER-003/004/005`，证明 active/pending/dormant/generation、current-DAT
   223/224 identity 与 EntityVisible/ShadowVisible 的真实 command/GPU 边界；
3. `R8-WP01G-R07C`：`D-RENDER-001`，证明 CentralOnly cold/current/last-good/replacement 的 URP
   pixel ownership；不得通过恢复 Legacy 绘制来通过。

第一实施包R07A现已完成到`UNITY JOINT S4 PASS / C++ FULL TRACE BLOCKED`；只新增Editor-only Play probe，
没有修改production。下一依赖包是R07B，但仍保持独立批准门。

## 2. C++ Release source contract

- `Makefile:11-35`确认`src/entity/game_tick.cpp`与`src/render/renderer.cpp`进入正式release构建；
- `game_tick.cpp:2061-2083`的顺序为PreFrame/background、current wave/stage immediate spawn、
  `pre_postprocess_render`、FramePostProcess、LateEntityUpdate；
- `renderer.cpp:1300-1438`按active slot收集、signed Z稳定排序，并逐entity执行shadow→body→overlay→
  hit-record；
- `renderer.cpp:687-758`在本次render中推进可画hit-record age，并只在invalid tail处减少count；
- collision kind0下一tick读取`hit_record_count < 10`，成功追加会消费两次global RNG。因此writeback
  不能降级成任意LateUpdate视觉副作用；它必须在C++对应render pass内完成。

以上为`VERIFIED(source)`，不是C++ executable trace。

## 3. Unity current mapping

- `NTSDBattleTickSystem.RunPresentationAndCleanupPhase`当前顺序为PreFrameBounds→CurrentWaveStage→
  RenderDispatch→FramePostProcess→LateEntityUpdate；
- `RenderDispatch`在non-worker publication后立即调用`FinalizePublishedHitRecordCycle`，CentralOnly
  no-publication使用`AdvanceHitRecordsWithoutPublication`；worker publication也在capture后立即finalize；
- cycle finalizer以stable id/runtime slot/generation/count保护一次性writeback，LateUpdate/worker ack只剩
  幂等fallback；
- `BattlePresentationShadowBuild`在advance前冻结hit-record sample，command读取本次render age，live owner
  在RenderDispatch内推进；
- active/pending/dormant/FirstPresentationTick/current-DAT/visibility与central ownership均已有独立合同和
  self-check，但没有一份G4联合production Play报告。

## 4. Current evidence classification

| D-ID | 当前最高证据 | 仍缺什么 |
|---|---|---|
| `D-SCHED-009` | `UNITY JOINT S4 PASS / C++ FULL TRACE BLOCKED` | R07A已完成同一production tick的stage/render/postprocess/late与spark writeback联合观察；只剩R1-WP02 full trace证据限制 |
| `D-RENDER-001` | source + self-check；普通Game/SceneView central pixels S4 | cold/current/last-good/replacement在真实URP Play的central-only ownership |
| `D-RENDER-002` | `UNITY JOINT S4 PASS / C++ FULL TRACE BLOCKED` | actual producer、published/no-publication、RNG、frozen/live age与Late幂等均由R07A Play闭合；只剩R1-WP02 full trace证据限制 |
| `D-RENDER-003` | logic-half + focused/self-check | pending/dormant/generation/T+1在真实central command与像素中的边界 |
| `D-RENDER-004` | exact identity self-check | current-DAT 223/224在真实Play的shadow command/像素；若current DAT不可达则诚实BLOCKED |
| `D-RENDER-005` | writer inventory + self-check | production writer reachability以及command/GPU不发生额外隐藏 |

WP01D-06已经证明普通Game链可形成3 snapshots→6 commands→1 draw；WP01D-07已经证明真实SceneView
central lease与575个isolated pixels。它们证明中央绘制链存在，不自动证明上述特殊状态和writeback。

## 5. R07A implementation boundary（已执行并收口）

R07A优先复用WP01C-04真实collision/hit生产路径产生hit record，不得以直接写`HitRecordCount`作为主要
Play结论。exact age gap、10-slot capacity、RNG call count、no-publication与idempotence继续使用既有
focused/self-check矩阵；Play证书只补真实producer与pass时点，不能用狭窄Play替代exact自动断言。

如果当前诊断无法在一个tick内记录snapshot age、RenderDispatch后live age、FramePostProcess/Late状态，
允许新增Editor-only probe；probe只能观察或在安全初始fixture边界配置参与者，不得直接调用finalizer、
advance、candidate/hit writer或修改runtime结果制造PASS。

执行结果见`R8-WP01G-R07A-render-writeback-joint-runtime-evidence-20260823.md`。最终probe遵守上述边界，
worker Play tick843～846、focused与full self-check均PASS；R07A不再是当前执行项。

## 6. Stop / reopen conditions

- production Play显示hit record仍到LateUpdate或下一tick才推进；
- next-tick 10-slot gate/RNG与C++ source合同不一致；
- worker与non-worker的writeback次数或tick不同；
- 需要改变pass order、RNG、FrameInputSet、worker、CentralOnly或lockstep checksum；
- 需要修改C++、DAT、scene/URP asset或恢复Legacy；
- current fixture无法产生真实hit record或无法取得pass内观察点。

命中以上任一项时停止R07A并登记first difference；若是production差异，另建最小修复Change，不在
certification probe中顺手修改。

## 7. R07B / R07C feasibility result

- `Assets/NTSD/Config/data.txt`正式包含OID7、8、51、223、224；R07B不需要新增或修改DAT；
- pending、generation、death/effect/hit-stop都有production producer。R07B可以要求正式producer产生状态，
  禁止probe直接写结果字段；OID7/8→51 dormant/split由独立R08认证，R07B不重复该fixture；
- OID223/224可通过正式factory按OID生成真实current-DAT对象；exact CLR shell/current-DAT mismatch仍由
  existing self-check负责，Play不应伪造该结构；
- CentralOnly已有Editor-only feature registration、failure plan、submission lease和cold/current/stale/
  replacement exact self-check；R07C可在不改URP asset的情况下补真实current→failure→replacement像素证据；
- live URP自动重注册与cold全局状态恢复仍是执行时unknown，已写入R07C stop conditions。

对应Task/Handoff已经建立。后续执行状态更正：R07A与R07B均已按独立合同收口；R07B只关闭
D-RENDER-003的pending/generation/T+1子集，dormant子分支等待独立R08，因此R08完成前不得把
D-RENDER-003整体关闭。R07C仍保持`PLANNED / APPROVAL PENDING / NO EXECUTION`。R07B证据见
`R8-WP01G-R07B-central-liveness-identity-visibility-runtime-evidence-20260823.md`。
