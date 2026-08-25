# R6-PRES-02 — shadow current DAT identity 合同

> 建立日期：2026-08-22  
> 状态：`RUNTIME_PENDING`（fresh compile/full self-check PASS；PlayMode/C++ trace待验）  
> 对应：`D-RENDER-004`  
> Change ID：`R6-PRES-002`

## Goal

让Unity CentralOnly shadow `{223,224}` gate读取与C++ `char_data->oid`等价的
`CurrentDatObjectId`，不改变其它render或battle行为。

## Scope

唯一允许脚本：

1. `Assets/NTSD/Scripts/Simulation/Presentation/BattlePresentationShadowBuild.cs`
2. `Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs`

## Required behavior

- shadow gate只改identity字段；
- body仍用VisualDataId解析sprite；
- shell ObjectId不被改写；
- P7 inverse identity fixture按current DAT裁决；
- checksum、sort、command order、state/link/hit-stop gate不变。

## Protected boundaries

CentralOnly、Texture2DArray、dynamic Mesh、URP、1.5 visual scale、fixed-world camera、
MobileExtended/DesktopExtended、30Hz/FrameInputSet、SoA/ECS、pool/worker/0-GC均不改。

## Verification

1. source/static grep；
2. full self-check中的P7 identity matrix；
3. Unity force scripts compile 0 error；
4. full self-check PASS；
5. ledger validator与scoped diff check；
6. 最高RUNTIME_PENDING，C++ trace/PlayMode待验。

## Stop conditions

- 需要改snapshot schema、body resource、identity writer或gameplay；
- 发现223/224有另一个C++ shadow exception；
- checksum发生变化；
- 测试暴露scope外first difference。

## Out of scope

D-RENDER-001/002/005、camera/perspective、spark、resource fail-closed、GPU像素、T8、Android、性能。

## Actual result

- shadow gate改读CurrentDatObjectId；P7三条inverse identity预期已按C++ current DAT更新；
- Tundra build success 6.02s，Assembly-CSharp 18:16:56，0 error CS；
- 18:18:10 full self-check PASS；
- 未做PlayMode/C++ trace，故状态保持RUNTIME_PENDING。
