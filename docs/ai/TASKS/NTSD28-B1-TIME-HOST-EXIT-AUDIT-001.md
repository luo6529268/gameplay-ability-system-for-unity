# Task Contract — NTSD28-B1-TIME-HOST-EXIT-AUDIT-001

> 状态：`VERIFIED / B1_CURRENT_PRODUCTION_READY / B2_READY / GOVERNANCE_ONLY`  
> 所属计划：`NTSD28-UNITY-BATTLE-REALIGNMENT-001 / B1`  
> 建立日期：2026-09-03

## 目标

对B1时间与Host全部证据作退出审计，判定当前正式生产路径是否满足进入B2的前置条件，同时把
OS实体键、inactive worker和旧R8 request场景等未覆盖项放到准确恢复点，防止上下文压缩后误报。

## 审计结论

- 权威source chain：33ms/3ms、debt cap2、pause debt reset、paused F2 exactly-one、running F2
  drop、F5 reset及Host loop/render accumulator独立性已闭合。
- Unity current production：精确0.033/0.003；每Update最多排空2 interval；F1/F2/F5独立edge
  latch；pause、single-step、cadence reset均在production Driver。
- focused：Host7/7，related31/31；parity tool 6/12/21/5，build0/0，Unity compile0。
- real Play synthetic Input System production path：Normal32.71883ms、Fast3.88752ms、ratio0.118816，
  pause/F2/F5通过；报告SHA `DAC9D1...C0ED`。早一轮独立报告`07364B...B95F`同样通过。
- 旧R8 poller no-request副作用已隔离；request存在后的历史完整探针未重跑，但不再影响普通Play。
- worker当前因`unity-presentation-bindings-are-still-attached`正式ineligible，failure为空，不是live path。
  B9若解除binding，必须先回到B1完成worker two-interval/33/3ms trace。
- OS实体键未自动化；临时虚拟Keyboard实际走production`Keyboard.current`/Driver而非diagnostic seam。
  OS人工验收保留B12，不阻塞当前逻辑阶段进入B2。

结论：B1当前正式生产路径可进入B2；不代表B2～B12或未来worker分支已对齐。

## 修改范围

治理文档 only，不修改脚本、资源、Scene、Prefab、ProjectSettings或权威目录。

## 回滚

如发现B1新first-difference，将本审计追加`SUPERSEDED`并链接新B1 Change；不得删除现有证据。
