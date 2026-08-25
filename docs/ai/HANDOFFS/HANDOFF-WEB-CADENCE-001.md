# HANDOFF — WEB-CADENCE-001 isolated read-only render cadence comparison

> 日期：2026-08-23  
> 状态：`RUNTIME_PENDING`

## User authorization

用户已明确选择方案 1：在 `Tools/DatSkillFlowWeb` 中建立独立、只读的 NTSD 2.4.1 三档渲染帧率对比入口。

## Goal

- 选择角色技能后运行一次现有 Native preview trace；
- 同时展示 30 / 60 / 120 Hz 三个 Canvas；
- 只插值 presentation position/camera/attachment-friendly float data，永不改变 frame、DAT wait、命中、opoint 或逻辑 state；
- 用户能通过有位置变化的技能比较离散与 60/120 Hz 纯视觉平滑的差别。

## Isolation contract

- 正常 `index.html` / `main.ts` 编辑器默认流程不改；
- 新入口为 `render-cadence.html`，新宏以只读 server mode 启动；
- 只读 mode 拒绝所有 edit/save/sidecar/workspace write routes；
- 不修改 DAT、sidecar、C++、Unity 或 resources；
- 当前 trace provider 是既有 `ntsd_cpp + NTSD 2.4.1`，不得写成 `ntsd_release` authority evidence。

## Active Change Record

`WEB-CADENCE-001 / RUNTIME_PENDING`：
[Change Record](../CHANGE-RECORDS/WEB-CADENCE-001.md)

## Required evidence before handoff completion

1. **完成**：sampler unit tests 覆盖 wait/frame 离散、30/60/120 alpha、same-lineage interpolation、spawn/despawn/slot-reuse no-blend；
2. **完成**：build manifest 已服务新 HTML/module/style；
3. **完成**：真实 HTTP 测试验证 `open/preview/close` 可用，mutation 返回 `403/read-only-mode`；
4. **待用户视觉验收**：在浏览器选择一个确有位置变化的技能，确认三栏都画出同一 trace，60/120 只平滑坐标且 frame/wait/opoint 不提前；
5. **部分完成**：默认编辑器代码没有被本包修改；全量 `npm test` 目前392 pass，剩余两个既有 `main.ts` 静态正则失败需单独处理；
6. **完成**：`Tools/Validate-ChangeLedger.ps1` 通过（78 records；本 Change 的 15 个 governed code path 全覆盖）。
