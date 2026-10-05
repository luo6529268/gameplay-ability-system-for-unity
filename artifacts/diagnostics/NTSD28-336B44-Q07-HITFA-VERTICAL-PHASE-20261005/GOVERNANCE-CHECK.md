# 本包最终治理与差异检查

实际命令：`pwsh -NoProfile -File Tools/Validate-ChangeLedger.ps1 -RepositoryRoot I:\GitHub\Unity_GAS\gameplay-ability-system-for-unity`，exit0。本次工具返回前四行：

```text
Change ledger validation PASSED.
  Records: 1275
  Governed code files in diff: 25
  COVERED: Assets/NTSD/Scripts/Animation/BattleEntityOverlayRenderer.cs -> NTSD28-336B44-Q07-D024-VERTICAL-PROJECTION-001, NTSD28-Q09-BPOINT-BLEED-LEGACY-001
```

`git diff --check`实际exit0。当前222份NTSD28-336B44同名前缀Record中61份未关闭，调度REUSE47/TRIGGER14/P0=DEP=ONE=0。1275份是治理记录数、25是当前dirty governed code文件数，不是本轮新任务数或执行测试数。

行为证据为原Editor新五参数＋局部SelfCheck6/6和完整Driver1/1；本检查不证明自然原Scene、正式根EXE同初态/GPU或父Q完成。[实际结果与限制](REPORT.md)。
