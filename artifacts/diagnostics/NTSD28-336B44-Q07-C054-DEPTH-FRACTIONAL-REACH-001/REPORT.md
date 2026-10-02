# Q07/C054 自然纵深小数候选：本输入窗阴性

权威：根目录正式 `NTSD2.8-Logan.exe` SHA-256 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3` 所对应 playable 源码与正式 `resources/runtime`。本包运行的是参与 playable 构建的源码诊断，不是根 EXE 或 Unity Play。

`NTSD28-336B44-Q07-C054-DEPTH-FRACTIONAL-REACH-001` 只给现有正式源码探针增加 `diagonal_run` 输入：OID10/11 在 tick 1 自然合体为 OID52；tick 2–3 按右，tick 4–5 放开，tick 6–15 同时按右和纵深上，之后中性到 tick 240。没有注入位置小数、改计时器或修改 DAT。

首次编译 `compile-v1.txt` 失败：诊断脚本误用不存在的 `InputKey28::up`。已仅更正为正式枚举 `depth_up`；`compile-v2.txt` 的 g++ 退出码 0。两次独立运行均退出码 0，`run-v1` 与 `run-v2` 的 CSV SHA-256 同为 `92669428F6DBE476A1D9DE0671F2FE2034FAD5DBF3B94647217652FA95EF385A`，summary SHA-256 同为 `79FE3BB5058597452DD10747DC152A9BB55E85CC13807A0C13EFCD2C4C75F712`。

两次均为 tick 1 合体、tick 201 解融合，`fractional_split=-1`。`diagonal_run` 的 240 行中小数入口 0 行；tick 1 后主角 X=327、Z=400，tick 6、15、200 均保持 X=327、精确 Z=400。合体后 tick 1–200 动作一直是 310。正式 `fusion.dat` 给记录 2 配置 `action:310`，但当前正式 OID52 的 `c/nar/kyu.dat` 无 frame 310；`input_routing.cpp` 的 `route_native_type0` 在 `definition->frame(action)==nullptr` 时立即返回。因此本窗的方向输入不能进入跑步分支，`running_speedz=3.7` 没有被消费。这是对本输入窗的源码与运行结果解释，不推断其它受击改帧等路径均不可达。

结论：本包是可复现的 **SOURCE_SCOPED_NEGATIVE**，未出现自然小数拆分，不送根 EXE 或原 Unity Battle Scene 做同态验收。C054 父项、Q07、总目标仍开放。下一步只在正式内容与可达调用链中筛能在拆分前改变动作或保留精确小数的自然入口；找不到时维持受控小数规则测试与未证自然入口的分层状态。

原件：`compile-argv-v1.txt`、`compile-v1.txt`、`compile-argv-v2.txt`、`compile-v2.txt`、`run-v1/source-ticks.csv`、`run-v1/summary.txt`、`run-v2/source-ticks.csv`、`run-v2/summary.txt`。本包没有改正式源码、Unity 生产、DAT、图片、Scene 或非战斗代码。

治理验证：`Tools/Validate-ChangeLedger.ps1` 退出0（1136 Record、当前代码差量12文件受记录覆盖；其余历史记录 warning 未阻断）；`git -c core.safecrlf=false diff --check` 退出0。
