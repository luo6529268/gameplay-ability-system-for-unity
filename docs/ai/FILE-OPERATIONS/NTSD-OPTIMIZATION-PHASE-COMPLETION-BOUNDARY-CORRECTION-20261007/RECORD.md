# NTSD-OPTIMIZATION-PHASE-COMPLETION-BOUNDARY-CORRECTION-20261007

状态：VERIFIED（仅文档执行边界与保留状态验证，不是优化收益或新优化子批）；以下执行计划保留原事前PLANNED内容。

需求来源：用户于2026-10-07明确要求“这种停止目标的问题，都需要处理”，并更正“边界不应该是停止目标的边界，应该是判断该阶段是否完成的边界”。授权本轮统一优化文档中的执行、验收和停止口径，不授权运行时代码或新增测量。

执行者：root。工作区：I:/GitHub/Unity_GAS/gameplay-ability-system-for-unity。
开始UTC：2026-10-07T08:58:27.0419879Z。HEAD：45bbed41c64e0599601fa0df4028072d9f83303a。
准确九文档写域、各操作前SHA-256/字节/Git状态、十五非写域保护SHA见[before.json](before.json)。操作前将九份当前内容Copy-Item至本Operation的backup/<原路径>并核同，现有dirty内容不得以HEAD替代。

拟变更：
- 执行合同当前第3—8节统一“阶段验收未达继续；次数只触发复盘；无效候选停止推广而非全局停Goal”；原次数停止条款与旧确认快照保留为HISTORICAL / SUPERSEDED。
- 唯一进度总表区分限定报告/评估交付、实际阶段完成、父项关闭、平台Goal真实状态；修正POST40将局部未批准推广和未知分配点推导为全目标不能继续的解释。
- 风险登记、共同启动门、H07/H11方案同步恢复入口和阶段完成条件；STATE/handoff仅追加修订恢复指针。
- 不改变六项选定范围、33/3ms/max2、正式336B44权威、0GC/容量、publication/segment/fail-closed、十一阶段关闭和原专项授权门；不自动推广Role-aware，不解冻EXT1/ATLAS/MONO/其余28项。

文件操作顺序：apply_patch创建Record和before.json → 核验当前SHA后Copy-Item九份备份 → apply_patch登记INDEX的PLANNED → apply_patch修订准确文档 → 静态条款/链接、git diff --check、Change Ledger、保护SHA/备份/HEAD检查 → 追加after.json及最终结果。所有编辑均用apply_patch；不删除、移动、还原或覆盖未声明路径。
本Operation自有Record/manifests/backup为新增留痕路径，不另建代码Change ID。

重要状态：平台Goal最后一次工具返回blocked，这是历史真实事实；其“无范围内安全动作”的全局阻塞解释不足，本次予以更正。文档更新不伪造Goal恢复，也不创建替代Goal、标complete或执行目标状态操作。旧Goal摘要中的8/1/2/3次数停止语义已被用户新要求取代，后继恢复必须以当前合同为准。
H07实际性能未达；H11完整camera严格0GC仍FAIL、两个事件调用点UNKNOWN。评估交付已完成的H06/M13/M14不因本轮文档整理重开。未知调用点先是范围内诊断待办，不自动等同新增授权阻塞；具体后继Task需事前声明准确路径/必要采集及共享Editor安全窗口，不凭猜测修复。

本轮不修改C#、shader、Scene/Prefab、资源、ProjectSettings、Server、PERF/ATLAS/MONO/EXT1正文；不读取活跃Q06方法体；不启动Unity、测试、Profiler/GPU capture/M0或其他测量；不执行破坏性Git/push。
验证只证明文档语义与保留状态，不证明运行时收益。恢复来源为上述九份现状备份；恢复仍须新授权和独立Operation，本轮不恢复。

执行结果（2026-10-07）：
- 九份声明文档已修订：当前规则统一按阶段验收判断；旧次数上限原文、原执行快照、失败和授权沿革保留为HISTORICAL / SUPERSEDED。执行合同第2节红线与操作前逐字核同。
- 主总表新增六项阶段条件/当前判定/下一必要动作；限定产物5/6、阶段条件通过4/6、父项关闭0分别记录。H07性能FAIL、H11完整camera严格FAIL未降低，Role-aware/EXT1/ATLAS/MONO等原门不改变。
- 新增本地文档链接15个均可解析；10项合同静态检查PASS，原历史快照及旧次数条款保留检查PASS。git diff --check声明九文档exit0，仅既有LF/CRLF提示。
- pwsh -NoProfile -File Tools/Validate-ChangeLedger.ps1 -RepositoryRoot I:/GitHub/Unity_GAS/gameplay-ability-system-for-unity：PASSED，1336 Records、4份既有governed code diff均覆盖、4298历史warning。本轮未改该四代码。
- 九份backup SHA与事前manifest核同；十五保护路径（含四代码及其meta/Q06、PERF/ATLAS/MONO/EXT1、权威/AGENTS/Ledger/版本/依赖）SHA保持；HEAD保持45bbed41c64e0599601fa0df4028072d9f83303a。最终after.json记录完整快照。
- 未修改C#/Scene/Prefab/资源/ProjectSettings，未读取Q06方法体；无Unity/测试/Profiler/M0测量、Goal状态操作、破坏性Git或push。未声称优化阶段完成/运行时收益。

工具观察失败与修正（不抹除）：初次合并读取九文档过长，输出截断导致JSON解析失败，改为精确文件/片段读取；进度表首次patch因hunk顺序错误校验失败，未写入，按行序重排后成功；一次静态diff命令的对象ToString写法语法失败，修正后exit0。最初用Windows PowerShell 5运行validator，分别遇PSScriptRoot默认值空和Git CRLF warning被当terminating error；未改validator/规则，改用已配置pwsh及显式RepositoryRoot后通过。第一次warning计数正则未包含缩进，已重计为4298，非0 warning。上述都不作为性能失败归因或关闭条件。

最终manifest观察补记：首次将Git状态/差异/所有SHA合并返回时输出预算不足导致JSON末尾截断；未写入after.json。改为精确字段和足够输出预算后重新核得九备份/十五保护/HEAD保持、diff exit0，已创建after.json；没有凭截断结果宣称通过。本Operation新增审计文件不改变九文档最终SHA。
