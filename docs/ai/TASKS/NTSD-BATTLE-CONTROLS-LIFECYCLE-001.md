# NTSD-BATTLE-CONTROLS-LIFECYCLE-001
Before: Awake subscribes until Destroy; disabled View can receive live button presses. Failed sink release leaves IsPressed true.
After: OnEnable/OnDisable own idempotent subscriptions; OnDestroy fallback unsubscribes/releases. Press rejected when View disabled. Release clears local state before attempting old binding route. BindPlayer/UnbindPlayer still release before clearing/changing player binding; no delayed release queued to new player.
Scope: BattleControlsView only. Preserve action names, OR merge, tick buffer, external player assignment. No pointer policy, focus policy, scene wiring or joystick changes.
Acceptance: local regression harness for press/release/drag event, disable/re-enable/destroy and sink disappearance/rebind; independent compilation if available; static reference/format check and ledger. No claim of Unity Play. Shutdown uses existing View disable/destroy cleanup only, no new service/queue or shutdown reordering.
Rollback: exact verified before-0.txt, preserve newer edits.
