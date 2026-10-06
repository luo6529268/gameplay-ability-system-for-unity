# 第二批：H-11中央表现容量封口

Task：NTSD-OPTIMIZATION-BATCH02-CAPACITY-SEAL-20261006 / RUNTIME_PENDING（本子批聚焦通过）。
Change：[NTSD-OPT-H11-CAPACITY-SEAL-002](../CHANGE-RECORDS/NTSD-OPT-H11-CAPACITY-SEAL-002.md)。
需求：用户批准执行下一批，继续H-11第3条；上一批预热与所有dirty均保留。
四生产文件与一个新测试，准确路径见Change；文档/备份见Operation/before.json。
仅既有slot的entity count、hit record count、command count、motion count/handle槽位硬限；
保留原CentralOnly失败时last-good或无像素，不切Legacy、不发布部分新帧。
正常预热容量沿用Driver现有runtime entityCapacity/CalculateMaximumCommandCapacity，
不用4096 chunk尺寸代替slot逻辑上限，不冻结新设备/资源预算。

设计：每slot冻结copy前预检；mesh在变更任何geometry前拒绝超command；
DisplayMotion在generation/数组写入前检查两代handle的slot；生产不用异常作为正常超限路径。
既有PrepareBattleCapacity封口，既有EndBattleCapacitySeal解除，禁止封口期间再PrepareCapacity。
检查每slot可复用后才准备，不能覆盖read lease；不把CPU lease或封口当作GPU完成证明。
owned缓存沿用central静态/slot/backend/DisplayMotion；无新队列/worker，11阶段不变。

test-first：exact/limit+1/0、数组实际大于逻辑限值、两代motion最大槽位、alpha1、
拒绝前后frame/storage/mutation version不变、seal后禁止扩容、unseal重预热、
生产超限last-good/无半帧、CPU lease保持、64次局部0B。
运行原Editor的具名EditMode和上一批/mesh/LatestFrame聚焦回归；不另开Editor/切Scene/进入Play。
后续完整MaterializeCommands内部缓存、native/GPU结构、高帧率全路径0GC/真实Battle/Android未闭合，
子批PASS不关闭父H-11；Q06 body不读写，GPU/EXT-1/MONO与资源格式专项保持原门。
回滚：另批批准后以本包before-backups为本轮原状，只逆向本批hunk，不恢复HEAD覆盖第一批。

2026-10-06 本子批限定验收：新16项先RED后GREEN；旧预热7、mesh/bounds8、
LatestFrame13、两项具名motion回归共30/30通过。原Editor实际编译、Menu clean/nonPlay，
error CS查询0；局部64次varying-alpha DisplayMotion.Prepare含预检0B。
父项完整0GC/真实Battle退出重进/Android仍未验收，不晋升VERIFIED。
