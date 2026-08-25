# R6-PRES-03 — Central visibility cache writer 合同

> 建立日期：2026-08-22  
> 状态：`RUNTIME_PENDING`（fresh compile/full self-check通过；PlayMode/C++ trace待验）  

## Actual result

- 两个production shadow cache writer已改读current DAT identity；
- 首次P7因fixture无LF2Sprite失败已保留，补rendererless binding后fresh Tundra 2.66s、0 error；
- fresh DLL 18:33:37，full self-check 18:35:48 PASS；
- PlayMode/C++ trace未运行，不能声明完整R6或runtime已对齐。
> 对应：`D-RENDER-005` / `D-RENDER-004` production cache closure  
> Change ID：`R6-PRES-003`

## Goal

让 Unity Legacy 与 Central managed shadow cache 都使用 C++ current `char_data->oid` 等价字段，
避免 shell identity 通过额外 `ShadowVisible` gate覆盖正确的 C++ shadow结果。

## Scope

唯一允许脚本：

1. `Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs`
2. `Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs`

## Required behavior

- `UpdateShadow` 与 `UpdateShadowManagedState` 使用 `ResolveCurrentDataObjectId(this)`；
- frame null、state3005/9997、negative link、223/224和hit-stop阈值/闪烁逻辑不改；
- P7必须先执行 production managed writer再capture/build；
- `EntityVisible/ShadowVisible` schema保留，Legacy renderer能力不删除；
- body command、snapshot identity、sort/order、checksum不变。

## Protected boundaries

不改 CentralOnly/Texture2DArray/dynamic Mesh/URP、1.5 visual scale、fixed-world camera、
MobileExtended/DesktopExtended、30Hz/FrameInputSet、SoA/ECS、pool/worker/0-GC、gameplay、DAT、scene或C++。

## Verification

1. source/static writer inventory；
2. P7 inverse identity production-cache matrix；
3. fresh Unity scripts compile 0 error；
4. full `BattleRuntimeSelfCheck` PASS；
5. ledger validator和task-scoped diff check；
6. C++ trace/PlayMode未取得时最高 `RUNTIME_PENDING`。

## Stop conditions

- 需要删除visibility schema、改body gate、snapshot layout、command order或resource owner；
- 发现production `EntityVisible=false` 独立于C++ descriptor gate可达；
- 需要修改 gameplay、pass ordering、C++、scene或已批准adapter；
- checksum或scope外fixture发生first difference。

## Out of scope

D-RENDER-001/002、body resource、camera/perspective、spark writeback、GPU像素、T8、Android、性能。
