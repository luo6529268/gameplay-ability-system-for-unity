# R7 optimization recertification — complete inventory summary

> 日期：2026-08-22  
> 状态：`INVENTORY AND REPAIR SEQUENCE COMPLETE / R8 PENDING`

## Completed inventory groups

R7计划列出的优化组均已完成C++ source contract→Unity fallback/optimized mapping：

1. PreInteraction no-op proof；
2. LateEntityUpdate exact-character；
3. Frame/Recovery SoA；
4. AI sensing/index与完整character decision chain；
5. broadphase / role-aware / Loose Quadtree；
6. cached/frozen presentation、central renderer；
7. worker publication/presentation acknowledgement；
8. pool、slot allocator、generation、dynamic capacity。

## Closed or conditionally certified

| Item | Result | Boundary |
|---|---|---|
| `R7-PERF-001` | cross-pass stale proof已删除；focused 15/15 + full self-check PASS | PlayMode/C++ trace待 |
| `R7-LATE-001` | 9995→4000→8000、9996 4×217+218、RNG/cursor已修 | real DAT/PlayMode待 |
| `R7-FRAME-001` | no-code conditional certification；无新confirmed difference | D-MOV-005可达性仍INFERRED |
| `R7-AI-01` | sensing/index no-code conditional certification | 完整decision由AI-02处理 |
| `R7-BROAD-01` | role-aware/Loose语义条件认证，83/83 | production仍默认BruteForce |
| `R7-PRES-WORK-01` | frozen/central/worker positive 46/46 + full self-check | joint fixture/PlayMode待 |
| `R7-CAP-01` | allocator/pool positive 44/44 + full self-check | D-CAP-001待决 |

## Open source-confirmed gameplay/data differences

### P0 — AI decision chain

- `D-INP-007A`：C++ outer gate内39个有序positions；Unity缺30个；
- `D-INP-007B`：Unity把positions28/38/39放到gate外；
- `D-INP-008`：optimized snapshot缺OID11 frame290 helper所需current `HitJ`；
- `D-INP-009`：现有75/75只是两条Unity路径的共享缩减oracle，不覆盖C++链。

这些项会改变技能选择、组合键、RNG数/顺序和early return，是下一组production修复的最高优先级。

## Open acceptance infrastructure

- `D-TEST-001`：至少一个focused suite留下跨测试static状态；fresh-domain full self-check仍是强制门；
- `D-TEST-002`：worker human-input fixture错误期待current key同tick清零；production与C++一致；
- `D-TEST-003`：缺driver worker buildPresentation=true→central materialize→ack→next tick joint fixture。

## Open performance/deployment

- `D-PERF-002`：LooseQuadtree未部署为普通production backend；当前仍为BruteForce；
- `D-PERF-003`：worker single-flight与当前`maxCatchUpTicksPerFrame=1`相容，是未来pipeline边界，不需要立即修；
- `D-CAP-001`：DesktopExtended在battle seal后不能增长；Windows默认512后实际fail closed，和“无production hard cap”
  文档合同冲突。

## Repair order

1. `R7-AI-02A`：source-derived 39-position dispatcher/gate/RNG fixture；保持production red；
2. `R7-AI-02B`：HitJ capture/publication/refresh/0 B数据合同；
3. `R7-AI-02C`：OID6/7/8/11 helpers；
4. `R7-AI-02D`：OID10/1、9/2、32/19/33 helpers；
5. `R7-AI-02E`：OID34/label464/35/36/38/39 helpers；
6. `R7-AI-02F`：完整1–39 ordered dispatcher；统一把28/38/39放回gate内，并运行profile-pair/RNG/0 B验收；
7. `R7-TEST-002`：修正stale human-key test-only合同；
8. `R7-TEST-003`：增加正式worker/central/ack联合夹具；
9. `R7-TEST-001`：二分定位static pollution owner；
10. `R7-BROAD-02`：真实production backend parity/performance矩阵后再决定是否把Loose设为默认；
11. `R7-CAP-01A`：先固定Desktop capacity/strict 0 B/admission合同；获批后才进入01B代码实现；
12. R8真实Play Mode/Player认证。

## 2026-08-23 repair follow-up

- `D-TEST-001` 已由R7-TEST-001关闭：static EmptyFrame sentinel污染及dependent隐藏依赖已隔离；
- `D-TEST-002` 已由R7-TEST-002关闭；
- `D-TEST-003` 已由R7-TEST-003关闭；
- `D-PERF-002` 已由R7-BROAD-02作出“保留BruteForce、不改GameConfig”的部署决策；切换Loose需要未来
  current-build real A/B与R8 scene parity的新证据；
- `D-CAP-001` 已由R7-CAP-01A按交付合同澄清关闭：Desktop无固定产品active cap，但每局在seal前准备
  有限容量，battle内strict 0 B并在超预算时确定性拒绝；现有实现符合，无需01B代码；
- R7 repair orders 1–11全部关闭。历史Open列表只保留发现时上下文，当前下一阶段是R8。

## Stop rules

- 不能绕过AI-02A直接整体搬运39 positions；
- 02F前不得把部分helper接成production默认链；
- 不得把fps改善当作behavior等价；
- 不得在未决容量合同前解除battle seal或允许tick内任意new；
- T8默认`stage.dat`与Android真机仍按用户要求排除。
