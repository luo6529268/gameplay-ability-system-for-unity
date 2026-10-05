# hit_Fa10 共用目标处理与旧运动尾部：限定验收

RUNTIME_PENDING / SCOPED_TWO_TICK_DRIVER_PASS，正式EXE336B44权威不变。两个正常Driver tick与指定三个目标模式已经验证；不是自然Play、自然HP耗尽全过程或完整目标系统一致，父Q与总目标开放。

当前正式native对Fa10先执行common目标缓存/重扫/无目标，再直接返回；旧Unity跳过目标处理，多做±1.1f加速、夹速±30、Y cap3、朝向及提前YInt。正式902/4/5实际可达，这些动作会进入下一物理tick，不是无消费者的旧代码。

## 精确改动

- LF2Entity.RunCurrentDatFrameLogicBeforeAdvance的10分支调用现有ResolveFrameLogicTargetByHitFa，仅解析后目标槽仍-1且Health存在时置HP0；直接返回，移除上述额外运动。HP0不提前退出，stale非负slot不清空。
- BattleRuntimeSelfCheck.CheckCurrentDatFrameLogicSharedRouting保持三个CLR/当前DAT非角色类型及后续单次SimTU/3/4/14代表检查，纠正旧Fa10运动预期：Vx±1、Y9/方向保持、无目标HP0；SimTU为X1/Vx0，未为旧断言改变正式规则。
- 一个既有Editor测试文件仅新增两方法/四案例/221行，旧行保留。三脚本准确Record/Task/Operation先于改动；两个生产/自检方法之外逐字节保持，DAT、共享resolver本体、其它hitFa/physics/GAS/非战斗保持。
- before十备份（三脚本七文档）保存已有dirty，不删除、移动或Git丢弃。

## 实际结果

| 检查 | 实际结果 |
| --- | --- |
| 生成RED/GREEN编译 | dotnet build Assembly-CSharp-Editor.csproj --no-restore --nologo -v:q -clp:ErrorsOnly，两次exit0/0错误，301/334警告，10.813/12.812秒 |
| 当前Core诊断编译 | 当前正式build声明28 CPP闭包相同，C++17/O2无fastmath，exit0/40.203秒；74项输入SHA前后稳定 |
| native两完整tick | 当前正式catalog902/0/HP0，源(500,-100,600)/V0/target0，正常step1 Fa1→frame4/counter1，step2实际Fa10→frame5/counter0；源位置/V0/HP0保持，AI common/cache/no special/unsupported0守卫通过 |
| 原Editor RED | job496fbe7d869d453998c3a3ad3be9d984 四项预期失败：完整tick2源整数501 vs500；缓存Vx2.100000023841858 vs1；成功重扫槽仍-1 vs0；无活体候选HP仍500 vs0 |
| 原Editor GREEN | job74b77c30068f402aba534966f0219ad8，同四案例＋既有局部SelfCheck5/5 PASS，0失败/0跳过，49.6382731秒 |
| 两tick声明字段 | 初态+tick1+tick2，13字段×3行：修前36/39同，tick2 sourceX501.10000002384186/整数501/Vx1.100000023841858首差；修后39/39同，view比例残差0 |
| 原Editor编译/状态 | MCP两批各Refresh一次，runtimeDLL分别新于Entity与SelfCheck，EditorDLL新于测试；终态idle/nonPlay/无活动测试、Battle clean/root11、Console0错误 |
| 正常关闭 | GREEN正常callback返回后既有wrapper objects/claimedslots/logicpool borrower0断言实际执行通过。RED异常不宣称这些尾部断言通过 |
| 独立只读审阅 | 新测试/native及最终diff无阻断；未由审阅者执行GREEN，实际结果由根代理验收 |

精确新增测试：
- NTSD.Test.Editor.NTSD28Q07NonCharacterHitFa7EditorTests.IndexedHitFa10CommonTargetHasNoMotion(0,1.0d)
- NTSD.Test.Editor.NTSD28Q07NonCharacterHitFa7EditorTests.IndexedHitFa10CommonTargetHasNoMotion(1,-1.0d)
- NTSD.Test.Editor.NTSD28Q07NonCharacterHitFa7EditorTests.IndexedHitFa10CommonTargetHasNoMotion(2,1.0d)
- NTSD.Test.Editor.NTSD28Q07NonCharacterHitFa7EditorTests.IndexedHitFa10AfterDeadFrameTransitionSurvivesTwoDriverTicks
- GREEN相邻：NTSD.Test.Editor.NTSD28Q07HitFa5FullDriverEditorTests.ExistingLiveHitFaRepresentativeRoutingRemainsValid

原件包含native-mode10.jsonl/run-result、native-compile-result、source-input-manifest-v2、generated-red/green-build、editor-red/green-launch/poll/DLL/scene/console、两unity-full-driver*.jsonl、declared-field-comparison、两delta.patch、before-manifest/test-delta-audit、INDEPENDENT-REVIEW和GOVERNANCE-CHECK。首次模板生成因旧CLI parser假设守卫失败发生于apply之前，基于实际源更正；manifest-v1复用文字误称单步，v2只更正为两个实际step，源/命令/JSONL一直两步，没有重跑或覆盖原件。

## 范围与剩余候选

完整Core诊断不是正式playable Host/GameSession/根EXE；Unity保留旧schema/seed/Stage23/部分target初态，实际使用当前LoganRuntime+项目mode Asset。仅声明subject13字段和指定type0目标模式，不宣称完整World/Host初态同态。JSONL实际核对HP0/frame/计数/sourceZ，exit0本身不替代字段比较。直接Y9案例只跑AI，不能当作物理后Y9；初始YInt9只能识别旧cap连带整数变化，删除提前同步由diff证明。SelfCheck最终X/Vx只证明单次物理/没有额外运动，不能独立证明幂等目标处理调用次数。

自然出生/自然命中跨零、其它缓存/失效目标、正式根/GPU未证明。五DAT两端和raw相同、四Scene/config、五权威及十before备份保持；最终Validator/diff/哈希见GOVERNANCE-CHECK。源规则中2F8排除组与缓存非角色类型两个既有共享resolver候选已另存COMMON-RESOLVER-FOLLOWUP.md并回链总表，不混修、也不冒称无剩余差异。

本必要ONE完成；227份同名Record/66未关闭，REUSE52/TRIGGER14/P0=DEP=ONE=0。当前限定证据复用，未覆盖条件仅相关逻辑改动/实际非例外首差触发，不扩角色/音效矩阵；父Q07/Q09/Q12及总目标保持开放。
