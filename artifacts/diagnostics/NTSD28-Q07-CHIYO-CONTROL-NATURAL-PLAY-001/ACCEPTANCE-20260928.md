# Q07 千代自然傀儡控制链：原 Battle Scene 限定验收

状态：`VERIFIED_SCOPED_PHYSICAL_INPUT_AND_NATURAL_CHAIN`。Q07/BATCH-04、D-024与总目标仍开放。本包验证正式内容在原 Unity Battle Scene 生产完整 Driver 的自然帧/生成/控制型 ITR 候选链，并补证千代 P1 物理键到生产输入帧的入口；物理例与正式例起始输入相位不同。技能资源数值同态、全 World/RNG、同画面像素和 Q12 整场不在本次通过范围。

正式依据：根正式 EXE SHA-256 `B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033` 的既有 LFR PASS，以及配对 playable 同一 HP150 千代、鸣人远距对手、mode0、seed682973786 的自然输入 run2。正式相对 tick18 生 OID419、27 生 OID854、56 千代420、57 傀儡400、68–69 傀儡407；`pup5.dat` frame407 作者 ordinal1 kind100100 缺几何，不应进入物理候选。正式 run2 的源码 CSV↔根 EXE 七组选定字段各120/120，限于前包的记录。

原 Editor 使用唯一保存的 `NTSD_Battle.unity`，Play 克隆在 `BattleTestBootstrap.Start` 前设角色 OID8/OID2，待阵容就绪后暂停生产 Driver。将千代 HP150、X500/Z650、对手 X1200/Z650 设在 Play 克隆；一笔中立 full-Driver 步使起始输入 phase=0，然后重新初始化角色。调用生产 `StepOneTick(FrameInputSet, ignorePaused:true, buildPresentation:true)` 共120个测量 tick。因项目既有交叉输入合同，正式防御/上/跳/攻击/防御在此入口分别编码为 Unity `Attack`/`Up`/`Defend`/`Jump`/`Attack`；输入时点保持1–2/3–4/5–6/54–55/56–57，不修改 DAT 或生产输入逻辑。实际 P1 buttons/press/release 和相位均逐 tick 记录。

最终报告 `chiyo-canonical-20260928-02.json` 为 `PASS_SCOPED_NATURAL_CHAIN`，SHA-256 `A088354FA242488B72612D6DB35D22BA4FD807C1B8A89284FB80B5AFCFB161A2`。以相对 tick 比较，Unity 在真实 Driver tick24 生 OID419、33 生 OID854、62 到千代420、63 到傀儡400、74–75 到傀儡407，即与正式序列相差固定的场景启动偏移6 tick，自然链内部相对时序相同。帧407从自然实体读取到 ordinal1 `hasGeometry=false`，对当前生产 World 执行一次诊断候选收集后，ordinal1 未入候选，随后结束候选消费；该帧没有其他物理候选，不能据此证明 ordinal0 的命中效果。

`selected-field-comparison-20260928.json` 独立按相对 tick 比较正式 source CSV 与 Unity 报告：输入 phase、OID419数量、OID854数量、首个傀儡动作各120/120同值；千代动作119/120，唯一首差在相对 tick54，正式动作60、Unity65；tick55及后续所选动作恢复一致。两端启动 World/Stage/BGM/RNG未构成同初态证书，**不能**把这一分支差直接归因为生产战斗规则或只凭本报告断定 RNG 因果。探针的 `chiyoMp` 字段读 Unity `Runtime.MP`，并非已确认对应正式 `mp` 的载体；本包不声明技能资源消费同态。无完整状态、碰撞命中、像素或千代物理键整链证明。

失败原件保留且不计入通过：两次物理键探针 `chiyo-natural-20260928-01/02.json`（SHA 分别为 `DCD387440637F26DE17C18AE0F09BA05B06D99E2665F5CE6B97D0C41AB4EBD34`、`2F91C8EBE845DE81CBB98E0AE3DC35C47E844671DF663ADD3AA73A3BEC9E3784`）中物理键已排队但 P1 canonical buttons 全为 None；第三次直接输入 `chiyo-canonical-20260928-01.json`（SHA `B66DC3A53F551B112B7C0DF5180E42C1E2FAB840836808A6DBFFA11BC7918CE5`）把正式防御错码为 Unity `Defend`，结果第2步跳跃210，属于夹具错误。更正映射后只复测同一序列，无角色/位置/时点矩阵。

原 Editor 首轮编译缺 `NTSD.Animation` using，补齐后程序集更新，最终探针编译没有新 CS 错。最终 Play 有序关闭 `stopped/detached=true`，World object、runtime slot、pool borrower 均0；无物理键注入的直接模式退出 Edit Mode，Scene clean。Battle Scene SHA 前后 `2EE465D83C7169A0589447F437E37CAEFF3CC6F1BA6C3AAA55B8068F2B48B77A`；Menu、GameConfig、ProjectBattleModeConfig 四保护文件最终 SHA 各与前基线一致。MCP `manage_scene/get_active` 独立读回 Battle 场景 `isDirty=false`。`Tools/Validate-ChangeLedger.ps1` exit0/PASSED；未运行全量 SelfCheck或Q12整场。只新增 Editor-only 探针和诊断/治理记录，未改生产脚本、DAT、角色图、Scene、Input Actions或非战斗功能。

2026-09-28 物理键限定补证：前两份 `chiyo-natural` 报告记载的是探针旧物理键映射（首步K）；当前编译后的 `PhysicalKeys` 首步L，严格对应项目 Player_1 的 Defend action→正式防御。原 Editor 同一 Battle Scene 用 `chiyo-physical-correctmap-20260928-01.json` 再跑固定120 tick，状态 `PASS_SCOPED_NATURAL_CHAIN`，SHA-256 `4369EBEFFE4DD86EE50866C6CFB3802D549406D78C200E4BB3349962B5981ABF`。独立重算报告中的P1 `Buttons`、`PressedButtons`、`ReleasedButtons` 各120/120符合探针L/W/K/J时点；千代420在原Driver tick62、傀儡400在63、407在74，自然frame407作者ordinal1无几何且未入候选。两个旧None报告仍原样保留，不再当作当前映射的物理入口失败。

这一物理例起始 `InputPhase=1`，首个测量tick后为0；正式源码及相位对齐的直接canonical例首个测量tick后为1。因此物理例的OID419于相对17、OID854于26、千代420于57、傀儡400于58、407于69，不能把它与正式相对18/27/56/57/68逐tick直比并宣称全态一致。此补证仅关闭千代P1物理键到生产输入帧、自然到407的子门，不证明资源值、全World/RNG或同相位严格对照。报告有序关闭 `stopped/detached=true`、对象/槽/借用均0、`neutralKeyboardObserved=true`、Edit Mode及Battle Scene clean；Battle/Menu/GameConfig/项目模式Asset四SHA不变。生产脚本、DAT数值、图片、Scene、Input Actions、非战斗均未改；Q07/BATCH-04及D-024碰撞域保持开放。
