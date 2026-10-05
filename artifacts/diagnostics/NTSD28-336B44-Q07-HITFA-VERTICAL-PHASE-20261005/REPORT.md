# 共用纵向追踪与物理阶段整数合同

当前状态：RUNTIME_PENDING / SCOPED_FULL_DRIVER_PASS；共用纵向/整数阶段必要修复及聚焦/完整tick已限定通过。下方PLANNED为事前快照，由本次实际结果覆盖。父Q07/Q09/Q12及总目标继续开放。

## 实际差异、修复与结果

正式native_ai.cpp543～559的2/4/12只改精确Y/Vy，未设置1.4高度上限或整数Y；Driver416→577→600，通常FrameMotion仅写motion，physics105～108先用旧整数Y决定摩擦、121～122积分精确Y、421尾同步整数。Unity共同尾部的两处多余写者已删除，正常physics同步/14排除/2选帧/目标HP/4捕获/XZ/速度/比较顺序及其它dirty保持，没有新增对象专属判断。

- RED jobfb98a1a4f9624902ad202d48b28f059e终态failed/completed6：完整Driver Vx期望9实8；907/219跨零AI后YInt期望-1实0；907/518正Y期望4.75/2.75实1.3999999761581421；14邻例未报失败。原件editor-red-observation-02.json，result=null，8873仅发现数，未当作全套执行数。
- GREEN job56676b15b8bc457b9f4605ec7aa8a8cd：6/6 Passed、0fail/0skip、41.4926562秒。五新参数＋既有SelfCheck局部共享路由入口；首个907例还执行实际CharacterMechanics，Y-2.25、Vx9及NativePreviousY104=-1符合。没有运行全SelfCheck/全EditMode。
- 完整Driver job2b416da5ae694a4fb6ecbc2f2e3df947：1/1 Passed、0fail/0skip、31.9143341秒；原件editor-full-driver-observation-01.json。仅907/type3/frame190/state3006/wait1/无Motion或OPoint/平台0/参考0/counter0、target99异队，sourceX400/Z600/Y-1.25/Vx9/Vy-2.8，target预Y100/Z640。唯一StepOneTick之后action190/Y-2.25/Vy-2/Vx9、NativePreviousY104=-1、sourceX409/Z约600.4、view两倍率、target0/count2符合。Z差40大于kind15半宽37，避开命中副作用；targetY100只用于AI前置，不要求整tick不落地。沿用旧schema/seed/诊断Stage23/部分target初始化、当前LoganRuntime和项目mode Asset；normal wrapper对象/槽/logic pool三计数0及publication恢复，不能扩大为自然原Scene证书。
- 两次生成命令均为dotnet build Assembly-CSharp-Editor.csproj --no-restore --nologo -v:q -clp:ErrorsOnly。RED exit0/301warnings/0errors/10.11秒，GREEN exit0/334warnings/0errors/18.47秒；各批现有MCP refresh一次，核新runtime/editor DLL、idle/nonPlay、原Battle clean才具名run_tests。没有第二Editor或额外矩阵。

独立测试审阅发现新完整方法机械复制115→190时把视野高1152误改1902，RED在Vx断言先失败；按原Task仅修回1152，原件保留。因此完整RED只裁决不依赖DepthScale的整数Y/摩擦，不能称前后完整初态逐项同值。五参数一直按1152，RED→GREEN同初态；907正Y控制/跨零和219受控正HP有区分力。旧14方法两个2/12整数控制期望改为初始整数保持，14五正例和生产证书不撤销。

## 身份、边界及后续

本包最终`Tools/Validate-ChangeLedger.ps1`实际PASS/exit0（1275份Record/25个dirty governed scripts），`git diff --check`exit0；[命令与返回摘录](GOVERNANCE-CHECK.md)。未扩大运行验证。

最终采集UTC2026-10-05T13:28:35.795124+00:00，final-authority-diff-and-editor-state.json保存两脚本精确before diff/新SHA，两个backup均有效；四保护文件和六DAT两端SHA保持，原Editor idle/nonPlay/无测试、原Battle clean/root11、Console0error。根正式EXE仍336B44，native_ai/tickdriver/physics源码SHA保持；外部build.ps1 SHA从C3151351…A00到BE7B002C…978，本包未写、执行者未知，观察副本及diff保留，仍含三个规则源码/playable入口。不能据此称整个source树/闭包未变或自动晋升候选。

没有修改DAT/图片、Scene/Input Actions、相机/背景/地图、1.5倍显示、项目模式、非战斗/GAS/Gen/Plugins、33ms/F5、pass及十一阶段关闭合同。正式Genma901/333→907/190只有静态资源可达和逐SHA生产者文件证据；219/0的已知自然出生HP0，这次只证明受控正HP分支，不能说自然正HP纵向已运行。自然原Scene、正式根同初态/GPU/设备键未验；本必要ONE完成回到REUSE，222份同名Record/61未关闭为REUSE47/TRIGGER14/P0=DEP=ONE=0，不重复已覆盖案例。

当前PLANNED。正式native_ai.cpp543～559只写精确Y/Vy，physics105～108先用旧整数Y判摩擦，421尾同步；Unity多写Y cap1.4及提前YInt。独立只读审阅确认907/190跨零初态理论Vx9/8首差，尚无本包RED或自然Play。准确权威/范围/未覆盖条件见[Task](../../../docs/ai/TASKS/NTSD28-336B44-Q07-HITFA-VERTICAL-PHASE-001.md)。
