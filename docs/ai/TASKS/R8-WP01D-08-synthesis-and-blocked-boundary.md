# R8-WP01D-08 — central rendering certification synthesis and blocked boundary

> 日期：2026-08-23  
> 状态：`COMPLETE AT AVAILABLE EVIDENCE / FULL CLOSURE BLOCKED`

## Verdict

WP01D-01～07已经完成当前资源可提供的source、catalog、GPU、Game和SceneView证据；production中央渲染
没有剩余可实施的confirmed first-difference。WP01D不能写为完整VERIFIED，因为两项必要证据不可取得，
但它们也不授权修改DAT或绕过C++只读边界。

## Completed evidence

- state8000通用writer已由HitStun140纠正为`RenderPicOffset=140`，raw pic999保持隐藏；
- C++ DAT `row`固定横向列，Unity不再用BMP物理尺寸交换row/col；
- declared-range与partial source clip已对齐；
- all-loaded-DAT：5537 catalog entries、6674 authored frames、23 clipped、0 descriptor/binding差异；
- GPU source→binding：84,327,319 pixels/hash相同、0差异；
- Game：3 snapshots→6 commands→1 draw，实体可见；
- SceneView：4 commands/1 segment，isolated 575 non-clear pixels，hash `C292967D753744C2`；
- production、scene、URP asset、DAT/BMP、Legacy owner和C++ authority均未因认证修改。

## Blockers

### B-R8-WP01D-08-01 — no authored state8000 source

当前完整loaded-DAT审计确认`authoredState8000FrameCount=0`。因此无法构造“真实已加载DAT触发state8000”
的production Play witness。现有synthetic contract、catalog、GPU、Game与Scene证据不能伪造这个缺失样本。
恢复条件：用户以后提供包含authored state8000且属于正式战斗数据的资源；不得为测试修改DAT。

### B-R8-WP01D-08-02 — C++ full trace unavailable

R1-WP02 full C++ trace保持BLOCKED；WP01D不能用Unity像素证据代替C++ executable trace。恢复条件沿用
R1-WP02，不得运行、hook、patch或向authority目录写入。

## Boundaries

- WP01D当前停止，不再新增probe或修改render；
- D-RENDER-006保持`RUNTIME_PENDING / MAX AVAILABLE S4`；
- WP01E/F/G与本两项blocker无依赖，可以继续；
- 本状态不等于完整渲染或完整战斗对齐。
