# 第18批：原Battle显示样本alpha/CPU时序验证

状态SCOPED_DISPLAY_SAMPLE_TIMING_PASS；Change NTSD-OPT-M03-DISPLAY-SAMPLE-TIMING-018。原Editor54/54、原Battle240tick/293camera、378delta/293alpha区间通过；只CPUqueue时序非screen latency，父OPEN/RUNTIME_PENDING。用户再次要求下一批，M-03第17批受控像素通过但自然显示时序仍缺（事前背景）。
唯一代码路径Assets/NTSD/Scripts/Test/Editor/BattleCentralProductionWindowSceneProbeEditor.cs；增加独立batch18入口、240自然tick动态窗、固定512command缓存/只读static field delegates（Expression编译仅启动），记录lastBuilt/lastResolved alpha、queue publication timestamp/version与CPU begin/end；不在hot Reflection boxing、不改production。
同publication跨camera的位置变化应等于两alpha独立rounded delta差；核X/Y/Z投影/category，generation/command sequence保持。新build alpha应落在publication age的CPUcamera begin/end区间（小数值clock误差显式容差），同UnityFrame复用样本不错误要求新alpha。禁止以CPU camera/lease/Execute返回宣称GPU完成或screen latency。
预备固定数组上限溢出拒绝观察整窗、不扩容/截断；初态真实original Battle/自然InputSystem移动和技能，不强制step/写位置帧HP/PP。600秒startup/900总deadline沿用；只本probe两mode旧batch14/15/16默认不变。运行原Editor focused54与唯一真实Battle窗，scene SHA/clean/11阶段关闭objects/slots/borrowers0，observer/camera端点0B各自记录但非full-chain/设备0GC。
8准确现存备份/653保护与正式336保持；EXT1 PROPOSED/MODIFY_REQUIRED、MONO USER_HOLD、ATLAS正文/bank/预算/格式/segment/failclosed及Q06未知不变。Parent OPEN/RUNTIME_PENDING；不是全部自然first-visible/整场GPU/1000AI/Android或新performance gain。
回滚：Operation当前字节backup，仅定向撤回本probe hunks和进度追加，保留旧任务及资源；无新runtime owner/关闭阶段。证据../FILE-OPERATIONS/NTSD-OPTIMIZATION-BATCH18-DISPLAY-SAMPLE-TIMING-20261007/RECORD.md，最终 ../../../artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH18-DISPLAY-SAMPLE-TIMING-20261007/REPORT.md。

