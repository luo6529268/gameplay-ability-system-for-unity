# NTSD28-B5-KIND4-DEAD-WEAPONCOUNT-SELECTION-RETIREMENT-001

<!-- CHANGE-RECORD
id: NTSD28-B5-KIND4-DEAD-WEAPONCOUNT-SELECTION-RETIREMENT-001
status: VERIFIED
change-kind: DEAD_AUTHORITY_BRANCH_RETIREMENT
code-path: Assets/NTSD/Scripts/Animation/Character/BruteForceSceneQuery.cs
code-path: Assets/NTSD/Scripts/Test/Editor/NTSD28B5Kind4AtomicProductionIntegrationEditorTests.cs
authority: NTSD 2.8-Logan candidate selection uses vrest/kind1/2/7/state1004 for multiple-vs-nearest and environment_state_320 for kind4 conversion/count; EXE B1E13AE1, closure 39DDDA15.
evidence: exit scan found one redundant kind4 WeaponCount selectFlag assignment immediately subsumed by the generic non-1/2/7 branch; valid red de9938f15af643e8bb17fed5c775147e 30 pass/1 expected fail; focused 57308b0416124834bbe9cb003123af97 31/31; B5 eda8e932ba4e4f3bbc123d79dfab42cf 505/505; Unity-side NTSD28 automatic regression 7fc9a38e1d364559b01ff9e3cfb5d27b 1063/1063; SelfCheck PASS 2026-09-06T08:08:03Z; Console 7 intentional errors; Scene unchanged.
-->

> 状态：`VERIFIED / DEAD_WEAPONCOUNT_KIND4_SELECTION_RETIRED`

## 结果

- 删除 `AcceptReleaseSelectFlagCandidate` 中行为中性的 `kind == 4 && attacker.WeaponCount != 0` 赋值；其结果原本已被紧随其后的non-1/2/7通用分支完全覆盖。
- source guard确保该选择方法不再出现 `WeaponCount`，防止被误读为当前kind4 authority。
- valid red：job `de9938f15af643e8bb17fed5c775147e`，31项中仅source guard按预期失败；focused green `57308b0416124834bbe9cb003123af97` 31/31。
- B5 `eda8e932ba4e4f3bbc123d79dfab42cf` 505/505；Unity侧 `NTSD28` 自动回归 `7fc9a38e1d364559b01ff9e3cfb5d27b` 1063/1063。
- SelfCheck 2026-09-06T08:08:03Z PASS；Console仅7条预期负路径；Scene基线不变。

回滚只恢复该冗余分支和移除源码守卫；不得恢复其他kind4 `WeaponCount` gates。
