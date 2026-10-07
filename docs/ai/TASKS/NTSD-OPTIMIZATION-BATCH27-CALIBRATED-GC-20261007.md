# 第27批 H-11 可靠采样与选定完整表现链复验

状态：`PARTIAL / CALIBRATION_AND_CPU_BRIDGE_PASS / CAMERA_ALLOCATION_FOUND`。实际第6个新批：校准4/4、两个原完整CPU桥2/2通过；原一次1800distinct camera完成，有效校准发现camera2/observer13分配事件，严格0GC未通过。无跳帧/放宽，十一阶段零残留/Battle同/Menu恢复；H11待定位/首阶段4of6/Goal active。下方原范围和固定矩阵保留，不重复正例或完整旧历史。

## 原因与范围

第25批在当前原Unity Editor分配、访问并保持存活1MiB数组，`GC.GetAllocatedBytesForCurrentThread`仍返回0；这是旧0B证据无效，不是已经证明生产路径没有分配。本批替换选定验收工具的证据来源，先校准再验收，不重复旧全量历史，不新增优化父项或生产算法。

使用Unity2022.3官方当前线程 `Internal / GC.Alloc` 的ProfilerRecorder事件采样作为候选：不使用默认逐帧求和或循环覆盖；准备阶段创建固定容量recorder并预暖采样API，收集前Reset/Start，范围结束Stop，再读取Count及样本。有效marker、正对照响应、空操作不误报、非wrapped/非饱和缺一不可；数值单位必须实际记录，不能把marker时间值臆称分配字节。校准失败、buffer饱和或证据缺口记INVALID/UNKNOWN，不判0GC。

参考仅定义API能力，不能代替本项目运行结果：

- [Unity2022.3当前线程GC.Alloc示例](https://docs.unity3d.com/2022.3/Documentation/ScriptReference/Unity.Profiling.ProfilerRecorderOptions.CollectOnlyOnCurrentThread.html)
- [Reset停止收集并清空样本](https://docs.unity3d.com/2022.3/Documentation/ScriptReference/Unity.Profiling.ProfilerRecorder.Reset.html)
- [默认采样包含逐帧求和和循环覆盖](https://docs.unity3d.com/2022.3/Documentation/ScriptReference/Unity.Profiling.ProfilerRecorderOptions.html)

当前只读工具事实：第26批已完成Dispersed smoke的report.json中，帧级 `Memory / GC Allocated In Frame` 有178样本、平均240500.0787B；它包含Editor/其它范围，不能归因本批选定表现链。旧 `GC.Alloc` 发现器要求UnitType Bytes，报告为该精确单位/marker组合未发现，这不证明所有GC.Alloc marker都不存在。新候选必须记录实际handle/category/unit并做正反校准，不沿用“必须Bytes”的发现假设，也不预宣告Recorder可用；旧API0B/全帧值都不能替代所声明scope的证书。

## 准确诊断代码路径

1. 新 `Assets/NTSD/Scripts/Test/Editor/BattleScopedGcAllocationRecorder.cs`：Editor-only固定容量采样/校准 helper及聚焦正反测试；没有Runtime manager、生产owner、可增长容器或新增流水线。
2. `Assets/NTSD/Scripts/Test/Editor/BattleRealTextureSubmissionEditorTests.cs`：接入校准采样；仍运行既有两个活动aux选定case。完整范围包括CopyFrom/DisplayMotion/resolve/Build/Foot/Health/mesh上传、CPU lease、CommandBuffer录制、Graphics.Execute及release/retire；采样和标量统计不豁免分配。保留旧API原始读数但明确未校准，不用它判证书。
3. `Assets/NTSD/Scripts/Test/Editor/BattleCentralProductionWindowSceneProbeEditor.cs`：新增隔离第27批菜单/输出与采样证据，既有原Battle BeginCamera→EndCamera范围完整保留；相机observer单独取样。原活动Foot/Health、两slot、draw录制/执行、generation、关闭和Scene保护验收不放宽。

不修改生产渲染、AI、Driver、World、pool、publication、Q06排序、GPU API或资源；不将本证据推广到其他线程、native/GPU分配、GPU完成、1000AI、PlayerLoop全局或Android。

## 固定执行顺序和矩阵

1. 原第26批套件必须先取得终态（包括有记录的失败收口）并回收最终报告；逐项保留已执行和未执行窗口，不为凑齐六份而放宽守卫。确认已停止、十一阶段关闭、原Menu clean/Scene SHA保持、无在途测试或编译。不得通过刷新、取消或重启长窗来抢Editor。2026-10-07本轮实际已取得PARTIAL / DONE：两个smoke有效、首个1800正式窗口GC边界守卫失败，后三窗未执行；最终关闭/Scene恢复通过，当前只确认安全窗口，不称H07报告已全交付或本Task已实施。
2. 脚本前创建唯一Change Record，回链Ledger/STATE/handoff；补充本Operation的准确脚本前脏字节备份和保护清单。复用第25批已证旧API正对照失败作为缺陷证据，不制造未实现helper造成的编译错误充当RED。
3. 当前Editor聚焦校准：已知存活1MiB分配、预暖空操作、固定小容量饱和/无效状态拒证书。只在实际校准有效后接入和执行完整链。Dispose/异常/非活动状态必须恢复，诊断输出位于采样范围外，不修改Profiler全局设置或另开Profiler窗口。
4. 既有CPU桥两个case各64warm+1800sample：1000 command StrictOrderedDraw/1000活动aux；4097 command OrderedChunks/17活动aux。保留两个submission slot、原纹理身份、44stride、segment/chunk口径和容量/CPUlease断言，不重跑旧六档或增加角色矩阵。
5. 原saved Battle一次1800个distinct camera，保持既有活动Foot/Health配置和全部BeginCamera→EndCamera范围；不为隐去分配而跳过前若干相机或残余子路径。新正反校准在相机采样范围外，observer开销单独记录；有分配如实失败并先定位实际调用点，不立即扩大到全局渲染重构。
6. 原十一阶段关闭、对象/slot/borrower零残留、退出Play、原Menu恢复、Scene SHA/保护字节、Ledger与diff-check。既有显示/像素/消重A-B/关闭等有效证据复用，不因计数器缺陷全部重做。

完整路径通过条件：有效且前后校准响应的当前线程GC.Alloc事件0、非溢出/非wrapped、容量增长0；既定链覆盖完整和全部正确性/生命周期断言通过。0事件只是所声明当前线程范围的managed allocation证据；不臆报总字节、不用CPU lease/Execute返回证明GPU已消费完。

## 输出、失败和恢复

只新建 `artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH27-CALIBRATED-GC-20261007/` 内不复用的结果文件；拒绝覆盖已存运行、拒绝未知request/owner，不删除旧失败。真实计数非0先留调用范围和证据；工具未校准则继续有据解决采样缺口，不把UNKNOWN变成PASS或机械次数停止。

本批不解冻EXT-1、Mono/asmdef、ATLAS bank/预算/格式/segment或其余28项；33ms/3ms/max2、checksum/RNG/publication只读和透明命令顺序不变。主进度只维护既有优化总表。任何后续生产修复需要准确新Change/path/测试，不从本诊断Task推导无边界授权。
