# NTSD-H07-INPUT-COST-DIAG-20261010
Operation ID: NTSD-H07-INPUT-COST-DIAG-20261010
状态：COMPLETED_WITH_DIAGNOSTIC_LIMITS
类型：诊断Editor脚本定点增量及文档覆盖；新证据独立写入
Task: docs/ai/TASKS/NTSD-H07-INPUT-COST-DIAG-20261010.md
Change: NTSD-H07-INPUT-COST-DIAG-001
授权：用户当前直接授权：那就先进性检测处理，确定哪些部分，这部分你来处理就行。仅H07-INPUT检测，不修改生产优化逻辑。
执行者：当前讨论会话 Codex；工作目录：I:\GitHub\Unity_GAS\gameplay-ability-system-for-unity
开始：2026-10-10T11:26:22.846892+08:00
准确覆盖与新增清单：before-manifest.json；真实dirty备份：before/，全部哈希已核对。
拟执行：定点编辑Suite，新增H07InputCostDiagnostics Editor-only；本地TCP6402 get_editor_state/execute_code/refresh_unity/run_tests/get_test_job/execute_menu_item；一次4窗combat1000 OFF ON ON OFF，各120warm+180sample；生产路径不写。每窗将覆盖既有SharedResult，已保存原件，Suite会另存每窗terminal-before。SharedRequest须不存在，禁止删除未知请求。
不读活跃Q06方法体；不改Scene/Prefab/资源/设置；不恢复producer/95/H06成本/旧Goal。
Owner：原Suite+唯一完整Driver GC observer，新增observer组合GC与逐tick采集；冷分配300行，采集在Driver计时/GC之外。退出/异常恢复输入边界开关，detach observer，沿原11阶段关闭。
回滚：只撤本次增量，逐字节真实dirty备份为依据；需另记Operation，禁止覆盖外部新改动。
范围外保护：artifacts/diagnostics/NTSD-H07-INPUT-COST-DIAG-20261010/protected-before.json，15562文件。

## 实际操作与恢复

两Editor文件定点修改和新文件meta由Unity导入生成；其余仅上述Task/Change/进度/授权文档及独立artifact。真实运行一次首OFF120+180；因可靠Driver steady2event停止，后三窗只预写request，未运行。Suite覆盖共享terminal前已另存report.json.shared-terminal-before.txt，原shared terminal亦在before备份；没有删除旧历史。输入开关及observer恢复、十一阶段关闭、Menu clean8。15562范围外SHA256无漂移。源文件/文档实际前后hash及新增清单见after-manifest.json（清单自身不自包含），结果REPORT/validation.txt。未用Git恢复或覆盖别人的工作；不做自动回滚。
