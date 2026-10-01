# C044 鬼鲛自然跨零原 Battle Scene 探针进度

状态：`CODE_WRITTEN / GENERATED_CSHARP_COMPILE_PASS / UNITY_EDITOR_IMPORT_PENDING`。正式OID17 action314/X550 的自然跨零在源/336B44根同LFR已证，X1200阴性亦已证；[源/根报告](../NTSD28-336B44-Q07-C044-KISAME-NATURAL-REACH-001/REPORT.md)。本包只新增原项目 Battle Scene Editor 请求探针及meta，不改生产、DAT、Scene或Prefab。近/远两次请求分别固定 `kis17-a314-x550-natural-scene-01` 与 `kis17-a314-x1200-natural-scene-01`；每次生产 Driver 60tick，中性输入，捕获实体、关系、timeout和RNG。原场景与菜单、GameConfig、ProjectBattleModeConfig四个保护SHA在脚本创建后仍与既有基线一致。

生成 `Assembly-CSharp-Editor.csproj` 已临时列入新脚本并执行 `dotnet build --no-restore -v:q`，结果0错误/263警告；日志见[构建输出](../NTSD28-336B44-Q07-C044-KISAME-NATURAL-REACH-001/generated-csproj-build.txt)。这**不等于**Unity Editor脚本编译。原项目Editor PID11944仍可响应，但 `Library/ScriptAssemblies/Assembly-CSharp-Editor.dll` 时间为2026-10-01 05:47:30，早于新脚本06:20:28；`unity status --project-path` 返回 `STATUS_NO_INSTANCES`，该项目未接入Pipeline。已请求用户在原Editor执行一次Assets→Refresh；收到后先核程序集时间/编译错误，再以文件请求运行两例。当前尚未写请求文件，尚未进入Play，也没有Unity自然首差结论。

待验核心：正式源tick46 timeout8→1，tick47 action124/timeout1→-6/动作0与181，目标速度仍0；tick48目标速度X4/Y-3。原场景将逐tick对比同初态的动作、计数、源规则位置、速度、HP、抓取槽/时限和RNG。若发现真实差异，另立最小修复Change，不能回写DAT数值或把受控机制证书直接晋升自然场景结果。

续查现有已编译入口：`BattleParityTraceEditor` 使用临时 `SimulationWorld`、自建角色且初态 `HitStun=75`/`Vx=Vz=0.1`，不运行原Battle Scene；`NTSD28UnityRawCaptureEditor` 的正式场景schema限既定案例，并要求其固定夹具。二者都不能替代本包的原Scene同初态生产Play，因此没有用兼容性较弱的回放冒充验收。原Editor PID11944仍存活/可响应，程序集时间未前进，继续等待同一实例导入。

2026-10-01 以新[限定验收报告](REPORT.md)覆盖上方导入/Play待验快照：两例已采、逐字段首差0、场景和配置稳定；C044其它入口及Q07仍开放。
