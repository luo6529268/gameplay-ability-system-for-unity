# C054 当前正式源码受控小数解融合结果

状态：`SOURCE_CONTROLLED_PASS / FORMAL_ROOT_NATURAL_AND_UNITY_SCENE_PENDING`。本证据只验证当前正式 336B44 playable Core 的条件分支，不把源码测试进程写作根目录正式 EXE 的自然战斗观察。

- 身份：根 `NTSD2.8-Logan.exe` SHA-256 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3`；正式 `resources/runtime/decoded_dat/data/fusion.dat` SHA-256 `E0D7BF92F222C63C04D6728ECD423369F0604D77FF12DF958CE4AE76FEBA5FEE`。诊断链接该根当前 28 Core + 3 playable C++ 文件，不编辑它们。
- 输入：复用旧融合诊断的正式 fusion 表和受控对象定义。融合成功后，在主角整数位置 `(320,0,250)` 不变的前提下，把主角精确位置明确设为 `(320.75,-0.25,250.5)`，并显式把融合计时设为 0。计时介入与小数注入都是诊断条件，不声称玩家自然达成。
- 结果：表行 0 与 1 都 `fused=1/defused=1`。拆分瞬间主角精确位置仍为 `(320.75,-0.25,250.5)`，恢复伙伴精确和整数位置均为 `(320,0,250)`；其后各推进一个正式完整 Driver tick 并记录两实体。严格 HP 门阴性行未融合，缺伙伴定义行 unresolved 且未恢复伙伴。
- 重复性：两次运行均 exit 0、各 4 行，输出 SHA-256 同为 `0A278F5512EAA402CE4B1BAF5146AC58EA80F8905133D885176A278817F78E2D`；独立逐字段断言 40 项通过。诊断 runner SHA-256 `E1A467713CB3B732AB82D9B37CA2BAF5109B80BC45C2B9740291EF0DA22AACCD`，可执行文件 SHA-256 `0A7A70B85E78E34692F86487E0F2FD52EF16335249B1EAF4A79EC7EC2686CF48`。原输出、重复输出、runner、编译日志和 [validation.json](source-fractional/validation.json) 均已归档在 `source-fractional/`。

原 Unity Editor C054 小数聚焦 EditMode 1/1 已另行通过；两边的受控分支结果一致。正式根 LFR 当前初态接口不能直接设精确小数，尚无根自然可达小数解融合或原 Battle Scene 后继 tick 证据，因此 C054 仍 `RUNTIME_PENDING`，Q07 和总目标开放。后续不应为了取得这一证据改 DAT；先找正式完整战斗中的非整数拆分前置，再决定是否扩展原 Scene 探针。
