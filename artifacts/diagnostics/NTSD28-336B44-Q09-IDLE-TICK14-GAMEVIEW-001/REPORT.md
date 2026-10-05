# Q09 鸣人／鼬静置画面：原 Editor 单例（2026-10-05）

## 裁决

原 Unity Editor 的 Battle Scene 已完成一次定向 Play，取得鸣人 OID2／鼬 OID9 的合成 Game View。探针先等待生产 World 就绪（全局 tick 5），重置两角色帧、位置、HP/MP、输入状态、战斗时钟及 RNG，再提交 14 个无按键的完整生产 Driver tick；结果是全局 tick 19、**重置后相对 tick 14**。正式根 EXE 的对照图是启动后全局 tick 14，二者标题可见字段均为 action3、双方 HP500/MP200、源 X500/620、Z400。两图都可见两角色本体与足下阴影；在项目背景、固定完整取景、HUD/角色名的用户例外之外，本单例没有发现可定位的角色缺图或阴影缺失。

这不是严格的全局同 tick / 全 World 一致性证明：进入可控重置之前的 5 个 Unity tick 是否留下未重置的其它 World 副作用，报告未逐字段覆盖；两端分辨率和相机/背景也不同，不能做逐像素相等判定。`CAPTURED_FORMAL_TITLE_FIELDS_MATCH` 仅指报告中明确检查的标题字段，不等于 Q09 或战斗总目标完全对齐。当前无可复现、非例外的表现首差，不据此修改生产渲染或扩展角色矩阵。

## 原件与观察

- 当前正式 EXE：SHA-256 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3`。[正式根 1280×720 PNG](../NTSD28-336B44-Q09-TICK-BOUND-CAPTURE-20261005-v3/formal-paused-client.png) SHA-256 `96DA6CF6E9EC4675984D981EF9108A0B4E3A5073D29C0CB02F2AC69794DC3450`；其 [REPORT](../NTSD28-336B44-Q09-TICK-BOUND-CAPTURE-20261005-v3/REPORT.md) 记录真实 D3D11 窗口暂停 tick14、双方 action3/HP500/MP200。
- [Unity 原始 JSON](idle-tick14-336b44-20261005-053238-349034.json)；[Unity 1920×1080 合成 Game View](idle-tick14-336b44-20261005-053238-349034.png) SHA-256 `049F7811527E6AC13C6BD26E0E6DDCF7DE53E59B89CEECF228308C8603A3FC20`。探针在原项目唯一 Battle Scene 上运行，未启动第二 Editor，未保存 Scene。内容根为 `Assets/NTSD/Content/LoganRuntime`；本例 14 行输入均为 0，源 X500/620 与 Z400、HP500/MP200 逐行稳定。相对 tick 1～3/4～7/8～11/12～14 的两角色动作依次为 0/1/2/3；RNG 标量在记录的 14 tick 内均为 state `3374725112`、calls `3000`。
- Unity 画面发布 tick、像素计划 tick、截图完成后逻辑 tick 均为 19，截图过程逻辑暂停。正交相机 size 5.76、aspect 16:9，Game View 1920×1080。不同背景、固定视野及普通 HUD/角色名按已批准项目例外排除；图像只做角色本体、相对站位、足下阴影的有限可见性核查。
- 原 Editor 经项目已安装的 MCP for Unity 桥接执行 `refresh_unity(mode=force, scope=all, compile=request)`；Unity `Assembly-CSharp-Editor.dll` 更新时间晚于探针源码。项目无 `com.unity.pipeline`，Unity CLI 仍不能连接，但不影响这次 MCP 桥接。工作树外的本地 `unity-mcp/Server` 源码含意外插入日志、启动时报 Python `SyntaxError`；本次以发布的 `mcpforunityserver==9.6.8` 启动 MCP 客户端连接既有 Editor，没有覆盖脏源码或更改项目包。
- 探针报告 `exitedPlay=true`、`sceneCleanAfter=true`；MCP 退出后再次读取唯一 Battle Scene 为 `isDirty=false`。Scene 文件运行前/后及独立复核 SHA-256 均为 `253B2EBAD322AA6EC18488DFBBF555BB93AD897940A342E4130E77CE778F9010`。请求文件位于忽略的 `Temp/NTSD28_Q09_IdleTick14.request.json`，已因同 runId 结果存在而不能再次触发；未删除或覆盖它。

## 后续边界

本次只完成 Q09 保留队列的一例静置画面取证。若后续发现具体非例外角色本体、阴影或战斗效果差异，先使初态与 World 时钟同态、比较首个不同 tick/字段，再开对应共用 owner 的最小修复；不自动重跑全角色/全技能，也不把普通背景、相机和 HUD 差异计为战斗首差。Q09/Q12 和总目标仍开放。
