# NTSD 优化文档重基线任务

Task ID：`NTSD-OPTIMIZATION-DOC-REBASELINE-20261006`
状态：`DOCUMENT_REVIEW_PASS / IMPLEMENTATION_WAITING_USER_APPROVAL`
用户授权：“那就开始整理吧，然后整理没问题后，告诉我，我来告诉你是否可以开始”。
范围：仅更新优化主表、独立方案、PERF/ATLAS/MONO/提案导航及本任务留痕。
不是代码Change，不加入active Change Ledger，不重开旧对齐任务。

## 目标与保护

34项统一高/中/低优先级、当前进展和证据；新增项各有解决方案、验收、测试、
依赖/决策/回滚；原历史记录保留。保护所有用户dirty内容，尤其UI/阴影任务。
不改C#/shader/Scene/Prefab/资源/ProjectSettings/Packages/Server，不启动Unity、
测试、Profiler/Frame Debugger/GPU capture/M0。原USER_HOLD不解除；EXT-1仍待批。

## 交付与恢复入口

- [主登记表](../../../Assets/NTSD/Docs/android-mobile-readiness-priority-risk-register.md)
- [共同合同、调整证据与启动门](../../../Assets/NTSD/Docs/battle-optimization-rebaseline-and-start-gates-20261006.md)
- [文件操作逐项before清单](../FILE-OPERATIONS/NTSD-OPTIMIZATION-DOC-REBASELINE-20261006/RECORD.md)

具体文档范围及恢复源由Operation Record声明。
已有干净文档可从固定commit精确路径读回；dirty INDEX有字节备份。
回滚需用户批准，采用审查后的逆向apply_patch，不reset/checkout/clean。

## 验收与实际结果

计划：文档ID/优先级/状态/链接/章节完整性、git diff --check、
before/after范围及保护哈希检查；不执行项目测试。
实际结果在完成后追加，不预填通过。
实施与测量启动由用户下一次批准决定。

### 2026-10-06 实际文档核对

- 主表与34份独立方案统一：高12、中14、低8；原28个ID/路径保留，新增6项，
  M-03调高但不改ID。方案、验收和测试条件留在独立文档。
- 首轮静态核对42份Markdown、127个本地链接，ID/优先级/状态/方案章节错误0；
  后续收口结果见 [validation.json](../FILE-OPERATIONS/NTSD-OPTIMIZATION-DOC-REBASELINE-20261006/validation.json)。
- 首次diff空白检查发现新引入的行尾空格，已用apply_patch修正；重跑成功。
  `git -c core.safecrlf=false diff --check -- <本任务33个原有业务文档路径>`仅对本命令设置只读Git选项，不修改配置。
- EXT-1第七节至文件末尾UTF-8正文SHA-256操作前后相同，未升格；
  本轮未启动专项M0。预算数值/设备门槛未冻结，待测假设不改写成实测。
- 本任务未修改脚本、Scene/Prefab、已有资源、ProjectSettings、Packages或Server；
  已登记的4个dirty脚本保护哈希在核对时一致。其他任务的现有工作保留。
- 新文档对应的6个.md.meta在工作树中出现：仅观察并保留，未修改/删除；
  生成进程来源未确认，本任务未调用Unity或import/refresh。
- 不运行项目编译、测试、Unity、Profiler、GPU capture、M0或真机测量；
  无脚本改动，不运行ChangeLedger代码验证器，也不登记活跃代码Change。

本任务的PASS只代表文档静态一致性与范围核对通过，不代表优化实现、
战斗表现或Android性能通过。当前停在等待用户批准，不自动进入下一批。
