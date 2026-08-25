# R8-WP01D-01 — generic C++ effective-pic / DAT sprite mapping contract

> 日期：2026-08-23  
> 状态：`IN_PROGRESS / FIRST REPAIR RUNTIME_PENDING`  
> Change ID：`R8-SPRITEMAP-001`

## Goal

不以任何具体角色、技能或 OID 为修复入口，按 C++ Release 所有实体共用的
`current DAT -> frame.pic -> unk_318 -> sprite range -> local pic -> source rect` 链，关闭
`D-RENDER-006` 的第一个通用代码差异，并为后续全 DAT catalog / slice / UV / CentralOnly
命令认证建立可重复的矩阵。

## Scope

1. 只读闭合 C++ `run_state_special_pre_collision`、`Renderer::draw_entity`、
   `Renderer::load_sprite` 与 `SpriteSheet::src_rect`；
2. 核对 Unity `ApplyStateDataTransform`、`GetRenderPicIndex`、DAT `SpriteFileInfo`、
   `BattleSpriteCatalog`、atlas binding 与 CentralOnly command；
3. 第一修复批只允许：
   - state `8000..8999` 把 `140` 写入通用 `RenderPicOffset`，不得写入 hit-stop；
   - raw `pic == 999` 在 offset 前保持隐藏；
   - 修正把旧错误写成正向合同的 self-check；
4. 后续同一 WP 的 catalog/atlas 子批必须用全部已加载 DAT/frame 的矩阵，不能用单角色、单技能或
   单 OID 成功替代全局合同；发现另一处 first-difference 时先记录，再决定独立 Change ID。

## Authority / Evidence

- C++ Release（均参与 `Makefile`）：
  - `src/entity/game_tick.cpp:352-383`：state `8000..8999` 切换当前 DAT、frame=0、
    `unk_318=140`；
  - `src/render/renderer.cpp:581-624`：先以 raw `fd->pic == 999` 隐藏，再计算
    `render_pic = fd->pic + e.unk_318`，按声明顺序选择 inclusive range；
  - `src/core/loading.cpp:100-120`、`include/renderer.h:9-22`：DAT `row` 是横向 columns，
    local pic 以 `% row` / `/ row` 取 source rect；
  - `Makefile:12,33,34`：loading/parser/renderer 进入 release build。
- Unity source-confirmed first-difference：
  - `LF2Entity.ApplyStateDataTransform(..., true)` 当前写 `HitStun=140`；
  - `GetRenderPicIndex()` 当前对 raw `pic=999` 也先加 offset；
  - 既有 self-check 明确期待 `HitStun=140`，是陈旧的错误 oracle。
- Evidence 等级：上述字段与顺序为 `VERIFIED`（C++ release source）；真实 C++ executable full trace
  仍由 `R1-WP02` 保持 `BLOCKED`。

## Files likely involved

- `Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs`
- `Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs`
- 本 Task、`R8-SPRITEMAP-001` Record、Ledger、STATE、D-RENDER-006、R8 matrix 与 handoff。

第一批不得修改 DAT、BMP、scene、atlas 资源、shader、Mesh、URP feature 或某个对象的专项类。

## Unknowns

- 全部 production loaded DAT/frame 的 catalog key、source rect、slice/page、UV 和 central command
  是否还有第二处差异；
- Texture2DArray 实际像素复制与 Game/Scene 最终可见结果；
- 真实玩家触发的全部技能视觉序列；
- C++ full trace / S5。

## Deliverables

1. `R8-SPRITEMAP-001` 最小通用字段/隐藏合同修复；
2. source-derived self-check，证明 offset 与 hit-stop 独立，raw `pic=999` 不受 offset 影响；
3. 全 DAT catalog / atlas / command 子批合同与结构化 first-difference 输出；
4. D-RENDER-006、STATE、matrix 与 handoff 的分层状态更新。

## Verification

1. scoped diff 只包含本合同允许的通用代码与测试；
2. Unity fresh compile 0 error；
3. `BattleRuntimeSelfCheck` 覆盖 state8000 writer、普通 pic+140 与 raw pic999；
4. 后续全 DAT audit 必须输出 definition/frame/raw pic/effective pic/range/source path/expected rect/
   catalog rect/binding mode/slice/page/UV；
5. live `NTSD_Battle` CentralOnly 命令至少取得通用动态选择的 S4 witness，并保证 Legacy 不成为
   production owner；
6. `Tools/Validate-ChangeLedger.ps1` 与 scoped diff check 通过。

## Stop conditions

- C++ source contract 无法闭合或发现当前 source 不进入 release build；
- 修复需要按角色、技能、OID、frame ID 或资源文件名分支；
- 需要改变 CentralOnly/Texture2DArray/dynamic Mesh/URP、1.5× scale、fixed-world camera、容量、
  30 Hz/FrameInputSet、SoA/ECS/pool/worker/0-GC；
- 需要修改 DAT/BMP/scene 来掩盖代码差异；
- 需要运行、构建、修改或向 C++ authority 写入；
- 发现与本 effective-pic 合同无关的第二个 first-difference。

## Out of scope

- 任何角色/技能专项补丁；
- gameplay pass ordering、input、collision/hit、CPoint/held/opoint；
- T8 默认 `stage.dat`、Android、1000 AI、Windows Player 与服务器；
- C++ executable、trace instrumentation、hook、patch 或 authority 写入。

## Preflight result

- 用户已明确批准 `R8-WP01D / D-RENDER-006`，并要求不得依赖具体角色/技能；
- C++ 与 Unity 的通用 effective-pic 字段 first-difference 已静态闭合；
- 对仓库当前可直接读取的 23 个 DAT file-range/BMP 定义做了无写入静态矩阵：当前
  `ResolveEffectiveGrid` 的最终横向列数与 C++ 在这 23 项上相同，故不能把 row/col 猜测直接当作本次
  用户症状根因；它仍须在后续 all-loaded-DAT audit 中作为风险项验证；
- 第一批可在不触碰批准适配边界、不增加对象特判的前提下最小实施。

## First repair result

- state8000 writer现写`RenderPicOffset=140`且不写HitStop；raw pic999在offset前保持隐藏；
- 首次10:11:07 self-check暴露GT-10/GT-11陈旧HitStop oracle，失败已留档并按同一C++合同修正；
- fresh source 10:12:16 < DLL 10:12:32，Console compile error=0；10:13:22 full self-check PASS；
- `R8-SPRITEMAP-001`提升为`RUNTIME_PENDING`，只关闭第一个代码差异的S1～S3；
- 下一步必须建立独立all-loaded-DAT catalog/slice/UV/CentralOnly command审计，不得用本结果直接关闭
  D-RENDER-006。
