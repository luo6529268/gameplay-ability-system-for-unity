# 第33批 H11 EndObserver 六阶段定位
最新终态：PARTIAL / PROVENANCE_PASS / ZERO_GC_FAIL。本批是必要的既有失败归属诊断，不是FPS改善。完整root范围与严格拒证门未变；H11/H07/Goal仍开放。下方CAMERA_READY/PLANNED为事前历史。

原Unity2022.3.62f3/PID19040/6401：有效RED 3汇总stub失败→GREEN 10/10（1.3274319s），新nested空/已知1MiB正反有效；旧已过正例/两CPU桥不重复。独立33菜单只一次，1800自然camera、tick8→1256，最终production-window-01.json phase DONE/status FAIL。

| 选定范围 | 校准后GC.Alloc事件 | 非零帧 |
|---|---:|---|
| camera原scope | 2 | ordinal1/5 |
| EndObserver原root | 13 | ordinal0 |
| snapshot/gates（含CaptureSample入口） | 11 | ordinal0 |
| segmentBindings | 0 | 无 |
| commandsAndDraws | 0 | 无 |
| alphaAndTiming | 0 | 无 |
| sampleWrite | 0 | 无 |
| completion | 2 | ordinal0 |

两recorder前/后正反校准均PASS；每帧固定六scope valid/calibrated，不wrap/不saturate，invalidFrames0，root-children unattributed0。marker unit TimeNanoseconds，raw总值不是字节。只能确认块级归属，具体调用/初始化/JIT/字符串等原因未证；camera另2准确来源未定位。不跳首camera、不排除observer、不制造冷启动例外。完整scope仍FAIL，原exception与1800raw frame保留，不能把10测试或子块0叫完整0GC。

显示/容量/关闭保护：两slot、Foot/Health每帧至少各2、11560 CPU DrawMesh录制=执行，growth0/CPU read lease0；orderedShutdown=true，objects/slots/borrowers0。Battle/Menu文件SHA保持，退出后原Menu8roots/isDirty=false已恢复。source全窗冻结95692A5EB0493A59251D3B795DE8C90ABF306D0B1C9B851BF5DD95691291440D，247保护/8备份/HEAD保持。

实际审计Tools/Validate-ChangeLedger.ps1 PASS（1329 records/11 governed code files/4248历史warning），git diff --check无诊断；旧4248 warning不删除/不包装成新增失败。窗口期间没有改脚本/刷新/测试/重复菜单；6401暂时重载拒绝后原实例恢复，未重启Editor。只声明测试/归属/保护证据，无新1000AI/native/逐tick/GPU/Android/帧率证书。

证据：red-01.json、green-01.json、pre-run-audit-01.json、camera-dispatch-01.json、camera-01/production-window-01.json、stage-summary-01.json、scene-restore-01.json、post-measurement-audit-01.json与最终final-audit-01.json。下一只对已识别块补直接因果/初始化证据或H07实际collector热点，不重复相同观察窗口/盲改Runtime/切默认；主进度仅原tracker。

当前FOCUSED_TEST_PASS / CAMERA_READY：原Editor有效3 RED→10/10 GREEN、1.3274s，nested空/已知1MiB正反通过。247保护/8备份/HEAD、validator1329Records/11files PASS（4248历史warning）、diff-check无诊断。源码已冻结；下一仅一次原1800camera，严格完整root失败不放宽，尚无新归属/0GC通过。PLANNED下文为历史。

PLANNED：尚未写代码或测量。准确矩阵见同名Task；本批是针对已有13事件的归属诊断，不是FPS优化/生产默认推广。root camera/observer完整严格门不变，H11/H07/Goal开放。
