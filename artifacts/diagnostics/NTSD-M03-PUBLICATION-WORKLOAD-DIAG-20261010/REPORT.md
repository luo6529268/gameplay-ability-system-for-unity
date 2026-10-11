> **2026-10-10 M03 workload diagnosis COMPLETE / NTSD-M03-PUBLICATION-WORKLOAD-DIAG-001 VERIFIED (diagnostic only).** User-resumed attempt03 successfully observed World/Base camera. 89 steady completed builds at tick120->298: publication89, alpha-only0, repeated0; no failure/rejection, all actual TextureArray/mean1992.056 quads/segment1/1000AI. No same-publication template reuse opportunity in this workload; do not implement cache or dispatch. Full runner120+180 and ordered cleanup passed, last2tick camera not observed. Focused2/2 fresh, 157 protections/12 backups unchanged, Menu clean8; no production/performance/Driver0GC/device claim. M03 parent OPEN, monitoring paused. Report: artifacts/diagnostics/NTSD-M03-PUBLICATION-WORKLOAD-DIAG-20261010/attempt-03/REPORT.md. Below preserves earlier failed attempts and stages.

# M03 publication/alpha 工作负载检验

Task: NTSD-M03-PUBLICATION-WORKLOAD-DIAG-20261010
Change: NTSD-M03-PUBLICATION-WORKLOAD-DIAG-001
Status: INCONCLUSIVE_OBSERVER_FAILURE / FINITE_DIAGNOSTIC_STOPPED

## 最终结论（优先于下方启动前计划及过程记录）

本轮没有取得足够的正式阶段构建分类数据，不能判断“同publication的未插值命令模板复用”在当前千人负载的命中率或收益，尚不能交给执行会话实施。原因是本讨论会话新增观察器的两处缺陷，不是生产策略经过验证后无收益，也不是GC门阻止本实验。

实际执行两次combat1000，各120warm+180sample；首次报告所有权比较把绝对/相对路径混用，FAILED/0行。保留失败后修正路径，以同任务一次替代采集作为明确更正；第二次因只筛CameraType.Game而未限定World/Base相机，观察到第二相机后PARTIAL退出。第二次仅有tick0/steady0的一行，且submission不可用；两次有效稳态camera分类行均0，未达到事前60行门槛。不把初始0计数解释为alpha-only为0，也不从300tick/150Unity帧推造完整Build分类。

最终已修正同一Editor文件：冷态匹配真实ProductionEntityStressConfig规范化路径，观察前复用生产CanRenderCamera限定World/Base。最终原Editor定向测试2/2通过（job32ce7b201fdb451c8632fcff1cfdd86b）；原计数器20/20通过的范围保持。**相机修正版未进行第三次实景采集，仍是运行时待验证；不能把定向通过写成诊断完成。** 旧失败输出不覆盖、不删除；工具默认无活动owner，原输出存在时拒绝覆写。

两次原runner均StoppedCleanly、300tick/120warm/180sample、容量压力180tick无拒绝/丢弃；ordered cleanup后activeGameObjects/worldObjects/worldEntities/claimedSlots/objectPoolActive/referencePoolActive均0，cleanupException0。两份tick300 extended-checksum JSON完整对象相同，仅证明末态一致，不替代逐tick。原报告89个steady渲染帧command均值1992.056、segment1支持工作负载存在；不补缺失的publication/alpha分类或binding证据。

本轮没有实施缓存、生产代码改动、优化对照组、CPU净收益、完整Driver0GC或设备验收。原无可靠校准的报告零分配字段不晋升为0GC；旧Driver失败及所有默认OFF/暂缓保持。M03父项仍OPEN，未派发新任务、未恢复监测。下一次若恢复本策略，仅补已修正观察器的一次有效World-camera采集；在数据成立前不写模板缓存或再跑旧UV策略。

保护初核：157个事前具名源码/Scene/Settings保护文件0漂移，8份原备份有效，HEAD及暂存区未变化，原Menu恢复clean/8根。最终validator及差异核验另见本报告末尾。

## 本轮准确问题

验证跨显示帧复用“未插值命令准备结果”在当前千人混战是否存在实际命中机会。旧两个小规模 Battle 窗 alpha-only 约43%，不代表千人负载。旧千人窗口300tick/150 Unity frame且每帧2tick只是线索，不能代替实际 Build 分类。

本轮仅一份 Editor 工具，读取现有 production Build 计数和已完成 submission 的实际 segment binding；不实现缓存，不改 backend/World/Host/Suite/资源/Scene文件。观察使用固定1024行；日志、序列化只在结束执行。分类证据不冒充 CPU收益或完整Driver0GC；旧GC失败不被该观察抹去。

## 有限实验与判据（启动前已声明）

- 一个 combat1000、1000 Sakura、Role、DataOrientedCanonical、seed1314149188；120warm+180steady，原presentation/动作/配置。
- phase/presentation/detail/frame timing 均OFF，Profiler OFF；无JIT准备，无增warm，无候选。
- 复用原 request processor 和有序关闭；先等待原 Battle 服务就绪，再提交唯一请求。资源准备不计逻辑warm或steady。
- 单Game相机、同counter epoch、固定容量不溢出、类别之和等于全部完成Build；至少60个稳态有效camera观察才裁决该fixture。以相邻camera累计量之差核构建，不把请求次数或末态当已完成构建。
- alpha-only为0则结束跨显示帧模板方向，不追加该缓存的成本组；该窗口内无命中机会，不等于其他负载永远无机会。
- 非零也不自动修复：仍需净成本、失效条件与载荷等价。未完成或数据无效则INCONCLUSIVE，不重跑寻求收益。

## 复用条件及代价（当前源码）

1. `BattleCentralRenderSystem.MaterializeLatestPublishedFrame` 已对相同Unity帧、相同publication+alpha复用。新增策略只可能作用于同publication而alpha改变的构建，不能把已跳过工作再计收益。
2. `BattlePresentationShadowBuild.BuildCommands`（2897起）读取 captured entity/hit/overlay 数据和 common bindings；2967起另读当前 viewport、WorldCamera 的位置、aspect、orthographicSize计算可见范围。因此publication不变不足以证明所有命令输入不变。
3. `BattlePresentationFrame.CopyFrom`（1132起）复制实体、当前/前一运动状态、hit、knockout、command、标签与顺序，并复制CommandsMaterialized。发布内容必须只读；不可就地把publication改成已经插值的数据。
4. `BattlePresentationDisplayMotion.ApplyToCapturedCommands`（158起）按handle/generation取本次delta，跳过HitRecord，区分Body/Ground anchor，调用WithPresentationOffsets（ShadowBuild.cs:929）生成新命令并替换。下一次必须从未插值基准开始，不能累加offset或用浮点相减恢复旧基准。
5. 模板必须在现有submission/presentation生命周期下拥有：World/publication更换、catalog/common binding变化、viewport/camera/投影变化、资源失效、容量拒绝、异常与Clear/Dispose均需正确失效；不能占住正在借用的immutable submission。Stage5清理、冷容量预备、generation/透明顺序/完整vertex及辅助挂点保持。
6. 每个新publication保存模板至少增加一次命令存储/复制及容量；每个alpha-only仍需复制模板或重建受插值影响字段，并完成必要Mesh/上传。未实测字节数或净时间，不能把旧4.156ms全算作可省时间。若无alpha-only命中，缓存不能消除这类重复构建。

## 检验入口修正记录

- 新脚本首次scripts-only compile未导入新asset；菜单不存在，0个实验样本。随后精确导入该脚本再compile。
- 只读execute_code类型检查受CodeDom命令长度限制失败，未执行，不绕过工具限制修改其配置。
- 首次通用request进入Menu Play但没有Battle服务，取消并退出，0样本。
- 增加干净单Scene的Battle加载/恢复后，通用request的120秒服务准备超时仍发生在runner之前；保留preparation-timeout.txt，退出并恢复原Menu。
- 修正同一观察器冷接线：按现有Suite先等BattleTestBootstrap和AreProductionServicesReady，再提交请求。没有改变原request processor/生产启动行为，没有增加逻辑warm/sample或重跑成本组。操作与各版本备份见Operation。

原计数器20/20 focused通过（job45e63c4fbfc34258ac3a1b60bfad3f66），覆盖classification/同sample/同Unity帧/epoch/World/强制入口等。仅为原计数器契约验证；新观察器及实际实景必须以其终态独立判断。


Correction 2026-10-10: attempt-01 actually completed 120+180 logic ticks, but observer rejected its own absolute report path against a relative string and recorded zero camera rows (FAILED). This is diagnostic implementation failure, not strategy or gain failure. Original report/observation retained. Ordered shutdown calls ResetRuntime and clears cumulative counters, so missing camera attribution cannot be recovered after cleanup. Under the user request to continue verification, fix only the same Editor observer and validate exact ProductionEntityStressConfig.FromRequest output ownership before ONE replacement acquisition at attempt-02. This explicitly supersedes the prior single-run/no-retry limit only for this tool defect: two workload runs total, at most one valid acquisition; no candidate/CPU-cost experiment, no altered warm/sample, no rerun based on outcome. Any further invalid acquisition ends INCONCLUSIVE. Prior source saved as observer-before-owner-path-fix.cs.txt.


Attempt-02 terminal: runner completed 120+180 and ordered cleanup, but observer stopped PARTIAL at its first second Game camera. Only one tick0/steady0 row exists, submission unavailable, so ZERO valid steady classification rows. No frequency/binding/gain conclusion. Second defect is observer camera scope (Game != world render camera). Retain all raw files and stop bounded experiment; no attempt-03. Same-file correction reuses production CanRenderCamera(world/base) before counting, with temporary in-memory Camera fixtures disposed in finally, no scene save or production change. This correction gets compile/focused only, NOT live acceptance. Final diagnostic state must remain INCONCLUSIVE/BLOCKED.

## 最终收尾证据

- `final-focused-result.json`：最新代码实际原Editor测试2/2通过；没有第三次实景采集。
- `editor-final.json` / `scenes-after-focused.json`：原Editor idle、非Play/非编译/非测试，NTSD_Menu clean、8根。
- `audit-final.json`：157个具名保护0漂移、8份原备份有效、HEAD/暂存不变；本轮只新增Editor观察器及其meta，无生产脚本修改。
- `Tools/Validate-ChangeLedger.ps1` 实际运行exit0，完整输出 `validator-final.log`；旧记录路径与当前diff不一致的WARNING保留，不称无警告。
- 本任务具名路径 `git diff --check` exit0；新观察器无尾随空白。日志 `diff-check-final.txt`。
- Change保持BLOCKED，Task为有限诊断停止/INCONCLUSIVE。没有派发、没有恢复自动监测，原M03/H07/Android与其它暂缓边界未改变。
