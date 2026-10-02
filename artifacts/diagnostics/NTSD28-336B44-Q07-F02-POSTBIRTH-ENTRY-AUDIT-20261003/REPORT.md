# Q07/F02 高速武器出生后入口只读审计

状态：`SOURCE_AND_CONTENT_CANDIDATE_ONLY`。这是当前336B44正式playable调用链与正式decoded DAT给出的下一复现候选，不是自然触发证明，也不关闭F02/Q07。

当前正式直接OPoint初生目录已证明13条type4/6、state1000入口的初生水平速度绝对上界只有5，见[目录报告](../NTSD28-336B44-Q07-F02-FAST-SPAWN-CATALOG-001/REPORT.md)。因此下一阳性不应继续仅改变同一出生点的初态X坐标。正式 `simulation_tick_driver.cpp` 先对已有对象执行物理，再于约686行做持有物定位/释放、构建候选，并在约872/897行分别消费type0/非type0命中。`battle_world.cpp` 的 `settle_held_refill_objects` 在持有者WPOINT的`dvx!=0`且子体type4/6时，把子体动作设为40、X速度设为有朝向符号的WPOINT `dvx`，随后解除持有关系。正式鸣人`c/nar/nar.dat` frame47/51/54分别声明投掷`dvx=55/60/23`。

正式OID422 `w/x.dat`和OID600 `w/6.dat`的frame40均为state1002，frame0均为state1000。正式 `BattleWorld28` 的kind10命中分支在type4/6目标当前帧不是state1000时把动作设回0，并将已有X速度除以1.07；kind15分支在相同非state1000条件下也设回0，且按相对位置给X速度加或减1。正式Tayuya(CS2) OID36 的`c/tay/tay.dat`具有kind10 ITR，例如frame243～247；`c/tem/a/tem.dat` frame120具有kind15 ITR。这里的DAT行只证明存在生产者声明，不能证明其与刚投出的武器自然相交。

因此存在一个**待验证的可能链**：真实持有type4/6武器→鸣人高`dvx`投掷使武器处在state1002且`|Vx|>9`→同一或后续有效kind10/15命中将其动作重置为state1000、速度仍大于9→下一物理入口经接地摩擦后仍严格大于9而选择动作40；若同tick落地分支覆盖40，还需排除该覆盖。它满足F02被测规则的候选先后关系，但尚未证实拾取、投掷、碰撞几何、候选消费、动作重置及下一物理入口在一个自然Play中实际共现。

既有`fast_weapon_natural_reachability_probe.cpp`的两次`spawn 22 128/127`中性输入尝试均没有达到frame128：请求127的源CSV首个tick已经是动作0，后续为站立帧。虽然`GameSession28`把配置动作传入SpawnRequest，首tick前后的转移未在该诊断中记录。两份128tick输出保存在目录报告下，不能作“鹿丸OPoint不可达”的阴性证书，也不能作为F02自然证据。

下个实施包应先利用正式物理输入在完整GameSession中获得一件type4/6武器，记录拾取、WPOINT高速度释放、kind10/15目标命中**每一pass前后**的武器动作/速度及下一物理入口，找出首个实际阳性。若需要新增/修改诊断脚本，先建独立Task/Change；先证正式源码自然链，再使用能保留初态与输入的正式LFR去根EXE复核，最后在原Battle Scene与当前正式DAT比同tick。不能把手设高Vx或手设持有关系冒充正式根同初态，也不能部署用户暂缓的stage DAT。只读审计未修改生产、DAT、Scene或非战斗代码。
