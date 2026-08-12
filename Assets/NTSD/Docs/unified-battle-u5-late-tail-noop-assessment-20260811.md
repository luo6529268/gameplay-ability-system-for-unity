# U5 Late tail no-op 候选评估

> 日期：2026-08-11  
> 结论：候选保持诊断关闭，生产默认继续使用完整权威 tail  
> 范围：`LateEntityUpdate/TailAndQueuedFlush` 中精确高槽角色的 N30/transition no-op 证明

## 1. 候选边界

权威 C# `GameTick.RunLatePerEntityUpdatePass` 要求每个 runtime slot 依次执行 state special、recovery、frame tick、death/opoint、cleanup、late tail、runtime snapshot 和 PrevFrame mirror。低 slot 在 late tail 中还可能触发 N30 输入效果，state 13/18/19 与 frame 200 的转换可能生成 transition effect。

候选路径只在以下条件全部成立时跳过 `RunLateTailBeforePrevFrame`：

- 精确 `LF2Character`，派生类型回退；
- runtime slot ≥ 10，低槽 N30 回退；
- previous/current frame 不满足任何 transition effect 分支。

每实体 opoint queue flush、active 检查、runtime snapshot、PrevFrame mirror 和逐 slot 结构可见顺序均未跳过。

## 2. 聚焦测试

`BattleEcsLateTailNoOpEditorTests` 覆盖：

1. 中性高槽精确角色与强制 Legacy 等价；
2. transition state 回退；
3. 低 runtime slot 回退到 N30 权威路径；
4. 派生角色保留虚调用副作用；
5. 候选预热后零托管分配；
6. 生产默认明确保持 Legacy。

新鲜结果：`6/6 passed`，job `edc7079cbb1046868045bcc4f1812592`。

## 3. 1000 AI 相邻 A/B

共同配置为 seed `1314149188`、`Combat1000`、`DataOrientedCanonical`、role-aware collector、Stage host snapshot、PreInteraction proof、30 warmup + 180 sample、每 Update 一个逻辑 tick、零 GC 门禁。

唯一变量为 `forceLegacyLateTailNoOp`。

| 指标 | Legacy A | Candidate B | 结果 |
|---|---:|---:|---:|
| Logic tick average | 23.6720 ms | 25.1598 ms | B 慢 6.29% |
| Logic tick P95 | 28.7732 ms | 35.0593 ms | B 慢 21.85% |
| LateEntityUpdate average | 2.9751 ms | 3.2447 ms | B 慢 9.06% |
| LateEntityUpdate P95 | 3.9595 ms | 4.4226 ms | B 慢 11.70% |
| TailAndQueuedFlush average | 0.7859 ms | 0.8500 ms | B 慢 8.17% |
| TailAndQueuedFlush P95 | 1.0597 ms | 1.0874 ms | B 慢 2.61% |
| Legacy tail 调用 | 210,000 | 0 | — |
| Candidate skip | 0 | 210,000 | — |
| 正式 tick GC | 0 B / 0 collections | 0 B / 0 collections | 一致 |
| 十域 lockstep overall hash | `a13929d8...04e1` | `a13929d8...04e1` | 完全一致 |

报告：

- `Temp/NTSD_ProductionEntityStress.combat1000.u5-latetail-legacy-a.json`
- `Temp/NTSD_ProductionEntityStress.combat1000.u5-latetail-fast-b.json`

## 4. 决策

候选虽保持行为等价和零 GC，但它的 type/slot/frame/state 证明本身包含一次额外帧表读取和多处分支；在当前数据下，这比原路径中两个低槽早退更贵。目标子段、完整 Late pass 和整体 tick 均出现回退，因此不满足任何性能晋升条件。

生产默认已显式设置为 `ForceLegacyLateTailNoOpForDiagnostics = true`。候选只保留为关闭状态的诊断路径，避免未来在没有新证据时重复实现同一方向。后续 Late 优化必须先进一步拆分 `RunLateTailBeforePrevFrame`、queue flush 与 runtime snapshot 的实际成本，不能继续靠外围 no-op 证明猜测。
