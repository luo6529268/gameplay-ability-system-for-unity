# 第29批现有 Role-aware 实际1000AI准入

## 最终结论：现有候选有实质收益，未默认推广或性能达标

SCOPED_CANDIDATE_ASSESSMENT_COMPLETE / POSITIVE_GAIN / NO_DEFAULT_PROMOTION。独立原Editor两固定120warm+180sample完成，实际观测activeAI/baseRoster下限各1000、每窗12次sample观察、harness/workload有效/StoppedCleanly，原9/9 request测试通过。仅collector brute→role/output不同，seed/完整逻辑/renderer/sound/33ms/3ms/max2保持，不重写算法或修改生产配置。

| 本次旧基线对比（ms） | Dispersed brute→role | Combat brute→role |
|---|---|---|
| logic mean | 467.5241→48.4233 | 431.0003→45.7206 |
| logic P95 | 706.7583→73.4995 | 691.9199→54.9208 |
| CandidateCollect mean | 429.1872→15.6509 | 398.0743→15.2590 |

约90%逻辑平均耗时下降、约96%候选收集下降仅为本次同参数旧/新单窗观测，不承诺设备/所有分布或120FPS。Role实际300tick：Dispersed Direct259/Tree41，Combat Direct268/Tree32，非只改标签。P95仍>33ms，droppedBacklog364/331，显示帧间隔mean165.068/157.497ms仍很慢；FrameTiming API另列在原report，不推导GPU batch/SetPass。H07性能未达标、四正式1800窗未被替换，0GC仍UNCALIBRATED_COUNTER/UNKNOWN，不发证书。

一致性：各末tick300扩展schema-v5的10hash＋lockstep10hash全同；两个完整final-checksum.json原文件SHA和规范化JSON与26对应基线全同，原JSON无首差（snapshot-equality-01.json）。只证明两个末tick出口，不证明每tick/native全链。baseline-reuse-01.json记录十个生产源码与26同/正式336同及原件身份，candidate-result-summary-01.json与各report/observation保留。没有生产默认切换，一般Battle仍brute，不把候选收益写成用户默认场景已获收益。

Suite MEASUREMENTS_COMPLETED/DONE、两窗关闭恢复true、11阶段objects/slots/borrowers0、两SceneSHA同/原Menu clean恢复。195保护/11当前dirty备份/HEAD同，共享terminal由26 FAIL变为29 Combat PASS是事前声明输出替换，原字节保留，不删除旧结果。首次备份JSON输出截断只重读原副本恢复manifest；只读JSON遍历旧helper自行终态，未终止Unity或其它任务。最终validator/diff证据见final-validation-01.json。

本Task评估闭合、Record VERIFIED仅此范围；H11完整0GC、第26批正式拒证与后三formal未跑均保留，H07父项/Goal仍OPEN/active，阶段4of6/新批8。下一沿现有role候选做必要长窗/一致性准入、识别剩余主线程成本，不重复同构GC观察/重跑已有效正例或解除默认/EXT1/Mono/ATLAS门。下面运行中/PLANNED为历史。

最新：RUNTIME_PENDING / CANDIDATE_WINDOWS_RUNNING。独立29菜单仅一次，原Battle已进入Play/服务准备；两个实际写出的request重读与26基线仅formalCollectorMode brute→role和output变化，字段核对原件frozen-request-check-01.json。完整逻辑/耗时/末tick300全部hash/最终关闭与Scene恢复尚待，不能拿9/9 request测试宣布收益。运行中不编辑代码/刷新/重启或再派发测试；原H11失败/正式长窗缺口保持。

FOCUSED_TEST_PASS / READY_TO_RUN。原Editor三新增RED后实现，九request9/9 GREEN jobdadecd2f5ed046eb9d15a11348510d23；唯一Suite owner小复用、原六request不变。baseline-reuse-01.json保存十个生产源码与26同、正式336B44、两valid smoke tick300完整hash与原report/request/snapshotSHA；末tick证据范围不扩大。validator1325Records/9代码diff PASS，4277历史warning。11当前副本/195保护；尚未实际候选窗口或收益，不称1000AI优化好。

PLANNED。只两旧smoke同参数候选、唯一Editor coordinator路径；未写脚本/未测试/未测量/未切默认。第26批有效brute两个smoke与tick300全部checksum组件作原基线，H07正式四窗未被候选窗替代。H11第28批严格0GC FAIL保持；性能主线转CandidateCollect，不重跑同构相机观察或GC正例。准确范围/生命周期/保护/验收见同IDTask/Change/Operation。
