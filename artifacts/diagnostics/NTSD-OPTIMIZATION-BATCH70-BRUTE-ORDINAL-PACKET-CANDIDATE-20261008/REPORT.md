# 第70批普通Brute原序packet候选
## 当前结论

SCOPED_CORRECTNESS_PASS / LOCAL_GAIN_SIGNAL / NOT_ADMITTED：25有效RED后实现默认OFF原序16-packet，34/34 GREEN（新25＋旧9）、固定两cost2/2 PASS，两个布局均满足事前mean及median≥5%局部收益门。阶段H07/H11仍OPEN、4/6、Goal active；49批已执行（22—70），次数只审计。后继应闭合完整Driver/正式Windows证据后才考虑推广，不能以局部收益宣称1000AI或FPS达标。

| 固定1000participant布局 | OFF mean/ms | ON mean/ms | mean减少 | OFF median/ms | ON median/ms | median减少 |
| --- | ---: | ---: | ---: | ---: | ---: | ---: |
| spacing120 | 24.230675 | 16.079750 | 33.6389% | 23.180350 | 16.081950 | 30.6225% |
| spacing12 | 43.009950 | 37.208575 | 13.4884% | 42.798750 | 36.897400 | 13.7886% |

每布局固定40attacker/960body、warm4/sample8、OFF/ON交替、capture→collect→end含每次envelope/packet重建；完整候选sequence/payload/handles/RNG一致，packet数组未替换。原始8＋8 samples在cost-audit-01.json/XML。不称实际1000AI、120FPS或0GC，GC UNKNOWN；未跑Driver/native/正式Windows/Profiler/GPUcapture/M0。完整父项及68实际性能FAIL/H11旧43 FAIL不变。

资格job34b4cc9886a34edf989f9953464df037实际34/34、0fail/0skip、3.318888s；cost jobfa687171e40146c38ad6ae259d52adda实际2/2、0fail、3.985824s；catalog10004不是执行数。RED/资格/cost独立XML SHA分别CDE31C8A...674955/7A61E17B...005310/2EEBBD67...C03302，每次先terminal/核fresh再Copy到ABSENT，不覆盖旧原件。旧tests/helpers剔除本批新增后normalized全文相同。当前Query SHA394EF397...3FAD8/tests6EF06591...7C941。

两个C#实际改动：Query默认falseproperty、value packet冷数组/active prefix每collection重建、未声明组合和capacity回原scalar；Editor新增25资格及2局部cost。普通union＋ALL kind5 exact rect使用既有envelope；first partial scalar，未来final partial active prefix仅保守拒绝；原ordinal/empty roster/方向/IsBound base gate/精确survivor不改。新增array没有entity refs/worker/borrower，Query/World既有关闭owner不变；容量ceil(prepared entities/16)，完整数组steady及cold resize旧＋新transition需计CPU working-set，实测stride/bytes UNKNOWN；未改ATLAS预算或正式renderer。

## 历史事前记录（不代表当前状态）

最终审计2026-10-08T07:05:31Z：490/490保护同、HEAD同；七准确before副本与06:50实际copy SHA同，两script before另与原manifest同、五治理bootstrap差异已在Operation明确保留。原Menu savedclean8roots/idle/nonPlay/CS0；旧tests/helpers全文normalized保持。两source git diff --check exit0；七before-relative --check无新增whitespace消息（no-index代码exit1仅有diff，不谎称exit0）。Validate-ChangeLedger.ps1 exit0、1367Records/6governed codediffs、0error/4314历史warnings；没有清理旧warnings或用户代码。Q06只hash/state、未读方法，无Scene/Prefab/resource/Settings/Gen/Plugins/Server写或Git丢弃/commit/push。delivery-audit-01.json保存实际hash/live范围。Next Driver资格尚未运行，不将READY当完成。

PLANNED / IMPLEMENTATION_NOT_STARTED；新默认false、当前没有测试/成本/性能结果，69几何资格复用不重采。
两C#准确边界、25新具名test-first＋旧9及仅两千entity局部cost见Task；保持原binding清理/顺序/容量回退，不切collector或专项。
阶段4/6、H07/H11 OPEN，目标active；只有实际代价和正确性结果后追加，不把准备文档当优化收益。
