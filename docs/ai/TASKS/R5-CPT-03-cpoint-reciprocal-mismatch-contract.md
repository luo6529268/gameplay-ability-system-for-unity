# R5-CPT-03 — CPoint reciprocal mismatch control-flow contract

> 建立日期：2026-08-22  
> 状态：RUNTIME_PENDING — 最小 writer、focused fixture、Unity compile与full self-check已通过；C++ trace / Play Mode待验。  
> 对应差异：D-CPT-003  
> Change ID：R5-CPT-003

## Goal

使 Unity active reciprocal-mismatch CPoint path与 C++ release 一致：
attacker raw fallback frame=0 后跳过 decrease/actions，但不错误跳过 CPoint throw tail 和 dircontrol；
且 mismatch throw 从 fallback current frame 0 读取 geometry/next。

## Authority / Evidence

- release build：J:/QQFile/NTSD2.4/ntsd_release/Makefile:20-21；
- live CPoint loop：src/entity/cpoint.cpp:20-172；
- step10 ordering：src/entity/game_tick.cpp:659-664；
- Unity mapping：BattleCpointWriter.RunKind1 / ApplyThrow / ApplyDirControl；
- raw frame wait contract：R5-CPT-001；
- phase owner contract：R5-CPT-004。

## Scope

允许文件：

1. Assets/NTSD/Scripts/Simulation/Ecs/BattleCpointWriter.cs
2. Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs

允许生产符号仅为 BattleCpointWriter.RunKind1 和其 existing ApplyThrow callsite参数选择。

## Required behavior

1. missing/inactive victim保留 raw frame0 immediate return；
2. active reciprocal mismatch或 invalid victim previous CPoint：raw frame0并只跳过 decrease/action；
3. mismatch不得执行 escape、action frame、attacking clear；
4. throwvx nonzero 时仍执行 throw tail，且从 fallback current frame读取 geometry/next；
5. no-throw、attacking=2、dircontrol有效时仍转向；
6. raw fallback不清 FrameWaitCounter；
7. 不改 valid relation、D-CPT-005 decrease-negative escape tail、kind2 validation、CPoint injury owner、global stats、throw transform semantics、
   pass order或任一 scope 外模块。

## 已写实现

- RunKind1 对 active mismatch 保留 raw frame0，但用 local skipActions / skipDecrease替代 direct return；
- missing victim仍保持 existing immediate return；
- mismatch throw明确传入 attacker 当前 fallback frame data；
- focused fixture覆盖 reciprocal throw、invalid previous throw、dircontrol-only、negative-decrease skip与
  existing missing-victim boundary；
- valid relation escape branch没有修改，继续仅由 D-CPT-005 负责。

## 实际验证

- UnityMCP scripts refresh 后 filtered C# compiler error：0；
- full BattleRuntimeSelfCheck：PASS，结果文件时间为 2026-08-22 09:59:58；
- D-CPT-003 focused mismatch matrix在 full self-check 内通过；
- D-CPT-005 valid-relation escape tail仍是待处理静态差异，不能被本次 PASS关闭；
- C++ release trace / real Play Mode仍未取得，最高状态维持 RUNTIME_PENDING。

## Verification

| 层级 | 验收 |
|---|---|
| S0 source | reread all relevant C++ branches与 Unity all callsites。 |
| S1 focused | reciprocal throw、invalid-prev throw、dircontrol-only、negative-decrease skip、missing-victim return矩阵；锁定frame/prev2/position/velocity/attacking/hitcount/FWC。 |
| S2 governance | Record、ledger、STATE、full diff、main plan、handoff；validator/scoped diff。 |
| S3 Unity | current Editor scripts refresh后的 C# error 0；full BattleRuntimeSelfCheck PASS。 |
| S4 honesty | only RUNTIME_PENDING；C++ trace / real Play Mode继续待验。 |

## Stop conditions

- source显示 mismatch tail还需改 throw transform、kind2 validation、pass order、other CPoint writer或
  scope 外 module；
- focused fixture显示 fallback frame data在当前 adapter无法最小选择；
- compile/self-check不能在列明两文件修复；
- 需要修改、运行、构建或写入 C++ authority。

## Out of scope

R1 trace、R2-R4、D-CPT-002、D-CPT-004、held/link/opoint、input/collision/render、server/lockstep、
performance、Android、T8 default stage asset、physical input与完整 Play Mode认证。
