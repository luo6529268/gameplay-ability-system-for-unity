# 第28批相机/观察器分配事件归属

## 最新终态：归属字段通过，完整0GC失败

PARTIAL / PROVENANCE_PASS / ZERO_GC_FAIL；camera-01 DONE。一次1800frames、tick8→1439，前后1MiB正对照各1event/空0通过，记录scope均有效。逐帧camera2、BeginObserver0、EndObserver13与窗口总计一致，未排除早期帧。observer13在ordinal0/UnityFrame3234/tick9/slot1；camera各1在ordinal2/UnityFrame3236/tick11/slot1和ordinal7/UnityFrame3241/tick16/slot0。只帧/阶段相关性，调用点未知；不能直接当0.7FPS原因或允许的冷启动例外。完整原件camera-01/production-window-01.json，摘要provenance-summary-01.json。

Foot/Health下限各2、两slot、11484 CPU DrawMesh录制=执行、growth0/CPU读lease0；不证明GPU batch/完成。11阶段objects/slots/borrowers0，Battle clean/SHA同/原Menu恢复。首次恢复误用不存在的Menu.unity路径被API拒绝、未改Scene；随后按重扫的NTSD_Menu.unity恢复，无保存或文件修改。187非写域/10当前副本SHA与HEAD8107196b保持。第27批四校准/两CPU桥不重跑，没有新NUnit/1000AI/GPU测试。

H11 EVIDENCE_PENDING、H07 PERFORMANCE_FAIL，首阶段4of6/新批7/Goal active；不继续同构GC观察。下一性能主线是现有collector实际完整tick候选准入，当前热点426.55ms/92.75%，不自动切默认/新索引/专项门。下面运行中内容保留为历史。

IN_PROGRESS / COMPILE_PASS / CAMERA_RUNNING。唯一probe值字段/sum审计已写，原Editor重载完成并在Menu clean/idle/无测试后只一次加载savedBattle、调用28菜单；camera-01 target1800正在启动。27有效校准和两个CPU桥复用，既有1800camera2/observer13event失败保留；原完整范围不缩小、不跳首camera，未有新的归属/0GC或FPS通过。没有调用栈就只声明帧/阶段相关性，不直接归因生产或0.7FPS。

Tools/Validate-ChangeLedger.ps1实际PASS1324Records/9 governed C#diff，4277历史warning保留。10当前脏字节副本/187非写域保护、HEAD已先存before.json。原Editor运行中不再编辑C#/刷新/另启测试或第二实例；最终Scene/有序关闭/保护/原Menu恢复尚待。
