# R8-WP01G-R08-R01 — overlapping DAT sprite range first-declared-wins repair

> 建立日期：2026-08-24  
> 状态：`VERIFIED / USER APPROVED / B-R8-R08-02 CLOSED`  
> Change ID：`R8-SPRITERANGE-001`  
> Blocker：`B-R8-R08-02`

## Goal

让Unity角色图片预热与中央catalog对DAT重叠`file(lo-hi)`范围采用C++ release的统一规则：按DAT声明顺序，
同一pic由第一条包含它的范围拥有；后续范围只能提供尚未被前序范围覆盖的pic。不得按OID56或文件名特殊处理。

## Authority / Evidence

- C++ release `src/render/renderer.cpp:590-606`按`char_data->sprite_ranges`顺序扫描，首个包含`render_pic`的范围
  命中后立即`break`；即first-declared-range-wins；
- Unity `CharacterAnimtorManager.LoadCharacterSpritesAsync`并行处理sheet，并在完成时直接写`allSprites[targetIndex]`，
  当前完成顺序可覆盖重叠index；
- Unity `BuildBattleSpriteCatalog`按每个file range再次Add全部key；`BattleSpriteCatalogBuilder.Add`遇重复key直接抛异常；
- 正式Play实际异常：`Duplicate battle sprite key (56,112)`；正式OID56 DAT范围`106-120`与`112-200`重叠；
- 2026-08-24以项目同一DAT解密密钥/减法算法只读审计`data.txt`全部137个对象：137/137成功解析，
  共347条`file(lo-hi)`范围，只有OID56这一组重叠；修复仍必须是通用声明顺序合同，不能写OID特例。

## Scope

### 允许

1. 在`CharacterAnimtorManager`预计算每个character/file/local-pic的first-declared ownership；
2. 异步sheet处理只把该file拥有的pic写入`stagedSprites`，不让任务完成顺序裁决重叠；
3. `BuildBattleSpriteCatalog`按相同first-declared规则跳过后续重复pic，并让每条entry使用其真实owner sheet texture/rect；
4. 新增focused Editor test覆盖106-120与112-200、反向任务完成顺序、owner texture/rect和无重复key；
5. fresh compile、focused tests、现有sprite/catalog回归、正常Play、R08 probe重跑、self-check与ledger validator。

### 禁止

- 不修改DAT范围，不删除OID56资源，不按OID/文件名硬编码；
- 不放宽`BattleSpriteCatalogBuilder.Add`的通用重复key保护来隐藏上游错误；
- 不改CentralOnly/Texture2DArray/atlas policy、render sort、pivot、1.5×scale、camera、gameplay tick或C++；
- 不在此包修改R08 merge/split逻辑或探针验收标准。

## Files likely involved

- `Assets/NTSD/Scripts/Animation/Manager/CharacterAnimtorManager.cs`；
- 新 focused test：`Assets/NTSD/Scripts/Test/Editor/BattleSpriteOverlappingRangeEditorTests.cs`及meta；
- Change Record/Ledger/STATE/R08 Task/Handoff。

## Verification

1. source-level first-declared ownership fixture；
2. focused overlapping-range test；
3. existing sprite catalog/atlas tests；
4. Unity full import compiler error=0；
5. 正常Play不再出现duplicate key，OID56重叠pic112按第一段106-120绑定；
6. 重新运行R08 merge/dormant/DJA/split probe；
7. full self-check和Change Ledger validator。

## Stop conditions

- 发现C++实际不是first-declared-wins；
- 修复必须改变DAT、atlas架构或CentralOnly contract；
- 重叠owner修复后出现新的production first difference；
- 需要扩大到资源格式重构或移动端渲染重构。

## Out of scope

R08 gameplay repair、AI、T8、IL2CPP、Android、服务器、C++ executable/full trace。

## Authorization

用户于2026-08-24明确批准执行`R8-WP01G-R08-R01 / R8-SPRITERANGE-001`并恢复总目标。批准范围仅限
本Task所述通用first-declared ownership修复、focused test和既定验证；不授权修改DAT、C++、gameplay或架构边界。

## Final result（2026-08-24）

通用first-declared ownership已完成并通过fresh compile0、overlap2/2、atlas/catalog29/29、normal Play
Console0、R08 4500-tick PASS、后续完整self-check PASS与Ledger validator。`B-R8-R08-02`关闭，
`R8-SPRITERANGE-001 / VERIFIED`。本结论不修改DAT，不按OID特判，也不替代C++ full trace。
