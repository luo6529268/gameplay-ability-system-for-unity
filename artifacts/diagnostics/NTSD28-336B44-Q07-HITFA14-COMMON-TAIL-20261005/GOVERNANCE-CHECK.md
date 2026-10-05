# 本包最终留痕与差异检查

实际命令：`pwsh -NoProfile -File Tools/Validate-ChangeLedger.ps1 -RepositoryRoot I:\GitHub\Unity_GAS\gameplay-ability-system-for-unity`，退出0。

本次工具返回的前四行原始输出为：

```text
Change ledger validation PASSED.
  Records: 1274
  Governed code files in diff: 25
  COVERED: Assets/NTSD/Scripts/Animation/BattleEntityOverlayRenderer.cs -> NTSD28-336B44-Q07-D024-VERTICAL-PROJECTION-001, NTSD28-Q09-BPOINT-BLEED-LEGACY-001
```

`git diff --check`实际退出0；工具同时提示若干现有LF/CRLF换行转换，未作换行清理。当前221份NTSD28-336B44同名前缀Record中60份未关闭（排除VERIFIED/ROLLED_BACK/SUPERSEDED/ABANDONED）；当前包从FOCUSED_TEST_PASS到RUNTIME_PENDING不改变未关闭总数，调度为REUSE46/TRIGGER14/P0=DEP=ONE=0。

本检查只证明脚本diff有治理记录且差异检查通过，不是全部1274项运行测试或25个新任务，也不证明父Q及总目标完成。实际限定行为证据、失败原件和未覆盖条件见[报告](REPORT.md)。
