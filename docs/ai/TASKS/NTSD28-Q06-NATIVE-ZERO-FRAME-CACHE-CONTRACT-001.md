> 当前父Task仍IN_PROGRESS / ACCESSOR_RESOURCE_SUBSTEP_VERIFIED；下一NTSD28-Q06-NATIVE-FRAME-RUNTIME-READER-MIGRATION-001，全部reader未迁移。原388为改前检索位置，最新406候选见reader-inventory-after-accessor.json。

> 当前IN_PROGRESS / ACCESSOR_CONTRACT_FROZEN；子实施NTSD28-Q06-NATIVE-FRAME-ACCESSOR-RESOURCE-ADMISSION-001已启动。全reader清单和合同在同ID artifacts/CONTRACT.md，不宣称全部映射或迁移完成。

# 当前native零帧缓存与reader合同

READY_READONLY，高优先级独立差异。source dat_document.cpp:91-116确认DatDocument.frame(id)先取声明帧，否则0..998返回id正确的共享零帧，其他id为null；Unity Animation/Character/LF2FrameCache.cs上界857，GetFrameDataById返回无id区分的EmptyFrame，HasFrame只判声明帧。Q06 HP/MP CanEnterNativeResource用了HasFrame，已有source invalid向量只设置9999，而Unity资格测试用未声明7；旧PASS不能证明这一区间资格一致。

完成当前独立出生资源后先本只读合同：native declared_frame/frame全部consumer、Unity HasFrame/GetFrameDataById/上界和frame struct默认值、parser实际语义/OPoint+CPoint/transition/动作latch/快照与hash边界。用原frame accessor及资源成员函数对0、7、856、857、998、999、-1及声明域外id（若source可声明）产生实际见证，再列准确Task/Change。区分文本声明查询和runtime零帧查询，不能全局替换HasFrame导致语义相反调用者变坏。

回访HP/MP pre/post资格、birth action准入与R05/R07/R09/R15；对既有记录追加限定纠正，保留有效声明帧与越界9999历史证据。不得自动改schema或parser数据内容，先确认所有reader及copy/snapshot身份影响。禁止computer-use、非战斗/框架/Scene/资源改动，用户例外保持。此问题不阻塞明确声明帧的出生HP/MP事务见证和实施；post-display完整实现前必须闭合。
