# Q05 ReleaseTick载体清理限定交付

状态 FOCUSED_TEST_PASS / SCOPED_PLAY_PASS / JOINT_SCHEMA_PENDING。总目标和Q05仍ACTIVE/FULL_ALIGNMENT_INCOMPLETE。

准确14脚本（8生产/6测试与诊断）。删除NTSDEntityRuntime.ReleaseTick、copy/reset、ECS fingerprint、checksum/parity字段。删除WeaponBase/ReleaseFlowResolver/ClearLinks的三个无效stampReleaseTick参数与全部caller。真实释放关系/action/motion/RNG/HP/guard和生命周期不变；四生产文件按移除参数后的规范文本与改前全文一致，见release-rule-stability.json。不会按名字删除真正每步RunReleaseTick或probe观测releaseTick。

旧SelfCheck/ReleaseTick/WPoint/Goal20 sentinel与报告已迁移，保留有效关系、HP/随机数/动作/速度断言；复用reset改验LinkState/HolderStableId/ThrowFrameGuard。历史报告未覆盖。数据格式处于获准同Q05未发布中间窗口，版本仍12/20/23/1/1，联合版本/identity/双OPoint guard/旧版本拒绝/replay后继不得跳过。

## 实测与命令

- 桥接目标本项目现有Unity2022.3.62f3，get_editor_state/refresh_unity/run_tests/get_test_job/read_console；没有computer-use或第二Editor。源读取/规则依据使用已有VERIFIED原版ReleaseTick owner/producer退休记录，本轮不冒称新执行完整native-world parity。
- RED job3f8b26d30aa643f7b30971f3b20b37a0：7/7 FAIL，见red-results.xml。代码后jobcd66a54a8e3740cba2e4487bc26ed1f4：267/267 PASS、0skipped，见green-results.xml/green-job.json，准确14测试类见test-selection.json。最终SelfCheck断言修正仅改旧测试预期，未重复无变化的267项。
- 完整SelfCheck请求文件机制：首轮失败于遗漏lowercase JSON releaseTick:99，SelfCheck-first-FAIL.result保留。仅同Record SelfCheck改为字段不存在，全部相邻block/Unk/owner/relation断言保留。第二请求2026-09-13T10:04:44.413622Z，结果10:05:23Z PASS，resultmtime晚于请求。自检会产生预期故障注入日志，不能称Console全无error；最终error CS过滤0。
- 通过manage_editor play与既有Goal20_R4_CurrentPlay.request=GREEN做两次真实NTSD_Battle当前数据probe。Gaara16/254→120、RockLee7/255→120、Sasori51/396,399→213共四例，每例seed424242、worldtick5；两份新结果PASS，before/afterObjects4→4，自动退出Play。after不存在releaseTickBefore/After，不用默认值伪造；除明确删除的两个诊断字段以外，每个row全部有效输出逐项相等，见before-play.json/after-play.json/play-comparison.json。包含真实runtime配置、AttachOpointHeldObject与HeldObjectProcessAll，但为paused-world注入setup，非物理按键/自然整技能/整场native checksum验收。
- Scene最终isDirtyfalse/root14，SHA a96e11064f1bd054d9d5547fe8f02c702d971b3972d55754886d75b2dce9d28f不变。保护基线3059：2977相同/64已存在或本包声明变化/18既有Foot缺失，较上包增加6路径全部在Record，无新增缺失。
- git diff --check通过；Tools/Validate-ChangeLedger.ps1 -RepositoryRoot $PWD.Path通过493 Records/14 governed scripts。当前HEAD cf35dbf0已包含前包，所以本次diff脚本数不与旧130累计数混淆。两份未跟踪authority-content原版捕获保留；未提交/推送。最终receipt在ledger-final.txt。

## 下一入口

NTSD28-Q05-WEAPONSTATE-CARRIER-RETIREMENT-001，先区分无行为WeaponState载体与真正frame.state及GetResolvedWeaponStateForExternalUse；再HolderCopy，再Q05 identity/双OPoint guard/统一13/21/24/2/2/旧版本拒绝与回放/Play。R13字段删除子条件追加PARTIAL_RETURN，完整R13/R15/B1–B12不提前关闭。Unity/GAS/非战斗、33ms/3ms、十一阶段、stage.dat USER_HOLD和例外保持，正式DAT/图片未整体迁移。
