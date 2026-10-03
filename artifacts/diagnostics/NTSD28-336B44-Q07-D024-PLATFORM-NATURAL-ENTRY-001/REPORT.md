# Q07/D-024 OID56 平台普通输入入口有限验证

状态：`VERIFIED_SCOPED_NATURAL_ENTRY_AND_BOUNDED_NO_CARRY`。正式根 EXE SHA-256 `336B44E58BEA637246B65204AFC50FD8734C9AA38969B82836FA685497EB7BD3`，只读正式 `resources/runtime` 和对应 playable 构建闭包。仅新增 [诊断脚本](../../../Tools/NTSD28Q07Diagnostics/d024_platform_natural_entry_probe.cpp)；没有修改正式 EXE/源码、DAT、Unity 生产/Scene/地图/相机或非战斗逻辑。

正式 `tay.dat` 中多由也OID36站立帧有 `hit_Ua:240`，frame240 的 state1250239 需真实输入和完整帧推进确认；frame239→241→242→243，243 带 kind10。正式 `BattleWorld28` 的 kind10 角色冲击默认写 action182，正式飞段OID56 `rea.dat` 的182帧带 `dvx:-3` 和 kind50。`BattleWorld28::rebuild_geometric_hit_candidates` 使用严格X/Z、前后Y以及碰撞参考建立平台关系；`apply_frame_motion` 仅在目标当前Y等于碰撞参考时消费链接源帧的DVX。相应源码见 `source/ntsd28_core/src/simulation/battle_world.cpp` 和 `physics_integrator.cpp`，DAT为正式 `decoded_dat/c/tay/tay.dat` 与 `decoded_dat/c/hid/rea.dat`。

新脚本对普通三人初态 `Tayuya36/X100/Y0/team1`、`Hidan56/X200/Y0/team2`、`Naruto2/team1` 用背景1/Z400/mode0/seed `0x28A55A5A` 做每案96个完整 `GameSession28::step`。多由也离散输入为防御tick1–2、上tick3–4、攻击tick5–6；鸣人独立跳跃输入按案例起点连续两tick。`natural_2_5` 中多由也tick6到239、tick22到243，tick23 kind10 applied且飞段从普通action0到182。它证明**自然动作入口**，不是从受控frame182起步。1tick输入反例未到243。原始首轮 [CSV/摘要](source-run-01/) 保留。

地面鸣人起始Y−5在命中前回Y0，无平台链接。鸣人从Y0起跳tick9、11、13、15、17且X205时各有一tick链接但X不动；tick19–23无链接。起跳9/X205在tick24源X197/Y−8、目标Y−85，链接但未贴合参考，tick25源移到X194后X205超出严格范围。[第二轮](source-run-02/) 保留。

固定起跳9后仅扫起始X175/180/185/190/195，链接tick数为0/2/3/3/3；五例目标X均不动。X185在tick29–31目标Y依次−73/−65/−58，本轮碰撞参考为−50/−58/−64，不满足帧运动的 `position.y == collision_y_reference`；tick32源X173，范围又不再含目标X185。这里是**自然链接但未搬运**，不能称自然非零搬运已验。[第三轮](source-run-03/) 和[第四轮同表+LFR](source-run-04/) 的摘要及逐tick CSV 各自逐SHA相同（摘要 `3C2926E3…26D35C7`、逐tick `6DA37C30…54749`）。

第四轮录两条LFR。根正式 EXE `--headless-playback-lfr` 经正确逐参数进程启动，两条均exit0、`passed=true`，声明96tick、报告完成97tick；仅对共有tick1–96的十选定字段比较，`natural_2_5` [960/960](root-playback-natural_2_5-v2/paired-selected-fields.json)、`landing_x185` [960/960](root-playback-landing_x185-v2/paired-selected-fields.json)，首差均无。字段含多由也动作、飞段动作/X/Y/环境状态、鸣人动作/X/Y/碰撞参考/平台来源。后者根报告的 `platformOperation30` 明确列tick29–31、slot2/sourceSlot1、参考−50/−58/−64；根和源码都无目标X位移。两份根报告均写 `nativeParityClaim=false`，回放PASS不能扩成正式EXE真实GPU或全内部相位同态。首次用 `Start-Process -ArgumentList` 启动 `landing_x185` 因包含空格的路径参数未逐项引用而exit10，原空结果目录保留；v2用 ProcessStartInfo.ArgumentList 正确逐参传递后exit0，不用失败尝试作证据。

本包仅关闭“多由也普通输入→飞段182及有限自然平台链接在正式源码/根回放可达”的前置问题。本矩阵中自然搬运为阴性，不能推断其它角色、动作、跳跃时机或位置不可达。前一个受控frame182连续搬运原Battle Scene已通过；本自然三人链尚未在原Unity Battle Scene跑同输入，真实Game View像素、更多实体及 Q07/D-024、Q09/Q12 总出口均开放。下一按总表先做这个已证自然链的原Battle Scene完整Driver首差，若零差则保留自然非零搬运条件门，转下一个正式可达差异，不继续无界扫阴性矩阵。
