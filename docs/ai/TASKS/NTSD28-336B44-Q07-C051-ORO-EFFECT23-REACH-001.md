# NTSD28-336B44-Q07-C051-ORO-EFFECT23-REACH-001

状态：VERIFIED（限定正式源码/根可达性）；Unity待。父目标 `NTSD28-UNITY-BATTLE-REALIGNMENT-001`，G1/BATCH-04/Q07/C051。

正式依据：所选336B44 playable `GameSession28::step`→`SimulationTickDriver28::step`→`BattleWorld28`。正式 `resources/runtime/decoded_dat/data/data.txt` 将OID20映射为`c/oro/oro.dat`、OID888映射为`c/oro/a/atk.dat`。OID20 frame288→289的OPoint生成OID888/action35；子体35→36→37→40，frame40～43有kind0/effect23/dvx-10。Unity共用writer与投影已有effect22/23相对方向分支，不能只据静态条目判为缺陷。

本包唯一脚本路径 `Tools/NTSD28Q07Diagnostics/oro_effect23_reachability_probe.cpp`，仅链接当前正式28 Core/playable source，配置OID20/action288与OID2普通目标、同seed中性输入和近/远/左右位置控制，运行完整GameSession并输出每tick父子动作、OPoint出生、实际命中effect/dvx、目标位置/水平速度/HP与LFR。先以正式源码双跑确认确定性和至少一个实际effect23命中；阳性后才用根336B44 LFR核对同条件，并决定是否送原Unity。阴性只记该受控初态范围，不改生产。

不修改正式DAT、图片、背景/模式、Unity生产/Scene/Prefab、非战斗功能及既有诊断原件。新输出只能使用本ID独立诊断目录，不覆盖原件。验证：g++正式构建闭包链接0错、明确进程退出码、近/远/左右计数与同输入双跑SHA；根EXE回放仅在LFR兼容且有阳性时；四保护Scene/Asset SHA不变。回滚仅撤销本包新增诊断及治理文字，需遵守文件删除批准规则。C051/Q07/总目标不因本包自动关闭。

结果：正式源面右X550与面左X350子体tick4出生、tick9 effect23应用；正式根各640/640选定字段同态。X1200仍命中，X2000被stage钳位且子体追踪，不作无命中对照。详同ID报告；Unity与Scene仍待。
