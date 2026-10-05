# 共用 hit_Fa1/3 非正 HP 运动门：限定完整 tick 验收

状态：RUNTIME_PENDING / SCOPED_FULL_DRIVER_PASS。当前正式 EXE 336B44 权威保持；本包的受控消费者与完整 Driver 已通过，父 Q07/Q09/Q12 和总目标仍开放。

两个真实共用分支在对象 HP0 时仍继续写运动。正式 native_ai.cpp 在共同目标解析后直接返回；Unity Fa1 缺自身 HP 门，Fa3 调用旧漂移。实际非角色生产调用无外层 HP 过滤。修复保留目标解析/缓存和无目标处理，只阻止本体非正 HP 的 AI 运动写入；物理、帧推进和注销仍执行原流程。不是角色特判，不改 DAT。

## 改动与保护

- LF2Entity.RunHitFa1FrameLogic：在原目标解析/目标有效性处理后、坐标与运动读写前加自身 Health null/HP<=0 返回。
- LF2Entity.RunHitFa3FrameLogic：删除原非正 HP 分支的 ApplyHitFa3NoTargetDrift 调用，保留条件和返回。旧 helper 定义不清理。
- 生产两个方法之外原字节完全不变。所有正 HP 运动语句/顺序、统一 source/view、其它 hit_Fa、生命周期、GAS/非战斗保持。
- 同一既有 Editor 测试文件新增两个方法，共三案例；原有全部行不变，无全套/角色矩阵。
- 两脚本、七文档的原 dirty 逐 SHA 备份九项；Operation 记录先于脚本修改，所有新诊断输出 CreateNew。无删除、移动或破坏性 Git。

## 实际验证

| 验证 | 结果 |
| --- | --- |
| 生成工程 RED / GREEN | dotnet build Assembly-CSharp-Editor.csproj --no-restore --nologo -v:q -clp:ErrorsOnly，各 exit0/0错误；分别301/334警告，11.578/10.110秒。不是独立的 Unity Play 证明。 |
| 当前 native Core 编译 | C++17/O2、无 fastmath，命令28 Core CPP与当前正式 build.ps1 声明闭包一致；编译exit0，51.031秒，74项输入哈希保持。 |
| native 完整 Driver | 902/0/1 与206/54/3 两个模式各真实单步exit0，HP0、target0、源(500,-100,600)、V0保持；902正常串行转4/counter1，206为54/counter1。实际 AI result 守卫 common目标/cache retained/无 special tail/unsupported0。 |
| 原 Editor RED | job77c232993d844af493fc38a014a5c81b 三项均预期失败：Fa1 Vx0.85、Fa3 Vx2、完整Fa3源整数502，目标预期均为0速度/源整数500。首次查询30秒超时，继续原handle取得终态，没有重启测试。 |
| 原 Editor GREEN | job8419f289e8b74cbb9cdf4452f092ab8e，同三项＋既有局部SelfCheck共4/4 PASS、0失败、0跳过，43.6676229秒。progress.total8890是发现库存，不是实际运行数量。 |
| 原 Editor 编译/状态 | MCP每批一次Refresh，两对应DLL均新于各自源文件；终态非Play/idle/无活动测试、Battle clean/root11、Console0错误。 |
| 206完整tick字段对照 | 初态＋tick1，13声明字段×2行，修前23/26同，首差为tick1 sourceX500/502、整数500/502、Vx0/2；修后26/26同。viewX比例残差0。 |
| 正常关闭 | GREEN callback正常返回后，既有wrapper的对象/claimed slots/logic pool borrower零断言实际通过。RED异常后不将这三个断言冒称通过。 |

原始参数测试名：
- NTSD.Test.Editor.NTSD28Q07NonCharacterHitFa7EditorTests.IndexedDeadHitFa1Or3PreservesNativeMotion(902,0,1)
- NTSD.Test.Editor.NTSD28Q07NonCharacterHitFa7EditorTests.IndexedDeadHitFa1Or3PreservesNativeMotion(206,54,3)
- NTSD.Test.Editor.NTSD28Q07NonCharacterHitFa7EditorTests.IndexedDeadHitFa3MotionSurvivesOneFullDriverTick
- NTSD.Test.Editor.NTSD28Q07HitFa5FullDriverEditorTests.ExistingLiveHitFaRepresentativeRoutingRemainsValid（GREEN相邻自检）

原件：source-input-manifest.json、native-compile-result.json、native-mode1/3-run-result.json/jsonl、generated-red/green-build.json、editor-red/green-launch及poll、两unity-full-driver*.jsonl、declared-field-comparison.json、production-delta-normalized.patch、test-delta-audit.json、INDEPENDENT-REVIEW.md。审阅者两次只读检查无阻断；实际GREEN由根代理取得。

## 证据边界与文字纠正

native是当前 paired Core完整Driver诊断，未运行GameSession/正式根EXE或自然Play。Unity wrapper使用旧schema/seed/诊断Stage23及部分target初态，DAT为当前LoganRuntime、模式为项目Asset；仅声明subject字段相同，不宣称完整World初态/Host同态。完整方法JSONL实际记录subject HP0，未新增目标最终HP500断言。没有声明自然出生、自然降血到该帧、其它失效目标分支、706链、GPU或所有实体/帧全面一致。

Genma901正式OPoint为902/action40，随后40→41→42→43→44→999进入0。Task原“OPoint902/0”已追加更正，本包902/0始终是受控初态。未修改正式资源或源文件来满足夹具。

五DAT/目录表的两端SHA/raw相同，四Scene/config保护SHA、五权威文件及九before备份稳定；最终复核和Ledger结果见GOVERNANCE-CHECK.md。全部已验证证据复用；自然/正式根等只由相关逻辑改动或可复现非例外首差触发。现有其它14个条件回访不因本包重跑。本必要ONE完成，226份同名Record/65未关闭，REUSE51/TRIGGER14/P0=DEP=ONE=0。
