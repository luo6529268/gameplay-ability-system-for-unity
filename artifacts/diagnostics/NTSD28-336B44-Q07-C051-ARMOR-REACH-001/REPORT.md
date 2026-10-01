# Q07/C051 effect23 护甲正式入口与根回放限定证据

权威为 336B44 根正式 EXE 及对应 playable live source，DAT 为其 `resources/runtime`。本包只验证正式内容的 OID78/action466→467、OPoint OID447/action58→54、effect23 ITR 对 OID97 护甲与 OID2 无甲对照；不改 DAT、Unity 生产或场景。

正式索引 `decoded_dat/data/data.txt` 指 OID78=`c/rai/rai.dat`、OID447=`c/rai/a/rai.dat`、OID97=`c/guy/gei.dat`。OID447/action54 ITR 为 kind0/effect23/dvx1/dvy-20/fall80/bdefend80/injury50；OID97 的 type1 armor 仅把 effect5 列为效果绕过。源 `GameSession28` 固定 seed682973786、mode0、OID78 X500/Z400 面右、目标 X550/Z400 面左、HP/MP500，中性输入80tick。

| 案例 | 当前正式源码完整 tick | 根正式 EXE 独立 LFR 回放 | 源/根已导出字段 |
|---|---|---|---|
| [OID97 护甲](armor97-x550-v2/source-ticks.csv) | tick2 生成447/action54；tick3 effect23 候选1、applied1、选中护甲1、armor decision `applies`1，HP500→495、无甲倒地动作未发生；同输入二次运行 CSV 与 LFR 均逐字节同 SHA | [根报告](armor97-x550-v2-root-v2-report.json) `passed:true`/failureCode0/declaredTicks80 | [逐字段比较](source-root-selected-fields-comparison-v1.json) 80tick×11字段=880/880，零差 |
| [OID2 无甲对照](control2-x550-v2/source-ticks.csv) | tick2 同样生成447；tick3 effect23 候选1、applied1、未选护甲，HP500→450、action180 | [根报告](control2-x550-v2-root-v2-report.json) `passed:true`/failureCode0/declaredTicks80 | 同上 880/880，零差 |

11 字段为父动作、子体数量/首槽/首动作/X、目标动作/X/HP/MP/armorHP/Vx。护甲决定 `applies` 由正式源码逐 hit 结果导出，根 trace 不暴露该决定字段，因此根一致性只声明上述 11 字段，不把根 `passed:true` 扩大成全 World 或护甲内部分支逐字段证明。正式 LFR 包装另有末尾 tick，不参与80 tick比较；报告 `nativeParityClaim:false`。

首次工具 v1 直接把初态放在 OPoint 帧467，首 tick 已进入468、没有子体；其 [护甲原件](armor97-x550-v1/source-ticks.csv)与[无甲原件](control2-x550-v1/source-ticks.csv)保留，不当成护甲阴性。v2 由正常前帧466进入467后阳性。根首次回放 v1 未指定目标面左，默认面右导致无甲 tick3 动作186而源码180；两份 v1 根 trace 与参数保留。根 v2 明确 `--lfr-slot1-facing 1` 后两案各880/880，首差无。

工具用当前 source Core/playable 的 [v2 构建参数](compile-argv-v2.txt)编译，exit0、0 error、0 warning；它是只读诊断候选，不替代正式 EXE。形式资源的 338 个解码 DAT 与 Unity staged 的 [逐文件复核 v2](decoded-dat-staged-vs-formal-v2.json)无缺失、规范化换行后内容差异0；25 个 raw-byte SHA 不同仅为 CRLF/LF，先前未归一化的 [v1 原件](decoded-dat-staged-vs-formal-v1.json)保留并被更正。

[effect22 静态清点](effect22-static-content-audit-v1.json)：338 个正式内容解码 DAT 中仅 6 文件有 `effect:22`，均在护甲绕过列表；没有一行 ITR 使用 effect22。这只是当前内容静态事实，不能证明运行时绝不生成该效果；后续若无正式可达生产入口，effect22 保持条件性，不凭合成 ITR 关闭整个 C051。

本包 `VERIFIED_SCOPED_SOURCE_ROOT / UNITY_PENDING`。下一步以这份 OID97/OID2 阳性同初态运行原 Unity 完整 Driver/原 Battle Scene，找首差；护甲真实效果与 Unity 逻辑是否一致仍未验。C051/Q07/总目标保持开放。
