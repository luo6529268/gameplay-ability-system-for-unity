# Batch79 当前执行准备更正（2026-10-09，UTC+08）
状态 PLANNED；与旧79同一Task/Change/Operation，不另建Goal，不新增候选。
开始观测：2026-10-08T18:09:25.4598488Z；HEAD a10c358eeb956f2153e7d5bfbb60c3c1b353fd50；暂存为空。旧 before-manifest-01 保留，但其九个 before-01 路径本次均不存在；旧“已Copy/核同”说明当前无执行证据，不作为可恢复备份。无法据此判定删除或执行者。正式修改前只使用本次 fresh before-manifest-02 与 before-02 经 SHA 核同副本；旧事实不抹除。
[准确12逐文件列表](before-manifest-02.json)：三Editor源码、原5治理文档、公共测试XML、旧Task/Change/Operation正文。保护继承82当前736非写域，附加82 admission/审计、三meta、79旧manifest及旧计划九ABSENT状态。第一遍只读检查曾误将正确ABSENT guard当漂移，已更正exists判定；无写入副作用。
需求/授权：现有六项Goal Task第0—8节的H11必要检测/归因；80人类批准SetPass修复已通过、82限定Driver资格已通过，79原DEFERRED已具备独立安全条件。仍未批准Role默认推广/新算法。阶段4/6、H07/H11 OPEN、已执行60；本次正式脚本/验证启动后61，不因诊断完成停止Goal。
源码写域严格沿旧Task三Editor文件。先12新RED，后12+旧33+literal8=53 GREEN（实际XML为准）；原Scope、ring8、late硬限1800、strict1800/所有旧请求不变。唯一16camera正对照请求第9次具名NoInlining 1MiB；若更早自然事件则保留归因不重复。最多一次fresh raw recovery；不得缩scope或用人工GC消去原43 FAIL。
即将执行的文件操作命令：
```powershell
$m=Get-Content -Raw -LiteralPath 'docs/ai/FILE-OPERATIONS/NTSD-OPTIMIZATION-BATCH79-LATE-CAMERA-POSITIVE-CONTROL-20261008/before-manifest-02.json' | ConvertFrom-Json
if(Test-Path -LiteralPath 'docs/ai/FILE-OPERATIONS/NTSD-OPTIMIZATION-BATCH79-LATE-CAMERA-POSITIVE-CONTROL-20261008/before-02'){throw 'Backup must be fresh'}
New-Item -ItemType Directory -Path 'docs/ai/FILE-OPERATIONS/NTSD-OPTIMIZATION-BATCH79-LATE-CAMERA-POSITIVE-CONTROL-20261008/before-02' -ErrorAction Stop
foreach($f in $m.files){
 if((Get-FileHash -LiteralPath $f.path).Hash -ne $f.sha){throw 'Source drift'}
 if(Test-Path -LiteralPath $f.backup){throw 'Backup exists'}
 Copy-Item -LiteralPath $f.path -Destination $f.backup -ErrorAction Stop
 if((Get-FileHash -LiteralPath $f.backup).Hash -ne $f.sha){throw 'Backup mismatch'}
}
```
后续源码/文档仅apply_patch；Unity existing6402/74724精确import三源，run_tests/get_test_job；公共XML每次实际Runner覆盖前保存新无Force副本、终态XML每次保存新档。只在Menu sole clean8/idle/noTests/noCompile fresh gate后测试；实际camera时正常manage_scene load原savedBattle、一次已登记菜单，完成后正常返回原Menu。不SceneSave/secondEditor/forceStop/ClearConsole/Profiler.Clear/Prefs/文件删除或Git mutation。
输出CreateNew：REPORT/red/green XML与JSON、camera-01旧Task列明raw/report/history/恢复文件、新审计JSON。历史备份/Profiler设置恢复由原owner完成，raw加载仅append；不保证in-memory全部历史字节保持。唯一作用检测资格，不称H11完整0GC、生产缺陷修复或性能收益。
回滚：12current副本保护所有dirty；需人类批准只撤本包hunk；不reset/restore/覆盖其它内容。

