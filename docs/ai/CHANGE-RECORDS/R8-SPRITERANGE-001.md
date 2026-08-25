# R8-SPRITERANGE-001 — DAT overlapping sprite range first-declared ownership

<!-- CHANGE-RECORD
id: R8-SPRITERANGE-001
status: VERIFIED
code-path: Assets/NTSD/Scripts/Animation/Manager/CharacterAnimtorManager.cs
code-path: Assets/NTSD/Scripts/Test/Editor/BattleSpriteOverlappingRangeEditorTests.cs
authority: J:\QQFile\NTSD2.4\ntsd_release\src\render\renderer.cpp:590-606 / B-R8-R08-02 / USER-APPROVED-2026-08-24
evidence: CPP-RELEASE-SOURCE-FIRST-RANGE-WINS / UNITY-FRESH-COMPILE-0-CS / FOCUSED-2-OF-2 / ATLAS-REGRESSION-29-OF-29 / NORMAL-PLAY-CONSOLE-0 / R08-PASS / FULL-SELFCHECK-PASS / LEDGER-PASS
-->

> 创建日期：2026-08-24  
> 状态：`VERIFIED / USER APPROVED / UNITY RUNTIME`

## 1. 原状与first difference

- R08正式Play在资源预热阶段、探针fixture创建前抛出：
  `Duplicate battle sprite key (56,112); overlapping DAT file ranges are not allowed.`；
- OID56 DAT声明`file(106-120)`后又声明`file(112-200)`；2026-08-24按项目正式DAT解密合同只读审计
  `data.txt`全部137个对象，137/137成功解析、共347条范围，只有这一组重叠；
- C++ renderer按声明顺序首个匹配后break；Unity异步sheet完成顺序会覆盖`stagedSprites[index]`，catalog又拒绝重复key；
- 结论：这是Unity通用sprite-range adapter差异，不是OID56 gameplay或本次路径替换造成的范围变化。

## 2. 计划改动

- 在加载调度前以DAT files顺序建立first-owner mask；
- sheet任务只创建/发布其owner pic，任务完成先后不改变结果；
- catalog构建复用同一声明顺序，后续范围跳过已拥有pic，并从owner sprite取得真实texture；
- builder的duplicate-key异常保护保持不变；
- 新增通用overlap focused test，不写OID56特殊分支。

用户于2026-08-24明确批准本Change。代码写入仍必须保持上述最小范围，并在每个验证阶段回写实际证据。

## 3. 不可回退边界

- 不改DAT、C++、CentralOnly、atlas policy、pivot、sorting、gameplay或R08 pass；
- 用户未批准前不得写上述production/test脚本；
- 出现架构扩大或新first difference立即停止。

## 4. 实际改动（2026-08-24）

- `CharacterAnimtorManager.LoadCharacterSpritesAsync`：按每个角色DAT `files`原始顺序建立ownership，并把只读
  owned effective-pic集合传入并行sheet任务；完全被前序范围覆盖的sheet不进入处理队列；
- `ProcessAndCreateSpritesAsync`：只为当前file拥有的effective pic创建并发布Sprite，异步完成顺序不再决定重叠归属；
- `BuildBattleSpriteCatalog`：复用同一ownership，first texture、source path、rect、pivot及legacy sprite均来自真实owner；
- `BuildFirstDeclaredSpriteOwnership`：新增通用声明顺序合同，保留builder duplicate-key异常作为最终不变量；
- 新增`BattleSpriteOverlappingRangeEditorTests`，覆盖106-120/112-200 ownership及later sheet先完成的反序staging。

本Change没有修改DAT、C++、`BattleSpriteCatalogBuilder`、CentralOnly/atlas policy、gameplay或R08探针。

## 5. 验收与回滚

- 验收按对应Task的七层verification执行；
- 回滚仅限本Change未来获批后的代码与focused test；不得回滚已恢复角色资源或R08 test-only probe。
- 当前仅`CODE_WRITTEN`；Unity编译、focused、Play、R08与self-check尚未运行，不能标通过。

## 6. 验证日志

- 第一次Unity force-all import/compile：`FAIL`。新focused test使用当前项目NUnit不支持的泛型
  `Does.Contain(int)`链式约束，产生5条test-only `CS1503`；production脚本没有编译诊断。修复范围只允许把
  这些断言改为`HashSet<int>.Contains(...)`布尔断言后重新fresh compile。
- 修正test-only断言后第二次Unity force-all import/compile：`PASS`。`Assembly-CSharp-Editor.dll`时间
  `2026-08-24 06:41:50.256`晚于test source，UnityMCP Console `error CS`=0、全部error=0。
- focused overlap job `64acdbff4e2f46aeafc519eed0f68d2b`：2/2 PASS；覆盖first ownership与later sheet反序完成。
- existing atlas/catalog regression job `3da7ae8f160a4e7cacf1a6e84a1c1dc5`：29/29 PASS；覆盖common atlas、
  catalog central resolver和desktop/mobile device policy。

## 7. R09 evidence reconciliation（2026-08-24）

- repair后normal `NTSD_Battle` Play 25秒Console0，OID56 duplicate catalog blocker不再出现；
- 同一修复后的R08正式Play完成4500 ticks并写入PASS，merge/dormant/split Central submission与cleanup通过；
- R08-R04之后fresh完整`BattleRuntimeSelfCheck`于`02:27:38Z`写入PASS，其中sprite range断言继续通过；
- Change Ledger validator在R08与R04收口均PASS；
- 原Task Verification 1～7现全部有对应证据，因此本Change从`FOCUSED_TEST_PASS`推进为`VERIFIED`。

该升级只证明通用first-declared ownership在当前正式DAT/catalog/Play覆盖中闭合，不把它扩大为所有未来资源或
C++ executable full-trace证书。
