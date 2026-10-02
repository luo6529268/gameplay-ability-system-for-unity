<!-- CHANGE-RECORD
id: NTSD28-336B44-Q10-C053-STEREO-CAMERA-AUDIT-001
status: VERIFIED
change-kind: CODE
code-path: Tools/NTSD28Q10Diagnostics/c053_natural_double_audio_probe.cpp
authority: 336B44 formal playable battle audio_backend and GameSession camera_x with same C053 event chain
evidence: docs/ai/TASKS/NTSD28-336B44-Q10-C053-STEREO-CAMERA-AUDIT-001.md
-->

# Q10/C053 声像相机首差诊断记录

脚本修改前建立。正式路径、Unity当前状态、精确代码范围、前置 C053 十事件、风险、不回退边界、验收与回滚方式见[同ID Task](../TASKS/NTSD28-336B44-Q10-C053-STEREO-CAMERA-AUDIT-001.md)。只给已有离线诊断加编译期开关/相机观测列，不修改生产或旧输出；默认 schema 必须逐字节维持。若源码侧数据无法复现，状态保持诊断未通过；不据静态差异直接改 Unity 声像。实际修改与验证待填。

实际只改 `Tools/NTSD28Q10Diagnostics/c053_natural_double_audio_probe.cpp` 的CSV表头与每行末尾：在 `NTSD_Q10_STEREO_CAMERA_AUDIT` 编译期开关下输出 `session.camera_x()`，默认分支字节不变。正式根EXE SHA与指定336B44完全相同；使用原28 Core+4 host编译参数两种模式都 exit0/stderr0。宏模式两次12tick/18行原始SHA相同 `9DFB159A…E9772`，cameraX为12/12 tick零，10音频事件按正式函数得到六组左右百分比63/37～78/22；宏输出剥列及重新编译的默认输出都与旧正式事件CSV原始SHA `3E4CA682…778AEF42`同。Unity正式cue fallback `range0`，实际声音voice设2D/居中，不使用正式左右矩阵。仅首差证据，设备L/R PCM未采、D-024固定全背景声像适配尚未决定，生产修复另立包。详细结果见[报告](../../../artifacts/diagnostics/NTSD28-336B44-Q10-C053-STEREO-CAMERA-AUDIT-001/REPORT.md)。未删除/覆盖已有代码或证据；回滚须按既有批准规则精确审阅本宏片段。

最终 `Tools/Validate-ChangeLedger.ps1` exit0/PASSED、1150 Records，`git diff --check` exit0；诊断无Unity生产改动，不运行全量Unity测试。默认与宏输出两种源码编译均0错误/stderr0，精确命令参数和原始CSV均在本包诊断目录。
