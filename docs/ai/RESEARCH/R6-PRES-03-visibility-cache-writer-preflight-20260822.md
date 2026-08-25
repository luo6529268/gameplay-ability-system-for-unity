# R6-PRES-03 — Central visibility cache writer 预检

> 日期：2026-08-22  
> 状态：`RUNTIME_PENDING`（difference fixed；fresh compile/full self-check PASS）  
> 对应：`D-RENDER-005`，并补足 `D-RENDER-004` 的 production cache 链  
> Change ID：`R6-PRES-003`

## 1. C++ authority

- release `src/render/renderer.cpp:517-556` 的 shadow 只读取当前 `char_data`、frame、
  `link_state`、`hit_stop` 和当前 `char_data->oid`；没有独立 `ShadowVisible` 字段；
- `renderer.cpp:558-685` 的 body 只读取 `hit_stop`、当前 DAT/frame/pic、sprite range/resource、
  position/facing 和 render offset；没有独立 `EntityVisible` 字段；
- `render_world` 只把 `active` entity 送入上述两个函数。

因此 Unity 的 `EntityVisible/ShadowVisible` 只能是 Unity-native 表现缓存或资源适配，不能覆盖
C++ 已有的 current-DAT/frame/link/hit-stop/OID 裁决。

## 2. Unity writer inventory

### `EntityVisible`

production writer 已收敛为：

1. `LF2Sprite.Initialize/ResolvePicManagedOnly`：初始化或成功解析有效 sprite 时为 true；
2. `LF2ObjectRenderer.UpdateSprite/UpdateCentralManagedSpriteState`：无效 frame/resource 不生成有效
   body descriptor，恢复有效 pic 时重新置 true；
3. `Destroy`、world reset、pool reset：写 false 后对象同时注销、pending 或归池，中央 capture 已由
   active/handle/pending gate 排除；
4. `LF2LivingObject.ProcessEffects` / legacy death blink：当前 exact-character DataOriented production
   frame-tick 不调用该 legacy TU 路径；weapon/special/other 的 production `SimTU` 也走各自 release
   frame-advance 路径。现有直接 `Hide()` 中仍在 active 且具有有效 C++ descriptor 的情形只存在测试/诊断夹具。

结论：当前 production writer inventory 未发现一个独立可达的 `EntityVisible=false` 状态会覆盖
C++ body gate。保留字段供 Legacy/诊断使用，但不得把这项静态结论扩大为未来所有 writer 都安全；
新增 production `Hide()` writer 时必须重开 D-RENDER-005。

### `ShadowVisible`

production `LF2ObjectRenderer.SimLateTick/ForceRefreshPresentation` 调用
`LF2Entity.UpdateShadowManagedState()`；Legacy route 调用 `UpdateShadow()`。两者都把 shell
`ObjectId` 传给 `ShouldHideShadowForPresentation`。

这与 C++ current `char_data->oid` 不同，而且会在 `BuildCommands` 的 current-DAT gate 之前通过
snapshot `ShadowVisible` 再次拦截命令。故 `R6-PRES-002` 的 direct gate 修复尚未闭合 production
cache 链。

## 3. Confirmed first difference

- shell `ObjectId=223` / current DAT `7300`：C++ 应画 shadow；旧 Unity managed shadow cache写 false，
  即使 BuildCommands current-DAT gate允许，仍被 `ShadowVisible` 拦截；
- shell `ObjectId=7300` / current DAT `223`：C++ 不画；direct gate已经正确拒绝；
- shell `ObjectId=224` / current DAT `7300`：同第一项，应画但旧 cache写 false。

existing P7 fixture只构造 snapshot/default LF2Sprite 状态，没有先执行 production managed shadow
writer，因此未覆盖这条 first difference。

## 4. 最小修复

1. `LF2Entity.UpdateShadow()` 与 `UpdateShadowManagedState()` 均向 helper传入
   `ResolveCurrentDataObjectId(this)`；
2. helper参数改名为 `currentDataObjectId`，不改变 frame/link/hit-stop逻辑；
3. P7 在 RenderDispatch 前对三条 identity case执行 production managed shadow writer，并断言
   snapshot `ShadowVisible` 与 current DAT一致。

不删除 visibility schema，不改变 body gate、central command order、Legacy ownership、resource、mesh、
shader、camera、gameplay或 C++ authority。

## 5. Acceptance / evidence boundary

- production-like managed writer后，shell223/current7300与shell224/current7300的 `ShadowVisible=true`
  且各有一个 shadow command；shell7300/current223的 `ShadowVisible=false`且无command；
- state3005/state9997/link/hit-stop/common-shadow/order/checksum assertions不变；
- fresh Unity compile、full self-check、ledger validator与scoped diff通过；
- C++ trace、真实 Play Mode/GPU像素仍待，最高状态 `RUNTIME_PENDING`。

## 6. Actual evidence

- two writer callsites与helper参数已改为current DAT identity；
- 首次18:31:32 self-check暴露fixture `Sprite=null`，该失败及诊断已写入Change Record；
- 三条identity case绑定rendererless catalog sprite后，test source `18:32:36` < DLL `18:33:37`；
- Tundra build success 2.66s、6 items updated、filtered compile errors=0；
- 18:35:48 full self-check=`PASS`；PlayMode/C++ trace/GPU像素未验。
