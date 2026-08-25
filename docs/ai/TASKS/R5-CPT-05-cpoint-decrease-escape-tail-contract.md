# R5-CPT-05 — CPoint valid decrease-negative escape tail contract

> 建立日期：2026-08-22  
> 状态：RUNTIME_PENDING — 最小 writer、focused fixture、Unity compile与full self-check已通过；C++ trace / Play Mode待验。  
> 对应差异：D-CPT-005  
> Change ID：R5-CPT-005

## Goal

使 Unity valid CPoint decrease-negative escape在保留既有 escape writes后，继续执行 C++ required
throw/dircontrol tail，并以 fallback current frame0作为 throw geometry/next source。

## Authority / Evidence

- release participation：J:/QQFile/NTSD2.4/ntsd_release/Makefile:20-21；
- CPoint writer：src/entity/cpoint.cpp:60-172；
- step14 consumer：src/entity/game_tick.cpp:666-684；
- Unity mapping：BattleCpointWriter.RunKind1 / ApplyThrow / ApplyDirControl、
  SimulationWorld.FramePostProcessAll；
- adjacent independent contracts：R5-CPT-001、R5-CPT-003、R5-CPT-004。

## Scope

允许文件：

1. Assets/NTSD/Scripts/Simulation/Ecs/BattleCpointWriter.cs
2. Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs

## Required behavior

1. valid escape仍写 existing attacker0/victim181/hit count/knockback；
2. escape不再直接 return；只阻止 action selection；
3. throwvx nonzero时，tail在 current fallback frame0上执行；
4. no-throw且 attacker attacking=2时，dircontrol仍执行；
5. hit count保留至 existing FramePostProcessAll，由该pass消费；
6. raw frames不清 FrameWaitCounter；
7. 不改 mismatch path、other CPoint branch、postprocess implementation或 scope 外模块。

## 已写实现

- RunKind1 valid decrease-negative escape保留frame0/181、hit count与knockback；
- 将escape direct return替换为existing skipActions=true和useFallbackFrameForThrow=true；
- existing throw tail因此从attacker current fallback Frame.D读取geometry/next；
- no-throw path继续进入existing ApplyDirControl；
- focused fixture更新为验证throw immediate、step14 hit-count消费与no-throw dircontrol；
- R5-CPT-003 mismatch和FramePostProcess implementation均未修改。

## 实际验证

- Unity 2022.3.62f3 Editor日志记录Tundra build success，未检出error CS；
- request-file驱动full BattleRuntimeSelfCheck，结果文件于2026-08-22 16:09:37为PASS；
- escape+throw immediate、step14 hit-count消费、no-throw dircontrol assertions均在full self-check内通过；
- C++ release trace与真实Play Mode未取得，最高状态维持RUNTIME_PENDING。

## Verification

| 层级 | 验收 |
|---|---|
| S0 source | reread escape / tail / postprocess order与 Unity all callsites。 |
| S1 focused | throw immediate/postprocess、no-throw dircontrol、no-throw neutral、action skip、FWC matrix。 |
| S2 governance | Record、ledger、STATE、full diff、main plan、handoff；validator/scoped diff。 |
| S3 Unity | current Editor scripts refresh C# error 0；full BattleRuntimeSelfCheck PASS。 |
| S4 honesty | only RUNTIME_PENDING；C++ trace / real Play Mode继续待验。 |

## Stop conditions

- source或fixture要求改 throw transform body、FramePostProcess、pass order、mismatch path、kind2 validation
  或其它 module；
- current adapter无法用 Frame.D精确表达 fallback current frame；
- compile/self-check无法在列明两文件内修复；
- 需要修改、运行、构建或写入 C++ authority。

## Out of scope

D-CPT-002/003/004、R2-R4/R6+、R1 trace/comparator、server/lockstep、performance、Android、T8、
physical input与完整 Play Mode认证。
