# R8-DATFIXTUREPATH-001 — production DAT self-check catalog path

<!-- CHANGE-RECORD
id: R8-DATFIXTUREPATH-001
status: VERIFIED
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
authority: Assets/NTSD/Config/data.txt / R8-CHARASSET-001 / R8-WP01G-R08-R04
evidence: SELF-CHECK-20260824-021428-OLD-ANIMATIONCONFIG-PATH
-->

> 创建日期：2026-08-24  
> 状态：`VERIFIED / FULL SELF-CHECK PASS`  
> 类型：TEST-ONLY / resource fixture / self-check unblock

## Current difference

production资源已通过获批的R8-CHARASSET迁移由`data.txt`指向`Config/Character`，但self-check仍显式传入旧
`AnimationConfig/Mingren|Kakashi|XiaoYing|ZuoZhu`与`FrameConfig/naruto_clone.dat`。最新full self-check越过
R-HC-01后首先在Naruto旧路径失败。

## Planned change

- 只在self-check helper中按objectId读取`GameDataManager.Instance.GetObjectById(objectId).file`；
- 继续使用现有decrypt/parser/private conversion路径与原字段断言；
- 替换所有production DAT callsites为catalog overload；
- production resource/loader/gameplay零改动。

## Acceptance and boundaries

- OID1/2/3/11/33/120/204/205/214当前catalog路径存在并成功解析；
- old AnimationConfig/FrameConfig literal references清零；
- compile/full self-check/resource focused/validator通过到本包范围；
- 不恢复旧资源目录、不写DAT/data.txt、不改production。

## Status / rollback

回滚只涉及本Change的test helper/callsite diff，需用户批准；提交hash未有。

## Actual change

- `BattleRuntimeSelfCheck.cs`新增objectId-only overload，查当前`ObjectDefinition.file`并复用production path resolver；
- 11个production DAT callsite改用该overload；旧AnimationConfig/FrameConfig literals清零；
- 原path overload、decrypt/parser/converter和所有field assertions保留；
- production/scripts/resources其他路径0改动。当前状态`CODE_WRITTEN`，验证待执行。

## Verification evidence

- fresh compile0；`Assembly-CSharp.dll 2026-08-24T02:25:59Z`晚于source；
- job `75849d918dec46d88b01f1253cecec63`：CharacterAssetDeployment 1/1 PASS；
- full self-check `2026-08-24T02:27:38Z`：PASS；旧路径问题关闭且所有原DAT字段合同继续通过；
- 负向registration/rest binding fixture产生的预期error日志在PASS后清空，最终Console error=0；
- production与资源0改动；本Change可标`VERIFIED`。
