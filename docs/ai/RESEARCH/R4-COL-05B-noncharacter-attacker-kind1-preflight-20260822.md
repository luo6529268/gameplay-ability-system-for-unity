# R4-COL-05B — non-character attacker / weapon kind1 reachability preflight

> 日期：2026-08-22  
> 类型：只读 C++/Unity source 与已部署 DAT 容器调查；本包未修改 Unity / C++ gameplay。  
> 唯一 authority：`J:\QQFile\NTSD2.4\ntsd_release` 的 release live source。  
> 关联差异：`D-COL-005` 的 non-character attacker、kind1 selector 与 weapon pickup 子范围。  

## 1. 结论

本次只读调查确认了两个不能混淆的事实：

1. C++ `kind == 1` 的 consume 语义是通用 Entity 抓取；C++ 的武器拾取写入在 `kind == 2` 和
   `kind == 7`，不是 `kind == 1`。
2. C++ 正式 battle callback 只对 `char_data->obj_type == 0` 的活动实体执行
   `prepare_ai_input` / `apply_input`。因此，`kind == 1` selector 虽直接读取 attacker 的
   `key_right` / `key_left`，却不能仅据此断言普通非角色实体会在正式输入链获得方向键。

Unity 当前将 non-character 的方向键读取限制为 `LF2Character`，同时将 weapon `kind1` 路由为
pickup。这两点组成了潜在语义偏差，但本次没有可读的 C++ release DAT `itr` 内容来证明一个
non-character `kind1` attacker 在正式资产中可达。故本项状态为：**源码语义已核实；资产可达性
UNKNOWN；不修改 Unity gameplay。**

## 2. C++ release 合同（VERIFIED）

`Makefile:11-35` 将 `src/core/main.cpp`、`src/entity/collision_collect.cpp` 与
`src/entity/collision.cpp` 编入正式 `ntsd_new.exe` release build。

### 2.1 正式输入回调是 character-DAT-only

`src/core/main.cpp:5505-5523` 在 `simulation_tick_driver.step_one_tick(...)` 的
post-cooldown input callback 中：

```cpp
for (int i = 0; i < MAX_OBJECTS; i++) {
    Entity& e = world.objects[i];
    if (!e.active || !e.char_data || e.char_data->obj_type != 0) continue;
    if (e.ai_controlled) input.prepare_ai_input(e, world, world.input_phase);
    input.apply_input(e, world.camera_x, &world);
}
```

这证明正式 human/AI 输入 producer 不直接为 type1/2/3/4/5/6 数据对象写 key fields。对
`src/entity/` 的非 `.bak` 源码只读搜索也未发现对象生成、物理或 collision path 对这些 key
fields 的赋值；现有 entity-side 命中均是 consumer/read。

### 2.2 kind1 selector 与 consume

- `src/entity/collision_collect.cpp:200-220`：`kind == 1` 先按距离筛选，再用
  `atk.key_right/key_left` 与 `x_int` 判定朝向；此 selector 未给 attacker 加 `obj_type` gate。
- `src/entity/collision_collect.cpp:276-277`：只有 kind3、kind8 明确拒绝 non-character target。
- `src/entity/collision.cpp:921-993`：`case 1` 清双方 `vx`、写 facing/raw frame/位置/
  caught-catcher relation/duration/fall，是 generic Entity consume。
- `src/entity/collision.cpp:996-1030` 的 `case 7` 与 `1032-1081` 的 `case 2` 才包含
  weapon pickup/link writer。

因此，“weapon kind1 = pickup”不是 C++ `case 1` 的 source contract。

## 3. 已部署 DAT 观察边界（UNKNOWN）

`src/core/main.cpp:57-60` 与 `1259-1263` 指向实际游戏根目录
`J:\QQFile\NTSD2.4\data\data.txt`。该清单确认存在 type1/2/3/4/5/6 non-character entries，
例如 oid120 `chars\\weapon4.dat`、oid434 `chars\\rasengan_ball.dat`。

但实际 `chars/*.dat` 为 Visual Data Changer（VDC）编码容器：`weapon4.dat` 的可读开头仅为工具
banner，余下字节不是可静态读取的 DAT field 文本。本包未运行、复制、解码、重建或修改 C++ release
runtime，也没有把非 authority parser/解码器作为 C++ 行为证据。因此无法确认：

- 是否有正式 non-character DAT 含 `itr kind: 1`；
- 该 ITR 是否会在正式 scene/opoint/lifecycle 中可达；
- 若可达，它是否通过任何非 `post_cooldown_input` 的 authority writer 取得方向 key。

这些都是 **UNKNOWN**，不能由 Unity、旧 C#、诊断注释或手工构造 runtime fields 填补。

## 4. Unity 现状（VERIFIED）

- `BruteForceSceneQuery.AcceptReleaseKind1TowardVictim:6198-6211` 使用
  `IsRightPressed/IsLeftPressed`；两 helper 在 `6242-6254` 仅接受 `LF2Character`。
- `LF2WeaponInteractionResolver.Resolve:81-96` 将 weapon `kind1` 路由到
  `LF2WeaponBase.HandlePreInteractionKind1`。
- `LF2WeaponBase.HandlePreInteractionKind1:673-712` 要求 target 为 `LF2Character`，并调用
  `Pick/HoldWeapon`，即 Unity pickup flow，不是 C++ generic `case 1` grab writer。

这不是本包授权修改的理由：现有 C++ source 不能证明该 weapon/non-character attacker path 在正式
资产和输入路径中可达。05A 已关闭的仅是 character attacker 对 non-character **target** 的 common
writer gate，不能被扩展为 weapon attacker 行为结论。

## 5. 后续打开条件与最小范围

仅当取得不修改 authority 的以下一种证据时，才可新建 implementation Task Contract：

1. 可重复的现有 C++ release 观察证据，显示 non-character kind1 attacker 的 slot/frame/keys/
   candidate/consume；或
2. 可只读解析且可证明与 release 载入一致的正式 DAT asset evidence，显示可达的 non-character
   `itr kind:1`，并在 source 中闭合其 key producer；或
3. 用户提供明确的 C++ release 场景与最短复现，足以在不改 authority 的前提下定位该 path。

未来包必须分别决定：selector 的 runtime-key source、weapon `kind1` 应走 generic grab 还是应被
正式 assets 排除、以及现有 pickup `kind1` 是否其实应收敛为 kind2/7。不得把这些判断合并为一次
weapon/held 大改。

## 6. 本包停止条件与后续

- 本包未修改 Unity gameplay、fixture、C++、DAT、资源、场景或任何 C++ executable；
- `D-COL-005B` 的可达性保持 `UNKNOWN / no gameplay change`，而非 `RUNTIME_PENDING` 或已对齐；
- 不再对该缺少可读 asset evidence 的分支反复静态搜索；
- 按 D-009 自动转入 `D-HIT-001` 的 C++ source preflight。该项有明确 C++ / Unity writer 差异，
  不依赖本包的 UNKNOWN。
