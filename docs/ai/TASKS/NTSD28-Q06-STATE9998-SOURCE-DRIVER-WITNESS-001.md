VERIFIED_SOURCE_MODEL_DIAGNOSTIC_ONLY，224/672全存活且frame/lifecycle无错误，最终manifest-final/REPORT留证。以下为事前范围。



# state9998原版完整driver见证

准确一个新的native诊断runner，直接调用原SimulationTickDriver28完整step，不重新实现规则。覆盖type0..6、state9998停留/从0进入、state0控制、HP0/500、地面/空中、slot0/70，各三个完整step，输出原raw50和存活/step结果，same-seed复跑。先以观察事实裁决SerialTickAll残余；grep无命中不能单独证明需要删除。若有差异另建Unity生产Record；本Record不改Unity、不改正式源或EXE，保留schema15/23/26/2/2、raw47/3。原构建器重验正式EXE/75源码manifest，输出只到Temp/artifacts。编译/运行失败保留；无服务/资源/shutdown职责变化，回滚仅本新脚本且需批准。
