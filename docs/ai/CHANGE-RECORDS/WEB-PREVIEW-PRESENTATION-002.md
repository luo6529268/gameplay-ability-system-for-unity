# WEB-PREVIEW-PRESENTATION-002 — DatSkillFlow 主预览的高频表现采样

<!-- CHANGE-RECORD
id: WEB-PREVIEW-PRESENTATION-002
status: RUNTIME_PENDING
code-path: Tools/DatSkillFlowWeb/src/client/main.ts
code-path: Tools/DatSkillFlowWeb/src/client/preview-renderer.ts
code-path: Tools/DatSkillFlowWeb/src/client/render-cadence-sampler.ts
code-path: Tools/DatSkillFlowWeb/tests/unit/render-cadence-sampler.test.ts
code-path: Tools/DatSkillFlowWeb/tests/unit/preview-renderer.test.ts
code-path: Tools/DatSkillFlowWeb/tests/unit/client-project-contract.test.ts
code-path: Tools/DatSkillFlowWeb/tests/unit/project-open-lifecycle.test.ts
authority: User explicit request dated 2026-08-30 to inspect Codex thread 019ff015-6f9c-7652-8c40-034b476b1c7a and improve the current DatSkillFlowWeb preview presentation using the local NTSD 2.8 DAT/render project as reference; presentation-only extension of WEB-CADENCE-001.
evidence: USER_AUTHORIZED / CODE_WRITTEN / BUILD_PASS / FOCUSED_23_OF_23_PASS / UNIT_315_PASS_1_SKIP / NONBUILD_INTEGRATION_78_OF_78_PASS / LEDGER_PASS / PRESENTATION_ONLY / DAT_AND_NATIVE_CLI_UNCHANGED / BROWSER_E4_PENDING
-->

> 创建日期：2026-08-30  
> 当前状态：`RUNTIME_PENDING / USER_AUTHORIZED / PRESENTATION_ONLY / BROWSER_E4_PENDING`

## 1. 需求与参考证据

- 用户要求读取 Codex 会话 `019ff015-6f9c-7652-8c40-034b476b1c7a`，参考其本地 NTSD 2.8 DAT、独立预览源码与本地项目表现实现，修改当前 `Tools/DatSkillFlowWeb` 的预览区域表现。
- 已只读检查 `J:\QQFile\NTSD2.8.3.3 zip\NTSD2.8.3.3\battle_scene_reverse\web28\public\app.js`、`ntsd28_core/render_snapshot.*` 与 `ntsd28_playable/presentation_interpolation.cpp`。
- 2.8 `dat_skill_flow_28_app` 的 `preview-renderer.ts` 与当前 2.4 工具除换行符外相同；可移植价值来自独立 render snapshot 与本地 120Hz presentation contract，而不是 2.8 Frame 范围。
- 2.8 本地表现合同：previous/current 相邻快照；精确位置；同 identity/relation；motion/teleport fail-closed；camera 与同实体的 sprite/shadow/nameplate 共用表现 delta；frame、生命周期、碰撞与规则状态保持当前 Tick 离散值。
- 当前项目已有 `WEB-CADENCE-001` 的只读 30/60/120 sampler，但主编辑器仍以 33ms timer 离散推进；本 Change 由用户新授权把该能力安全接入主预览，不改变 `WEB-CADENCE-001` 的历史事实。

## 2. 改前事实

- 主预览从 `project.nativeTicks[tickIndex]` 直接绘制，播放时每 33ms 递增一次 `tickIndex`。
- `render-cadence-sampler.ts` 已能做基础同-lineage位置插值，但优先使用整数位置，尚未应用 2.8 的 relation、adjacent tick 与 motion discontinuity gates。
- `drawPreviewCanvas` 以同一 Tick 同时绘制 sprite 与 DAT overlay；若直接传入插值 Tick，会把表现位置误当作碰撞/编辑位置。
- 预览轴线始终显示，缺少当前表现频率与 interpolation alpha 的明确标识。

## 3. 允许范围

- 主预览增加 `30 / 60 / 120Hz` 表现选择，并使用 `requestAnimationFrame` 进行一次表现时刻一次 Canvas render。
- 采样器优先使用 Native Trace 的精确 `x/y/z`，以 velocity 建立 2.8 同类 motion continuity gate；同 lineage、相邻 Tick、relation 连续时才插值。
- camera、角色与阴影共享同一采样 Tick；frame/pic/facing/spawn/despawn/hit/opoint/DAT wait 保持当前 Native Tick 离散值。
- overlay、坐标轴、站位拖动和几何编辑继续绑定离散 authority Tick；编辑/暂停态必须显示精确 Tick。
- 更新主预览 HUD/样式和 focused tests。

## 4. 禁止与不可回退边界

- 不修改 DAT、sidecar、`native/dat_preview_cli.cpp`、Native build、服务端保存/API、Unity runtime、C++ release authority、30Hz battle logic 或对象生命周期。
- 不把 NTSD 2.8 的 `0..998` Frame 范围、DAT 规则或战斗行为混入当前 2.4 工具。
- 不把 `ntsd_cpp` Native preview 或浏览器插值写成 `ntsd_release` gameplay parity 证据。
- 不覆盖或清理当前工作树中的用户修改和既有 `dist` 构建产物。

## 5. 验收标准

1. 30Hz 模式逐 Tick 离散显示且最终 Tick 可达。
2. 60/120Hz 模式只对相邻、同 lineage、relation 连续且 motion 连续的实体位置及 camera 插值。
3. slot reuse/新生实体、holder/link/target 切换、非相邻 Tick 与 teleport/reset 不产生 ghost interpolation。
4. frame/pic/facing/lifecycle 等离散字段来自 current Native Tick；输入 trace 不被修改。
5. 编辑 overlay 以 authority Tick 定位；暂停、拖动站位与几何编辑不显示中间逻辑位置。
6. focused sampler/renderer/client contract tests 与 build 通过；运行 Change Ledger validator。
7. 浏览器 E4 若继续被 localhost 权限拒绝，则状态保持 `RUNTIME_PENDING`，不得声明视觉验收完成。

## 6. 回滚方式

- 移除主页面表现频率控件与 `main.ts` 的 presentation sampling 接线。
- 恢复 `drawPreviewCanvas` 的单 Tick 输入和 sampler 的原实现。
- 回滚不需要恢复 DAT、Native trace、服务端状态、Unity 资源或 C++ 二进制。

## 7. 实际修改与验证

- `index.html` 增加 30/60/120Hz 表现选择和实时 `Native Tick / alpha` HUD；默认 120Hz，30Hz 可随时回到离散 snapshot。
- `main.ts` 以 `requestAnimationFrame` 驱动单次完整 Canvas 表现提交；播放时调用 sampler，暂停、seek、step、站位拖动与 Frame/动作切换时清除中间采样并回到 authority Tick。
- `render-cadence-sampler.ts` 新增 main playback sampler，并按 2.8 参考实现增加相邻 Tick、lineage、holder/link/target 与 velocity motion continuity gates；位置优先使用精确 `x/y/z`，frame/lifecycle 保持 current Tick。
- `preview-renderer.ts` 分离 `tick` 与 `authorityTick`：sprite/shadow/background 使用表现 Tick，DAT overlay、axis、position handles 使用 authority Tick；默认不再常驻绘制十字轴。
- `styles.css` 增加场景边框、阴影、宽屏放大与 cadence HUD；未改变 Canvas 内部 794x550 authority viewport。
- 修复两个主页面源码合同测试的 CRLF 函数边界匹配，使 Windows 工作树不会把真实函数误判为空。

### 验证

- `npm run build`：PASS；最终 build `20260830084617618-18ef901e469444d9b80e355a62838458`，145 files。
- focused sampler/renderer/main/open tests：`23 passed / 0 failed`。
- 全部 unit tests：`315 passed / 0 failed / 1 skipped`；唯一 skip 是未显式提供外部 DAT corpus。
- 非构建 integration tests：`78 passed / 0 failed`；覆盖 Native preview、真实补丁 OID 466、project API、只读模式、Windows handle-safe 文件事务、安全保存、server 与 verified loader。
- manifest/server focused integration：`25/25 PASS`。
- built `main.js`、`preview-renderer.js`、`render-cadence-sampler.js` 的 `node --check`：全部 0。
- `git diff --check`：PASS。
- `Tools/Validate-ChangeLedger.ps1`：PASS，114 records / 32 governed code diffs covered。
- 未运行 `tests/integration/build.test.ts`：该既有测试会在真实 `dist` 发布新 build，并已知在 Windows 并发 manifest 读取时偶发 `EBUSY`；本次已通过真实 build、manifest integrity 和 server tests，不把跳过写成全量 `npm test` PASS。
- 未完成浏览器 E4：localhost 浏览器权限已在当前会话拒绝，未重试、换浏览器或绕过。
