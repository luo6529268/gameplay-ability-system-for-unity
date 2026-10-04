<!-- CHANGE-RECORD
id: NTSD-BATTLE-COMBO-FIXED-SLOTS-001
status: BLOCKED
change-kind: CODE
code-path: Assets/NTSD/Scripts/UI/Battle/BattleComboView.cs
authority: User requests authored fixed UI slots
evidence: artifacts/diagnostics/NTSD-BATTLE-COMBO-FIXED-SLOTS-001/before.json
-->
# NTSD-BATTLE-COMBO-FIXED-SLOTS-001
User requests use existing Editor-created six icon/five arrow siblings under Canvas/ComboPanel/ComboBg/ComboList. Retain RectTransform measurements, current zero gap, .35 Scene delay, native maximum five snapshot/events and consumed/new-input cancellation. Remove View-only MM Mini dependency and runtime object construction/destruction/pool lifecycle; no framework deletion. Scene ownership confirmation pending (Editor read-only bridge timeout); script can proceed independently. Only bind saved existing RectTransforms after confirmation, never generate duplicate slots or overwrite user hierarchy.
Paths: BattleComboView InitializeVisuals/Render/ClearVisualState/OnDestroy, serialized iconSlots/arrowSlots; NTSD_Battle scene component1463265079 bindings only. Original fields retained for dimensions; seven keySprites untouched. No DAT/input/battle/font/button changes. View owns event/timer state, not scene objects; disable/destroy only unsubscribe/cancel/hide, no runtime allocation during shutdown. Validation: compile, actual saved hierarchy in Unity, 0..5/max5 with six capacity, arrows/background widths, .35 consumed timer/new input, repeated enable/no object growth; originalEditor only if available/clean, otherwise saved-scene extracted exact hierarchy isolated Play clearly reported. No deleted tests restoration. Rollback audited before bytes after preserving subsequent user edits; no irreversible operation.

Code written: removed Mini imports/components/borrow/return/reset helper, serialized two RectTransform arrays and caches existing Images, renders/hides authored slots only; Scene unchanged pending ownership confirmation.

Independent validation: runtime/editor generated-project compile0errors; actual isolated Unity2022.3.62f3 Play loaded fresh runtime DLL and imported original saved ComboPanel YAML subtree, PASS96 assertions. Only probe removes View component before dynamically attaching fresh DLL and centers root for capture; authored descendant objects/Rects/Images/sprite GUIDs preserved. Existing scene bindings read; proposed array references in pending-scene-bindings.diff, not yet applied awaiting saved-state confirmation. .35 timer/native reset/consumed/new input/session guard/5keylimit/6capacity/5arrows/repeated enable/zero growth checked. Library attachment materialization failed on Windows os.setxattr; no image inspection claimed. No originalScene whole-battle Play acceptance yet.

Current delivery state BLOCKED only on original Scene ownership confirmation/binding; code and isolated authored-slot focused validation passed. OriginalScene currently lacks new iconSlots/arrowSlots serialized arrays; do not treat original-scene integration as complete.
