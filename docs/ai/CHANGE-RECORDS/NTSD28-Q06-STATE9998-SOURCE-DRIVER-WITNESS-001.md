<!-- CHANGE-RECORD
id: NTSD28-Q06-STATE9998-SOURCE-DRIVER-WITNESS-001
status: VERIFIED
change-kind: STATE9998_FULL_DRIVER_SOURCE_WITNESS
code-path: Tools/NTSD28AuthorityTrace/state9998_driver_witness.cpp
authority: Formal playable SimulationTickDriver28::step and complete original BattleWorld28 call chain.
evidence: Unity SerialTickAll still unconditionally frees frame.state9998; no explicit native9998 branch found, source full-step behavior not yet measured.
-->

# state9998原版完整driver见证

准确一个新的native诊断runner，直接调用原SimulationTickDriver28完整step，不重新实现规则。覆盖type0..6、state9998停留/从0进入、state0控制、HP0/500、地面/空中、slot0/70，各三个完整step，输出原raw50和存活/step结果，same-seed复跑。先以观察事实裁决SerialTickAll残余；grep无命中不能单独证明需要删除。若有差异另建Unity生产Record；本Record不改Unity、不改正式源或EXE，保留schema15/23/26/2/2、raw47/3。原构建器重验正式EXE/75源码manifest，输出只到Temp/artifacts。编译/运行失败保留；无服务/资源/shutdown职责变化，回滚仅本新脚本且需批准。

VERIFIED / SOURCE_MODEL_FULL_DRIVER_DIAGNOSTIC_ONLY；224场景672step全存活，action/state与输入一致；每step一条frame事件，frame/lifecycle错误检查通过；重复字节一致。48条diagnostic为dead type0 input suppressed的正常输入抑制，非frame错误，详见validation.json。最终manifest/source/binary新哈希以build-manifest-final.json为准。
