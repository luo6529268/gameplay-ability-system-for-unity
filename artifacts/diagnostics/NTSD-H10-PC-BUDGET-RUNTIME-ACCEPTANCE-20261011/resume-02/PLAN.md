# H10 resume-02（修改脚本前）

Task NTSD-H10-PC-BUDGET-RUNTIME-ACCEPTANCE-20261011 / Change NTSD-H10-PC-BUDGET-RUNTIME-ACCEPTANCE-001。人类继续H10与已批准预算授权保持；主讨论会话独立审阅后派发本次四点修正及一组新版Play。原根及resume-01失败/校准/报告/源指纹不可覆盖。

唯一代码写域：H10ResidentClosureDiagnostics.cs中PcBudgetOutput/RestoreKey改至resume-02；PcBudgetGenericCue改为真实raw ID `@"data\016.wav"`；PcBudgetRuntimeQualification增加Builtin的Ordinal成员身份/文件路径分别记录；RunThreeSessions先读sealed preparedCues原key和wrapper非null/同引用，cold证据落盘后检查clips/cache/跨轮身份，formal/nonmono读取前非null；CloseDriver仅将采样字段重命名pendingAtPublicShutdownReturn并说明stage10返回时点。除此不改任何实现。三个recorder类/资格算法/sink/emitter/ExerciseHot逐字保持。

风险：目录raw ID与磁盘path混用、空wrapper掩盖首差、诊断字段错标stage5。修正不注册alias、不补资源、不改root/controller、公开prepare与封存、音频规则/33ms/输入RNG/checksum/pass或有序关闭。无新的缓存/manager/queue；新增字符串和cold匿名证据由该测试调用拥有，在热scope之外创建、输出后随测试释放，不进入生产。原固定容量和Stage1/5/10/11/async drain/下一帧Destroy合同不变。

最小验证：当前dirty备份/源DLL/旧artifact及4094保护；原Editoridle/nonPlay/noncompile/update/nontest/Menu clean8；最窄compile；本版本两项无解码资格（raw-key与现场、原三个独立recorder的有效正控/empty/invalid门）。全部通过后且仅一次EditMode Enter/ExitPlayMode组：三轮prepare、音频普通/1000同cue、多cue、同帧及连续tick发布播放、公开DriverShutdown/drain/后帧Destroy/重入；随后同组pending取消迟到与Unity自然Destroy。1000为音频请求，不是实体。任一硬门首差停止，finally限声明内安全清理，不追加组/全目录/旧回归。

构造时捕获临时host/MM/manager/listener/driver，保留当前cache/globalclip/config/组件/Scene/Settings状态；不得误毁原对象。generic三轮借用同引用；最终无prepared/voice/config/旧cache/跨key借用才按精确key与ID去重RemoveCache/Destroy。owner未知保留失败，不全局清理，不重启Editor。stage10/11只覆盖无真实地图的诊断World，不强写Stopped。

回滚依据：本轮11份真实dirty字节备份及前指纹，旧失败源b3a289d8…5c5；只允许审计后撤本轮诊断增量，不用HEAD恢复或撤既有生产。新原件CreateNew在resume-02，共享Temp XML每次覆盖前准确备份。当前未运行新测试。预算已批准/旧218.313758MiB对象门复用保持；Player/整进程/目标机未测。无Q06新正文/子代理/其他点/Goal/Settings/Player，最终H10仍需独立核原件。
