> Final status (2026-09-24): VERIFIED for independent Asset schema and immutable snapshot; production publication is a separate verified package.

# NTSD28-Q07-PROJECT-MODE-CONFIG-ASSET-001

Status: PLANNED. User confirmed 2026-09-24 that both native mode DAT families are excluded and asked for a new Unity ScriptableObject asset, similar in use to GameConfig but independent from it, to replace `data/mode.dat`.

Scope of this first bounded package: define a project-owned battle mode ScriptableObject containing only the fields currently consumed from the native mode DAT pair (combo gating; KO feed timing, layout, icon keys and cue keys); create one serialized asset with explicit initial values that preserve the current project-visible mode behavior; give it a stable resource lookup path and a deterministic immutable snapshot method suitable for later worker-side publication. Do not modify original DAT tokens, existing GameConfig, Menu, Scene, original mode reader or its current production consumers in this package.

Dependency: `NTSD28-Q07-EXCLUDED-NATIVE-BG-MODE-001/MODE-CONSUMER-GATE.md`. The next independent package must replace the native DAT capture, content identity/freshness, first-tick combo and KO publication, icon candidate and audio/presentation consumers atomically; until that passes focused tests and Battle Scene Play, the two original mode DATs remain in Assets to avoid a broken battle. This asset package alone does not fulfill the user request to stop using native mode DAT at runtime.

Acceptance: original Editor imports the asset and compiles; a focused test reads it through Unity's intended resource path, confirms all serialized fields and captures a worker-safe immutable value without Unity API calls on the worker; asset and script GUIDs unique; both Scene disk hashes unchanged; ledger validator passes. The currently running accidental full EditMode suite is not a substitute for this focused test.

Rollback: remove only package-owned new script, meta and asset after explicit repository deletion approval, or stop referencing them in the next package; preserve existing GameConfig/Scene and all user work.
