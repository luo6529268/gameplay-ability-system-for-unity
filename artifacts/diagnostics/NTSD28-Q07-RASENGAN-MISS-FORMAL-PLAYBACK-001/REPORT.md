# Q07 鸣人螺旋丸错过转换：正式发行受控回放

2026-09-25。结论仅限本次明确初态和 32 个逻辑 tick：源码桥与根目录正式 EXE 的 11 个逐 tick 指标共 352 次比较均相等；当前 Unity 的旧 `OID434/slot52/action397` 在 tick30 的 `Vx=550` 首差已经变为 `Vx=0`，与正式 EXE 一致。Unity 与发行版的 X 坐标有 26 处数值差异，逐项分组可由用户批准的相对位移比例与整数投影解释，不能按原始像素要求相等。Q07/R18 和完整螺旋丸自然按键窗口仍未关闭：此次是受控 LFR 回放而非独立物理键/画面录制，也没有建立所有战斗行为等价证书。

## 权威、输入和重现

- 正式根 `NTSD2.8-Logan.exe` 前后 SHA-256 均为 `B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033`。新桥只编译对应 playable/core 的 28 个 core `.cpp`、`game_session.cpp`、`selection_flow.cpp`、`lfr_recorder.cpp`、`game_session_lfr.cpp` 与本诊断脚本，GCC 15.1.0 编译 exit 0，`compile-bgm-fixed.log` 为空。未修改原版 EXE、源码或 DAT。
- Naruto OID2/action241/HP500/MP200 at (500,0,650)，OID7/action0/HP500/MP500 at (1200,0,650)，stage23，seed682973786，玩家0在 tick27–28 按攻击，其他 tick 无键。源码桥显式设背景音乐选择2，对应正式 LFR 回放的 `bgm\\stage1.wma`，避免默认随机选曲在战斗前消耗一次同步 RNG。
- 桥 `rasengan_miss_lfr_probe_bgm_fixed.exe` exit 0，输出 `rasengan_miss_source_packets_bgm_fixed.lfr`（SHA-256 `90645E2E58A8BEDBE31438B566FEA35C42AEAF42FBAE78B3C9A2B1380048C491`）与 `rasengan_miss_source_ticks_bgm_fixed.csv`（`AE52A8331EF56426E7A0A29B14662E18BD213E81E3385DD09BEA020BA3EB6C0B`）。正式 EXE 通过 `--headless-playback-lfr` 加两项初始 action/MP override 回放，report `passed=true`、`failureCode=0`、`declaredTicks=32`，trace 34 行（含 tick0 与 terminal），文件 SHA-256 分别为 `13C3C0D9A9555C17D0EBFDCF8053DFBF93A7F345CEA47DBAC03B3C515EAFA6FA`、`3D7885020F809B02E5B31051E54A2EB214BA02952B937B96633014A540A7E37E`。GUI EXE 命令启动时 shell 未取得可靠进程 exit code，因此以完成后的 report/trace 判定本次回放；report 自身声明 `nativeParityClaim=false`。

## 对照结果

| 范围 | 结果 |
|---|---|
| 源码桥 → 正式 EXE | tick1–32 的 actor action/MP、OID434 总数、slot52 的 OID/action/X/Vx、同步 RNG counter/index/calls/last site：352/352 相等；RNG table hash 均为 `a1ba1b90ea55796d`。tick29 选动作25、同步调用 site `0x84`；tick30 OID434/slot52/action397/X507/Vx0，actor MP100。 |
| 正式 EXE → 原 Editor Unity raw | 32 tick 的活跃 slot 集合完全相同，94 个实体行的 OID/action/state/Y/Z/Vx/Vy/Vz/HP/MP 均相同；所检字段只有 X 不同，共 26 个字段差异。首差 tick24 slot50/OID204：正式 X471、Unity X455。tick29–31 slot52/OID434 正式 X507、Unity X494，Vx 两边为0；tick32 Naruto 正式 X555、Unity X584。 |

X 差异要按每次位移或每个生成父体的**局部原点**比较，不能统一拿 Naruto 的初始 X500 缩放。正式 `source/ntsd28_core/src/simulation/object_spawning.cpp` 的 `collect_frame_opoint_intents` 使用 `parent.position.x + (point.x - parent.frame_center_x)`；Unity `BattleLogicObjectPointRuntime.ConfigureLateOpointPosition` 将该相对差乘 `world.FixedViewRunDistanceScale`。实际 World 的合同是 `2048/1333`。只读正式 `decoded_dat/c/nar/a/ras.dat` 的 frame100（第212–214行）有 `centerx:24`、`opoint x:0 / action396 / oid434`。tick29 slot51 父体 X531 两边相同：正式 `531-24=507`，Unity 整数投影 `531-24×2048/1333=494`，正好解释 slot52 的 X507/494。其余差异按出现点分组也吻合：slot50 的 `500-29=471` / `500-29×scale→455`；tick26 slot51 `500+3=503` / `500+3×scale→504`；OID518 子体从 Unity 已缩放的父体 X504 再加局部 `5×scale`，得到 X511，对应正式父体 X503+5=508；tick32 Naruto 原始 +55→X555，Unity `55×scale`→X584，其跟随子体继承相同位移。26 处差异属于这些已批准比例出口的可解释数值结果，不是本受控记录证明出的新 X 规则缺陷。它也不证明画面、碰撞空间和所有生成链都已验收。

前两版探针及输出保留：默认随机 BGM 的源码桥在 tick1 已有一次 site `0x004021E0` 同步调用，而发行 LFR 的选曲已记录、战斗前没有该调用；因此 tick29 源码桥动作25、发行动作20 的旧结果是诊断初态不一致。固定选择2后两端均无战斗前同步调用且 tick29 都为25。两次旧输出不可作为 Unity 规则差异证据。

本包只新增 `Tools/NTSD28Q07Diagnostics/rasengan_miss_lfr_probe.cpp` 及新诊断产物。没有修改 Unity 生产脚本、正式 DAT 数值、PNG、Scene、Prefab、模式 Asset 或项目设置。旧 Unity raw 来源为原项目 Editor 聚焦 (26,2) 测试，路径 `artifacts/diagnostics/NTSD28-Q07-STATE15-POSTPHYSICS-VELOCITY-001/rasengan-miss-t26-unity-32.raw.jsonl`；该重建 seed 与人工初态只支撑本受控回访。下一 Q07 精确出口是自然物理输入、组合窗口及画面可见结果，不重复做已解释的 X 首差。
