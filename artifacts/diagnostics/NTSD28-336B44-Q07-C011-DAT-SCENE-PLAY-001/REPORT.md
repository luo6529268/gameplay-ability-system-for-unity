# Q07/C011 原 Battle Scene 正式 DAT 传送链

状态：`VERIFIED_SCOPED`。最新闭合结论见下段；以下保留过程记录。原Scene运行副本中的正式李350→242已完成12生产Driver tick且声明12字段144/144与当前formal source/root同态。小樱96→97下一唯一请求仍待结果。Q07/C011父包未关闭。

原Unity2022.3.62f3 Editor Assets/Refresh新探针编译4.00s；第一次Lee01报告DIFFERENCE/DONE/exit/clean。其tick4 X680/动作/HP已对，但sourceZ650投影至项目walkregion之外，首tick双方Z被限制至481；probe还误以AnimSub当原生+0x088，真正C25计数是AttackingCounter。12tick共27差异只在counter/Z，原JSON与lee-source-unity-comparison.json保留。没有为此修改生产战斗、项目地图或DAT。

探针修订只改正确计数字段、共享初态Z400、UTC归一化10min等待（真实入场/初始化6–7min）。正式诊断改用BG1/San z375..575，使Z400在两边可行走区内部；原Unity自己的背景/地图不变。新源码/rootv3四例12tick/864声明字段均exit0/PASS，旧BG23/Z650证据仍保留并区分版本。原Editor再次Refresh编译9.28s，重载ready后在原Scene空闲Edit提交Lee02。

[Lee02原报告](teleport-lee-scene-02.json)：PASS/DONE/exitedPlay/sceneCleanAfter，运行副本正式roster7/2，HP/base/MP500、X500/800、Z400、seed682973786、输入phase0/FrameToggle0，中性完整Driver12ticks。相对tick2尾进入242、tick3仍X500，tick4相位0传送X680/Z401并尾入243；tick9后继命中也与formal source一致。[144字段对照](lee-v3-source-unity-comparison.json)严格整数相等，0首差。它是受控初态后自然DAT链，不是物理键选招或全World证书。

同一编译探针在Lee02完全DONE/退出及MCP原Editor idle后，只提交一次teleport-sakura-scene-01；Sakura/Naruto同team，action96→97/state401。等最终12tick报告、声明字段比较与Scene保护后才能关闭此出口。进入Play重载期间不因为一次观测超时重启。


2026-09-30 C011限定关闭：336B44正式根v3四例12tick/864声明字段PASS；原Battle Scene Lee02与Sakura01各12tick/144字段严格一致，合计288/288、首差0。李tick4 X680/Z401，小樱tick4 X740/Z401；两轮均PASS/DONE/exitedPlay/sceneCleanAfter，MCP确认原Editor idle/nonPlay。两Scene、GameConfig、ProjectBattleModeConfig四SHA保持，DAT与生产脚本未因补证修改。初次Lee01计数字段/地图边界夹具失败和根v1 EOF46原记录保留。该出口证明受控初态后的正式DAT自然帧链与相位，不声称物理键选招、全World/全画面或Q07整组完成。下一G1/C012。

[Sakura原报告](teleport-sakura-scene-01.json)、[144字段比较](sakura-v3-source-unity-comparison.json)、[四保护SHA](protected-hashes-after.json)。

闭合留痕检查：Ledger 1056 records/16 governed code files PASS，git diff --check exit0；未因诊断夹具修订重复整个测试套件。
