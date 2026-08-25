# WEB-CADENCE-001 — NTSD 2.4.1 只读三档技能渲染帧率对比入口

<!-- CHANGE-RECORD
id: WEB-CADENCE-001
status: RUNTIME_PENDING
code-path: Tools/DatSkillFlowWeb/src/client/render-cadence-main.ts
code-path: Tools/DatSkillFlowWeb/src/client/render-cadence-sampler.ts
code-path: Tools/DatSkillFlowWeb/src/server/cli-args.ts
code-path: Tools/DatSkillFlowWeb/src/server/cli.ts
code-path: Tools/DatSkillFlowWeb/src/server/server.ts
code-path: Tools/DatSkillFlowWeb/scripts/start-local.ps1
code-path: Tools/DatSkillFlowWeb/tests/unit/cli-args.test.ts
code-path: Tools/DatSkillFlowWeb/tests/unit/render-cadence-sampler.test.ts
code-path: Tools/DatSkillFlowWeb/tests/unit/render-cadence-readonly-contract.test.ts
code-path: Tools/DatSkillFlowWeb/tests/unit/render-cadence-client-contract.test.ts
code-path: Tools/DatSkillFlowWeb/tests/integration/client-build-manifest.test.ts
code-path: Tools/DatSkillFlowWeb/tests/integration/project-api.test.ts
code-path: Tools/DatSkillFlowWeb/src/diagnostics/envelope.ts
code-path: Tools/DatSkillFlowWeb/tests/unit/diagnostics.test.ts
code-path: Tools/DatSkillFlowWeb/tests/unit/launcher-contract.test.ts
authority: USER-approved scheme 1; existing DatSkillFlowWeb NTSD 2.4.1 native-preview trace contract. This is a presentation-only diagnostic and does not assert C++ release gameplay parity.
evidence: Tools/DatSkillFlowWeb/scripts/build-native-preview.ps1 currently compiles against J:/QQFile/NTSD2.4/ntsd_cpp and dat_preview_cli.cpp defaults to J:/QQFile/NTSD 2.4.1; existing client main.ts consumes nativeTicks and preview-renderer.ts renders those snapshots.
-->

> 创建日期：2026-08-23  
> 状态：`RUNTIME_PENDING`  
> 用户授权：用户明确选择“方案 1 做独立只读渲染对比入口”。

## 1. 修改前状态

- `Tools/DatSkillFlowWeb/index.html` 是完整 DAT 编辑器，包含字段编辑、结构修改、sidecar 与 DAT 保存路径；
- 现有 `main.ts` 的单 Canvas 以一个离散 `nativeTicks[tickIndex]` 渲染；其播放器每 33 ms 推进逻辑 tick；
- 当前 Native preview adapter 使用 `J:\QQFile\NTSD2.4\ntsd_cpp` 与 `J:\QQFile\NTSD 2.4.1` 资源根；
- 现有页面没有 30 / 60 / 120 Hz 并列呈现、也没有纯表现位置插值；
- 当前仓库 battle authority 是 `J:\QQFile\NTSD2.4\ntsd_release`，但该独立 Web 诊断入口不得把现有 `ntsd_cpp` trace 伪称为 release parity 证据。

## 2. 允许改动

- 新增独立 `render-cadence.html` 与只读 client entry，不改正常编辑器默认页面；
- 新增纯函数 cadence sampler：三个 pane 共用同一 Native trace，仅对同 lineage 的连续表现位置、显示 Z、render offset 和 camera X 进行渲染层插值；
- Sprite `frameId/pic`、facing、对象出生/销毁、命中、opoint、DAT wait 与所有战斗状态保持离散的 Native snapshot 值；
- 新增独立启动宏和只读 server flag；只读模式拒绝 edit、edit-batch、edit-structure、save、skill metadata save 和 workspace/document write routes；
- 新增最小单元/构建合同测试与使用说明。

## 3. 禁止与保护

- 不改 `Tools/DatSkillFlowWeb/index.html`、现有 `src/client/main.ts` 的默认编辑器流程、现有 DAT 编辑器的默认启动语义；
- 不修改 DAT、sidecar、C++ 工程、native preview source、资源、Unity `Assets`、场景或战斗 runtime；
- 不将浏览器插值回写 `nativeTicks`、Native trace、逻辑位置、速度、frame、命中、碰撞或对象生命周期；
- 不声称此工具是 `ntsd_release` 的 gameplay authority trace；页面必须明确标为“NTSD 2.4.1 Native Preview / Presentation Comparison”。

## 4. 预期行为与验收

- 选择一个有位移的角色技能后，只生成一次 Native trace；30/60/120 三个 Canvas 使用同一 tick、frame、对象集合；
- 30 Hz 只显示离散 snapshot；60/120 Hz 在前一与当前同 lineage snapshot 间插值，并明确显示一逻辑 tick 的表现延迟；
- 新出生、销毁、slot/OID/lineage 变化实体不跨实体插值；
- 无论渲染率，frame/pic/facing/opoint/命中状态在对应逻辑 tick 相同；
- 只读启动时编辑/保存 API 被服务器拒绝，正常启动模式保持原行为；
- 运行 focused sampler tests、build、现有 test suite 中适用部分，并对至少一个有角色位置变化的 Native skill trace 做运行时三栏验证；
- 仅在用户拥有对应刷新率显示器时才把 120 Hz 视为物理显示验收；60 Hz 屏幕上只能验算 120 Hz 采样逻辑，不宣称人眼 120 Hz 观感已验收。

## 5. 回滚方式

- 新入口、采样器、专用样式、启动宏和只读分支均为独立文件/显式 flag；
- 默认编辑器路径不通过该 flag，不依赖新采样器；
- 若出现回归，只禁用/移除独立比较入口及其只读启动参数，不触及 DAT、Native trace 或正常编辑器保存链。

## 6. 实际实现

- 新增 `render-cadence.html`、专用 CSS 和 `render-cadence-main.ts`；默认 `index.html` 与 `main.ts` 未改；
- `render-cadence-sampler.ts` 将 30 Hz 固定为离散 Native snapshot；60/120 Hz 以一逻辑 tick 的表现延迟，在同 lineage 前后 snapshot 间插值 `x/y/z`、`displayZ`、`renderOffsetX` 和 `cameraX`；
- 帧号、pic、facing、wait、命中、opoint、spawn/despawn、slot/OID/lineage 切换保持当前离散 snapshot，未做预测或跨实体混合；
- server 增加显式 `--read-only`，并拒绝 workspace/document 写、project edit/batch/structure/save 与 skill sidecar save；`project/open`、`project/preview`、`project/close`、catalog 与 assets 保持可用；
- 新增 `read-only-mode` 诊断码，避免被 schema 当作未知码而意外转换为 500；
- 新增 `一键启动-渲染帧率对比.cmd`，以 `-Mode Test -ReadOnly -OpenPath /render-cadence.html` 启动隔离的 LocalAppData 测试副本。

## 7. 实际验证

- `npm run build`：通过；最新构建发布 145 个文件，manifest 包含新的 HTML、client module、sampler 和 CSS；
- focused tests：48/48 通过，覆盖 cadence sampler、client isolation、CLI、diagnostic schema、launcher、build allowlist 以及 project API 的真实 HTTP `open → preview → close` / mutation reject；
- 本机真实只读服务（`ntsd_cpp + NTSD 2.4.1`）：`render-cadence.html` HTTP 200、三栏标记存在；catalog 有 308 个 type-0 角色；OID 2 的 `open → 16-tick preview → close` 均 HTTP 200；`/api/project/edit` 返回 `403/read-only-mode`；
- 全量 `npm test`：392 passed、2 failed、1 skipped。两个失败均是既有普通编辑器 `main.ts` 的静态正则契约：`client-project-contract.test.ts` 未接受现有 `function renderFields(): void` 形式，`project-open-lifecycle.test.ts` 未接受现有 `async function open(...): Promise<void>` 形式；它们不读取本 Change 的新入口，也不影响本 Change 的 focused/runtime evidence。未改用户既有编辑器代码或借机修改其无关测试；
- `Tools/Validate-ChangeLedger.ps1`：通过（78 records；本 Change 的 15 个 governed `.ts`/`.ps1` 路径均已覆盖）；
- 未运行浏览器 Canvas 的人工视觉验收；浏览器自动化在当前会话不可用。仍需要用户选择一个确有位置变化的技能，并实际观察三栏。

## 8. 当前状态与回滚

- 当前为 `RUNTIME_PENDING`：代码、构建、focused 以及真实 Native HTTP 生命周期已完成；最终三栏视觉验收仍待；
- 未修改 C++、Unity、DAT、sidecar、资源或正常编辑器逻辑；
- 回滚只需移除独立入口/launcher/只读分支，不需要恢复任何 DAT 或战斗数据。
