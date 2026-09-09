# Task Contract — NTSD28-B0-RAW-ENTITY-DIFFERENCE-001

> 状态：`FOCUSED_TEST_PASS / BUILD-0-WARN-0-ERROR / RAW-SELF-TEST-5-OF-5 / REAL-REPORT-29-EQUAL-18-DIFFERENT`  
> 所属计划：`NTSD28-UNITY-BATTLE-REALIGNMENT-001 / B0`  
> 建立日期：2026-09-02

## 目标

在独立 `Tools/NTSD28Parity` 中新增 authority source raw 与 Unity raw 的严格验证、逐字段比较和
unique-difference 汇总命令。工具必须稳定复现公共 OID2/7 三 tick 当前观察到的 29 equal / 18
different unique fields，并区分 `UNITY_BINDING_MISSING` 与真实非空 `VALUE_DIFFERENCE`。

## 允许文件

- `Tools/NTSD28Parity/RawEntityCaptureComparator.cs`
- `Tools/NTSD28Parity/Program.cs`
- `Tools/NTSD28Parity/README.md`
- 本 Task、同 ID Change、Ledger、STATE、handoff、总表

## 不变量

- authority raw 继续由现有 validator 严格验证；Unity raw 只允许声明为 MISSING 的字段为 null。
- 不归一化、不忽略、不自动修正 `allocationEpoch` 或任何 approved exception。
- 不修改 Unity runtime/projection、authority source、DAT、Scene 或资源。
- 输出 diagnostic only、certificate false；equal raw 也不能成为 parity certificate。

## 验收

- .NET Release build 0 warning / 0 error。
- 独立 raw self-test 覆盖 equal、14 missing、非法 candidate null、tick 不连续和字段值差异。
- 对真实公共双端 raw 输出 3 ticks / 6 entity pairs / 47 fields / 105 occurrence differences /
  18 unique fields / 29 unique equal fields；首差异稳定。
- 现有总 self-test、Ledger 和 diff check 通过。

## 当前证据

- 首次 build 在 synthetic default-value 的 switch-expression null-forgiving 位置报 CS1002/CS1525；
  改为显式非空局部后 Release build 0 warning / 0 error。
- raw self-test：5/5 PASS，覆盖 equal、14 missing、candidate null fail-closed、非连续 tick
  fail-closed、单字段 value difference。
- 真实报告：3 ticks / 6 pairs / 282 field occurrences / 105 difference occurrences /
  18 unique differences / 29 unique equal fields；first difference 为 slot1 allocationEpoch 2 vs 1。
- 现有总 self-test 21/21 PASS；`dotnet format --verify-no-changes` PASS。
- 全局 Ledger：70 records / 14 governed code files / PASS；diff check 无 error。
