# HANDOFF — R5-LIFE-01B pending/free/generation/render logic adapter

> 日期：2026-08-22  
> 状态：`RUNTIME_PENDING`  
> 类型：no-code adapter certification

## 已完成

1. 闭合C++ `free_entity` immediate inactive、next-spawn reset、render-before-late-opoint合同；
2. 闭合Unity PendingFlushDestroy、generation invalidation、old-object pool finalization与CentralOnly capture映射；
3. 确认production `FirstPresentationTick`只有Reset=0，late opoint的T+1首次可见来自pass顺序；
4. 新登记D-LIFE-001：C++ merge partner inactive与Unity OidMergeDormant占low slot的结构差异；
5. 完整扫描release live battle allocator后，确认partner只在0..19，而stage从20、opoint/effect从50开始；当前判`INFERRED safe adapter`；
6. 没有修改任何C#、shader、scene、prefab、DAT或C++ authority文件。

## Fresh verification

- UnityMCP force scripts refresh/compile request：成功，domain reload断线后恢复ready；本包无C# diff，Assembly-CSharp仍为17:14:38；
- focused EditMode job `582b9e9212264d39b4377b72d7e0374d`：19/19 PASS；
- full self-check：2026-08-22 17:49:18 `PASS`；
- compile日志与后续运行未见`error CS`/`Compilation failed`；
- 上述证据不等于C++ runtime trace或真实Play Mode。

## Reopen conditions

- production出现非零FirstPresentationTick writer；
- battle-time allocator开始写0..19；
- merge partner域扩大到20以上；
- old finalization可破坏same-slot newborn generation；
- Play Mode或未来trace出现slot/ObjectCount/first-visible/split first difference。

## 下一包

R5普通lifecycle/cursor/opoint自动证据层已收口。下一步应进入R6的第一个独立source-contract包，先检查
C++ renderer active/Z/slot painter order与Unity CentralOnly command descriptor/order；不得借R6回退Legacy、
修改1.5 visual scale、fixed-world camera、Texture2DArray/dynamic Mesh/URP或扩展容量。
