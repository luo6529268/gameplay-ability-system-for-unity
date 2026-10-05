# 共用追踪与回收规则坐标：限定修复

2026-10-05；Change NTSD28-336B44-Q07-D024-TRACKING-RULE-POSITION-001，RUNTIME_PENDING / SCOPED_FULL_DRIVER_PASS。本包只有两个生产脚本和一个既有测试档；不改DAT、资源、Scene、非战斗。原Editor新12项RED中六项实际失败，修后新12项及原7六邻例18/18通过；518/frame1单个完整生产Driver tick 1/1通过。自然原Scene/正式根同初态/GPU等未覆盖，父Q与总目标仍开放。

## 实际首差与共用修复

当前正式336B44 EXE及对应native_ai.cpp/build闭包SHA见before-manifest。NativeAi28::step_non_character_hit_fa使用源整数X/Z：1死区7/加速度0.3，3死区10/0.17，2/4/12/14/7死区5/0.4；4在X严格±30、Z严格±10与Y严格0..80时停止/action60/目标E4=100。当前Unity目标排名已有source门，但后继死区/回收比较仍用view整数；这属于新消费者首差，不重做旧目标扫描。

固定2048×1152视野下，875源gap±10错误Vz±0.17、700源gap6错误0.3、518与type4武器124源gap4错误0.4；219源X29/Z9应回收到60却仍action0。原EditorRED具名12项完成，其中这六项失败，另六项恒等/正向/不完整source/严格边界阴性通过。不是自然Play或正式根EXE同初态结果。

生产新增LF2Entity.ResolveFrameLogicPositionPair统一入口：active target与双方source历史完整且self当前数据非type0时成对读源X/Z，否则保留view/raw。通用1/3/2/4/12/14/非角色7调用；4的回收与追踪共享同一读数；LF2WeaponFrameLogicResolver4/12调用，4在source不完整时保留原GetRenderZInt取整fallback。所有Y、扫描、HP门、速度常量、clamp、frame写者、旧7与平台阴影增量保持。没有新增生命周期模块/服务或改变关闭顺序。

当前正式type4武器124/frame40由天天frame248 OPoint直接生成；生产pool将type4创建为LF2Weapon，专门12确实使用统一入口。初次仅筛type1/2无4/12的结果遗漏type4，不能推断全部武器无追踪入口；Task已更正。其他正式indexed定义/帧与source召唤入口由只读审阅定位，身份记录见indexed-definition-identities.json；逐文件注明原字节同值或仅CRLF/LF同值，不把后者当逐SHA同版。本轮未改任何DAT字节。

## 实际验证

生成命令均为dotnet build Assembly-CSharp-Editor.csproj --no-restore --nologo -v:q -clp:ErrorsOnly。

| 验证 | 实际结果 | 原件 |
| --- | --- | --- |
| 测试先行生成构建 | exit0/301warnings/0errors/7.67秒 | editor-red-build.log |
| 新8追踪+4回收具名RED，job804fecf984b24ae1b42c89be989fad61 | completed12、六预期失败/六通过；result=null，计数来自progress/failures_so_far | editor-red-result.json |
| 生产修改生成构建 | exit0/334warnings/0errors/15.42秒 | editor-green-build.log |
| 同12项+旧7四边界/active/raw两邻例，jobbb07a0d205e84ffaa0a3d4a656ce985f | summary18/18通过/0失败/124.5701521秒 | editor-green-result.json |
| 单个完整Driver方法生成构建 | exit0/301warnings/0errors/8.58秒 | editor-full-driver-build.log |
| 518/frame1单生产tick，job242dd61ff0514d7d9eee2746486fb120 | summary1/1通过/0失败/24.1515287秒 | editor-full-driver-result.json |

8858等是发现总数，实际没有全套执行。GREEN两次includeDetails=true观察超时已保存；同handle includeDetails=false取得正式成功summary，没有重启/重复测试。MCP三次Refresh都收到request成功，并在脚本对应新程序集、Editor空闲/非Play、原Battle clean后才启动具名测试。

## 完整Driver取证边界

875/frame50的dvz550在后续FrameMotion把任何追踪Vz归零，所以该帧tick末静止不能裁决死区；不改DAT或pass来制造证书。本包选正式518/frame1，无dvz/OPoint，错误0.4可进入积分。复用旧wrapper schema/seed/诊断Stage23和部分target初态，读取当前LoganRuntime与项目模式Asset；旧5EDA元数据只是既有夹具约束，没有晋升为336B44行为权威。初始化回调在tick外注销direct-new旧875，确认槽/绑定清空，new正式518注册同slot1并更新诊断Roster身份。明确两对象，双方源X400、Z600/604、Y-100/0及零速度，唯一StepOneTick后检查精确源Z/Vz/view增量、目标及数量；没有新scenario/trace/请求文件。成功scope正常检查对象/槽/logic pool三项0并恢复发布，失败不能借异常Dispose宣称正常关闭通过。

## 尚未证明的边界

正常完整Driver返回已直接断言Vz0/精确source600/target604/viewDeltaZ0、两实体及目标槽正确；既有scope完成对象/槽/logic pool三计数0并恢复发布。最终[authority/diff/Editor原件](final-authority-diff-and-editor-state.json)记录原Editor idle/非Play/无测试、原Battle clean/root11、Console0error，四保护文件/四authority/七DAT两端SHA均保持。独立只读最终审阅同时核对1/1结果JSON、无dvz/OPoint判别、旧槽解绑/新注册与Roster新StableId，没有扩大测试的理由；审阅者未执行测试。当前本必要ONE完成，REUSE45/TRIGGER14/P0=DEP=ONE=0；220份同名Record/59未关闭不是59个必跑任务。

18项只证明受控正式内容的共同坐标读口、分支判定与一次既有mechanics积分，不能据此关闭Q07/D-024/Q12总门。自然按键Play、正式根EXE同初态、GPU/真人输入与未覆盖分支仍未知；type0/raw等兼容范围按静态/邻例证据各自报告。正式hitFa14的Y写者未在本包修改或取得全规则证书，其旧SelfCheck只属历史回归；不能把本包X/Z通过扩大成整个hitFa14规则一致。

最终治理检查：pwsh -NoProfile -File Tools/Validate-ChangeLedger.ps1 -RepositoryRoot I:\GitHub\Unity_GAS\gameplay-ability-system-for-unity 实际exit0/PASSED，1273份Record覆盖当前24个dirty governed脚本；本包仍仅拥有声明的三个脚本增量。首次git diff --check发现四份本轮状态文档新增EOF空行，已仅去掉这四个尾空行，未重排其它内容；最终git diff --check实际exit0。
