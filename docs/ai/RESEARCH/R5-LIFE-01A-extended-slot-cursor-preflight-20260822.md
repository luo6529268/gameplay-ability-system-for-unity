# R5-LIFE-01A — extended slot/newborn cursor adapter preflight

> 日期：2026-08-22  
> 状态：RUNTIME_PENDING — Extended joint fixture、fresh Unity编译/full self-check已通过；C++ trace/PlayMode待验。  
> 对应差异：D-SCHED-012（cursor subset）  
> Change ID：R5-LIFE-001A

## 结论

C++ Release 的 authority contract是：normal opoint从slot50起选择最低空闲槽；late pass按slot
升序扫描。child落在当前cursor之后会在同一late pass执行，落在已扫描的低slot则到下一
late pass才执行。`MAX_OBJECTS=400`只定义authority fixture的地址范围，不是Unity production
容量上限。

Unity当前实现已经使用`RuntimeSlotAllocator/RuntimeSlotTable.AllocateLowest(50, ...)`并让
`LateEntityUpdateAll`按`0..RuntimeSlotCapacity-1`查询current occupant。既有self-check分别覆盖：

- 400/512→768 allocator的lower-hole-first；
- Authority400 late pass的later-slot same-pass与lower-slot next-pass。

缺口是扩展profile中slot>399的cursor与required/growth adapter没有joint fixture。当前没有
静态证据要求修改gameplay；本包只补MobileExtended与DesktopExtended-growth的high/low cursor
认证，并复用既有lowest allocator证据。

## Authority / mapping

| 边界 | C++ Release | Unity current |
|---|---|---|
| dynamic allocation start | `collision.cpp:1280-1285`，从50找最低inactive | `SimulationWorld.Registry:1117-1184`→`RuntimeSlotTable.AllocateLowest(50)` |
| free/reuse | `GameWorld::free_entity`立即active=false | pending slot release adapter在后续allocation前释放claimed slot；generation只防stale handle |
| late cursor | `game_tick.cpp:687-691` / `run_late_per_entity_update_pass`按0..MAX_OBJECTS-1 | `LateEntityUpdateAll`按0..RuntimeSlotCapacity-1读取current occupant |
| extended capacity | C++无此production需求 | 用户批准A-RENDER-004；MobileExtended 1050 addresses/1000 active，DesktopExtended可增长 |

## Focused acceptance

1. MobileExtended：source slot>399，required child在更高slot时同pass执行；required child在
   较低已扫描slot时本pass不执行、下一pass恰好一次；
2. DesktopExtended：初始512，在注册source/child时增长到>399后，重复同一high/low矩阵；
3. existing auto allocator lower-hole-first assertions仍通过；
4. generation、object count、slot identity不得污染cursor结果；
5. 不把explicit required-slot test当作C++ allocation本身；它只隔离验证extended scan cursor，
   allocation顺序由existing allocator/table tests独立证明。

## Exclusions

- 不改RuntimeSlotAllocator、RuntimeSlotTable、Registry、StructuralWriter、LateEntityUpdateAll；
- 不回退Authority400 production cap，不限制DesktopExtended；
- 不改pending destroy/free semantics、render visibility、D-RENDER-003、D-OP-001；
- 不运行/构建/写入C++ authority，不启动trace或PlayMode。

## Evidence status

- C++ source/release participation：VERIFIED；
- Unity mapping与Authority400/allocator分立fixture：VERIFIED；
- Extended joint cursor fixture：PASS（Mobile/Desktop high/low >399 matrix）；
- Unity compile/full self-check：PASS（fresh assembly 17:14:38；result 17:15:48）；
- C++ runtime trace：BLOCKED；real PlayMode：PENDING。
