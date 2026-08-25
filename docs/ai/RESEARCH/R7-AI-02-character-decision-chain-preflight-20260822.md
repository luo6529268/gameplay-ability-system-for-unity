# R7-AI-02 — character-specific AI decision chain preflight

> 日期：2026-08-22  
> 状态：`SOURCE-CONFIRMED DIFFERENCE / INVENTORIED / NO GAMEPLAY CHANGE`

## 1. Goal and boundary

只读比较 C++ `InputHandler::prepare_ai_input(...)` 在special scan之后的character/OID-specific
decision chain，与Unity Legacy / `AiDecisionKernel` / canonical-store direct路径。发现差异先登记，
本包不修改production AI。

## 2. Authority sequence

C++ release `src/input/input_handler.cpp:2055-2204` 在单一外层门
`ntsd_rand() % (world.ai_rand5 + 1) == 0` 内，按固定顺序执行 **39个** helper/call position；
任一返回true立即执行input edges并return。void side-effect继续落到后续helper。

39个位置按组如下：

| Position | C++ helpers | Unity status |
|---|---|---|
| 1–6 | first、teammate guard、OID1 close/combo、OID4、OID5 | Legacy与kernel存在，且在outer gate内 |
| 7–13 | OID6；OID7 frame/close/midfar/facing/frame255；OID8 | 两条Unity路径均缺失 |
| 14–16 | OID11 first、frame290 side-effect、DUA | 两条Unity路径均缺失 |
| 17–22 | OID10/1 first/frame271/predicted/midrange、HP team scan、HP advantage | 两条Unity路径均缺失 |
| 23–27 | OID9/2 predicted/midfar/nearest；OID32/19 midfar/close | 两条Unity路径均缺失 |
| 28 | OID33/19/16 predicted DUA | Unity存在，但错误放在outer gate外 |
| 29–30 | OID34/10/5/14 low-HP DDJ、teammate guard | 两条Unity路径均缺失 |
| 31–32 | label464 long/close group | 两条Unity路径均缺失 |
| 33 | OID35 long | 两条Unity路径均缺失 |
| 34–35 | OID36/16 team DUJ、range DUA | 两条Unity路径均缺失 |
| 36–37 | OID38、OID39/10 | 两条Unity路径均缺失 |
| 38–39 | OID52/1/2/21、label591 OID51/2/18/7 | Unity存在，但错误放在outer gate外 |

结论：Unity只保留位置1–6并正确入门；位置7–27和29–37共 **30个 call positions缺失**；
位置28/38/39虽然实现体与C++逐段相符，却被放到outer gate外，改变RNG、技能选择和early return时点。

## 3. Data-contract dependency

大部分缺失helper所需字段已经在 `AiSensingSnapshot` / `AiDecisionInputState` 中存在：OID、frame/state、
X/Y/Z、Vx、facing、HP/HP3/HPMax、PP、team、link、target slot、combo/input bytes。

已确认独立数据缺口：

- C++ OID11 `ai_update_oid11_frame290_side_effect` 读取current frame `hit_j`；
- current optimized `AiSensingSnapshot` 没有`HitJ` row；
- Legacy可通过current DAT frame读取，但optimized无法在不回读GameObject/DAT的前提下等价执行。

登记为 `D-INP-008`，必须先补数据合同，再接OID11 helper；不得用Unity渲染/Frame对象作为optimized热路径真值。

## 4. Common tail audit

C++ `input_handler.cpp:2206-2304` 的active-window movement、held helper、attack/defend/jump RNG、
sub-prewrite与input edges，对应：

- Unity kernel `AiDecisionKernel.cs:571-672`；
- Unity Legacy `SimulationWorld.AiInput.partial.cs:1976-2028`。

本段的条件、常量、左右/上下键、RNG模数和调用顺序当前可映射，未发现新的source-confirmed
difference。coordinate target branch `input_handler.cpp:2309-2353` 也已有对应路径。本预检的first difference
集中在39-position character-specific chain。

## 5. Existing test result and circular-oracle gap

UnityMCP focused job `3eaff2c1bb474565b2dd4c66d02c49db`：

- `AiDecisionKernelEditorTests` + `AiDecisionSoAShadowEditorTests`；
- 75/75 PASS，0 fail/skip；
- 覆盖indexed/legacy parity、RNG clone、snapshot、same-pass visibility、warmed 0 B等Unity内部合同。

该PASS不能反驳C++差异，因为Legacy与optimized都只实现相同的6+3缩减链；现有profile A/B是共享缺失
oracle。登记为`D-INP-009`验收覆盖缺口。

## 6. Difference inventory

| ID | Classification | Difference | Impact |
|---|---|---|---|
| `D-INP-007A` | VERIFIED source difference | C++ 39-position outer-random-gated chain；Unity缺失30 positions | 多角色技能决策、combo/input、RNG call count/order与early-return不一致 |
| `D-INP-007B` | VERIFIED source difference | Unity现有position 28/38/39被放到outer gate外 | 即使缺失helper不触发，这3组也会在C++不执行时额外消费RNG/释放技能 |
| `D-INP-008` | VERIFIED data-contract difference | optimized snapshot缺current frame `hit_j` | OID11 frame290 side-effect无法纯数据等价实现 |
| `D-INP-009` | VERIFIED acceptance gap | Legacy/optimized 75 tests共享缩减oracle | Unity内部全绿不能证明C++ decision chain |

## 7. Proposed implementation Work Packages

以下仅是后续合同拆分；本预检未授权代码：

1. `R7-AI-02A — authority fixture / dispatcher contract`
   - 建立39-position顺序、outer-gate、early return与RNG trace测试；
   - 测试先红，不能改production；
   - 作为所有后续包共同验收入口。
2. `R7-AI-02B — HitJ data contract`
   - 只补current frame `hit_j`在fallback/unified/canonical snapshot的capture/publication/refresh；
   - 验证slot/generation、same-pass refresh、0 B；
   - 不接技能helper。
3. `R7-AI-02C — OID6/7/8/11 module`
   - 实现positions7–16；
   - 每个helper用source-derived fixture独立测试，集成前标`待集成测试`。
4. `R7-AI-02D — OID10/1、9/2、32/19/33 module`
   - 实现positions17–28；
   - 包含full/team scan顺序、strict tie与side-effect；
   - OID33仍不改变默认dispatcher，直到final integration。
5. `R7-AI-02E — OID34/label464/35/36/38/39 module`
   - 实现positions29–37；
   - 明确保留OID36 team scan命中随机门后即使无人求助仍return true的C++语义。
6. `R7-AI-02F — full dispatcher integration`
   - 一次性切换为完整1–39顺序；
   - 把position28/38/39移回outer gate；
   - Legacy与DataOriented分别对source-derived fixtures，随后profile-pair逐seed RNG/input对照；
   - 依赖02A～02E全部完成，未满足前不得激活部分默认链。

为符合用户要求，02C～02E若只能独立验证helper，必须标`待集成测试`；只有02F联合验收通过后才可提升。
新模块不得使用`partial`拆分；具体实例/纯数据执行形态需在02A合同中结合0 GC与现有架构确定，不能在本预检
直接作跨模块架构决定。

## 8. Stop / protected boundaries

- 不修改/运行/构建C++ authority；
- 不修改Unity gameplay、RNG、profile、pass order、capacity、render、input binding；
- 不把75/75或full self-check升级为C++ decision VERIFIED；
- CentralOnly、1.5×、fixed camera、extended capacity、30Hz/FrameInputSet、SoA/ECS、pool/worker/0 GC保持；
- >399 slot的Unity extension另列adapter验收，不反向定义C++ 400-slot扫描。

