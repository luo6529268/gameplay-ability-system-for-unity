> 本Task已完成，仅FindEffect单点修复与100/原千人聚焦门。见同名diagnostics/REPORT.md；下方IN_PROGRESS为事前合同。

# FindEffect 枚举分配最小修复

状态：IN_PROGRESS。用户本会话明确授权；Task/Operation NTSD-H06-FIND-EFFECT-ZEROGC-20261009，Change NTSD-H06-FIND-EFFECT-ZEROGC-001。

唯一生产写域 Assets/NTSD/Scripts/DatParser/Runtime/Models/LoganKindCatalog.cs::FindEffect(int)，foreach改索引for。保持记录顺序、首匹配对象身份、重复effect、未命中null以及null元素遇到时原NullReferenceException时点；不改构造器/其它查找/规则。无新runtime状态、缓存或owner，无容量/失效/十一阶段关闭变化。
唯一新增测试 Assets/NTSD/Scripts/Test/Editor/H06FindEffectAllocationEditorTests.cs 和Unity生成meta。复用准备脚本的私有CreateFixture创建同一100对象（76/24、50/50、40列16X20Z）及可靠GC recorder。先原代码语义测试与100基线（3次collect）；再改生产最小循环、同100候选顺序/handle/载荷、双RNG、声明checksum与0GC；最后调用原H06千人OPointBurst-shaped无hook4warm8sample门，不改变其阈值或先认定234同源。
验证范围：顺序/首匹配/重复/未命中/null元素的独立语义oracle，100前后完整候选序列及状态摘要一致，校准GC0，千人剩余GC，封存无增长及清理对象/slot0。测量不含断言/输出/构造，无Profiler全局录制、不清缓存/旧数据。正常Brute与95OFF保持，不跑H07/Android/91/93/95或重新四场景全量。
测试fixture/数组/recorder均测试owner在finally解除seal、注销实体并Dispose；无新增正式Runtime缓存。
范围外 Scene/Prefab/资源/ProjectSettings/InputActions/Server/Gen/Plugins及活跃Q06方法体不读写。需要其它生产改动、规则或验收合同变化则先报告。回滚仅本Task增量，真实dirty备份见Operation，不自动恢复覆盖、不reset/checkout/clean/stash/delete/push。
风险：枚举器与索引异常行为需保留；100不等于千人，局部0GC不等于H06限定准入/H07/Android。D1=A继续有效，33ms不是局部准入阻塞。
