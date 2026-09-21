# Ordinary landing source witness

Source file written only. No compilation, execution, Unity edits or tests performed by delegated writer.

Expected186 rows: binding102, priority60, control24. One type0 entity/OID77/slot0; source frame normally10,state0,wait100,nextself; counter7,latch11,Prev/snapshot initial action. Priority current212 substitutes initial action212. XInt300/ZInt250; preciseX300.25/Z250.5; Y fromparams, pre-step integerY truncates toward0. PreviousXYZ280/-40/230 are independent sentinels. HP500/MP497, seed42; collision_y_reference equals floor0 or-10. Motion Vz2. No cpoint, input or attack geometry. States12/18 deliberately excluded.

Binding: hit_g0/1/422/857/900/998/999/-1/-422/1000 × floor0/-10 × Vx-6/0/6. Vy2,Yfloor-1 crosses contact. Every row implicit target; append declared counterpart only when resolved target0..999. hit_g0 resolves219; declaration applies to219 rather than source0. Thus17 target variants ×6 =102.

Priority: five source(action,state) tuples(10,100),(212,100),(10,6),(212,6),(212,0) × hit_g0/422/-1 × twofloors × final target declared/implicit =60. Expected target94 forstate100 else215. Does not retest catching/throw or state12/18.

Controls: initialaction10/state0, hit_g0/422 × floor0/-10 × target declaration × control0/1/2 =24. Control0 Yfloor-5,Vy1 remains airborne; control1 Yfloor,Vy0 is already contact; control2 Yfloor+2,Vy1 begins beyond floor and does not meet strict crossing from above. All useVx6/Vz2, checking damping/clamp without claiming another landing transition.

params: group,action,state,hitG,targetDeclared,target,floor,y,vx,vy,vz,control,seed. dat contains the complete definition. before/after/following each have entities[0] (null if freed), full existing raw, descriptor availability/state/wait/next, snapshot availability/state, previousXYZ, B2 input, native RNG scalar. calls/followingCalls preserve complete native call traces. config specifies stage800/180/350, default PhysicsContext and actual formal BattleConfig HP/MP/drop gates.

Immediate call is BattleWorld28.step_physics(0,{}). step records WorldPhysicsResult success/slot/message/environment damage, every PhysicsStepResult flag plus vertical enum/selectedAction/weaponHpAfter and audio. The header defines vertical enum0 airborne,1 grounded,2 nativeNoop,3 requiresActionResolution,4 frameSuppressed. No invented contact bool is added; infer only using sourceflags and explicit position/floor when needed.

Following uses a separate full SimulationTickDriver28.step on the post-physics world with default controls and stage/mode configuration. Descriptor999 validity differs from nonheld C25 lifecycle survival; do not conflate immediate binding and nexttick. Stage bounds apply during the following driver, while immediate step_physics uses only default physics context plus entity collision reference.

Expected matrix count and all outcomes require actual build/run and independent validation. No assertion of source/Unity parity is made by this artifact.

Independent validator now executed once: 186 rows,97845 checks,0 failures; landings162/airborne8/contactWithoutLanding16. SHA ed75a284cc6a23423d4aee09241bf6bdafa8e516ca732fbab81131a85a773f2e. All before and immediate keys strictly compared from independently constructed params-based state; no after-derived formulas. No initial model failure occurred. Following remains outside model. Read-only Unity search found no previousXYZ equivalent in NTSDEntityRuntime; Ai Runtime RowRefreshResult.PreviousX is rows.X before sensing-row refresh, not the physics previous-position carrier. No core field added and no Unity previousXYZ parity claimed.
