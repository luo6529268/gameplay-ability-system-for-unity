# NTSD 2.8-Logan AI render-phase consumer audit

> Change ID：`NTSD28-B3-AI-RENDER-PHASE-CONSUMER-AUDIT-001`  
> 状态：`VERIFIED / GOVERNANCE_ONLY / CONSUMER_CROSSWALK_COMPLETE`  
> 前置：`NTSD28-B3-C25F-J-RENDER-PHASE-BINDING-001 / VERIFIED`

## 1. 裁决

Authority `EntityState28::render_phase_008` 已由 Y/phase 互异双端 raw 唯一绑定到 Unity `NTSDEntityRuntime.HitStop`。AI snapshot 已同时保存物理 `Y` 与 `HitStop`，因此不需要新增 carrier；需要纠正的是把若干 render-phase predicate 错读为 `rows.Y` / `world.Y` 的 consumer。

不能全局替换 Y。Authority `position.y` 仍用于真实空中高度、角色专项动作和跳跃概率，必须保留。

## 2. Authority consumer 闭包

| Authority live path | 语义 | Unity 当前错误 consumer |
|---|---|---|
| `native_ai.cpp:24-31` `confirmed_local_character_candidate` | normal target 要求 `abs(render_phase)<=2` | `AiSensingKernel` ground role/target 的 `Abs(rows.Y)<=2`；`SimulationAiSensingModule` SoA role；`SimulationAiInputModule` legacy facts/spatial role |
| `native_ai.cpp:301-344` cached target 与 primary scan | cached/ordinary target 以 render phase 排除异常对象 | 上述 role index及candidate predicate共同驱动，Y高度变化会错误改变target集合 |
| `native_ai.cpp:771-845` primary + abnormal second scan | state14或`abs(render_phase)>2`进入 abnormal role | `AiSensingKernel` air role；`AiDecisionKernel` abnormal branch；`SimulationAiDecisionModule` legacy fallback |
| `native_ai.cpp:1237-1252,1995-2003` abnormal target dispatch | selected target 的异常判定 | `AiDecisionKernel:476/527`、legacy `SimulationAiDecisionModule:4872/4911` 误读Y |
| `native_ai.cpp:2076-2095` held same-group line obstruction | blocker要求`abs(render_phase)<=2` | `AiDecisionKernel:2073/2100`及legacy `SimulationAiDecisionModule:5750/5786`误读Y |
| `native_ai.cpp:2174-2184` weapon-run state17 | subject `render_phase!=0` | synchronized `ProcessNativeHeld` 的 `rows.Y[self]!=0`；legacy held分支已正确读`rows.HitStop[self]` |

## 3. Unity 数据传播现状

- `AiSensingSnapshot.HitStop[]`、SoA rows `HitStop[]` 已存在，并由 unified writer、runtime fallback与snapshot copy填充。
- `AiNearestSlotFacts`只有`Y/GroundRole/AirRole`，其role目前由`runtime.YInt`计算；实现包需增加`HitStop`事实并改为render-phase分类。
- `SimulationWorld.GetAiYForInputModule`是真实物理Y getter，不得改语义；另加明确HitStop getter或直接使用runtime字段。
- role index、spatial incremental mutation和full-scan fallback必须使用同一predicate，否则indexed/full结果会分叉。

## 4. 必须保留的物理 Y consumer

- `AiCharacterDecisionModule` 中 OID11/动作271/state12/-40 等角色专项高度判断。
- `AiDecisionKernel` OID1/21/17 近身分支的双方Y正负与随机跳跃消费（约1574-1581）。
- `SimulationAiDecisionModule` 上述legacy镜像（约5486-5488）。
- snapshot capture/copy/equality中的`rows.Y`字段本身。

这些读取Authority `position.y`，与render phase无关。

## 5. 实施包

实施包：`NTSD28-B3-AI-RENDER-PHASE-CONSUMERS-001 / VERIFIED`。

2026-09-05 已完成全部列明consumer的HitStop迁移并保留物理Y分支；证据为focused121、all-AI385、NTSD28 broad428、10:29:46 SelfCheck PASS、Scene unchanged与Console0。下一转入C25g frame body / C25i armor recovery。

允许修改：

- `AiSensingKernel.cs`
- `AiDecisionKernel.cs`
- `SimulationAiInputModule.cs`
- `SimulationAiSensingModule.cs`
- `SimulationAiDecisionModule.cs`
- 必要的 `SimulationWorld`只读 getter
- focused tests与被新Authority supersede的旧Y断言

验收必须使用Y/HitStop互异矩阵，分别覆盖normal/abnormal target、role index、full/indexed一致、held blocker和state17；同时用真实高度fixture证明上节物理Y consumer未被替换。RNG分支必须比较draw count/order/hash，不能只看最终按键。

## 6. 结论

- 已观察事实：这是一组共享错误绑定，不是给snapshot再加一个字段的问题。
- 已观察事实：legacy held state17已经正确用HitStop；synchronized held仍错误用Y。
- 已观察事实：目标role/index与held blocker的Y读取均对应Authority render phase；角色专项高度读取仍对应position.y。
- 下一步：test-first迁移上述明确consumer，不触碰C25g/C25i、内容资源或AI其他算法。
