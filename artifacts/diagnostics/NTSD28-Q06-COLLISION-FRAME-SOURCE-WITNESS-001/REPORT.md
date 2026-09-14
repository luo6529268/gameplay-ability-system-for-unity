# 碰撞快照原函数见证结果

VERIFIED / SOURCE_MODEL_WITNESS_ONLY，2026-09-14。

实际命令：`Tools/NTSD28AuthorityTrace/Build-AuthoritySourceCapture.ps1 -OutputDirectory Temp/NTSD28CollisionFrameSource -RunnerSource Tools/NTSD28AuthorityTrace/collision_frame_lookup_witness.cpp -ExecutableName collision_frame_lookup_witness.exe`，退出0。随后 Python subprocess 两次运行同一 EXE、按字节保存 stdout；均退出0、stderr空、252行、两遍一致。

- 正式 EXE SHA：B1E13AE17C86B77240B61A971AFD4C3374B645705F42B0BBCE304FD1D2819033。
- 75 source/header manifest SHA：07CD47A0623F23D2C439E0E85EABF2ED10F8EAE8FC7D70DDB8396C704B3D778F。
- 输出 SHA：fa3586923fc1b51be6437b9da0a6e8c3fe990efc2438eda03ff6f973b1e41897。
- 252输入：snapshot 0/99/857/998/999/1000/-1 × 声明2 × current 0/10/1000 × 替换侧 none/attacker/target × 目标沿用0或同snapshot。
- 3780项断言通过；72组产生一个候选、180组零候选；108组推进kind1、144组不推进；无损坏关系。RNG零，两个实体 raw 在 before/afterCollection/afterCatch 均无额外修改，独立 timeout 按预期78或75。

判定边界：implicit帧0..998可查询但未声明帧无块；声明999可读取，未声明999/1000/-1无帧。当前frame0/10有数据也不能让无效snapshot借用它；替换target定义后 snapshot centerx=500、碰撞从1变0，替换attacker定义后kind1 decrease从2变5、timeout从78变75。

额外观察：当前action1000但snapshot有效的诊断初值中，几何pass可以 success=false/diagnostics=2，同时保留一个snapshot几何候选及正常kind1推进。SimulationTickDriver:693–700 收集诊断后继续，并不在此立即return。不过正式driver在collection前刚执行snapshot_actions，本夹具故意在两者之间扰动current，因此**尚未证明该分离初值在正式同一tick此时点可达**。此发现不能单独授权删掉Unity所有current资格门，下一Task必须区分直接API合同、消费时点和正式可达性。

定义替换同样是诊断扰动，用于确认frame descriptor归属；不代表正式变身、throwinjury==-1 或transform流程已经验收。未运行Unity编译、SelfCheck或Play，本次无Unity生产改动。此前05:10:10Z SelfCheck属于上一生产版本证据，不能记为本次新运行。

证据：first.jsonl、repeat.jsonl、两个stderr、build-manifest.json、validation.json。用户Scene当前SHA仍BCD1047BF912C6A4A8BC9F3A76EAF3FA954211AD064E0402B1C01BF3BA0E9FB6。

## 后续追加84输入

原252证据保持，当前runner追加84（42两端current=snapshot、42混合）。expanded目录336两遍一致SHA43f6713de73a76ba5f2c07c1627bf058ecf9e1706baa93701112200ef1ff7499、原252字节前缀一致；96有候选/144kind1/RNG0，EXE/75源身份不变。Unity对应四组1344读帧/raw/catch无差异，仍96次候选资格首差；详见COLLISION-FRAME-UNITY-001报告，不能因source通过关闭Unity。
