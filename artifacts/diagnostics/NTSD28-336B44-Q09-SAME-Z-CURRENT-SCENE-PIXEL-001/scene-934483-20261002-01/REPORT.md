# Q09/P-04 当前 336B44 原 Battle Scene 同 Z 像素限定验收

状态：`VERIFIED_SCOPED_CURRENT_SCENE_CENTRAL_GPU`。正式根 `NTSD2.8-Logan.exe` 本轮 SHA-256 为 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3`。其 playable `render_snapshot.cpp` 的同深度实体绘制排序为物理槽位降序；当前 Unity CentralOnly radix 同 Z run 保持对应顺序。此包只验当前原 Battle Scene 的自然完整 tick GPU 遮挡，不证明正式 EXE 像素、Legacy、其它深度组合或整场表现。

当前原 Scene 磁盘 SHA-256 为 `93448372834A1BEAF2C9ACD90E2EF17E2EA487E19F601B9815A907A974D7BF60`。原 Editor 2022.3.62f3 的 Unity MCP 本地桥报告非 Play、Scene clean 后，在同一 Scene 版本分别执行 `baseline/a/b/both` 四次独立 Play。每次均用正式 OID120/121 按相同顺序占物理槽51/52、generation1，生产 Driver 从 tick5 完整推进至 tick6，frame1/pic1、Z240；起始共享 RNG state/calls 为 `3126301419/5`，原生 CRT state `3878484156`。四份结果均为 `PASS_CAPTURE`，均通过自身 pause、World 对象/槽位、Renderer/logic pool 清理与 Scene SHA 前后相同检查，随后退出 Play。结束时 Editor idle、非 Play、Scene clean；Battle/Menu/GameConfig/ProjectBattleModeConfig 四项保护 SHA 4/4 不变。

`baseline` 把两武器放 X1400，`a` 仅将 OID120 放 X1100，`b` 仅将 OID121 放 X1100，`both` 两者均放 X1100；未选武器仍存在并占原槽位。实际 CentralOnly 命令中 OID121/slot52 的 body index1 先于 OID120/slot51 的 body index3。投影 body 重叠范围按底原点为 x[1000,1069)、y[550,606)；四张 1920×1080 PNG 的上原点行是 y[474,530)。图像已独立打开检查。

四图交叠范围的非白像素分别为 463/536/635/708，故不能使用旧的“baseline 必须全白”或 exact-color-only 探针字段。独立分析遍历**全部**满足 `A!=U`、`B!=U`、`A!=B` 的 82 个像素；其中 `U=baseline`、`A=a`、`B=b`、`C=both`。分别对 `C=A+t(B-U)`（OID120 后绘制）和 `C=B+u(A-U)`（相反顺序）做 0～1 有界最小二乘拟合，以 RGB 各通道最大残差不超过 1 作为符合条件。82/82 符合 OID120 后绘制，最大残差 0.267；相反模型仅 38/82 符合。44 个像素仅支持 OID120 后绘制，0 个仅支持相反顺序，38 个两模型均可解释，0 个两模型均不符。77 个合成像素与 A 精确相等，其余 5 个由边缘混色模型解释。示例 PNG 坐标 `(1035,502)`：U `(130,130,130)`、A `(66,66,66)`、B `(102,102,93)`、C `(52,52,47)`；模型残差约 0.267 对 2.667。全部计算值、原始 JSON/PNG SHA 和保护 SHA 在 [pixel-analysis.json](pixel-analysis.json)。

为保护旧证据，Editor 测试探针只增加安全 run ID 和版本化输出目录，且写前拒绝覆盖；原受控分支、生产、DAT、图片、Scene、相机 enabled 状态和非战斗代码未改。原 Editor 刷新后 Editor 程序集时间晚于源码，当前 Scene 重新进入 idle；生成 `Assembly-CSharp-Editor.csproj` 的 `dotnet msbuild` exit0，Unity Console 本轮未见 C# 编译错误。四个本任务临时请求在创建前将原文与 SHA 存入 `requests/` 和 [request-lifecycle.json](request-lifecycle.json)，由探针在每次 Play 自动删除；结束时请求不存在，未手动删除任何文件。旧版四张 PNG 的已公布 SHA 4/4 不变。原始失败和旧版证据未覆盖或改写。

Change Ledger validator 最终 exit0/PASSED，1137 Records、13个当前代码 diff 全覆盖；`git -c core.safecrlf=false diff --check` exit0。原 Editor Console 只读 `error CS` 过滤返回0项。生成工程编译存在现有 warning，但无错误；它和原 Editor 程序集新时间、真实四次 Play 分别构成不同层级的证据。

结论仅为 **当前 Scene / 当前中央绘制出口 / 正式 OID120 与 OID121 / tick6 同 Z body 遮挡像素限定通过**。Q09/P-04 父项、Legacy 绘制、正式 EXE 同条件实际像素 A/B、插值和其它战斗画面仍开放。此诊断白底相机 readback 是受控像素见证，不代替完整 Game View 的整体观感验收。
