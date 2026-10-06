# H-11第三子批：辅助绘制缓存封口

Task NTSD-OPTIMIZATION-BATCH03-AUX-CAPACITY-SEAL-20261006 / RUNTIME_PENDING（本子批聚焦通过）；Change NTSD-OPT-H11-AUX-CAPACITY-SEAL-003 / RUNTIME_PENDING。
用户批准下一批，沿用H-11方案第1/3条；前两批成果保留。
原状脚下marker/头顶health bar已预热，但热Build可PrepareCapacity增长，缺少sealed logical cap。
准确四生产路径和新test见Change，11个existing文档/代码备份见Operation before.json。

只改变表现缓存准入：按eligible Entity command数量分别计marker/bar，不把entity/command总数
或物理chunk/power-of-two余量当辅助容量。禁用/缺marker sprite/无有效health的零输出不误拒绝。
在缓存/mesh mutation前预检；生产在capture前及materialize后检查，整份拒绝，保留last-good。
Prepare/End复用已有生命周期；既有CPU lease禁止slot复用，不把它作为GPU完成证明。

test-first：logical17与physical32余量、各辅助物理极限、limit+1、0/disabled/filtered、
健康direct Build、两slotSeal/End、last-good有无/read lease与交错帧局部0B及旧几何回归。
原Unity2022.3.62f3/PID19040/TCP6402/Menu clean/nonPlay/URP；只具名EditMode与编译。
不切Scene/Play，不执行完整SelfCheck/真实EXE/Android/GPU/M0；不修改Q06 body或规则。
超限异常只用于直接误用，不算生产0GC正例；新增扫描CPU成本待测。
父H-11命令物化内其余缓存、完整capture/upload/record/submit0GC与预算/运行时仍开放。
回滚仅另批批准的本包逆向hunk，无资源或数据迁移，不还原HEAD覆盖dirty。

2026-10-06 原Editor实际编译19:25:09/12，新增15/15 GREEN、旧61/61回归，0 failed/skipped。
测试前生产四文件仍与before SHA相同；实际RED15项预期失败原件保留。
局部64次交错帧辅助双backend构建含预检/mesh上传0B，非完整central热路径或设备认证。
原Scene/全SelfCheck/正式EXE trace/Android/GPU/M0未执行，父项保持OPEN。
