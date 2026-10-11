# M03 当前千人 publication/alpha 构建分类检验

Status: FINITE_DIAGNOSTIC_STOPPED / INCONCLUSIVE_OBSERVER_FAILURE

用户授权：继续检验修订后的 M03 策略。Change：NTSD-M03-PUBLICATION-WORKLOAD-DIAG-001；Operation：同 Task ID。

先验证策略命中频率，避免先写缓存再发现高负载不命中。仅新增 Editor 观察器及必要 focused，不修改生产或已有 Suite。一次既有 combat1000、Sakura1000、Role、DataOrientedCanonical、seed1314149188、120warm+180sample。关闭 phase/presentation/detail/frame timing、Profiler，保持原动作/资源/渲染与候选 OFF。原窗口会改变 Play 内存场景，使用原 request processor 和 ordered shutdown，不保存 Scene，不新建第二实例。

固定预备1024 camera记录：tick/warm/steady/累计 publication、alpha、repeated Build与epoch、完整command/segment/binding计数。仅在 endCameraRendering 中复制原计数/短暂借用已完成 submission，立即释放。无每角色计时，不声称测得命令净成本或完整Driver0GC。至少60个有效steady camera记录、同epoch/单Game相机/所有权及容量有效才能判断本fixture；不足则INCONCLUSIVE，不追加窗口。alpha-only=0则该窗口内可节省工作为0，停止跨显示帧模板方向；非零仍须准确受益工作与成本再进入后继。两种出口均保留旧失败、最终门和当前暂缓范围。

补静态失效条件和模板代价评审；只在可复用真实工作存在时再设计局部成本组，本Task不自动派生产修复。异步观察失败/溢出/非预期reload停止，不重跑找PASS。最终report/保护/hash/原Scene clean/validator/diff收尾。


Correction 2026-10-10: attempt-01 actually completed 120+180 logic ticks, but observer rejected its own absolute report path against a relative string and recorded zero camera rows (FAILED). This is diagnostic implementation failure, not strategy or gain failure. Original report/observation retained. Ordered shutdown calls ResetRuntime and clears cumulative counters, so missing camera attribution cannot be recovered after cleanup. Under the user request to continue verification, fix only the same Editor observer and validate exact ProductionEntityStressConfig.FromRequest output ownership before ONE replacement acquisition at attempt-02. This explicitly supersedes the prior single-run/no-retry limit only for this tool defect: two workload runs total, at most one valid acquisition; no candidate/CPU-cost experiment, no altered warm/sample, no rerun based on outcome. Any further invalid acquisition ends INCONCLUSIVE. Prior source saved as observer-before-owner-path-fix.cs.txt.


Attempt-02 terminal: runner completed 120+180 and ordered cleanup, but observer stopped PARTIAL at its first second Game camera. Only one tick0/steady0 row exists, submission unavailable, so ZERO valid steady classification rows. No frequency/binding/gain conclusion. Second defect is observer camera scope (Game != world render camera). Retain all raw files and stop bounded experiment; no attempt-03. Same-file correction reuses production CanRenderCamera(world/base) before counting, with temporary in-memory Camera fixtures disposed in finally, no scene save or production change. This correction gets compile/focused only, NOT live acceptance. Final diagnostic state must remain INCONCLUSIVE/BLOCKED.
