# R8-GEOMETRYCHECK-001 — negative-height body risk classification

<!-- CHANGE-RECORD
id: R8-GEOMETRYCHECK-001
status: VERIFIED
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
authority: J:\QQFile\NTSD2.4\ntsd_release\src\entity\hit.cpp / collision_collect.cpp / R8-WP01G-R08-R03
evidence: R-HC-01-20260824-013325 / CPP-RAW-INVERTED-RECT-SOURCE
-->

> 创建日期：2026-08-24  
> 最后更新：2026-08-24  
> 类型：TEST-ONLY / collision geometry / self-check unblock

## 1. 状态与范围

- 当前状态：`VERIFIED / R-HC-01 CLOSED`；
- 所属Work Package：`R8-WP01G-R08-R03`；
- 只允许修正self-check分类与测试夹具；production collision、DAT、parser零改动；
- 不属于本次：R1 full trace、角色/技能、candidate算法、render、AI、T8、Android、IL2CPP、服务器。

## 2. Authority / 当前证据

- C++ `hit.cpp`对body纵向坐标直接执行`bottom=top+h`，`aabb_overlap`使用严格不等式；`h<0`形成倒置rect。
  普通小itr通常不满足两条条件，但一个同时跨过倒置两端点的大itr仍会命中；不能把它概括为全局inert；
- Unity production`BruteForceSceneQuery`同样保留非null body并执行raw `top+h`与strict overlap；
- 正式部署数据只有5个negative-height body，全部为OID58 frame75/76和OID10 frame75/76/77的
  `x39/y-555/w21/h-999`；
- 现有self-check只允许zero-width positive-height itr，尚未分类negative-height body，导致全量检查在R-HC-01提前终止。

## 3. 计划改动

| 文件 | 符号 | 改前职责 | 目标职责 |
|---|---|---|---|
| `BattleRuntimeSelfCheck.cs` | `CheckDeployableResolvedGeometryRisks` | 所有非正body一律invalid | 精确统计当前已知raw inverted body；其他形态继续fail |
| 同文件 | focused collision fixture | 只覆盖zero-width itr/body line | 增加negative-height body普通不命中/跨端点命中及左右朝向严格断言 |

## 4. 不可回退边界

- 不把body高度改正数，不删除DAT entry，不在production过滤；
- 不接受负宽、零高或未知负高模式；
- 不修改碰撞pass、候选顺序、broadphase、slot、RNG或命中结果；
- 未取得compile/self-check证据前不得升级状态。

## 5. 验收

- 5个正式negative-height body被精确分类，其他non-positive body=0；
- zero-width itr现有合同继续通过；
- production collector倒置body的普通不命中、跨端点命中及左右朝向fixture通过；
- fresh compile0、full self-check实际运行、validator PASS；
- 若后续self-check暴露独立失败，本Change只记录R-HC-01已关闭，不虚报全量PASS。

## 6. 当前执行边界

用户已明确批准`R8-WP01G-R08-R03 / R8-GEOMETRYCHECK-001`；脚本仍为0改动，下一步按合同只写
negative-height body精确分类与production collector raw strict-overlap夹具。

## 6.1 实际改动

仅修改`Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs`：

1. 增加`negativeHeightPositiveWidthBodyCount`、`unexpectedNegativeHeightBodyCount`与
   `otherNonPositiveBodyCount`；
2. 要求部署数据恰有5个`kind0/x39/y-555/w21/h-999` raw inverted body，其他负高/负宽/零高继续失败；
3. 增加production collector raw strict-overlap四矩阵：ordinary/right=0、ordinary/left=0、
   enclosing/right=1、enclosing/left=1；
4. helper只新增可选target left-facing设置，不改变任何production方法。

当前脚本状态`CODE_WRITTEN`；compile/full self-check/回归待执行。

## 6.2 验证证据

- fresh compile：`Assembly-CSharp.dll`时间`2026-08-24T02:13:18Z`，晚于source；Console error=0；
- full self-check日志：`definitions=137 / deployable=137 / resolvedFrames=82200 / itrs=4389 / bodies=13847 /
  invalidItrs=90 / zeroWidthPositiveHeightItrs=90 / otherNonPositiveItrs=0 / invalidBodies=5 /
  negativeHeightPositiveWidthBodies=5 / unexpectedNegativeHeightBodies=0 / otherNonPositiveBodies=0`；
- 新增raw overlap四矩阵全部通过；R-HC-01没有再抛异常；
- full self-check继续到`CheckMovementDatLoadingContracts`后因旧AnimationConfig Naruto路径缺失而失败，证明本Change
  已完成但全量检查暴露新的独立test fixture blocker；已拆`R8-WP01G-R08-R04 / R8-DATFIXTUREPATH-001`，批准前不修；
- production collision、DAT、parser与其他脚本0改动。

## 7. 回滚与Git

- 若后续实现失败，只回滚本Change的self-check diff并保留失败证据；回滚需用户批准；
- 回滚范围仅本Change的self-check分类/夹具；需用户批准；提交hash：未提交。
