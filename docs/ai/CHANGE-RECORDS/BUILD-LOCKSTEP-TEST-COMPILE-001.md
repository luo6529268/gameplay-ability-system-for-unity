# BUILD-LOCKSTEP-TEST-COMPILE-001 — restore the missing local test controller

<!-- CHANGE-RECORD
id: BUILD-LOCKSTEP-TEST-COMPILE-001
status: CODE_WRITTEN
code-path: Assets/NTSD/Scripts/Test/Editor/InProcessLockstepAuthoritySessionEditorTests.cs
authority: UNITY-COMPILE-RECOVERY-20260824 / USER-REQUESTED-UNITY-VALIDATION
evidence: UNITY-EDITOR-CS0246-20260824-201328
-->

> 创建日期：2026-08-24  
> 当前状态：`CODE_WRITTEN`  
> 类型：test-only compile recovery

## Goal

让现有 `InProcessLockstepAuthoritySessionEditorTests` 再次自包含地编译，以解除本次纯表现验证的外部 Unity 编译阻塞。

## Observed evidence

- Unity assets refresh 已让缺失 `.meta` 的 `InProcessLockstepChecksumWitness.cs` 进入 `Assembly-CSharp`，此前的三个
  runtime `CS0246` 已消失；
- 最新唯一 C# error 是 `Assets/NTSD/Scripts/Test/Editor/InProcessLockstepAuthoritySessionEditorTests.cs(268,40)`：
  `EmptyController` 不存在；
- 该文件在 `CreateCharacter` 中仅将它用作无输入 `ILF2Controller`；同仓库的 `AiMoveModeSnapshotEditorTests` 等其他
  test class 各自有同等私有 helper，证明这是漏写的 test-local fixture，而不是 battle runtime 类型。

## Scope

- 仅在 `InProcessLockstepAuthoritySessionEditorTests` 的末尾补一个 private `EmptyController : ILF2Controller`；
- 提供全 false 的方向/攻击状态、空的 `SimInputBuffer`、零移动输入及 no-op `SetInputID`；
- 不修改 production lockstep、battle runtime、输入、simulation、scene、DAT、资源或 C++。

## Acceptance

1. Unity 编译不再报该 `EmptyController` CS0246；
2. `InProcessLockstepAuthoritySessionEditorTests` 编译并可被 Test Runner 发现；
3. 本次 helper 无任何 production code path；
4. 随后重新运行背景 focused tests；
5. Change Ledger validator 覆盖该改动。

## Risks / rollback

唯一风险是 helper 接口实现不完整导致新的 test assembly compile error。回滚只删除该 test-local private class；不使用 Git restore、
不删除任何用户文件，且需要用户另行授权才执行回滚。

## Verification

已在 `Frame(...)` 后、test class 结束前补齐 private `EmptyController`。它完全复用已存在于其他 editor tests 的
无输入 `ILF2Controller` contract：零按钮、零方向、私有 `SimInputBuffer`、no-op input id。未修改任何 production 路径。

Unity compile、test discovery、focused background tests 和 validator 均待执行。
