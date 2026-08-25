# R7-TEST-001 — AI shared-shadow static frame sentinel isolation

<!-- CHANGE-RECORD
id: R7-TEST-001
status: VERIFIED
code-path: Assets/NTSD/Scripts/Test/Editor/AiDecisionSoAShadowEditorTests.cs
authority: test isolation requirement D-TEST-001; C++ gameplay authority not modified
evidence: BINARY-ISOLATED OWNER + EXACT/CLASS/AI MATRIX + SAME-DOMAIN/FRESH SELF-CHECK + COMPILE PASS
-->

> 创建日期：2026-08-23  
> 最后更新：2026-08-23  
> 类型：test / isolation

## 1. 状态与范围

- 当前状态：`VERIFIED / TEST-ONLY`
- Work Package：R7 repair order 9 / D-TEST-001
- 只覆盖一个Editor test的静态sentinel清理；production无授权。

## 2. 已观察事实

- shadow 9/9 → same-domain self-check PASS（02:31:27）；
- stress 254/254 → same-domain self-check PASS（02:33:13）；
- AI 286/286 → same-domain R3-INP-01 FAIL（02:35:58）；
- decision 163/163 → same-domain FAIL（02:37:55）；
- module/full/kernel 37/37 → PASS（02:39:30）；authority/positions 60/60 → PASS（02:41:15）；
- `AiDecisionSoAShadowEditorTests` 66/66 alone → FAIL（02:42:43）；
- binary 36-case/23-case partitions与5/3 case partitions定位到单一owner；
- owner exact 1/1 → same-domain FAIL（02:52:32）。

## 3. Root cause

owner用例arm `SetAiDecisionSharedPostLegacyStateMutationForSelfCheck(0,14)`；hook在AI legacy action后直接写
`entity.Frame.D.state=14`。此时low entity因missing target frame绑定到`LF2FrameCache.EmptyFrame`静态sentinel，
测试结束未恢复，后续missing-frame读者看到state14。该污染属于test fixture生命周期，不是C++/Unity gameplay差异。

首次owner cleanup后，完整class job暴露
`UnifiedAuthority_AscendingRefreshMakesLowVisibleToHighWithoutReverseEarlyVisibility` expected14/actual0；
fresh-domain exact复现。该fixture过去依赖owner留下的sentinel=14。其自身shared-shadow mutation不构成
data-oriented canonical-store mutation witness，因此post-input unified row仍为0。必须改用已有
`SetCharacterInputPassMutationOverrideForSelfCheck`，该入口会触发同pass full runtime snapshot refresh。

## 4. 计划改动

| 文件 | 方法 | 改前 | 目标 |
|---|---|---|---|
| `Assets/NTSD/Scripts/Test/Editor/AiDecisionSoAShadowEditorTests.cs` | `SharedShadow_BuildsOnceAndRefreshesLowSlotBeforeHighSlotEvaluation` | state14 mutation后不清理shared sentinel | 捕获sentinel与原state，在finally恢复；现有行为断言不变 |
| 同上 | `UnifiedAuthority_AscendingRefreshMakesLowVisibleToHighWithoutReverseEarlyVisibility` | 隐式依赖前测留下sentinel14 | 使用character-input mutation override形成自身canonical refresh witness，并finally恢复sentinel |

## 5. 保护边界

- 不改production AI、frame cache、scheduler、input、OID maintenance或render；
- 不改CentralOnly、30Hz、FrameInputSet、ECS/SoA、slot/generation、capacity、pool或0GC合同；
- 不掩盖其他static owner；修复后必须在不reload域的情况下复跑owner/class/AI matrix→self-check。

## 6. 实际改动

第一步已在owner测试中通过reflection取得private static `LF2FrameCache.EmptyFrame`，记录原state；
test hook执行后显式断言low当前帧就是该shared sentinel且state为14，原有shared/indexed row断言保持；
`finally`无条件恢复原state，并在正常路径尾部断言恢复完成。production无diff。

第二步已提取test-local sentinel helper，并将dependent unified-refresh fixture改为
`SetCharacterInputPassMutationOverrideForSelfCheck`的slot0 state14 mutation；该既有hook会令post-input
runtime snapshot执行full refresh。dependent fixture同样在finally清除override并恢复sentinel原state。

## 7. 验收

| 层级 | 结果 | 状态 |
|---|---|---|
| compile | dotnet 0 error；Unity force scripts compile Console 0 error | `PASS` |
| dependent exact | `b8e926eb862a4c4a83ed3124180f3267` 1/1 | `PASS` |
| full owner class | `4a05c94370434bddbd1e2afc38425c9e` 66/66 | `PASS` |
| AI matrix | `c90b67cf6eb740dfb2ed2715f56dbaf4` 286/286 | `PASS` |
| same-domain self-check | class后03:03:54；AI matrix后03:06:15 | `PASS` |
| fresh-domain final | Console 0 error；03:07:32 self-check | `PASS` |

## 8. 回滚

若恢复逻辑不关闭污染，回滚该test-only diff并保留本Record为BLOCKED；不得改production迎合测试。

## 9. Git / audit

- 修改前工作树包含既有用户/项目diff；目标文件此前无本轮改动；
- validator：`Change ledger validation PASSED`（54 Records / 51 governed code files）；scoped whitespace PASS；
- 未提交。

## 10. Failed evidence retained

- pre-fix owner job `8b29655757224b2d963216e77d832884` 1/1 PASS，但02:52:32 same-domain
  self-check FAIL，证明测试自身成功不代表清理成功；
- first owner-cleanup class job `67af562e0507491280212ad059c2b4c7` 的dependent expected14/actual0，
  以及fresh exact `5bd128b4578c4249bbe789d515517b12` FAIL，证明dependent曾依赖污染；
- 这些失败不是production gameplay first difference，均由最终独立fixture与same-domain验收关闭。
