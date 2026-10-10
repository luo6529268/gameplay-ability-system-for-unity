# 现有 Role-aware 的限定默认准入

状态：Role限定准入完成；H-06父项/H07/Android未通过。唯一进度见 battle-optimization-progress-tracker.md。

原问题：现有Role收益没有进入普通空配置生产入口。真实改前备份见FILE-OPERATIONS/NTSD-H06-ROLE-DEFAULT-ADMISSION-20261009/promotion-before.json。

原代码（合法override/config解析后）：
```csharp
return CollisionBroadphaseBackend.BruteForce;
```
修改后同一位置：
```csharp
if (string.IsNullOrEmpty(explicitOverride) && string.IsNullOrEmpty(configuredBackend))
    return CollisionBroadphaseBackend.LooseQuadtree;
return CollisionBroadphaseBackend.BruteForce;
```
为何：复用现有Role自适应Direct/Sweep/tree路径，无新网格/Burst/Jobs；只改变没有显式选择时的策略，合法值原优先级与非法安全回退保持。正常GameConfig配置为空，其AI已为DataOrientedCanonical，不改AI。

解决：同配置两布局collector55.39→15.66/59.90→17.49ms、tick85.96→46.55/91.31→49.84ms；四窗完整Driver稳态0GC、容量/关闭通过。同布局完整末态一致，历史四场景逐tick按不受影响域复用。9种解析/生产source组合及实际Role/Brute/Role关闭重建通过。没有新增runtime缓存/state/owner，十一阶段原流程复用。

边界：这些是当前Windows Editor受控1000AI窗口；Role fallback峰值4/3而不是0，混战P95波动仍在，正常drop仍非0。不能声称H07/Android或整tick33ms通过，不把均值当P95或把父子耗时相加。

证据：[完整报告](../../../../artifacts/diagnostics/NTSD-H06-ROLE-DEFAULT-ADMISSION-20261009/REPORT.md)、[Task](../../TASKS/NTSD-H06-ROLE-DEFAULT-ADMISSION-20261009.md)、[生产Change](../../CHANGE-RECORDS/NTSD-H06-ROLE-PRODUCTION-DEFAULT-001.md)、[窗口Change](../../CHANGE-RECORDS/NTSD-H06-ROLE-ADMISSION-WINDOWS-001.md)、[H06方案](../../../../Assets/NTSD/Docs/android-mobile-readiness/h-06-collision-broadphase-1000.md)。原失败与备份保留。
