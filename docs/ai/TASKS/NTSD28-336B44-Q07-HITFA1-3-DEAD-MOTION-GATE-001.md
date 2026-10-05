# NTSD28-336B44-Q07-HITFA1-3-DEAD-MOTION-GATE-001

PLANNED. 新版正式native_ai.cpp在common目标解析后current_hp<=0返回；Unity1缺自身HP门，3调用旧NoTargetDrift额外加2。World真实non-type0生产调用未见外层HP过滤。此前DEAD-NONCHAR-HITFA-MOTION-GATE-001仅修2/4/12/14，不重复已证职责。

准确scope仅两脚本：LF2Entity.cs RunHitFa1FrameLogic在现有目标解析/有效性后增加HP null/nonpositive return；RunHitFa3FrameLogic去非正HP分支的ApplyHitFa3NoTargetDrift调用、保留return。先native实际完整Core一步与原Unity RED证实再改；若无生产可观察首差则保持条件门。source/view、目标扫描顺序/保存/无目标、正HP行为、其它hitFa/结构生成/帧推进/physics都不改。不新增生命周期模块。

NTSD28Q07NonCharacterHitFa7EditorTests.cs仅新增IndexedDeadHitFa1Or3PreservesNativeMotion(902/0/1,206/54/3)两参数真实AI调用，HP0有活体异组有效目标与source/view双历史，守卫motion/Y/源坐标/HP/frame/target不变；IndexedDeadHitFa3MotionSurvivesOneFullDriverTick一个既有wrapper完整tick，用当前LoganRuntime＋项目mode Asset，注销875同slot换206/54/HP0，原schema/seed/Stage23/部分target初态如实保留，不宣称与native完整World同态。一次初始化sourceX500/Y-100/Z600/V0/counter0，target99/slot0/source1000/Y0/Z604/HP500；自然StepOneTick tick1，先CreateNew JSONL留原始值再assert。RED3项、GREEN同3＋既有局部共享路由SelfCheck共4，不全套/角色矩阵。

diagnostic-source-path: artifacts/diagnostics/NTSD28-336B44-Q07-HITFA1-3-DEAD-MOTION-GATE-20261005/native_dead_tracking_witness.cpp。新当前正式ObjectCatalog/BattleWorld/SimulationTickDriver实际一步，mode1/3两次单步同binary，整数前置如上、subjectHP0/sourceY-100，正常源码门守卫common_target/retained_cached=true/special_tail=false/unsupported0，保留出生和step后的motion/HP/帧/计数/源XYZ；具体帧counter先观测而非强造。C++17/O2无fastmath/28Core正式闭包核对及compiler/headers/DAT哈希，read-only authority，无改正式EXE/外部source/旧锁门构建器。结果不冒称GameSession/正式根EXE/自然战斗HP跨零/完整World/GPU或所有诊断为空。

正式Genma901静态OPoint902/0、Deidara58→206/50..54与资源字段可达，但本受控HP0/V0不是自然命中降血链。既有2/4/12/14零血证据复用；共同Fa5源整数除法已对齐，kind14 source flags已有共用写者，未形成新的首差；HF10实际正式资源存在只保留静态待核、不混入本包。

保护当前dirty、四Scene/config、DAT/图片/InputActions/非战斗/GAS/Gen/Plugins/相机完整背景/项目地图mode/1.5视觉/33ms/F5/pass/11阶段关闭。before逐SHA备份两脚本七文档；Operation索引先登记，所有新输出CreateNew。先native与RED，最小production，再生成编译0error/原MCP刷新一次并核DLL与clean/idle/nonPlay，同job超时只观察原handle；正常callback零计数才可声称。最终Validate-ChangeLedger/git diff --check/四保护/DAT两端/authority/精确delta审计，记录失败与限制。回滚另获批准，仅before逐hunk、不restore/reset/clean/删除。

226份同名Record/65未关闭，REUSE50/TRIGGER14/ONE1/P0=DEP=0；父Q/总目标开放，旧证据不重跑。


2026-10-05 correction: Genma901正式OPoint为902/action40，后经40→41→42→43→44→999进入0；原“OPoint902/0”为静态链简写错误。本包902/0始终是受控初态，不声称自然出生或自然降血链。
