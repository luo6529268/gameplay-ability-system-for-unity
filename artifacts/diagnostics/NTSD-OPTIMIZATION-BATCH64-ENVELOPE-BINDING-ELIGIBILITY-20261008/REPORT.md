# 第64批：既有包络＋绑定复用基线的参与资格复用增量

最终治理/保护追加：pwsh -NoProfile -File Tools/Validate-ChangeLedger.ps1 实际exit0、1361Records、4373WARNING/0ERROR（非零warning保留，完整stdout未持久化，仅摘要）。全工作树git diff --check exit2：四共享治理文档在并行TMP新增尾段有EOF空行，未改动或归因；本批两C#＋总表＋Task64范围exit0，仅4CRLF提示。UTC02:22:01.9699666Z/HEAD527350afa08633357ba20e7453a9d260eaa2b97c、116guards零差，四原report/完整snapshot及suite SHA仍同；派生三Markdown无尾部空白，validation-final/post-document-audit-01.json留实际结果。

续轮审查已得新信息：[当前路径计时缺口](NEXT-ACTION-FINDINGS.md)，包络构建和direction入口均排除旧branch timing，旧58 testcase明确保持该行为。第65批准确Task已READY（未实施/未测试/未测量，不加已执行数），下一明确opt-in维持当前路径再获取必要分支证据，不重开45/64窗口或盲堆微候选。

当前：SCOPED_WINDOWS_AB_COMPLETED / MIXED_RESULT / PERFORMANCE_FAIL / NOT_ADMITTED。UTC2026-10-08T01:54:35.2808391Z原suite-result.json已DONE/MEASUREMENTS_COMPLETED、completedRuns4/error空；不是仍在运行。下方WINDOWS_LAUNCHED/READY是当时快照，本段及原件定义最新结果。Change VERIFIED仅表示已声明的诊断接线和必要验证闭合，不代表H07/H11优化完成或候选推广。

## 第64批实际四短窗结果

同一Suite依次采样，两侧包络和拒绝绑定复用ON，只有参与资格复用OFF/ON变化；每窗实际minimumActiveAi/baseRoster1000、120warmup＋180sample/完整300Driver tick，workload/harness均有效。原件及逐字段汇总见[原始终态](windows-01/suite-result.json)、[四窗汇总](windows-final-summary-01.json)、[组内比较](windows-comparison-01.json)。

| 布局 / 资格复用 | logic平均 ms | logic P95 ms | dropped ticks | PairExactLoop平均 ms | 显示帧平均 ms |
|---|---:|---:|---:|---:|---:|
| Dispersed OFF | 72.253690 | 85.615885 | 693 | 39.314933 | 234.973690 |
| Dispersed ON | 75.485089 | 111.139810 | 672 | 37.519451 | 230.334505 |
| Combat OFF | 78.237568 | 100.492790 | 667 | 43.465384 | 231.290019 |
| Combat ON | 75.283896 | 97.746965 | 866 | 38.887202 | 271.384426 |

组内logic平均：分散增加4.472296%，混战减少3.775261%；PairExactLoop分别减少4.566922%/10.532939%，不等于整体逻辑稳定收益。Combat ON可见帧最大8208.046913ms，原因未定位；不能无证归因外部干扰、删除异常值或重复采集寻PASS。一次顺序A/B仅支持MIXED_RESULT/不推广，不能定因、更不能用非同期旧63数值当本批基线。maxBacklog0伴随dropped>0不代表逻辑跟得上。显示值为Time.unscaledDeltaTime，非GPU/Present/Android/120FPS证书。

## 正确性、容量、GC和关闭

- 两布局分别20项终态checksum/lockstep哈希0差、完整末snapshot SHA相同（Dispersed F1A2CAC863D1AA995D49BEF7DA2D5C1B30337465FA846DB0CEFAF86D8AD7B6AA；Combat E4916ACA8C5BE675B8192D5A6C1429029FD94DB6F838CF6C82E532B030836B12）。这是末态配对证据；本批8具名Driver资格另覆盖88paired/176实际tick的声明域，不晋升正式native全World逐位证书。
- 四完整同步StepOneTick scope均前后1MiB正例/空例校准PASS，300accepted/180steady/0invalid/steady0allocationEvents、provenSteadyAllocatedBytes0；不是从TimeNanoseconds读数推导字节。warmup非零事件保留。该逻辑scope不覆盖H11完整物化—上传—录制—提交，旧43迟发12event FAIL及61byte预算UNKNOWN不变。
- 三机制configured/unchanged/restored均PASS，资格实际OFF0/ON300；包络与绑定确实应用、原三值false已恢复，四admitted production defaults未变、exact fallback=false。没有新Runtime/default/collector/backend修改。
- capacity gate四窗passed，180observed/0violating/capacityCriticalDelta0/runtimeGrowthReject0；无截断或以拒绝替代有效千人。十一阶段有序关闭、activeGO/World objects/claimed slots/pool active/cleanup exception均0，suite remainingObjects/Slots/Borrowers0，双Scene同/原Scene restored。
- 中央每窗89accepted frames，source/resolved聚合相同，末reason/refusal空；这不足以证明独立central unresolved/stale门，保持待验。CPU DrawMesh/segment不推导真实GPU batch，不在本批启动Frame Debugger/GPU capture。
- 原资格8、Suite60、label3共71实际case的原XML已本轮重核SHA/8+60+3 passed/0fail/0skip，不重复运行。RED和原失败不删除。

## 边界和下一必要动作

本批接线结果可以收口，候选继续OFF/NOT_ADMITTED；H07仍PERFORMANCE_FAIL、H11 OPEN，本阶段4/6、限定产物5/6、父项34关闭0、22—64实际43批、Goal active。短窗失败不追加正式1800找PASS；不重复64/63/旧资格/失效byte API，不切Role-aware/collector/backend或解冻EXT1/ATLAS/Mono。

下一用本批残余PairExactLoop37.519/38.887ms和当前Brute路径复盘高杠杆重复工作。细分BruteCoarse/RejectedBinding/PairAllowed/ExactWork计时开关本批OFF，零值不代表零成本；不得无证决定是某分支根因。先确认旧已存分支证据是否仍覆盖当前包络路径，再声明有新信息的最小动作，而不是堆微候选。

[最终保护核对](final-integrity-01.json)：UTC02:08:47.7230059Z/HEAD527350afa08633357ba20e7453a9d260eaa2b97c，116guard零差、10dirty副本SHA保持、5声明源与窗口冻结同。并行TMP工作与本批分离保留，未执行Git修改、删除、移动、Scene保存、资源或ProjectSettings操作。本轮只读取已结束原件并写派生证据/文档；没有重新launch、测试、Profiler或测量。

当前：WINDOWS_LAUNCHED / RUNTIME_PENDING / GAIN_UNKNOWN / NOT_ADMITTED。菜单只一次；四实际requests均65字段除output与旧63相同，原Editor实际Battle Play/transition已确认；仍待新窗口实际样本、三flag、完整logicGC、终态hash和关闭。71资格/接线case通过不替代这些终态。request-comparison-01.json/launch-01.json留痕，不能把read timeout或未出报告当终态。

当前：FOCUSED_TEST_PASS / WINDOWS_READY / GAIN_UNKNOWN / NOT_ADMITTED。8 Driver资格＋60接线回归＋3 label guard全部实际通过，88paired/176tick声明字段一致与候选应用已核；native仅两适用root的四字段，非全World等价。Suite216123B/SHA4DBE1189...9119E1、Admission53559B/SHA214B2B28...B1BD4及3未改runtime/harness/observer源在windows-preflight-01.json冻结。116guard保持/原Menu clean8roots idle、新输出根不存在、共享request无/terminal归63，下一同批一次四固定1000AI120+180；没有本批收益或完整GC/关闭结论。历史的GREEN运行中/PLANNED描述由本段覆盖当前状态。

当前：CODE_WRITTEN / DRIVER_GREEN_RUNNING / GAIN_UNKNOWN / NOT_ADMITTED。仅Admission修改；RED实际5失败均缺新组合接口，原件保留。实现后原Editor唯一job 8bad57e0cb8e4974976bf619a0672abc（新5＋旧纯3）已启动、终态待，source freeze及请求/启动输出已保存；Suite/runtime/default未改、千人四窗未启动。43已执行、阶段4/6、H07/H11 OPEN、Goal active；下文PLANNED是保留的事前事实。

PLANNED / COMBINATION_PENDING / GAIN_UNKNOWN / NOT_ADMITTED。事前10准确dirty副本/116guards及新HEAD527350afa08633357ba20e7453a9d260eaa2b97c已冻结；原Menu idle。只有准确合同与备份，没有新C#/测试或实景收益。42已执行＋64准备、阶段4/6、H07/H11 OPEN、Goal active；完整结果后续追加，不复制旧50/63数字作本批成绩。
