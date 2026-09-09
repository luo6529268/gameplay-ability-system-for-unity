# NTSD28-R2-CPOINT-SYNC-EXACT-RELATION-FIXTURE-CORRECTION-001

<!-- CHANGE-RECORD
id: NTSD28-R2-CPOINT-SYNC-EXACT-RELATION-FIXTURE-CORRECTION-001
status: VERIFIED
change-kind: TEST_FIXTURE_CORRECTION
code-path: Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs
authority: 用户2026-09-09 Goal 1.B test-only授权；直接采用GLM 5.B exact CatchSourceSlot90缺失结论。当前NTSD 2.8-Logan EXE B1E13AE1/closure 39DDDA15权威不变；参照现有LinkCpointEntities与BattleGrabCpointLinkPlayModeProbeEditor夹具。
evidence: Fresh SelfCheck RED at 2026-09-09T10:44:00Z stopped at R2-SCHED-001 T14 CPoint sync. After one exact CatchSourceSlot90 fixture assignment and Unity reload, fresh result at 10:46:01Z passed the unchanged R2 assertions and stopped later at GT-06 real character HP/PP recovery in CheckGameTickCurrentDatDispatchMatrix. Runtime build exit0/47 warnings/0 errors; Editor build exit0/104 warnings/0 errors. Whole-file incremental comparison confirms only the intended fixture line, and repository status/hash comparison shows only the five authorized files changed. VERIFIED applies solely to this test-fixture correction, not full SelfCheck, production parity or Goal 2. Final validator output is appended below.
-->

完整授权、精确路径、前置条件、不变量、停止条件、验收及回滚见
[同ID Task Contract](../TASKS/NTSD28-R2-CPOINT-SYNC-EXACT-RELATION-FIXTURE-CORRECTION-001.md)。

## 改前事实与预期职责

- 现有方法的compat关系与断言均保留；本包只补victim的exact source-slot初始化。
- `LinkCpointEntities()`与Play probe已采用该赋值；不复制完整grab算法到夹具。
- 预期从R2-SCHED-001 CPoint同步断言继续推进；full SelfCheck最终可能停在后续独立断言。
- SelfCheck是现有Editor入口，不进入Play、不启动第二个Unity、不修改production。
- 本Record只覆盖指定方法的本轮增量，不能把整个SelfCheck的既有迁移diff归给本包。

## 实施与验证日志（追加）

- 事前：目标instance `gameplay-ability-system-for-unity@b1b02287`实测可连接；
  project root匹配、Unity `2022.3.62f3`、active scene `NTSD_Battle`、dirty=false/rootCount=13。
- 待执行：修改前RED、最小test-only diff、修改后SelfCheck最终停点、两套build与validator。

### 新鲜RED（脚本修改前）

通过目标instance的既有`execute_menu_item`调用`NTSD/验证/运行战斗运行时自检`。
`Temp/NTSD_BattleRuntimeSelfCheck.result`实测于`2026-09-09T10:44:00Z`更新为FAIL：

```text
System.InvalidOperationException: R2-SCHED-001: T14 CPoint sync must still execute after candidate consumption
BattleRuntimeSelfCheck.CheckReleaseTickCpointSyncFollowsCandidates():13303
```

此前candidate pre-T14断言已越过；与GLM判断一致。此时尚未修改测试脚本。

### 实际脚本改动

只在`CheckReleaseTickCpointSyncFollowsCandidates()`的`catcher.CaughtSlotIndex = 1;`
之后增加`victim.Runtime.CatchSourceSlot90 = catcher.Runtime.SlotIndex;`。
其他compat字段、FrameDelay、candidate probe、三条断言及生产代码保持原样。
状态推进到CODE_WRITTEN；编译和修改后SelfCheck尚未取证。

### 修改后SelfCheck：R2通过，最终停在后续GT-06

执行`refresh_unity(mode=if_dirty, scope=scripts, compile=request)`后重新按目标instance的
status文件建连接；实际返回project root正确、Unity `2022.3.62f3`、`NTSD_Battle`、
idle、非Play、非编译、非测试运行；Scene dirty=false/rootCount=13。
再次执行同一SelfCheck菜单，没有改断言或跳过检查。

菜单命令返回`Command processing timed out after 30000 ms`，没有重发菜单或启动Editor。
随后从磁盘只读取得该次运行的最终result：`2026-09-09T10:46:01Z`，1260 bytes。
同一Editor.log也记录如下最终失败与调用栈：

```text
FAIL
System.InvalidOperationException: GT-06 real character: current character DAT must recover HP/PP without treating negative WeaponCount as environment damage
BattleRuntimeSelfCheck.ExpectRecoveryFixture():25520
BattleRuntimeSelfCheck.CheckGameTickCurrentDatDispatchMatrix():25356
```

`RunAllChecksStatic()`先在第126行调用本包目标方法，再在第180行调用GT-06矩阵。
结果证明原样保留的三条R2断言均已越过；不是以菜单超时响应冒充成功。
全量SelfCheck仍为FAIL，GT-06根因与修复不在本包，不作甄别或修改。

### 两套Assembly build

按顺序执行：

```text
dotnet build Assembly-CSharp.csproj --no-restore -v:quiet
exit 0; 47 warnings; 0 errors; elapsed 00:00:08.54

dotnet build Assembly-CSharp-Editor.csproj --no-restore -v:quiet
exit 0; 104 warnings; 0 errors; elapsed 00:00:06.19
```

Unity已实际完成脚本重载并用新程序集执行上述SelfCheck；没有第二个写Library的Unity实例。
保留build warnings，不修改第三方、production或项目配置。

### 增量审查与状态

- 对比本轮修改前保存的整个SelfCheck文本：仅新增指定exact赋值一行，所有断言与其他方法不变。
- 对比本轮修改前工作树status/内容SHA-256：仅Ledger、SelfCheck与新增A Record、B Task、B Record
  五个授权文件发生增量。原Scene、GLM报告、continuation prompt、`.codex/config.toml`及其余
  既有modified/deleted/untracked均未被本包改动；未将旧SelfCheck整文件diff归入本包。
- 去除本轮新Ledger区块与B索引行后，其全部历史文本与改前一致；B3原行保持原样。
- `VERIFIED / TEST_FIXTURE_ONLY / FRESH_RED / R2_ASSERTIONS_PASS / BUILDS_0_ERROR /
  FULL_SELFCHECK_BLOCKED_GT06`仅关闭本包的夹具修正；不证明生产规则、全SelfCheck或阶段完成。
- held-refill Play、stress甄别、B6剩余族及所有production修改继续USER_HOLD；本包收尾后停止。

### Validator过程记录

证据收尾前、Record仍为CODE_WRITTEN时执行`& ./Tools/Validate-ChangeLedger.ps1`，exit 1，
唯一ERROR为本包活跃ID未出现在STATE.md，531条旧路径WARNING。原因已在事前Task中明确：
用户本轮不授权修改STATE。没有修改STATE或validator，也没有使用跳过校验参数。
随后根据已实际取得的R2 RED/通过证据、两套build与增量审查关闭本test-only包；
完整SelfCheck的GT-06 FAIL事实继续保留。最终validator输出另行追加。

最终以相同命令、无跳过参数执行，exit `0`：

```text
Change ledger validation PASSED.
  Records: 431
  Governed code files in diff: 373
Warnings: 531
```

WARNING均为既有Record声明路径不在当前code diff的提示；未清理历史记录。
Goal 1限定工作完成，到此停止等待用户复核，未启动Goal 2。
