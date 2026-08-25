# R8-SPRITEMAP-003 — C++ DAT row-horizontal catalog contract

<!-- CHANGE-RECORD
id: R8-SPRITEMAP-003
status: RUNTIME_PENDING
code-path: Assets/NTSD/Scripts/Animation/Manager/CharacterAnimtorManager.cs
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
authority: J:\QQFile\NTSD2.4\ntsd_release\src\data\dat_parser.cpp:335-368; src\core\loading.cpp:108-120; include\renderer.h:9-24; src\render\renderer.cpp:581-624; Makefile
evidence: R8-SPRITEMAP-002 first Play audit found 1301 generic differences across 4373 entries
-->

> 创建日期：2026-08-23  
> 最后更新：2026-08-23  
> 类型：production / render resource mapping / generic

## 1. 状态与范围

- 当前状态：`RUNTIME_PENDING`
- 所属Work Package：`R8-WP01D-03`
- 允许脚本：`CharacterAnimtorManager.cs`与对应`BattleRuntimeSelfCheck.cs`夹具；
- 禁止：角色/技能/OID/frame/BMP特判，DAT/资源/scene、shader/Mesh/URP、gameplay、C++ authority。

## 2. Authority / 需求依据

- C++ parser不交换DAT row/col；loading把`sr.row`原样作为`SpriteSheet.cols`；renderer用该cols做
  `localPic % cols`和`localPic / cols`；
- Unity当前`ResolveEffectiveGrid`先把`fileInfo.col`当横向列，并按BMP物理尺寸有条件交换；这不是C++规则；
- 用户明确要求从C++通用流程排查，不提供角色/技能以避免专项修复；
- 首次全量Play audit覆盖100 definitions/4373 entries/6674 frames，累计1301 differences，足以证明
  generic production first difference；记录中的visualDataId仅为证据行，不进入实现分支。

## 3. Unity 原状

- `ResolveEffectiveGrid`的输出`col`被`BuildIndexedSpriteRects`作为横向除数；
- 物理尺寸完全匹配时可能交换为C++方向，存在BMP边缘/尾部/部分sheet时则保留错误方向；
- 结果表现为部分sheet正确、部分sheet从第一个横向换行点开始持续取错图片；
- self-check把该heuristic及`col`横向解释写成green oracle。

## 4. 计划改动

| 文件 | 符号 | 改前 | 改后 |
|---|---|---|---|
| `CharacterAnimtorManager.cs` | `ResolveEffectiveGrid` | BMP尺寸heuristic决定是否交换 | 固定输出vertical rows=`fileInfo.col`、horizontal columns=`fileInfo.row` |
| `BattleRuntimeSelfCheck.cs` | P2 grid/prewarm fixtures | 保护Unity旧heuristic | 保护C++ row-horizontal与既有hole/transaction职责 |

## 5. 不可回退边界

- 保留Unity bottom-left Y转换、共享Texture2D、Sprite.Create、catalog/atlas/CentralOnly架构；
- 保留out-of-bounds cell为hole、完整prewarm transaction和对象所有权；
- 不改变战斗tick、输入、碰撞、opoint、状态、位置、层级或实体容量。

## 6. 实际改动

- `ResolveEffectiveGrid`删除全部BMP宽高猜测；null返回0/0，正常固定输出vertical rows=`fileInfo.col`、
  horizontal columns=`fileInfo.row`；调用者、`BuildIndexedSpriteRects`和Unity bottom-left转换未改；
- self-check的非对称grid断言改为无论纹理尺寸都保持C++ row-horizontal，并加入row3/col2的换行边界
  localPic2/localPic3 source-rect witness；
- 既有40-cell transaction fixture只把synthetic DAT声明从row5/col8改为row8/col5，使其继续测试原有
  40 materialized cells/hole/ownership职责，而不再依赖错误语义；
- 首次full self-check在production flash parser fixture失败：fixture仍用`col`计算synthetic texture width、
  用`row`计算height，导致C++ row-horizontal resolver把合法key视为hole；已把夹具纹理维度改为
  width=`row*(w+1)`、height=`col*(h+1)`，catalog range/overlap职责不变；
- 没有角色、技能、OID、frame或资源文件名的production分支。

## 7. 验收与证据

| 层级 | 结果 | 状态 |
|---|---|---|
| C++ source | parser→loading→SpriteSheet→render闭合 | `PASS` |
| Unity first Play audit | 1301 generic differences；cleanup baseline另有probe bug | `FAIL / FIRST DIFFERENCE` |
| Unity compile | 修正flash fixture后fresh compile，主Editor idle、Console compiler error=0 | `PASS` |
| self-check | 首次FAIL已保留；第二次结果文件10:41:17为`PASS` | `PASS` |
| repaired all-DAT Play audit | 4933 entries；`CPP_SOURCE_DESCRIPTOR_MISMATCH=0`；cleanup 4/2恢复 | `PASS / ROW CONTRACT` |
| GPU/Game/Scene | 未运行 | `PENDING` |
| C++ full trace | R1-WP02 | `BLOCKED` |

## 8. 风险与回滚

- 风险：大量catalog rect会变化；这是C++合同要求，必须用全DAT矩阵而非单样本验收；
- 风险：旧test可能依赖错误heuristic，必须只修与同一source合同冲突的断言；
- 回滚：仅反向恢复本Record列出的两个脚本diff；不得触碰其他用户修改。

## 9. Git / 交接

- 工作树包含大量历史/用户修改；本Record只拥有上表两个脚本的本合同差异；
- 提交：未提交；
- handoff：`HANDOFF-R8-WP01D-generic-sprite-mapping.md`。
