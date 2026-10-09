# 优化点 A：消除 FindEffect 查表的临时内存分配

记录标识：`H06-FIND-EFFECT`。父项：`H-06` 碰撞候选收集。记录日期：2026-10-09。

**结论：这个单点问题已修复并完成所列验证。** `FindEffect(int effect)` 从只读集合上的 `foreach` 改为按原顺序的 `for` 索引遍历，保持原查找结果。在相同100混合对象用例中，collector三次采样的分配事件从 `22 / 22 / 22` 降到 `0 / 0 / 0`；原千人混合用例修复后的八次采样也全部为0。

这项工作消除了已定位的查表临时分配。它没有证明整tick达到33ms，也没有完成Role-aware默认推广、H-06父项关闭、H-07性能验收或Android认证。本文件整理已有结果，本轮没有重新运行测试或测量。

## 1. 原来的问题是什么

碰撞候选收集过程中会调用 `LoganKindCatalog.FindEffect` 查找效果配置。它接收一个 `effect`，从头往后找，找到第一个匹配记录就返回；找不到返回 `null`。

本次集合声明和初始化是：

```csharp
public ReadOnlyCollection<LoganKindRecord> Records { get; }

Records = new List<LoganKindRecord>(records).AsReadOnly();
```

这里第二行发生在目录构造时。本次修复没有消除构造期创建List和只读包装的成本。

已观察和已测量的定位过程：

- 原千人混合collector资格用例在修复前的八次采样中，每次有234个分配事件。最初只知道这项资格失败，尚不能把所有事件认作同一来源。
- 为控制诊断数据量，使用100混合对象短用例定位。它是76 ordinary / 24 special、两个阵营各50个对象，不是100个完全相同的角色。
- 排除测试断言污染后，三次collect各有22个分配事件。另一次详细调用栈采集共66个事件，每个40B，首个managed frame均指向改前的 `FindEffect`（原调用栈标记71行）。详见[100对象准备报告](../../../../artifacts/diagnostics/NTSD-H06-PROFILER-RESET-100-PREP-20261009/REPORT.md)。
- 随后只替换本方法的循环，再验证相同100对象结果和分配，最后回到原千人混合用例，观察是否仍有剩余分配。

这是频繁查表产生的临时托管分配问题。GC.Alloc事件表示发生了分配，不等于当时发生了一次GC回收或同等时长的卡顿。

## 2. 原来的代码

来源：修复任务保存的[真实改前dirty备份](../../FILE-OPERATIONS/NTSD-H06-FIND-EFFECT-ZEROGC-20261009/before/Assets__NTSD__Scripts__DatParser__Runtime__Models__LoganKindCatalog.cs)，不是从HEAD重建。当前方法定位在[LoganKindCatalog.cs](../../../../Assets/NTSD/Scripts/DatParser/Runtime/Models/LoganKindCatalog.cs)的 `FindEffect(int effect)`，本次整理时起始行为70。

```csharp
public LoganKindRecord FindEffect(int effect)
{
    foreach (LoganKindRecord record in Records)
        if (record.Effect == effect)
            return record;
    return null;
}
```

代码会按顺序取记录，比较 `record.Effect`，返回第一个匹配记录。问题定位到这条只读集合遍历路径，不能由此推出“每个foreach都会分配”。

## 3. 修改后的代码

```csharp
public LoganKindRecord FindEffect(int effect)
{
    for (int index = 0; index < Records.Count; index++)
    {
        LoganKindRecord record = Records[index];
        if (record.Effect == effect)
            return record;
    }
    return null;
}
```

改动只有遍历方式：通过 `Records.Count` 和 `Records[index]` 读取已经存在的记录。没有创建结果列表、没有复制所有记录、没有新增缓存，也没有改变查找顺序或首匹配规则。

`index`只是局部计数，`record`接收现有对象引用。本段代码没有为了每次遍历再建立一个枚举器对象的要求。循环体仍然可以写出会分配的操作，所以这不表示任何 `for` 都天然0GC。

## 4. 为什么能解决这次问题

大白话说，原来是“通过集合提供的遍历工具，挨个取记录”；现在是“直接按已有下标，挨个取记录”。记录本身没有增加，也没有重新复制。

本次应分开看两层证据：

| 层次 | 可以得出的结论 |
|---|---|
| 已测事实 | 100对象的分配栈落在改前FindEffect；唯一生产循环替换后，同100对象和原千人混合collector的声明范围均不再观察到分配事件。 |
| 机制推断 | 结合ReadOnlyCollection底层List及其枚举器接口路径，可能涉及枚举器装箱等临时对象。没有直接捕获40B对象的具体类型，不能把具体对象类型写成已证实。 |
| 不能推广的结论 | 不能说所有foreach都会分配、for循环体永远不分配，或已经修复95完整Driver资格中的GC问题。 |

因此不建议全仓机械替换foreach。以后遇到相似问题，先核对集合的实际类型、调用频率和分配证据，再做最小改动。

改前改后仍是顺序线性查找，最坏复杂度仍为 `O(n)`。这次没有改变算法复杂度，没有测得可报告的CPU毫秒或P95收益。

## 5. 实际解决了什么

| 验证范围 | 修复前 | 修复后 | 证据边界 |
|---|---|---|---|
| 同一100混合对象collector，4次预热＋3次采样 | 22 / 22 / 22个GC.Alloc事件 | 0 / 0 / 0 | 修复任务前后各实际采集；正分配和空scope可靠性校准有效。测量范围只含collect。 |
| 同一100对象的结果 | 已保存三份完整结果 | 三份逐项一致 | 有序候选完整载荷、handle、固定空输入、extended checksum、双RNG完整状态和计数一致。不是完整Driver逐tick trace。 |
| 原千人混合collector，4次预热＋8次采样 | 复用同fixture历史结果：每次234个事件 | 本次修复任务新鲜结果：八次全部0 | 千人测量范围含snapshot / collect / end；没有重新运行旧版，也没有在千人场景新抓调用栈。 |
| 原千人容量和清理 | 原门保持 | 通过 | capacities `[1000, 2000, 3000]`、数组身份和容量不变；reject 0→0、fallback 0、无abort；结束后objects / slots均0。 |

100用例每次97个pair检查、1个最终candidate；千人用例每次975个pair检查。两者均实际使用Role路径。这里的pair检查数和最终candidate数不同，不能混用，也不能拿CPU命令数当GPU batch。

千人用例保持760 ordinary / 240 special、两个阵营各500个对象及原布局。100与1000的测量范围不同，分配数字不能简单按人数倍数外推。修复后原千人范围没有观察到剩余分配，支持这次修复解决了该用例中先前的已知GC阻塞；这不等于独立捕获并证明千人234个对象全部具有同一对象类型。

## 6. 查找行为与验证结果

原有行为全部保留：按记录顺序查找；重复effect返回第一个匹配记录的同一对象；找不到返回null；空集合、负数、int边界和SourceAvailable=false保持原语义；null元素原有的异常或提前返回行为不改变。

修复前后各通过48组查找语义组合，并在相同100对象上核对结果。修复没有新增runtime状态、owner、容量或生命周期责任，因此不需要引入新的关闭阶段。

实际验证来自原修复任务，详见[修复结果报告](../../../../artifacts/diagnostics/NTSD-H06-FIND-EFFECT-ZEROGC-20261009/REPORT.md)：

| 原Editor测试 | 终态 | 保存结果 |
|---|---|---|
| 修复前语义检查＋100对象基线 | 2 / 2 PASS | [before-tests.xml](../../../../artifacts/diagnostics/NTSD-H06-FIND-EFFECT-ZEROGC-20261009/before-tests.xml) |
| 修复后语义检查＋相同100对象 | 2 / 2 PASS | [after-tests.xml](../../../../artifacts/diagnostics/NTSD-H06-FIND-EFFECT-ZEROGC-20261009/after-tests.xml) |
| 原千人OPointBurst-shaped collector资格 | 1 / 1 PASS | [thousand-tests.xml](../../../../artifacts/diagnostics/NTSD-H06-FIND-EFFECT-ZEROGC-20261009/thousand-tests.xml) |

这不是五个独立优化点。测试是本单点修复的验证；“OPointBurst-shaped”是混合对象测试形状名称，不等于真实Play中自然生成OPoint的全域证明。

## 7. 本问题完成条件与后续安排

本单点的完成条件已满足：定位到具名方法；保持查找语义；相同100对象结果一致且声明collector范围0GC；回到原千人混合用例后八次0GC、容量稳定、清理无残留。可以结束这次FindEffect问题处理，复用这些证据，不因编写说明再次运行同样的用例。

尚未完成的工作分开保留：

- **H-06 Role-aware限定准入与默认接线**：还要核对必要门、同配置收益和实际默认接线。目前普通生产默认仍为BruteForce。D1=A允许必要正确性、容量、0GC、有序关闭和收益门通过后先限定推广，不因整tick尚未达到33ms单独阻止；没有自动推广权限。
- **下一优化点H07-INPUT**：按现有进度建议，先使用91已有归因，找CharacterInput内有证据的大块重复采样/行发布/状态同步，明确读写时点、失效条件和最小验证。Role默认推广不是其分析的硬前置；未发现值得改的成本块时应给出有据的不实施结论。
- **H-07整tick与Android最终认证**：仍未通过。本次没有新做完整Driver0GC、真实Battle Play、同配置CPU收益窗、120＋180冒烟、120＋1800正式性能门或Android设备验证。
- **95资格GC**：仍属于其原有未知调用点，不能用本次collector修复冒称95已0GC或候选已准入。95仍默认OFF，旧自动执行目标未恢复。

下一项目前只是建议和方案分析范围。本记录不向执行会话下发新任务，也不启动后继测试或代码修改。

## 8. 方案、代码与审计证据

- [H-06单项方案](../../../../Assets/NTSD/Docs/android-mobile-readiness/h-06-collision-broadphase-1000.md)；[唯一进度](../../../../Assets/NTSD/Docs/battle-optimization-progress-tracker.md)；[H07-INPUT下一项方案](../H07-INPUT.md)；[跨项依赖](../DEPENDENCIES.md)。
- [修复Task](../../TASKS/NTSD-H06-FIND-EFFECT-ZEROGC-20261009.md)；[Change Record：NTSD-H06-FIND-EFFECT-ZEROGC-001](../../CHANGE-RECORDS/NTSD-H06-FIND-EFFECT-ZEROGC-001.md)；[修复文件操作审计](../../FILE-OPERATIONS/NTSD-H06-FIND-EFFECT-ZEROGC-20261009/RECORD.md)。
- [100对象修复前完整结果](../../../../artifacts/diagnostics/NTSD-H06-FIND-EFFECT-ZEROGC-20261009/before-100.json)；[100对象修复后完整结果](../../../../artifacts/diagnostics/NTSD-H06-FIND-EFFECT-ZEROGC-20261009/after-100.json)；[原千人修复后结果](../../../../artifacts/diagnostics/NTSD-H06-ROLE-SCOPED-ADMISSION-20261009/OPointBurst-shaped-collector-gc-7f5a826ab9dc4874822a67910dbc7bf6.json)；[原修复指纹](../../../../artifacts/diagnostics/NTSD-H06-FIND-EFFECT-ZEROGC-20261009/fingerprints.json)。
- [本次纯文档整理的备份与操作记录](../../FILE-OPERATIONS/NTSD-OPTIMIZATION-SOLUTION-RECORDS-20261009/RECORD.md)；[以后同类记录的格式](README.md)。

本次整理时核对的生产文件SHA-256：`CC7409790933E446CB0F45BDA7681AF70CE674B27FBED7B6FB133B54FEF65D47`。真实改前备份SHA-256：`5FF6FB202117F8F37C5986C5E890EA8A472BBE0EC6EDA176D5B48A9C65429392`。后续代码变化后以新的具名记录和验证为准，不把本文当前片段当作永久现状。

## 9. 后续补充

2026-10-09：基于已完成修复及已有报告建立本说明；只新增文档与导航，不重跑测试、不修改生产代码，不改变现有验收标准或默认配置。后续若调整本问题，在此追加原因、代码变化、验证及更正结论。
