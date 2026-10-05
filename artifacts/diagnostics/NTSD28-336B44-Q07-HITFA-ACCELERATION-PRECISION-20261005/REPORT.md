# 共用追踪加速精度与严格动作阈值

状态：RUNTIME_PENDING / SCOPED_TEN_TICK_DRIVER_PASS。当前必要ONE完成，按REUSE复用；父Q07/Q09/Q12及总目标仍开放。

## 实际差异与修复

正式336B44对应native_ai.cpp493～585的共用2/4/12/14加速使用double0.7/0.4，behavior2在clamp之后以严格abs(Vx)>7选动作3。Unity double载体先从0.7f/0.4f取得float舍入值。实际首差并非纯推断：修前完整Driver tick1 motionX0.699999988079071，修后0.7；第十tick动作从2变3，速度由6.9999998807907104变7.000000000000001。完整前后同夹具各11行，paired-unity-first-difference.json记录首个速度差与首个动作差。

生产本包before增量只一个hunk：四处±0.7f/±0.4f去f，并加一行合同注释。没有修改7/14阈值（整数float可精确表示）、严格比较顺序、死区/clamp、目标HP、4回收、14纵向排除、2选帧、源坐标入口或任何其它分支。所有前序dirty保持。无角色特判、无DAT值改动。

## 实际运行结果

- 原Editor RED jobdb0df1b9a4a64fc6a8414fc36c1f50e7：终态failed/completed5，五项均出现预期首差。完整十tick末帧期望3/4实际2；左右十次AI末帧期望3实际1；±Z单步实际±0.40000000596046448而非double±0.4。editor-red-observation-02.json原件result=null；8878仅发现数，未执行全套。
- 原Editor GREEN job83829345b2c84ee79aab5a96cb42e6ce：6/6 Passed、0failed/0skipped、56.3994129秒，原件editor-green-observation-01.json。四参数＋一个完整十tick＋既有SelfCheck局部共享路由入口，合并一次运行；没有全SelfCheck、全EditMode或重复完整方法。
- 原Unity完整十tickGREEN末帧3、Vx7.000000000000001、Y/YInt-90、Vy/Vz0、previousY-91、target0、count2。末尾严格速度、sourceX438.5/viewX438.5×2048/1333、sourceZ600/Vz0断言均执行通过。逐tickJSON不含HP和X，不能宣称这些字段逐tickraw对照；修前在第十tick帧断言失败，尾部X检查尚未执行。
- 新native_ai_acceleration_witness.cpp用正式catalog99/type0、518/type3/frame1、world.spawn_at与NativeAi28::step_non_character_hit_fa真实消费者，未复制加速算法。28个当前core源码构建exit0，native-ai-build.log无诊断；独立运行exit0/11行、stderr空，第九次Vx6.300000000000001/action1、第十次7.000000000000001/action3。精确compiler参数/所有28core及core头/runner/EXE/catalog/buildscript身份在source-input-manifest.json，最终所有inputs保持。仅十次AI源码消费者，未执行physics/frame advance；seed42与Unity旧scenario seed不同，不能冒充正式EXE或native完整十tick逐字段一致。
- 两次生成命令均为dotnet build Assembly-CSharp-Editor.csproj --no-restore --nologo -v:q -clp:ErrorsOnly。RED exit0/301warnings/0errors/13.22秒；GREEN exit0/334warnings/0errors/12.31秒。每批原MCP refresh一次，核新runtime/editor DLL、非Play/idle/干净原Scene后才具名测试；未开第二Editor。

## 边界、审阅及文件治理

两既有脚本（生产LF2Entity、既有NonCharacterHitFa7测试档）及一个新artifact诊断cpp已在Task/Record事前声明；独立只读审阅无阻断，确认共同四处精度/阈值不变、DAT和既有dirty保留。新native build不调用旧B1E13锁门脚本、不修改外部源码、也不晋升候选EXE。

治理预检查exit1是Record把artifact cpp错误列为受治理code-path；当前validator只治理Assets/NTSD/Scripts和Tools。更正为diagnostic-source-path并保留Task/Record/Operation/哈希，未改validator或移动删除源。首轮Ledger注册也因有多个分隔行触发唯一性断言，后来仅补当前表首行；两个失败都保留且发生于非战斗记录环节。最终validator及diff-check实际结果另回链GOVERNANCE-CHECK.md。

最终采集UTC2026-10-05T13:49:47.479200+00:00：两个before副本逐SHA有效，四保护文件及六DAT正式/Unity两端SHA保持，原Battle clean/root11，Editor idle/nonPlay/无活动测试，Console0error。正式根EXE仍336B44，native_ai/driver/physics三个规则文件保持。外部build.ps1由BE7B002C…978变5A06544B…EDE，本任务未写、执行者未知；有前观察字节匹配before及新观察副本/diff，C++source-input构建时快照与最终相同。不能称整个外部source树/闭包不变。

完整Driver仍沿用旧schema/seed/诊断Stage23及部分target初态，实际读取当前正式LoganRuntime与项目mode Asset；当前source518资源帧1～4可达但自然出生的零速度前置未证。没有原Scene自然技能、正式根EXE同初态十tick/GPU/设备键验收；不从受控样本推出整阶段完成。没有改变DAT/图片/Scene/Input Actions/相机背景/地图/项目mode/1.5倍显示/GAS/Gen/Plugins/非战斗、33ms/F5、pass或十一阶段关闭，未新增生产module/worker/queue/pool。

当前223份同名范围Record/62未关闭，REUSE48/TRIGGER14/P0=DEP=ONE=0，不是62个必跑任务；未知条件只由真实非例外首差或相关改动触发，不为抬状态重复。

最终治理检查实际PASS：validator exit0（1276份Record、25个dirty governed scripts）、git diff --check exit0，见[GOVERNANCE-CHECK](GOVERNANCE-CHECK.md)。预检查失败已如实更正，不以PASS删除历史。
