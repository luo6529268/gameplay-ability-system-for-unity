# R8-LATEPLAY-001 — live random weapon / late chain / effect S4 probe

<!-- CHANGE-RECORD
id: R8-LATEPLAY-001
status: VERIFIED
code-path: Assets/NTSD/Scripts/Test/Editor/BattleRandomWeaponLateEffectPlayModeProbeEditor.cs
authority: J:\QQFile\NTSD2.4\ntsd_release\src\entity\game_tick.cpp:352-428,1657-1824,2080-2110;Makefile
evidence: R7-LATE-001 and corrected R8-SPRITEMAP-001 source/compile/self-check evidence; GT-03/GT-11; live joint Play pending
-->

> 创建日期：2026-08-23  
> 状态：`VERIFIED`  
> 所属：`R8-WP01C-06`

## 1. 修改前状态

- natural random、late chain、9996 writer与exhaustion source合同已只读闭合；
- R7-LATE旧state8000 HitStun结论已被R8纠正，当前production写RenderPicOffset140；
- GT-03/GT-11静态/self-check已有，但缺current Play process的live catalog/pool/slot证据；
- 未发现新production静态首差。

## 2. 允许改动

- 仅新增`BattleRandomWeaponLateEffectPlayModeProbeEditor.cs`及meta；
- 更新本包治理/证据文档；
- 禁止修改production gameplay、allocator/pool、RNG、DAT/scene、render、C++。

## 3. 保护与副作用

- live段只在paused/worker-idle执行，保存并恢复RNG/sounds/object/slot/pool/pause；
- live生成对象全部纳入pooled cleanup；synthetic chain/exhaustion使用独立logic-only world；
- candidate与expected fields由当前sealed catalog和同一LCG机械预测，不硬编码角色/技能；
- first-difference只能令Record BLOCKED，不能顺手改production。

## 4. 验收与回滚

- Task required matrix、compile/focused/Play/self-check/validator/diff全部PASS；
- 回滚仅删除新增probe/meta并标记`ROLLED_BACK`，不触碰其他dirty worktree。

## 5. 实际改动（待编译）

- 新增Editor-only probe/meta；live natural random按current sealed catalog预测candidate/RNG/slot/position；
- live state9996用current OID217/218验证5 child、34 RNG与reset字段；
- logic-only synthetic fixture验证完整9995→4000→8000→9996同调用chain；
- authority400 synthetic fixture填满50..399，验证natural 1 RNG和late 0 RNG/0 child；
- cleanup恢复live RNG/sounds/entity/slot/pool/pause；production0改动。

当前`CODE_WRITTEN`，尚未compile/focused/Play/self-check。

首次fresh compile捕获4条probe-only API错误：Facing/Generation错误地读自Runtime，且缺LF2Tasks using。
已按现有GT-11合同改用`Runtime.Dir`、`TryGetCurrentRuntimeHandleForDiagnostics`与`IsFacingLeft`，并补
`NTSD.Animation.LF2Tasks`；production0改动，需重编译。

首次Play在tick0、未进入任何matrix前因driver/runtime catalog尚未ready而probe-only FAIL。已把依赖解析
移入Editor update等待，只有driver、pool、sealed catalog全部可用后才采基线并暂停；production0改动。

第二次Play进入natural random并在首个OID/position断言失败，cleanup恢复；原报告不足以裁决。复核发现
probe预测漏写C++ `100<=oid<200` candidate范围，现补上authority filter并扩展actual OID/slot/position/RNG
错误信息。若复跑仍不同即为production first-difference；尚未修改production。

## 6. Final verification

| 层级 | 结果 | 状态 |
|---|---|---|
| compile | final Editor DLL 14:06:36；C# error0 | `PASS` |
| focused | job `faa6cee5653347b69fd31410832e1fcb` 14/14 | `PASS` |
| clean Play | natural/live9996/synthetic chain/exhaustion | `PASS` |
| worker | final Play active | `PASS` |
| cleanup | objects/claimed/pools 4/2/2/2，RNG/sounds/pause恢复 | `PASS` |
| Console | error0 | `PASS` |
| full self-check | 2026-08-23 14:08:15 | `PASS` |
| validator | 71 records / 70 governed code files | `PASS` |
| production changes | 无 | `NOT APPLICABLE` |

首次tick0与第二次prediction失败均是probe-only并完整留痕；最终authority-range预测和Unity production结果一致，
未形成gameplay first-difference。Record为`VERIFIED`，仅裁决WP01C-06 Unity S4；C++ full trace仍BLOCKED。

`B-R8-WP01C-06-TEARDOWN-01`：probe runtime结束前Console0且4/2/2/2恢复；退出Play后Unity列出两个
AutoCreated manager未清理。无probe对照Play→Stop为0 error，因此这是probe创建/回收renderer后的Editor
teardown hygiene，不是active runtime leak或gameplay first-difference。Record的VERIFIED只覆盖明确S4矩阵，
不声称post-stop teardown零告警。
