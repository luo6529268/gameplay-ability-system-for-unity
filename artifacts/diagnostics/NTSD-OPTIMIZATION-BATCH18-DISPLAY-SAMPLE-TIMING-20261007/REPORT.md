# 第18批：原Battle显示样本alpha与CPU时序报告

结论：SCOPED_DISPLAY_SAMPLE_TIMING_PASS。原Editor新鲜focused54/54与原saved Battle自然240tick/293camera通过；生产脚本/DLL无新修改。父M-03/H-11仍OPEN/RUNTIME_PENDING，非1000AI、120FPS、完整0GC或Android证书。

## 实际结果

- 原Editor2022.3.62f3、PID19040/6401；原Menu clean8roots确认后仅载入原saved Battle clean11roots，实际菜单 NTSD/Validation/Optimization/Batch18 Display Sample Timing。启动UTC2026-10-06T17:33:43.9426125Z，原件输出run-01；没有第二Editor、Scene保存或资源/设置编辑。
- 自然tick8→248，293camera，240个不同camera观察publication（首tick9至248），latest logic/publication/display对应。
- 53对同publication样本全部builtAlpha变化；378个命令identity/order与位置差值核验，100次非零插值运动差；最大误差0.0000476837158203125 pixel，显式浮点容差0.005 pixel。只命令位置/序列，不是整场逐像素一致。
- 293次new plan generation的builtAlpha均在queue timestamp→CPU camera begin/end年龄区间内，alpha区间数值容差0.001（正常33ms时约33us）；不让host解析alpha冒充当前网格alpha。实际builtAlpha范围0.1039848486～1；没有alpha越界、版本滞后。
- 首观察某publication的CPUcamera-end queue age：240点，min4.1003/mean5.7194075/max10.0458ms。全部camera-end年龄max35.6957ms（同publication再次渲染alpha可正常钳到1），不代表逻辑cadence变更。
- 21个实际actor source坐标变化，1126 snapshot字段核对，18新增/16离开publication身份，最多8entity/12command/12physical segment，两submission slot观察。
- window Build293=publication240＋alpha53，vertex upload293/8608 vertices/378752 entity vertex bytes，failed/rejected/growth0，repeated0。不是Foot/Health/索引/submesh/GPU流量或draw-batch证据；无新收益A/B。
- observer及camera envelope分别0B，既有tick/Update/Late/PlayerLoop端点分别0B/collections0。Editor collection control与PlayerLoop hard gate均false；不相加、不当1800全链/其他线程/native/Player/设备0GC。
- 有序关闭对象/slot/borrower0，Scene clean/SHA相同；之后原Menu恢复clean8roots/idle/nonPlay。

[原窗口](run-01/production-window-01.json) / [计数](run-01/materialization-window-01.json) / [复算摘要](window-summary.json)。
相关focused job12b13f3dfba1464594c37f34ab651fcf，54/54 Passed/2.3084324s，[完整原件](focused-test-result.json)。
Deferral14＋Latest13＋MotionSampler4＋DisplayMotion三个纯测试＋Capacity16＋Shutdown4。并非两个旧UnityTest或9262 discovered tests全部执行。
refresh_unity原实例编译、正常reload完成，read_console(error CS)0；reload retry只未就绪，不当运行失败。

## 实施范围与oracle边界

唯一代码修改：已有Editor [probe](../../../Assets/NTSD/Scripts/Test/Editor/BattleCentralProductionWindowSceneProbeEditor.cs)增加189行、0删除，本批事前当前字节backup为基线。
sampleTiming默认false，新增独立Batch18菜单/输出，不覆盖旧14/15/16证据。
四私有field reader只启动Expression编译、camera hot不Reflection boxing；512command/offset数组预备、超限拒绝观察整窗而不扩容/截断。
不改production/private field值，不新增runtime owner、worker或lease；正常InputSystem运动/技能复用，无强制step/位置/帧/HP/PP修改。Finish清空缓存/delegates，11阶段顺序保持。
技能先查实际URP管线、只使用现有Editor；UV/PPU/pivot/importer/Scene/config不变，通用60Hz建议不适用于33ms权威。

同publication的baseline命令位置取上个camera样本；当前−上个位置应等于两个alpha的独立source-space AwayFromZero取整后D024显示delta之差。
采样是否合资格借用现有MotionSampler，不重新定义其关系/身份/瞬移规则；命令排序输入仍由当前publication物化提供，固定同publicationidentity/localSequence/SortOrder检查不等于证明Q06排序器所有规则。
不读取Q06活跃body，不新闭合透明重叠、首可见资格或native full-world回放。第17批90受控像素样本保持，但没有本批原Battle GPU像素readback。

## CPU时序不是屏幕latency

pendingPublicationTimestamp是中央排队时刻，不是native tick完成、输入采集、正式render handoff时间戳；
camera begin/end是CPU回调，不是GPU consumer完成/Present扫描输出。CPU lease0、ExecuteCommandBuffer返回均不能推出GPU完成。
因此上述5.72/10.05ms仅queue→首次观察CPUcamera-end，不能说“真实屏幕延迟10ms”或“延迟优化通过”。
同样不把renderFps配置120当实测120FPS。GPU真实batch、全场/设备latency和高负载指标继续未知。

## 审计及状态

8准确当前字节备份、653保护SHA零漂移；正式336B44和Menu/Battle两Scene字节稳定，production DLL仍A49C69CE527D7E0268661F96F6A5A28B0C79E6204872BDA6EAF1A1930270AE8D。
EditorDLL05BC004E004625ED644CDF61A82E1EC96C660CC63916BD3A627D87C7E3D58C86、probe5898FE0F9C5BA1A2B1DF7252316C4478DFC9CA0CDD99CF04AE38A2CABFB5415A对应运行版本。
[最终验证](final-validation.json) / [Operation](../../../docs/ai/FILE-OPERATIONS/NTSD-OPTIMIZATION-BATCH18-DISPLAY-SAMPLE-TIMING-20261007/RECORD.md)。
Validate-ChangeLedger实际pwsh显式RepositoryRoot exit0/1314Records/10governed脚本覆盖/0error/本Change0warning，全库4260历史warning不扩大修复范围；git diff --check exit0。HEAD2cccd597.../staged空，无文件删除/移动/Git丢弃/commit/push/旧证据覆盖。
进度34项=高12/中14/低8、父关闭0；EXT1 PROPOSED/MODIFY_REQUIRED无专项M0、MONO USER_HOLD、ATLAS正文/bank/预算/格式/segment/failclosed边界保持。
[Task](../../../docs/ai/TASKS/NTSD-OPTIMIZATION-BATCH18-DISPLAY-SAMPLE-TIMING-20261007.md) / [Change](../../../docs/ai/CHANGE-RECORDS/NTSD-OPT-M03-DISPLAY-SAMPLE-TIMING-018.md)，Change RUNTIME_PENDING只诚实保留其它总体未验收门；本Task限定出口完成。

下一优先真实素材/同布局高segment与高负载、完整物化—上传—录制—提交1800样本0GC；GPU/Android/1000AI证书依独立设备及当前合同，不反复重跑本次低roster窗口或把Q06未知制造为全部优化的全局阻塞。

