# R8-WP01G — certification synthesis and next executable work

> 日期：2026-08-23  
> 状态：`COMPLETE / DOCUMENT-AND-EVIDENCE ONLY`  
> 脚本改动：`禁止`

## Goal

在不把Unity compile/self-check/Play/performance证据扩大成C++ runtime full-trace证书的前提下，逐项汇总
`R1-SOURCE-ALL-DIFF-REGISTER.md`中的全部D-ID，区分：

1. 已有C++ release source合同且Unity代码差异已关闭；
2. 已有Unity S4/自动证据但C++ full trace继续BLOCKED；
3. 代码已写但仍缺Unity joint/Play证据；
4. C++ source/reachability仍为UNKNOWN或INFERRED，当前不得改gameplay；
5. 用户明确排除、暂缓或保留的适配边界；
6. 真正存在source-confirmed且尚未关闭的Unity代码差异。

输出依赖顺序最前、可独立执行的后续Work Package；若第6类为空，必须如实写明，不得为了产生代码diff
而把证据缺口伪装成脚本错误。

## Scope

- 只读审计C++ release source合同、现有Unity实现、Task/Change Record与R8证据；
- 汇总68个当前D-ID并给出最高证据层与未关闭原因；
- 更新R8 orchestration、STATE、总计划、差异登记与handoff的一致状态；
- 将用户最新决定固定为：IL2CPP Player不在当前处理范围，不继续build/run/诊断/修复；
- 推荐下一项source/reachability调查或Unity运行时验收包。

## Authority / Evidence

- 唯一行为权威：`J:\QQFile\NTSD2.4\ntsd_release` release live source；只读，不运行、构建、修改或写入；
- Unity production runtime、Change Records、Play probes与current-build报告仅证明其覆盖范围；
- `R1-WP02` full trace保持`BLOCKED`；
- 用户最新范围决定高于旧WP01F双backend认证安排；
- T8默认`stage.dat`继续暂缓。

## Deliverables

1. `docs/ai/RESEARCH/R8-WP01G-certification-synthesis-20260823.md`；
2. 更新`R8-WP01-production-certification-orchestration.md`的WP01F/G状态；
3. 更新`STATE.md`、总计划、all-diff register必要状态说明与当前handoff；
4. 至少一个后续Work Package建议，写明Goal、Scope、Authority/Evidence、Unknowns、Deliverables、
   Verification、Stop conditions与Out of scope。

## Verification

- all-diff register的D-ID计数与synthesis一致；
- 不把`RUNTIME_PENDING/UNKNOWN/INFERRED`写成`VERIFIED`或“需要修改”；
- Change Ledger validator通过；
- 本包Unity脚本diff必须为0；
- 不运行Unity Play、Player、性能测试或C++ executable。

## Stop conditions

- 发现source-confirmed且未关闭的gameplay差异：登记下一独立Task/Change，当前本包不直接修改；
- 需要改变pass ordering、长期架构、CentralOnly、容量、30Hz、pool/worker/0-GC边界；
- 需要恢复IL2CPP、T8、Android、服务器或C++ runtime观察；
- 需要用户批准的R3+修复包。

## Out of scope

任何Unity脚本/scene/config修改；IL2CPP Player；C++运行/构建/写入；R1-WP02替代方案；T8默认资产；
Android真机；服务器；以WP01G宣称完整C++ runtime对齐。
