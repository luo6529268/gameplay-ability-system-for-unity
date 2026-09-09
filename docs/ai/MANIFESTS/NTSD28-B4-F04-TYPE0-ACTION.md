# NTSD28 B4 F04 type0 action crosswalk

| 分支 | Authority | Unity现状 | 路由 |
|---|---|---|---|
| ordinary crossing | `physics_integrator.cpp` strict previousY<floor/currentY>=floor | `BattleMechanicsStepResult.Landed`已具备 | single body |
| ordinary action | state100→94；action212/state6→215；hit_g；fallback219 | exact/shared有94/215/219，无hit_g | first implementation |
| landing Y | effective floor | 两handler重写0 | first implementation |
| state12 airborne | post-gravity bands；negative env 180族看upcoming phase12 | duplicated selector看absolute Y/tickIndex | later selector package |
| state18 airborne | action<205且post-gravity Vy>1→205 | duplicated absolute-Y gate | later selector package |
| state12/18 contact | contact-side专用resolution，不要求crossing | 只在Landed edge调用 | transaction package |
| environment damage | abs env320、scale340、HP+maxHP、consumed/score/KO、env→1 | WeaponCount伤害并清WeaponCount | transaction package |
| pending hit motion | gain1cc与dx/dy/dz/picked/picking fields | 无正式完整carrier | prerequisite audit |
| audio/effect | 另查正式callsite | Unity landing直接SFX006/BrokenEffect | B10/B9 |
