# behavior1 共用追踪消费者收尾

状态：`RUNTIME_PENDING / SCOPED_FULL_DRIVER_PASS`。本次必要 ONE 已完成，后续按 REUSE 复用；Q07/Q09/Q12 及总目标仍开放。

## 实际规则差异与改动

正式336B44对应 playable live path 的 `native_ai.cpp` behavior1 使用 double 0.85/0.3/1.2、Vy 除以1.4，两个独立高度比较按顺序消费写后高度；没有 Y 上限1、没有在 AI 阶段同步整数Y。Unity 既有单精度步长和额外写者会改变结果。

本包生产仅修改 `LF2Entity.RunHitFa1FrameLogic`：六处步长去 float 后缀、Vy恢复除法、移除 cap1 与提前 YInt。目标解析、HP、统一源X/Z、±7死区、独立比较顺序、非角色目标正Y加1、夹紧、朝向和其他分支保持。相对 before 的其他生产代码完全相同；未改 DAT 或非战斗功能。具体 SHA/diff 见 `written-diff.json`。

## 实际验证

- 原 Editor RED job `6ee8f4ec1cd44ecb9b81cd9ef0c3bf1e` 终态 failed/completed6。五参数及一完整 tick 均在预期高度断言首差失败：边界 -11.2→-11.199999999999999（预期-10）；跨零 -0.04999995231628418（预期-.05）；正Y被压到1（预期4.95）；左右增量例 -28.799999952316284；完整 tick -2.0499999523162842（预期-2.05）。后续摩擦断言在 RED 首个失败后未执行，不能称修前完整 tick Vx8 已实测。首轮观察超时不是终态，随后仅轮询同一个 job；8884是发现数，实际没有执行全套。
- 原 Editor GREEN job `d4c4cc76de7342d7a877c919abd3baae` 终态 succeeded：**7/7 Passed、0failed、0skipped，63.3800063秒**。同五参数＋同一个完整 tick＋既有局部共享路由 SelfCheck，一次具名运行；未运行全量 SelfCheck、全 EditMode 或角色矩阵。
- 完整 Unity Driver 例902/type3/frame0/state3000：一 tick 后 Y=-2.05、Vy=-2、Vx9、Vz.3、previousY=-1、sourceX409/sourceZ600.3，实际X/Z增量分别按2048/1333及1152/730投影，target0、对象2。这些断言实际通过；本方法没有逐字段原始 trace 或正式根 EXE 同初态证明，不能包装成完整双端 trace 比较。
- 两次生成命令均为 `dotnet build Assembly-CSharp-Editor.csproj --no-restore --nologo -v:q -clp:ErrorsOnly`。RED exit0/301warnings/0errors/14.26s；GREEN exit0/334warnings/0errors/18.49s。原MCP各批仅一次refresh，核对应脚本的程序集新鲜状态再具名测试，使用原项目Editor。
- native v2调用当前正式源码 `NativeAi28::step_non_character_hit_fa` 一次，三受控模式均exit0/初态守卫通过：boundary Y-11.2→-10、integerY仍-11；cross Y-1.25→-.050000000000000044、integerY仍-1、Vx9/Vy-2/Vz.3；positive Y3.75→4.95、integerY仍3。这是 AI 消费者源码证据，未执行 native 完整 Driver 或正式 EXE。命令、编译器和66输入 SHA见 `source-input-manifest-v2.json`；28个core源码与当前正式build声明集合相同，C++17/O2、无fast-math，构建exit0且无诊断。

## 诊断更正与治理边界

native v1首次exit0，但 `spawn_at` 将声明分数Y从整数重建，call0实际-11/-1/3，初态无效；全部v1源码before副本、binary、输出仍保留，不能作原案例证书。预登记后仅在诊断spawn后恢复preciseY并守卫，v2使用新文件名及CreateNew；未改正式出生规则。

一轮production apply_patch因旧注释编码精确匹配失败而未写，读取实际字节后同范围成功。GREEN最初本地启动守卫误将两程序集都与最新生产脚本比较，失败发生于run_tests之前；改为各自脚本/程序集新鲜度，未重复刷新或启动测试。JSON默认GBK读取失败、内容检查误把byte同版与未修改合并，以及闭包检查错误选择authority数组末项／漏source路径，都是只读诊断问题；失败原件保留，`content-and-source-closure-check-v2.json`为守卫正确的后继。只有w/e.dat存在事前换行差异，文本相同，本包两端均未改。未因这些诊断问题修改生产或测试期望。

独立只读审阅无阻断，见 `INDEPENDENT-REVIEW.md`。测试try内异常有finally注销，setup异常全覆盖未证；完整wrapper正常返回后的零残留断言已随GREEN执行，失败callback后的后置断言不作证明。

四保护文件、八DAT两端及正式根EXE/native_ai/driver/physics在核对时保持；before九个备份逐SHA有效。原Battle Scene clean/root11，原Editor idle/nonPlay/无活动测试，Console0error，见 `editor-final-state.json`。外部build.ps1从5A06544B…EDE变D4B4BAE8…63F，执行者未知、root未写；旧观察字节匹配before，新观察和diff已保存。v2构建66输入在核对时均与构建快照相同，不宣称整个外部source树未变化或晋升候选EXE。

正式Genma901/282→902/40→41→42→43→44→0是静态资源可达证据，未证明本例分数Y/Vx9/Vy-2.8自然出生。完整Unity例复用旧schema/seed/Stage23及部分target初态，读取当前LoganRuntime＋项目mode Asset；没有自然原Scene技能、真人设备键、GPU或正式根同初态验收。没有修改DAT/图片/Scene/Input Actions/相机背景/地图/1.5倍显示/项目mode/GAS/Gen/Plugins/非战斗、33ms/F5、pass或十一阶段关闭；没有新增生产module/queue/worker/pool。

总表及Task/Record/Ledger/STATE/handoff/Operation同步后，最终治理命令与结果回链 `GOVERNANCE-CHECK.md`。未知条件仍仅按实际非例外首差或相关改动回访。

最终核对更正：final-scope-and-editor-state.json在原Scene clean/idle/nonPlay/Console0error及保护/DAT/EXE/三规则保持之后，仅对“66构建输入全未变”的宽断言失败。外部build.ps1又由D4B4BAE8…63F变D46CD858…E0D；执行者未知，本任务未写。最终65/66输入保持，当前声明28core集合仍与实际命令相同，源码消费者和正式根EXE保持；新观察字节/diff见final-external-build-observation.json。没有因此重启测试/构建或称整个source闭包身份不变。先前“66输入保持”只描述较早核对时点，不能当最终快照。
