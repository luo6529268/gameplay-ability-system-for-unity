# E3 同源缓存与实际caller接线

2026-09-13，NTSD28-B11-SOURCE-CACHE-CALLER-PRODUCTION-001 / VERIFIED_SOURCE_CACHE_CALLER_LOAD_GATES_ONLY。E3及Q02加载基础限定交付；正式内容迁移和总目标未完成。

## 实际改动

- GameConfig新增BattleContentRuntimeRoot，空默认保留迁移前内容；现有asset未改。Native目录按明确root或相对application data父目录解析。
- manager的PrewarmConfiguredLoganContentAsync统一候选输入cache及实际发布。root locator只指向完整root+VisualFingerprint key；命中后仍核对DAT/catalog/images，漂移重新捕获。Unity publication不缓存bool，新owner从输入重新发布；同owner必须配置/对象/UI三key一致且资源仍存在才能复用。候选配置从捕获的immutable DAT重建，消除跨owner可变value共享。
- 同source并发请求共用一次加载并转发进度；关闭stage1及manager销毁取消configured输入世代，E2生产scope在每次materialize/commit核验该许可。迟到输入不写cache/发布，Unity资源继续E2 stage5回收。
- LoadingPrewarm实际走所选source，旧配置cache命中也在完成回调应用；sprite不缓存true。前置成功后才预热pool，最终source/owner核验后才Ready。直接LoadCharacterData及Start、App.InitializeBattleAsync读同一来源，出生前后及异步边界复验，禁止新版失败退旧。
- pool保留旧PrepareCapacityAsync签名，复用分批算法增加generation/caller predicate，在yield之后和创建前检查；取消不得继续创建或记目标完成。旧World/准备世代不会被过期Start失败关闭。
- SelectRoleItem只调整已有资源重绑的引用身份判定；manager保存owned头像到角色ID的metadata，支持UI.Clear和owner重建时重绑Idle头像，保留选择状态/输入/倒计时文本。

九生产脚本及三个测试脚本均在同ID Task/Record声明。通用NTSD_ResourceLoader调度和音频代码未改，无Gen/Plugins/asmdef/schema/33ms/顶层关闭顺序改动；WAV、背景与正式资源迁移保持后继范围。

## 当前验证

| 检查 | 实际结果 | 证据 |
|---|---|---|
| 初始RED | 6FAIL/1PASS；实际复现可变配置别名和generic cache跳过Execute | focused-red.json |
| 首次实现 | 29项完成、2FAIL：InvalidDataException捕获范围及EditMode销毁夹具 | focused-v1.json |
| 第二轮 | 35/35 | focused-v2.json |
| 加入头像重绑和旧关闭回归 | 39/39 | focused-v3.json，job42c7004f675c4ad0a2885e1fcd4757b7 |
| 强化已销毁owner的Idle头像断言 | 13/13 | focused-v4.json，jobc10fea1a49c84b09a5e7ab7b44722952 |
| 完整SelfCheck | 本次实际PASS，结果晚于请求 | selfcheck-result.txt及selfcheck-requested-at.txt |
| 真实direct首次 | FAIL，来源核验拦截，未进入战斗；原测试在Play后注入配置，存在启动时序竞争 | e3-direct-1.json、e3-direct-1-errors.json |
| before-scene配置安装后的direct | PASS；World4、三key相同、RuntimeMapCleared、两帧仍Stopped、实际pool0、46资源全部释放 | e3-direct-2.json |
| 真实App/menu及menu重进 | 全PASS；每轮World4、三key一致、两帧仍Stopped、pool0、46资源零残留；menu cache hit1且Ready | e3-app-1.json、e3-menu-1.json、e3-menu-2.json |
| 基线保护 | 3059中3047原hash不变，12个声明既有脚本变化，零缺失 | workspace-protection.json |

EditMode owner销毁测试显式调用生产幂等OnDestroy；真实Play不这样模拟，使用实际Unity对象生命周期。所有失败artifact保留，不删除以伪装首次成功。

## Play方法与边界

Editor probe在进入Play前准备临时合法catalog/DAT/PNG、实际场景角色ID集合和GameConfig asset path。仅测试的BeforeSceneLoad hook克隆配置并安装root，保证生产Start从最初读取所选来源；原asset不变，不提前创建runtime manager。direct由现有Start加载；app/menu用既有stress抑制只准备内容，menu再销毁无实体的旧manager，通过真实PrewarmOnceAsync从缓存重建，然后实际App.InitializeBattleAsync。运行后核对source key/定义名、World、ordered shutdown、两帧停止、真实pool与owned Sprite/Texture卸载。

数据为合法隔离夹具（当前IDs含0/2/50/52），不是正式330对象内容证明。完整6DAT Converter问题仍Q03，Q07正式迁移未执行；既有声音缺失警告不因此视为音源已对齐。本包只能在三caller及退出重进出口全部有证据后限定关闭。

通过现有Unity2022.3.62f3与Temp/Goal13_bridge.py执行refresh_unity、run_tests/get_test_job、read_console和manage_scene；没有第二Editor写Library。完整SelfCheck走现有request入口。Play request为Temp/NTSD28_B11_SourceCaller.request.json，mode direct/app/menu；结果文件按唯一runId保存。Ledger实际命令为`& Tools/Validate-ChangeLedger.ps1 -RepositoryRoot $PWD.Path`，当前470 records/40 governed diff PASS。最终Scene/CS/保护与Ledger仍须在最后Play后刷新。

## 最终补充证据

final focused40/40见focused-final.json（jobca5bbc179ca44add941bf701f0f779a9）；最后完整SelfCheck新结果见selfcheck-final-result.txt及对应请求时间。加入global owner替换guard，仍存活的旧owner也不能让迟到输入写cache/发布。默认旧内容真实App回归见legacy-play-regression.json（原E2目录e3-legacy-regression-1），Running/World4关闭0残留及E2四项/113资源回收均通过。

editor-state-final.json为Playfalse，scene-final.json为NTSD_Battle dirtyfalse/root14，compile-console-final.json实际0条error CS；workspace-protection-final.json为3059/3047不变/12声明变化/0缺失与范围外变化；change-ledger-final.log为470 records/40 governed diff PASS。没有声称完整Console无error：旧音频缺失和预期负例日志与本包通过范围分开。

下一为Q03 NTSD28-NATIVE-DAT-AND-JOINT-FIELD-CONTRACT-AUDIT-001，资源默认root仍空、原资产未迁移。Q02返回R17加载子条件，不关闭B11整域或Q07/Q09/Q10/Q12。
