# R6-PRES-05 — hit-record render writeback 合同

> 建立日期：2026-08-22  
> 状态：`CODE_WRITTEN`  
> 对应：`D-RENDER-002`  
> Change ID：`R6-PRES-005`

## Goal

保证每个C++等价RenderDispatch tick都恰好推进/回收一次hit-record，不依赖Unity LateUpdate、实际GPU
帧或是否构建presentation。

## Scope

唯一允许脚本：

1. `Assets/NTSD/Scripts/Simulation/Presentation/BattlePresentationShadowBuild.cs`
2. `Assets/NTSD/Scripts/Simulation/NTSDBattleTickSystem.cs`
3. `Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs`

## Required behavior

- publication tick：capture旧age后、RenderDispatch返回前应用existing frozen cycle一次；
- CentralOnly no-publication tick：用runtime lifecycle catalog执行相同valid-age/tail规则；
- invalid non-tail不删不增；每tick最多移除当时一个invalid tail；
- unavailable lifecycle不推进；
- Late/worker finalizer幂等，不重复写；
- pass顺序、snapshot command、RNG/collision writer、checksum schema不改。

## Protected boundaries

CentralOnly/Texture2DArray/dynamic Mesh/URP、1.5 scale、fixed-world camera、扩展容量、30Hz/
FrameInputSet、SoA/ECS、pool/worker/0-GC、T8暂缓和C++只读全部保持。

## Verification

1. source consumer/writer inventory；
2. no-publication + worker-publication age/expiry/idempotence matrix；
3. unavailable lifecycle control；
4. fresh Unity compile 0 error；
5. full `BattleRuntimeSelfCheck` PASS；
6. validator/scoped diff；PlayMode/C++ trace未取得时最高`RUNTIME_PENDING`。

## Stop conditions

- 需要改lockstep checksum/schema、worker ownership、pass order或GPU resource architecture；
- cycle resource在immediate apply后无法由published frame安全持有；
- first difference指向collision/RNG或spark resource定义本身；
- 需要修改scope外脚本。

## Out of scope

R7性能、camera、D-RENDER-001、shader/pixel、Android、T8、C++ executable。
