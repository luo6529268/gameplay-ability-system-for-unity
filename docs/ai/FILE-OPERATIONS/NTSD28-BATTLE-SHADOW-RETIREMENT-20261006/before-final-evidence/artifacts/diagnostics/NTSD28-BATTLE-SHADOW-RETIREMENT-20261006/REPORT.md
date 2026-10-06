# 武器 / 投掷技能阴影残留：限定修复验收

当前状态：SCOPED_SHADOW_DISPLAY_FIX_VERIFIED（共用显示门、上述两例限定验收）。旧总目标收尾状态保持。

## 已观察问题和范围
用户例子是仙人鸣人的仙法螺旋手里剑与随机掉落的石头。本包处理共用阴影显示出口，不修改 DAT/图片/导入器/Scene/ProjectSettings/InputActions/声音/非战斗或模拟结果。没有删除、移动、Git 丢弃、第二 Editor 或 computer-use。

旧结束路径已同时回收本体和阴影，不能把它误记为遗漏 HideShadow。生产完整 Driver 的实际 stone break 在20 tick触发后产生20个碎片；240 tick后18个已下沉到画面下方，仍有阴影资格。原3种代表捕获实际1+2项；这证明下沉对象还在 World，并非删除对象重新出现。
仙人99/278释放段通过正式DAT的OPoint进入518/350，命中和爆炸后消耗：HP500->295，最后518/361是state3005/pic999控制对象，原有规则已经禁止其阴影。未找到这条已结束技能独立的逻辑槽泄漏；原 Scene还需验证显示出口，不能因用户报道就补写另一份角色专用规则。

## 共用修复
准确生产3文件：
- LF2ObjectRenderer.ShouldDrawShadowForBodyViewport：只对Y向下为正的非角色对象，按原有屏幕Z、DAT centery、1.5视觉尺寸、统一vertical projection和当前相机计算本体上边缘；本体整体已在当前相机下方时，停止其地面阴影。
- LF2Entity.UpdateShadow：legacy表现出口使用同一判定。
- BattlePresentationShadowBuild.BuildCommands：central表现出口使用同一判定。
字符/角色、正常地面物体、上方飞行物及原native影子开关/HitStop/pic999闪烁合同保留。相机判定只在main-thread表现执行，不加入managed worker状态/世界位置/碰撞/帧推进或有序关闭。无新runtime field/service/cache。

这是用户要求的Unity画面残留清理；正式336B44受控trace也看到999继续下沉，所以不称为已证native物理生命周期首差。逻辑实体仍按原规则运行，未凭空删除原生循环碎片或隐藏控制对象；本包不证明这些对象无限停留是否另有规则问题。

## 验证原件
- viewport-red-result-01.json：322c8f4d3b884fe78e019e1c731afe32，实际legacy下方本体保留阴影，预期RED。
- focused-green-result-01.json：69c037d8c9c94d1ea1800a41090aee06，5/5 PASS。
- focused-final-result-01.json：e06390911ed040898259f666f5280ed0，12/12 PASS，六非角色type0..6边界/角色保留及原四native shadow字段门。不执行全套9000+或每角色矩阵。
- 原Editor compile为0 CS error，Assets/Refresh均经原PID19040 MCP。
- original-scene-01.json：原saved NTSD_Battle (root11/clean)，normal Play预热，tick5暂停后受控99/278技能释放段和正式150石头生产spawn/break，随后300个实际生产Driver tick。没有实际物理组合键或等待随机掉落，本包不宣称这两项自然入口验收。
- 实际central提交存在，skillSpawned/hit/removed和rockRemoved均true；P2 HP500->295；碎片阴影峰值20，末尾fragment shadows0 / skill shadows0 / below-screen shadows0。17个逻辑碎片仍下沉，但不再产生孤立地面阴影。
- original-scene-active-fragments.png / original-scene-after-retirement.png：实际当前scene central mesh/material/texture，按原world camera范围GPU readback；底色为诊断纯色，不是完整Game View/背景或正式EXE像素对照，图像已人工检查。
- 既有11阶段有序关闭PASS：objects0/slots0/borrowers0；退出Play Scene clean，前后SHA253B2EBAD322AA6EC18488DFBBF555BB93AD897940A342E4130E77CE778F9010，Editor idle/non-Play。
- protected-final-01.json：5087 DAT/图/Scene/Shader/ProjectSettings文件SHA全部保持（包含前一轮红线Shader dirty bytes）；之前工作保护。
- validator-01.json：Tools/Validate-ChangeLedger.ps1 / PowerShell7 exit0，1294 Record，8 governed code files覆盖；git diff --check exit0。

## 正式 EXE 诊断边界 / 保留的失败
正式SHA336B44不变，未修改根EXE或当前C++。首轮3份LFR参与数/tick/HP期望夹具错误，首tickchecksum失败；第二轮3份已记录141个完成tick及142行trace，最终header失败。不得报整个LFR PASS、同世界/同输入/同seed/同tick完整parity或GPU相同。局部trace观察：999碎片持续下沉、518命中结束后消耗、150到期结束消耗；对应argv/fixture/report/stdout/stderr全部留存。
旧helper只允许冻结schema，初次新schema身份不匹配保留；重用neutral fixture仅bootstrap实际Logan catalog，未将其旧EXE标签当规则authority。
reported-cases-result-01.json为0-selected，不计成功；诊断List<ILF2Object>误用已改成已有List<LF2Entity>后实际2/2通过，无生产编译错误或扩大任务。

## 未验证 / 风险
不声称全部角色/模式/输入、正式EXE全画面、设备或整个Unity框架已经验证。本次没有为未复现的手里剑逻辑额外特判。以后若有新的具体残留，应根据对象/当前action/World slot/实际shadow command首差回访；不自动恢复旧Q campaign。
回滚仅本包准确3 production-before备份/自身新增文件或hunk，另行批准，不改前一轮红线修复。操作留痕见 docs/ai/FILE-OPERATIONS/NTSD28-BATTLE-SHADOW-RETIREMENT-20261006。

## 最终最窄 self-check / 收尾
- selfcheck-result-01.json：原 Editor job39e323194f7d4baabedc31882bfc4151 实际1/1 PASS，调用现有 BattleRuntimeSelfCheck.CheckEntityAndShadowRenderPositionFormula，不执行完整自检/9000+全套。
- final-editor-01.json / final-scene-01.json：原 Editor idle/non-Play，Battle loaded/root11/isDirty=false。最后仅诊断测试新增上述调用，三生产脚本未改；不重复已通过的12例/Scene。
- 本轮期间他人提交11069c9f已保留；本任务没有执行提交、回退或清理。收尾前当前10文件另存before-final-manifest及逐文件字节，保留历史计划/失败记录。
