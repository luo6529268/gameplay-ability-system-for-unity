# 正式hit_Fa14共用尾部：限定修复

当前状态：`RUNTIME_PENDING / SCOPED_FULL_DRIVER_PASS`。必要共用规则修复及聚焦/完整tick出口已通过，回到证据复用；本报告下方含尚未运行/COMPILE_PASS等文字均为先前快照，由本节实际终态覆盖，失败原件不删除。

## 本次实际终态及边界

本次`Tools/Validate-ChangeLedger.ps1`实际PASS/exit0（1274份Record/25个dirty governed scripts），`git diff --check`exit0；[原始返回摘录与命令](GOVERNANCE-CHECK.md)。没有因留痕或状态收尾扩大运行验证。

- 原Editor七例RED：job84cf269f86564004a1849f23cd300e74完成7，五个14规则首差、两个2/12控制通过；`editor-red-result.json`。
- 修复后原Editor GREEN：joba0b934cb6063428f83f34380f09df63d，8/8 Passed、0fail/0skip、51.9754726秒；`editor-green-result.json`。包含七正式内容例和既有SelfCheck局部共用路由入口，没有执行全SelfCheck/全套EditMode。
- 单个完整Driver：jobdc0fb1ad938f44cdabc8348f9847ef99，1/1 Passed、0fail/0skip、41.5442971秒；`editor-full-driver-type-guard-observation-01.json`。注册后实际type3，唯一完整tick得action115/state3003、Y-97.45/Vy2.8、sourceX409/Z600、viewX增量9×2048/1333、target0/count2，正常scope对象/槽/logic pool三项0并恢复发布。
- 两次完整方法历史失败：job2e6e9dc23ce542cea799633b9c6aabee及job4ad8b188865b4d86aed407c55737e4c6，都在callback前误把type_sub当type而失败（raw0/catalog207）。type_sub其实是OID别名，实际type从World RuntimeDataCatalog中的wrapper OID定义读取；新guard验证catalog Type3/OID别名207及注册后共同解析器3。`editor-full-driver-first-failure.json`与`editor-full-driver-repair-observation-01.json`保留。旧518完整tick仍读取catalog type3，限定证书有效，无须重跑。
- 最新生成命令 `dotnet build Assembly-CSharp-Editor.csproj --no-restore --nologo -v:q -clp:ErrorsOnly`，exit0/301warnings/0errors/9.09秒，`editor-full-driver-type-guard-build.log`。同原Editor MCP refresh_unity成功，核新Editor DLL/idle/nonPlay/Scene clean后才用testNames启动一个方法；`editor-full-driver-type-guard-launch.json`。此前RED/GREEN构建及刷新超时/reload观察原件保留，没有第二Editor或重复并发作业。
- 最终采集UTC2026-10-05T13:06:03.485146+00:00，`final-authority-diff-and-editor-state.json`保存三脚本相对before实际diff及新SHA；三个backup逐SHA有效，四保护文件与五DAT两端保持。原Editor idle/nonPlay/无测试、原Battle clean/root11、Console0error。

正式根EXE仍336B44，native_ai.cpp、simulation_tick_driver.cpp、physics_integrator.cpp三文件SHA保持；但外部build.ps1在13:05:59Z改为C3151351D683CE557B2E47C89742E62D69B51D68B2DA006D2D36CDF33AA68A00，本任务未写该路径，执行者未知。观察副本`observed-authority-build-script.ps1/json`记录当前仍将三个规则文件列入coreSources并用于playable；不能据此称全source树/构建闭包未变，也不自动晋升新候选。这不改变本包引用的正式EXE与三份规则源码身份。

没有改DAT/图片、Scene、1.5倍图片尺寸、相机/背景/地图、非战斗/GAS、pass/33ms或十一阶段关闭合同。正式Sai334→207/115只是源码/内容可达证明；尚未运行自然Sai原Scene技能、根正式EXE同初态/GPU/物理设备按键。父Q07/Q09/Q12和总目标仍开放。本必要ONE完成，221份同名Record中60份未关闭的调度为REUSE46/TRIGGER14/P0=DEP=ONE=0，不是60个必跑任务。

## 实际首差与修改范围

正式native_ai.cpp543～559明确排除14的纵向跟随，562～591只有2选帧；14保留Y/Vy/YInt/action并仅执行已声明X/Z追踪、Vz±1.5及朝向。该文件在playable build.ps1:68闭包，根EXE仍336B44；Sai334 OPoint直接生产207/action115，115→116→117→118→115均为正式14路径。正式/镜像ink DAT逐SHA同BFD40B2B4BB3FC3CD173113535321FFC128F8C642AD584D498FB56D0EC5713D9。

原Editor job84cf269f86564004a1849f23cd300e74 completed7，五个14正例实际失败、518/1与907/190两个控制通过；editor-red-result.json保留完整原件，result=null，实际计数由completed7/五failures_so_far取得，8866只是发现数。两视野Y-100.25误为-99.25；Y-40.25例精确Y回到初态但Vy-2.8误衰减到-2；正Y3.75误钳至float1.3999999761581421；Vx9/115误写65。不是夹具异常或自然Play证书。

生产只以hitFa共同门让14跳过这些多余纵向写入并删除旧±50动作尾部；保留2/4/12写序、2选帧、目标/HP/回收、source XZ共用入口、速度clamp和朝向。SelfCheck只纠正14三CLR壳的人工action0与Y-10.25/Vy2.8/YInt-10保持期望；其3/4相邻路由未动。既有测试档七正式内容例及一个预声明完整Driver方法；三脚本独立静态审阅确认精确before增量与四保护SHA保持，未执行额外测试。没有新增模块或改变关闭阶段。

## 构建及原Editor进度

生成命令均为dotnet build Assembly-CSharp-Editor.csproj --no-restore --nologo -v:q -clp:ErrorsOnly。测试先行exit0/301warnings/0errors/102.50秒（editor-red-build.log）；生产与必要完整方法exit0/334warnings/0errors/24.56秒（editor-green-build.log）。首次refresh观察超时，实际编译154.79秒后reload期间6401短时拒绝连接，再自行恢复；只提交一次，不开新Editor、不重发。第二次refresh回执成功，已确认runtime/editor新程序集、Editor idle/nonPlay/原Battle clean/root11，才启动唯一GREEN八项job a0b934cb6063428f83f34380f09df63d：七正式例＋既有private SelfCheck共用路由入口。终态待追加，完整Driver尚未启动，不把生成0错当运行证据。

## 必要完整Driver边界

复用旧wrapper schema/seed/诊断Stage23及部分target初态，读取当前LoganRuntime+项目mode Asset。注销direct-new旧875、确认slot/World解绑，再将新正式207/115注册同slot1并更新Roster新StableId与CharacterId，active count2维持。新frame counter0、三Motion为0、无OPoint/type3/state3003/wait1；sourceX400/Z600、Y-100.25、Vx9/Vy2.8，target同X/源Z604/Y0。唯一完整tick的正式预期为action115/state3003、Y-97.45/Vy2.8、sourceX409/Z600及统一view比例；115的hit_a2可在帧尾减HP，未错误要求整tickHP不变。错路65本身三轴550会清零，正路115不会掩盖差异，因此同时检查帧、状态与速度/坐标。

scope正常回调结束后检查对象/slot/logic pool三项0并恢复发布；setup异常或callback失败不能据Dispose推断正常后置检查通过。未新增scenario/trace/request、未改DAT/图片/Scene/非战斗/1.5倍尺寸。原Scene自然Sai技能、正式根EXE同初态/GPU/设备输入等仍未知，父Q07/Q09/Q12与总目标开放；本例通过后回到证据复用，不继续扩角色矩阵。

2026-10-05；NTSD28-336B44-Q07-HITFA14-COMMON-TAIL-001 / COMPILE_PASS。正式consumer14不写纵向或action±50，Unity同一尾部确有静态差异；尚无本轮运行结果。Sai334→207/115正式DAT可达，但不据此称已运行自然技能。三脚本pre-change记录/逐SHA备份已建立；七例RED→必要GREEN及一个Driver出口由[Task](../../../docs/ai/TASKS/NTSD28-336B44-Q07-HITFA14-COMMON-TAIL-001.md)声明。上一包X/Z与其他已证场景保持REUSE。

实际生成构建dotnet build Assembly-CSharp-Editor.csproj --no-restore --nologo -v:q -clp:ErrorsOnly退出0、301warnings、0errors、102.50秒，editor-red-build.log。原Editor MCP refresh请求唯一提交后观察30秒超时，随后editor-red-readiness-01明确同Editor正在compiling、Editor程序集仍旧；没有重发或开跑测试，原Battle仍clean/nonPlay/root11。现有编译后只运行预声明七例。

