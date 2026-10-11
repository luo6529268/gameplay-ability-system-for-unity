# NTSD-H01-STRATEGY-AND-SCOPE-20261010

状态：PLANNED / DOCUMENTATION_ONLY。
开始：2026-10-10T10:06:46.420416+08:00。执行者：当前Codex方案讨论会话；工作目录：I:\GitHub\Unity_GAS\gameplay-ability-system-for-unity。

用户授权原文：“所有优化处理，都按照优化方案文档来处理，所有额外发现的可优化方向，一律不管”；“接下来，我们来处理第一项，然后我们先确认具体的优化策略”。

精确范围：before-manifest.json中的11份已有文档（含本索引），新增本Task，以及本Operation的审计文件。仅记录范围、更新当前导航、整理H-01待确认策略；不修改代码/资源/设置，不删除/移动文件，不运行Unity/测试/构建，不读取活跃Q06方法体，不发送任务、不恢复监测/Goal。

Task：../../TASKS/NTSD-H01-STRATEGY-AND-SCOPE-20261010.md。Change：N/A，未改项目脚本。
真实备份：before/中的执行前字节及逐文件SHA-256；HEAD不能代替dirty备份。保存HEAD、Git状态、working/staged binary diff；17份具名保护文件见manifest，不声称全仓字节审计。

拟执行API：functions.exec -> tools.exec_command，PowerShell单引号here-string传入Python stdin。精确正文/替换串/目标清单已先保存edit-plan.json；每个替换必须唯一命中，保留原文件换行与历史主体。先更新计划所列10份正文及新增Task，再核对身份/链接/diff，最后追加本Record、索引和Task收尾。执行命令另存execute-command.txt，验证命令另存verify-command.txt。

预期：OPTIMIZATION_PLAN_ONLY已记录；H-01为当前第一项、STRATEGY_REVIEW_PENDING；路线仅为提案，不批准H-08/ATLAS/H-10及Android设置；旧Role/FindEffect成果及producer暂缓/OFF、95 OFF保持，H06-ROLE-EXACT不再续办。
回滚：依据before/复核本项增量；不自动整文件恢复，不覆盖后续用户工作，任何撤销仍遵守操作审计合同。

## 文档验证与链接修正计划

执行prepare/execute命令均exit0；10份正文及Task已写，未改代码。verification-01发现新Task的一个相对链接层级错误，其他28链接有效；diff-check0、11备份和17保护文件完整、HEAD/暂存及范围外tracked状态不变。原结果保留。修正仅本轮新Task的H-01链接../../Assets为../../../Assets，原字节保存task-before-link-fix.md，SHA-256 c835e851b89106db3197d2f5bc3e7f64a496de03d6e4f7dfa85e041fee4c829e；准确API/命令fix-link-command.txt已先保存。

## 执行后追加：VERIFIED（仅文档操作）

结束：2026-10-10T10:09:00.013540+08:00。prepare、execute、链接修正和两次验证命令均exit0。首次验证发现本轮新Task单个相对链接层级错误，修正后29个新增/修改本地链接全部有效，失败观察未覆盖。

verification-02.json证明：11份真实备份完整；17份具名保护文件0变化；10份正文增量严格匹配edit-plan，历史主体保持；HEAD/暂存不变、范围外tracked无变化，diff-check0。实际触及before-manifest列出的11份已有文档、新Task及本Operation审计文件。没有删除/移动文件、修改项目脚本/资源/Scene/Prefab/Settings/Packages、读取活跃Q06方法体，未调用Unity/测试/Profiler/构建，未发送执行消息或改变自动监测。

当前状态：OPTIMIZATION_PLAN_ONLY已记录，H-01策略草案交付、STRATEGY_REVIEW_PENDING；不是实施通过或H-01关闭。未运行Change Ledger validator，本任务没有项目脚本改动。精确执行/验证命令、SHA与最终范围复核均在本目录保存。
