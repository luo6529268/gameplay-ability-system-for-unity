# NTSD28-336B44-Q07-HITFA10-COMMON-TARGET-001

PLANNED，唯一正式EXE336B44及paired playable当前Core；native_ai.cpp common_only_object_hit_fa默认正值含10，common目标扫描后366-371非recovered motion直接返回。Unity RunCurrentDatFrameLogicBeforeAdvance的10分支跳过目标解析并额外±1.1、clamp30、Y cap3/facing/YInt。正式902/4/5、429、416、526与228实际索引可达；不扩大为其它稀有未可达分支。

准确code-path三脚本：LF2Entity.cs仅RunCurrentDatFrameLogicBeforeAdvance hitFa10分支，复用ResolveFrameLogicTargetByHitFa(10)，若解析后ObjectAiTargetSlot3F8==-1且Health存在则HP0；随后返回，不额外运动。不能只删运动漏common目标副作用，不改共用resolver其余分支与缓存/源比例排序合同。

NTSD28Q07NonCharacterHitFa7EditorTests.cs仅新增两方法：IndexedHitFa10CommonTargetHasNoMotion三参数(缓存目标0/重扫-1有活体/重扫-1仅死目标)，正式902/frame4，直接RunFrameLogicBeforeAdvance，HP500/Vx1/Y9/朝向left，重扫caseVx-1，已初始化source/view位置；先对target与HP判别再motion/Y/dir/YInt，真实类型/资源守卫。完整方法IndexedHitFa10AfterDeadFrameTransitionSurvivesTwoDriverTicks：复用旧scenario wrapper当前LoganRuntime+项目mode，注销875/同slot注册902/0/HP0，source(500,-100,600)/V0、活体target99(source1000,0,604)，正常两tick从hitFa1自然串行到frame4/hitFa10，再进frame5。JSONL在断言前CreateNew输出13共享字段/附加viewX/count，第一tickframe4/counter1、第二frame5/counter0须先由当前完整Core实测确认，不每tick重置。输入仍旧schema/seed/Stage23/部分target初态，不声明完整World/正式EXE同态。

BattleRuntimeSelfCheck.cs仅CheckCurrentDatFrameLogicSharedRouting，把三个现有非角色CLR/当前DAT类型夹具的旧10加速/Ycap/方向断言改为native仅common目标、无目标HP0、Vx±1/Y9/方向保持；其SimTU单次物理应按实际生产结果及native既有physics合同验证，不盲写通过；其它representative3/4/14不改。只同4个新增RED，修后三案例+完整两tick+已有局部自检共5GREEN，不全套/角色矩阵。

diagnostic-source-path: artifacts/diagnostics/NTSD28-336B44-Q07-HITFA10-COMMON-TARGET-20261005/native_hitfa10_driver_witness.cpp。当前完整Core Driver正常两step，902/0 HP0先1后10，实际AI适用/common/cache retained/no special/unsupported0；读取正式catalog/frames无OPoint/Motion。当前28Core闭包/C++17 O2无fastmath/所有输入前后SHA。正式EXE、外部source不写、不晋升。

测试先行，native两tick与原Editor RED首差后才改production和旧selfcheck。保护dirty、DAT/图片/Scene/InputActions/GAS/非战斗/地图模式/33ms/F5/pass/有序关闭；不新增生命周期模块。三脚本七文档before逐SHA十备份；先Operation/索引/Record/Ledger/STATE/handoff注册再任何脚本。生成编译与MCPRefresh一次，对应DLL新鲜/clean/idle/nonPlay，超时续查同job。最终逐hunk/sourceview/保护与DAT两端/关闭计数/独立只读审阅/ValidateChangeLedger/gitdiffcheck留证。回滚另获批，仅before精确hunk，无restore/reset/clean/删除。

227份同名Record/66未关闭：REUSE51/TRIGGER14/ONE1/P0=DEP=0；父Q/总目标开放。自然birth/自然降血/Host/完整World/正式根EXE/GPU等只真实首差或相关改动回访。


2026-10-05 独立只读审阅：正式common10目标合同与旧运动确有差异，902两tick合适。现有resolver排除组仍读SpawnerEntityIndex而非独立2F8 carrier，缓存检查额外要求type0；本包普通type0目标/不区分排除组只裁决本候选，不冒称全部目标系统一致，不顺手扩修共有resolver。将两个既有合同候选保留在当前总表/条件回访边界，后续实际可达首差另开独立包。
