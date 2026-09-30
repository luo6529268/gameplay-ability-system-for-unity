# Q07 千代自然傀儡控制链：正式根 EXE 可达证据

状态：`VERIFIED_SCOPED_FORMAL_ROUTE / UNITY_RUNTIME_PENDING`。正式根 `NTSD2.8-Logan.exe` 本轮 SHA-256 为 `B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033`。本包用配对 playable `GameSession28::step` 的普通输入录制，并由正式根 EXE headless LFR 回放；未强制动作、生成或碰撞，未改 DAT。两次根报告均为 `passed=true/failureCode=0/nativeParityClaim=false`，不把 LFR PASS 写成整状态等价。

共同初态沿用前一自然 Uj 夹具：千代 OID8/slot0 HP150、MP500、X500/Z650，鸣人 OID2/slot1 HP500、X1200/Z650；mode0、背景23、seed682973786、显式 BGM2、双方 team1/2。tick1–2 防御、3–4 上、5–6 跳，正式 Session 自然到千代301、OID419 tick18、OID854 tick27。

正式作者链是 `chi.dat` frame60 `hit_ad:254` → frame254 kind8/Y8501000 对傀儡普通帧 BDY → 千代420/421的Y8504500 BDY 对傀儡普通帧kind8/Y8504500 → 傀儡400→401→402→403→407。`pup5.dat` frame407 的完整kind8为作者ordinal0，缺几何kind100100为作者ordinal1；后者是control-only，不能作为物理候选。配对源码入口为 `input_routing.cpp:205-233,446-465`、`hit_candidates.cpp:201-255`、`battle_world.cpp:5682-5746`。这段控制交互的目标是千代与自己的傀儡，不是远距鸣人的普通受击框。

| 输入 | 正式源码 Session 与根 EXE 结果 | 结论 |
| --- | --- | --- |
| run1：攻击tick40–41、防御42–43 | 千代tick42转420，MP400→250；傀儡仍frame109，无控制ITR，120tick无400/407。根LFR PASS。 | 命中作者帧时机缺少傀儡普通帧；原阴性证据保留。 |
| run2：同键序列依实测窗口移至攻击54–55、防御56–57 | 傀儡tick55 frame0、tick56 frame5；千代tick56转420；傀儡tick57转400，tick68–69到407。根LFR PASS。 | 普通输入→自然出生→控制交互→frame407 在正式根发行可达。只做一次由run1观测决定的时点移动，无位置或角色矩阵。 |

run2 源 CSV 120 行与根 trace 相同 tick 的千代action、MP、OID419数量、OID854数量、OID854 `slot:action` 列表、frame407指示、输入采样phase **七组各120/120一致，首差0**。根trace第68 tick为slot59/OID854/action407。根报告`completedTicks=121`包含额外宿主计数；比较只覆盖CSV声明的tick1–120，不是全部状态/RNG证书。

GCC 15.1 用正式build closure的28 core、四个playable源文件和本工具编译两次均exit0，`compile.log`/`compile-v2.log`非覆盖保留。run1 CSV/LFR/根报告/根trace SHA-256 依次为 `538610B0CA660D0572D7D233A366502320ABA559D903B75D944C2FADCBEF90E5`、`AA2642EF5AA33157DD0A9E9FC4C3D36BD16F99CBD222E944C480831FB08F31F4`、`BD5D6DE89BCFD09DA63E429FB6F23F1D8F644CF15A6361F6775CD7B49DBFCA1D`、`CBC0F1D28DAE1EA5D7DD1107B7B5C0536FD3DE9B97E344EE972070F351B69F5C`。run2 对应为 `CA2A236CFB1C4D32198D32E593528F0D54F9FA16A85529FE0032884047246308`、`7ECEAB164351EC6ACE7F50F01DE3FC08F75BDCE9271BAA890657A3892B8521DA`、`532044EB8D757E747707C5DAC913852B4CD731AEDBEB9EFAF336AAE4C7A8B5BD`、`67BE5300FC9BD8553AE4CB61435C89FD5ACA5FB627147D5EC6B534D1862A044E`。

本包只关闭正式版自然可达入口。下一门是在**原Unity Battle Scene** 同正式内容、初态和输入，经生产完整Driver检验OID854/frame407的作者ITR ordinal及候选结果；此前原Editor受控作者帧3/3不能替代。Q07/BATCH-04、D-024碰撞域及总目标仍开放。Unity生产、DAT、Scene、Asset、相机和非战斗代码均未改。Battle/Menu Scene、GameConfig、ProjectBattleModeConfig的SHA分别保持 `2EE465D83C7169A0589447F437E37CAEFF3CC6F1BA6C3AAA55B8068F2B48B77A`、`785F828C4E64182BEA214E4794B198E3C82E3C42002FDADD3932A7E061B81E13`、`0527D737A1FA38FC56B51D00DC6E96A421D3C67222546368B147C2D074CB8EA7`、`B57CFEF32CC3ECE37AC98A4A1EF04FEB2FEE4EB3CC08466A4BFD32C1EDD85B82`。未运行Unity编译、SelfCheck或Play，本包不声称Unity运行时已验。
