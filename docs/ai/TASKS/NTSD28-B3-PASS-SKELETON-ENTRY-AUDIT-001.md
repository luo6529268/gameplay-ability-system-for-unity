# Task Contract — NTSD28-B3-PASS-SKELETON-ENTRY-AUDIT-001

> 状态：`VERIFIED / ENTRY-AUDIT-CLOSED / ORDER-AND-BOUNDARY-DIFFERENCES-CONFIRMED / NEXT-PASS-ORDER-CONTRACT`  
> 所属计划：`NTSD28-UNITY-BATTLE-REALIGNMENT-001 / B3 ENTRY`  
> 建立日期：2026-09-04

## 目标

在B2退出后，完整审计NTSD 2.8-Logan正式playable每个simulation tick及GameSession前后尾部的
实际pass顺序，并与Unity `NTSDBattleTickSystem`、`SimulationWorld`、Host/worker、presentation和
post-frame tail的当前生产链逐项交叉。输出B3只负责的结构边界、不可变快照边界、single-writer归属和
最小实施包；不得把B4～B8各领域的具体行为重写混入B3。

## Authority

- 正式EXE：根目录SHA-256为
  `1277B70BA030A1F33B625EEA20B43834325B280CEC555650BF43CD90A64DAF75`的
  `NTSD2.8-Logan.exe`。
- 正式源码入口：
  `source/ntsd28_playable/src/game_session.cpp::GameSession28::step()`、
  `source/ntsd28_core/src/simulation/simulation_tick_driver.cpp::SimulationTickDriver28::step(...)`，
  并沿其调用的world/frame/physics/collision/spawn/stage/render live closure追踪。
- Unity入口：`SimulationTickDriver` → `NTSDBattleTickSystem.RunTick(...)` → `SimulationWorld`
  各生产pass、worker completion、presentation publication及post-frame tail。

## 允许操作

- 只读authority、Unity源码、现有tests/trace/manifest与治理文档。
- 新建并维护本Task、Change Record、B3 pass-order manifest，以及Ledger/STATE/handoff/总表。
- 可运行只读搜索、源码参与性核验、现有测试和workspace `Temp`诊断。
- `code-path: NONE`；本审计包不得修改C#/C++/tool源码，不得修改Config/DAT、Scene/Prefab、
  ProjectSettings、Packages或authority目录。

## 强制边界

- B3只决定pass骨架、前后置边界、遍历/快照可见性、single-writer和后续owner；Frame/Physics/Revival
  归B4，Collision/Hit/Damage归B5，Catch/Held/Weapon归B6，Spawn/Lifecycle归B7，Stage/Flow归B8。
- Unity的slot容量模型、随机掉武器、固定世界相机等用户例外不转回待修项。
- 不能因某个Unity方法名相似就判定顺序相同；必须追到实际调用者、条件、字段读写和live build参与性。
- 不把legacy与ECS shadow同时写同一逻辑真相；需列出当前双写、后刷、快照或presentation反向依赖。
- function-key Session dispatch在tick前、F7/F8/F9 effect在tick后；B2 carrier完成不等于B3 placement/effect完成。

## 验收

- 形成逐序号authority pass表，覆盖GameSession pre-tick、core step、post-tick和render/presentation handoff。
- 形成Unity现状映射：`EXACT / ORDER_DIFF / BOUNDARY_DIFF / MISSING / DOWNSTREAM / USER_EXCEPTION`。
- 明确每项读快照、写状态、遍历集合、birth/death可见性和RNG owner。
- 明确B3与B4～B8边界，列出不可在B3提前实现的具体行为。
- 选择一个最小、可test-first验证、可回滚的B3首个实施包；如无需代码，也必须给出exit证据。
- Change Ledger validator与`git diff --check`通过。

## 回滚

仅回滚本次新增的B3治理记录、交叉表及状态链接；production与authority零修改。

## 审计结论

- Authority完整顺序已冻结到`docs/ai/MANIFESTS/NTSD28-B3-PASS-ORDER-CROSSWALK.md`，覆盖
  GameSession pre/core/post、C24逐slot嵌套tail及completed-world render snapshot。
- S-01～S-16全部完成B3/后续owner分界：S-11继续为用户随机掉落例外，其余结构性顺序与快照差异进入B3。
- Unity可复用基础为升序动态slot traversal、B2两遍input、Host/worker seam和互斥ECS writer；当前
  `SerialTickAll`逐entity交织与mid-tail `RenderDispatch`不得直接认定已对齐。
- 唯一下一包：`NTSD28-B3-PASS-ORDER-CONTRACT-001`。先建立不可变、allocation-free的2.8顺序合同与红测
  锚点，不在同包重写B4～B8行为。
