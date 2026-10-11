> **2026-10-10 当前使用限制：此文件下方为历史执行提示词，不得直接启动。** 用户新决定只按既有优化方案推进，额外发现方向一律不跟进；当前第一项[H-01内容部署](../../../Assets/NTSD/Docs/android-mobile-readiness/h-01-content-deployment.md)尚在策略确认，本轮没有生成或派发实施指令。策略与准确范围确认后，另行形成具名H-01指令；不按下方旧H06入口恢复诊断、旧Goal或自动队列。[最新决定](DECISIONS.md) · [唯一进度](../../../Assets/NTSD/Docs/battle-optimization-progress-tracker.md)。

# 新会话执行提示词（D1=A已确认）

文档状态：FINAL_FOR_USER_DISPATCH。用户2026-10-09已确认“可以先用A”；本文件供用户发送给新会话，当前会话没有执行优化。复制下方分隔线后的正文即可。

---

请在 I:\GitHub\Unity_GAS\gameplay-ability-system-for-unity 执行H-06现有Role-aware碰撞收集器的限定生产准入任务。

我已经确认方案A：必要正确性、容量、0GC、生命周期/关闭和实际收益门通过后，可以让正常游戏默认使用现有Role-aware，不必等整个tick达到33ms。H06父项/H07最终33ms、无持续backlog、正常dropped tick0及Android认证条件不降低，未通过仍保持开放。不要再问我选A还是B，不要仅因尚未33ms就阻止这个已批准子阶段。

先读：
1. docs/ai/CURRENT-AUTHORITY.md、AGENTS.md。
2. docs/ai/OPTIMIZATION-REVIEW-20261009/README.md、DECISIONS.md。
3. Assets/NTSD/Docs/android-mobile-readiness-priority-risk-register.md文首总清单。
4. Assets/NTSD/Docs/battle-optimization-progress-tracker.md文首当前/下一项。
5. docs/ai/OPTIMIZATION-REVIEW-20261009/DEPENDENCIES.md。
6. Assets/NTSD/Docs/android-mobile-readiness/h-06-collision-broadphase-1000.md，以及25/81/82和相关现有证据；按需核对80/86/91/93/95，不重读全部历史。

授权你处理该具名任务所必需的项目自编写生产代码、测试和治理文档，以及安全协调后的原项目编译、聚焦测试与必要性能验证。先只读确认当前dirty状态、实际默认选择入口、受影响调用链、候选资格和证据指纹；修改脚本前登记准确Task/Change、路径/符号、风险、验证与回滚，不为例行范围内步骤反复请求许可。

限定任务：
- 复用现有Role路径，保留Brute对照和回退。不新建网格/Burst/Jobs，不捆绑AI行为或架构重构。
- 先列四场景强一致、逐tick输入/双RNG/checksum/候选顺序、fallback、容量/0GC和关闭的现有证据及真实缺口。25局部collector与82有限Driver不能互相冒充全域证明；但充分通过且未受影响的证据不重跑。
- 在候选实际接线下完成受影响门与可追溯同配置收益比较。数据不能解释或正确性/0GC失败时先定位，候选保持关闭；不通过重复长窗寻找PASS，不以降低标准切默认。
- 必要门全部满足后，完成正常生产默认接线并验证解析出的实际路径，保留回退。若需Scene/资源/ProjectSettings/Input Actions等原排除写域，先提出具体改动供我确认。
- 默认推广后只声明“Role限定准入完成”；整tick尚未达标则H06父项/H07保持开放。如需后续正式阶段验收，按原120warm+180冒烟和120warm+1800正式门执行，不把限定准入短窗当正式性能证书。
- 当前仅授权这个具名任务，不恢复旧自动Goal、不自动连续实施34项。结束时整理下一项H07-INPUT建议及依赖；需要新路线、独立专项或新行为合同再由我决定。

当前事实：旧自动执行已取消；最新完成94/73包，95是第74个启动包、未验收且默认OFF。34父项关闭0，六项首阶段5/6是限定成果。80同Sakura1000 SetPass约1995→5、中央CPU DrawMesh约2；Role历史实局有收益但未默认推广。91/93实际scope有效0分配和95 Manual/headless资格GC FAIL分别保留，不能彼此覆盖。

工作规则：
保留所有dirty/未跟踪内容；不reset/checkout/clean/stash、删除或覆盖无关内容，不push。脚本前Task/Change，文件替换/恢复前Operation及真实dirty备份；所有新增状态/缓存声明owner、容量、失效与11阶段关闭位置。
不读活跃Q06方法体；若当前任务确需，准确请求范围。不要改Scene/Prefab/资源/ProjectSettings/Input Actions/Server/Gen/Plugins或解冻PERF/ATLAS/MONO/EXT-1，除非用户另有明确授权。
输入/RNG/checksum、33ms正常tick、pass/消费顺序、publication只读、透明segment/fail-closed、热路径容量和0GC保持。AI降频/疏散是独立模式提案，不偷偷实施。
修改前写出最小必要验证，运行获批的原项目Editor时先核安全状态，不启第二个同Library实例、不打断他人Play或保存dirty Scene。编译、focused、实景、正式权威、设备证据分开报告。
每个新实验先说明要解答什么以及将如何改变决定。无收益具体候选结束采用；不重复同版长窗刷PASS，不为每个诊断步骤拆包，不扩展无关全量回归。必要正确性/0GC门不能为了赶进度取消。
同窗父子计时不能相加；不能从均值推出P95，也不能把GC.Alloc次数当回收停顿或CPU DrawMesh当GPU batch。当前输入和late的优化收益没有固定承诺。
完成后更新单项方案评审卡、唯一进度对应项、必要依赖和Change Record，运行适用ChangeLedger validator。报告实际命令、通过/失败/未执行、证据复用理由及下一项。区分候选准入、Windows H07阶段通过与Android最终认证。
